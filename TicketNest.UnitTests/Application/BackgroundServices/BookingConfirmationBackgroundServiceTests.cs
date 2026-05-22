using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TicketNest.Application.BackgroundServices;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Application.BackgroundServices;

[TestFixture]
public class BookingConfirmationBackgroundServiceTests
{
    private IBookingRepository _bookingRepository = null!;
    private IBookingConfirmationService _confirmationService = null!;
    private ILogger<BookingConfirmationBackgroundService> _logger = null!;
    private BookingConfirmationBackgroundService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = Substitute.For<IBookingRepository>();
        _confirmationService = Substitute.For<IBookingConfirmationService>();
        _logger = Substitute.For<ILogger<BookingConfirmationBackgroundService>>();
        _service = new BookingConfirmationBackgroundService(
            Substitute.For<IServiceProvider>(), _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
    }

    [Test]
    public async Task HandleMessage_Should_ConfirmAndSave_When_BookingExists()
    {
        var bookingId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);

        var message = new BookingCreatedMessage(bookingId);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);

        _confirmationService.Confirm(booking, Arg.Any<CancellationToken>())
            .Returns(UnitResult<Error>.FromSuccess());

        var result = await _service.HandleMessage(message, _bookingRepository, _confirmationService, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        await _bookingRepository.Received(1).Save(booking, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleMessage_Should_ReturnError_When_BookingNotFound()
    {
        var bookingId = Guid.CreateVersion7();
        var message = new BookingCreatedMessage(bookingId);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?) null);

        var result = await _service.HandleMessage(message, _bookingRepository, _confirmationService, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain(bookingId.ToString());
        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }
}
