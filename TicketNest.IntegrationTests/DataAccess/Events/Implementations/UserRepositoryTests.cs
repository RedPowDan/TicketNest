using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Auth.DbContext;
using TicketNest.DataAccess.Auth.Implementations;
using TicketNest.Domain.Auth.Models.Users;
using TicketNest.IntegrationTests.Infrastructure;

namespace TicketNest.IntegrationTests.DataAccess.Events.Implementations;

[Collection("Database")]
public class UserRepositoryTests
{
    private readonly PostgreSqlContainerFixture _fixture;

    public UserRepositoryTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private AuthDbContext CreateDbContext() =>
        new AuthDbContext(
            new DbContextOptionsBuilder<AuthDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

    private static User CreateTestUser(string? login = null)
    {
        login ??= $"user-{Guid.CreateVersion7()}";
        return User.LoadFromStorage(Guid.CreateVersion7(), login, "AQAAAAEAACcQAAAAE...", UserRole.User);
    }

    [Fact]
    public async Task Save_should_create_new_user_when_user_does_not_exist()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser();

        await repository.Save(user);

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
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser();
        await repository.Save(user);

        var changedUser = User.LoadFromStorage(user.Id, user.Login, "new-password-hash", UserRole.Admin);
        await repository.Save(changedUser);

        await using var assertContext = CreateDbContext();
        var updated = await assertContext.Users.FindAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.PasswordHash.Should().Be("new-password-hash");
        updated.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public async Task Save_should_throw_when_login_is_not_unique()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var firstUser = CreateTestUser(login: "duplicate-login");
        await repository.Save(firstUser);

        var secondUser = User.LoadFromStorage(Guid.CreateVersion7(), "duplicate-login", "hash-2", UserRole.User);

        var act = async () => await repository.Save(secondUser);

        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task Get_should_return_user_when_user_exists()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser(login: "findable-user");
        await repository.Save(user);

        var result = await repository.Get(user.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Login.Should().Be("findable-user");
        result.PasswordHash.Should().Be(user.PasswordHash);
        result.Role.Should().Be(user.Role);
    }

    [Fact]
    public async Task Get_should_return_null_when_user_does_not_exist()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        var result = await repository.Get(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByLogin_should_return_user_when_user_exists()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);
        var user = CreateTestUser(login: "present-user");
        await repository.Save(user);

        var result = await repository.GetByLogin("present-user");

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.Login.Should().Be("present-user");
    }

    [Fact]
    public async Task GetByLogin_should_return_null_when_user_does_not_exist()
    {
        await using var dbContext = CreateDbContext();
        var repository = new UserRepository(dbContext);

        var result = await repository.GetByLogin("missing-user");

        result.Should().BeNull();
    }
}
