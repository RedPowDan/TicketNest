using System.Net.Mime;
using Newtonsoft.Json;
using TicketNest.Api.Infrastructure;
using TicketNest.Api.Models.V1;

namespace TicketNest.Api.Middlewares;

internal sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private static readonly JsonSerializer Serializer = GetSerializer();

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next.Invoke(context);
        }
        catch (Exception exception)
        {
            var errorModel = ErrorModelFactory.Create(exception);

            context.Response.StatusCode = (int) errorModel.HttpStatusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var result = new ResultModel<EmptyResultModel>(result: null, error: errorModel);
            await using var writer = new StreamWriter(context.Response.Body);
            Serializer.Serialize(writer, result);
        }
    }

    private static JsonSerializer GetSerializer()
    {
        var settings = new JsonSerializerSettings();
        JsonSettingsConfigurator.ConfigureSettings(settings);
        return JsonSerializer.Create(settings);
    }
}