using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace TicketNest.Shared.Guard;

/// <summary>
/// Предоставляет методы для валидации аргументов (Guard Clauses).
/// </summary>
public static class Ensure
{
    /// <summary>
    /// Проверяет, что значение не является отрицательным.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если значение отрицательное.</exception>
    public static void NonNegative(double value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Значение не может быть отрицательным.");
        }
    }

    /// <summary>
    /// Проверяет, что значение больше нуля.
    /// </summary>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если значение меньше или равно нулю.</exception>
    public static void GreaterThanZero(double value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, "Значение должно быть больше нуля.");
        }
    }

    /// <summary>
    /// Проверяет, что условие истинно.
    /// </summary>
    /// <param name="condition">Проверяемое условие.</param>
    /// <param name="message">Сообщение об ошибке.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <exception cref="ArgumentException">Выбрасывается, если условие ложно.</exception>
    public static void That(bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? paramName = null)
    {
        if (!condition)
        {
            throw new ArgumentException(message, paramName);
        }
    }

    /// <summary>
    /// Проверяет, что значение не равно null (для ссылочных типов).
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходное значение, если оно не null.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если значение равно null.</exception>
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : class
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
        return value;
    }

    /// <summary>
    /// Проверяет, что nullable значение не равно null (для значимых типов).
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходное значение, если оно не null.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если значение равно null.</exception>
    public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
        return value.Value;
    }

    /// <summary>
    /// Проверяет, что значение не равно значению по умолчанию.
    /// </summary>
    /// <typeparam name="T">Тип значения.</typeparam>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходное значение, если оно не дефолтное.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если значение равно default.</exception>
    public static T NotDefault<T>([NotNull] T value, [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default))
        {
            throw new ArgumentException($"Значение не может быть значением по умолчанию ({default(T)}).", paramName);
        }
        return value;
    }

    /// <summary>
    /// Проверяет, что строка не равна null и не пуста.
    /// </summary>
    /// <param name="value">Проверяемая строка.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходная строка, если валидна.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если строка равна null.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если строка пуста.</exception>
    public static string NotNullOrEmpty([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
        
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Строка не может быть пустой.", paramName);
        }
        
        return value;
    }

    /// <summary>
    /// Проверяет, что строка не равна null, не пуста и не состоит из пробелов.
    /// </summary>
    /// <param name="value">Проверяемая строка.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходная строка, если валидна.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если строка равна null.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если строка пуста или состоит из пробелов.</exception>
    public static string NotNullOrWhiteSpace([NotNull] string? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
    {
        if (value is null)
        {
            throw new ArgumentNullException(paramName);
        }
        
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Строка не может быть пустой или состоять только из пробелов.", paramName);
        }
        
        return value;
    }

    /// <summary>
    /// Проверяет, что значение находится в указанном диапазоне (включительно).
    /// </summary>
    /// <typeparam name="T">Тип, реализующий IComparable.</typeparam>
    /// <param name="value">Проверяемое значение.</param>
    /// <param name="min">Минимальное допустимое значение.</param>
    /// <param name="max">Максимальное допустимое значение.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходное значение, если оно в диапазоне.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Выбрасывается, если значение вне диапазона.</exception>
    public static T InRange<T>(T value, T min, T max, 
        [CallerArgumentExpression(nameof(value))] string? paramName = null) where T : IComparable<T>
    {
        if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
        {
            throw new ArgumentOutOfRangeException(paramName, value, $"Значение должно быть между {min} и {max} (включительно).");
        }
        return value;
    }

    /// <summary>
    /// Проверяет, что коллекция не равна null и не пуста.
    /// </summary>
    /// <typeparam name="T">Тип элементов коллекции.</typeparam>
    /// <param name="collection">Проверяемая коллекция.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходная коллекция, если валидна.</returns>
    /// <exception cref="ArgumentNullException">Выбрасывается, если коллекция равна null.</exception>
    /// <exception cref="ArgumentException">Выбрасывается, если коллекция пуста.</exception>
    public static ICollection<T> NotNullOrEmpty<T>([NotNull] ICollection<T>? collection, 
        [CallerArgumentExpression(nameof(collection))] string? paramName = null)
    {
        if (collection is null)
        {
            throw new ArgumentNullException(paramName);
        }
        
        if (collection.Count == 0)
        {
            throw new ArgumentException("Коллекция не может быть пустой.", paramName);
        }
        
        return collection;
    }

    /// <summary>
    /// Проверяет, что email адрес имеет корректный формат.
    /// </summary>
    /// <param name="email">Проверяемый email адрес.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходный email, если он валиден.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если email имеет неверный формат.</exception>
    public static string ValidEmail([NotNull] string? email, 
        [CallerArgumentExpression(nameof(email))] string? paramName = null)
    {
        var value = NotNullOrWhiteSpace(email, paramName);
        
        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            if (addr.Address == value)
            {
                return value;
            }
            throw new ArgumentException("Неверный формат email адреса.", paramName);
        }
        catch (FormatException ex)
        {
            throw new ArgumentException("Неверный формат email адреса.", paramName, ex);
        }
    }

    /// <summary>
    /// Проверяет, что GUID не является пустым.
    /// </summary>
    /// <param name="guid">Проверяемый GUID.</param>
    /// <param name="paramName">Имя параметра (автоматически определяется).</param>
    /// <returns>Исходный GUID, если он не пустой.</returns>
    /// <exception cref="ArgumentException">Выбрасывается, если GUID пустой.</exception>
    public static Guid NotEmpty(Guid guid, [CallerArgumentExpression(nameof(guid))] string? paramName = null)
    {
        if (guid == Guid.Empty)
        {
            throw new ArgumentException("GUID не может быть пустым.", paramName);
        }
        return guid;
    }
}