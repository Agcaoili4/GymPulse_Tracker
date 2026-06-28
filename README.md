# GymPulse

GymPulse is a web application for finding gyms across Calgary and the surrounding area and checking their estimated crowd level before heading out. It covers many gym brands rather than a single chain.

The product focuses on quick gym discovery, estimated occupancy visibility, and saved favorite gyms.

Crowd levels are estimates, not official capacity numbers from any gym. In the current build the live crowd levels are produced by a simulated occupancy engine and are labeled as such. The system is built so a real source, such as community reports or a paid foot-traffic API, can replace the simulated source later without changing the rest of the app.

## Product Scope

### Current Build

- Search and browse gyms across Calgary and nearby areas, across brands.
- View gym details, location information, and current estimated crowd status.
- Watch crowd levels update live through a simulated occupancy engine and SignalR.
- Save favorite gyms.
- Build a responsive frontend and REST API foundation.

### Later Enhancements

- User accounts and sign-in.
- User-submitted crowd reports that feed the live crowd level.
- Reviews and ratings.
- Occupancy analytics and prediction.
- Push notifications.
- Administrative dashboard.

## Technology Direction

- Frontend: React, Vite, TypeScript, Tailwind CSS.
- Backend: ASP.NET Core Web API (.NET 8).
- Database: Microsoft SQL Server, with Entity Framework Core migrations.
- Gym directory data: OpenStreetMap (Overpass API), imported once into the database.
- Real-time updates: SignalR.
- Authentication (later): JWT-based authentication with hashed passwords.
- Deployment target: free hosting tiers, with GitHub Actions for CI/CD.

## Project Structure

```text
backend/GymPulse.Api/   ASP.NET Core Web API (controllers, services, DTOs, data)
frontend/               React + Vite + TypeScript client
docs/                   Product, architecture, API, and database docs
website_design/         UI mockups
docker-compose.yml      Local SQL Server for development
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
cd backend/GymPulse.Api
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

The frontend runs at `http://localhost:5173` and calls the backend at `http://localhost:5279`. Start the API first so gym data loads. `npm start` runs the same Vite development server.

## API

Current endpoints:

- `GET /health` returns the API status.
- `GET /api/clubs` returns gyms with search, city and province filters, and pagination.
- `GET /api/clubs/{id}` returns a single gym by ID.

See [docs/api-design.md](docs/api-design.md) for the full contract, including occupancy and real-time updates, and the planned auth, reports, and favorites endpoints.

## Documentation

- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [API Design](docs/api-design.md)
- [Database Schema](docs/database_schema.md)
- [Project Roadmap](docs/project_roadmap.md)
