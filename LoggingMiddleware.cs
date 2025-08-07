using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;

namespace ShiftManagement
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var request = context.Request;

            // Log request info
            _logger.LogInformation("Request {method} {url} from {ip} | Query: {query}",
                request.Method,
                request.Path,
                context.Connection.RemoteIpAddress,
                request.QueryString);

            // Optionally log request body (for POST/PUT)
            if (request.ContentLength > 0 && request.Body.CanSeek)
            {
                request.Body.Position = 0;
                using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                var body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
                _logger.LogDebug("Request Body: {body}", body);
            }

            try
            {
                await _next(context);
                sw.Stop();

                _logger.LogInformation("Response {statusCode} for {method} {url} in {elapsed} ms",
                    context.Response.StatusCode,
                    request.Method,
                    request.Path,
                    sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error {statusCode} for {method} {url} in {elapsed} ms",
                    context.Response.StatusCode,
                    request.Method,
                    request.Path,
                    sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}