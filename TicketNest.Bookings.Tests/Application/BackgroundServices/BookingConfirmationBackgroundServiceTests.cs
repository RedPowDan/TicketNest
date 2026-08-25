using NSubstitute;
using TicketNest.Application.Bookings.BackgroundServices;
using TicketNest.Application.Bookings.Services;
using TicketNest.Contracts.Kafka.Messages;
using TicketNest.Domain.Bookings.Models;
using TicketNest.Domain.Bookings.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.Bookings.Tests.Application.BackgroundServices;

[TestFixture]
public class BookingConfirmationBackgroundServiceTests
{
    private IBookingConfirmationService _confirmationService = null!;
    private IBookingsConsumer _consumer = null!;
    private BookingConfirmationBackgroundService _service = null!;
    private Func<BookingApprovedMessage, CancellationToken, Task>? _capturedHandler;

    [SetUp]
    public void SetUp()
    {
        _confirmationService = Substitute.For<IBookingConfirmationService>();
        _consumer = Substitute.For<IBookingsConsumer>();
        _consumer
            .HandleBookingApprovedMessage(
                Arg.Any<Func<BookingApprovedMessage, CancellationToken, Task>>(),
                Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                _capturedHandler = ci.Arg<Func<BookingApprovedMessage, CancellationToken, Task>>();
                return Task.CompletedTask;
            });

        _service = new BookingConfirmationBackgroundService(_consumer, _confirmationService);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    [Test]
    public async Task HandleMessage_Should_Confirm_When_BookingApprovedMessageReceived()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var message = new BookingApprovedMessage(bookingId, Guid.CreateVersion7());

        _confirmationService.Confirm(bookingId, Arg.Any<CancellationToken>())
            .Returns(UnitResult<Error>.FromSuccess());

        await _service.StartAsync(CancellationToken.None);

        // Act
        await _capturedHandler!(message, CancellationToken.None);

        // Assert
        await _confirmationService.Received(1).Confirm(bookingId, Arg.Any<CancellationToken>());
    }
}
