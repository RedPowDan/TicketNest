namespace TicketNest.Domain.Services.Auth;

/// <summary>
/// Данные пользователя, которые используются при формировании JWT-токена.
/// </summary>
public sealed class TokenUser : ValueObject
{
    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Логин пользователя.
    /// </summary>
    public string Login { get; }

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public string Role { get; }

    private TokenUser(Guid id, string login, string role)
    {
        Id = id;
        Login = login;
        Role = role;
    }

    /// <summary>
    /// Создаёт значение объекта.
    /// </summary>
    public static TokenUser Create(Guid id, string login, string role)
    {
        Ensure.NotEmpty(id, nameof(id));
        Ensure.NotNullOrWhiteSpace(login, nameof(login));
        Ensure.NotNullOrWhiteSpace(role, nameof(role));

        return new TokenUser(id, login, role);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Id;
        yield return Login;
        yield return Role;
    }

    public override string ToString() => $"{Login} ({Role})";
}