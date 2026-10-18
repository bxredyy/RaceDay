// A saved record of an action and its user.

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
