namespace TicketNest.Shared.Objects;

public sealed class UnitResult<TError> : ResultBase<TError>
{
    private UnitResult()
    {
    }

    private UnitResult(TError error) : base(error)
    {
    }

    public static UnitResult<TError> FromSuccess() => new();

    public static UnitResult<TError> FromFailure(TError error) => new(error);

    public static implicit operator UnitResult<TError>(TError error) => FromFailure(error);
}