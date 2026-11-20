using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaceDay.MVC.Models;

public class RegisterViewModel
{
    [Required] public string FullName { get; set; } = string.Empty;
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, MinLength(6)] public string Password { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = "Participant";
    public string? PhoneNumber { get; set; }
}

public class LoginViewModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class EventDto
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string? BannerImageUrl { get; set; }
    public List<CategoryDto> Categories { get; set; } = new();
}

public class EventFormViewModel
{
    public int EventId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string Description { get; set; } = string.Empty;
    [Required] public DateTime EventDate { get; set; } = DateTime.Today.AddDays(14);
    [Required] public string Location { get; set; } = string.Empty;
    [Required] public string EventType { get; set; } = "Run";
    public string? BannerImageUrl { get; set; }
    public IFormFile? BannerFile { get; set; }
}

public class CategoryDto
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal DistanceKm { get; set; }
    public decimal Fee { get; set; }
}

public class CategoryFormViewModel
{
    [Required] public int EventId { get; set; }
    [Required] public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required, Range(0.1, 500)] public decimal DistanceKm { get; set; }
    [Range(0, 100000)] public decimal Fee { get; set; }
}

public class EnrolmentDto
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public string? ParticipantName { get; set; }
    public int EventId { get; set; }
    public string? EventName { get; set; }
    public DateTime? EventDate { get; set; }
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public string Status { get; set; } = "Confirmed";
}

public class ResultDto
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public string EventName { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TimeSpan FinishTime { get; set; }
    public int OverallPosition { get; set; }
    public int CategoryPosition { get; set; }
    public int TotalFinishers { get; set; }
}

public class ResultFormViewModel
{
    [Required] public int EventId { get; set; }
    [Required] public int EnrolmentId { get; set; }
    [Required] public string FinishTime { get; set; } = "00:45:00";
    [Required, Range(1, 100000)] public int OverallPosition { get; set; }
    [Required, Range(1, 100000)] public int CategoryPosition { get; set; }
}

public class ProfileViewModel
{
    [Required] public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public IFormFile? ProfilePictureFile { get; set; }
}

public class OrganiserDashboardViewModel
{
    public List<EventDto> Events { get; set; } = new();
    public Dictionary<int, int> EnrolmentCountsByEvent { get; set; } = new();
    public int TotalEvents => Events.Count;
    public int TotalEnrolments => EnrolmentCountsByEvent.Values.Sum();
}
public class ErrorViewModel { public string? RequestId { get; set; } public bool ShowRequestId => !string.IsNullOrEmpty(RequestId); } 
