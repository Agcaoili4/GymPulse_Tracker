using GoodLifePulse.Api.Dtos;

namespace GoodLifePulse.Api.Services;

public interface IClubService
{
    Task<PagedResult<ClubDto>> GetClubsAsync(ClubsQuery query, CancellationToken cancellationToken);

    Task<ClubDto?> GetClubByIdAsync(int id, CancellationToken cancellationToken);
}
