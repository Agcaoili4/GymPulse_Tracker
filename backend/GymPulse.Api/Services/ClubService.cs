using GymPulse.Api.Data;
using GymPulse.Api.Dtos;
using GymPulse.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GymPulse.Api.Services;

public class ClubService : IClubService
{
    private const int MaxPageSize = 100;

    private readonly AppDbContext _db;

    public ClubService(AppDbContext db)
    {
        _db = db;
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

        var items = await clubs
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => ToDto(c))
            .ToListAsync(cancellationToken);

        return new PagedResult<ClubDto>(items, page, pageSize, totalCount);
    }

    public async Task<ClubDto?> GetClubByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.Clubs
            .AsNoTracking()
            .Where(c => c.Id == id && c.IsActive)
            .Select(c => ToDto(c))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static ClubDto ToDto(Club c) =>
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
            c.Longitude);
}
