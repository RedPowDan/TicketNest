namespace TicketNest.Api.Models;

public class PaginationRequestModel
{
    /// <summary>
    /// Номер текущей страницы, начинается с 0
    /// </summary>
    public int Page { get; set; } = 0;

    /// <summary>
    /// Текущее кол-во строк
    /// </summary>
    public int PageSize { get; set; } = 20;
}