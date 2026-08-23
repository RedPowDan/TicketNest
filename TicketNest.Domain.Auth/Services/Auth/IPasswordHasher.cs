namespace TicketNest.Domain.Auth.Services.Auth;

public interface IPasswordHasher
{
    string HashPassword(string password);

    bool Verify(string password, string hash);
}