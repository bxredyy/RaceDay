# Changes from Part 1

Date: 17 October 2026

The Part 1 plan is in `progpart1.docx`, dated September 2026. Part 2 keeps its six entities and 16 routes. `original-part1-erd.png` is the original diagram and `erd.png` shows the database used by the API.

The table explains changes from the Part 1 plan [7] while meeting the Part 2 brief [6]. Database relationships use primary and foreign keys to keep linked records consistent. [10] These changes still need to be compared with any feedback on Part 1.

| Item | Original plan | Implementation and reason |
|---|---|---|
| Event distance | Categories contain DistanceKm; Events does not | Adds Events.DistanceKm, decimal(8,2), because the current brief explicitly requires distance on each event |
| Endpoint descriptions | The original table has method, route, role, request and response | Adds the missing Description column and documents success/failure statuses |
| Logout | Not planned | Adds POST /api/auth/logout so a real session can end and the demonstration can switch roles safely |
| Category changes | Only list and create planned | Adds PUT and DELETE /api/events/{eventId}/categories/{categoryId} for the owner's category management |
| Enrolment consistency | Separate EventId and CategoryId | Retains both fields and adds a composite foreign key to prevent cross-event category assignments |
| Duplicate enrolments | No explicit unique constraint visible | Adds a unique ParticipantId/EventId index: one participant entry per event |
| Duplicate results | Diagram shows enrolment/result one-to-one | Enforces it through a unique Results.EnrolmentId foreign key |
| Duplicate category names | Not specified | Names are unique per event; the same category name can be used for different events |
| Timestamps | Diagram uses datetime | Uses datetime2 for timestamp fields with UTC application values; FinishTime remains time |
| Field lengths and precision | ERD names nvarchar and decimal without sizes | Specifies lengths and numeric precision in EF configuration and the generated SQL script |
| AuditLogs | In the original model but no endpoint planned | Adds audit rows for successful operations; does not publish a public audit-history endpoint |
| SQL file | No original .sql attachment supplied | Reconstructs docs/RaceDay.sql from the EF migration and adds seeded sample data. Exact alignment with an unseen original script remains unverified |

Public event/category reads remain public, exactly as planned. Personal profile, enrolment and result history require a session. Role selection follows the brief; registration cannot accept any role beyond Organiser or Participant. User IDs and event ownership always come from the server-side identity.

The GitHub repository was created after the local API was built. Its history does not prove when the original Part 1 planning was done.
