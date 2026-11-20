# Video plan

Record the running API and explain it in your own words. Use your own voice, as required by the brief. Upload the video as unlisted on YouTube and add the link to the README.

The walkthrough uses Swagger to send requests and compare the responses with the expected behaviour. [16] The 401, 403 and 409 responses are explained using the HTTP status meanings in [12].

## Preparation

Run the build and tests, initialise SQL Server with the included seed and open Swagger. Keep Visual Studio, SSMS and the actual GitHub Actions run ready. Use the demo accounts in the README, and note the IDs returned by your API rather than assuming fixed values. Start with a fresh demo database only if you intentionally want to remove your own old demo data; the provided setup does not delete it for you.

## What to show

1. Introduce RaceDay, the two roles and the Part 2 scope. Show the project folder structure and Part 1 alignment notes, including why event distance was added.
2. Show the ERD and SSMS tables. Explain Users -> Events, Events -> Categories, Users -> Enrolments and the one-result-per-enrolment rule. Show migrations and the matching SQL script.
3. Run the tests and show the final pass count. Explain that the browser/session tests send real HTTP requests and that the CI has an additional SQL Server job.
4. Open Swagger while logged out. Browse GET events, GET one event and GET categories. Attempt GET profile and show 401.
5. Register a Participant with your own fictional demonstration details. Show the safe response, and show that the database saves a password hash rather than the entered password. Attempt duplicate registration and a wrong password to show the failures.
6. Log in as Organiser 1. Show GET/PUT profile. Create an event with all six required event fields; update it. Create, list, update and delete an unused category. Add a category for the next entry demonstration. Create/delete a separate unused event to show event deletion without losing the main demonstration event.
7. Log in as Organiser 2. Try to edit Organiser 1's event or read its enrolments and show 403. Explain ownership versus role checking.
8. Log in as a Participant. Enter Organiser 1's future event and chosen category. Show the linked enrolment in SSMS and in GET my-enrolments. Attempt the same entry again to show 409. Attempt POST events to show 403.
9. Log in as a second Participant. Show that their own enrolments do not contain the first participant's entry.
10. Log in as Organiser 1. View all entries for the future event. Attempt to delete that event and show the 409 that preserves participant history.
11. Use the seeded completed heritage walk for results. Read that event's enrolments, find Participant 2's entry without a result, and capture `00:48:00`, overall position 2, category position 2. Show that recording a future event's result fails.
12. Log in as Participant 2 and show GET my-results with the finish time and positions. Log out, retry GET profile and show 401.
13. Open the main code files. Explain Program middleware order, SessionAuthenticationHandler, the role attributes, RequireOwner, request validation, EF configuration and the tests covering them.
14. Show the real GitHub commit history and both green workflow jobs. Explain the workflow's structure validation, restore, build and test steps. Show the README evidence links.

## Questions to prepare for

- Why is the role read from server-side session state instead of a request body?
- What does 401 mean compared with 403?
- Why can one Organiser not edit another Organiser's event?
- What prevents a Participant selecting a category belonging to another event?
- What happens when two duplicate entry requests arrive together?
- How does the password hash differ from plaintext or reversible encryption?
- Why are personal responses separate classes from User?
- Why use both SQLite tests and SQL Server tests?
- How do Code First migrations keep the database schema reproducible?
- Which documents changed from Part 1, and why?

Do a final check that every operation listed in docs/endpoint-plan.md has been demonstrated or explained, the on-screen test run is real, your voice is audible and the YouTube link opens for someone who is not signed into your account.
