# GoodLife Pulse Tracker

GoodLife Pulse Tracker is a web application for finding GoodLife Fitness clubs in the Calgary area (and later other locations) and checking their estimated crowd level before visiting.

The product focuses on quick club discovery, estimated occupancy visibility, user-submitted crowd reports, and saved favorite locations.

Crowd levels are application estimates based on user reports and system logic. They should not be presented as official GoodLife Fitness capacity data unless an official data integration is added later.

## Product Scope

### Phase 1 Scope (IN ROUTE)

- Search and browse GoodLife Fitness clubs in Calgary.
- View club details, location information, and current estimated crowd status.
- Submit simple crowd reports: Empty, Moderate, Busy, or Packed.
- Save favorite clubs after authentication.
- Build a responsive frontend and REST API foundation.

### Later Enhancements

- Reviews and ratings.
- SignalR-powered real-time occupancy updates.
- Occupancy analytics and prediction.
- Push notifications.
- Administrative dashboard.

## Technology Direction

- Frontend: React, Vite, TypeScript, Tailwind CSS.
- Backend: ASP.NET Core Web API (.NET 8).
- Database: Microsoft SQL Server, with Entity Framework Core migrations.
- Authentication: JWT-based authentication with hashed passwords.
- Deployment target: Azure, with GitHub Actions for CI/CD.

## Project Structure

```text
backend/GoodLifePulse.Api/   ASP.NET Core Web API (controllers, services, DTOs, data)
frontend/                    React + Vite + TypeScript client
docs/                        Product, architecture, API, and database docs
website_design/              UI mockups
docker-compose.yml           Local SQL Server for development
```

## Getting Started

Run the stack in three steps: database, backend, then frontend.

### Prerequisites

- .NET 8 SDK
- Node.js 18 or newer
- Docker (for local SQL Server)

### 1. Database

Start SQL Server in Docker from the repository root:

```bash
docker compose up -d
```

This runs SQL Server on `localhost:1433`. The credentials are for local development only and should never be reused in production.

### 2. Backend API

```bash
cd backend/GoodLifePulse.Api
dotnet run
```

The API runs at `http://localhost:5279`. It reads the `Default` connection string from `appsettings.Development.json`, which already points at the Docker database above.

- Swagger UI: `http://localhost:5279/swagger`
- Health check: `http://localhost:5279/health`

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at `http://localhost:5173` and calls the backend at `http://localhost:5279`. Start the API first so club data loads. `npm start` runs the same Vite development server.

## API

Current endpoints:

- `GET /health` returns the API status.
- `GET /api/clubs` returns clubs with search, city and province filters, and pagination.
- `GET /api/clubs/{id}` returns a single club by ID.

See [docs/api-design.md](docs/api-design.md) for the full contract, including planned auth, occupancy, reports, and favorites endpoints.

## Documentation

- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [API Design](docs/api-design.md)
- [Database Schema](docs/database_schema.md)
- [Project Roadmap](docs/project_roadmap.md)
</content>
