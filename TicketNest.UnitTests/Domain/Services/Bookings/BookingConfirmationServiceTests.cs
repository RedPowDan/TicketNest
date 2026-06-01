using FluentAssertions;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Services.Bookings;

namespace TicketNest.UnitTests.Domain.Services.Bookings;

[TestFixture]
public class BookingConfirmationServiceTests
{
    [Test]
    public async Task Confirm_Should_SetStatusToConfirmed()
    {
        var booking = Booking.LoadFromStorage(
            id: Guid.CreateVersion7(),
            eventId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);

        var service = new BookingConfirmationService();

        var result = await service.Confirm(booking, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        booking.ProcessedAt.Should().NotBeNull();
        booking.ProcessedAt.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Test]
    public async Task Confirm_Should_SetProcessedAt_AfterCreatedAt()
    {
        var createdAt = DateTime.UtcNow.AddHours(-1);
        var booking = Booking.LoadFromStorage(
            id: Guid.CreateVersion7(),
            eventId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: createdAt,
            processedAt: null);

        var service = new BookingConfirmationService();

        var result = await service.Confirm(booking, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        booking.ProcessedAt.Should().BeAfter(createdAt);
    }
}
