// Endpoints for saving results and viewing personal results.
// References: HTTP methods: [15]. Status codes: [12]. Role checks: [11], [18]. See docs/references.md.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController, Route("api/results"), Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    public class ResultsController : ControllerBase
    {
        private readonly ResultService _results;
        public ResultsController(ResultService results) { _results = results; }

        /// <summary>Capture a finish time and positions for an enrolment in your event.</summary>
        /// <remarks>Organiser only. Supply enrolmentId, finishTime in HH:mm:ss (00:00:01 to 23:59:59), overallPosition and categoryPosition. The event must have started; each enrolment has at most one result.</remarks>
        [HttpPost, Authorize(Roles = "Organiser")]
        [ProducesResponseType(typeof(ResultResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Capture(ResultRequest request) => StatusCode(201, await _results.Capture(CurrentUser.Id(User), request));

        /// <summary>Read the logged-in Participant's personal results.</summary>
        /// <remarks>Participant only. Returns finish times, positions and event/category names. No body or participant ID is accepted.</remarks>
        [HttpGet("my-results"), Authorize(Roles = "Participant")]
        [ProducesResponseType(typeof(List<ResultResponse>), 200)]
        public async Task<IActionResult> GetMine() => Ok(await _results.GetMine(CurrentUser.Id(User)));
    }
}
