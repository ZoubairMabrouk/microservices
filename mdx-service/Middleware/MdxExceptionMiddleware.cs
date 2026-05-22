using MdxServices.Models;
using Microsoft.AnalysisServices.AdomdClient;
using System.Net;
using System.Text.Json;

namespace MdxServices.Middleware
{
    public class MdxExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MdxExceptionMiddleware> _logger;

        public MdxExceptionMiddleware(RequestDelegate next, ILogger<MdxExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (AdomdConnectionException ex)
            {
                _logger.LogError(ex, "ADOMD connection failure");
                await WriteError(context, HttpStatusCode.ServiceUnavailable, $"OLAP connection unavailable: {ex.Message}");
            }
            catch (AdomdErrorResponseException ex)
            {
                _logger.LogError(ex, "ADOMD server error");
                await WriteError(context, HttpStatusCode.UnprocessableEntity, $"MDX execution error: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteError(context, HttpStatusCode.InternalServerError, "An unexpected error occurred.");
            }
        }

        private static Task WriteError(HttpContext ctx, HttpStatusCode code, string message)
        {
            ctx.Response.StatusCode = (int)code;
            ctx.Response.ContentType = "application/json";
            var payload = JsonSerializer.Serialize(ApiResponse<object>.Fail(message));
            return ctx.Response.WriteAsync(payload);
        }
    }
}
