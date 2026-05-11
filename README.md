# GoodLife Pulse Tracker

GoodLife Pulse Tracker is a web application for finding GoodLife Fitness clubs in the Calgary area (and later other locations) and checking their estimated crowd level before visiting.

The product focuses on quick club discovery, estimated occupancy visibility, user-submitted crowd reports, and saved favorite locations.

Crowd levels are application estimates based on user reports and system logic. They should not be presented as official GoodLife Fitness capacity data unless an official data integration is added later.

## Product Scope

### Phase 1 Scope

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
- Backend: ASP.NET Core Web API.
- Database: Microsoft SQL Server, with Entity Framework Core migrations.
- Authentication: JWT-based authentication with hashed passwords.
- Deployment target: Azure, with GitHub Actions for CI/CD.

## Documentation

- [Requirements](docs/requirements.md)
- [Architecture](docs/architecture.md)
- [API Design](docs/api-design.md)
- [Database Schema](docs/database_schema.md)
- [Project Roadmap](docs/project_roadmap.md)

## Frontend Development

```bash
cd frontend
npm install
npm run dev
```

The frontend also supports:

```bash
npm start
```

which runs the same Vite development server.
