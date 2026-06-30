# Occupancy Engine Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a simulated gym-occupancy engine that serves estimated crowd levels over REST and pushes them live over SignalR, shown as scannable crowd badges in the React UI.

**Architecture:** A pure baseline calculator + an in-memory `IOccupancySource` (the swap point) hold each gym's current reading; a background simulator ticks every 15s, applies jitter, updates the source, and broadcasts changed gyms over a SignalR hub. Controllers read from the source; the React app subscribes to the hub and falls back to polling.

**Tech Stack:** ASP.NET Core 8 Web API, EF Core (SQL Server in prod, SQLite in tests), SignalR, xUnit + `WebApplicationFactory`; React 19 + Vite + TypeScript + Tailwind, `@microsoft/signalr`, Vitest + React Testing Library.

Source spec: `docs/specs/2026-06-28-occupancy-engine-design.md`.

## Global Constraints

- Backend namespace/project is `GymPulse.Api` after Task 1. New code uses `GymPulse.Api.*` namespaces.
- Crowd levels: exactly `Empty`, `Moderate`, `Busy`, `Packed`; serialized as their string names.
- Percent→level thresholds: `0–25 Empty`, `26–60 Moderate`, `61–85 Busy`, `86–100 Packed`.
- Occupancy is always labelled an estimate: `isEstimated: true`, `source: "simulated"` this build.
- Tick interval `15s`; jitter `±5` percent points.
- The `OccupancySnapshots` DB table stays unused this build (occupancy is in-memory).
- Do NOT rename the string `GoodLife` inside seed gym names (e.g. "GoodLife Fitness Calgary Stephen Avenue"); only the `GoodLifePulse` project token is renamed.
- `.env`, `.env.local`, `.env.production` are off-limits — never read or edit them.

## Pre-flight (before Task 1)

The working tree has an uncommitted change to `backend/GoodLifePulse.Api/Program.cs` that predates this work. Decide with the user whether to commit or `git stash` it BEFORE starting, so the rename/DI edits don't entangle it. Do not silently sweep it into a task commit.

---

## Task 1: Rename `GoodLifePulse.Api` → `GymPulse.Api`

**Files:**
- Move: `backend/GoodLifePulse.Api/` → `backend/GymPulse.Api/`
- Move: `GoodLifePulse.Api.csproj` → `GymPulse.Api.csproj`; `GoodLifePulse.Api.http` → `GymPulse.Api.http`
- Modify: every `*.cs` under the project (namespaces/usings), `docker-compose.yml`

**Interfaces:**
- Consumes: nothing.
- Produces: a building, running `GymPulse.Api` project with no `GoodLifePulse` token in source.

- [ ] **Step 1: Move folder and brand-named files with git**

```bash
cd /Users/test/Documents/GitHub/GymPulse_Tracker
git mv backend/GoodLifePulse.Api backend/GymPulse.Api
git mv backend/GymPulse.Api/GoodLifePulse.Api.csproj backend/GymPulse.Api/GymPulse.Api.csproj
git mv backend/GymPulse.Api/GoodLifePulse.Api.http backend/GymPulse.Api/GymPulse.Api.http
```

- [ ] **Step 2: Replace the project token in source files only**

```bash
grep -rl 'GoodLifePulse' backend/GymPulse.Api --include='*.cs' --include='*.http' | xargs sed -i '' 's/GoodLifePulse/GymPulse/g'
sed -i '' 's/goodlife-db/gympulse-db/g' docker-compose.yml
```

- [ ] **Step 3: Verify no project token remains (seed gym names must survive)**

Run: `grep -rn 'GoodLifePulse' backend/ docker-compose.yml ; grep -c 'GoodLife Fitness' backend/GymPulse.Api/Data/Seeder.cs`
Expected: first grep prints nothing; second prints `15` (seed names untouched).

- [ ] **Step 4: Clean stale build artifacts and build**

```bash
rm -rf backend/GymPulse.Api/bin backend/GymPulse.Api/obj
dotnet build backend/GymPulse.Api/GymPulse.Api.csproj
```
Expected: `Build succeeded`.

- [ ] **Step 5: Smoke-run the API and hit /health**

```bash
dotnet run --project backend/GymPulse.Api/GymPulse.Api.csproj &
sleep 12 && curl -s http://localhost:5279/health ; curl -s 'http://localhost:5279/api/clubs?pageSize=1'
kill %1
```
Expected: `{"status":"Healthy"}` and a clubs page with one item (no occupancy block yet).

- [ ] **Step 6: Commit**

```bash
git add -A backend/ docker-compose.yml
git commit -m "Rename GoodLifePulse.Api to GymPulse.Api"
```

---

## Task 2: Backend test project scaffold

**Files:**
- Create: `backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj`
- Create: `backend/GymPulse.Api.Tests/SmokeTests.cs`
- Modify: `backend/GymPulse.Api/Program.cs` (append `public partial class Program { }`)

**Interfaces:**
- Consumes: the `GymPulse.Api` project.
- Produces: a runnable `dotnet test` target and a publicly-referenceable `Program` for `WebApplicationFactory`.

- [ ] **Step 1: Create the test csproj**

`backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.11.1" />
    <PackageReference Include="xunit" Version="2.9.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.8.2" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.10" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\GymPulse.Api\GymPulse.Api.csproj" />
  </ItemGroup>

</Project>
```

- [ ] **Step 2: Make Program referenceable from tests**

Append to the very end of `backend/GymPulse.Api/Program.cs`:
```csharp

// Exposes the implicit Program class to the test project (WebApplicationFactory<Program>).
public partial class Program { }
```

- [ ] **Step 3: Write the smoke test**

`backend/GymPulse.Api.Tests/SmokeTests.cs`:
```csharp
namespace GymPulse.Api.Tests;

public class SmokeTests
{
    [Fact]
    public void TestProjectRuns()
    {
        Assert.True(true);
    }
}
```

- [ ] **Step 4: Run the test**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj`
Expected: `Passed!  - Failed: 0, Passed: 1`.

- [ ] **Step 5: Commit**

```bash
git add backend/GymPulse.Api.Tests backend/GymPulse.Api/Program.cs
git commit -m "Add backend test project scaffold"
```

---

## Task 3: Occupancy domain types + CrowdLevelMapper

**Files:**
- Create: `backend/GymPulse.Api/Services/Occupancy/CrowdLevel.cs`
- Create: `backend/GymPulse.Api/Services/Occupancy/OccupancyReading.cs`
- Create: `backend/GymPulse.Api/Services/Occupancy/CrowdLevelMapper.cs`
- Test: `backend/GymPulse.Api.Tests/Occupancy/CrowdLevelMapperTests.cs`

**Interfaces:**
- Produces: `enum CrowdLevel { Empty, Moderate, Busy, Packed }`; `record OccupancyReading(int ClubId, int Percent, CrowdLevel CrowdLevel, bool IsEstimated, string Source, DateTime LastUpdatedAt)`; `static CrowdLevel CrowdLevelMapper.FromPercent(int percent)`.

- [ ] **Step 1: Write the failing test**

`backend/GymPulse.Api.Tests/Occupancy/CrowdLevelMapperTests.cs`:
```csharp
using GymPulse.Api.Services.Occupancy;

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
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter CrowdLevelMapperTests`
Expected: FAIL — `CrowdLevel`/`CrowdLevelMapper` do not exist (compile error).

- [ ] **Step 3: Write the types and mapper**

`backend/GymPulse.Api/Services/Occupancy/CrowdLevel.cs`:
```csharp
namespace GymPulse.Api.Services.Occupancy;

