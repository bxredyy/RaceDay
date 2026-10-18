// A participant entry linked to an event and category.
// References: Entry fields: [7]. Table relationships: [10]. See docs/references.md.

namespace RaceDay.Api.Models
{
    public class Enrolment
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public int EventId { get; set; }
        public int CategoryId { get; set; }
        public DateTime EnrolmentDate { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Confirmed";
    }
}
