using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TicketNest.Application.Events.Cache;
using TicketNest.Application.Events.Services.Events;
using TicketNest.Domain.Events.Models.Events;
using TicketNest.Domain.Events.Repositories;

namespace TicketNest.Events.Tests.Application.Services.Events;

[TestFixture]
public class EventTopServiceTests
{
    private IEventsRepository _eventsRepository = null!;
    private ICacheService _cacheService = null!;
    private EventTopService _eventTopService = null!;

    [SetUp]
    public void SetUp()
    {
        _eventsRepository = Substitute.For<IEventsRepository>();
        _cacheService = Substitute.For<ICacheService>();
        var logger = NullLogger<EventTopService>.Instance;
        _eventTopService = new EventTopService(_eventsRepository, _cacheService, logger);
    }

    [Test]
    public async Task GetTop10_Should_ReturnCachedEvents_When_CacheHit()
    {
        var cachedEvents = new List<Event>
        {
            CreateValidEvent("Event 1"),
            CreateValidEvent("Event 2")
        };

        _cacheService.GetAsync<IReadOnlyList<Event>>(Arg.Is(CacheKeys.TopEvents), Arg.Any<CancellationToken>())
            .Returns(cachedEvents);

        var result = await _eventTopService.GetTop10();

        result.Should().HaveCount(2);
        await _eventsRepository.DidNotReceive().GetTop10ByPopularity(Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetTop10_Should_QueryRepositoryAndCache_When_CacheMiss()
    {
        var dbEvents = new List<Event>
        {
            CreateValidEvent("Event 1"),
            CreateValidEvent("Event 2")
        };

        _cacheService.GetAsync<IReadOnlyList<Event>>(Arg.Is(CacheKeys.TopEvents), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>?)null);

        _eventsRepository.GetTop10ByPopularity(Arg.Any<CancellationToken>())
            .Returns(dbEvents);

        var result = await _eventTopService.GetTop10();

        result.Should().HaveCount(2);
        await _eventsRepository.Received(1).GetTop10ByPopularity(Arg.Any<CancellationToken>());
        await _cacheService.Received(1).SetAsync(
            Arg.Is(CacheKeys.TopEvents),
            Arg.Any<IReadOnlyList<Event>>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task GetTop10_Should_ReturnEmptyList_When_NoEventsExist()
    {
        _cacheService.GetAsync<IReadOnlyList<Event>>(Arg.Is(CacheKeys.TopEvents), Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<Event>?)null);

        _eventsRepository.GetTop10ByPopularity(Arg.Any<CancellationToken>())
            .Returns(new List<Event>());

        var result = await _eventTopService.GetTop10();

        result.Should().BeEmpty();
        await _cacheService.Received(1).SetAsync(
            Arg.Is(CacheKeys.TopEvents),
            Arg.Any<IReadOnlyList<Event>>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }

    private Event CreateValidEvent(string title = "Test Event")
    {
        return Event.LoadFromStorage(Guid.NewGuid(), title, "Description", DateTime.UtcNow.AddDays(1), DateTime.UtcNow.AddDays(2), totalSeats: 100, availableSeats: 50);
    }
}
