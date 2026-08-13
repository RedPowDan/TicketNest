using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TicketNest.Application.BackgroundServices;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;
using TicketNest.Domain.Repositories;
using TicketNest.Domain.Services.Events;
using TicketNest.Shared.Objects;

namespace TicketNest.UnitTests.Application.BackgroundServices;

[TestFixture]
public class BookingCancellationBackgroundServiceTests
{
    private IEventReleaseSeatsService _releaseSeatsService = null!;
    private IQueueMessageRepository _queueMessageRepository = null!;
    private ILogger<BookingCancellationBackgroundService> _logger = null!;
    private BookingCancellationBackgroundService _service = null!;
    private IServiceScope _scope = null!;

    [SetUp]
    public void SetUp()
    {
        _releaseSeatsService = Substitute.For<IEventReleaseSeatsService>();
        _queueMessageRepository = Substitute.For<IQueueMessageRepository>();
        _logger = Substitute.For<ILogger<BookingCancellationBackgroundService>>();

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IEventReleaseSeatsService)).Returns(_releaseSeatsService);

        _scope = Substitute.For<IServiceScope>();
        _scope.ServiceProvider.Returns(serviceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(_scope);

        _service = new BookingCancellationBackgroundService(scopeFactory, _logger);
    }

    [TearDown]
    public void TearDown()
    {
        _service.Dispose();
        _scope.Dispose();
    }

    [Test]
    public async Task HandleMessage_Should_Commit_When_ReleaseSucceeds()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        var message = QueueMessage<BookingCanceledMessage>.LoadFromStorage(
            queueName: QueueNames.BookingQueue,
            messageId: messageId,
            data: new BookingCanceledMessage(bookingId));

        _releaseSeatsService.ReleaseSeats(bookingId, ct: Arg.Any<CancellationToken>())
            .Returns(UnitResult<Error>.FromSuccess());

        // Act
        await _service.HandleMessage(message, _queueMessageRepository, CancellationToken.None);

        // Assert
        await _releaseSeatsService.Received(1).ReleaseSeats(bookingId, ct: Arg.Any<CancellationToken>());
        await _queueMessageRepository.Received(1).Commit(messageId, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task HandleMessage_Should_Commit_When_ReleaseFails()
    {
        // Arrange
        var bookingId = Guid.CreateVersion7();
        var messageId = Guid.CreateVersion7();
        var message = QueueMessage<BookingCanceledMessage>.LoadFromStorage(
            queueName: QueueNames.BookingQueue,
            messageId: messageId,
            data: new BookingCanceledMessage(bookingId));

        _releaseSeatsService.ReleaseSeats(bookingId, ct: Arg.Any<CancellationToken>())
            .Returns(UnitResult<Error>.FromFailure(new Error(ErrorCode.BadRequest, "Невозможно вернуть бронь")));

        // Act
        await _service.HandleMessage(message, _queueMessageRepository, CancellationToken.None);

        // Assert
        await _releaseSeatsService.Received(1).ReleaseSeats(bookingId, ct: Arg.Any<CancellationToken>());
        await _queueMessageRepository.Received(1).Commit(messageId, Arg.Any<CancellationToken>());
    }
}
