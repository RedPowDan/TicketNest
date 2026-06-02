using FluentAssertions;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Events;

namespace TicketNest.UnitTests.DataAccess.Events.Mappers;

[TestFixture]
public class EventMapperTests
{
    [Test]
    public void To_domain_should_map_all_properties_correctly()
    {
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Description = "Test Event Description",
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        domainEvent.Id.Should().Be(persistenceEvent.Id);
        domainEvent.Title.Should().Be(persistenceEvent.Title);
        domainEvent.Description!.Should().Be(persistenceEvent.Description);
        domainEvent.StartAt.Should().Be(persistenceEvent.StartAt);
        domainEvent.EndAt.Should().Be(persistenceEvent.EndAt);
    }

    [Test]
    public void To_domain_should_map_description_as_null_when_source_description_is_null()
    {
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Description = null,
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        Assert.That(domainEvent.Description, Is.Null);
    }

    [Test]
    public void To_domain_should_throw_when_source_is_null()
    {
        var act = () => EventMapper.ToDomain(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void To_domain_should_handle_empty_description()
    {
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Description = "Description",
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        Assert.That(domainEvent.Description, Is.Not.Null);
    }

    [Test]
    public void To_domain_should_handle_very_long_title()
    {
        var longTitle = new string('A', 500);
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = longTitle,
            Description = "Description",
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        domainEvent.Title.Should().Be(longTitle);
    }

    [Test]
    public void To_domain_should_preserve_date_time_kind()
    {
        var startAt = new DateTime(2024, 12, 25, 18, 0, 0, DateTimeKind.Utc);
        var endAt = new DateTime(2024, 12, 25, 22, 0, 0, DateTimeKind.Utc);

        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event",
            Description = "Description",
            StartAt = startAt,
            EndAt = endAt
        };

        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        domainEvent.StartAt.Kind.Should().Be(startAt.Kind);
        domainEvent.EndAt.Kind.Should().Be(endAt.Kind);
    }

    [Test]
    public void To_persistence_should_map_all_properties_correctly()
    {
        var eventId = Guid.NewGuid();
        var eventTitle = "Test Event Title";
        var eventDescription = "Test EventDescription";

        var domainEvent = Event.LoadFromStorage(
            id: eventId,
            title: eventTitle,
            description: eventDescription,
            startAt: new DateTime(2024, 12, 25, 18, 0, 0),
            endAt: new DateTime(2024, 12, 25, 22, 0, 0),
            totalSeats: 100,
            availableSeats: 100);

        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        persistenceEvent.Id.Should().Be(eventId);
        persistenceEvent.Title.Should().Be(eventTitle);
        persistenceEvent.Description.Should().Be(eventDescription);
        persistenceEvent.StartAt.Should().Be(domainEvent.StartAt);
        persistenceEvent.EndAt.Should().Be(domainEvent.EndAt);
    }

    [Test]
    public void To_persistence_should_map_description_as_null_when_domain_description_is_null()
    {
        var eventId = Guid.NewGuid();
        var eventTitle = "Test Event Title";

        var domainEvent = Event.LoadFromStorage(
            id: eventId,
            title: eventTitle,
            description: null,
            startAt: new DateTime(2024, 12, 25, 18, 0, 0),
            endAt: new DateTime(2024, 12, 25, 22, 0, 0),
            totalSeats: 100,
            availableSeats: 100);

        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        persistenceEvent.Description.Should().BeNull();
    }

    [Test]
    public void To_persistence_should_throw_when_source_is_null()
    {
        var act = () => EventMapper.ToPersistence(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void To_persistence_should_preserve_all_value_object_properties()
    {
        var expectedId = Guid.NewGuid();
        var expectedTitle = "Annual Conference 2024";
        var expectedDescription = "The biggest tech conference";

        var domainEvent = Event.LoadFromStorage(
            id: expectedId,
            title: expectedTitle,
            description: expectedDescription,
            startAt: new DateTime(2024, 10, 15, 9, 0, 0),
            endAt: new DateTime(2024, 10, 17, 18, 0, 0),
            totalSeats: 100,
            availableSeats: 100);

        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        persistenceEvent.Id.Should().Be(expectedId);
        persistenceEvent.Title.Should().Be(expectedTitle);
        persistenceEvent.Description.Should().Be(expectedDescription);
    }

    [Test]
    public void To_persistence_should_handle_minimum_dates()
    {
        var nowDateTime = DateTime.Now;
        var afterDay = nowDateTime.AddDays(1);
        var domainEvent = Event.LoadFromStorage(
            id: Guid.NewGuid(),
            title: "Test Event",
            description: "Description",
            startAt: nowDateTime,
            endAt: afterDay,
            totalSeats: 100,
            availableSeats: 100);

        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        persistenceEvent.StartAt.Should().Be(nowDateTime);
        persistenceEvent.EndAt.Should().Be(afterDay);
    }

    [Test]
    public void To_persistence_should_handle_special_characters_in_title_and_description()
    {
        var titleWithSpecialChars = "Test Event !@#$%^&*()_+{}[]|\\:;\"'<>,.?/~`";
        var descriptionWithSpecialChars = "Description with special chars: 😀🎉🚀";

        var domainEvent = Event.LoadFromStorage(
            id: Guid.NewGuid(),
            title: titleWithSpecialChars,
            description: descriptionWithSpecialChars,
            startAt: new DateTime(2024, 12, 25, 18, 0, 0),
            endAt: new DateTime(2024, 12, 25, 22, 0, 0),
            totalSeats: 100,
            availableSeats: 100);

        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        persistenceEvent.Title.Should().Be(titleWithSpecialChars);
        persistenceEvent.Description.Should().Be(descriptionWithSpecialChars);
    }
}
