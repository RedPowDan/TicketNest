using TicketNest.Domain.Auth.Constants;
using TicketNest.Domain.Auth.Models;
using TicketNest.Domain.Auth.Models.Users;
using TicketNest.Domain.Auth.Repositories;
using TicketNest.Domain.Auth.Services.Auth;
using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Auth.Services.Users;

public sealed class UserFactory(IUserRepository userRepository, IPasswordHasher passwordHasher) : IUserFactory
{
    /// <inheritdoc />
    public async Task<Result<User, Error>> Create(string login, string password, UserRole? role, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(login))
        {
            return new Error(ErrorCode.BadRequest, "Логин не должен быть пустым");
        }

        if (string.IsNullOrEmpty(password))
        {
            return new Error(ErrorCode.BadRequest, "Пароль не должен быть пустым");
        }

        if (role == null)
        {
            role = UserRole.User;
        }

        var existUserWithLogin = await userRepository.GetByLogin(login, ct);
        if (existUserWithLogin != null)
        {
            return new Error(ErrorCode.BadRequest, "Пользователь с таким логином уже существует");
        }

        var passwordHash = passwordHasher.HashPassword(password);

        var result = User.Create(login, passwordHash, role.Value);
        if (result.IsFailure)
        {
            return new Error(ErrorCode.BadRequest, result.Error);
        }

        return result.Value;
    }
}