public enum CrowdLevel
{
    Empty,
    Moderate,
    Busy,
    Packed,
}
```

`backend/GymPulse.Api/Services/Occupancy/OccupancyReading.cs`:
```csharp
namespace GymPulse.Api.Services.Occupancy;

public record OccupancyReading(
    int ClubId,
    int Percent,
    CrowdLevel CrowdLevel,
    bool IsEstimated,
    string Source,
    DateTime LastUpdatedAt);
```

`backend/GymPulse.Api/Services/Occupancy/CrowdLevelMapper.cs`:
```csharp
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
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter CrowdLevelMapperTests`
Expected: `Passed!` (8 cases).

- [ ] **Step 5: Commit**

```bash
git add backend/GymPulse.Api/Services/Occupancy backend/GymPulse.Api.Tests/Occupancy
git commit -m "Add crowd level types and percent mapper"
```

---

## Task 4: BaselineOccupancyCalculator

**Files:**
- Create: `backend/GymPulse.Api/Services/Occupancy/IOccupancyCalculator.cs`
- Create: `backend/GymPulse.Api/Services/Occupancy/BaselineOccupancyCalculator.cs`
- Test: `backend/GymPulse.Api.Tests/Occupancy/BaselineOccupancyCalculatorTests.cs`

**Interfaces:**
- Consumes: nothing.
- Produces: `interface IOccupancyCalculator { int GetBaselinePercent(int clubId, DateTime utcNow); }` and `class BaselineOccupancyCalculator : IOccupancyCalculator`.

- [ ] **Step 1: Write the failing test**

`backend/GymPulse.Api.Tests/Occupancy/BaselineOccupancyCalculatorTests.cs`:
```csharp
using GymPulse.Api.Services.Occupancy;

namespace GymPulse.Api.Tests.Occupancy;

public class BaselineOccupancyCalculatorTests
{
    private readonly BaselineOccupancyCalculator _sut = new();

    [Fact]
    public void GetBaselinePercent_IsDeterministic_ForSameInputs()
    {
        var when = new DateTime(2026, 6, 29, 18, 0, 0, DateTimeKind.Utc); // Mon 6pm
        Assert.Equal(_sut.GetBaselinePercent(42, when), _sut.GetBaselinePercent(42, when));
    }

    [Fact]
    public void GetBaselinePercent_StaysWithinZeroToHundred()
    {
        var when = new DateTime(2026, 6, 29, 18, 0, 0, DateTimeKind.Utc);
        for (var clubId = 1; clubId <= 200; clubId++)
        {
            var value = _sut.GetBaselinePercent(clubId, when);
            Assert.InRange(value, 0, 100);
        }
    }

