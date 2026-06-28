# Requirements

GymPulse helps users choose when and where to work out by showing gyms across Calgary and the surrounding area, across many brands, with estimated crowd levels.

This document defines the product requirements that the roadmap, architecture, API design, and database schema must support.

## Product Goals

- Help users quickly find a nearby gym.
- Make crowd levels easy to understand at a glance.
- Let users contribute simple crowd reports.
- Allow authenticated users to save favorite clubs.
- Keep the system modular enough for real-time updates, reviews, analytics, and notifications in later phases.

## Product Assumptions

- First release focuses on gyms across Calgary and nearby areas, across many brands.
- Additional cities can be added later without changing the core data model.
- Gym records are imported from OpenStreetMap, which is free and openly licensed, rather than from a paid data provider.
- Crowd levels are estimates produced by a simulated occupancy engine now, and by user reports in a later stage.
- Crowd levels are not official capacity data from any gym. The app labels them as estimates.

## Core Users

- Guest users who want to browse clubs and check crowd levels.
- Authenticated users who want to submit reports and save favorites.
- Future administrators who may manage club data, reports, and moderation workflows.

## Functional Requirements

### Club Discovery

- Users can browse gyms across Calgary and nearby areas.
- Users can search clubs by name, address, neighborhood, or city.
- Users can filter clubs by location attributes such as city, province, and amenities when available.
- Users can view key club information, including name, address, contact details, coordinates, amenities, and current crowd status.

### Occupancy Monitoring

- Users can view a current estimated crowd level for each club.
- Supported crowd levels are Empty, Moderate, Busy, and Packed.
- Occupancy data should show when it was last updated.
- In the current build, occupancy comes from a simulated engine that updates on a schedule. User reports will feed the estimate in a later stage.
- The target refresh interval for visible occupancy data is under 30 seconds, delivered through the real-time updates in this build.

### Crowd Reporting

- Authenticated users can submit crowd reports for a club.
- Each report records the club, user, crowd level, optional note, and timestamp.
- Reports should be validated before storage.
- The API should prevent obviously abusive submission patterns through validation first and rate limiting in a later phase.

### Favorites

- Authenticated users can save clubs as favorites.
- Authenticated users can remove clubs from favorites.
- Authenticated users can view their favorite clubs with current crowd status.
- A user cannot favorite the same club more than once.

### Authentication

- Users can register with email and password.
- Users can sign in and receive a JWT.
- Passwords must be hashed before storage.
- Protected routes require a valid JWT.

### Reviews And Ratings

- Reviews and ratings are not part of the first release.
- Later phases may allow authenticated users to submit ratings, comments, and review history for clubs.

## Non-Functional Requirements

### Performance

- API responses should complete in under 500 ms for common read endpoints under normal load.
- Club search and favorite lookups should use indexed fields.
- Occupancy reads should avoid expensive aggregation on every request when traffic grows.

### Security

- Passwords must never be stored in plain text.
- JWT signing secrets must be stored outside source control.
- API inputs must be validated.
- User-scoped operations must verify ownership.
- Future production deployment should include rate limiting and structured audit logging for sensitive actions.

### Reliability

- The API should return consistent error responses.
- The database schema should enforce required relationships with foreign keys.
- Seed data should allow the app to run in local development without manual setup.

### Usability

- The frontend must be responsive for mobile and desktop.
- Crowd status should be visually scannable.
- Navigation should be accessible by keyboard.
- Empty, loading, and error states should be clear and recoverable.

### Maintainability

- Backend code should be organized by responsibility: controllers, services, data access, models, and DTOs.
- Frontend code should use TypeScript, reusable components, and API service modules.
- Documentation must stay aligned across product scope, API contracts, and database design.
