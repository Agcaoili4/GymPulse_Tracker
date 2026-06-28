# Architecture

GymPulse uses a React frontend, an ASP.NET Core Web API, and a SQL Server database. The architecture supports a simple first release while leaving clear extension points for analytics, reviews, notifications, and administration. Live crowd updates are part of the first build rather than a later extension point.

## System Overview

```mermaid
flowchart LR
    User[User Browser] --> Frontend[React + Vite Frontend]
    Frontend --> API[ASP.NET Core Web API]
    API --> Auth[Authentication Service]
    API --> ClubService[Club Service]
    API --> OccupancyService[Occupancy Service]
    API --> FavoriteService[Favorite Service]
    API --> Db[(SQL Server)]
    OsmImporter[OpenStreetMap Importer] --> Db
    Simulator[Occupancy Simulator] --> OccupancyService
    OccupancyService --> SignalR[SignalR Hub]
    SignalR --> Frontend
```

## Frontend

### Current Stack

- React
- Vite
- Tailwind CSS
- TypeScript/TSX

### Frontend Responsibilities

- Render club search and discovery screens.
- Show current estimated crowd status.
- Submit crowd reports through the API.
- Support authentication flows.
- Manage favorite clubs for signed-in users.
- Provide responsive layouts for mobile and desktop.

### Recommended Structure

```text
frontend/
  src/
    components/
    pages/
    services/
    hooks/
    context/
    assets/
```

TypeScript is the frontend standard so component props, API clients, and DTO mappings can be checked before runtime.

## Backend

### Stack

- ASP.NET Core Web API
- C#
- Entity Framework Core
- SQL Server
- JWT authentication

### Backend Responsibilities

- Expose REST endpoints for clubs, occupancy, reports, favorites, authentication, and later reviews.
- Validate incoming requests.
- Enforce authentication and user ownership.
- Calculate or retrieve current occupancy estimates.
- Persist application data through Entity Framework Core.
- Return consistent API responses and status codes.

### Recommended Structure

```text
backend/
  Controllers/
  Services/
  Data/
  Models/
  Dtos/
  Middleware/
  Configuration/
```

## Data Flow

### Club Discovery

1. The frontend requests clubs from `GET /api/clubs`.
2. The API applies search and filter parameters.
3. The API reads gyms from SQL Server and the current crowd level from the occupancy engine.
4. The frontend renders the results with crowd status labels.

### Crowd Reporting

1. An authenticated user submits a report to `POST /api/clubs/{clubId}/reports`.
2. The API validates the club, user token, crowd level, and optional note.
3. The report is stored in `CrowdReports`.
4. The occupancy snapshot for the club is recalculated or updated.
5. In a later real-time phase, SignalR broadcasts the updated status to connected clients.

### Favorites

1. An authenticated user saves a club through `POST /api/favorites`.
2. The API verifies the user and club.
3. The database enforces one favorite per user and club.
4. The frontend can retrieve favorites from `GET /api/favorites`.

## Occupancy And Real-Time Strategy

Current crowd levels come from a simulated occupancy engine. A background service recalculates each gym's level on a schedule from a typical weekly gym pattern plus small variation, holds the latest value in memory, and broadcasts changes over SignalR. The frontend subscribes and updates without a page refresh, and falls back to polling the occupancy endpoint if the connection drops.

Occupancy values sit behind a single source interface, so community reports or a paid foot-traffic API can replace the simulated engine later without changing the API or the frontend.

Real-time updates publish:

- Gym ID
- Current crowd level
- Last updated timestamp
- A flag marking the value as estimated, and its source

## Deployment Direction

- Local database: SQL Server running in Docker.
- Hosting target: free tiers, since this is a portfolio project with no budget.
- Frontend deployment: a free static hosting platform such as Vercel or Netlify.
- Backend deployment: a free application hosting tier. Free SQL Server hosting is limited, so the database host is an open question for the deployment phase.
- CI/CD: GitHub Actions.
- Secrets: environment variables or managed secret storage.

## Cross-Document Alignment

- Requirements define the product behaviors.
- API Design defines the HTTP contract for those behaviors.
- Database Schema defines persistence for the API.
- Project Roadmap defines the build order.
