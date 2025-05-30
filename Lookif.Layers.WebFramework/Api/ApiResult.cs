using System.Linq;
using Lookif.Library.Common;
using Lookif.Library.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Lookif.Layers.WebFramework.Api;

public class ApiResult
{
    public bool IsSuccess { get; set; }
    public ApiResultStatusCode StatusCode { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public string Message { get; set; }

    public ApiResult(bool isSuccess, ApiResultStatusCode statusCode, string message = null)
    {
        this.IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message ?? statusCode.ToDisplay();
    }

    #region Implicit Operators
    public static implicit operator ApiResult(OkResult result)
    {
        return new ApiResult(true, ApiResultStatusCode.Success);
    }

    public static implicit operator ApiResult(UnauthorizedResult result)
    {
        return new ApiResult(false, ApiResultStatusCode.UnAuthorized);
    }

    public static implicit operator ApiResult(BadRequestResult result)
    {
        return new ApiResult(false, ApiResultStatusCode.BadRequest);
    }

    public static implicit operator ApiResult(BadRequestObjectResult result)
    {
        var Message = result.Value?.ToString();
        if (result.Value is SerializableError errors)
        {
            var errorMessages = errors.SelectMany(p => (string[])p.Value).Distinct();
            Message = string.Join(" | ", errorMessages);
        }
        return new ApiResult(false, ApiResultStatusCode.BadRequest, Message);
    }

    public static implicit operator ApiResult(ContentResult result)
    {
        return new ApiResult(true, ApiResultStatusCode.Success, result.Content);
    }

    public static implicit operator ApiResult(NotFoundResult result)
    {
        return new ApiResult(false, ApiResultStatusCode.NotFound);
    }
    #endregion
}

public class ApiResult<TData> : ApiResult
    where TData : class
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TData Data { get; set; }

    public ApiResult(bool isSuccess, ApiResultStatusCode statusCode, TData data, string Message = null)
        : base(isSuccess, statusCode, Message)
    {
        Data = data;
    }

    #region Implicit Operators
    public static implicit operator ApiResult<TData>(TData data)
    {
        return new ApiResult<TData>(true, ApiResultStatusCode.Success, data);
    }
    public static implicit operator ApiResult<TData>(UnauthorizedResult result)
    {
        return new ApiResult<TData>(false, ApiResultStatusCode.UnAuthorized, null);
    }
    public static implicit operator ApiResult<TData>(OkResult result)
    {
        return new ApiResult<TData>(true, ApiResultStatusCode.Success, null);
    }

    public static implicit operator ApiResult<TData>(OkObjectResult result)
    {
        return new ApiResult<TData>(true, ApiResultStatusCode.Success, (TData)result.Value);
    }

    public static implicit operator ApiResult<TData>(BadRequestResult result)
    {
        return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, null);
    }

    public static implicit operator ApiResult<TData>(BadRequestObjectResult result)
    {
        var Message = result.Value?.ToString();
        if (result.Value is SerializableError errors)
        {
            var errorMessages = errors.SelectMany(p => (string[])p.Value).Distinct();
            Message = string.Join(" | ", errorMessages);
        }
        return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, null, Message);
    }

    public static implicit operator ApiResult<TData>(ContentResult result)
    {
        return new ApiResult<TData>(true, ApiResultStatusCode.Success, null, result.Content);
    }

    public static implicit operator ApiResult<TData>(NotFoundResult result)
    {
        return new ApiResult<TData>(false, ApiResultStatusCode.NotFound, null);
    }

    public static implicit operator ApiResult<TData>(NotFoundObjectResult result)
    {
        return new ApiResult<TData>(false, ApiResultStatusCode.NotFound, (TData)result.Value);
    }
    #endregion
}

public class PagedApiResult<TData> : ApiResult
    where TData : class
{
    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public TData Data { get; set; }

    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedApiResult(bool isSuccess, ApiResultStatusCode statusCode, TData data, int pageNumber, int pageSize, int totalCount, string message = null)
        : base(isSuccess, statusCode, message)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
        TotalPages = (int)System.Math.Ceiling(totalCount / (double)pageSize);
    }

    #region Implicit Operators
    public static implicit operator PagedApiResult<TData>(TData data)
    {
        return new PagedApiResult<TData>(true, ApiResultStatusCode.Success, data, 1, 10, 0);
    }

    public static implicit operator PagedApiResult<TData>(OkObjectResult result)
    {
        if (result.Value is PagedApiResult<TData> pagedResult)
        {
            return pagedResult;
        }
        return new PagedApiResult<TData>(true, ApiResultStatusCode.Success, (TData)result.Value, 1, 10, 0);
    }

    public static implicit operator PagedApiResult<TData>(BadRequestResult result)
    {
        return new PagedApiResult<TData>(false, ApiResultStatusCode.BadRequest, null, 1, 10, 0);
    }

    public static implicit operator PagedApiResult<TData>(NotFoundResult result)
    {
        return new PagedApiResult<TData>(false, ApiResultStatusCode.NotFound, null, 1, 10, 0);
    }
    #endregion
}
