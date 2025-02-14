using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lookif.Layers.Core.MainCore.Base;
using Lookif.Layers.Core.Infrastructure.Base;
using System;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using System.Reflection;
using Lookif.Layers.Core.Model;
using Lookif.Library.Common.Exceptions;
using System.Runtime.CompilerServices;
using System.Net;
using Lookif.Layers.Core.Enums;

namespace Lookif.Layers.WebFramework.Api;

[ApiVersion("1")]
public class CrudController<TDto, TSelectDto, TEntity, TService, TKey> : BaseController<TService>
    where TDto : BaseDto<TDto, TEntity, TKey>, new()
    where TSelectDto : BaseDto<TSelectDto, TEntity, TKey>, new()
    where TEntity : class, IEntity<TKey>, new()
     where TService : IBaseService<TEntity, TKey>
{
    protected readonly IMapper Mapper;
    protected  List<string> Ignores = [];

   
    public CrudController(IMapper mapper, ControllerExecutionEnum controllerExecution)
    { 
        Mapper = mapper;
        List<string> target = controllerExecution switch
        {
            ControllerExecutionEnum.Read => ["Delete", "UpdateByModel", "Update", "Create"],
            ControllerExecutionEnum.CreateAndRead => ["Delete", "UpdateByModel", "Update"],
            ControllerExecutionEnum.CreateAdnReadAndUpdate => ["Delete"],
            ControllerExecutionEnum.Full => [],
        };
        Ignores.AddRange(target);
    }
    private void IgnoreIfNeeded([CallerMemberName] string methodName ="")
    {
        if (Ignores.Contains(methodName))
            throw new NotFoundException();

    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet]
    public virtual async Task<ApiResult<List<TSelectDto>>> Get([FromQuery] GetAllFilter filter,CancellationToken cancellationToken)
    {
        IgnoreIfNeeded();
        var list = await Service.GetAll(filter).ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return Ok(list);
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet("{fieldName}")]
    public virtual async Task<Dictionary<string, string>> GetDropDown(string fieldName, CancellationToken cancellationToken)
    {
        try
        {
            IgnoreIfNeeded();
            var list = await Service.GetAll().ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
                  .ToListAsync(cancellationToken);

            Dictionary<string, string> rtn = new();
            foreach (var item in list)
            {
                var idProp = item.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                var displayProp = item.GetType().GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);
                rtn.Add(idProp?.GetValue(item, null)?.ToString(), displayProp?.GetValue(item, null)?.ToString());

            }

            return rtn is { Count: > 0 } ? rtn : throw new BadRequestException("Not found");

        }
        catch (Exception e)
        {

            throw;
        }

    }
    [ProducesResponseType(StatusCodes.Status200OK)]
    [Produces("application/json")]
    [HttpGet("{id}")]
    public virtual async Task<ApiResult<TSelectDto>> Get(TKey id, CancellationToken cancellationToken)
    {
        IgnoreIfNeeded();

        var dto = await Service.GetAll().ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(p => p.Id.Equals(id), cancellationToken);

        if (dto == null)
            return NotFound();

        return dto;
    }
    [ProducesResponseType(StatusCodes.Status201Created)]
    [Produces("application/json")]
    [HttpPost]
    public virtual async Task<ApiResult<TSelectDto>> Create(TDto dto, CancellationToken cancellationToken)
    {
        IgnoreIfNeeded();
        var model = dto.ToEntity(Mapper);
        model.LastEditedDateTime = Time;
        model.LastEditedUserId = UserId;

        await Service.AddAsync(model, cancellationToken);



        var resultDto = await Service.GetAll().ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

        return resultDto;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Produces("application/json")]
    [HttpPut("{id}")]
    public virtual async Task<ApiResult<TSelectDto>> Update(TKey id, TDto dto, CancellationToken cancellationToken)
    {
        IgnoreIfNeeded();
        var model = await Service.GetAll().Where(x => x.Id.Equals(id)).AsNoTracking().FirstOrDefaultAsync(cancellationToken: cancellationToken);

        model = dto.ToEntity(Mapper, model);
        model.Id = id;
        model.LastEditedDateTime = Time;
        model.LastEditedUserId = UserId;

        await Service.UpdateAsync(model, cancellationToken);



        var resultDto = await Service.GetAll().ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

        return resultDto;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [Produces("application/json")]
    [HttpPut("{id}")]
    protected virtual async Task<ApiResult<TSelectDto>> UpdateByModel(TEntity model, TDto dto, CancellationToken cancellationToken)
    {
        IgnoreIfNeeded();
        dto.Id = model.Id;
        model = dto.ToEntity(Mapper, model);

        model.LastEditedDateTime = Time;
        model.LastEditedUserId = UserId;
        try
        {
            await Service.UpdateAsync(model, cancellationToken);
        }
        catch (System.Exception e)
        {
            if (e.InnerException != null) throw e.InnerException;
        }


        var resultDto = await Service.GetAll().ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
            .SingleOrDefaultAsync(p => p.Id.Equals(model.Id), cancellationToken);

        return resultDto;
    }


    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [HttpDelete("{id}")]
    public virtual async Task<ApiResult> Delete(TKey id, CancellationToken cancellationToken)
    {
        IgnoreIfNeeded();
        var model = await Service
            .GetAll()
            .Where(x => x.Id.Equals(id))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        model.LastEditedDateTime = Time;
        model.LastEditedUserId = UserId;
        await Service.DeleteAsync(model, cancellationToken);
        return Ok();
    }


    [NonAction]
    protected virtual async Task<ApiResult<TSelectDto>> ReturnData(TKey id, CancellationToken cancellationToken)
    {
        return await Service.GetAll().ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
              .SingleOrDefaultAsync(p => p.Id.Equals(id), cancellationToken);
    }

    [NonAction]
    protected virtual async Task<ApiResult<List<TSelectDto>>> ReturnData(Expression<Func<TEntity, bool>> exp, CancellationToken cancellationToken)
    {
        return await Service.GetAll().Where(exp).ProjectTo<TSelectDto>(Mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }









}
[ApiVersion("1")]
public class CrudController<TDto, TSelectDto, TEntity, TService> : CrudController<TDto, TSelectDto, TEntity, TService, Guid>
    where TDto : BaseDto<TDto, TEntity, Guid>, new()
    where TSelectDto : BaseDto<TSelectDto, TEntity, Guid>, new()
    where TEntity : class, IEntity<Guid>, new()
    where TService : IBaseService<TEntity, Guid>
{
    public CrudController(IMapper mapper)
        : base(mapper,ControllerExecutionEnum.Full)
    {
    }
    public CrudController(IMapper mapper,ControllerExecutionEnum controllerExecution)
       : base(mapper, controllerExecution)
    {
    }
}



[ApiVersion("1")]
public class CrudController<TDto, TEntity, TService> : CrudController<TDto, TDto, TEntity, TService, Guid>
    where TDto : BaseDto<TDto, TEntity, Guid>, new()
    where TEntity : class, IEntity<Guid>, new()
    where TService : IBaseService<TEntity, Guid>
{
    public CrudController(IMapper mapper)
         : base(mapper, ControllerExecutionEnum.Full)
    {
    }
    public CrudController(IMapper mapper, ControllerExecutionEnum controllerExecution)
       : base(mapper, controllerExecution)
    {
    }
}

