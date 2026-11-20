// Endpoints for event categories.
// References: HTTP methods: [15]. Status codes: [12]. Role checks: [11], [18]. See docs/references.md.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Models;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController, Route("api/events/{eventId:int}/categories"), Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public class CategoriesController : ControllerBase
    {
        private readonly EventService _events;
        public CategoriesController(EventService events) { _events = events; }

        /// <summary>List an event's age or distance categories, including fees in rand.</summary>
        /// <remarks>Public endpoint. No body is required. A missing event returns 404.</remarks>
        [HttpGet, AllowAnonymous]
        [ProducesResponseType(typeof(List<Category>), 200)]
        public async Task<IActionResult> GetAll(int eventId) => Ok(await _events.GetCategories(eventId));

        /// <summary>Add a category to your event.</summary>
        /// <remarks>Organiser only. Supply categoryName, description, positive distanceKm and non-negative fee (ZAR). Names must be unique within an event.</remarks>
        [HttpPost, Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(Category), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Create(int eventId, CategoryRequest request)
        {
            return StatusCode(201, await _events.CreateCategory(eventId, CurrentUser.Id(User), request));
        }

        /// <summary>Update a category belonging to your event.</summary>
        /// <remarks>Organiser only. Send the complete category fields. CategoryId and EventId cannot be reassigned.</remarks>
        [HttpPut("{categoryId:int}"), Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(Category), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Update(int eventId, int categoryId, CategoryRequest request)
        {
            return Ok(await _events.UpdateCategory(eventId, categoryId, CurrentUser.Id(User), request));
        }

        /// <summary>Delete an unused category from your event.</summary>
        /// <remarks>Organiser only. No body is required. Categories with enrolments return 409.</remarks>
        [HttpDelete("{categoryId:int}"), Authorize(Roles = "Organiser")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        [ProducesResponseType(typeof(ProblemDetails), 403)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Delete(int eventId, int categoryId)
        {
            await _events.DeleteCategory(eventId, categoryId, CurrentUser.Id(User));
            return NoContent();
        }
    }
}
