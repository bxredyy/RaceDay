// A saved record of an action and its user.
// References: Audit model: [7]. Audit logging: [11]. See docs/references.md.

namespace RaceDay.Api.Models
{
    public class AuditLog
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
