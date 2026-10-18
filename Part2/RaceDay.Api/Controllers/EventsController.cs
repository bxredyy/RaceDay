// Public event browsing and organiser event management.
// References: HTTP methods: [15]. Status codes: [12]. Role checks: [11], [18]. See docs/references.md.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Models;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController, Route("api/events"), Produces("application/json")]
    public class EventsController : ControllerBase
    {
        private readonly EventService _events;
        private readonly EnrolmentService _enrolments;
        public EventsController(EventService events, EnrolmentService enrolments) { _events = events; _enrolments = enrolments; }

        /// <summary>List events in date order for public browsing and both roles.</summary>
        /// <remarks>No session or request body is required. Returns event details including distance in kilometres.</remarks>
        [HttpGet, AllowAnonymous]
        [ProducesResponseType(typeof(List<RaceEvent>), 200)]
        public async Task<IActionResult> GetAll() => Ok(await _events.GetAll());

        /// <summary>Read one event by ID.</summary>
        /// <param name="id">The positive event ID.</param>
        [HttpGet("{id:int}"), AllowAnonymous]
        [ProducesResponseType(typeof(RaceEvent), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> Get(int id) => Ok(await _events.Get(id));

        /// <summary>Create an event owned by the logged-in Organiser.</summary>
        /// <remarks>Supply name, description, eventDate (ISO 8601 UTC), location, distanceKm, eventType (Run/Walk/Cycle), and optional bannerImageUrl. OrganiserId comes from the session.</remarks>
        [HttpPost, Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(RaceEvent), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        public async Task<IActionResult> Create(EventRequest request)
        {
            var race = await _events.Create(CurrentUser.Id(User), request);
            return CreatedAtAction(nameof(Get), new { id = race.EventId }, race);
        }

        /// <summary>Update an event belonging to the logged-in Organiser.</summary>
        /// <remarks>Supply the complete editable event details, as for creation. Another Organiser's event returns 403.</remarks>
        [HttpPut("{id:int}"), Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(RaceEvent), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> Update(int id, EventRequest request) => Ok(await _events.Update(id, CurrentUser.Id(User), request));

        /// <summary>Delete your event and its categories when it has no enrolments.</summary>
        /// <remarks>No body is required. Returns 409 if deleting would remove participant history.</remarks>
        [HttpDelete("{id:int}"), Authorize(Roles = "Organiser")]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Delete(int id)
        {
            await _events.Delete(id, CurrentUser.Id(User));
            return Ok(new { message = "Event deleted." });
        }

        /// <summary>View every enrolment for one of your events.</summary>
        /// <remarks>Organiser only. Returns participant names, chosen categories, entry dates and statuses. Another Organiser is rejected.</remarks>
        [HttpGet("{eventId:int}/enrolments"), Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(List<EnrolmentResponse>), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        public async Task<IActionResult> GetEnrolments(int eventId) => Ok(await _enrolments.GetForEvent(eventId, CurrentUser.Id(User)));
    }
}
