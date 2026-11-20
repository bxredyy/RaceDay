using Microsoft.AspNetCore.Mvc;
using RaceDay.MVC.Services;
using RaceDay.MVC.Models;
using RaceDay.MVC.Services;

namespace RaceDay.MVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => RedirectToAction("Index", "Events");
}

public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;
    public AccountController(RaceDayApiClient api) => _api = api;

    [HttpGet] public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, user, err) = await _api.LoginAsync(model);
        if (!ok || user == null) { ModelState.AddModelError("", err ?? "Login failed."); return View(model); }

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);
        return user.Role == "Organiser" ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Index", "Events");
    }

    [HttpGet] public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var (ok, user, err) = await _api.RegisterAsync(model);
        if (!ok || user == null) { ModelState.AddModelError("", err ?? "Registration failed."); return View(model); }

        HttpContext.Session.SetInt32("UserId", user.UserId);
        HttpContext.Session.SetString("FullName", user.FullName);
        HttpContext.Session.SetString("Role", user.Role);
        return user.Role == "Organiser" ? RedirectToAction("Dashboard", "Organiser") : RedirectToAction("Index", "Events");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }
}

public class EventsController : Controller
{
    private readonly RaceDayApiClient _api;
    public EventsController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? eventType, string? search)
    {
        var events = await _api.GetEventsAsync();
        if (!string.IsNullOrWhiteSpace(eventType) && eventType != "All")
            events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(search))
            events = events.Where(e => e.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

        ViewBag.SelectedType = eventType ?? "All";
        ViewBag.Search = search ?? "";
        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetEventByIdAsync(id);
        if (ev == null) return NotFound();
        return View(ev);
    }

    [HttpPost]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        if (HttpContext.Session.GetInt32("UserId") == null) return RedirectToAction("Login", "Account");
        if (HttpContext.Session.GetString("Role") != "Participant") { TempData["Error"] = "Only Participants can enrol."; return RedirectToAction("Details", new { id = eventId }); }

        var (ok, err) = await _api.EnrolAsync(eventId, categoryId);
        TempData[ok ? "Success" : "Error"] = ok ? "Enrolled successfully!" : err;
        return RedirectToAction("Details", new { id = eventId });
    }
}

