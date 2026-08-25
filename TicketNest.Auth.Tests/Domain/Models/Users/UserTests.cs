using TicketNest.Domain.Auth.Models.Users;

namespace TicketNest.Auth.Tests.Domain.Models.Users;

[TestFixture]
public class UserTests
{
    [Test]
    public void LoadFromStorage_should_restore_user_with_all_properties()
    {
        var userId = Guid.CreateVersion7();
        var login = "user01";
        var passwordHash = "AQAAAAEAACcQAAAAE...";
        var role = UserRole.Admin;

        var user = User.LoadFromStorage(userId, login, passwordHash, role);

        user.Id.Should().Be(userId);
        user.Login.Should().Be(login);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
    }

    [Test]
    public void LoadFromStorage_should_throw_when_login_is_null_or_whitespace()
    {
        var act = () => User.LoadFromStorage(Guid.CreateVersion7(), "", "hash", UserRole.User);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("login");
    }

    [Test]
    public void LoadFromStorage_should_throw_when_passwordHash_is_null_or_whitespace()
    {
        var act = () => User.LoadFromStorage(Guid.CreateVersion7(), "user01", "   ", UserRole.User);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("passwordHash");
    }
}
