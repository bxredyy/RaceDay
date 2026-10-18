// Tests result capture and personal result history.
// References: Test setup, action and assertions: [13]. API test host: [3]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using RaceDay.Api.Contracts;
using Xunit;

namespace RaceDay.Tests
{
    public class ResultTests
    {
        [Fact]
        public async Task Owner_captures_a_result_and_only_its_participant_sees_it()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var first = await scenario.Account("Participant");
            var second = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            var entry = await scenario.Enter(first.Client, race.EventId, category.CategoryId);
            scenario.StartEvent(race.EventId);
            var request = Details(entry.EnrolmentId);
            var response = await owner.Client.PostAsJsonAsync("/api/results", request);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = (await response.Content.ReadFromJsonAsync<ResultResponse>())!;
            Assert.Equal(TimeSpan.FromMinutes(45), result.FinishTime);
            Assert.Equal(race.Name, result.EventName);
            Assert.Equal(1, result.OverallPosition);
            var mine = await first.Client.GetFromJsonAsync<List<ResultResponse>>("/api/results/my-results");
            Assert.Equal(result.ResultId, Assert.Single(mine!).ResultId);
            Assert.Empty((await second.Client.GetFromJsonAsync<List<ResultResponse>>("/api/results/my-results"))!);
            Assert.Equal(HttpStatusCode.Conflict, (await owner.Client.PostAsJsonAsync("/api/results", request)).StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Single(db.Results));
        }

        [Fact]
        public async Task Another_organiser_cannot_capture_a_result_for_your_event()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var other = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            var entry = await scenario.Enter(participant.Client, race.EventId, category.CategoryId);
            scenario.StartEvent(race.EventId);
            Assert.Equal(HttpStatusCode.Forbidden, (await other.Client.PostAsJsonAsync("/api/results", Details(entry.EnrolmentId))).StatusCode);
        }

        [Theory]
        [InlineData("future")]
        [InlineData("time")]
        [InlineData("zeroTime")]
        [InlineData("position")]
        [InlineData("categoryPosition")]
        public async Task Invalid_result_is_rejected(string kind)
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            var entry = await scenario.Enter(participant.Client, race.EventId, category.CategoryId);
            if (kind != "future") scenario.StartEvent(race.EventId);
            var request = Details(entry.EnrolmentId);
            if (kind == "time") request.FinishTime = "25:99:00";
            if (kind == "zeroTime") request.FinishTime = "00:00:00";
            if (kind == "position") request.OverallPosition = 0;
            if (kind == "categoryPosition") request.CategoryPosition = 2;
            Assert.Equal(HttpStatusCode.BadRequest, (await owner.Client.PostAsJsonAsync("/api/results", request)).StatusCode);
            scenario.Factory.WithDatabase(db => Assert.Empty(db.Results));
        }

        [Fact]
        public async Task Missing_enrolment_returns_404()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            Assert.Equal(HttpStatusCode.NotFound, (await owner.Client.PostAsJsonAsync("/api/results", Details(99999))).StatusCode);
        }

        public static ResultRequest Details(int enrolmentId)
        {
            return new ResultRequest { EnrolmentId = enrolmentId, FinishTime = "00:45:00", OverallPosition = 1, CategoryPosition = 1 };
        }
    }
}
