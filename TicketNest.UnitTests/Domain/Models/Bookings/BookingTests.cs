using FluentAssertions;
using TicketNest.Domain.Bookings.Constants;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Models.Users;

namespace TicketNest.UnitTests.Domain.Models.Bookings;

[TestFixture]
public class BookingTests
{
    private static Booking CreateBooking(Guid? userId = null, BookingStatus status = BookingStatus.Pending) =>
        Booking.LoadFromStorage(
            id: Guid.CreateVersion7(),
            eventId: Guid.CreateVersion7(),
            userId: userId ?? Guid.CreateVersion7(),
            status: status,
            createdAt: DateTime.UtcNow.AddMinutes(-10),
            processedAt: null);

    private static User CreateUser(Guid id, UserRole role) =>
        User.LoadFromStorage(id, "login", "hash", role);

    [Test]
    public void LoadFromStorage_should_return_booking_with_given_values()
    {
        // Arrange
        var id = Guid.CreateVersion7();
        var eventId = Guid.CreateVersion7();
        var userId = Guid.CreateVersion7();
        var createdAt = DateTime.UtcNow.AddMinutes(-5);

        // Act
        var booking = Booking.LoadFromStorage(id, eventId, userId, BookingStatus.Pending, createdAt, null);

        // Assert
        booking.Id.Should().Be(id);
        booking.EventId.Should().Be(eventId);
        booking.UserId.Should().Be(userId);
        booking.Status.Should().Be(BookingStatus.Pending);
        booking.CreatedAt.Should().Be(createdAt);
        booking.ProcessedAt.Should().BeNull();
    }

    [Test]
    public void Cancel_should_set_status_to_canceled_and_processed_at()
    {
        // Arrange
        var booking = CreateBooking();
        var processedAt = DateTime.UtcNow;

        // Act
        booking.Cancel(processedAt);

        // Assert
        booking.Status.Should().Be(BookingStatus.Canceled);
        booking.ProcessedAt.Should().Be(processedAt);
    }

    [Test]
    public void CanCancel_should_succeed_for_owner()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();
        var booking = CreateBooking(userId: ownerId);
        var owner = CreateUser(ownerId, UserRole.User);

        // Act
        var result = booking.CanCancel(owner.Id, owner.Role);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void CanCancel_should_succeed_for_admin_even_if_not_owner()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();
        var adminId = Guid.CreateVersion7();
        var booking = CreateBooking(userId: ownerId);
        var admin = CreateUser(adminId, UserRole.Admin);

        // Act
        var result = booking.CanCancel(admin.Id, admin.Role);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public void CanCancel_should_return_forbidden_for_other_regular_user()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();
        var otherId = Guid.CreateVersion7();
        var booking = CreateBooking(userId: ownerId);
        var other = CreateUser(otherId, UserRole.User);

        // Act
        var result = booking.CanCancel(other.Id, other.Role);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.Forbidden);
    }

    [Test]
    public void CanCancel_should_return_bad_request_when_already_canceled()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();
        var booking = CreateBooking(userId: ownerId, status: BookingStatus.Canceled);
        var owner = CreateUser(ownerId, UserRole.User);

        // Act
        var result = booking.CanCancel(owner.Id, owner.Role);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
    }

    [Test]
    public void CanCancel_should_return_bad_request_when_rejected()
    {
        // Arrange
        var ownerId = Guid.CreateVersion7();
        var booking = CreateBooking(userId: ownerId, status: BookingStatus.Rejected);
        var owner = CreateUser(ownerId, UserRole.User);

        // Act
        var result = booking.CanCancel(owner.Id, owner.Role);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.BadRequest);
    }
}
