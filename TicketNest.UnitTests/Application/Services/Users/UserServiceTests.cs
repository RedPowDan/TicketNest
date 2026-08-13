using FluentAssertions;
using NSubstitute;
using TicketNest.Application.Services.Users;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Users;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Auth;
using TicketNest.Domain.Services.Users;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Application.Services.Users;

[TestFixture]
public class UserServiceTests
{
    private IUserFactory _userFactory = null!;
    private IUserRepository _userRepository = null!;
    private IPasswordHasher _passwordHasher = null!;
    private IJwtTokenGenerator _jwtTokenGenerator = null!;
    private UserService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _userFactory = Substitute.For<IUserFactory>();
        _userRepository = Substitute.For<IUserRepository>();
        _passwordHasher = Substitute.For<IPasswordHasher>();
        _jwtTokenGenerator = Substitute.For<IJwtTokenGenerator>();
        _service = new UserService(_userFactory, _userRepository, _passwordHasher, _jwtTokenGenerator);
    }

    [Test]
    public async Task Register_WithValidData_Should_CreateAndSaveUser()
    {
        // Arrange
        var login = "user1";
        var password = "password";
        var role = UserRole.User;
        var user = User.LoadFromStorage(Guid.CreateVersion7(), login, "hash", role);

        _userFactory.Create(login, password, role, Arg.Any<CancellationToken>())
            .Returns(Result<User, Error>.FromSuccess(user));

        // Act
        var result = await _service.Register(login, password, role, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(user);
        await _userRepository.Received(1).Save(user, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Register_WithInvalidData_Should_PropagateError_WithoutSaving()
    {
        // Arrange
        var login = "user1";
        var password = "password";

        _userFactory.Create(login, password, Arg.Any<UserRole?>(), Arg.Any<CancellationToken>())
            .Returns(Result<User, Error>.FromFailure(new Error(ErrorCode.BadRequest, "Логин уже занят")));

        // Act
        var result = await _service.Register(login, password, null, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        await _userRepository.DidNotReceive().Save(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Login_WithValidCredentials_Should_ReturnToken()
    {
        // Arrange
        var login = "user1";
        var password = "password";
        var userId = Guid.CreateVersion7();
        var user = User.LoadFromStorage(userId, login, "hash", UserRole.User);

        _userRepository.GetByLogin(login, Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(true);
        _jwtTokenGenerator.GenerateToken(Arg.Any<TokenUser>())
            .Returns("jwt-token");

        // Act
        var result = await _service.Login(login, password, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("jwt-token");
        _jwtTokenGenerator.Received(1).GenerateToken(
            Arg.Is<TokenUser>(t => t.Id == userId && t.Login == login && t.Role == "User"));
    }

    [Test]
    public async Task Login_WithUnknownLogin_Should_ReturnBadRequest()
    {
        // Arrange
        _userRepository.GetByLogin(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        var result = await _service.Login("missing", "password", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<TokenUser>());
    }

    [Test]
    public async Task Login_WithWrongPassword_Should_ReturnBadRequest()
    {
        // Arrange
        var user = User.LoadFromStorage(Guid.CreateVersion7(), "user1", "hash", UserRole.User);

        _userRepository.GetByLogin("user1", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>())
            .Returns(false);

        // Act
        var result = await _service.Login("user1", "wrong", CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
        _jwtTokenGenerator.DidNotReceive().GenerateToken(Arg.Any<TokenUser>());
    }
}
