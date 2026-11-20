// Saves race results and returns personal results.
// References: Result rules: [6], [7]. Related records: [2], [10]. See docs/references.md.

using System.Globalization;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services
{
    public class ResultService
    {
        private readonly RaceDayContext _db;
        private readonly EventService _events;
        private readonly AuditService _audit;
        public ResultService(RaceDayContext db, EventService events, AuditService audit)
        {
            _db = db; _events = events; _audit = audit;
        }

        public async Task<ResultResponse> Capture(int organiserId, ResultRequest request)
        {
            var entry = await _db.Enrolments.FindAsync(request.EnrolmentId)
                ?? throw new ApiException(404, "Enrolment not found.");
            var race = await _events.RequireOwner(entry.EventId, organiserId);
            if (race.EventDate > DateTime.UtcNow)
                throw new ApiException(400, "Results cannot be recorded before the event starts.");
            if (!TimeSpan.TryParseExact(request.FinishTime, @"hh\:mm\:ss", CultureInfo.InvariantCulture, out var finishTime) || finishTime <= TimeSpan.Zero)
                throw new ApiException(400, "Finish time must be between 00:00:01 and 23:59:59.");
            if (request.CategoryPosition > request.OverallPosition)
                throw new ApiException(400, "Category position cannot be greater than overall position.");
            if (await _db.Results.AnyAsync(result => result.EnrolmentId == request.EnrolmentId))
                throw new ApiException(409, "A result has already been recorded for this enrolment.");

            var result = new RaceResult
            {
                EnrolmentId = entry.EnrolmentId, FinishTime = finishTime,
                OverallPosition = request.OverallPosition, CategoryPosition = request.CategoryPosition
            };
            _db.Results.Add(result);
            _audit.Log(organiserId, "Result recorded for enrolment " + entry.EnrolmentId + ".");
            await _db.SaveChangesAsync();
            return await Query().Where(item => item.ParticipantId == entry.ParticipantId)
                .Select(item => item.Response).SingleAsync(item => item.ResultId == result.ResultId);
        }

        public Task<List<ResultResponse>> GetMine(int participantId)
        {
            return Query().Where(item => item.ParticipantId == participantId)
                .Select(item => item.Response).OrderByDescending(item => item.RecordedAt).ToListAsync();
        }

        private IQueryable<OwnedResult> Query()
        {
            return from result in _db.Results.AsNoTracking()
                   join entry in _db.Enrolments on result.EnrolmentId equals entry.EnrolmentId
                   join race in _db.Events on entry.EventId equals race.EventId
                   join category in _db.Categories on entry.CategoryId equals category.CategoryId
                   select new OwnedResult
                   {
                       ParticipantId = entry.ParticipantId,
                       Response = new ResultResponse
                       {
                           ResultId = result.ResultId, EnrolmentId = entry.EnrolmentId,
                           EventId = race.EventId, EventName = race.Name, CategoryName = category.CategoryName,
                           FinishTime = result.FinishTime, OverallPosition = result.OverallPosition,
                           CategoryPosition = result.CategoryPosition, RecordedAt = result.RecordedAt
                       }
                   };
        }

        private class OwnedResult
        {
            public int ParticipantId { get; set; }
            public ResultResponse Response { get; set; } = new ResultResponse();
        }
    }
}
