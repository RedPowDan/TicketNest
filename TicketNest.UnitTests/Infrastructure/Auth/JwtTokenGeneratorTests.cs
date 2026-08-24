using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TicketNest.Domain.Auth.Services.Auth;
using TicketNest.Infrastructure.Auth;

namespace TicketNest.UnitTests.Infrastructure.Auth;

[TestFixture]
public class JwtTokenGeneratorTests
{
    private JwtTokenGenerator _tokenGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        var options = Options.Create(CreateTokenOptions());
        _tokenGenerator = new JwtTokenGenerator(options);
    }

    private static JwtOptions CreateTokenOptions()
    {
        return new JwtOptions
        {
            Secret = "COyKb3tyro3pG1EzM7wZf7x2Jk4uN9vHsiL6aDq0cPr5",
            Issuer = "TicketNest",
            Audience = "TicketNest.Api",
            LifetimeMinutes = 60,
        };
    }

    private static TokenUser CreateTokenUser(Guid? userId = null, string? login = null, string? role = null)
    {
        return TokenUser.Create(
            id: userId ?? Guid.CreateVersion7(),
            login: login ?? "user01",
            role: role ?? "Customer");
    }

    private static TokenValidationParameters CreateValidationParameters(string? secret = null)
    {
        var tokenSecret = secret ?? CreateTokenOptions().Secret;

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret)),
            ValidIssuer = CreateTokenOptions().Issuer,
            ValidAudience = CreateTokenOptions().Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
    }

    [Test]
    public void Generate_token_should_return_token_in_valid_jwt_format()
    {
        // Arrange
        var user = CreateTokenUser();

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        token.Split('.').Should().HaveCount(3);
    }

    [Test]
    public void Generate_token_should_create_token_that_can_be_validated_with_configured_secret()
    {
        // Arrange
        var user = CreateTokenUser();
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = CreateValidationParameters();

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var principal = handler.ValidateToken(token, validationParameters, out _);
        principal.Should().NotBeNull();
    }

    [Test]
    public void Generate_token_should_create_token_that_fails_validation_with_different_secret()
    {
        // Arrange
        var user = CreateTokenUser();
        var handler = new JwtSecurityTokenHandler();
        var validationParameters = CreateValidationParameters(secret: "another-secret-that-does-not-match-fully");

        // Act
        var token = _tokenGenerator.GenerateToken(user);

        // Assert
        var act = () => handler.ValidateToken(token, validationParameters, out _);
        act.Should().Throw<SecurityTokenSignatureKeyNotFoundException>();
    }

    [Test]
    public void Generate_token_should_embed_user_id_in_token()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var user = CreateTokenUser(userId: userId);

        // Act
        var token = _tokenGenerator.GenerateToken(user);
        var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jsonToken.Subject.Should().Be(userId.ToString());
    }

    [Test]
    public void Generate_token_should_embed_login_in_token()
    {
        // Arrange
        var user = CreateTokenUser();

        // Act
        var token = _tokenGenerator.GenerateToken(user);
        var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jsonToken.Claims
            .Single(c => c.Type == ClaimTypes.Name)
            .Value.Should().Be(user.Login);
    }

    [Test]
    public void Generate_token_should_embed_role_in_token()
    {
        // Arrange
        var user = CreateTokenUser(role: "Organizer");

        // Act
        var token = _tokenGenerator.GenerateToken(user);
        var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jsonToken.Claims
            .Single(c => c.Type == ClaimTypes.Role)
            .Value.Should().Be(user.Role);
    }

    [Test]
    public void Generate_token_should_set_issuer_and_audience_in_token()
    {
        // Arrange
        var user = CreateTokenUser();

        // Act
        var token = _tokenGenerator.GenerateToken(user);
        var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jsonToken.Issuer.Should().Be(CreateTokenOptions().Issuer);
        jsonToken.Audiences.Should().ContainSingle().Which.Should().Be(CreateTokenOptions().Audience);
    }

    [Test]
    public void Generate_token_should_set_expiration_within_lifetime()
    {
        // Arrange
        var user = CreateTokenUser();
        var beforeGeneration = DateTime.UtcNow;
        var lifetimeMinutes = CreateTokenOptions().LifetimeMinutes;

        // Act
        var token = _tokenGenerator.GenerateToken(user);
        var jsonToken = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        jsonToken.ValidTo.Should().BeAfter(beforeGeneration);
        jsonToken.ValidTo.Should().BeBefore(beforeGeneration.AddMinutes(lifetimeMinutes + 1));
    }
}