    [Fact]
    public void GetBaselinePercent_EveningPeak_IsBusierThanEarlyMorning()
    {
        var evening = new DateTime(2026, 6, 29, 18, 0, 0, DateTimeKind.Utc);
        var night = new DateTime(2026, 6, 29, 3, 0, 0, DateTimeKind.Utc);
        Assert.True(_sut.GetBaselinePercent(42, evening) > _sut.GetBaselinePercent(42, night));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter BaselineOccupancyCalculatorTests`
Expected: FAIL — type does not exist (compile error).

- [ ] **Step 3: Write the calculator**

`backend/GymPulse.Api/Services/Occupancy/IOccupancyCalculator.cs`:
```csharp
namespace GymPulse.Api.Services.Occupancy;

public interface IOccupancyCalculator
{
    int GetBaselinePercent(int clubId, DateTime utcNow);
}
```

`backend/GymPulse.Api/Services/Occupancy/BaselineOccupancyCalculator.cs`:
```csharp
namespace GymPulse.Api.Services.Occupancy;

public class BaselineOccupancyCalculator : IOccupancyCalculator
{
    public int GetBaselinePercent(int clubId, DateTime utcNow)
    {
        var hourly = HourlyWeight(utcNow.Hour);
        var dayFactor = IsWeekend(utcNow.DayOfWeek) ? 0.85 : 1.0;
        var popularity = Popularity(clubId);
        var value = hourly * dayFactor * popularity * 100.0;
        return Math.Clamp((int)Math.Round(value), 0, 100);
    }

    private static bool IsWeekend(DayOfWeek d) => d is DayOfWeek.Saturday or DayOfWeek.Sunday;

    // Typical weekly gym busyness by hour of day (0..1): morning + evening peaks.
    private static double HourlyWeight(int hour) => hour switch
    {
        >= 0 and <= 4 => 0.05,
        5 => 0.25,
        6 => 0.55,
        7 => 0.75,
        8 => 0.70,
        9 => 0.55,
        10 => 0.45,
        11 => 0.45,
        12 => 0.55,
        13 => 0.50,
        14 => 0.45,
        15 => 0.55,
        16 => 0.70,
        17 => 0.85,
        18 => 0.90,
        19 => 0.80,
        20 => 0.65,
        21 => 0.45,
        22 => 0.25,
        _ => 0.10, // 23
    };

    // Stable per-gym popularity in 0.7..1.1 from a deterministic hash of the id.
    private static double Popularity(int clubId)
    {
        unchecked
        {
            var h = (uint)(clubId * 2654435761u);
            var frac = (h % 1000) / 1000.0;
            return 0.7 + frac * 0.4;
        }
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter BaselineOccupancyCalculatorTests`
Expected: `Passed!` (3 tests).

- [ ] **Step 5: Commit**

```bash
git add backend/GymPulse.Api/Services/Occupancy backend/GymPulse.Api.Tests/Occupancy
git commit -m "Add baseline occupancy calculator"
```

---

## Task 5: SimulatedOccupancySource

**Files:**
- Create: `backend/GymPulse.Api/Services/Occupancy/IOccupancySource.cs`
- Create: `backend/GymPulse.Api/Services/Occupancy/SimulatedOccupancySource.cs`
- Test: `backend/GymPulse.Api.Tests/Occupancy/SimulatedOccupancySourceTests.cs`

**Interfaces:**
- Consumes: `IOccupancyCalculator`, `OccupancyReading`, `CrowdLevelMapper`, `System.TimeProvider`.
- Produces: `interface IOccupancySource { OccupancyReading Get(int clubId); IReadOnlyDictionary<int, OccupancyReading> GetMany(IEnumerable<int> clubIds); }`; `class SimulatedOccupancySource : IOccupancySource` with `const string SourceName = "simulated"`, `void Update(OccupancyReading)`, `IReadOnlyCollection<OccupancyReading> Snapshot()`.

- [ ] **Step 1: Write the failing test**

`backend/GymPulse.Api.Tests/Occupancy/SimulatedOccupancySourceTests.cs`:
```csharp
using GymPulse.Api.Services.Occupancy;

namespace GymPulse.Api.Tests.Occupancy;

public class SimulatedOccupancySourceTests
{
    private static SimulatedOccupancySource NewSource() =>
        new(new BaselineOccupancyCalculator(), TimeProvider.System);

    [Fact]
    public void Get_ComputesAndCaches_OnMiss()
    {
        var source = NewSource();
        var first = source.Get(7);
        var second = source.Get(7);

        Assert.Equal("simulated", first.Source);
        Assert.True(first.IsEstimated);
        Assert.InRange(first.Percent, 0, 100);
        Assert.Same(first, second); // cached, not recomputed
    }

    [Fact]
    public void GetMany_ReturnsReadingForEveryRequestedId()
    {
        var source = NewSource();
        var result = source.GetMany(new[] { 1, 2, 3, 3 });

        Assert.Equal(3, result.Count);
        Assert.True(result.ContainsKey(1));
        Assert.True(result.ContainsKey(2));
        Assert.True(result.ContainsKey(3));
    }

    [Fact]
    public void Update_OverwritesStoredReading()
    {
        var source = NewSource();
        var updated = new OccupancyReading(5, 99, CrowdLevel.Packed, true, "simulated", DateTime.UtcNow);

        source.Update(updated);

        Assert.Same(updated, source.Get(5));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter SimulatedOccupancySourceTests`
Expected: FAIL — types do not exist (compile error).

- [ ] **Step 3: Write the source**

`backend/GymPulse.Api/Services/Occupancy/IOccupancySource.cs`:
```csharp
namespace GymPulse.Api.Services.Occupancy;

public interface IOccupancySource
{
    OccupancyReading Get(int clubId);

    IReadOnlyDictionary<int, OccupancyReading> GetMany(IEnumerable<int> clubIds);
}
```

`backend/GymPulse.Api/Services/Occupancy/SimulatedOccupancySource.cs`:
```csharp
using System.Collections.Concurrent;

namespace GymPulse.Api.Services.Occupancy;

public class SimulatedOccupancySource : IOccupancySource
{
    public const string SourceName = "simulated";

    private readonly IOccupancyCalculator _calculator;
    private readonly TimeProvider _time;
    private readonly ConcurrentDictionary<int, OccupancyReading> _readings = new();

    public SimulatedOccupancySource(IOccupancyCalculator calculator, TimeProvider time)
    {
        _calculator = calculator;
        _time = time;
    }

    public OccupancyReading Get(int clubId) => _readings.GetOrAdd(clubId, Baseline);

    public IReadOnlyDictionary<int, OccupancyReading> GetMany(IEnumerable<int> clubIds)
    {
        var result = new Dictionary<int, OccupancyReading>();
        foreach (var id in clubIds.Distinct())
        {
            result[id] = Get(id);
        }

        return result;
    }

    public void Update(OccupancyReading reading) => _readings[reading.ClubId] = reading;

    public IReadOnlyCollection<OccupancyReading> Snapshot() => _readings.Values.ToArray();

    private OccupancyReading Baseline(int clubId)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var percent = _calculator.GetBaselinePercent(clubId, now);
        return new OccupancyReading(clubId, percent, CrowdLevelMapper.FromPercent(percent), true, SourceName, now);
    }
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter SimulatedOccupancySourceTests`
Expected: `Passed!` (3 tests).

- [ ] **Step 5: Commit**

```bash
git add backend/GymPulse.Api/Services/Occupancy backend/GymPulse.Api.Tests/Occupancy
git commit -m "Add in-memory simulated occupancy source"
```

---

## Task 6: REST read surface (DTOs, enrichment, endpoint) + integration tests

**Files:**
- Create: `backend/GymPulse.Api/Dtos/OccupancyDto.cs`, `backend/GymPulse.Api/Dtos/ClubOccupancyDto.cs`
- Modify: `backend/GymPulse.Api/Dtos/ClubDto.cs`
- Modify: `backend/GymPulse.Api/Services/IClubService.cs`, `backend/GymPulse.Api/Services/ClubService.cs`
- Modify: `backend/GymPulse.Api/Controllers/ClubsController.cs`
- Modify: `backend/GymPulse.Api/Program.cs` (register TimeProvider, calculator, source)
- Create: `backend/GymPulse.Api.Tests/TestApiFactory.cs`, `backend/GymPulse.Api.Tests/ClubsOccupancyApiTests.cs`

**Interfaces:**
- Consumes: `IOccupancySource`, `OccupancyReading`, `CrowdLevel`.
- Produces: `record OccupancyDto(string CrowdLevel, int Percent, bool IsEstimated, string Source, DateTime LastUpdatedAt)`; `record ClubOccupancyDto(int ClubId, string CrowdLevel, int Percent, bool IsEstimated, string Source, DateTime LastUpdatedAt)`; `ClubDto` gains trailing `OccupancyDto? Occupancy`; `IClubService.GetClubOccupancyAsync(int clubId, CancellationToken) -> Task<ClubOccupancyDto?>`.

- [ ] **Step 1: Add the DTOs**

`backend/GymPulse.Api/Dtos/OccupancyDto.cs`:
```csharp
namespace GymPulse.Api.Dtos;

public record OccupancyDto(
    string CrowdLevel,
    int Percent,
    bool IsEstimated,
    string Source,
    DateTime LastUpdatedAt);
```

`backend/GymPulse.Api/Dtos/ClubOccupancyDto.cs`:
```csharp
namespace GymPulse.Api.Dtos;

public record ClubOccupancyDto(
    int ClubId,
    string CrowdLevel,
    int Percent,
    bool IsEstimated,
    string Source,
    DateTime LastUpdatedAt);
```

- [ ] **Step 2: Add `Occupancy` to `ClubDto`**

Replace `backend/GymPulse.Api/Dtos/ClubDto.cs` with:
```csharp
namespace GymPulse.Api.Dtos;

public record ClubDto(
    int Id,
    string Name,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string Province,
    string? PostalCode,
    string? PhoneNumber,
    decimal? Latitude,
    decimal? Longitude,
    OccupancyDto? Occupancy);
```

- [ ] **Step 3: Extend the service interface**

Replace `backend/GymPulse.Api/Services/IClubService.cs` with:
```csharp
using GymPulse.Api.Dtos;

namespace GymPulse.Api.Services;

public interface IClubService
{
    Task<PagedResult<ClubDto>> GetClubsAsync(ClubsQuery query, CancellationToken cancellationToken);

    Task<ClubDto?> GetClubByIdAsync(int id, CancellationToken cancellationToken);

    Task<ClubOccupancyDto?> GetClubOccupancyAsync(int clubId, CancellationToken cancellationToken);
}
```

- [ ] **Step 4: Rewrite `ClubService` to enrich with occupancy**

Replace `backend/GymPulse.Api/Services/ClubService.cs` with:
```csharp
using GymPulse.Api.Data;
using GymPulse.Api.Dtos;
using GymPulse.Api.Models;
using GymPulse.Api.Services.Occupancy;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Api.Services;

public class ClubService : IClubService
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _db;
    private readonly IOccupancySource _occupancy;

    public ClubService(AppDbContext db, IOccupancySource occupancy)
    {
        _db = db;
        _occupancy = occupancy;
    }

    public async Task<PagedResult<ClubDto>> GetClubsAsync(ClubsQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);

        var clubs = _db.Clubs
            .AsNoTracking()
            .Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            clubs = clubs.Where(c =>
                EF.Functions.Like(c.Name, $"%{term}%") ||
                EF.Functions.Like(c.AddressLine1, $"%{term}%") ||
                EF.Functions.Like(c.City, $"%{term}%"));
        }

        if (!string.IsNullOrWhiteSpace(query.City))
        {
            var city = query.City.Trim();
            clubs = clubs.Where(c => c.City == city);
        }

        if (!string.IsNullOrWhiteSpace(query.Province))
        {
            var province = query.Province.Trim();
            clubs = clubs.Where(c => c.Province == province);
        }

        var totalCount = await clubs.CountAsync(cancellationToken);

        var entities = await clubs
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var readings = _occupancy.GetMany(entities.Select(c => c.Id));
        var items = entities
            .Select(c => ToDto(c, readings.TryGetValue(c.Id, out var reading) ? reading : null))
            .ToList();

        return new PagedResult<ClubDto>(items, page, pageSize, totalCount);
    }

    public async Task<ClubDto?> GetClubByIdAsync(int id, CancellationToken cancellationToken)
    {
        var club = await _db.Clubs
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.IsActive, cancellationToken);

        return club is null ? null : ToDto(club, _occupancy.Get(id));
    }

    public async Task<ClubOccupancyDto?> GetClubOccupancyAsync(int clubId, CancellationToken cancellationToken)
    {
        var exists = await _db.Clubs
            .AsNoTracking()
            .AnyAsync(c => c.Id == clubId && c.IsActive, cancellationToken);

        if (!exists)
        {
            return null;
        }

        var r = _occupancy.Get(clubId);
        return new ClubOccupancyDto(r.ClubId, r.CrowdLevel.ToString(), r.Percent, r.IsEstimated, r.Source, r.LastUpdatedAt);
    }

    private static ClubDto ToDto(Club c, OccupancyReading? reading) =>
        new(
            c.Id,
            c.Name,
            c.AddressLine1,
            c.AddressLine2,
            c.City,
            c.Province,
            c.PostalCode,
            c.PhoneNumber,
            c.Latitude,
            c.Longitude,
            reading is null
                ? null
                : new OccupancyDto(reading.CrowdLevel.ToString(), reading.Percent, reading.IsEstimated, reading.Source, reading.LastUpdatedAt));
}
```

- [ ] **Step 5: Add the occupancy endpoint to the controller**

In `backend/GymPulse.Api/Controllers/ClubsController.cs`, add this action after `GetClubById`:
```csharp
    [HttpGet("{clubId:int}/occupancy")]
    public async Task<ActionResult<ClubOccupancyDto>> GetClubOccupancy(
        int clubId,
        CancellationToken cancellationToken)
    {
        var occupancy = await _clubs.GetClubOccupancyAsync(clubId, cancellationToken);
        return occupancy is null ? NotFound() : Ok(occupancy);
    }
```

- [ ] **Step 6: Register occupancy services in `Program.cs`**

In `backend/GymPulse.Api/Program.cs`, add `using GymPulse.Api.Services.Occupancy;` at the top, and after `builder.Services.AddScoped<IClubService, ClubService>();` add:
```csharp
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IOccupancyCalculator, BaselineOccupancyCalculator>();
builder.Services.AddSingleton<SimulatedOccupancySource>();
builder.Services.AddSingleton<IOccupancySource>(sp => sp.GetRequiredService<SimulatedOccupancySource>());
```

- [ ] **Step 7: Build to confirm the API compiles**

Run: `dotnet build backend/GymPulse.Api/GymPulse.Api.csproj`
Expected: `Build succeeded`.

- [ ] **Step 8: Add the SQLite-backed test factory**

`backend/GymPulse.Api.Tests/TestApiFactory.cs`:
```csharp
using GymPulse.Api.Data;
using GymPulse.Api.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymPulse.Api.Tests;

public class TestApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        _connection.Open();
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                            || d.ServiceType == typeof(AppDbContext))
                .ToList();
            foreach (var d in toRemove)
            {
                services.Remove(d);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            SeedTestClubs(db);
        });
    }

    private static void SeedTestClubs(AppDbContext db)
    {
        if (db.Clubs.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;
        db.Clubs.AddRange(
            new Club { Name = "Active Gym A", AddressLine1 = "1 Test St", City = "Calgary", Province = "AB", IsActive = true, CreatedAt = now },
            new Club { Name = "Active Gym B", AddressLine1 = "2 Test St", City = "Calgary", Province = "AB", IsActive = true, CreatedAt = now },
            new Club { Name = "Closed Gym", AddressLine1 = "3 Test St", City = "Calgary", Province = "AB", IsActive = false, CreatedAt = now });
        db.SaveChanges();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
```

- [ ] **Step 9: Write the integration tests**

`backend/GymPulse.Api.Tests/ClubsOccupancyApiTests.cs`:
```csharp
using System.Net;
using System.Net.Http.Json;
using GymPulse.Api.Data;
using GymPulse.Api.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace GymPulse.Api.Tests;

public class ClubsOccupancyApiTests : IClassFixture<TestApiFactory>
{
    private readonly TestApiFactory _factory;

    public ClubsOccupancyApiTests(TestApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetClubs_IncludesPopulatedOccupancyBlock()
    {
        var client = _factory.CreateClient();

        var page = await client.GetFromJsonAsync<PagedResult<ClubDto>>("/api/clubs");

        Assert.NotNull(page);
        Assert.NotEmpty(page!.Items);
        foreach (var club in page.Items)
        {
            Assert.NotNull(club.Occupancy);
            Assert.InRange(club.Occupancy!.Percent, 0, 100);
            Assert.True(club.Occupancy.IsEstimated);
            Assert.Equal("simulated", club.Occupancy.Source);
            Assert.Contains(club.Occupancy.CrowdLevel, new[] { "Empty", "Moderate", "Busy", "Packed" });
        }
    }

    [Fact]
    public async Task GetClubOccupancy_ReturnsReading_ForActiveClub()
    {
        var client = _factory.CreateClient();
        int activeId = FirstActiveClubId();

        var occupancy = await client.GetFromJsonAsync<ClubOccupancyDto>($"/api/clubs/{activeId}/occupancy");

        Assert.NotNull(occupancy);
        Assert.Equal(activeId, occupancy!.ClubId);
        Assert.Equal("simulated", occupancy.Source);
    }

    [Fact]
    public async Task GetClubOccupancy_Returns404_ForInactiveClub()
    {
        var client = _factory.CreateClient();
        int inactiveId = FirstInactiveClubId();

        var response = await client.GetAsync($"/api/clubs/{inactiveId}/occupancy");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetClubOccupancy_Returns404_ForMissingClub()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/clubs/999999/occupancy");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private int FirstActiveClubId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Clubs.Where(c => c.IsActive).OrderBy(c => c.Id).Select(c => c.Id).First();
    }

    private int FirstInactiveClubId()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Clubs.Where(c => !c.IsActive).OrderBy(c => c.Id).Select(c => c.Id).First();
    }
}
```

- [ ] **Step 10: Run the integration tests**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter ClubsOccupancyApiTests`
Expected: `Passed!` (4 tests).

- [ ] **Step 11: Commit**

```bash
git add backend/GymPulse.Api backend/GymPulse.Api.Tests
git commit -m "Serve occupancy over REST with integration tests"
```

---

## Task 7: SignalR hub + wiring

**Files:**
- Create: `backend/GymPulse.Api/Hubs/OccupancyHub.cs`
- Modify: `backend/GymPulse.Api/Program.cs` (AddSignalR, MapHub, CORS AllowCredentials)

**Interfaces:**
- Produces: `class OccupancyHub : Hub` at route `/hubs/occupancy`; SignalR services registered.

- [ ] **Step 1: Create the hub**

`backend/GymPulse.Api/Hubs/OccupancyHub.cs`:
```csharp
using Microsoft.AspNetCore.SignalR;

namespace GymPulse.Api.Hubs;

// Clients only receive occupancyUpdated broadcasts; no server-side methods needed yet.
public class OccupancyHub : Hub
{
}
```

- [ ] **Step 2: Register SignalR and map the hub**

In `backend/GymPulse.Api/Program.cs`:
- add `using GymPulse.Api.Hubs;` at the top;
- after `builder.Services.AddSwaggerGen();` add `builder.Services.AddSignalR();`
- in the `FrontendDev` CORS policy, add `.AllowCredentials()` to the chain (after `.AllowAnyMethod()`);
- after `app.MapControllers();` add `app.MapHub<OccupancyHub>("/hubs/occupancy");`

The CORS policy becomes:
```csharp
        policy
            .WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
```

- [ ] **Step 3: Build and confirm the hub negotiates**

```bash
dotnet build backend/GymPulse.Api/GymPulse.Api.csproj
dotnet run --project backend/GymPulse.Api/GymPulse.Api.csproj &
sleep 12 && curl -s -o /dev/null -w "%{http_code}\n" -X POST http://localhost:5279/hubs/occupancy/negotiate
kill %1
```
Expected: `Build succeeded`; negotiate returns `200`.

- [ ] **Step 4: Commit**

```bash
git add backend/GymPulse.Api
git commit -m "Add SignalR occupancy hub and wiring"
```

---

## Task 8: OccupancySimulator background service + broadcast

**Files:**
- Create: `backend/GymPulse.Api/Services/Occupancy/OccupancySimulator.cs`
- Modify: `backend/GymPulse.Api/Program.cs` (AddHostedService)
- Modify: `backend/GymPulse.Api.Tests/TestApiFactory.cs` (remove the hosted simulator in tests)
- Test: `backend/GymPulse.Api.Tests/Occupancy/OccupancySimulatorTests.cs`

**Interfaces:**
- Consumes: `SimulatedOccupancySource`, `IOccupancyCalculator`, `IHubContext<OccupancyHub>`, `IServiceScopeFactory`, `TimeProvider`, `AppDbContext`, `ClubOccupancyDto`.
- Produces: `class OccupancySimulator : BackgroundService` with `public static bool HasChanged(OccupancyReading previous, OccupancyReading next)`.

- [ ] **Step 1: Write the failing test (diff logic)**

`backend/GymPulse.Api.Tests/Occupancy/OccupancySimulatorTests.cs`:
```csharp
using GymPulse.Api.Services.Occupancy;

namespace GymPulse.Api.Tests.Occupancy;

public class OccupancySimulatorTests
{
    private static OccupancyReading Reading(int percent, CrowdLevel level) =>
        new(1, percent, level, true, "simulated", DateTime.UtcNow);

    [Fact]
    public void HasChanged_False_WhenPercentAndLevelEqual()
    {
        Assert.False(OccupancySimulator.HasChanged(Reading(50, CrowdLevel.Moderate), Reading(50, CrowdLevel.Moderate)));
    }

    [Fact]
    public void HasChanged_True_WhenPercentDiffers()
    {
        Assert.True(OccupancySimulator.HasChanged(Reading(50, CrowdLevel.Moderate), Reading(52, CrowdLevel.Moderate)));
    }

    [Fact]
    public void HasChanged_True_WhenLevelDiffers()
    {
        Assert.True(OccupancySimulator.HasChanged(Reading(60, CrowdLevel.Moderate), Reading(61, CrowdLevel.Busy)));
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter OccupancySimulatorTests`
Expected: FAIL — `OccupancySimulator` does not exist (compile error).

- [ ] **Step 3: Write the background service**

`backend/GymPulse.Api/Services/Occupancy/OccupancySimulator.cs`:
```csharp
using GymPulse.Api.Data;
using GymPulse.Api.Dtos;
using GymPulse.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Api.Services.Occupancy;

public class OccupancySimulator : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromSeconds(15);
    private const int JitterMagnitude = 5;

    private readonly SimulatedOccupancySource _source;
    private readonly IOccupancyCalculator _calculator;
    private readonly IHubContext<OccupancyHub> _hub;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _time;
    private readonly Random _random = new();

    private int[] _clubIds = Array.Empty<int>();

    public OccupancySimulator(
        SimulatedOccupancySource source,
        IOccupancyCalculator calculator,
        IHubContext<OccupancyHub> hub,
        IServiceScopeFactory scopeFactory,
        TimeProvider time)
    {
        _source = source;
        _calculator = calculator;
        _hub = hub;
        _scopeFactory = scopeFactory;
        _time = time;
    }

    public static bool HasChanged(OccupancyReading previous, OccupancyReading next) =>
        previous.Percent != next.Percent || previous.CrowdLevel != next.CrowdLevel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _clubIds = await LoadActiveClubIdsAsync(stoppingToken);

        await TickAsync(stoppingToken);

        using var timer = new PeriodicTimer(TickInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await TickAsync(stoppingToken);
        }
    }

    private async Task TickAsync(CancellationToken ct)
    {
        var now = _time.GetUtcNow().UtcDateTime;
        var changed = new List<ClubOccupancyDto>();

        foreach (var clubId in _clubIds)
        {
            var previous = _source.Get(clubId);
            var next = ComputeNext(clubId, now);
            if (HasChanged(previous, next))
            {
                _source.Update(next);
                changed.Add(ToDto(next));
            }
        }

        if (changed.Count > 0)
        {
            await _hub.Clients.All.SendAsync("occupancyUpdated", changed, ct);
        }
    }

    private OccupancyReading ComputeNext(int clubId, DateTime now)
    {
        var target = _calculator.GetBaselinePercent(clubId, now);
        var jitter = _random.Next(-JitterMagnitude, JitterMagnitude + 1);
        var percent = Math.Clamp(target + jitter, 0, 100);
        return new OccupancyReading(clubId, percent, CrowdLevelMapper.FromPercent(percent), true, SimulatedOccupancySource.SourceName, now);
    }

    private async Task<int[]> LoadActiveClubIdsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.Clubs.Where(c => c.IsActive).Select(c => c.Id).ToArrayAsync(ct);
    }

    private static ClubOccupancyDto ToDto(OccupancyReading r) =>
        new(r.ClubId, r.CrowdLevel.ToString(), r.Percent, r.IsEstimated, r.Source, r.LastUpdatedAt);
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj --filter OccupancySimulatorTests`
Expected: `Passed!` (3 tests).

- [ ] **Step 5: Register the hosted service**

In `backend/GymPulse.Api/Program.cs`, after the `AddSingleton<IOccupancySource>(...)` line add:
```csharp
builder.Services.AddHostedService<OccupancySimulator>();
```

- [ ] **Step 6: Keep the simulator out of integration tests**

In `backend/GymPulse.Api.Tests/TestApiFactory.cs`, inside `ConfigureServices` (before building the provider), add:
```csharp
            var hosted = services
                .Where(d => d.ImplementationType == typeof(GymPulse.Api.Services.Occupancy.OccupancySimulator))
                .ToList();
            foreach (var d in hosted)
            {
                services.Remove(d);
            }
```

- [ ] **Step 7: Run the full backend suite**

Run: `dotnet test backend/GymPulse.Api.Tests/GymPulse.Api.Tests.csproj`
Expected: `Passed!` — all backend tests green.

- [ ] **Step 8: Commit**

```bash
git add backend/GymPulse.Api backend/GymPulse.Api.Tests
git commit -m "Add occupancy simulator background service with broadcast"
```

---

## Task 9: Frontend test infra + occupancy types

**Files:**
- Modify: `frontend/package.json` (devDeps + `test` script)
- Modify: `frontend/vite.config.ts` (Vitest config)
- Create: `frontend/src/test/setup.ts`
- Modify: `frontend/src/types/club.ts`
- Test: `frontend/src/types/club.test.ts`

**Interfaces:**
- Produces: `type CrowdLevel`, `type Occupancy`, `type ClubOccupancy`; `Club` gains `occupancy: Occupancy | null`; a working `npm test`.

- [ ] **Step 1: Install dev dependencies**

```bash
cd frontend
npm install -D vitest@^3 jsdom@^25 @testing-library/react@^16 @testing-library/jest-dom@^6 @testing-library/dom@^10
npm install @microsoft/signalr@^8
```

- [ ] **Step 2: Add the `test` script**

In `frontend/package.json` `scripts`, add:
```json
    "test": "vitest run",
    "test:watch": "vitest",
```

- [ ] **Step 3: Configure Vitest**

Replace `frontend/vite.config.ts` with:
```ts
/// <reference types="vitest/config" />
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  test: {
    environment: 'jsdom',
    globals: true,
    setupFiles: './src/test/setup.ts',
  },
})
```

`frontend/src/test/setup.ts`:
```ts
import '@testing-library/jest-dom';
```

- [ ] **Step 4: Add occupancy types and write the failing test**

Replace `frontend/src/types/club.ts` with:
```ts
export type CrowdLevel = 'Empty' | 'Moderate' | 'Busy' | 'Packed';

export type Occupancy = {
  crowdLevel: CrowdLevel;
  percent: number;
  isEstimated: boolean;
  source: string;
  lastUpdatedAt: string;
};

export type ClubOccupancy = Occupancy & { clubId: number };

export type Club = {
  id: number;
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  province: string;
  postalCode: string | null;
  phoneNumber: string | null;
  latitude: number | null;
  longitude: number | null;
  occupancy: Occupancy | null;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export function formatFullAddress(club: Club): string {
  const parts = [club.addressLine1, club.addressLine2, club.city, club.province, club.postalCode];
  return parts.filter((part): part is string => Boolean(part)).join(', ');
}
```

`frontend/src/types/club.test.ts`:
```ts
import { describe, expect, it } from 'vitest';
import { formatFullAddress, type Club } from './club';

const baseClub: Club = {
  id: 1, name: 'Test', addressLine1: '1 St', addressLine2: null,
  city: 'Calgary', province: 'AB', postalCode: 'T2P 1B3',
  phoneNumber: null, latitude: null, longitude: null, occupancy: null,
};

describe('formatFullAddress', () => {
  it('joins present parts and skips empty ones', () => {
    expect(formatFullAddress(baseClub)).toBe('1 St, Calgary, AB, T2P 1B3');
  });
});
```

- [ ] **Step 5: Run the test**

Run: `cd frontend && npm test`
Expected: `1 passed`.

- [ ] **Step 6: Commit**

```bash
git add frontend/package.json frontend/package-lock.json frontend/vite.config.ts frontend/src/test frontend/src/types/club.ts frontend/src/types/club.test.ts
git commit -m "Add frontend test setup and occupancy types"
```

---

## Task 10: CrowdBadge component

**Files:**
- Create: `frontend/src/components/CrowdBadge.tsx`
- Test: `frontend/src/components/CrowdBadge.test.tsx`

**Interfaces:**
- Consumes: `Occupancy` type.
- Produces: `function CrowdBadge({ occupancy }: { occupancy: Occupancy | null })`.

- [ ] **Step 1: Write the failing test**

`frontend/src/components/CrowdBadge.test.tsx`:
```tsx
import { render, screen } from '@testing-library/react';
import { describe, expect, it } from 'vitest';
import { CrowdBadge } from './CrowdBadge';
import type { Occupancy } from '../types/club';

const occ = (overrides: Partial<Occupancy> = {}): Occupancy => ({
  crowdLevel: 'Moderate', percent: 55, isEstimated: true,
  source: 'simulated', lastUpdatedAt: '2026-06-28T18:30:00Z', ...overrides,
});

describe('CrowdBadge', () => {
  it('shows the level label and an estimate-aware accessible name', () => {
    render(<CrowdBadge occupancy={occ({ crowdLevel: 'Busy', percent: 70 })} />);
    expect(screen.getByText('Busy')).toBeInTheDocument();
    expect(screen.getByLabelText(/Crowd level: Busy, estimated 70%/i)).toBeInTheDocument();
  });

  it('renders Unknown when occupancy is null', () => {
    render(<CrowdBadge occupancy={null} />);
    expect(screen.getByText('Unknown')).toBeInTheDocument();
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd frontend && npx vitest run src/components/CrowdBadge.test.tsx`
Expected: FAIL — cannot resolve `./CrowdBadge`.

- [ ] **Step 3: Write the component**

`frontend/src/components/CrowdBadge.tsx`:
```tsx
import type { CrowdLevel, Occupancy } from '../types/club';

const STYLES: Record<CrowdLevel, { dot: string; text: string }> = {
  Empty: { dot: 'bg-emerald-500', text: 'text-emerald-700' },
  Moderate: { dot: 'bg-amber-500', text: 'text-amber-700' },
  Busy: { dot: 'bg-orange-500', text: 'text-orange-700' },
  Packed: { dot: 'bg-rose-500', text: 'text-rose-700' },
};

type CrowdBadgeProps = {
  occupancy: Occupancy | null;
};

export function CrowdBadge({ occupancy }: CrowdBadgeProps) {
  if (!occupancy) {
    return (
      <span
        className="inline-flex items-center gap-1.5 rounded-full bg-slate-100 px-2.5 py-1 text-xs font-medium text-slate-500"
        aria-label="Crowd level: unknown"
      >
        <span className="h-2 w-2 rounded-full bg-slate-400" aria-hidden="true" />
        Unknown
      </span>
    );
  }

  const style = STYLES[occupancy.crowdLevel];

  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-full bg-white px-2.5 py-1 text-xs font-medium ${style.text}`}
      title="Estimated — not official gym capacity"
      aria-label={`Crowd level: ${occupancy.crowdLevel}, estimated ${occupancy.percent}%`}
    >
      <span className={`h-2 w-2 rounded-full ${style.dot}`} aria-hidden="true" />
      {occupancy.crowdLevel}
      <span className="text-slate-400">· est. {occupancy.percent}%</span>
    </span>
  );
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd frontend && npx vitest run src/components/CrowdBadge.test.tsx`
Expected: `2 passed`.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/components/CrowdBadge.tsx frontend/src/components/CrowdBadge.test.tsx
git commit -m "Add CrowdBadge component"
```

