using System.Net;
using System.Net.Mime;
using Newtonsoft.Json;
using TicketNest.Api.Infrastructure;
using TicketNest.Api.Models.V1;
using TicketNest.Api.Models.V1.Errors;

namespace TicketNest.Api.Middlewares;

internal sealed class ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger) : IMiddleware
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
            LogError(errorModel);

            context.Response.StatusCode = (int) errorModel.HttpStatusCode;
            context.Response.ContentType = MediaTypeNames.Application.Json;

            var result = new ResultModel<EmptyResultModel>(result: null, error: errorModel);
            await using var writer = new StreamWriter(context.Response.Body);
            Serializer.Serialize(writer, result);
        }
    }

    private void LogError(ErrorModel errorModel)
    {
        if (errorModel.HttpStatusCode >= HttpStatusCode.InternalServerError)
        {
            logger.LogError(
                "{ErrorModelMessage}, детали: {ErrorModelDetail}, статус код: {ErrorModelErrorCode}, httpStatusCode: {ErrorModelHttpStatusCode}",
                errorModel.Message, errorModel.Detail, errorModel.ErrorCode, errorModel.HttpStatusCode);
            return;
        }

        logger.LogWarning(
            "{ErrorModelMessage}, детали: {ErrorModelDetail}, статус код: {ErrorModelErrorCode}, httpStatusCode: {ErrorModelHttpStatusCode}",
            errorModel.Message, errorModel.Detail, errorModel.ErrorCode, errorModel.HttpStatusCode);
    }

    private static JsonSerializer GetSerializer()
    {
        var settings = new JsonSerializerSettings();
        JsonSettingsConfigurator.ConfigureSettings(settings);
        return JsonSerializer.Create(settings);
    }
}