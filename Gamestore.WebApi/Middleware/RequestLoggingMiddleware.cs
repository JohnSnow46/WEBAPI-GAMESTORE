using System.Diagnostics;
using System.Text;

namespace Gamestore.WebApi.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        var originalRequestBody = context.Request.Body;
        var requestBodyContent = string.Empty;

        try
        {
            if (context.Request.ContentLength > 0)
            {
                context.Request.EnableBuffering();

                using var requestReader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                requestBodyContent = await requestReader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            using var responseMemoryStream = new MemoryStream();
            var originalResponseBody = context.Response.Body;
            context.Response.Body = responseMemoryStream;

            try
            {
                await _next(context);
            }
            finally
            {
                responseMemoryStream.Position = 0;
                var responseBodyContent = await new StreamReader(responseMemoryStream).ReadToEndAsync();

                stopwatch.Stop();

                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var url = $"{context.Request.Method} {context.Request.Scheme}://{context.Request.Host}{context.Request.Path}{context.Request.QueryString}";
                var statusCode = context.Response.StatusCode;
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                if (requestBodyContent.Length > 4000)
                {
                    requestBodyContent = string.Concat(requestBodyContent.AsSpan(0, 4000), "... [truncated]");
                }

                if (responseBodyContent.Length > 4000)
                {
                    responseBodyContent = string.Concat(responseBodyContent.AsSpan(0, 4000), "... [truncated]");
                }

                _logger.LogInformation(
                    "HTTP Request: {IpAddress} | {Url} | Status: {StatusCode} | Duration: {ElapsedMs}ms | " +
                    "Request: {RequestContent} | Response: {ResponseContent}",
                    ipAddress,
                    url,
                    statusCode,
                    elapsedMs,
                    requestBodyContent,
                    responseBodyContent);

                responseMemoryStream.Position = 0;
                await responseMemoryStream.CopyToAsync(originalResponseBody);
                context.Response.Body = originalResponseBody;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while logging request");
            context.Request.Body = originalRequestBody;
            throw;
        }
    }
}