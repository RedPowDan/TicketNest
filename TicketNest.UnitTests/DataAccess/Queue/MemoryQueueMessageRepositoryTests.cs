using FluentAssertions;
using TicketNest.DataAccess.Queue.Implementations;
using TicketNest.Domain.Constants;
using TicketNest.Domain.Models.Queue;
using TicketNest.Domain.Models.Queue.QueueMessageModels;

namespace TicketNest.UnitTests.DataAccess.Queue;

[TestFixture]
public class MemoryQueueMessageRepositoryTests
{
    private MemoryQueueMessageRepository _repository = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new MemoryQueueMessageRepository();
    }

    [Test]
    public async Task Create_Should_StoreMessage()
    {
        // Arrange
        var message = QueueMessage<BookingCreatedMessage>.Create(
            QueueNames.BookingQueue,
            new BookingCreatedMessage(Guid.NewGuid()));

        // Act
        await _repository.Create(message);

        // Assert
        var retrieved = await _repository.Get<BookingCreatedMessage>(QueueNames.BookingQueue);
        retrieved.Should().NotBeNull();
        retrieved!.Data.BookingId.Should().Be(message.Data.BookingId);
    }

    [Test]
    public async Task Get_Should_ReturnMessage_When_Exists()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var message = QueueMessage<BookingCreatedMessage>.Create(
            QueueNames.BookingQueue,
            new BookingCreatedMessage(bookingId));
        await _repository.Create(message);

        // Act
        var result = await _repository.Get<BookingCreatedMessage>(QueueNames.BookingQueue);

        // Assert
        result.Should().NotBeNull();
        result!.Data.BookingId.Should().Be(bookingId);
        result.QueueName.Should().Be(QueueNames.BookingQueue);
    }

    [Test]
    public async Task Get_Should_ReturnNull_When_QueueIsEmpty()
    {
        // Arrange & Act
        var result = await _repository.Get<BookingCreatedMessage>("NonExistentQueue");

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task Get_Should_DequeueMessage()
    {
        // Arrange
        var message = QueueMessage<BookingCreatedMessage>.Create(
            QueueNames.BookingQueue,
            new BookingCreatedMessage(Guid.NewGuid()));
        await _repository.Create(message);

        // Act
        var first = await _repository.Get<BookingCreatedMessage>(QueueNames.BookingQueue);
        var second = await _repository.Get<BookingCreatedMessage>(QueueNames.BookingQueue);

        // Assert
        first.Should().NotBeNull();
        second.Should().BeNull();
    }

    [Test]
    public async Task GetAll_Should_ReturnAllMessages()
    {
        // Arrange
        for (var i = 0; i < 3; i++)
        {
            var message = QueueMessage<BookingCreatedMessage>.Create(
                QueueNames.BookingQueue,
                new BookingCreatedMessage(Guid.NewGuid()));
            await _repository.Create(message);
        }

        // Act
        var messages = await _repository.GetAll<BookingCreatedMessage>(QueueNames.BookingQueue);

        // Assert
        messages.Should().HaveCount(3);
    }

    [Test]
    public async Task Commit_Should_NotThrow()
    {
        // Arrange & Act
        var act = () => _repository.Commit(Guid.NewGuid());

        // Assert
        await act.Should().NotThrowAsync();
    }
}
