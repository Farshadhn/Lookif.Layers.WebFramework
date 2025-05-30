using System.Collections.Generic;

namespace Lookif.Layers.WebFramework.Api;

public interface IPagedList
{
    int PageNumber { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
}

public interface IPagedList<T> : IPagedList, IList<T>
{
} 