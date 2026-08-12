using System.Security.Claims;
using FluentAssertions;
using TicketNest.Domain.Services.Auth;
using TicketNest.Infrastructure.Auth;

namespace TicketNest.UnitTests.Infrastructure.Auth;

[TestFixture]
public class JwtTokenReaderTests
{
    private JwtTokenReader _reader = null!;

    [SetUp]
    public void SetUp()
    {
        _reader = new JwtTokenReader();
    }

    private static ClaimsPrincipal CreatePrincipal(
        bool isAuthenticated,
        string? id = null,
        string? login = null,
        string? role = null)
    {
        var claims = new List<Claim>();
        if (id is not null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id));
        }

        if (login is not null)
        {
            claims.Add(new Claim(ClaimTypes.Name, login));
        }

        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var identity = new ClaimsIdentity(claims, isAuthenticated ? "test" : null);
        return new ClaimsPrincipal(identity);
    }

    [Test]
    public void Read_should_return_user_when_all_claims_present()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var principal = CreatePrincipal(isAuthenticated: true, id: userId.ToString(), login: "user01", role: "Customer");

        // Act
        var user = _reader.Read(principal);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
        user.Login.Should().Be("user01");
        user.Role.Should().Be("Customer");
    }

    [Test]
    public void Read_should_return_null_when_principal_not_authenticated()
    {
        // Arrange
        var principal = CreatePrincipal(isAuthenticated: false, id: Guid.CreateVersion7().ToString(), login: "user01", role: "Customer");

        // Act
        var user = _reader.Read(principal);

        // Assert
        user.Should().BeNull();
    }

    [Test]
    public void Read_should_return_null_when_required_claim_missing()
    {
        // Arrange
        var principal = CreatePrincipal(isAuthenticated: true, id: Guid.CreateVersion7().ToString(), login: "user01");

        // Act
        var user = _reader.Read(principal);

        // Assert
        user.Should().BeNull();
    }

    [Test]
    public void Read_should_use_sub_claim_when_name_identifier_absent()
    {
        // Arrange
        var userId = Guid.CreateVersion7();
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNamesSub(), userId.ToString()),
            new(ClaimTypes.Name, "user01"),
            new(ClaimTypes.Role, "Customer"),
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));

        // Act
        var user = _reader.Read(principal);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(userId);
    }

    [Test]
    public void Read_should_return_null_when_id_is_not_a_guid()
    {
        // Arrange
        var principal = CreatePrincipal(isAuthenticated: true, id: "not-a-guid", login: "user01", role: "Customer");

        // Act
        var user = _reader.Read(principal);

        // Assert
        user.Should().BeNull();
    }

    private static string JwtRegisteredClaimNamesSub()
    {
        return System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub;
    }
}
