namespace DegeneraTube.Shared;

public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    
    private PagedList(IEnumerable<T> items, int page, int pageSize, int totalCount)
    {
        Items = items.ToList();
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
 
    public static PagedList<T> Create(IEnumerable<T> source, int page, int pageSize, int totalCount) =>
        new(source, page, pageSize, totalCount);
 
    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> query,
        int page,
        int pageSize,
        Func<IQueryable<T>, Task<int>> countAsync,
        Func<IQueryable<T>, Task<List<T>>> toListAsync)
    {
        var total = await countAsync(query);
        var items = await toListAsync(query.Skip((page - 1) * pageSize).Take(pageSize));
        return new PagedList<T>(items, page, pageSize, total);
    }
 
    public PagedList<TOut> Map<TOut>(Func<T, TOut> mapper) =>
        new(Items.Select(mapper), Page, PageSize, TotalCount);
}