// Tests event and category management.
// References: Test setup, action and assertions: [13]. API test host: [3]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class EventTests
    {
        [Fact]
        public async Task Organiser_can_create_read_update_and_delete_an_unused_event()
        {
            using var scenario = new TestScenario();
            var account = await scenario.Account("Organiser");
            var race = await scenario.Event(account.Client);
            Assert.Equal(account.Profile.UserId, race.OrganiserId);
            var anonymous = scenario.Client();
            var read = await anonymous.GetFromJsonAsync<RaceEvent>($"/api/events/{race.EventId}");
            Assert.Equal(race.Name, read!.Name);
            var change = TestScenario.EventDetails(); change.Name = "Updated Road Run";
            var update = await account.Client.PutAsJsonAsync($"/api/events/{race.EventId}", change);
            Assert.Equal(HttpStatusCode.OK, update.StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Equal(change.Name, db.Events.Find(race.EventId)!.Name));
            await scenario.Category(account.Client, race.EventId);
            Assert.Equal(HttpStatusCode.OK, (await account.Client.DeleteAsync($"/api/events/{race.EventId}")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await anonymous.GetAsync($"/api/events/{race.EventId}")).StatusCode);
            scenario.Factory.WithDatabase(db => { Assert.Empty(db.Categories); Assert.Equal(6, db.AuditLogs.Count()); });
        }

        [Fact]
        public async Task Both_roles_and_anonymous_visitors_can_browse_events_and_categories()
        {
            using var scenario = new TestScenario();
            var organiser = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(organiser.Client);
            await scenario.Category(organiser.Client, race.EventId);
            foreach (var client in new[] { organiser.Client, participant.Client, scenario.Client() })
            {
                Assert.Single((await client.GetFromJsonAsync<List<RaceEvent>>("/api/events"))!);
                Assert.Single((await client.GetFromJsonAsync<List<Category>>($"/api/events/{race.EventId}/categories"))!);
            }
        }

        [Fact]
        public async Task Another_organiser_cannot_edit_delete_or_manage_your_event()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var other = await scenario.Account("Organiser");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            string path = $"/api/events/{race.EventId}";
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.PutAsJsonAsync(path, TestScenario.EventDetails())).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.DeleteAsync(path)).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.PostAsJsonAsync(path + "/categories", TestScenario.CategoryDetails())).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.PutAsJsonAsync(path + $"/categories/{category.CategoryId}", TestScenario.CategoryDetails())).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.DeleteAsync(path + $"/categories/{category.CategoryId}")).StatusCode);
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.GetAsync(path + "/enrolments")).StatusCode);
        }

        [Theory]
        [InlineData("eventType")]
        [InlineData("distance")]
        [InlineData("name")]
        [InlineData("date")]
        [InlineData("precision")]
        public async Task Invalid_event_fields_are_rejected(string field)
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var request = TestScenario.EventDetails();
            if (field == "eventType") request.EventType = "Swimming";
            if (field == "distance") request.DistanceKm = 0;
            if (field == "name") request.Name = "   ";
            if (field == "date") request.EventDate = null;
            if (field == "precision") request.DistanceKm = 10.001m;
            Assert.Equal(HttpStatusCode.BadRequest, (await owner.Client.PostAsJsonAsync("/api/events", request)).StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Empty(db.Events));
        }

        [Fact]
        public async Task Category_crud_works_and_duplicate_names_are_rejected()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            string path = $"/api/events/{race.EventId}/categories";
            Assert.Equal(HttpStatusCode.Conflict, (await owner.Client.PostAsJsonAsync(path, TestScenario.CategoryDetails())).StatusCode);
            var request = TestScenario.CategoryDetails("Senior");
            Assert.Equal(HttpStatusCode.OK, (await owner.Client.PutAsJsonAsync(path + $"/{category.CategoryId}", request)).StatusCode);
            Assert.Equal("Senior", (await owner.Client.GetFromJsonAsync<List<Category>>(path))!.Single().CategoryName);
            Assert.Equal(HttpStatusCode.NoContent, (await owner.Client.DeleteAsync(path + $"/{category.CategoryId}")).StatusCode);
            Assert.Empty((await owner.Client.GetFromJsonAsync<List<Category>>(path))!);
        }

        [Theory]
        [InlineData(-1, 10)]
        [InlineData(100, 0)]
        public async Task Negative_fee_or_zero_distance_is_rejected(decimal fee, decimal distance)
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var race = await scenario.Event(owner.Client);
            var request = TestScenario.CategoryDetails(); request.Fee = fee; request.DistanceKm = distance;
            Assert.Equal(HttpStatusCode.BadRequest, (await owner.Client.PostAsJsonAsync($"/api/events/{race.EventId}/categories", request)).StatusCode);
        }

        [Fact]
        public async Task Events_and_categories_with_enrolments_cannot_be_deleted()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            await scenario.Enter(participant.Client, race.EventId, category.CategoryId);
            Assert.Equal(HttpStatusCode.Conflict, (await owner.Client.DeleteAsync($"/api/events/{race.EventId}")).StatusCode);
            Assert.Equal(HttpStatusCode.Conflict, (await owner.Client.DeleteAsync($"/api/events/{race.EventId}/categories/{category.CategoryId}")).StatusCode);
            scenario.Factory.WithDatabase(db => { Assert.Single(db.Events); Assert.Single(db.Enrolments); Assert.Single(db.Categories); });
        }

        [Fact]
        public async Task Missing_event_and_mismatched_category_route_return_404()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            Assert.Equal(HttpStatusCode.NotFound, (await owner.Client.GetAsync("/api/events/99999")).StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, (await owner.Client.GetAsync("/api/events/99999/categories")).StatusCode);
            var first = await scenario.Event(owner.Client);
            var second = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, first.EventId);
            Assert.Equal(HttpStatusCode.NotFound, (await owner.Client.DeleteAsync($"/api/events/{second.EventId}/categories/{category.CategoryId}")).StatusCode);
        }
    }
}
