// Tests database constraints and demo data.
// References: Test setup, action and assertions: [13]. API test host: [3]. See docs/references.md.

using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class DatabaseTests
    {
        [Fact]
        public async Task Database_rejects_a_category_and_event_mismatch()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var first = await scenario.Event(owner.Client);
            var second = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, first.EventId);
            scenario.Factory.WithDatabase(db =>
            {
                db.Enrolments.Add(new Enrolment { EventId = second.EventId, CategoryId = category.CategoryId, ParticipantId = participant.Profile.UserId });
                Assert.Throws<DbUpdateException>(() => db.SaveChanges());
            });
        }

        [Fact]
        public async Task Database_rejects_duplicate_entries_and_result_rows()
        {
            using var scenario = new TestScenario();
            var owner = await scenario.Account("Organiser");
            var participant = await scenario.Account("Participant");
            var race = await scenario.Event(owner.Client);
            var category = await scenario.Category(owner.Client, race.EventId);
            var entry = await scenario.Enter(participant.Client, race.EventId, category.CategoryId);
            scenario.Factory.WithDatabase(db =>
            {
                db.Enrolments.Add(new Enrolment { ParticipantId = participant.Profile.UserId, EventId = race.EventId, CategoryId = category.CategoryId });
                Assert.Throws<DbUpdateException>(() => db.SaveChanges());
            });
            scenario.Factory.WithDatabase(db =>
            {
                db.Results.Add(new RaceResult { EnrolmentId = entry.EnrolmentId, FinishTime = TimeSpan.FromMinutes(40), OverallPosition = 1, CategoryPosition = 1 });
                db.SaveChanges();
                db.Results.Add(new RaceResult { EnrolmentId = entry.EnrolmentId, FinishTime = TimeSpan.FromMinutes(41), OverallPosition = 2, CategoryPosition = 2 });
                Assert.Throws<DbUpdateException>(() => db.SaveChanges());
            });
        }

        [Fact]
        public async Task Demo_seed_is_idempotent_and_both_roles_can_log_in()
        {
            using var scenario = new TestScenario();
            using (var scope = scenario.Factory.Services.CreateScope())
            {
                await SeedData.Initialise(scope.ServiceProvider);
                await SeedData.Initialise(scope.ServiceProvider);
                var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
                Assert.Equal(4, await db.Users.CountAsync());
                Assert.Equal(3, await db.Events.CountAsync());
                Assert.Equal(3, await db.Categories.CountAsync());
                Assert.Equal(4, await db.Enrolments.CountAsync());
                Assert.Equal(1, await db.Results.CountAsync());
                Assert.Equal(1, await db.AuditLogs.CountAsync());
            }
            foreach (var email in new[] { "organiser1@raceday.example", "participant1@raceday.example" })
            {
                var response = await scenario.Client().PostAsJsonAsync("/api/auth/login", new LoginRequest { Email = email, Password = SeedData.DemoPassword });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            }
        }
    }
}
