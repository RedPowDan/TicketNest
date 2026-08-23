using TicketNest.Domain.Auth.Models.Users;

namespace TicketNest.Events.Api.Mappers;

internal static class UserRoleMapper
{
    public static UserRole Map(string role)
    {
        if (Enum.TryParse<UserRole>(role, out var parsedRole))
        {
            return parsedRole;
        }

        throw new InvalidOperationException($"Invalid role: {role}");
    }
}