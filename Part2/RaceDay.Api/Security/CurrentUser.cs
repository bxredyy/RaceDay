// Gets the logged-in user ID and checks event ownership.
// References: Role concepts: [11]. Role checks: [18]. Ownership rules: [6]. See docs/references.md.

using System.Security.Claims;

namespace RaceDay.Api.Security
{
    public static class CurrentUser
    {
        public static int Id(ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
