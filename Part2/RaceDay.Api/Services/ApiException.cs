// Stores the status code and message for an API error.
// References: HTTP status meanings: [12]. See docs/references.md.

namespace RaceDay.Api.Services
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public ApiException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
