namespace ProductService.Application.Common;

public class PagedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPage
    {
        get
        {
            if (PageSize > 0)
            {
                return (int)Math.Ceiling(TotalCount / (double)PageSize);
            }
            else
            {
                return 0;
            }
        }
    }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPage;
}