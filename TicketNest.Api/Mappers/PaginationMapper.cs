using TicketNest.Api.Models;
using TicketNest.Domain.Pagination;

namespace TicketNest.Api.Mappers;

internal static class PaginationRequestMapper
{
    public static PaginationRequest Map(PaginationRequestModel source)
    {
        return new PaginationRequest(page: source.Page, pageSize: source.PageSize);
    }
}