using FluentAssertions;
using TicketNest.Domain.Events.Models.Events;

namespace TicketNest.UnitTests.Domain.Models.Events;

[TestFixture]
public class EventTests
{
    [Test]
    public void LoadFromStorage_should_create_event_with_all_properties()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventTitle = "Test Event";
        var eventDescription = "Test Description";
        var startAt = new DateTime(2024, 12, 25, 18, 0, 0);
        var endAt = new DateTime(2024, 12, 25, 22, 0, 0);

        // Act
        var eventEntity = Event.LoadFromStorage(eventId, eventTitle, eventDescription, startAt, endAt, totalSeats: 100, availableSeats: 100);

        // Assert
        eventEntity.Id.Should().Be(eventId);
        eventEntity.Title.Should().Be(eventTitle);
        eventEntity.Description.Should().Be(eventDescription);
        eventEntity.StartAt.Should().Be(startAt);
        eventEntity.EndAt.Should().Be(endAt);
        eventEntity.TotalSeats.Should().Be(100);
        eventEntity.AvailableSeats.Should().Be(100);
    }

    [Test]
    public void LoadFromStorage_should_create_event_with_null_description()
    {
        // Arrange
        var eventId = Guid.NewGuid();
        var eventTitle = "Test Event";
        var startAt = new DateTime(2024, 12, 25, 18, 0, 0);
        var endAt = new DateTime(2024, 12, 25, 22, 0, 0);

        // Act
        var eventEntity = Event.LoadFromStorage(eventId, eventTitle, null, startAt, endAt, totalSeats: 100, availableSeats: 100);

        // Assert
        eventEntity.Description.Should().BeNull();
    }

    [Test]
    public void Create_should_return_successful_result_when_all_parameters_are_valid()
    {
        // Arrange
        var title = "Valid Event";
        var description = "Valid Description";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result = Event.Create(title, description, startAt, endAt, totalSeats: 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be(title);
        result.Value.Description.Should().Be(description);
        result.Value.StartAt.Should().Be(startAt);
        result.Value.EndAt.Should().Be(endAt);
        result.Value.TotalSeats.Should().Be(100);
        result.Value.AvailableSeats.Should().Be(100);
    }

    [Test]
    public void Create_should_return_successful_result_with_null_description()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result = Event.Create(title, null, startAt, endAt, totalSeats: 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Should().BeNull();
    }

    [Test]
    public void Create_should_return_failure_when_title_is_null()
    {
        // Arrange
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result = Event.Create(null!, null, startAt, endAt, totalSeats: 100);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Название события не должно быть пустое");
    }

    [Test]
    public void Create_should_return_failure_when_startAt_is_default()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = default(DateTime);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result = Event.Create(title, null, startAt, endAt, totalSeats: 100);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Начало события не может быть значением по умолчанию");
    }

    [Test]
    public void Create_should_return_failure_when_startAt_is_greater_than_endAt()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = DateTime.Now.AddDays(2);
        var endAt = DateTime.Now.AddDays(1);

        // Act
        var result = Event.Create(title, null, startAt, endAt, totalSeats: 100);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Начало события не может быть больше чем его окончание");
    }

    [Test]
    public void Create_should_generate_new_id()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result1 = Event.Create(title, null, startAt, endAt, totalSeats: 100);
        var result2 = Event.Create(title, null, startAt, endAt, totalSeats: 100);

        // Assert
        result1.Value.Id.Should().NotBe(result2.Value.Id);
    }

    [Test]
    public void ChangeTitle_should_update_title_successfully()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var newTitle = "New Title";

        // Act
        var result = eventEntity.ChangeTitle(newTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
        eventEntity.Title.Should().Be(newTitle);
    }

    [Test]
    public void ChangeTitle_should_throw_when_title_is_null()
    {
        // Arrange
        var eventEntity = CreateValidEvent();

        // Act
        var changeResult = eventEntity.ChangeTitle(null!);

        // Assert
        changeResult.IsFailure.Should().BeTrue();
    }

    [Test]
    public void ChangeDescription_should_update_description_successfully()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var newDescription = "New Description";

        // Act
        var result = eventEntity.ChangeDescription(newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        eventEntity.Description.Should().Be(newDescription);
    }

    [Test]
    public void ChangeDescription_should_set_description_to_null()
    {
        // Arrange
        var eventEntity = CreateValidEventWithDescription();

        // Act
        var result = eventEntity.ChangeDescription(null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        eventEntity.Description.Should().BeNull();
    }

    [Test]
    public void ChangeStartAtAndEndAt_should_update_dates_successfully()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var newStartAt = DateTime.Now.AddDays(3);
        var newEndAt = DateTime.Now.AddDays(4);

        // Act
        var result = eventEntity.ChangeStartAtAndEndAt(newStartAt, newEndAt);

        // Assert
        result.IsSuccess.Should().BeTrue();
        eventEntity.StartAt.Should().Be(newStartAt);
        eventEntity.EndAt.Should().Be(newEndAt);
    }

    [Test]
    public void ChangeStartAtAndEndAt_should_return_failure_when_startAt_is_default()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var newStartAt = default(DateTime);
        var newEndAt = DateTime.Now.AddDays(4);

        // Act
        var result = eventEntity.ChangeStartAtAndEndAt(newStartAt, newEndAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Начало события не может быть значением по умолчанию");

        eventEntity.StartAt.Should().NotBe(newStartAt);
    }

    [Test]
    public void ChangeStartAtAndEndAt_should_return_failure_when_startAt_is_greater_than_endAt()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var originalStartAt = eventEntity.StartAt;
        var originalEndAt = eventEntity.EndAt;
        var newStartAt = DateTime.Now.AddDays(5);
        var newEndAt = DateTime.Now.AddDays(4);

        // Act
        var result = eventEntity.ChangeStartAtAndEndAt(newStartAt, newEndAt);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("Начало события не может быть больше чем его окончание");

        eventEntity.StartAt.Should().Be(originalStartAt);
        eventEntity.EndAt.Should().Be(originalEndAt);
    }

    [Test]
    public void ChangeStartAtAndEndAt_should_allow_equal_dates()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var sameDateTime = DateTime.Now.AddDays(3);

        // Act
        var result = eventEntity.ChangeStartAtAndEndAt(sameDateTime, sameDateTime);

        // Assert
        result.IsSuccess.Should().BeTrue();
        eventEntity.StartAt.Should().Be(sameDateTime);
        eventEntity.EndAt.Should().Be(sameDateTime);
    }

    [Test]
    public void Multiple_changes_should_persist_correctly()
    {
        // Arrange
        var eventEntity = CreateValidEvent();
        var newTitle = "Updated Title";
        var newDescription = "Updated Description";
        var newStartAt = DateTime.Now.AddDays(3);
        var newEndAt = DateTime.Now.AddDays(5);

        // Act
        eventEntity.ChangeTitle(newTitle);
        eventEntity.ChangeDescription(newDescription);
        eventEntity.ChangeStartAtAndEndAt(newStartAt, newEndAt);

        // Assert
        eventEntity.Title.Should().Be(newTitle);
        eventEntity.Description.Should().Be(newDescription);
        eventEntity.StartAt.Should().Be(newStartAt);
        eventEntity.EndAt.Should().Be(newEndAt);
    }

    [Test]
    public void Create_should_handle_startAt_and_endAt_being_equal()
    {
        // Arrange
        var title = "Valid Event";
        var sameDateTime = DateTime.Now.AddDays(1);

        // Act
        var result = Event.Create(title, null, sameDateTime, sameDateTime, totalSeats: 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StartAt.Should().Be(sameDateTime);
        result.Value.EndAt.Should().Be(sameDateTime);
    }

    [Test]
    public void Create_should_handle_minimum_date_values()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = DateTime.Now;
        var endAt = startAt.AddHours(1);

        // Act
        var result = Event.Create(title, null, startAt, endAt, totalSeats: 100);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.StartAt.Should().Be(startAt);
        result.Value.EndAt.Should().Be(endAt);
    }

    [Test]
    public void TryReserveSeats_should_reduce_available_seats()
    {
        // Arrange
        var eventEntity = CreateValidEvent();

        // Act
        var result = eventEntity.TryReserveSeats(DateTime.UtcNow, 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        eventEntity.AvailableSeats.Should().Be(7);
    }

    [Test]
    public void TryReserveSeats_should_return_false_when_not_enough_seats()
    {
        // Arrange
        var eventEntity = CreateValidEvent();

        // Act
        var result = eventEntity.TryReserveSeats(DateTime.UtcNow, 20);

        // Assert
        result.IsFailure.Should().BeTrue();
        eventEntity.AvailableSeats.Should().Be(10);
    }

    [Test]
    public void ReleaseSeats_should_increase_available_seats()
    {
        // Arrange
        var eventEntity = Event.LoadFromStorage(
            id: Guid.NewGuid(), title: "Test", description: null,
            startAt: DateTime.Now.AddDays(1), endAt: DateTime.Now.AddDays(2),
            totalSeats: 100, availableSeats: 7);

        // Act
        var success = eventEntity.ReleaseSeats(3);

        // Assert
        success.Should().BeTrue();
        eventEntity.AvailableSeats.Should().Be(10);
    }

    [Test]
    public void ReleaseSeats_should_not_exceed_total_seats()
    {
        // Arrange
        var eventEntity = CreateValidEvent();

        // Act
        var success = eventEntity.ReleaseSeats(1);

        // Assert
        success.Should().BeFalse();
        eventEntity.AvailableSeats.Should().Be(10);
    }

    [Test]
    public void Create_should_return_failure_when_totalSeats_is_zero()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result = Event.Create(title, null, startAt, endAt, totalSeats: 0);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public void Create_should_return_failure_when_totalSeats_is_negative()
    {
        // Arrange
        var title = "Valid Event";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        // Act
        var result = Event.Create(title, null, startAt, endAt, totalSeats: -1);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    // Helper methods
    private static Event CreateValidEvent()
    {
        var title = "Test Event";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        var result = Event.Create(title, null, startAt, endAt, totalSeats: 10);
        return result.Value;
    }

    private static Event CreateValidEventWithDescription()
    {
        var title = "Test Event";
        var description = "Test Description";
        var startAt = DateTime.Now.AddDays(1);
        var endAt = DateTime.Now.AddDays(2);

        var result = Event.Create(title, description, startAt, endAt, totalSeats: 10);
        return result.Value;
    }
}
