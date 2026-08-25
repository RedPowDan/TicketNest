using Microsoft.EntityFrameworkCore;
using NSubstitute;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Bookings.Implementations;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Repositories;

namespace TicketNest.Bookings.Tests.DataAccess.Bookings.Repositories;

[TestFixture]
public class BookingRepositoryTests
{
    private readonly string _databaseName = $"BookingRepositoryTests_{Guid.NewGuid()}";

    private BookingsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        return new BookingsDbContext(options);
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
        await using var dbContext = CreateDbContext();
        var outbox = Substitute.For<IOutboxRepository>();
        var repository = new BookingRepository(dbContext, outbox);
        var booking = CreateBooking(Guid.NewGuid(), Guid.NewGuid());

        await repository.Save(booking);

        var saved = await dbContext.Bookings.FindAsync(booking.Id);
        saved.Should().NotBeNull();
        saved!.EventId.Should().Be(booking.EventId);
        saved.UserId.Should().Be(booking.UserId);
        saved.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public async Task Save_Should_UpdateExistingBooking_When_BookingExists()
    {
        await using var dbContext = CreateDbContext();
        var outbox = Substitute.For<IOutboxRepository>();
        var repository = new BookingRepository(dbContext, outbox);
        var booking = CreateBooking(Guid.NewGuid(), Guid.NewGuid());
        await repository.Save(booking);

        booking.Confirm(DateTime.UtcNow);

        await repository.Save(booking);

        var updated = await dbContext.Bookings.FindAsync(booking.Id);
        updated!.Status.Should().Be(BookingStatus.Confirmed);
        updated.ProcessedAt.Should().NotBeNull();
    }

    [Test]
    public async Task Get_Should_ReturnBooking_When_Exists()
    {
        await using var dbContext = CreateDbContext();
        var outbox = Substitute.For<IOutboxRepository>();
        var repository = new BookingRepository(dbContext, outbox);
        var booking = CreateBooking(Guid.NewGuid(), Guid.NewGuid());
        await repository.Save(booking);

        var result = await repository.Get(booking.Id);

        result.Should().NotBeNull();
        result!.EventId.Should().Be(booking.EventId);
        result.UserId.Should().Be(booking.UserId);
        result.Status.Should().Be(BookingStatus.Pending);
    }

    [Test]
    public async Task Get_Should_ReturnNull_When_DoesNotExist()
    {
        await using var dbContext = CreateDbContext();
        var outbox = Substitute.For<IOutboxRepository>();
        var repository = new BookingRepository(dbContext, outbox);

        var result = await repository.Get(Guid.NewGuid());

        result.Should().BeNull();
    }
}
