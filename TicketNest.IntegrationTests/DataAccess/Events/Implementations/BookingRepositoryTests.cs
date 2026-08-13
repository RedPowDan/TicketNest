using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Models.Users;
using TicketNest.IntegrationTests.Infrastructure;

namespace TicketNest.IntegrationTests.DataAccess.Events.Implementations;

[Collection("Database")]
public class BookingRepositoryTests : EventsPgDatabaseTestBase
{
    public BookingRepositoryTests(PostgreSqlContainerFixture fixture) : base(fixture)
    {
    }

    #region Save

    [Fact]
    public async Task Save_with_null_booking_should_throw_ArgumentNullException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);

        // Act
        var act = async () => await repository.Save(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Save_booking_with_nonexistent_EventId_should_throw_DbUpdateException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var user = await CreateUser(dbContext);

        var booking = CreateBooking(eventId: Guid.NewGuid(), userId: user.Id);

        // Act
        var act = async () => await repository.Save(booking);

        // Assert
        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Save_booking_with_cancelled_token_should_throw_OperationCanceledException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = await CreateEvent(dbContext);
        var user = await CreateUser(dbContext);

        var booking = CreateBooking(@event.Id, user.Id);

        var ct = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.Save(booking, ct);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Save_multiple_bookings_for_same_event_should_succeed()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = await CreateEvent(dbContext);
        var user = await CreateUser(dbContext);

        var booking1 = CreateBooking(@event.Id, user.Id);

        var booking2 = CreateBooking(@event.Id, user.Id);

        // Act
        await repository.Save(booking1);
        await repository.Save(booking2);

        // Assert
        var saved1 = await dbContext.Bookings.FindAsync(booking1.Id);
        var saved2 = await dbContext.Bookings.FindAsync(booking2.Id);

        saved1.Should().NotBeNull();
        saved2.Should().NotBeNull();
        saved1!.EventId.Should().Be(@event.Id);
        saved2!.EventId.Should().Be(@event.Id);
    }

    [Fact]
    public async Task Save_duplicate_id_should_update_existing_instead_of_creating_new()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = await CreateEvent(dbContext);
        var user = await CreateUser(dbContext);

        var bookingId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var booking = CreateBooking(@event.Id, user.Id, id: bookingId);

        await repository.Save(booking);

        var updatedBooking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: @event.Id,
            userId: user.Id,
            status: BookingStatus.Confirmed,
            createdAt: createdAt,
            processedAt: DateTime.UtcNow);

        // Act
        await repository.Save(updatedBooking);

        // Assert
        var saved = await dbContext.Bookings.FindAsync(bookingId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(BookingStatus.Confirmed);
        saved.ProcessedAt.Should().NotBeNull();

        var count = await dbContext.Bookings.CountAsync(b => b.Id == bookingId);
        count.Should().Be(1);
    }

    #endregion

    #region Get

    [Fact]
    public async Task Get_with_cancelled_token_should_throw_OperationCanceledException()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);

        var ct = new CancellationToken(canceled: true);

        // Act
        var act = async () => await repository.Get(Guid.NewGuid(), ct);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Get_should_return_updated_booking_after_Save_update()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = await CreateEvent(dbContext);
        var user = await CreateUser(dbContext);

        var bookingId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var booking = CreateBooking(@event.Id, user.Id, id: bookingId);

        await repository.Save(booking);

        var updatedBooking = Booking.LoadFromStorage(
            id: bookingId,
            eventId: @event.Id,
            userId: user.Id,
            status: BookingStatus.Confirmed,
            createdAt: createdAt,
            processedAt: DateTime.UtcNow);

        await repository.Save(updatedBooking);

        // Act
        var result = await repository.Get(bookingId);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(BookingStatus.Confirmed);
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_booking_with_Rejected_status_then_Get_should_return_Rejected()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = await CreateEvent(dbContext);
        var user = await CreateUser(dbContext);

        var booking = CreateBooking(@event.Id, user.Id);

        await repository.Save(booking);

        booking.Reject(DateTime.UtcNow);
        await repository.Save(booking);

        // Act
        var result = await repository.Get(booking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be(BookingStatus.Rejected);
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_and_Get_should_preserve_all_booking_properties()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = await CreateEvent(dbContext);
        var user = await CreateUser(dbContext);

        var bookingId = Guid.NewGuid();
        var eventId = @event.Id;
        var userId = user.Id;
        var createdAt = DateTime.UtcNow;

        var booking = CreateBooking(eventId, userId, id: bookingId);

        await repository.Save(booking);

        // Act
        var result = await repository.Get(bookingId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(bookingId);
        result.EventId.Should().Be(eventId);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be(BookingStatus.Pending);
        result.CreatedAt.Should().BeCloseTo(createdAt, TimeSpan.FromSeconds(1));
        result.ProcessedAt.Should().BeNull();
    }

    [Fact]
    public async Task Get_should_return_null_for_Empty_Guid()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);

        // Act
        var result = await repository.Get(Guid.Empty);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    private static async Task<Event> CreateEvent(EventsDbContext dbContext, int totalSeats = 100)
    {
        var @event = Event.Create("Test Event", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), totalSeats).Value;
        dbContext.Events.Add(EventMapper.ToPersistence(@event));
        await dbContext.SaveChangesAsync();
        return @event;
    }

    private static async Task<User> CreateUser(EventsDbContext dbContext)
    {
        var user = User.Create("booking-user", "hash", UserRole.User).Value;
        dbContext.Users.Add(UserMapper.ToPersistence(user));
        await dbContext.SaveChangesAsync();
        return user;
    }

    private static Booking CreateBooking(Guid eventId, Guid userId, Guid? id = null, BookingStatus status = BookingStatus.Pending, DateTime? processedAt = null)
    {
        return Booking.LoadFromStorage(
            id: id ?? Guid.NewGuid(),
            eventId: eventId,
            userId: userId,
            status: status,
            createdAt: DateTime.UtcNow,
            processedAt: processedAt);
    }
}
