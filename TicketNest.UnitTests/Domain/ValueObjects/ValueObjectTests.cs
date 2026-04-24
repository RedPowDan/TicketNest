using FluentAssertions;
using TicketNest.Domain.ValueObjects;

namespace TicketNest.UnitTests.Domain.ValueObjects;

[TestFixture]
public class ValueObjectTests
{
    // Concrete implementation for testing
    private record TestValueObject : ValueObject
    {
        private string StringValue { get; }
        private int IntValue { get; }
        public Guid? NullableValue { get; }

        public TestValueObject(string stringValue, int intValue, Guid? nullableValue = null)
        {
            StringValue = stringValue;
            IntValue = intValue;
            NullableValue = nullableValue;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return StringValue;
            yield return IntValue;
            yield return NullableValue;
        }
    }

    private record TestValueObjectWithSingleProperty : ValueObject
    {
        public string Value { get; }

        public TestValueObjectWithSingleProperty(string value)
        {
            Value = value;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Value;
        }
    }

    [Test]
    public void Equals_should_return_true_for_same_reference()
    {
        // Arrange
        var valueObject = new TestValueObject("test", 123);

        // Act
        var result = valueObject.Equals(valueObject);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_should_return_true_for_objects_with_same_values()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123, Guid.NewGuid());
        var obj2 = new TestValueObject("test", 123, obj1.NullableValue);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_should_return_false_when_other_is_null()
    {
        // Arrange
        var valueObject = new TestValueObject("test", 123);

        // Act
        var result = valueObject.Equals(null);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Equals_should_return_false_for_different_types()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObjectWithSingleProperty("test");

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Equals_should_return_false_for_different_string_values()
    {
        // Arrange
        var obj1 = new TestValueObject("test1", 123);
        var obj2 = new TestValueObject("test2", 123);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Equals_should_return_false_for_different_int_values()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObject("test", 456);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Equals_should_return_false_for_different_nullable_values()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123, Guid.NewGuid());
        var obj2 = new TestValueObject("test", 123, Guid.NewGuid());

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void Equals_should_handle_both_nullable_values_as_null()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123, null);
        var obj2 = new TestValueObject("test", 123, null);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equals_should_return_false_when_one_nullable_is_null_and_other_is_not()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123, null);
        var obj2 = new TestValueObject("test", 123, Guid.NewGuid());

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void GetHashCode_should_be_equal_for_equal_objects()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123, Guid.Parse("12345678-1234-1234-1234-123456789012"));
        var obj2 = new TestValueObject("test", 123, Guid.Parse("12345678-1234-1234-1234-123456789012"));

        // Act
        var hashCode1 = obj1.GetHashCode();
        var hashCode2 = obj2.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Test]
    public void GetHashCode_should_be_different_for_different_objects()
    {
        // Arrange
        var obj1 = new TestValueObject("test1", 123);
        var obj2 = new TestValueObject("test2", 456);

        // Act
        var hashCode1 = obj1.GetHashCode();
        var hashCode2 = obj2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Test]
    public void GetHashCode_should_be_different_for_different_string_values()
    {
        // Arrange
        var obj1 = new TestValueObject("test1", 123);
        var obj2 = new TestValueObject("test2", 123);

        // Act
        var hashCode1 = obj1.GetHashCode();
        var hashCode2 = obj2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Test]
    public void GetHashCode_should_be_different_for_different_int_values()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObject("test", 456);

        // Act
        var hashCode1 = obj1.GetHashCode();
        var hashCode2 = obj2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }

    [Test]
    public void GetHashCode_should_be_consistent_for_same_object()
    {
        // Arrange
        var valueObject = new TestValueObject("test", 123);

        // Act
        var hashCode1 = valueObject.GetHashCode();
        var hashCode2 = valueObject.GetHashCode();

        // Assert
        hashCode1.Should().Be(hashCode2);
    }

    [Test]
    public void Equals_should_be_symmetric()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObject("test", 123);

        // Act & Assert
        obj1.Equals(obj2).Should().Be(obj2.Equals(obj1));
    }

    [Test]
    public void Equals_should_be_transitive()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObject("test", 123);
        var obj3 = new TestValueObject("test", 123);

        // Assert
        obj1.Equals(obj2).Should().BeTrue();
        obj2.Equals(obj3).Should().BeTrue();
        obj1.Equals(obj3).Should().BeTrue();
    }

    [Test]
    public void ValueObject_with_complex_types_should_work_correctly()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var dateTime = DateTime.UtcNow;

        var obj1 = new TestValueObject("test", 123, guid);
        var obj2 = new TestValueObject("test", 123, guid);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ValueObject_should_handle_empty_strings()
    {
        // Arrange
        var obj1 = new TestValueObject("", 123);
        var obj2 = new TestValueObject("", 123);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ValueObject_should_handle_very_long_strings()
    {
        // Arrange
        var longString = new string('A', 10000);
        var obj1 = new TestValueObject(longString, 123);
        var obj2 = new TestValueObject(longString, 123);

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void ValueObject_with_no_properties_should_work()
    {
        // Arrange
        var obj1 = new TestValueObjectWithSingleProperty("test");
        var obj2 = new TestValueObjectWithSingleProperty("test");

        // Act
        var result = obj1.Equals(obj2);

        // Assert
        result.Should().BeTrue();
    }

    [Test]
    public void Equality_operator_should_work_with_record_syntax()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObject("test", 123);

        // Act
        var areEqual = obj1 == obj2;

        // Assert (since it's a record, == operator uses Equals)
        areEqual.Should().BeTrue();
    }

    [Test]
    public void ValueObject_should_preserve_all_components_in_hash_code()
    {
        // Arrange
        var obj1 = new TestValueObject("test", 123);
        var obj2 = new TestValueObject("test", 124); // Different int value

        // Act
        var hashCode1 = obj1.GetHashCode();
        var hashCode2 = obj2.GetHashCode();

        // Assert
        hashCode1.Should().NotBe(hashCode2);
    }
}