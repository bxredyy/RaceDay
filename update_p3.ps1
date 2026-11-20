$clientPath = 'C:\RaceDayPoE\RaceDay.MVC\Services\RaceDayApiClient.cs' 
$c = Get-Content $clientPath -Raw 
if ($c.Contains($oldGet)) { $c = $c.Replace($oldGet, $newGet) } else { $c = $c.Replace('GetEventsAsync()', 'GetEventsAsyncSafe()') } 
Set-Content -Path $clientPath -Value $c -Encoding UTF8 
$readmePath = 'C:\RaceDayPoE\README.md' 
$existing = if (Test-Path $readmePath) { Get-Content $readmePath -Raw } else { '' } 
$ytBlock = if ($ytMatch.Success) { "`r`n`r`n## Presentation Video`r`n" + $ytMatch.Value } else { '' } 
$cleaned = if ($ytMatch.Success) { $existing.Replace($ytMatch.Value, '').TrimEnd() } else { $existing.TrimEnd() } 
$p3Lines = @('','## Part 3 - ASP.NET Core 8 MVC, Azure Blob Storage and Docker','- **Web Frontend (MVC):** Built ASP.NET Core 8 MVC (RaceDay.MVC) with Models, Services (RaceDayApiClient, BlobStorageService), Controllers, and Razor Views.','- **Azure Blob Storage:** Integrated Azure.Storage.Blobs for container and blob uploads with public URL generation.','- **REST API Integration:** Connected MVC controllers to the RaceDay REST API backend using IHttpClientFactory and session auth headers.','- **Containerization and CI/CD:** Containerized with multi-stage Dockerfile (raceday-mvc:latest on port 8080) and wired to GitHub Actions on bxredyy/RaceDay.') 
$finalText = $cleaned + ($p3Lines -join "`r`n") + $ytBlock + "`r`n" 
Set-Content -Path $readmePath -Value $finalText -Encoding UTF8 
