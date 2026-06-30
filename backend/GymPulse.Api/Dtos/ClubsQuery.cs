namespace GymPulse.Api.Dtos;

public record ClubsQuery(
    string? Search = null,
    string? City = null,
    string? Province = null,
    int Page = 1,
    int PageSize = 20);
