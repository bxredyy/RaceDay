// Sets up the API, database, sessions and Swagger.
// References: Session setup: [1]. Swagger setup: [4]. App structure: [14]. See docs/references.md.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RaceDay.Api.Data;
using RaceDay.Api.Middleware;
using RaceDay.Api.Models;
using RaceDay.Api.Security;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.ConsumesAttribute("application/json")));
builder.Services.AddDbContext<RaceDayContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.Name = "RaceDay.Session";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() || builder.Environment.IsEnvironment("Testing")
        ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddAuthentication(SessionAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, SessionAuthenticationHandler>(SessionAuthenticationHandler.SchemeName, _ => { });
builder.Services.AddAuthorization();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<EventService>();
builder.Services.AddScoped<EnrolmentService>();
builder.Services.AddScoped<ResultService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "RaceDay API", Version = "v1",
        Description = "PROG6212 Part 2. Register and log in here; Swagger automatically sends the same-origin session cookie. Use logout before changing accounts. Dates use UTC; fees use ZAR."
    });
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "RaceDay.Api.xml"));
});

var app = builder.Build();
app.UseMiddleware<ErrorMiddleware>();

// JSON-only writes plus a same-origin check protect cookie-authenticated browser calls.
app.Use(async (context, next) =>
{
    string? origin = context.Request.Headers.Origin.FirstOrDefault();
    bool writing = !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method);
    if (writing && origin != null && origin != context.Request.Scheme + "://" + context.Request.Host)
    {
        await Results.Problem(statusCode: 403, title: "Cross-origin write rejected").ExecuteAsync(context);
        return;
    }
    await next();
});
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

// This switch is explicit: normal startup never silently creates or changes a database.
if (args.Contains("--initialise-database"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<RaceDayContext>();
    await db.Database.MigrateAsync();
    await SeedData.Initialise(scope.ServiceProvider);
    return;
}

app.Run();

// WebApplicationFactory needs access to this entry point for real HTTP tests.
public partial class Program { }
