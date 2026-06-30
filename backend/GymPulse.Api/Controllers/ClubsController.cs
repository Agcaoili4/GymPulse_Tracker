using GymPulse.Api.Dtos;
using GymPulse.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GymPulse.Api.Controllers;

[ApiController]
[Route("api/clubs")]
public class ClubsController : ControllerBase
{
    private readonly IClubService _clubs;

    public ClubsController(IClubService clubs)
    {
        _clubs = clubs;
    }

    [HttpGet]
    public Task<PagedResult<ClubDto>> GetClubs(
        [FromQuery] ClubsQuery query,
        CancellationToken cancellationToken)
    {
        return _clubs.GetClubsAsync(query, cancellationToken);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClubDto>> GetClubById(
        int id,
        CancellationToken cancellationToken)
    {
        var club = await _clubs.GetClubByIdAsync(id, cancellationToken);
        return club is null ? NotFound() : Ok(club);
    }
}
