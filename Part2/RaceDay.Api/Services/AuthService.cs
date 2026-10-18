// Registers users and checks login details.
// References: PasswordHasher: [5]. Account rules: [6]. Role concepts: [11]. See docs/references.md.

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Contracts;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Services
{
    public class AuthService
    {
        private readonly RaceDayContext _db;
        private readonly IPasswordHasher<User> _hasher;
        private readonly AuditService _audit;

        public AuthService(RaceDayContext db, IPasswordHasher<User> hasher, AuditService audit)
        {
            _db = db;
            _hasher = hasher;
            _audit = audit;
        }

        public async Task<User> Register(RegisterRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();
            if (await _db.Users.AnyAsync(user => user.Email == email))
                throw new ApiException(409, "An account with this email already exists.");

            var user = new User
            {
                FullName = request.FullName.Trim(), Email = email,
                Role = request.Role, PhoneNumber = request.PhoneNumber.Trim()
            };
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            _db.Users.Add(user);

            // The first save gets the identity ID. Both saves belong to one transaction.
            await using var transaction = await _db.Database.BeginTransactionAsync();
            await _db.SaveChangesAsync();
            _audit.Log(user.UserId, "Account registered as " + user.Role + ".");
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
            return user;
        }

        public async Task<User> Login(LoginRequest request)
        {
            string email = request.Email.Trim().ToLowerInvariant();
            var user = await _db.Users.SingleOrDefaultAsync(item => item.Email == email);
            if (user == null || _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
                throw new ApiException(401, "Email or password is incorrect.");

            _audit.Log(user.UserId, "Account logged in.");
            await _db.SaveChangesAsync();
            return user;
        }
    }
}
