using System.ComponentModel.DataAnnotations;

namespace TicketNest.Api.Models.V1.Errors;

public class ErrorModel
{
    [Required] public string Message { get; set; } = null!;
}