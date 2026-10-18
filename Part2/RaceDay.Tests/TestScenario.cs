// Shared helpers for creating accounts, events and entries in tests.
// References: API test host and clients: [3]. Test setup: [13]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.Api.Contracts;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class TestScenario : IDisposable
    {
        public ApiFactory Factory { get; } = new ApiFactory();
        public const string Password = "RaceDay!Test2026";
        private readonly List<HttpClient> _clients = new List<HttpClient>();

        public TestScenario() { Factory.Initialise(); }

        public HttpClient Client()
        {
            var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true, AllowAutoRedirect = false });
            _clients.Add(client);
            return client;
        }

        public async Task<(HttpClient Client, ProfileResponse Profile)> Account(string role)
        {
            var client = Client();
            var request = Registration(role);
            var response = await client.PostAsJsonAsync("/api/auth/register", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = request.Email, Password = Password });
            Assert.Equal(HttpStatusCode.OK, login.StatusCode);
            return (client, (await login.Content.ReadFromJsonAsync<ProfileResponse>())!);
        }

        public static RegisterRequest Registration(string role = "Participant")
        {
            return new RegisterRequest
            {
                FullName = "Test User", Email = Guid.NewGuid().ToString("N") + "@example.test",
                Password = Password, Role = role, PhoneNumber = "0721234567"
            };
        }

        public static EventRequest EventDetails()
        {
            return new EventRequest
            {
                Name = "Pretoria Road Run", Description = "A community road running event.",
                EventDate = DateTime.UtcNow.AddDays(10), Location = "Pretoria", DistanceKm = 10, EventType = "Run"
            };
        }

        public async Task<RaceEvent> Event(HttpClient organiser)
        {
            var response = await organiser.PostAsJsonAsync("/api/events", EventDetails());
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<RaceEvent>())!;
        }

        public static CategoryRequest CategoryDetails(string name = "Open")
        {
            return new CategoryRequest { CategoryName = name, Description = "Open distance category", DistanceKm = 10, Fee = 120 };
        }

        public async Task<Category> Category(HttpClient organiser, int eventId, string name = "Open")
        {
            var response = await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories", CategoryDetails(name));
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<Category>())!;
        }

        public async Task<EnrolmentResponse> Enter(HttpClient participant, int eventId, int categoryId)
        {
            var response = await participant.PostAsJsonAsync("/api/enrolments", new EnrolmentRequest { EventId = eventId, CategoryId = categoryId });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            return (await response.Content.ReadFromJsonAsync<EnrolmentResponse>())!;
        }

        public void StartEvent(int eventId)
        {
            Factory.WithDatabase(db => { db.Events.Find(eventId)!.EventDate = DateTime.UtcNow.AddDays(-1); db.SaveChanges(); });
        }

        public void Dispose()
        {
            foreach (var client in _clients) client.Dispose();
            Factory.Dispose();
        }
    }
}
