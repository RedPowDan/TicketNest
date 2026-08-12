using TicketNest.Shared.Objects;

namespace TicketNest.Domain.Models.Users;

/// <summary>
/// Пользователь системы.
/// </summary>
public class User
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; private set; }

    /// <summary>
    /// Хеш пароля пользователя.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public UserRole Role { get; private set; }

    private User(Guid id, string login, string passwordHash, UserRole role)
    {
        Ensure.NotNullOrWhiteSpace(login, nameof(login));
        Ensure.NotNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        Id = id;
        Login = login;
        PasswordHash = passwordHash;
        Role = role;
    }

    public static User LoadFromStorage(Guid id, string login, string passwordHash, UserRole role)
    {
        return new User(id, login, passwordHash, role);
    }

    public static Result<User, string> Create(string login, string passwordHash, UserRole role)
    {
        if (CanCreate(login, passwordHash) is { IsFailure: true } validation)
        {
            return validation.Error;
        }

        return new User(Guid.CreateVersion7(), login, passwordHash, role);
    }

    private static UnitResult<string> CanCreate(string login, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(login))
        {
            return "Логин не должен быть пустым";
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return "Хеш пароля не должен быть пустым";
        }

        return UnitResult<string>.FromSuccess();
    }
}