---

## Task 11: Occupancy API client + merge reducer

**Files:**
- Create: `frontend/src/api/occupancy.ts`
- Create: `frontend/src/hooks/mergeOccupancy.ts`
- Test: `frontend/src/hooks/mergeOccupancy.test.ts`

**Interfaces:**
- Consumes: `Club`, `ClubOccupancy`, `@microsoft/signalr`.
- Produces: `createOccupancyConnection(): HubConnection`; `getClubOccupancy(clubId, signal?): Promise<ClubOccupancy>`; `mergeOccupancy(clubs: Club[], updates: ClubOccupancy[]): Club[]`.

- [ ] **Step 1: Write the failing reducer test**

`frontend/src/hooks/mergeOccupancy.test.ts`:
```ts
import { describe, expect, it } from 'vitest';
import { mergeOccupancy } from './mergeOccupancy';
import type { Club, ClubOccupancy } from '../types/club';

const club = (id: number): Club => ({
  id, name: `Gym ${id}`, addressLine1: '1 St', addressLine2: null,
  city: 'Calgary', province: 'AB', postalCode: null, phoneNumber: null,
  latitude: null, longitude: null, occupancy: null,
});

const update = (clubId: number, percent: number): ClubOccupancy => ({
  clubId, crowdLevel: 'Busy', percent, isEstimated: true,
  source: 'simulated', lastUpdatedAt: '2026-06-28T18:30:00Z',
});

describe('mergeOccupancy', () => {
  it('applies a matching update by clubId and leaves others untouched', () => {
    const clubs = [club(1), club(2)];
    const result = mergeOccupancy(clubs, [update(2, 70)]);
    expect(result[0].occupancy).toBeNull();
    expect(result[1].occupancy?.percent).toBe(70);
    expect(result[1].occupancy?.crowdLevel).toBe('Busy');
  });

  it('returns the same array reference when no update matches', () => {
    const clubs = [club(1)];
    expect(mergeOccupancy(clubs, [update(999, 50)])).toBe(clubs);
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `cd frontend && npx vitest run src/hooks/mergeOccupancy.test.ts`
Expected: FAIL — cannot resolve `./mergeOccupancy`.

- [ ] **Step 3: Write the reducer and API client**

`frontend/src/hooks/mergeOccupancy.ts`:
```ts
import type { Club, ClubOccupancy } from '../types/club';