public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;
    public OrganiserController(RaceDayApiClient api) => _api = api;

    private IActionResult? Guard()
    {
        if (HttpContext.Session.GetInt32("UserId") == null) return RedirectToAction("Login", "Account");
        if (HttpContext.Session.GetString("Role") != "Organiser") return RedirectToAction("Index", "Events");
        return null;
    }

    public async Task<IActionResult> Dashboard()
    {
        if (Guard() is { } g) return g;
        var events = await _api.GetEventsAsync();
        var counts = new Dictionary<int, int>();
        foreach (var ev in events) counts[ev.EventId] = (await _api.GetEventEnrolmentsAsync(ev.EventId)).Count;
        return View(new OrganiserDashboardViewModel { Events = events, EnrolmentCountsByEvent = counts });
    }

    public async Task<IActionResult> Events() { if (Guard() is { } g) return g; return View(await _api.GetEventsAsync()); }
    [HttpGet] public IActionResult CreateEvent() { if (Guard() is { } g) return g; return View(new EventFormViewModel()); }

    [HttpPost]
    public async Task<IActionResult> CreateEvent(EventFormViewModel model)
    {
        if (Guard() is { } g) return g;
        if (!ModelState.IsValid) return View(model);
        var (ok, created) = await _api.CreateEventAsync(model);
        if (!ok || created == null) { ModelState.AddModelError("", "Failed to create event."); return View(model); }
        if (model.BannerFile != null) await _api.UploadEventBannerAsync(created.EventId, model.BannerFile);
        TempData["Success"] = "Event created.";
        return RedirectToAction("Events");
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        if (Guard() is { } g) return g;
        var ev = await _api.GetEventByIdAsync(id);
        if (ev == null) return NotFound();
        return View(new EventFormViewModel { EventId = ev.EventId, Name = ev.Name, Description = ev.Description, EventDate = ev.EventDate, Location = ev.Location, EventType = ev.EventType, BannerImageUrl = ev.BannerImageUrl });
    }

    [HttpPost]
    public async Task<IActionResult> EditEvent(EventFormViewModel model)
    {
        if (Guard() is { } g) return g;
        if (!ModelState.IsValid) return View(model);
        if (model.BannerFile != null)
        {
            var url = await _api.UploadEventBannerAsync(model.EventId, model.BannerFile);
            if (!string.IsNullOrEmpty(url)) model.BannerImageUrl = url;
        }
        if (!await _api.UpdateEventAsync(model.EventId, model)) { ModelState.AddModelError("", "Update failed."); return View(model); }
        TempData["Success"] = "Event updated.";
        return RedirectToAction("Events");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        if (Guard() is { } g) return g;
        await _api.DeleteEventAsync(id);
        TempData["Success"] = "Event deleted.";
        return RedirectToAction("Events");
    }

    [HttpGet]
    public async Task<IActionResult> Categories(int eventId)
    {
        if (Guard() is { } g) return g;
        ViewBag.Event = await _api.GetEventByIdAsync(eventId);
        return View(new CategoryFormViewModel { EventId = eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Categories(CategoryFormViewModel model)
    {
        if (Guard() is { } g) return g;
        if (!ModelState.IsValid) return View(model);
        TempData[await _api.AddCategoryAsync(model) ? "Success" : "Error"] = "Category operation finished.";
        return RedirectToAction("Categories", new { eventId = model.EventId });
    }

    public async Task<IActionResult> Enrolments(int eventId)
    {
        if (Guard() is { } g) return g;
        ViewBag.Event = await _api.GetEventByIdAsync(eventId);
        return View(await _api.GetEventEnrolmentsAsync(eventId));
    }

    [HttpGet]
    public async Task<IActionResult> Results(int eventId)
    {
        if (Guard() is { } g) return g;
        ViewBag.Event = await _api.GetEventByIdAsync(eventId);
        ViewBag.Enrolments = await _api.GetEventEnrolmentsAsync(eventId);
        return View(new ResultFormViewModel { EventId = eventId });
    }

    [HttpPost]
    public async Task<IActionResult> Results(ResultFormViewModel model)
    {
        if (Guard() is { } g) return g;
        if (!ModelState.IsValid) { ViewBag.Event = await _api.GetEventByIdAsync(model.EventId); ViewBag.Enrolments = await _api.GetEventEnrolmentsAsync(model.EventId); return View(model); }
        TempData[await _api.SubmitResultAsync(model) ? "Success" : "Error"] = "Result saved.";
        return RedirectToAction("Results", new { eventId = model.EventId });
    }
}

public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;
    public ParticipantController(RaceDayApiClient api) => _api = api;

    private IActionResult? Guard()
    {
        if (HttpContext.Session.GetInt32("UserId") == null) return RedirectToAction("Login", "Account");
        if (HttpContext.Session.GetString("Role") != "Participant") return RedirectToAction("Dashboard", "Organiser");
        return null;
    }

    public async Task<IActionResult> MyEnrolments() { if (Guard() is { } g) return g; return View(await _api.GetMyEnrolmentsAsync()); }
    public async Task<IActionResult> MyResults() { if (Guard() is { } g) return g; return View(await _api.GetMyResultsAsync()); }

    [HttpGet]
    public async Task<IActionResult> Profile() { if (Guard() is { } g) return g; return View(await _api.GetProfileAsync() ?? new ProfileViewModel()); }

    [HttpPost]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (Guard() is { } g) return g;
        if (model.ProfilePictureFile != null)
        {
            var url = await _api.UploadAvatarAsync(model.ProfilePictureFile);
            if (!string.IsNullOrEmpty(url)) model.ProfilePictureUrl = url;
        }
        if (await _api.UpdateProfileAsync(model)) { HttpContext.Session.SetString("FullName", model.FullName); TempData["Success"] = "Profile saved."; }
        else ModelState.AddModelError("", "Failed to update profile.");
        return View(model);
    }
}
