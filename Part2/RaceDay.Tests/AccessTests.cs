// Tests that login, role and ownership rules are enforced.
// References: Test setup, action and assertions: [13]. API test host: [3]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace RaceDay.Tests
{
    public class AccessTests
    {
        public static IEnumerable<object[]> ProtectedRoutes()
        {
            yield return new object[] { "POST", "/api/auth/logout", "Any" };
            yield return new object[] { "GET", "/api/users/profile", "Any" };
            yield return new object[] { "PUT", "/api/users/profile", "Any" };
            yield return new object[] { "POST", "/api/events", "Organiser" };
            yield return new object[] { "PUT", "/api/events/1", "Organiser" };
            yield return new object[] { "DELETE", "/api/events/1", "Organiser" };
            yield return new object[] { "POST", "/api/events/1/categories", "Organiser" };
            yield return new object[] { "PUT", "/api/events/1/categories/1", "Organiser" };
            yield return new object[] { "DELETE", "/api/events/1/categories/1", "Organiser" };
            yield return new object[] { "GET", "/api/events/1/enrolments", "Organiser" };
            yield return new object[] { "POST", "/api/enrolments", "Participant" };
            yield return new object[] { "GET", "/api/enrolments/my-enrolments", "Participant" };
            yield return new object[] { "POST", "/api/results", "Organiser" };
            yield return new object[] { "GET", "/api/results/my-results", "Participant" };
        }

        [Theory, MemberData(nameof(ProtectedRoutes))]
        public async Task Unauthenticated_requests_are_rejected(string method, string path, string requiredRole)
        {
            Assert.Contains(requiredRole, new[] { "Any", "Organiser", "Participant" });
            using var scenario = new TestScenario();
            using var request = Request(method, path);
            Assert.Equal(HttpStatusCode.Unauthorized, (await scenario.Client().SendAsync(request)).StatusCode);
        }

        public static IEnumerable<object[]> RoleRoutes() => ProtectedRoutes().Where(row => (string)row[2] != "Any");

        [Theory, MemberData(nameof(RoleRoutes))]
        public async Task Wrong_role_is_rejected_on_every_role_specific_endpoint(string method, string path, string requiredRole)
        {
            using var scenario = new TestScenario();
            var wrongRole = requiredRole == "Organiser" ? "Participant" : "Organiser";
            var account = await scenario.Account(wrongRole);
            using var request = Request(method, path);
            Assert.Equal(HttpStatusCode.Forbidden, (await account.Client.SendAsync(request)).StatusCode);
        }

        private static HttpRequestMessage Request(string method, string path)
        {
            var request = new HttpRequestMessage(new HttpMethod(method), path);
            if (method == "POST" || method == "PUT")
            {
                object body = new { };
                if (path == "/api/users/profile") body = new { fullName = "Test User", phoneNumber = "0721234567" };
                else if (path.Contains("categories")) body = TestScenario.CategoryDetails();
                else if (path == "/api/results") body = ResultTests.Details(1);
                else if (path == "/api/enrolments") body = new { eventId = 1, categoryId = 1 };
                else if (path.StartsWith("/api/events")) body = TestScenario.EventDetails();
                request.Content = JsonContent.Create(body);
            }
            return request;
        }
    }
}