export function mergeOccupancy(clubs: Club[], updates: ClubOccupancy[]): Club[] {
  if (updates.length === 0) {
    return clubs;
  }

  const byId = new Map(updates.map((u) => [u.clubId, u]));
  let changed = false;

  const next = clubs.map((club) => {
    const u = byId.get(club.id);
    if (!u) {
      return club;
    }
    changed = true;
    const { clubId: _clubId, ...occupancy } = u;
    return { ...club, occupancy };
  });

  return changed ? next : clubs;
}
```

`frontend/src/api/occupancy.ts`:
```ts
import { HubConnectionBuilder, type HubConnection } from '@microsoft/signalr';
import type { ClubOccupancy } from '../types/club';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5279';

export function createOccupancyConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/hubs/occupancy`)
    .withAutomaticReconnect()
    .build();
}

export async function getClubOccupancy(clubId: number, signal?: AbortSignal): Promise<ClubOccupancy> {
  const response = await fetch(`${API_BASE_URL}/api/clubs/${clubId}/occupancy`, { signal });
  if (!response.ok) {
    throw new Error(`GET /api/clubs/${clubId}/occupancy failed with ${response.status}`);
  }
  return (await response.json()) as ClubOccupancy;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run: `cd frontend && npx vitest run src/hooks/mergeOccupancy.test.ts`
Expected: `2 passed`.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/api/occupancy.ts frontend/src/hooks/mergeOccupancy.ts frontend/src/hooks/mergeOccupancy.test.ts
git commit -m "Add occupancy SignalR client and merge reducer"
```

