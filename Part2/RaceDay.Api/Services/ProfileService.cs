// Reads and updates the current user profile.
// References: Profile requirements: [6], [7]. See docs/references.md.

using RaceDay.Api.Contracts;
using RaceDay.Api.Data;

namespace RaceDay.Api.Services
{
    public class ProfileService
    {
        private readonly RaceDayContext _db;
        private readonly AuditService _audit;
        public ProfileService(RaceDayContext db, AuditService audit) { _db = db; _audit = audit; }

        public async Task<ProfileResponse> Get(int userId)
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new ApiException(404, "Account not found.");
            return ProfileResponse.From(user);
        }

        public async Task<ProfileResponse> Update(int userId, ProfileRequest request)
        {
            var user = await _db.Users.FindAsync(userId) ?? throw new ApiException(404, "Account not found.");
            user.FullName = request.FullName.Trim();
            user.PhoneNumber = request.PhoneNumber.Trim();
            user.ProfilePictureUrl = request.ProfilePictureUrl;
            _audit.Log(userId, "Profile updated.");
            await _db.SaveChangesAsync();
            return ProfileResponse.From(user);
        }
    }
}
