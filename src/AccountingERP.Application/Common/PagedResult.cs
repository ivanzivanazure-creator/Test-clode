namespace AccountingERP.Application.Common;

/// <summary>
/// Wraps a page of items together with pagination metadata.
/// </summary>
public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items      { get; }
    public int              Page       { get; }
    public int              PageSize   { get; }
    public int              TotalCount { get; }
    public int              TotalPages { get; }

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage     => Page < TotalPages;

    public PagedResult(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        Items      = items.ToList().AsReadOnly();
        Page       = page;
        PageSize   = pageSize;
        TotalCount = totalCount;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }

    /// <summary>Creates an empty paged result for the requested page.</summary>
    public static PagedResult<T> Empty(int page, int pageSize)
        => new(Enumerable.Empty<T>(), page, pageSize, 0);

    /// <summary>Creates a paged result from a full in-memory list (applies Skip/Take).</summary>
    public static PagedResult<T> Create(IEnumerable<T> source, int page, int pageSize)
    {
        var list  = source.ToList();
        var items = list.Skip((page - 1) * pageSize).Take(pageSize);
        return new PagedResult<T>(items, page, pageSize, list.Count);
    }
}
