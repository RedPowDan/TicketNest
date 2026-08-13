namespace TicketNest.Domain.Services.Auth;

public interface IPasswordHasher
{
    string HashPassword(string password);
}