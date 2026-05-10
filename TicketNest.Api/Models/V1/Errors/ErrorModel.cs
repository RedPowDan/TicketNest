using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;

namespace TicketNest.Api.Models.V1.Errors;

public class ErrorModel
{
    [Required] public string Message { get; }

    public ErrorCode ErrorCode { get; }

    public string? Detail { get; }

    [JsonIgnore]
    public HttpStatusCode HttpStatusCode { get; }

    public ErrorModel(ErrorCode errorCode, string message, string? detail, HttpStatusCode httpStatusCode)
    {
        ErrorCode = errorCode;
        Message = message;
        Detail = detail;
        HttpStatusCode = httpStatusCode;
    }
}