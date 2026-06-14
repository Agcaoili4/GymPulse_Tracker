namespace GoodLifePulse.Api.Dtos;

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
    decimal? Longitude);
