using Xunit;

namespace GymPulse.Api.Tests;

public class SmokeTests
{
    [Fact]
    public void ProgramEntryPointIsReferenceable()
    {
        Assert.NotNull(typeof(Program));
    }
}
