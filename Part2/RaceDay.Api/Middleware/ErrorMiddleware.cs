// Returns API errors as Problem Details responses.
// References: HTTP error status meanings: [12]. See docs/references.md.

using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Services;

namespace RaceDay.Api.Middleware
{
    public class ErrorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorMiddleware> _logger;
        public ErrorMiddleware(RequestDelegate next, ILogger<ErrorMiddleware> logger) { _next = next; _logger = logger; }

        public async Task InvokeAsync(HttpContext context)
        {
            try { await _next(context); }
            catch (ApiException error)
            {
                await Results.Problem(statusCode: error.StatusCode, title: "Request could not be completed", detail: error.Message).ExecuteAsync(context);
            }
            catch (DbUpdateException error)
            {
                _logger.LogWarning(error, "A database write failed.");
                await Results.Problem(statusCode: 409, title: "Database conflict", detail: "The change conflicts with existing data or a database constraint.").ExecuteAsync(context);
            }
            catch (DbException error)
            {
                _logger.LogError(error, "The database could not be reached.");
                await Results.Problem(statusCode: 503, title: "Database unavailable", detail: "Check the SQL Server connection and try again.").ExecuteAsync(context);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Unexpected request failure.");
                await Results.Problem(statusCode: 500, title: "Unexpected server error", detail: "The request failed. Check the server log for details.").ExecuteAsync(context);
            }
        }
    }
}
