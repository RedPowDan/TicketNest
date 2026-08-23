using TicketNest.Auth.Api.Models.V1.Errors;

namespace TicketNest.Auth.Api.Models.V1;

public class ResultModel<TResult> where TResult : class
{
    /// <summary>
    /// Информация об успешном результате выполнения endpoint.
    /// </summary>
    /// <remarks>
    /// Not null если ответ успешный
    /// </remarks>
    public TResult? Result { get; }

    /// <summary>
    /// Информация об ошибке.
    /// </summary>
    /// <remarks>
    /// Not null если ответ ошибочный
    /// </remarks>
    public ErrorModel? Error { get; }

    public ResultModel(TResult? result, ErrorModel? error)
    {
        Result = result;
        Error = error;
    }

    public static ResultModel<TResult> FromSuccess(TResult result) => new(result: result, error: null);

    public static ResultModel<TResult> FromFailure(ErrorModel error) => new(result: null, error: error);

    public static implicit operator ResultModel<TResult>(TResult value) => FromSuccess(value);

    public static implicit operator ResultModel<TResult>(ErrorModel error) => FromFailure(error);
}