---

## Task 12: useClubsWithLiveOccupancy hook

**Files:**
- Create: `frontend/src/hooks/useClubsWithLiveOccupancy.ts`

**Interfaces:**
- Consumes: `getClubs`, `createOccupancyConnection`, `mergeOccupancy`, `Club`, `ClubOccupancy`, `HubConnectionState`.
- Produces: `useClubsWithLiveOccupancy(): { state: ClubsState; connectionStatus: ConnectionStatus }` where `ClubsState = { status: 'loading' } | { status: 'error'; message: string } | { status: 'ready'; clubs: Club[]; totalCount: number }` and `ConnectionStatus = 'connecting' | 'live' | 'reconnecting' | 'offline'`.

- [ ] **Step 1: Write the hook**

`frontend/src/hooks/useClubsWithLiveOccupancy.ts`:
```ts
import { useEffect, useState } from 'react';
import { HubConnectionState } from '@microsoft/signalr';
import { getClubs } from '../api/clubs';
import { createOccupancyConnection } from '../api/occupancy';
import { mergeOccupancy } from './mergeOccupancy';
import type { Club, ClubOccupancy } from '../types/club';

export type ConnectionStatus = 'connecting' | 'live' | 'reconnecting' | 'offline';

export type ClubsState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; clubs: Club[]; totalCount: number };

const PAGE_SIZE = 50;
const POLL_INTERVAL_MS = 20_000;

export function useClubsWithLiveOccupancy() {
  const [state, setState] = useState<ClubsState>({ status: 'loading' });
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>('connecting');

  useEffect(() => {
    const controller = new AbortController();
    let disposed = false;
    let pollTimer: ReturnType<typeof setInterval> | undefined;

    const applyClubs = (clubs: Club[], totalCount: number) => {
      if (!disposed) {
        setState({ status: 'ready', clubs, totalCount });
      }
    };

    const startPolling = () => {
      if (pollTimer) {
        return;
      }
      pollTimer = setInterval(() => {
        getClubs({ pageSize: PAGE_SIZE }, controller.signal)
          .then((page) => applyClubs(page.items, page.totalCount))
          .catch(() => {
            /* keep last good data on a failed poll */
          });
      }, POLL_INTERVAL_MS);
    };

    const stopPolling = () => {
      if (pollTimer) {
        clearInterval(pollTimer);
        pollTimer = undefined;
      }
    };

    const connection = createOccupancyConnection();

    connection.on('occupancyUpdated', (updates: ClubOccupancy[]) => {
      setState((prev) =>
        prev.status === 'ready'
          ? { ...prev, clubs: mergeOccupancy(prev.clubs, updates) }
          : prev,
      );
    });

    connection.onreconnecting(() => {
      if (!disposed) {
        setConnectionStatus('reconnecting');
      }
      startPolling();
    });
    connection.onreconnected(() => {
      if (!disposed) {
        setConnectionStatus('live');
      }
      stopPolling();
    });
    connection.onclose(() => {
      if (!disposed) {
        setConnectionStatus('offline');
      }
      startPolling();
    });

    void (async () => {
      try {
        const page = await getClubs({ pageSize: PAGE_SIZE }, controller.signal);
        applyClubs(page.items, page.totalCount);
      } catch (error: unknown) {
        if (error instanceof DOMException && error.name === 'AbortError') {
          return;
        }
        const message = error instanceof Error ? error.message : 'Unknown error';
        if (!disposed) {
          setState({ status: 'error', message });
        }
        return;
      }

      try {
        await connection.start();
        if (!disposed && connection.state === HubConnectionState.Connected) {
          setConnectionStatus('live');
        }
      } catch {
        if (!disposed) {
          setConnectionStatus('offline');
        }
        startPolling();
      }
    })();

    return () => {
      disposed = true;
      controller.abort();
      stopPolling();
      void connection.stop();
    };
  }, []);

  return { state, connectionStatus };
}
```

