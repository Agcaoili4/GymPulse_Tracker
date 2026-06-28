# API Design

The GymPulse API is an ASP.NET Core REST API used by the React frontend. It provides gym discovery, occupancy visibility, crowd reporting, authentication, and favorites.

## API Principles

- Use JSON for request and response bodies.
- Use resource-oriented routes under `/api`, except for the health check.
- Use JWT bearer authentication for protected endpoints.
- Return consistent validation and error responses.
- Keep first-release endpoints focused on clubs, occupancy, reports, and favorites.

## Base URL

Local development:

```text
https://localhost:{port}/api
```

Production:

```text
https://{api-domain}/api
```

## Authentication

Protected endpoints require:

```http
Authorization: Bearer {token}
```

## Shared Types

### CrowdLevel

```text
Empty
Moderate
Busy
Packed
```

### Standard Error Response

```json
{
  "message": "Validation failed.",
  "errors": {
    "email": ["Email is required."]
  }
}
```

For unexpected errors, the API should return a safe message without leaking internal exception details.

## Endpoints

### Health

#### GET `/health`

Checks whether the API is running.

Success response:

```json
{
  "status": "Healthy"
}
```

### Auth

#### POST `/api/auth/register`

Creates a user account.

Request:

```json
{
  "displayName": "Alex",
  "email": "alex@example.com",
  "password": "StrongPassword123!"
}
```

Success response:

```json
{
  "user": {
    "id": 1,
    "displayName": "Alex",
    "email": "alex@example.com"
  },
  "token": "jwt-token"
}
```

#### POST `/api/auth/login`

Authenticates an existing user.

Request:

```json
{
  "email": "alex@example.com",
  "password": "StrongPassword123!"
}
```

Success response:

```json
{
  "user": {
    "id": 1,
    "displayName": "Alex",
    "email": "alex@example.com"
  },
  "token": "jwt-token"
}
```

#### GET `/api/auth/me`

Returns the currently authenticated user.

Authentication: required.

Success response:

```json
{
  "id": 1,
  "displayName": "Alex",
  "email": "alex@example.com"
}
```

### Clubs

#### GET `/api/clubs`

Returns clubs with current occupancy information.

Query parameters:

- `search`: optional text search by name, address line 1, or city.
- `brand`: optional brand filter, for example `Anytime Fitness`.
- `city`: optional city filter.
- `province`: optional province filter.
- `amenity`: optional amenity filter. Planned for a later phase.
- `page`: optional page number, default `1`.
- `pageSize`: optional page size, default `20`, capped at `100`.

Success response:

