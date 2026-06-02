using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TicketNest.Application.BackgroundServices;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Bookings;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Application.BackgroundServices;

[TestFixture]
public class BookingConfirmationBackgroundServiceTests
{
    private IBookingConfirmationService _confirmationService = null!;
    private IQueueMessageRepository _queueMessageRepository = null!;
    private ILogger<BookingConfirmationBackgroundService> _logger = null!;
    private BookingConfirmationBackgroundService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _confirmationService = Substitute.For<IBookingConfirmationService>();
        _queueMessageRepository = Substitute.For<IQueueMessageRepository>();
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
    public async Task HandleMessage_Should_Commit_When_ConfirmationSucceeds()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        var message = QueueMessage<BookingCreatedMessage>.LoadFromStorage(
            queueName: QueueNames.BookingQueue,
            messageId: messageId,
            data: new BookingCreatedMessage(bookingId));

        _confirmationService.Confirm(bookingId, Arg.Any<CancellationToken>())
            .Returns(UnitResult<Error>.FromSuccess());

        // Act
        await _service.HandleMessage(message, _confirmationService, _queueMessageRepository, CancellationToken.None);

        // Assert
        await _queueMessageRepository.Received(1).Commit(messageId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleMessage_Should_NotCommit_When_ConfirmationFails()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        var message = QueueMessage<BookingCreatedMessage>.LoadFromStorage(
            queueName: QueueNames.BookingQueue,
            messageId: messageId,
            data: new BookingCreatedMessage(bookingId));

        _confirmationService.Confirm(bookingId, Arg.Any<CancellationToken>())
            .Returns(UnitResult<Error>.FromFailure(new Error(ErrorCode.Conflict, "Seats unavailable")));

        // Act
        await _service.HandleMessage(message, _confirmationService, _queueMessageRepository, CancellationToken.None);

        // Assert
        await _queueMessageRepository.DidNotReceive().Commit(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