- [ ] **Step 2: Type-check**

Run: `cd frontend && npx tsc --noEmit`
Expected: no errors.

- [ ] **Step 3: Commit**

```bash
git add frontend/src/hooks/useClubsWithLiveOccupancy.ts
git commit -m "Add live-occupancy clubs hook with polling fallback"
```

---

## Task 13: Wire the badge and live status into the UI

**Files:**
- Modify: `frontend/src/components/ClubCard.tsx`
- Modify: `frontend/src/App.tsx`

**Interfaces:**
- Consumes: `CrowdBadge`, `useClubsWithLiveOccupancy`, `ConnectionStatus`.
- Produces: rendered crowd badges + last-updated time on cards; a header live-status indicator.

- [ ] **Step 1: Render the badge and last-updated time in ClubCard**

In `frontend/src/components/ClubCard.tsx`:
- add `import { CrowdBadge } from './CrowdBadge';` at the top;
- replace the `<header>` block with the version below (adds the badge to the right of the name);
- add the `formatRelativeTime` helper at the bottom (next to the icon helpers).

Replace the existing `<header>...</header>` with:
```tsx
      <header className="flex items-start justify-between gap-3">
        <div>
          <h3 className="text-base font-semibold text-slate-900">{club.name}</h3>
          <p className="mt-1 flex items-start gap-1 text-sm text-slate-500">
            <MapPinIcon />
            <span>
              {club.addressLine1}
              {club.addressLine2 ? `, ${club.addressLine2}` : ''}
              <br />
              {club.city}, {club.province}
              {club.postalCode ? ` ${club.postalCode}` : ''}
            </span>
          </p>
        </div>
        <div className="flex flex-col items-end gap-1">
          <CrowdBadge occupancy={club.occupancy} />
          {club.occupancy && (
            <time dateTime={club.occupancy.lastUpdatedAt} className="text-[11px] text-slate-400">
              updated {formatRelativeTime(club.occupancy.lastUpdatedAt)}
            </time>
          )}
        </div>
      </header>
```

