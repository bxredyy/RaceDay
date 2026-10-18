param(
    [string]$ConnectionString = "Server=(localdb)\MSSQLLocalDB;Database=RaceDay;Trusted_Connection=True;TrustServerCertificate=True",
    [switch]$SkipTests
)

# Start-RaceDay.ps1
# Run from PowerShell. Restores, builds, tests, initialises the database and starts Swagger.
# Pass -ConnectionString for a SQL Server instance other than LocalDB.
$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "Install the .NET 10 SDK, then run this script again. See README.md."
}

dotnet restore RaceDay.sln
if ($LASTEXITCODE -ne 0) { throw "Package restore failed." }
dotnet build RaceDay.sln --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { throw "Build failed." }
if (-not $SkipTests) {
    dotnet test RaceDay.sln --configuration Release --no-build
    if ($LASTEXITCODE -ne 0) { throw "Tests failed; check the output before proceeding." }
}

$env:ConnectionStrings__RaceDay = $ConnectionString
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://localhost:5080"
dotnet run --project Part2/RaceDay.Api --configuration Release --no-build --no-launch-profile -- --initialise-database
if ($LASTEXITCODE -ne 0) { throw "Database setup failed. Confirm SQL Server/LocalDB is running and check the connection string." }

Write-Host "Open http://localhost:5080/swagger after the server starts. Press Ctrl+C to stop."
dotnet run --project Part2/RaceDay.Api --configuration Release --no-build --no-launch-profile
if ($LASTEXITCODE -ne 0) { throw "API stopped with an error." }
