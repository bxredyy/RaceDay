// Checks the demo password hashes in the SQL script.
// References: Password hashing: [5]. Test assertions: [13]. See docs/references.md.

using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using RaceDay.Api.Data;
using RaceDay.Api.Models;
using Xunit;

namespace RaceDay.Tests
{
    public class SqlSeedTests
    {
        [Fact]
        public void Sql_script_demo_hashes_match_the_documented_password_and_use_distinct_salts()
        {
            var folder = new DirectoryInfo(AppContext.BaseDirectory);
            while (folder != null && !File.Exists(Path.Combine(folder.FullName, "RaceDay.sln"))) folder = folder.Parent;
            Assert.NotNull(folder);
            string sql = File.ReadAllText(Path.Combine(folder!.FullName, "docs", "RaceDay.sql"));
            var hashes = Regex.Matches(sql, @"N'(AQAAAA[^']+)'", RegexOptions.CultureInvariant)
                .Select(match => match.Groups[1].Value).ToList();
            Assert.Equal(4, hashes.Count);
            Assert.Equal(4, hashes.Distinct().Count());
            var hasher = new PasswordHasher<User>();
            foreach (var hash in hashes)
                Assert.NotEqual(PasswordVerificationResult.Failed, hasher.VerifyHashedPassword(new User(), hash, SeedData.DemoPassword));
        }
    }
}
