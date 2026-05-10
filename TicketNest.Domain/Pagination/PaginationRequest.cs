namespace TicketNest.Domain.Pagination;

public class PaginationRequest
{
    public int Page { get; }
    
    public int PageSize { get; }

    public PaginationRequest(int page, int pageSize)
    {
        Ensure.That(page >= 1, $"Номер страницы не должен быть меньше 1");
        Ensure.NonNegative(pageSize, nameof(pageSize));

        Page = page;
        PageSize = pageSize;
    }
}