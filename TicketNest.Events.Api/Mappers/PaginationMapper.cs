using TicketNest.Domain.Events.Pagination;
using TicketNest.Events.Api.Models;

namespace TicketNest.Events.Api.Mappers;

internal static class PaginationRequestMapper
{
    public static PaginationRequest Map(PaginationRequestModel source)
    {
        return new PaginationRequest(page: source.Page, pageSize: source.PageSize);
    }
}