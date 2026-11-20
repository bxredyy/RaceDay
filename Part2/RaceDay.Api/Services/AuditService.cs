// Saves a record of important actions.
// References: Audit logging: [11]. Audit model: [7]. See docs/references.md.

using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services
{
    public class AuditService
    {
        private readonly RaceDayContext _db;
        public AuditService(RaceDayContext db) { _db = db; }

        public void Log(int userId, string action)
        {
            _db.AuditLogs.Add(new AuditLog { UserId = userId, Action = action });
        }
    }
}
