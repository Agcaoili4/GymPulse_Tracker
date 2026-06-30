namespace GymPulse.Api.Services.Occupancy;

public static class CrowdLevelMapper
{
    public static CrowdLevel FromPercent(int percent) => percent switch
    {
        <= 25 => CrowdLevel.Empty,
        <= 60 => CrowdLevel.Moderate,
        <= 85 => CrowdLevel.Busy,
        _ => CrowdLevel.Packed,
    };
}
