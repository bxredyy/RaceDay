// A category and entry fee for an event.
// References: Category fields: [7]. Table relationships: [10]. See docs/references.md.

namespace RaceDay.Api.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public int EventId { get; set; }
        public string CategoryName { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal DistanceKm { get; set; }
        public decimal Fee { get; set; }
    }
}
