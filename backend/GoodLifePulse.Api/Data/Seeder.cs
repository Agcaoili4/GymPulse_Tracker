using GoodLifePulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GoodLifePulse.Api.Data;

public static class Seeder
{
    // SeedAsync will add new clubs from the seed list and update it
    public static async Task SeedAsync(AppDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var clubs = GetCalgaryAreaClubs();
        var existingClubs = await dbContext.Clubs.ToListAsync();

        foreach (var seed in clubs)
        {
            var club = existingClubs.FirstOrDefault(c =>
                string.Equals(c.Name, seed.Name, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(c.AddressLine1, seed.AddressLine1, StringComparison.OrdinalIgnoreCase) &&
                 string.Equals(c.City, seed.City, StringComparison.OrdinalIgnoreCase)));

            // If no matching club is found, add a new one
            if (club == null)
            {
                dbContext.Clubs.Add(new Club
                {
                    Name = seed.Name,
                    AddressLine1 = seed.AddressLine1,
                    AddressLine2 = seed.AddressLine2,
                    City = seed.City,
                    Province = seed.Province,
                    PostalCode = seed.PostalCode,
                    PhoneNumber = seed.PhoneNumber,
                    IsActive = true,
                    Latitude = seed.Latitude,
                    Longitude = seed.Longitude,
                    CreatedAt = now
                });

                continue;
            }

            // Update exisiting club if any details have changed
            if (ApplySeed(club, seed))
            {
                club.UpdatedAt = now;
            }
        }

        // Deactivate clubs that are not in the seed list
        await dbContext.SaveChangesAsync();
    }

    private static bool ApplySeed(Club club, ClubSeed seed)
    {
        var changed = false;

        changed |= SetIfChanged(club.Name, seed.Name, value => club.Name = value);
        changed |= SetIfChanged(club.AddressLine1, seed.AddressLine1, value => club.AddressLine1 = value);
        changed |= SetIfChanged(club.AddressLine2, seed.AddressLine2, value => club.AddressLine2 = value);
        changed |= SetIfChanged(club.City, seed.City, value => club.City = value);
        changed |= SetIfChanged(club.Province, seed.Province, value => club.Province = value);
        changed |= SetIfChanged(club.PostalCode, seed.PostalCode, value => club.PostalCode = value);
        changed |= SetIfChanged(club.PhoneNumber, seed.PhoneNumber, value => club.PhoneNumber = value);
        changed |= SetIfChanged(club.Latitude, seed.Latitude, value => club.Latitude = value);
        changed |= SetIfChanged(club.Longitude, seed.Longitude, value => club.Longitude = value);

        if (!club.IsActive)
        {
            club.IsActive = true;
            changed = true;
        }

        return changed;
    }

    // Compares the current value with the new value, udpates the value if it has changed
    private static bool SetIfChanged<T>(T currentValue, T newValue, Action<T> setValue)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, newValue))
        {
            return false;
        }

        setValue(newValue);
        return true;
    }

    private static ClubSeed[] GetCalgaryAreaClubs()
    {
        return
        [
            new(
                "GoodLife Fitness Calgary Stephen Avenue",
                "140 8th Avenue SW",
                null,
                "Calgary",
                "AB",
                "T2P 1B3",
                "(587) 538-1900",
                51.046020m,
                -114.065100m),
            new(
                "GoodLife Fitness Calgary Mount Royal Village",
                "880 16th Avenue SW, Unit 200",
                null,
                "Calgary",
                "AB",
                "T2R 1J9",
                "(403) 802-1102",
                51.038470m,
                -114.081590m),
            new(
                "GoodLife Fitness Calgary Richmond Square",
                "50-3915 51 Street SW",
                null,
                "Calgary",
                "AB",
                "T3E 6N1",
                "(403) 286-3481",
                51.020280m,
                -114.162840m),
            new(
                "GoodLife Fitness Calgary Deerfoot City",
                "#2120, 901 64th Avenue NE",
                null,
                "Calgary",
                "AB",
                "T2E 7P4",
                "(587) 538-1700",
                51.108660m,
                -114.041660m),
            new(
                "GoodLife Fitness Calgary Northland Village",
                "Unit 2230, 5235 Northland Drive NW",
                null,
                "Calgary",
                "AB",
                "T2L 2J8",
                "(403) 247-2121",
                51.100000m,
                -114.143080m),
            new(
                "GoodLife Fitness Calgary Sunridge",
                "2929 Sunridge Way",
                null,
                "Calgary",
                "AB",
                "T1Y 7K7",
                "(403) 291-0259",
                51.068520m,
                -113.933740m),
            new(
                "GoodLife Fitness Calgary Westwinds and Castleridge",
                "3633 Westwinds Dr NE",
                null,
                "Calgary",
                "AB",
                "T3J 5K3",
                "(403) 590-0519",
                51.109940m,
                -113.971670m),
            new(
                "GoodLife Fitness Calgary Trinity Hills at Olympic Park",
                "2200 Na'a Drive SW",
                null,
                "Calgary",
                "AB",
                "T3B 2S6",
                "(403) 288-8571",
                51.081760m,
                -114.206920m),
            new(
                "GoodLife Fitness Calgary Canyon Meadows",
                "13226 MacLeod Trail SE",
                null,
                "Calgary",
                "AB",
                "T2J 7E5",
                "(403) 271-4348",
                50.932640m,
                -114.066630m),
            new(
                "GoodLife Fitness Calgary Country Village and Harvest Hills",
                "100 Country Village Road NE",
                null,
                "Calgary",
                "AB",
                "T3K 5Z2",
                "(403) 226-3949",
                51.162250m,
                -114.069510m),
            new(
                "GoodLife Fitness Calgary Creekside",
                "12330 Symons Valley Rd NW",
                null,
                "Calgary",
                "AB",
                "T3P 0A3",
                "(403) 274-0397",
                51.164825m,
                -114.129577m),
            new(
                "GoodLife Fitness Calgary Shepard and 130th SE",
                "4916 130 Avenue SE",
                null,
                "Calgary",
                "AB",
                "T2Z 0B2",
                "(403) 232-1144",
                50.934300m,
                -113.963090m),
            new(
                "GoodLife Fitness Calgary McKenzie Towne Centre",
                "18 McKenzie Towne Gate S.E.",
                null,
                "Calgary",
                "AB",
                "T2Z 0N2",
                "(403) 726-9408",
                50.916180m,
                -113.962330m),
            new(
                "GoodLife Fitness Airdrie Towerlane Centre",
                "303-505 Main Street SW",
                null,
                "Airdrie",
                "AB",
                "T4B 3K3",
                "(587) 360-1559",
                51.294140m,
                -114.016520m),
            new(
                "GoodLife Fitness Cochrane Points West",
                "210 5th Avenue West",
                null,
                "Cochrane",
                "AB",
                "T4C 2G4",
                "(403) 851-9292",
                51.187830m,
                -114.475560m)
        ];
    }

    private sealed record ClubSeed(
        string Name,
        string AddressLine1,
        string? AddressLine2,
        string City,
        string Province,
        string PostalCode,
        string PhoneNumber,
        decimal Latitude,
        decimal Longitude);
}
