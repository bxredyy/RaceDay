// Tests event entries and personal enrolment history.
// References: Test setup, action and assertions: [13]. API test host: [3]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Contracts;
using Xunit;

namespace RaceDay.Tests
{
    public class EnrolmentTests
    {
        [Fact]
        public async Task Participant_entry_records_the_correct_user_event_and_category()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            var response = await participant.Client.PostAsJsonAsync("/api/enrolments", new
            {
                eventId = race.EventId, categoryId = category.CategoryId, participantId = owner.Profile.UserId
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var entry = (await response.Content.ReadFromJsonAsync<EnrolmentResponse>())!;
            Assert.Equal(participant.Profile.UserId, entry.ParticipantId);
            Assert.Equal(race.EventId, entry.EventId);
            Assert.Equal(category.CategoryId, entry.CategoryId);
            Assert.Equal("Confirmed", entry.Status);
            scenario.Factory.WithDatabase(db =>
            {
                var saved = db.Enrolments.Single();
                Assert.Equal(entry.EnrolmentId, saved.EnrolmentId);
                Assert.Equal(participant.Profile.UserId, saved.ParticipantId);
            });
            Assert.Single((await owner.Client.GetFromJsonAsync<List<EnrolmentResponse>>($"/api/events/{race.EventId}/enrolments"))!);
        }

        [Fact]
        public async Task Duplicate_entry_is_rejected_without_creating_another_row()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            await scenario.Enter(participant.Client, race.EventId, category.CategoryId);
            var response = await participant.Client.PostAsJsonAsync("/api/enrolments", new EnrolmentRequest { EventId = race.EventId, CategoryId = category.CategoryId });
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Single(db.Enrolments));
        }

        [Fact]
        public async Task Category_from_another_event_is_rejected()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var first = await scenario.Event(owner.Client);
            var second = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, second.EventId);
            Assert.Equal(HttpStatusCode.BadRequest, (await participant.Client.PostAsJsonAsync("/api/enrolments", new EnrolmentRequest { EventId = first.EventId, CategoryId = category.CategoryId })).StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Empty(db.Enrolments));
        }

        [Fact]
        public async Task Participants_cannot_see_each_others_entries()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var first = await scenario.Account("Participant");
            var second = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            await scenario.Enter(first.Client, race.EventId, category.CategoryId);
            Assert.Single((await first.Client.GetFromJsonAsync<List<EnrolmentResponse>>("/api/enrolments/my-enrolments"))!);
            Assert.Empty((await second.Client.GetFromJsonAsync<List<EnrolmentResponse>>("/api/enrolments/my-enrolments"))!);
        }

        [Theory]
        [InlineData("past", HttpStatusCode.BadRequest)]
        [InlineData("event", HttpStatusCode.NotFound)]
        [InlineData("category", HttpStatusCode.NotFound)]
        [InlineData("zero", HttpStatusCode.BadRequest)]
        public async Task Invalid_entries_return_the_expected_status(string kind, HttpStatusCode expected)
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            if (kind == "past") scenario.StartEvent(race.EventId);
            var request = new EnrolmentRequest { EventId = race.EventId, CategoryId = category.CategoryId };
            if (kind == "event") request.EventId = 99999;
            if (kind == "category") request.CategoryId = 99999;
            if (kind == "zero") request.CategoryId = 0;
            Assert.Equal(expected, (await participant.Client.PostAsJsonAsync("/api/enrolments", request)).StatusCode);
        }
    }
}
