// Checks that Swagger lists the required routes.
// References: Swagger setup: [4]. Request and response checks: [16]. See docs/references.md.

using System.Net;
using System.Text.Json;
using Xunit;

namespace RaceDay.Tests
{
    public class SwaggerTests
    {
        [Fact]
        public async Task Swagger_UI_and_document_are_available_without_a_session()
        {
            using var scenario = new TestScenario();
            var client = scenario.Client();
            Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/index.html")).StatusCode);
            using var document = JsonDocument.Parse(await client.GetStringAsync("/swagger/v1/swagger.json"));
            var paths = document.RootElement.GetProperty("paths");
            Assert.True(paths.TryGetProperty("/api/auth/register", out _));
            Assert.True(paths.TryGetProperty("/api/auth/login", out _));
            Assert.True(paths.TryGetProperty("/api/users/profile", out _));
            Assert.True(paths.TryGetProperty("/api/events", out _));
            Assert.True(paths.TryGetProperty("/api/events/{id}", out _));
            Assert.True(paths.TryGetProperty("/api/events/{eventId}/categories", out _));
            Assert.True(paths.TryGetProperty("/api/enrolments", out _));
            Assert.True(paths.TryGetProperty("/api/enrolments/my-enrolments", out _));
            Assert.True(paths.TryGetProperty("/api/events/{eventId}/enrolments", out _));
            Assert.True(paths.TryGetProperty("/api/results", out _));
            Assert.True(paths.TryGetProperty("/api/results/my-results", out _));
            int endpoints = 0;
            foreach (var path in paths.EnumerateObject())
            {
                foreach (var operation in path.Value.EnumerateObject())
                {
                    Assert.False(string.IsNullOrWhiteSpace(operation.Value.GetProperty("summary").GetString()));
                    Assert.True(operation.Value.GetProperty("responses").EnumerateObject().Any());
                    if (operation.Name == "post" && path.Name != "/api/auth/logout" || operation.Name == "put")
                        Assert.True(operation.Value.TryGetProperty("requestBody", out _));
                    endpoints++;
                }
            }
            Assert.Equal(19, endpoints);
        }
    }
}
