// Runs the API with a separate database for each test.
// References: API test host: [3]. Factory disposal order: [19]. See docs/references.md.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;

namespace RaceDay.Tests
{
    public class ApiFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection? _sqlite;
        private int _databaseCleanupStarted;
        private readonly string? _sqlServer = Environment.GetEnvironmentVariable("RACEDAY_TEST_SQLSERVER");
        private readonly string _databaseName = "RaceDayTests_" + Guid.NewGuid().ToString("N");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<RaceDayContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<RaceDayContext>>();
                if (string.IsNullOrWhiteSpace(_sqlServer))
                {
                    _sqlite = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
                    _sqlite.Open();
                    services.AddDbContext<RaceDayContext>(options => options.UseSqlite(_sqlite));
                }
                else
                {
                    var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_sqlServer) { InitialCatalog = _databaseName };
                    services.AddDbContext<RaceDayContext>(options => options.UseSqlServer(connection.ConnectionString));
                }
            });
        }

        public void Initialise()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
            if (string.IsNullOrWhiteSpace(_sqlServer)) db.Database.EnsureCreated();
            else db.Database.Migrate();
        }

        public void WithDatabase(Action<RaceDayContext> action)
        {
            using var scope = Services.CreateScope();
            action(scope.ServiceProvider.GetRequiredService<RaceDayContext>());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !string.IsNullOrWhiteSpace(_sqlServer)
                && System.Threading.Interlocked.Exchange(ref _databaseCleanupStarted, 1) == 0)
            {
                // Clean up once without using the stopped host's service provider.
                var connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_sqlServer) { InitialCatalog = _databaseName };
                var options = new DbContextOptionsBuilder<RaceDayContext>().UseSqlServer(connection.ConnectionString).Options;
                using var db = new RaceDayContext(options);
                db.Database.EnsureDeleted();
            }
            base.Dispose(disposing);
            if (disposing) _sqlite?.Dispose();
        }
    }
}
