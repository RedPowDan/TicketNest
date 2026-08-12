using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Events.DbContext;
using TicketNest.DataAccess.Events.Implementations;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.Domain.Models.Bookings;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.Models.Users;

namespace TicketNest.UnitTests.DataAccess.Events.Repositories;

[TestFixture]
public class BookingRepositoryTests
{
    private readonly string _databaseName = $"BookingRepositoryTests_{Guid.NewGuid()}";

    private EventsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<EventsDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new EventsDbContext(options);
    }

    private static Event CreateEvent(EventsDbContext dbContext, int totalSeats = 100)
    {
        var eventResult = Event.Create("Test Event", null, DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), totalSeats);
        var @event = eventResult.Value;
        dbContext.Events.Add(EventMapper.ToPersistence(@event));
        dbContext.SaveChanges();
        return @event;
    }

    private static User CreateUser(EventsDbContext dbContext)
    {
        var user = User.Create("user01", "hash", UserRole.User).Value;
        dbContext.Users.Add(UserMapper.ToPersistence(user));
        dbContext.SaveChanges();
        return user;
    }

    private static Booking CreateBooking(Guid eventId, Guid userId)
    {
        return Booking.LoadFromStorage(
            id: Guid.NewGuid(),
            eventId: eventId,
            userId: userId,
            status: BookingStatus.Pending,
            createdAt: DateTime.UtcNow,
            processedAt: null);
    }

    [Test]
    public async Task Save_Should_CreateNewBooking_When_BookingDoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = CreateEvent(dbContext);
        var user = CreateUser(dbContext);

        var booking = CreateBooking(@event.Id, user.Id);

        // Act
        await repository.Save(booking);

        // Assert
        var saved = await dbContext.Bookings.FindAsync(booking.Id);
        saved.Should().NotBeNull();
        saved!.EventId.Should().Be(@event.Id);
        saved.UserId.Should().Be(user.Id);
        saved.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public async Task Save_Should_UpdateExistingBooking_When_BookingExists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = CreateEvent(dbContext);
        var user = CreateUser(dbContext);

        var booking = CreateBooking(@event.Id, user.Id);
        await repository.Save(booking);

        booking.Confirm(DateTime.UtcNow);

        // Act
        await repository.Save(booking);

        // Assert
        var updated = await dbContext.Bookings.FindAsync(booking.Id);
        updated!.Status.Should().Be(BookingStatus.Confirmed);
        updated.ProcessedAt.Should().NotBeNull();
    }

    [Test]
    public async Task Get_Should_ReturnBooking_When_Exists()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);
        var @event = CreateEvent(dbContext);
        var user = CreateUser(dbContext);

        var booking = CreateBooking(@event.Id, user.Id);
        await repository.Save(booking);

        // Act
        var result = await repository.Get(booking.Id);

        // Assert
        result.Should().NotBeNull();
        result!.EventId.Should().Be(@event.Id);
        result.UserId.Should().Be(user.Id);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public async Task Get_Should_ReturnNull_When_DoesNotExist()
    {
        // Arrange
        await using var dbContext = CreateDbContext();
        var repository = new BookingRepository(dbContext);

        // Act
        var result = await repository.Get(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }
}