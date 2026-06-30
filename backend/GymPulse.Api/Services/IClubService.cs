using GymPulse.Api.Dtos;

namespace GymPulse.Api.Services;

public interface IClubService
{
    Task<PagedResult<ClubDto>> GetClubsAsync(ClubsQuery query, CancellationToken cancellationToken);

    Task<ClubDto?> GetClubByIdAsync(int id, CancellationToken cancellationToken);
}
