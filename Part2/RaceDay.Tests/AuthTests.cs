// Tests accounts, login, profiles and sessions.
// References: Test setup, action and assertions: [13]. API test host: [3]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using RaceDay.Api.Contracts;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class AuthTests
    {
        [Theory]
        [InlineData("Organiser")]
        [InlineData("Participant")]
        public async Task Both_roles_can_register_login_and_read_their_profile(string role)
        {
            using var scenario = new TestScenario();
            var account = await scenario.Account(role);
            var profile = await account.Client.GetFromJsonAsync<ProfileResponse>("/api/users/profile");
            Assert.Equal(role, profile!.Role);
            Assert.Equal(account.Profile.UserId, profile.UserId);
        }

        [Fact]
        public async Task Registration_saves_a_hash_and_never_returns_it()
        {
            using var scenario = new TestScenario();
            var client = scenario.Client();
            var request = TestScenario.Registration();
            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.DoesNotContain("password", json, StringComparison.OrdinalIgnoreCase);
            scenario.Factory.WithDatabase(db =>
            {
                var user = db.Users.Single();
                Assert.NotEqual(request.Password, user.PasswordHash);
                Assert.NotEqual(PasswordVerificationResult.Failed, new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password));
                Assert.Single(db.AuditLogs);
            });
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/profile")).StatusCode);
        }

        [Fact]
        public async Task Email_is_normalised_and_duplicates_are_rejected()
        {
            using var scenario = new TestScenario();
            var client = scenario.Client();
            var request = TestScenario.Registration();
            Assert.Equal(HttpStatusCode.Created, (await client.PostAsJsonAsync("/api/auth/register", request)).StatusCode);
            request.Email = request.Email.ToUpperInvariant();
            Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/auth/register", request)).StatusCode);
            Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = request.Email, Password = request.Password })).StatusCode);
        }

        [Theory]
        [InlineData("Admin", "RaceDay!Test2026", "test@example.test")]
        [InlineData("Participant", "short", "test@example.test")]
        [InlineData("Participant", "RaceDay!Test2026", "not-an-email")]
        public async Task Invalid_registration_returns_400(string role, string password, string email)
        {
            using var scenario = new TestScenario();
            var request = TestScenario.Registration(role);
            request.Password = password; request.Email = email;
            Assert.Equal(HttpStatusCode.BadRequest, (await scenario.Client().PostAsJsonAsync("/api/auth/register", request)).StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Empty(db.Users));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Wrong_password_and_unknown_account_return_401(bool unknown)
        {
            using var scenario = new TestScenario();
            var account = await scenario.Account("Participant");
            var client = scenario.Client();
            var response = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest
            {
                Email = unknown ? "absent@example.test" : account.Profile.Email, Password = "IncorrectPassword!"
            });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/profile")).StatusCode);
        }

        [Fact]
        public async Task Logout_invalidates_the_server_side_session()
        {
            using var scenario = new TestScenario();
            var account = await scenario.Account("Participant");
            Assert.Equal(HttpStatusCode.NoContent, (await account.Client.PostAsync("/api/auth/logout", null)).StatusCode);
            Assert.Equal(HttpStatusCode.Unauthorized, (await account.Client.GetAsync("/api/users/profile")).StatusCode);
        }

        [Fact]
        public async Task Session_cookie_is_http_only_and_same_site_strict()
        {
            using var scenario = new TestScenario();
            var client = scenario.Client();
            var request = TestScenario.Registration();
            await client.PostAsJsonAsync("/api/auth/register", request);
            var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = request.Email, Password = request.Password });
            var cookies = string.Join(";", login.Headers.GetValues("Set-Cookie"));
            Assert.Contains("httponly", cookies, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("samesite=strict", cookies, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Changing_account_role_invalidates_an_existing_session()
        {
            using var scenario = new TestScenario();
            var account = await scenario.Account("Participant");
            scenario.Factory.WithDatabase(db => { db.Users.Find(account.Profile.UserId)!.Role = "Organiser"; db.SaveChanges(); });
            Assert.Equal(HttpStatusCode.Unauthorized, (await account.Client.GetAsync("/api/users/profile")).StatusCode);
        }

        [Theory]
        [InlineData("Organiser")]
        [InlineData("Participant")]
        public async Task Profile_updates_cannot_change_role_or_another_users_data(string role)
        {
            using var scenario = new TestScenario();
            var first = await scenario.Account(role);
            var second = await scenario.Account(role);
            var response = await first.Client.PutAsJsonAsync("/api/users/profile", new
            {
                fullName = "Updated Name", phoneNumber = "0825551234", profilePictureUrl = "https://example.test/photo.png",
                role = "Admin", userId = second.Profile.UserId
            });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>();
            Assert.Equal(role, profile!.Role);
            Assert.Equal(first.Profile.UserId, profile.UserId);
            Assert.Equal("Updated Name", profile.FullName);
            var unchanged = await second.Client.GetFromJsonAsync<ProfileResponse>("/api/users/profile");
            Assert.Equal("Test User", unchanged!.FullName);
        }

        [Fact]
        public async Task Cross_origin_browser_write_is_rejected()
        {
            using var scenario = new TestScenario();
            var account = await scenario.Account("Organiser");
            account.Client.DefaultRequestHeaders.Add("Origin", "https://unrelated.example");
            Assert.Equal(HttpStatusCode.Forbidden, (await account.Client.PostAsJsonAsync("/api/events", TestScenario.EventDetails())).StatusCode);
        }
    }
}
