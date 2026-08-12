using FluentAssertions;
using TicketNest.Domain.Models.Users;

namespace TicketNest.UnitTests.Domain.Models.Users;

[TestFixture]
public class UserTests
{
    [Test]
    public void LoadFromStorage_should_restore_user_with_all_properties()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var login = "user01";
        var passwordHash = "AQAAAAEAACcQAAAAE...";
        var role = UserRole.Admin;

        // Act
        var user = User.LoadFromStorage(userId, login, passwordHash, role);

        // Assert
        user.Id.Should().Be(userId);
        user.Login.Should().Be(login);
        user.PasswordHash.Should().Be(passwordHash);
        user.Role.Should().Be(role);
    }

    [Test]
    public void Create_should_return_successful_result_when_all_parameters_are_valid()
    {
        // Arrange
        var login = "user01";
        var passwordHash = "AQAAAAEAACcQAAAAE...";
        var role = UserRole.User;

        // Act
        var result = User.Create(login, passwordHash, role);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Login.Should().Be(login);
        result.Value.PasswordHash.Should().Be(passwordHash);
        result.Value.Role.Should().Be(role);
    }

    [Test]
    public void Create_should_generate_new_id()
    {
        // Arrange
        var login = "user01";
        var passwordHash = "AQAAAAEAACcQAAAAE...";

        // Act
        var user1 = User.Create(login, passwordHash, UserRole.User);
        var user2 = User.Create(login, passwordHash, UserRole.User);

        // Assert
        user1.Value.Id.Should().NotBe(user2.Value.Id);
    }

    [Test]
    public void Create_should_return_failure_when_login_is_null_or_whitespace()
    {
        // Act
        var result = User.Create("   ", "AQAAAAEAACcQAAAAE...", UserRole.User);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Create_should_return_failure_when_passwordHash_is_null_or_whitespace()
    {
        // Act
        var result = User.Create("user01", " ", UserRole.User);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void LoadFromStorage_should_throw_when_login_is_null_or_whitespace()
    {
        // Act
        var act = () => User.LoadFromStorage(Guid.CreateVersion7(), "", "hash", UserRole.User);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("login");
    }

    [Test]
    public void LoadFromStorage_should_throw_when_passwordHash_is_null_or_whitespace()
    {
        // Act
        var act = () => User.LoadFromStorage(Guid.CreateVersion7(), "user01", "   ", UserRole.User);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("passwordHash");
    }
}