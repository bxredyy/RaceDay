// Response classes that leave out private account fields.
// References: Response requirements and fields: [6], [7]. See docs/references.md.

using RaceDay.Api.Models;

namespace RaceDay.Api.Contracts
{
    public class ProfileResponse
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string? ProfilePictureUrl { get; set; }

        public static ProfileResponse From(User user)
        {
            return new ProfileResponse
            {
                UserId = user.UserId, FullName = user.FullName, Email = user.Email,
                Role = user.Role, PhoneNumber = user.PhoneNumber, ProfilePictureUrl = user.ProfilePictureUrl
            };
        }
    }

    public class EnrolmentResponse
    {
        public int EnrolmentId { get; set; }
        public int ParticipantId { get; set; }
        public string ParticipantName { get; set; } = "";
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = "";
        public DateTime EnrolmentDate { get; set; }
        public string Status { get; set; } = "";
    }

    public class ResultResponse
    {
        public int ResultId { get; set; }
        public int EnrolmentId { get; set; }
        public int EventId { get; set; }
        public string EventName { get; set; } = "";
        public string CategoryName { get; set; } = "";
        public TimeSpan FinishTime { get; set; }
        public int OverallPosition { get; set; }
        public int CategoryPosition { get; set; }
        public DateTime RecordedAt { get; set; }
    }
}
