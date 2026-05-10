namespace TicketNest.Domain.Models;

public class PaginatedResult<T>
{
    public IReadOnlyCollection<T> Items { get; }

    public int TotalCount { get; }

    public int CurrentPage { get; }

    public PaginatedResult(IReadOnlyCollection<T> items, int totalCount, int currentPage)
    {
        Ensure.NotNull(items, nameof(items));
        Ensure.NonNegative(totalCount, nameof(totalCount));
        Ensure.NonNegative(currentPage, nameof(currentPage));

        Items = items;
        TotalCount = totalCount;
        CurrentPage = currentPage;
    }

    public int Count => Items.Count;
}