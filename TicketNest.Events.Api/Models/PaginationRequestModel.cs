namespace TicketNest.Events.Api.Models;

public class PaginationRequestModel
{
    /// <summary>
    /// Номер текущей страницы, начинается с 1
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Текущее кол-во строк
    /// </summary>
    public int PageSize { get; set; } = 10;
}