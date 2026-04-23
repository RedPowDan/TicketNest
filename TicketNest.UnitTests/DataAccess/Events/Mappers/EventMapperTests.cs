using FluentAssertions;
using TicketNest.DataAccess.Events.Mappers;
using TicketNest.DataAccess.Events.Models;
using TicketNest.Domain.Models.Events;
using TicketNest.Domain.ValueObjects;

namespace TicketNest.UnitTests.DataAccess.Events.Mappers;

[TestFixture]
public class EventMapperTests
{
    [Test]
    public void To_domain_should_map_all_properties_correctly()
    {
        // Arrange
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Description = "Test Event Description",
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        // Act
        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        // Assert
        domainEvent.Id.Value.Should().Be(persistenceEvent.Id);
        domainEvent.Title.Value.Should().Be(persistenceEvent.Title);
        domainEvent.Description!.Value.Should().Be(persistenceEvent.Description);
        domainEvent.StartAt.Should().Be(persistenceEvent.StartAt);
        domainEvent.EndAt.Should().Be(persistenceEvent.EndAt);
    }

    [Test]
    public void To_domain_should_map_description_as_null_when_source_description_is_null()
    {
        // Arrange
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Description = null,
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        // Act
        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        // Assert
        Assert.That(domainEvent.Description, Is.Null);
    }

    [Test]
    public void To_domain_should_throw_when_source_is_null()
    {
        // Act
        var act = () => EventMapper.ToDomain(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void To_domain_should_handle_empty_description()
    {
        // Arrange
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = "Test Event Title",
            Description = "Description",
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        // Act
        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        // Assert
        Assert.That(domainEvent.Description, Is.Not.Null);
    }

    [Test]
    public void To_domain_should_handle_very_long_title()
    {
        // Arrange
        var longTitle = new string('A', 500);
        var persistenceEvent = new PersistenceEvent
        {
            Id = Guid.NewGuid(),
            Title = longTitle,
            Description = "Description",
            StartAt = new DateTime(2024, 12, 25, 18, 0, 0),
            EndAt = new DateTime(2024, 12, 25, 22, 0, 0)
        };

        // Act
        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        // Assert
        domainEvent.Title.Value.Should().Be(longTitle);
    }

    [Test]
    public void To_domain_should_preserve_date_time_kind()
    {
        // Arrange
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

        // Act
        var domainEvent = EventMapper.ToDomain(persistenceEvent);

        // Assert
        domainEvent.StartAt.Kind.Should().Be(startAt.Kind);
        domainEvent.EndAt.Kind.Should().Be(endAt.Kind);
    }

    [Test]
    public void To_persistence_should_map_all_properties_correctly()
    {
        // Arrange
        var eventId = EventId.From(Guid.NewGuid());
        var eventTitle = EventTitle.From("Test Event Title");
        var eventDescription = EventDescription.From("Test Event Description");

        var domainEvent = Event.LoadFromStorage(
            id: eventId,
            title: eventTitle,
            description: eventDescription,
            startAt: new DateTime(2024, 12, 25, 18, 0, 0),
            endAt: new DateTime(2024, 12, 25, 22, 0, 0));

        // Act
        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        // Assert
        persistenceEvent.Id.Should().Be(eventId.Value);
        persistenceEvent.Title.Should().Be(eventTitle.Value);
        persistenceEvent.Description.Should().Be(eventDescription.Value);
        persistenceEvent.StartAt.Should().Be(domainEvent.StartAt);
        persistenceEvent.EndAt.Should().Be(domainEvent.EndAt);
    }

    [Test]
    public void To_persistence_should_map_description_as_null_when_domain_description_is_null()
    {
        // Arrange
        var eventId = EventId.From(Guid.NewGuid());
        var eventTitle = EventTitle.From("Test Event Title");

        var domainEvent = Event.LoadFromStorage(
            id: eventId,
            title: eventTitle,
            description: null,
            startAt: new DateTime(2024, 12, 25, 18, 0, 0),
            endAt: new DateTime(2024, 12, 25, 22, 0, 0));

        // Act
        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        // Assert
        persistenceEvent.Description.Should().BeNull();
    }

    [Test]
    public void To_persistence_should_throw_when_source_is_null()
    {
        // Act
        var act = () => EventMapper.ToPersistence(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void To_persistence_should_preserve_all_value_object_properties()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var expectedTitle = "Annual Conference 2024";
        var expectedDescription = "The biggest tech conference";

        var domainEvent = Event.LoadFromStorage(
            id: EventId.From(expectedId),
            title: EventTitle.From(expectedTitle),
            description: EventDescription.From(expectedDescription),
            startAt: new DateTime(2024, 10, 15, 9, 0, 0),
            endAt: new DateTime(2024, 10, 17, 18, 0, 0));

        // Act
        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        // Assert
        persistenceEvent.Id.Should().Be(expectedId);
        persistenceEvent.Title.Should().Be(expectedTitle);
        persistenceEvent.Description.Should().Be(expectedDescription);
    }

    [Test]
    public void To_persistence_should_handle_minimum_dates()
    {
        // Arrange
        var nowDateTime = DateTime.Now;
        var afterDay = nowDateTime.AddDays(1);
        var domainEvent = Event.LoadFromStorage(
            id: EventId.From(Guid.NewGuid()),
            title: EventTitle.From("Test Event"),
            description: EventDescription.From("Description"),
            startAt: nowDateTime,
            endAt: afterDay);

        // Act
        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        // Assert
        persistenceEvent.StartAt.Should().Be(nowDateTime);
        persistenceEvent.EndAt.Should().Be(afterDay);
    }

    [Test]
    public void To_persistence_should_handle_special_characters_in_title_and_description()
    {
        // Arrange
        var titleWithSpecialChars = "Test Event !@#$%^&*()_+{}[]|\\:;\"'<>,.?/~`";
        var descriptionWithSpecialChars = "Description with special chars: 😀🎉🚀";

        var domainEvent = Event.LoadFromStorage(
            id: EventId.From(Guid.NewGuid()),
            title: EventTitle.From(titleWithSpecialChars),
            description: EventDescription.From(descriptionWithSpecialChars),
            startAt: new DateTime(2024, 12, 25, 18, 0, 0),
            endAt: new DateTime(2024, 12, 25, 22, 0, 0));

        // Act
        var persistenceEvent = EventMapper.ToPersistence(domainEvent);

        // Assert
        persistenceEvent.Title.Should().Be(titleWithSpecialChars);
        persistenceEvent.Description.Should().Be(descriptionWithSpecialChars);
    }
}