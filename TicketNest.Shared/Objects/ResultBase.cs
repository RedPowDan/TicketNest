namespace TicketNest.Shared.Objects;

public abstract class ResultBase<TError>
{
    private readonly TError _error;

    public TError Error
        => IsFailure
            ? _error
            : throw new InvalidOperationException("Объект не инициализирован значением Failure");

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
        
    protected ResultBase(TError error)
    {
        _error = error;
        IsSuccess = false;
    }

    protected ResultBase() => IsSuccess = true;
}