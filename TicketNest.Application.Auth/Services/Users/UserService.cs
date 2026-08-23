using TicketNest.Domain.Auth.Constants;
using TicketNest.Domain.Auth.Models;
using TicketNest.Domain.Auth.Models.Users;
using TicketNest.Domain.Auth.Repositories;
using TicketNest.Domain.Auth.Services.Auth;
using TicketNest.Domain.Auth.Services.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.Application.Auth.Services.Users;

internal sealed class UserService(
    IUserFactory userFactory,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator) : IUserService
{
    /// <inheritdoc />
    public async Task<Result<User, Error>> Register(string login, string password, UserRole? role, CancellationToken ct = default)
    {
        var createResult = await userFactory.Create(login, password, role, ct);
        if (createResult.IsFailure)
        {
            return createResult;
        }

        var user = createResult.Value;
        await userRepository.Save(user, ct);

        return user;
    }

    /// <inheritdoc />
    public async Task<Result<string, Error>> Login(string login, string password, CancellationToken ct = default)
    {
        var user = await userRepository.GetByLogin(login, ct);
        if (user is null || !passwordHasher.Verify(password, user.PasswordHash))
        {
            return new Error(ErrorCode.BadRequest, "Пользователь не найден");
        }

        var tokenUser = TokenUser.Create(user.Id, user.Login, user.Role.ToString());
        var token = jwtTokenGenerator.GenerateToken(tokenUser);

        return token;
    }
}
