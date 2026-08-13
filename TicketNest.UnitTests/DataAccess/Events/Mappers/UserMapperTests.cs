using FluentAssertions;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Users;

namespace TicketNest.UnitTests.DataAccess.Events.Mappers;

[TestFixture]
public class UserMapperTests
{
    [Test]
    public void To_domain_should_map_all_properties_correctly()
    {
        var persistenceUser = new PersistenceUser
        {
            Id = Guid.NewGuid(),
            Login = "user01",
            PasswordHash = "AQAAAAEAACcQAAAAE...",
            Role = UserRole.Admin
        };

        var domainUser = UserMapper.ToDomain(persistenceUser);

        domainUser.Id.Should().Be(persistenceUser.Id);
        domainUser.Login.Should().Be(persistenceUser.Login);
        domainUser.PasswordHash.Should().Be(persistenceUser.PasswordHash);
        domainUser.Role.Should().Be(persistenceUser.Role);
    }

    [Test]
    public void To_domain_should_throw_when_source_is_null()
    {
        var act = () => UserMapper.ToDomain(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void To_domain_should_map_user_role()
    {
        var persistenceUser = new PersistenceUser
        {
            Id = Guid.NewGuid(),
            Login = "user01",
            PasswordHash = "AQAAAAEAACcQAAAAE...",
            Role = UserRole.User
        };

        var domainUser = UserMapper.ToDomain(persistenceUser);

        domainUser.Role.Should().Be(UserRole.User);
    }

    [Test]
    public void To_persistence_should_map_all_properties_correctly()
    {
        var userId = Guid.NewGuid();
        var login = "user01";
        var passwordHash = "AQAAAAEAACcQAAAAE...";
        var domainUser = User.LoadFromStorage(
            id: userId,
            login: login,
            passwordHash: passwordHash,
            role: UserRole.Admin);

        var persistenceUser = UserMapper.ToPersistence(domainUser);

        persistenceUser.Id.Should().Be(userId);
        persistenceUser.Login.Should().Be(login);
        persistenceUser.PasswordHash.Should().Be(passwordHash);
        persistenceUser.Role.Should().Be(UserRole.Admin);
    }

    [Test]
    public void To_persistence_should_throw_when_source_is_null()
    {
        var act = () => UserMapper.ToPersistence(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Map_should_update_target_properties()
    {
        var persistenceUser = new PersistenceUser
        {
            Id = Guid.NewGuid(),
            Login = "old_login",
            PasswordHash = "old_hash",
            Role = UserRole.User
        };
        var domainUser = User.LoadFromStorage(
            id: persistenceUser.Id,
            login: "new_login",
            passwordHash: "new_hash",
            role: UserRole.Admin);

        UserMapper.Map(domainUser, persistenceUser);

        persistenceUser.Id.Should().Be(domainUser.Id);
        persistenceUser.Login.Should().Be("new_login");
        persistenceUser.PasswordHash.Should().Be("new_hash");
        persistenceUser.Role.Should().Be(UserRole.Admin);
    }

    [Test]
    public void Map_should_throw_when_target_is_null()
    {
        var domainUser = User.LoadFromStorage(Guid.NewGuid(), "user01", "hash", UserRole.User);

        var act = () => UserMapper.Map(domainUser, null!);

        act.Should().Throw<ArgumentNullException>();
    }
}