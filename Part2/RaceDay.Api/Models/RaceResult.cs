// The finish time and positions for an enrolment.
// References: Result fields: [7]. One result per entry: [10]. See docs/references.md.

namespace RaceDay.Api.Models
{
    public class RaceResult
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public TimeSpan FinishTime { get; set; }
        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