Add at the bottom of the file:
```tsx
function formatRelativeTime(iso: string): string {
  const seconds = Math.max(0, Math.round((Date.now() - new Date(iso).getTime()) / 1000));
  if (seconds < 60) return `${seconds}s ago`;
  const minutes = Math.round(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.round(minutes / 60);
  return `${hours}h ago`;
}
```

- [ ] **Step 2: Consume the hook and show live status in App**

Replace `frontend/src/App.tsx` with:
```tsx
import { ClubCard } from './components/ClubCard';
import { useClubsWithLiveOccupancy, type ConnectionStatus } from './hooks/useClubsWithLiveOccupancy';

const STATUS_LABEL: Record<ConnectionStatus, string> = {
  connecting: 'Connecting…',
  live: 'Live',
  reconnecting: 'Reconnecting…',
  offline: 'Updates paused',
};

function LiveIndicator({ status }: { status: ConnectionStatus }) {
  const dot = status === 'live' ? 'bg-emerald-500' : status === 'offline' ? 'bg-rose-500' : 'bg-amber-500';
  return (
    <span className="inline-flex items-center gap-1.5 text-xs font-medium text-slate-500">
      <span className={`h-2 w-2 rounded-full ${dot}`} aria-hidden="true" />
      {STATUS_LABEL[status]}
    </span>
  );
}

function App() {
  const { state, connectionStatus } = useClubsWithLiveOccupancy();

  return (
    <main className="min-h-screen bg-slate-50 px-6 py-10 sm:px-10">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8 flex items-start justify-between gap-3">
          <div>
            <h1 className="text-2xl font-semibold text-slate-900">
              {state.status === 'ready' ? `${state.totalCount} Clubs` : 'Clubs'}
            </h1>
            <p className="text-sm text-slate-500">All clubs across Calgary</p>
          </div>
          <LiveIndicator status={connectionStatus} />
        </header>

        {state.status === 'loading' && <p className="text-sm text-slate-500">Loading clubs…</p>}

        {state.status === 'error' && (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
            Failed to load clubs: {state.message}
            <p className="mt-2 text-rose-600/80">Is the API running at http://localhost:5279?</p>
          </div>
        )}

        {state.status === 'ready' && (
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
            {state.clubs.map((club) => (
              <ClubCard key={club.id} club={club} />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}

export default App;
```

- [ ] **Step 3: Type-check, lint, and build**

Run: `cd frontend && npx tsc --noEmit && npm run lint && npm run build`
Expected: no type errors, lint clean, `vite build` succeeds.

- [ ] **Step 4: Manual end-to-end check**

```bash
# terminal 1
dotnet run --project backend/GymPulse.Api/GymPulse.Api.csproj
# terminal 2
cd frontend && npm run dev
```
Open http://localhost:5173. Expected: each card shows a colored crowd badge + "updated Ns ago"; the header shows "Live"; badges/percentages change within ~15–30s without a refresh.

- [ ] **Step 5: Commit**

```bash
git add frontend/src/components/ClubCard.tsx frontend/src/App.tsx
git commit -m "Show live crowd badges and connection status in the UI"
```

---

## Task 14: Documentation reconciliation

**Files:**
- Modify: `docs/api-design.md`, `docs/architecture.md`, `README.md`

**Interfaces:** none (documentation only).

- [ ] **Step 1: Update the real-time payload + fallback in api-design.md**

In `docs/api-design.md`, in the "Real-Time Updates" section, change the description so `occupancyUpdated` carries a **batch array** of changed gyms (each item: `clubId, crowdLevel, percent, isEstimated, source, lastUpdatedAt`), and note that the list view's polling fallback re-fetches `GET /api/clubs`, while `GET /api/clubs/{clubId}/occupancy` is the fallback for the single-club detail view.

- [ ] **Step 2: Mirror the fallback note in architecture.md**

In `docs/architecture.md`, in "Occupancy And Real-Time Strategy", add one line: the list view falls back to periodic `GET /api/clubs` refetch; the detail view falls back to `GET /api/clubs/{id}/occupancy`.

- [ ] **Step 3: Add the occupancy endpoint to the README API list**

In `README.md` under "Current endpoints", add: `- `GET /api/clubs/{id}/occupancy` returns the current estimated crowd level for a gym.` and note live updates over `/hubs/occupancy`.

- [ ] **Step 4: Commit**

```bash
git add docs/api-design.md docs/architecture.md README.md
git commit -m "Reconcile docs with batched occupancy events and fallbacks"
```

---

## Self-Review

**Spec coverage:**
- Rename prep → Task 1. ✅
- Engine internals (CrowdLevel, OccupancyReading, CrowdLevelMapper, calculator, source, simulator) → Tasks 3, 4, 5, 8. ✅
- REST read surface (DTOs, ClubDto, enrichment, endpoint, DI) → Task 6. ✅
- SignalR (hub, batched broadcast, CORS, wiring) → Tasks 7, 8. ✅
- Frontend (types, CrowdBadge, client, merge reducer, hook, ClubCard, App, live status, fallback) → Tasks 9–13. ✅
- Testing (backend unit + integration, frontend unit) → Tasks 2–13. ✅
- Documentation reconciliations → Task 14. ✅
- Cold-start guarantee (compute-on-miss) → Task 5 source + asserted in Task 6. ✅

**Type consistency:** `OccupancyReading`, `OccupancyDto`, `ClubOccupancyDto`, and the SignalR payload item are consistent across tasks; `IOccupancySource.Get/GetMany`, `SimulatedOccupancySource.Update/Snapshot/SourceName`, `IClubService.GetClubOccupancyAsync`, `OccupancySimulator.HasChanged`, and the frontend `CrowdLevel`/`Occupancy`/`ClubOccupancy`/`mergeOccupancy`/`ConnectionStatus` signatures match their consumers.

**Placeholder scan:** no TBD/TODO/"add error handling"/"similar to" — every code step shows complete code.
