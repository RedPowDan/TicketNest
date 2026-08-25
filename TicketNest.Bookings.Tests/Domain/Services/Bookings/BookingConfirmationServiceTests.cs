using NSubstitute;
using TicketNest.Domain.Bookings.Constants;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Repositories;
using TicketNest.Domain.Bookings.Services.Bookings;

namespace TicketNest.Bookings.Tests.Domain.Services.Bookings;

[TestFixture]
public class BookingConfirmationServiceTests
{
    private IBookingRepository _bookingRepository = null!;
    private BookingConfirmationService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _bookingRepository = Substitute.For<IBookingRepository>();
        _service = new BookingConfirmationService(_bookingRepository);
    }

    [Test]
    public async Task Confirm_Should_ConfirmAndSave_When_BookingExists()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var booking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: Guid.CreateVersion7(),
            userId: Guid.CreateVersion7(),
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow.AddMinutes(-5),
            processedAt: null);

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns(booking);

        _bookingRepository.Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.Confirm(bookingId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        booking.Status.Should().Be(BookingStatus.Confirmed);
        await _bookingRepository.Received(1).Save(booking, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task Confirm_Should_ReturnNotFound_When_BookingDoesNotExist()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();

        _bookingRepository.Get(bookingId, Arg.Any<CancellationToken>())
            .Returns((Booking?)null);

        // Act
        var result = await _service.Confirm(bookingId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.StatusCode.Should().Be(ErrorCode.NotFound);
        result.Error.Message.Should().Contain(bookingId.ToString());

        await _bookingRepository.DidNotReceive().Save(Arg.Any<Booking>(), Arg.Any<CancellationToken>());
    }
}
