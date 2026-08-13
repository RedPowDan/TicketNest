using System.Security.Cryptography;
using System.Text;
using TicketNest.Domain.Services.Auth;

namespace TicketNest.Infrastructure.Auth;

internal sealed class PasswordHasher : IPasswordHasher
{
    /// <inheritdoc />
    public string HashPassword(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string password, string hash)
    {
        return HashPassword(password) == hash;
    }
}