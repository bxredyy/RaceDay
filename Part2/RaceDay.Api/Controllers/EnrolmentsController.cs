// Endpoints for entering an event and viewing entries.
// References: HTTP methods: [15]. Status codes: [12]. Role checks: [11], [18]. See docs/references.md.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController, Route("api/enrolments"), Authorize(Roles = "Participant"), Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    [ProducesResponseType(typeof(ProblemDetails), 403)]
    public class EnrolmentsController : ControllerBase
    {
        private readonly EnrolmentService _enrolments;
        public EnrolmentsController(EnrolmentService enrolments) { _enrolments = enrolments; }

        /// <summary>Enter an upcoming event in one of its categories.</summary>
        /// <remarks>Supply eventId and categoryId. The Participant comes from the session. Duplicate entries return 409; mismatched categories or closed entries return 400.</remarks>
        [HttpPost]
        [ProducesResponseType(typeof(EnrolmentResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 404)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Enter(EnrolmentRequest request) => StatusCode(201, await _enrolments.Enter(CurrentUser.Id(User), request));

        /// <summary>Read only the logged-in Participant's enrolments.</summary>
        /// <remarks>No body or participant ID is accepted. Returns an empty list when you have not entered any events.</remarks>
        [HttpGet("my-enrolments")]
        [ProducesResponseType(typeof(List<EnrolmentResponse>), 200)]
        public async Task<IActionResult> GetMine() => Ok(await _enrolments.GetMine(CurrentUser.Id(User)));
    }
}
