# Test notes

## Windows testing

On 17 October 2026, the startup script passed 74 SQLite tests. It then created the RaceDay database on SQL Server LocalDB, applied the migration and added demo data. Swagger returned the saved event data.

The first SQL Server test run failed during test cleanup. `ApiFactory` was trying to use the service provider after it had stopped. Cleanup now runs once and uses a separate database context.

After that fix, the Windows SQL Server run passed 74 tests, with no failures or skipped tests. The terminal screenshot showed a duration of 47.9 seconds. The test results file was saved under `TestResults/SqlServer/sqlserver-tests.trx`. A copy can be kept in `docs/evidence/windows-sqlserver-tests.trx`.

The SQL Server run used the existing 74-test version. It does not cover the four newer decimal validation cases.

## Test approach

Test cases prepare inputs, run an action and check the result. This follows arrange, act and assert, with normal, invalid and boundary cases. [13] The HTTP/database tests are integration tests and use the Microsoft test-host approach. [3]

## Earlier checks

The updated download has 78 tests, including four decimal validation cases for en-US, en-ZA, af-ZA and de-DE. Before the SQL cleanup change, all 78 passed on Linux with both default and af-ZA settings. The reports in `docs/evidence` are from those runs.

Other completed checks were the Release build, migration model comparison, Swagger publication and SQL script generation. The tests check all 19 Swagger operations and the four demo password hashes.

The decimal validation fix was needed because decimal-comma settings caused range validation to fail. The range limits now use invariant culture using the options described in [17].

## Coverage

The saved coverage report has an overall line rate of 40.28%. This includes generated migration code, which SQLite tests do not run. Counting application code without the generated migrations gives 558 of 579 lines, or 96.37%. These are coverage figures, not assignment marks.

## Still to do

- Run the updated 78-test version on Windows if the newer tests are added.
- Get both GitHub Actions jobs passing. The SQL Server job also runs the SQL script twice.
- Add the real passing build screenshot to the README.
- Finish the commit history and record the video with your own voice.
- Confirm that the repository used for submission is accepted by the lecturer and that the marker can access it.

The original Part 1 SQL script was not available. The included SQL script was made from the EF migration.
