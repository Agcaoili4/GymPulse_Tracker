namespace GymPulse.Api.Services.Occupancy;

public record OccupancyReading(
    int ClubId,
    int Percent,
    CrowdLevel CrowdLevel,
    bool IsEstimated,
    string Source,
    DateTime LastUpdatedAt);
