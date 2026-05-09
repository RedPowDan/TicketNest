namespace TicketNest.Domain.Pagination;

public class PaginationRequest
{
    public int Page { get; }
    
    public int PageSize { get; }

    public PaginationRequest(int page, int pageSize)
    {
        Ensure.NonNegative(page, nameof(page));
        Ensure.NonNegative(pageSize, nameof(pageSize));

        Page = page;
        PageSize = pageSize;
    }
}