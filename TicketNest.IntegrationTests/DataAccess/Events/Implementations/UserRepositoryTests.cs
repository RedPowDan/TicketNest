using FluentAssertions;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.Domain.Models.Users;
using TicketNest.IntegrationTests.Infrastructure;

namespace TicketNest.IntegrationTests.DataAccess.Events.Implementations;

[Collection("Database")]
public class UserRepositoryTests : EventsPgDatabaseTestBase
{
    public UserRepositoryTests(PostgreSqlContainerFixture fixture) : base(fixture)
    {
    }

    #region Save

    [Fact]
    public async Task Save_should_create_new_user_when_user_does_not_exist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser();

        // Act
        await repository.Save(user);

        // Assert
        await using var assertContext = CreateDbContext();
        var saved = await assertContext.Users.FindAsync(user.Id);
        saved.Should().NotBeNull();
        saved!.Login.Should().Be(user.Login);
        saved.PasswordHash.Should().Be(user.PasswordHash);
        saved.Role.Should().Be(user.Role);
    }

    [Fact]
    public async Task Save_should_update_existing_user_when_user_exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser();
        await repository.Save(user);

        var changedUser = User.LoadFromStorage(
            id: user.Id,
            login: user.Login,
            passwordHash: "new-password-hash",
            role: UserRole.Admin);

        // Act
        await repository.Save(changedUser);

        // Assert
        await using var assertContext = CreateDbContext();
        var updated = await assertContext.Users.FindAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.PasswordHash.Should().Be("new-password-hash");
        updated.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Save_should_throw_when_login_is_not_unique()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var firstUser = CreateTestUser(login: "duplicate-login");
        await repository.Save(firstUser);

        var secondUser = User.Create("duplicate-login", passwordHash: "hash-2", role: UserRole.User).Value;

        // Act
        var act = async () => await repository.Save(secondUser);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

    #endregion

    #region Get

    [Fact]
    public async Task Get_should_return_user_when_user_exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser(login: "findable-user");
        await repository.Save(user);

        // Act
        var result = await repository.Get(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Login.Should().Be("findable-user");
        result.PasswordHash.Should().Be(user.PasswordHash);
        result.Role.Should().Be(user.Role);
    }

    [Fact]
    public async Task Get_should_return_null_when_user_does_not_exist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        // Act
        var result = await repository.Get(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByLogin

    [Fact]
    public async Task GetByLogin_should_return_user_when_user_exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser(login: "present-user");
        await repository.Save(user);

        // Act
        var result = await repository.GetByLogin("present-user");

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Login.Should().Be("present-user");
    }

    [Fact]
    public async Task GetByLogin_should_return_null_when_user_does_not_exist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        // Act
        var result = await repository.GetByLogin("missing-user");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    private static User CreateTestUser(string login = "user01")
    {
        var result = User.Create(login, passwordHash: "AQAAAAEAACcQAAAAE...", role: UserRole.User);
        return result.Value;
    }
}