# RaceDay - Part 3

## Roles
- **Organiser**: Manage events (including banner upload), categories, enrolments, and race results.
- **Participant**: Browse/filter events, enrol in categories, view enrolments and race results, and update profile.

## Docker Run Command
```bash
docker build -t raceday-mvc:latest .
docker run -d -p 8080:8080 --name raceday-mvc -e ApiSettings__BaseUrl="[http://host.docker.internal:5000/](http://host.docker.internal:5000/)" raceday-mvc:latest
```

## CI/CD Screenshot
<!-- Paste CI/CD green build screenshot here -->

## Video Presentation
- Unlisted YouTube Link: https://youtu.be/YOUR_VIDEO_ID
