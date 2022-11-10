using System.Net;
using System.Text.Json;
using RiseDiary.Model;

namespace RiseDiary.WebAPI.Middleware;

internal sealed class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next)
    {
        _next = next;
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
            response.ContentType = "application/json";

            (response.StatusCode, var contentData) = error switch
            {
                RecordNotFoundException => ((int)HttpStatusCode.NotFound, new { Message = "Запись не найдена" }),
                ImageNotFoundException => ((int)HttpStatusCode.NotFound, new { Message = "Изображение не найдено" }),
                ArgumentException => ((int)HttpStatusCode.BadRequest, new { error.Message }),
                OperationCanceledException => (499, new { Message = "Отменено пользователем" }),
                _ => ((int)HttpStatusCode.InternalServerError, new { error.Message })
            };

            var result = JsonSerializer.Serialize(contentData);
            await response.WriteAsync(result);
        }
    }
}
