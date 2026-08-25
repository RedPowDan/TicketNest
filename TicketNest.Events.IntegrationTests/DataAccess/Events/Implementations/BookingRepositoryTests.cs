using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TicketNest.DataAccess.Bookings.DbContext;
using TicketNest.DataAccess.Bookings.Implementations;
using TicketNest.DataAccess.Bookings.Outbox;
using TicketNest.Domain.Bookings.Models.Bookings;
using TicketNest.Domain.Bookings.Repositories;
using TicketNest.Events.IntegrationTests.Infrastructure;

namespace TicketNest.Events.IntegrationTests.DataAccess.Events.Implementations;

[Collection("Database")]
public class BookingRepositoryTests
{
    private readonly PostgreSqlContainerFixture _fixture;

    public BookingRepositoryTests(PostgreSqlContainerFixture fixture)
    {
        _fixture = fixture;
    }

    private BookingsDbContext CreateDbContext() =>
        new BookingsDbContext(
            new DbContextOptionsBuilder<BookingsDbContext>()
                .UseNpgsql(_fixture.ConnectionString)
                .Options);

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

    [Fact]
    public async Task Save_with_null_booking_should_throw_ArgumentNullException()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);

        var act = async () => await repository.Save(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task Save_multiple_bookings_for_same_event_should_succeed()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var booking1 = CreateBooking(eventId, userId);
        var booking2 = CreateBooking(eventId, userId);

        await repository.Save(booking1);
        await repository.Save(booking2);

        var saved1 = await dbContext.Bookings.FindAsync(booking1.Id);
        var saved2 = await dbContext.Bookings.FindAsync(booking2.Id);

        saved1.Should().NotBeNull();
        saved2.Should().NotBeNull();
        saved1!.EventId.Should().Be(eventId);
        saved2!.EventId.Should().Be(eventId);
    }

    [Fact]
    public async Task Save_duplicate_id_should_update_existing_instead_of_creating_new()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bookingId = Guid.NewGuid();

        var booking = CreateBooking(eventId, userId, id: bookingId);
        await repository.Save(booking);

        var updatedBooking = CreateBooking(eventId, userId, id: bookingId, status: BookingStatus.Confirmed, processedAt: DateTime.UtcNow);
        await repository.Save(updatedBooking);

        var saved = await dbContext.Bookings.FindAsync(bookingId);
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(BookingStatus.Confirmed);
        saved.ProcessedAt.Should().NotBeNull();

        var count = await dbContext.Bookings.CountAsync(b => b.Id == bookingId);
        count.Should().Be(1);
    }

    [Fact]
    public async Task Get_with_cancelled_token_should_throw_OperationCanceledException()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);

        var ct = new CancellationToken(canceled: true);
        var act = async () => await repository.Get(Guid.NewGuid(), ct);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Get_should_return_updated_booking_after_Save_update()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bookingId = Guid.NewGuid();
        var booking = CreateBooking(eventId, userId, id: bookingId);
        await repository.Save(booking);

        var updatedBooking = CreateBooking(eventId, userId, id: bookingId, status: BookingStatus.Confirmed, processedAt: DateTime.UtcNow);
        await repository.Save(updatedBooking);

        var result = await repository.Get(bookingId);

        result.Should().NotBeNull();
        result!.Status.Should().Be(BookingStatus.Confirmed);
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_booking_with_Rejected_status_then_Get_should_return_Rejected()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var booking = CreateBooking(eventId, userId);
        await repository.Save(booking);

        booking.Reject(DateTime.UtcNow);
        await repository.Save(booking);

        var result = await repository.Get(booking.Id);

        result.Should().NotBeNull();
        result!.Status.Should().Be(BookingStatus.Rejected);
        result.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Save_and_Get_should_preserve_all_booking_properties()
    {
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);
        var eventId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var bookingId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;
        var booking = CreateBooking(eventId, userId, id: bookingId);

        await repository.Save(booking);

        var result = await repository.Get(bookingId);

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
        await using var dbContext = CreateDbContext();
        var outbox = new OutboxRepository(dbContext);
        var repository = new BookingRepository(dbContext, outbox);

        var result = await repository.Get(Guid.Empty);

        result.Should().BeNull();
    }
}
