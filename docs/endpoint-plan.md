# API endpoints

The first 16 routes come from Part 1. [7] Part 2 adds logout, category editing and category deletion. JSON names use camelCase. The routes use GET, POST, PUT and DELETE for the relevant actions. [15] Swagger shows the request fields and validation rules. [16]

Protected routes return 401 if the user is not logged in. They return 403 if the role is wrong or the user does not own the event. Known errors use Problem Details responses. The response-code meanings follow [12], with roles enforced using [18].

| Method | Route | Description | Role | Request body | Expected response |
|---|---|---|---|---|---|
| POST | /api/auth/register | Create an account without logging it in | Public | fullName, email, password, role, phoneNumber | 201 safe profile; 400 invalid input; 409 duplicate email |
| POST | /api/auth/login | Verify credentials and store user ID and role in session | Public | email, password | 200 safe profile and session cookie; 400 invalid input; 401 bad credentials |
| GET | /api/users/profile | Read own profile | Either role | None | 200 safe profile |
| PUT | /api/users/profile | Change own profile fields | Either role | fullName, phoneNumber, optional profilePictureUrl | 200 updated profile; 400 invalid input |
| GET | /api/events | Browse events in date order | Public | None | 200 event list |
| GET | /api/events/{id} | Read event details | Public | None | 200 event; 404 missing |
| POST | /api/events | Create an event owned by the current user | Organiser | name, description, eventDate, location, distanceKm, eventType, optional bannerImageUrl | 201 event with Location header; 400 invalid input |
| PUT | /api/events/{id} | Edit an owned event | Organiser owner | Complete editable event fields | 200 event; 400 invalid input; 404 missing |
| DELETE | /api/events/{id} | Delete an unused event and its categories | Organiser owner | None | 200 message; 404 missing; 409 existing enrolments |
| GET | /api/events/{eventId}/categories | Browse an event's categories and fees | Public | None | 200 category list; 404 missing event |
| POST | /api/events/{eventId}/categories | Add an age or distance category | Organiser owner | categoryName, description, distanceKm, fee | 201 category; 400 invalid input; 404 missing event; 409 duplicate name |
| POST | /api/enrolments | Enter a future event in its category | Participant | eventId, categoryId | 201 joined enrolment; 400 invalid pair/closed entries; 404 missing event/category; 409 duplicate entry |
| GET | /api/enrolments/my-enrolments | Read own entry history | Participant | None | 200 joined enrolment list |
| GET | /api/events/{eventId}/enrolments | View all entries for an owned event | Organiser owner | None | 200 entries with names/categories/status; 404 missing event |
| POST | /api/results | Capture one result for an enrolled participant | Organiser owner | enrolmentId, finishTime, overallPosition, categoryPosition | 201 joined result; 400 invalid time/positions or event not started; 404 missing enrolment; 409 existing result |
| GET | /api/results/my-results | Read own performance history | Participant | None | 200 joined results with times and positions |
| POST | /api/auth/logout | Clear the current server-side session | Either role | None | 204 |
| PUT | /api/events/{eventId}/categories/{categoryId} | Edit an owned category | Organiser owner | Complete editable category fields | 200 category; 400 invalid input; 404 missing; 409 duplicate name |
| DELETE | /api/events/{eventId}/categories/{categoryId} | Delete an unused owned category | Organiser owner | None | 204; 404 missing; 409 existing enrolments |

## Example category body

```json
{
  "categoryName": "Senior 10km",
  "description": "Senior runners entering the 10km route",
  "distanceKm": 10,
  "fee": 120
}
```

## Example result body

```json
{
  "enrolmentId": 4,
  "finishTime": "00:48:00",
  "overallPosition": 2,
  "categoryPosition": 2
}
```

Use the actual returned IDs. Category positions are positive and cannot exceed overall positions. Elapsed times are restricted to less than 24 hours by the original SQL time design. Both Organiser and Participant profile responses exclude PasswordHash; none of the personal history routes accept a target user ID.
