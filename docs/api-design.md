# API Design

The GoodLife Pulse Tracker API is an ASP.NET Core REST API used by the React frontend. It provides club discovery, occupancy visibility, crowd reporting, authentication, and favorites.

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

- `search`: optional text search by name, address, neighborhood, or city.
- `city`: optional city filter.
- `province`: optional province filter.
- `amenity`: optional amenity filter.
- `page`: optional page number, default `1`.
- `pageSize`: optional page size, default `20`.

Success response:

```json
{
  "items": [
    {
      "id": 1,
      "name": "GoodLife Fitness Calgary Stephen Avenue",
      "addressLine1": "140 8 Ave SW",
      "city": "Calgary",
      "province": "AB",
      "postalCode": "T2P 1B3",
      "latitude": 51.0451,
      "longitude": -114.0659,
      "amenities": ["Weights", "Cardio", "Group Fitness"],
      "occupancy": {
        "crowdLevel": "Moderate",
        "lastUpdatedAt": "2026-05-10T18:30:00Z",
        "confidenceScore": 0.75
      }
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 1
}
```

#### GET `/api/clubs/{clubId}`

Returns one club by ID.

Success response:

```json
{
  "id": 1,
  "name": "GoodLife Fitness Calgary Stephen Avenue",
  "addressLine1": "140 8 Ave SW",
  "addressLine2": null,
  "city": "Calgary",
  "province": "AB",
  "postalCode": "T2P 1B3",
  "phoneNumber": "403-000-0000",
  "latitude": 51.0451,
  "longitude": -114.0659,
  "amenities": ["Weights", "Cardio", "Group Fitness"],
  "occupancy": {
    "crowdLevel": "Moderate",
    "lastUpdatedAt": "2026-05-10T18:30:00Z",
    "confidenceScore": 0.75
  }
}
```

### Occupancy

#### GET `/api/clubs/{clubId}/occupancy`

Returns the current occupancy snapshot for a club.

Success response:

```json
{
  "clubId": 1,
  "crowdLevel": "Moderate",
  "lastUpdatedAt": "2026-05-10T18:30:00Z",
  "confidenceScore": 0.75,
  "reportCountWindow": 8
}
```

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
      "clubName": "GoodLife Fitness Calgary Stephen Avenue",
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
