using FluentValidation;
using RiseDiary.Model;
using System.Net;
using System.Text.Json;

namespace RiseDiary.WebAPI.Middleware;

internal sealed class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            var response = context.Response;
            response.Clear();
            response.ContentType = "application/json";

            (response.StatusCode, var contentData) = error switch
            {
                ValidationException => ((int)HttpStatusCode.BadRequest, new { Message = $"Ошибка валидации: {error.Message}" }),
                RecordNotFoundException => ((int)HttpStatusCode.NotFound, new { Message = "Запись не найдена" }),
                ImageNotFoundException => ((int)HttpStatusCode.NotFound, new { Message = "Изображение не найдено" }),
                ArgumentException => ((int)HttpStatusCode.BadRequest, new { error.Message }),
                OperationCanceledException => (499, new { Message = "Отменено пользователем" }),
                _ => ((int)HttpStatusCode.InternalServerError, new { error.Message })
            };

            _logger.LogWarning(
                error,
                "Server error: {serverErrorMessage}. Returned status code: {statusCode}",
                contentData.Message, response.StatusCode);

            var result = JsonSerializer.Serialize(contentData);

            await response.WriteAsync(result);
        }
    }
}
