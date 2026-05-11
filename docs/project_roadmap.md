# Project Roadmap

This roadmap defines the implementation path for GoodLife Pulse Tracker. It keeps documentation, product scope, backend services, database design, frontend work, and deployment aligned across phases.

## Product Direction

GoodLife Pulse Tracker helps users find GoodLife Fitness clubs in the Calgary area and check estimated crowd levels before visiting.

The first release focuses on:

- Calgary club discovery.
- Current crowd status visibility.
- User-submitted crowd reports.
- Authenticated favorites.
- A stable API and database foundation.

Reviews, analytics, prediction, push notifications, and admin tooling are later enhancements.

## Phase 1: Product Documentation And Planning

Status: In progress

### Objectives

- Define the product scope.
- Align functional and non-functional requirements.
- Establish the architecture, API contract, and database model.
- Confirm the technology stack and implementation order.

### Deliverables

- Requirements document.
- Architecture document.
- API design document.
- Database schema document.
- Project roadmap.
- Initial React, Vite, TypeScript, and Tailwind CSS frontend scaffold.

### Exit Criteria

- Documents use consistent product names, features, entities, and phases.
- First-release features are separated from future enhancements.
- Backend and database implementation can begin without unresolved product ambiguity.

## Phase 2: UX/UI Design And User Flows

### Objectives

- Design the main user experience before deeper frontend implementation.
- Keep the interface optimized for quick mobile use.

### Deliverables

- User flow diagrams.
- Wireframes for club discovery, club detail, report submission, login, signup, and favorites.
- Responsive layout plan.
- Design tokens for color, spacing, typography, and crowd-status states.
- Accessibility notes for navigation, forms, and status labels.

### Exit Criteria

- Core screens are designed for mobile and desktop.
- Crowd levels are visually distinct and accessible.
- Empty, loading, and error states are defined.

## Phase 3: Backend Foundation

### Objectives

- Build the ASP.NET Core Web API foundation.
- Prepare the backend for authentication, persistence, and feature modules.

### Deliverables

- ASP.NET Core Web API project.
- Configuration management.
- Controller, service, DTO, and data-layer structure.
- Global error handling.
- Request validation approach.
- Swagger/OpenAPI setup.

### Exit Criteria

- API can run locally.
- Health endpoint is available.
- Project structure matches the architecture document.

## Phase 4: Database Integration

### Objectives

- Implement SQL Server persistence using Entity Framework Core.
- Support the first-release data model.

### Deliverables

- SQL Server Docker setup.
- Entity Framework Core models and DbContext.
- Initial migration.
- Seed data for Calgary clubs, amenities, and occupancy snapshots.
- Indexes and constraints from the database schema.

### Exit Criteria

- Local database starts consistently.
- Migrations apply from a clean database.
- Seed data supports frontend and API development.

## Phase 5: Core API Features

### Objectives

- Implement first-release API endpoints.
- Make the backend usable by the frontend.

### Deliverables

- Auth endpoints: register, login, current user.
- Club endpoints: list, search, detail.
- Occupancy endpoint: current club occupancy.
- Crowd report endpoints: create and list recent reports.
- Favorite endpoints: list, add, remove.
- Consistent error responses and status codes.

### Exit Criteria

- API behavior matches `docs/api-design.md`.
- Protected endpoints require JWT authentication.
- Basic integration tests cover the main success and failure paths.

## Phase 6: Frontend Implementation

### Objectives

- Build the responsive React frontend and connect it to the API.

### Deliverables

- Club search and list experience.
- Club detail screen.
- Crowd status display.
- Crowd report form.
- Login and signup screens.
- Favorites view and favorite toggle.
- API service layer.
- Loading, empty, and error states.

### Exit Criteria

- Frontend can run locally with `npm run dev` or `npm start`.
- Frontend production builds run TypeScript checks before Vite builds.
- Users can complete the first-release workflows through the UI.
- The UI is responsive and keyboard accessible for core flows.

## Phase 7: Real-Time Occupancy

### Objectives

- Improve occupancy freshness after the core API is stable.

### Deliverables

- SignalR hub for occupancy updates.
- Backend event flow after new crowd reports.
- Frontend subscription to occupancy updates.
- Fallback polling strategy.

### Exit Criteria

- Crowd updates appear in connected clients without a full page refresh.
- Polling fallback still works if the real-time connection fails.

## Phase 8: Testing, DevOps, And Deployment

### Objectives

- Prepare the application for reliable deployment.

### Deliverables

- Unit tests for services and validation.
- Integration tests for API endpoints.
- Frontend tests for critical UI flows.
- GitHub Actions CI workflow.
- Environment configuration for local and production.
- Azure deployment plan.
- Logging and monitoring setup.

### Exit Criteria

- CI runs build and tests.
- Deployment process is documented.
- Production secrets are not stored in source control.

## Phase 9: Future Enhancements

Potential enhancements after the first production slice:

- User reviews and ratings.
- Occupancy analytics.
- AI occupancy prediction.
- Push notifications.
- Club recommendation system.
- Rate limiting and API throttling.
- Administrative dashboard.
- Moderation workflow for user-submitted content.
