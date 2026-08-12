using FluentAssertions;
using TicketNest.Domain.Services.Auth;

namespace TicketNest.UnitTests.Domain;

[TestFixture]
public class ValueObjectTests
{
    private static TokenUser CreateTokenUser() => TokenUser.Create(Guid.CreateVersion7(), "user01", "Customer");

    private static TokenUser CreateAnotherTokenUser() => TokenUser.Create(Guid.CreateVersion7(), "user02", "Organizer");

    [Test]
    public void Equals_should_return_true_when_components_are_equal()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var first = TokenUser.Create(userId, "user01", "Customer");
        var second = TokenUser.Create(userId, "user01", "Customer");

        // Act
        var areEqual = first.Equals(second);

        // Assert
        areEqual.Should().BeTrue();
    }

    [Test]
    public void Equals_should_return_false_when_components_differ()
    {
        // Arrange
        var first = CreateTokenUser();
        var second = CreateAnotherTokenUser();

        // Act
        var areEqual = first.Equals(second);

        // Assert
        areEqual.Should().BeFalse();
    }

    [Test]
    public void Equals_should_return_false_when_other_is_null()
    {
        // Arrange
        var user = CreateTokenUser();

        // Act
        var areEqual = user.Equals(null);

        // Assert
        areEqual.Should().BeFalse();
    }

    [Test]
    public void Equals_should_return_false_when_other_is_not_same_type()
    {
        // Arrange
        var user = CreateTokenUser();

        // Act
        var areEqual = user.Equals(new object());

        // Assert
        areEqual.Should().BeFalse();
    }

    [Test]
    public void Equality_operator_should_return_true_when_components_are_equal()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var first = TokenUser.Create(userId, "user01", "Customer");
        var second = TokenUser.Create(userId, "user01", "Customer");

        // Act
        var areEqual = first == second;

        // Assert
        areEqual.Should().BeTrue();
    }

    [Test]
    public void Inequality_operator_should_return_true_when_components_differ()
    {
        // Arrange
        var first = CreateTokenUser();
        var second = CreateAnotherTokenUser();

        // Act
        var areNotEqual = first != second;

        // Assert
        areNotEqual.Should().BeTrue();
    }

    [Test]
    public void GetHashCode_should_return_same_hash_for_equal_components()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var first = TokenUser.Create(userId, "user01", "Customer");
        var second = TokenUser.Create(userId, "user01", "Customer");

        // Act
        var firstHash = first.GetHashCode();
        var secondHash = second.GetHashCode();

        // Assert
        firstHash.Should().Be(secondHash);
    }
}