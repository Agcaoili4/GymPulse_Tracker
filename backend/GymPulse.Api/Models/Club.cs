// Class Models for Club

namespace GymPulse.Api.Models
{
    public class Club
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string AddressLine1 { get; set; }
        // Optional value, meaning it can be null
        public string? AddressLine2 { get; set; }
        public required string City { get; set; }
        public required string Province { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

    }
}