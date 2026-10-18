// Checks the session and loads the user and role.
// References: Sessions: [1]. Role concepts: [11]. Role checks: [18]. See docs/references.md.

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RaceDay.Api.Data;

namespace RaceDay.Api.Security
{
    public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public const string SchemeName = "RaceDaySession";
        private readonly RaceDayContext _db;

        public SessionAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger, UrlEncoder encoder, RaceDayContext db) : base(options, logger, encoder)
        {
            _db = db;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Context.Session.LoadAsync();
            int? userId = Context.Session.GetInt32("UserId");
            string? role = Context.Session.GetString("Role");
            if (!userId.HasValue || role == null) return AuthenticateResult.NoResult();

            // A removed account or changed role invalidates the old session immediately.
            var user = await _db.Users.AsNoTracking().SingleOrDefaultAsync(item => item.UserId == userId.Value);
            if (user == null || user.Role != role)
            {
                Context.Session.Clear();
                return AuthenticateResult.Fail("The session is no longer valid.");
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName), new Claim(ClaimTypes.Role, user.Role)
            };
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
            return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            return Results.Problem(statusCode: 401, title: "Login required", detail: "Log in to access this endpoint.").ExecuteAsync(Context);
        }

        protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
        {
            return Results.Problem(statusCode: 403, title: "Access denied", detail: "Your role cannot use this endpoint.").ExecuteAsync(Context);
        }
    }
}
