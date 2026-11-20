// Request classes and input validation.
// References: Request fields: [6], [7]. Range validation: [17]. See docs/references.md.

using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Contracts
{
    public class RegisterRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = "";
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = "";
        [Required, StringLength(128, MinimumLength = 12)]
        public string Password { get; set; } = "";
        [Required, RegularExpression("^(Organiser|Participant)$")]
        public string Role { get; set; } = "";
        [Required, Phone, StringLength(30)]
        public string PhoneNumber { get; set; } = "";
    }

    public class LoginRequest
    {
        [Required, EmailAddress, StringLength(254)]
        public string Email { get; set; } = "";
        [Required, StringLength(128)]
        public string Password { get; set; } = "";
    }

    public class ProfileRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = "";
        [Required, Phone, StringLength(30)]
        public string PhoneNumber { get; set; } = "";
        [Url, StringLength(2048)]
        public string? ProfilePictureUrl { get; set; }
    }

    public class EventRequest
    {
        [Required, StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = "";
        [Required, StringLength(2000)]
        public string Description { get; set; } = "";
        [Required]
        public DateTime? EventDate { get; set; }
        [Required, StringLength(200)]
        public string Location { get; set; } = "";
        [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
        public decimal DistanceKm { get; set; }
        [Required, RegularExpression("^(Run|Walk|Cycle)$")]
        public string EventType { get; set; } = "";
        [Url, StringLength(2048)]
        public string? BannerImageUrl { get; set; }
    }

    public class CategoryRequest
    {
        [Required, StringLength(100, MinimumLength = 2)]
        public string CategoryName { get; set; } = "";
        [Required, StringLength(1000)]
        public string Description { get; set; } = "";
        [Range(typeof(decimal), "0.01", "999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
        public decimal DistanceKm { get; set; }
        [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
        public decimal Fee { get; set; }
    }

    public class EnrolmentRequest
    {
        [Range(1, int.MaxValue)]
        public int EventId { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }

    public class ResultRequest
    {
        [Range(1, int.MaxValue)]
        public int EnrolmentId { get; set; }
        [Required, RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d:[0-5]\d$")]
        public string FinishTime { get; set; } = "";
        [Range(1, int.MaxValue)]
        public int OverallPosition { get; set; }
        [Range(1, int.MaxValue)]
        public int CategoryPosition { get; set; }
    }
}
