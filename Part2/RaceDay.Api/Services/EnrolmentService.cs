// Checks event entries and returns enrolment details.
// References: Entry rules: [6], [7]. Related records: [2], [10]. See docs/references.md.

using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services
{
    public class EnrolmentService
    {
        private readonly RaceDayContext _db;
        private readonly EventService _events;
        private readonly AuditService _audit;
        public EnrolmentService(RaceDayContext db, EventService events, AuditService audit)
        {
            _db = db; _events = events; _audit = audit;
        }

        public async Task<EnrolmentResponse> Enter(int participantId, EnrolmentRequest request)
        {
            var race = await _events.Get(request.EventId);
            if (race.EventDate <= DateTime.UtcNow)
                throw new ApiException(400, "Entries are closed because the event has started.");
            var category = await _db.Categories.FindAsync(request.CategoryId);
            if (category == null) throw new ApiException(404, "Category not found.");
            if (category.EventId != request.EventId)
                throw new ApiException(400, "The selected category belongs to a different event.");
            if (await _db.Enrolments.AnyAsync(entry => entry.ParticipantId == participantId && entry.EventId == request.EventId))
                throw new ApiException(409, "You have already entered this event.");

            var entry = new Enrolment
            {
                ParticipantId = participantId, EventId = request.EventId, CategoryId = request.CategoryId
            };
            _db.Enrolments.Add(entry);
            _audit.Log(participantId, "Entered event " + request.EventId + " in category " + request.CategoryId + ".");
            await _db.SaveChangesAsync();
            return await Query().SingleAsync(item => item.EnrolmentId == entry.EnrolmentId);
        }

        public Task<List<EnrolmentResponse>> GetMine(int participantId)
        {
            return Query().Where(item => item.ParticipantId == participantId).OrderByDescending(item => item.EnrolmentDate).ToListAsync();
        }

        public async Task<List<EnrolmentResponse>> GetForEvent(int eventId, int organiserId)
        {
            await _events.RequireOwner(eventId, organiserId);
            return await Query().Where(item => item.EventId == eventId).OrderBy(item => item.EnrolmentId).ToListAsync();
        }

        private IQueryable<EnrolmentResponse> Query()
        {
            return from entry in _db.Enrolments.AsNoTracking()
                   join race in _db.Events on entry.EventId equals race.EventId
                   join category in _db.Categories on entry.CategoryId equals category.CategoryId
                   join participant in _db.Users on entry.ParticipantId equals participant.UserId
                   select new EnrolmentResponse
                   {
                       EnrolmentId = entry.EnrolmentId, ParticipantId = entry.ParticipantId,
                       ParticipantName = participant.FullName, EventId = entry.EventId, EventName = race.Name,
                       CategoryId = entry.CategoryId, CategoryName = category.CategoryName,
                       EnrolmentDate = entry.EnrolmentDate, Status = entry.Status
                   };
        }
    }
}
