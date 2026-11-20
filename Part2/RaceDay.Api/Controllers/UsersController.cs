// Endpoints for the logged-in user profile.
// References: HTTP methods: [15]. Status codes: [12]. Role checks: [11], [18]. See docs/references.md.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController, Route("api/users"), Authorize, Produces("application/json")]
    [ProducesResponseType(typeof(ProblemDetails), 401)]
    public class UsersController : ControllerBase
    {
        private readonly ProfileService _profiles;
        public UsersController(ProfileService profiles) { _profiles = profiles; }

        /// <summary>Read the logged-in user's profile without the password hash.</summary>
        /// <remarks>Requires a session for either role. No body or user ID is accepted.</remarks>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ProfileResponse), 200)]
        public async Task<IActionResult> GetProfile() => Ok(await _profiles.Get(CurrentUser.Id(User)));

        /// <summary>Update your full name, phone number and optional profile image URL.</summary>
        /// <remarks>Email, role and user ID cannot be changed through this endpoint.</remarks>
        [HttpPut("profile")]
        [ProducesResponseType(typeof(ProfileResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        public async Task<IActionResult> UpdateProfile(ProfileRequest request) => Ok(await _profiles.Update(CurrentUser.Id(User), request));
    }
}