```json
{
  "items": [
    {
      "id": 1,
      "name": "Anytime Fitness Beltline",
      "brand": "Anytime Fitness",
      "addressLine1": "140 8 Ave SW",
      "addressLine2": null,
      "city": "Calgary",
      "province": "AB",
      "postalCode": "T2P 1B3",
      "phoneNumber": "(587) 538-1900",
      "latitude": 51.0451,
      "longitude": -114.0659,
      "occupancy": {
        "crowdLevel": "Moderate",
        "percent": 55,
        "isEstimated": true,
        "source": "simulated",
        "lastUpdatedAt": "2026-06-27T18:30:00Z"
      }
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

The `occupancy` block is included now and is served by the simulated occupancy engine. The `isEstimated` and `source` fields make clear the value is an estimate, not official capacity data.

Field planned for a later phase (non-breaking addition):

- `amenities: string[]`: added with the amenities feature.

#### GET `/api/clubs/{clubId}`

Returns one club by ID.

Success response:

```json
{
  "id": 1,
  "name": "Anytime Fitness Beltline",
  "brand": "Anytime Fitness",
  "addressLine1": "140 8 Ave SW",
  "addressLine2": null,
  "city": "Calgary",
  "province": "AB",
  "postalCode": "T2P 1B3",
  "phoneNumber": "(587) 538-1900",
  "latitude": 51.0451,
  "longitude": -114.0659,
  "occupancy": {
    "crowdLevel": "Moderate",
    "percent": 55,
    "isEstimated": true,
    "source": "simulated",
    "lastUpdatedAt": "2026-06-27T18:30:00Z"
  }
}
```

The `occupancy` block is included as shown above. The `amenities` array is planned for a later phase.

Inactive clubs (`IsActive = false`) are excluded from both list and detail responses.

### Occupancy

#### GET `/api/clubs/{clubId}/occupancy`

Returns the current occupancy estimate for a club. Used by the detail view and as the polling fallback for real-time updates.

Success response:

```json
{
  "clubId": 1,
  "crowdLevel": "Moderate",
  "percent": 55,
  "isEstimated": true,
  "source": "simulated",
  "lastUpdatedAt": "2026-06-27T18:30:00Z"
}
```

### Real-Time Updates

Live occupancy is delivered over a SignalR hub at `/hubs/occupancy`. When the simulated engine changes a gym's level, it broadcasts an `occupancyUpdated` event carrying the gym ID, crowd level, percent, estimated flag, source, and timestamp. Clients that cannot hold a socket fall back to polling `GET /api/clubs/{clubId}/occupancy`.

### Crowd Reports

#### POST `/api/clubs/{clubId}/reports`

Creates a crowd report for a club.

Authentication: required.

Request:

```json
{
  "crowdLevel": "Busy",
  "note": "Cardio area is full, but weights are available."
}
```

Success response:

```json
{
  "id": 101,
  "clubId": 1,
  "crowdLevel": "Busy",
  "note": "Cardio area is full, but weights are available.",
  "reportedAt": "2026-05-10T18:42:00Z"
}
```

#### GET `/api/clubs/{clubId}/reports`

Returns recent crowd reports for a club.

Authentication: optional for first release, but notes may be hidden from guests if moderation is not implemented.

Query parameters:

- `limit`: optional number of reports, default `20`.

Success response:

```json
{
  "items": [
    {
      "id": 101,
      "clubId": 1,
      "crowdLevel": "Busy",
      "reportedAt": "2026-05-10T18:42:00Z"
    }
  ]
}
```

### Favorites

#### GET `/api/favorites`

Returns the authenticated user's favorite clubs.

Authentication: required.

Success response:

```json
{
  "items": [
    {
      "id": 1,
      "clubId": 1,
      "clubName": "Anytime Fitness Beltline",
      "city": "Calgary",
      "province": "AB",
      "occupancy": {
        "crowdLevel": "Moderate",
        "lastUpdatedAt": "2026-05-10T18:30:00Z"
      }
    }
  ]
}
```

#### POST `/api/favorites`

Adds a club to the authenticated user's favorites.

Authentication: required.

Request:

```json
{
  "clubId": 1
}
```

Success response:

```json
{
  "id": 1,
  "clubId": 1,
  "createdAt": "2026-05-10T18:45:00Z"
}
```

#### DELETE `/api/favorites/{clubId}`

Removes a club from the authenticated user's favorites.

Authentication: required.

Success response: `204 No Content`

## Future Reviews

Review endpoints are planned for a later phase and should not block the first release.

Planned endpoints:

- `GET /api/clubs/{clubId}/reviews`
- `POST /api/clubs/{clubId}/reviews`
- `PUT /api/reviews/{reviewId}`
- `DELETE /api/reviews/{reviewId}`

## Status Codes

- `200 OK`: request succeeded.
- `201 Created`: resource created.
- `204 No Content`: resource deleted or no response body needed.
- `400 Bad Request`: invalid request body or query.
- `401 Unauthorized`: missing or invalid authentication.
- `403 Forbidden`: authenticated user cannot access the resource.
- `404 Not Found`: resource does not exist.
- `409 Conflict`: duplicate favorite or conflicting state.
- `422 Unprocessable Entity`: validation failed.
- `500 Internal Server Error`: unexpected server error.
