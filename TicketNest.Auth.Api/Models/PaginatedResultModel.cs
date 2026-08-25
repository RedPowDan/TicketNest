namespace TicketNest.Auth.Api.Models;

public class PaginatedResultModel<T>
{
    public T[] Items { get; set; } = [];

    public int TotalCount { get; set; }

    public int CurrentPage { get; set; }

    public int Count { get; set; }
}