using GymPulse.Api.Services.Occupancy;
using Xunit;

namespace GymPulse.Api.Tests.Occupancy;

public class CrowdLevelMapperTests
{
    [Theory]
    [InlineData(0, CrowdLevel.Empty)]
    [InlineData(25, CrowdLevel.Empty)]
    [InlineData(26, CrowdLevel.Moderate)]
    [InlineData(60, CrowdLevel.Moderate)]
    [InlineData(61, CrowdLevel.Busy)]
    [InlineData(85, CrowdLevel.Busy)]
    [InlineData(86, CrowdLevel.Packed)]
    [InlineData(100, CrowdLevel.Packed)]
    public void FromPercent_MapsToExpectedLevel(int percent, CrowdLevel expected)
    {
        Assert.Equal(expected, CrowdLevelMapper.FromPercent(percent));
    }
}
