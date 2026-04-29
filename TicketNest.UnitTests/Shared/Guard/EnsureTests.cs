using System.Collections.ObjectModel;
using FluentAssertions;
using TicketNest.Shared.Guard;

namespace TicketNest.UnitTests.Shared.Guard;

[TestFixture]
public class EnsureTests
{
    [Test]
    public void NonNegative_should_not_throw_when_value_is_zero()
    {
        // Act
        var act = () => Ensure.NonNegative(0);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void NonNegative_should_not_throw_when_value_is_positive()
    {
        // Act
        var act = () => Ensure.NonNegative(10.5);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void NonNegative_should_throw_when_value_is_negative()
    {
        // Act
        var act = () => Ensure.NonNegative(-1);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Значение не может быть отрицательным*");
    }

    [Test]
    public void GreaterThanZero_should_not_throw_when_value_is_positive()
    {
        // Act
        var act = () => Ensure.GreaterThanZero(5);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void GreaterThanZero_should_throw_when_value_is_zero()
    {
        // Act
        var act = () => Ensure.GreaterThanZero(0);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Значение должно быть больше нуля*");
    }

    [Test]
    public void GreaterThanZero_should_throw_when_value_is_negative()
    {
        // Act
        var act = () => Ensure.GreaterThanZero(-5);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Значение должно быть больше нуля*");
    }

    [Test]
    public void That_should_not_throw_when_condition_is_true()
    {
        // Act
        var act = () => Ensure.That(true, "Condition failed");

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void That_should_throw_when_condition_is_false()
    {
        // Act
        var act = () => Ensure.That(false, "Condition failed");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void NotNull_for_reference_type_should_return_value_when_not_null()
    {
        // Arrange
        string value = "test";

        // Act
        var result = Ensure.NotNull(value);

        // Assert
        result.Should().Be(value);
    }

    [Test]
    public void NotNull_for_reference_type_should_throw_when_null()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Ensure.NotNull(value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void NotNull_for_nullable_struct_should_return_value_when_not_null()
    {
        // Arrange
        int? value = 42;

        // Act
        var result = Ensure.NotNull(value);

        // Assert
        result.Should().Be(42);
    }

    [Test]
    public void NotNull_for_nullable_struct_should_throw_when_null()
    {
        // Arrange
        int? value = null;

        // Act
        var act = () => Ensure.NotNull(value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void NotDefault_should_return_value_when_not_default()
    {
        // Arrange
        int value = 42;

        // Act
        var result = Ensure.NotDefault(value);

        // Assert
        result.Should().Be(42);
    }

    [Test]
    public void NotDefault_should_throw_when_value_is_default()
    {
        // Arrange
        int value = 0;

        // Act
        var act = () => Ensure.NotDefault(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Значение не может быть значением по умолчанию*");
    }

    [Test]
    public void NotDefault_should_throw_when_guid_is_empty()
    {
        // Arrange
        Guid value = Guid.Empty;

        // Act
        var act = () => Ensure.NotDefault(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*значением по умолчанию*");
    }

    [Test]
    public void NotNullOrEmpty_for_string_should_return_value_when_not_null_or_empty()
    {
        // Arrange
        string value = "test";

        // Act
        var result = Ensure.NotNullOrEmpty(value);

        // Assert
        result.Should().Be("test");
    }

    [Test]
    public void NotNullOrEmpty_for_string_should_throw_when_null()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Ensure.NotNullOrEmpty(value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void NotNullOrEmpty_for_string_should_throw_when_empty()
    {
        // Arrange
        string value = "";

        // Act
        var act = () => Ensure.NotNullOrEmpty(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Строка не может быть пустой*");
    }

    [Test]
    public void NotNullOrWhiteSpace_should_return_value_when_not_null_or_whitespace()
    {
        // Arrange
        string value = "test";

        // Act
        var result = Ensure.NotNullOrWhiteSpace(value);

        // Assert
        result.Should().Be("test");
    }

    [Test]
    public void NotNullOrWhiteSpace_should_throw_when_null()
    {
        // Arrange
        string? value = null;

        // Act
        var act = () => Ensure.NotNullOrWhiteSpace(value);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void NotNullOrWhiteSpace_should_throw_when_empty()
    {
        // Arrange
        string value = "";

        // Act
        var act = () => Ensure.NotNullOrWhiteSpace(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Строка не может быть пустой или состоять только из пробелов*");
    }

    [Test]
    public void NotNullOrWhiteSpace_should_throw_when_only_spaces()
    {
        // Arrange
        string value = "   ";

        // Act
        var act = () => Ensure.NotNullOrWhiteSpace(value);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Строка не может быть пустой или состоять только из пробелов*");
    }

    [Test]
    public void InRange_should_return_value_when_within_range()
    {
        // Arrange
        int value = 5;

        // Act
        var result = Ensure.InRange(value, 1, 10);

        // Assert
        result.Should().Be(5);
    }

    [Test]
    public void InRange_should_not_throw_when_value_equals_min()
    {
        // Act
        var act = () => Ensure.InRange(1, 1, 10);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void InRange_should_not_throw_when_value_equals_max()
    {
        // Act
        var act = () => Ensure.InRange(10, 1, 10);

        // Assert
        act.Should().NotThrow();
    }

    [Test]
    public void InRange_should_throw_when_below_min()
    {
        // Act
        var act = () => Ensure.InRange(0, 1, 10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Значение должно быть между 1 и 10*");
    }

    [Test]
    public void InRange_should_throw_when_above_max()
    {
        // Act
        var act = () => Ensure.InRange(11, 1, 10);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Значение должно быть между 1 и 10*");
    }

    [Test]
    public void InRange_should_work_with_dates()
    {
        // Arrange
        var minDate = new DateTime(2024, 1, 1);
        var maxDate = new DateTime(2024, 12, 31);
        var date = new DateTime(2024, 6, 15);

        // Act
        var result = Ensure.InRange(date, minDate, maxDate);

        // Assert
        result.Should().Be(date);
    }

    [Test]
    public void NotNullOrEmpty_for_collection_should_return_collection_when_not_null_or_empty()
    {
        // Arrange
        var collection = new List<int> { 1, 2, 3 };

        // Act
        var result = Ensure.NotNullOrEmpty(collection);

        // Assert
        result.Should().BeSameAs(collection);
    }

    [Test]
    public void NotNullOrEmpty_for_collection_should_throw_when_null()
    {
        // Arrange
        List<int>? collection = null;

        // Act
        var act = () => Ensure.NotNullOrEmpty(collection);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void NotNullOrEmpty_for_collection_should_throw_when_empty()
    {
        // Arrange
        var collection = new List<int>();

        // Act
        var act = () => Ensure.NotNullOrEmpty(collection);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Коллекция не может быть пустой*");
    }

    [Test]
    public void NotNullOrEmpty_for_collection_should_work_with_array()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };

        // Act
        var result = Ensure.NotNullOrEmpty(array);

        // Assert
        result.Should().BeSameAs(array);
    }

    [Test]
    public void NotNullOrEmpty_for_collection_should_work_with_collection()
    {
        // Arrange
        var collection = new Collection<int> { 1, 2, 3 };

        // Act
        var result = Ensure.NotNullOrEmpty(collection);

        // Assert
        result.Should().BeSameAs(collection);
    }

    [Test]
    public void NotEmpty_for_guid_should_return_guid_when_not_empty()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var result = Ensure.NotEmpty(guid);

        // Assert
        result.Should().Be(guid);
    }

    [Test]
    public void NotEmpty_for_guid_should_throw_when_empty()
    {
        // Arrange
        var guid = Guid.Empty;

        // Act
        var act = () => Ensure.NotEmpty(guid);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*GUID не может быть пустым*");
    }

    [Test]
    public void NonNegative_should_work_with_decimal_values()
    {
        // Act & Assert
        Ensure.NonNegative(0.0);
        Ensure.NonNegative(0.5);

        var act = () => Ensure.NonNegative(-0.1);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}