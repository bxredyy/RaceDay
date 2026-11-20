// Registration, login and logout endpoints.
// References: HTTP methods: [15]. Status codes: [12]. Role checks: [11], [18]. See docs/references.md.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Api.Contracts;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

namespace RaceDay.Api.Controllers
{
    [ApiController, Route("api/auth"), Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _auth;
        public AuthController(AuthService auth) { _auth = auth; }

        /// <summary>Register an Organiser or Participant account.</summary>
        /// <remarks>Supply fullName, email, a 12–128 character password, role and phoneNumber. Registration does not log you in. Both roles can self-register for this coursework system.</remarks>
        [HttpPost("register"), AllowAnonymous]
        [ProducesResponseType(typeof(ProfileResponse), 201)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 409)]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var user = await _auth.Register(request);
            return StatusCode(201, ProfileResponse.From(user));
        }

        /// <summary>Log in and create a server-side session containing user ID and role.</summary>
        /// <remarks>Send email and password. The browser stores an HttpOnly session cookie. Keep using the same Swagger browser tab for protected requests.</remarks>
        [HttpPost("login"), AllowAnonymous]
        [ProducesResponseType(typeof(ProfileResponse), 200)]
        [ProducesResponseType(typeof(ValidationProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _auth.Login(request);
            HttpContext.Session.Clear();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Role", user.Role);
            return Ok(ProfileResponse.From(user));
        }

        /// <summary>End the current session so subsequent protected requests are rejected.</summary>
        /// <remarks>Either authenticated role may log out. No body is required.</remarks>
        [HttpPost("logout"), Authorize]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 401)]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("RaceDay.Session");
            return NoContent();
        }
    }
}
