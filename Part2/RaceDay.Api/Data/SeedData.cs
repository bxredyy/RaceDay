// Adds demo users, events, entries and results to an empty database.
// References: Demo requirements: [6]. Linked records: [2], [10]. See docs/references.md.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public static class SeedData
    {
        public const string DemoPassword = "RaceDay!Demo2026";

        public static async Task Initialise(IServiceProvider services)
        {
            var db = services.GetRequiredService<RaceDayContext>();
            if (await db.Users.AnyAsync()) return;
            var hasher = services.GetRequiredService<IPasswordHasher<User>>();
            var users = new[]
            {
                new User { FullName = "Naledi Mokoena", Email = "organiser1@raceday.example", Role = "Organiser", PhoneNumber = "0125550101" },
                new User { FullName = "Thabo Dlamini", Email = "organiser2@raceday.example", Role = "Organiser", PhoneNumber = "0125550102" },
                new User { FullName = "Lerato Ndlovu", Email = "participant1@raceday.example", Role = "Participant", PhoneNumber = "0725550103" },
                new User { FullName = "Sipho Nkosi", Email = "participant2@raceday.example", Role = "Participant", PhoneNumber = "0725550104" }
            };
            await using var transaction = await db.Database.BeginTransactionAsync();
            foreach (var user in users) user.PasswordHash = hasher.HashPassword(user, DemoPassword);
            db.Users.AddRange(users);
            await db.SaveChangesAsync();
            var events = new[]
            {
                new RaceEvent { OrganiserId = users[0].UserId, Name = "Pretoria Community Run", Description = "Community road run in Pretoria.", EventDate = DateTime.UtcNow.AddDays(30), Location = "Pretoria", DistanceKm = 10, EventType = "Run" },
                new RaceEvent { OrganiserId = users[1].UserId, Name = "Lynnwood Charity Cycle", Description = "Charity cycling event.", EventDate = DateTime.UtcNow.AddDays(45), Location = "Lynnwood, Pretoria", DistanceKm = 40, EventType = "Cycle" },
                new RaceEvent { OrganiserId = users[0].UserId, Name = "Tshwane Heritage Walk", Description = "Completed walk for result demonstrations.", EventDate = DateTime.UtcNow.AddDays(-7), Location = "Tshwane", DistanceKm = 5, EventType = "Walk" }
            };
            db.Events.AddRange(events);
            await db.SaveChangesAsync();
            var categories = events.Select(race => new Category
            {
                EventId = race.EventId, CategoryName = "Open", Description = "Open entry category", DistanceKm = race.DistanceKm, Fee = 100
            }).ToArray();
            db.Categories.AddRange(categories);
            await db.SaveChangesAsync();
            var entries = new[]
            {
                new Enrolment { ParticipantId = users[2].UserId, EventId = events[0].EventId, CategoryId = categories[0].CategoryId },
                new Enrolment { ParticipantId = users[3].UserId, EventId = events[1].EventId, CategoryId = categories[1].CategoryId },
                new Enrolment { ParticipantId = users[2].UserId, EventId = events[2].EventId, CategoryId = categories[2].CategoryId, EnrolmentDate = events[2].EventDate.AddDays(-1) },
                new Enrolment { ParticipantId = users[3].UserId, EventId = events[2].EventId, CategoryId = categories[2].CategoryId, EnrolmentDate = events[2].EventDate.AddDays(-1) }
            };
            db.Enrolments.AddRange(entries);
            await db.SaveChangesAsync();
            db.Results.Add(new RaceResult { EnrolmentId = entries[2].EnrolmentId, FinishTime = TimeSpan.FromMinutes(42), OverallPosition = 1, CategoryPosition = 1 });
            db.AuditLogs.Add(new AuditLog { UserId = users[0].UserId, Action = "Demo data initialised." });
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
    }
}
