namespace TicketNest.Shared.Objects;

public sealed class Result<TValue, TError> : ResultBase<TError>
{
    private readonly TValue _value = default;

    /// <exception cref="InvalidOperationException"></exception>
    public TValue Value
        => IsSuccess
            ? _value
            : throw new InvalidOperationException("Объект не инициализирован значением Success");

    private Result(TValue value) => _value = value;

    private Result(TError error) : base(error)
    {
    }

    public static Result<TValue, TError> FromSuccess(TValue value) => new Result<TValue, TError>(value);

    public static Result<TValue, TError> FromFailure(TError error) => new Result<TValue, TError>(error);

    public static implicit operator Result<TValue, TError>(TValue value) => FromSuccess(value);

    public static implicit operator Result<TValue, TError>(TError error) => FromFailure(error);
}