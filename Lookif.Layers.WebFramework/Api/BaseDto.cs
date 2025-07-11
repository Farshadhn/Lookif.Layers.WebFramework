﻿using System;
using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Lookif.Layers.Core.MainCore.Base;
using Lookif.Layers.Core.MainCore.Identities;
using Lookif.Layers.WebFramework.CustomMapping;
using Microsoft.AspNetCore.Identity;

namespace Lookif.Layers.WebFramework.Api;

public abstract class BaseDto<TDto, TEntity,  TKey> : ICustomMapping
    where TDto : class, new()
    where TEntity : class, IEntity<TKey>, new()
{
    [Display(Name = "ردیف")]
    public TKey Id { get; set; }
    public DateTime LastEditedDateTime { get; set; } 
    public TEntity ToEntity(IMapper mapper)
    {
        return mapper.Map<TEntity>(CastToDerivedClass(mapper, this));
    }

    public TEntity ToEntity(IMapper mapper, TEntity entity)
    {
        return mapper.Map(CastToDerivedClass(mapper, this), entity);
    }

    public static TDto FromEntity(IMapper mapper, TEntity model)
    {
        return mapper.Map<TDto>(model);
    }

    private TDto CastToDerivedClass(IMapper mapper, BaseDto<TDto, TEntity,  TKey> baseInstance)
    {
        return mapper.Map<TDto>(baseInstance);
    }

    public void CreateMappings(Profile profile)
    {
        var mappingExpression = profile.CreateMap<TDto, TEntity>();

        var dtoType = typeof(TDto);
        var entityType = typeof(TEntity);
        //Ignore any property of source (like Post.Author) that dose not contains in destination 
        foreach (var property in entityType.GetProperties())
        {
            if (dtoType.GetProperty(property.Name) == null)
                mappingExpression.ForMember(property.Name, opt => opt.Ignore());
        }

        CustomMappings(mappingExpression.ReverseMap());
    }

    public virtual void CustomMappings(IMappingExpression<TEntity, TDto> mapping)
    {
    }
}

public abstract class BaseDto<TDto, TEntity> : BaseDto<TDto, TEntity,  Guid>
    where TDto : class, new()
    where TEntity : class, IEntity<Guid>, new()
{

}