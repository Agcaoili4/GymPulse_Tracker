using Microsoft.EntityFrameworkCore;
using GymPulse.Api.Models;

namespace GymPulse.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Club> Clubs => Set<Club>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // API configuration for Club entities
        modelBuilder.Entity<Club>(entity =>
        {
            entity.Property(c => c.Name).HasMaxLength(200).IsRequired();
            entity.Property(c => c.AddressLine1).HasMaxLength(200).IsRequired();
            entity.Property(c => c.AddressLine2).HasMaxLength(200);
            entity.Property(c => c.City).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Province).HasMaxLength(50).IsRequired();
            entity.Property(c => c.PostalCode).HasMaxLength(20);
            entity.Property(c => c.PhoneNumber).HasMaxLength(30);
            entity.Property(c => c.Latitude).HasPrecision(9, 6);
            entity.Property(c => c.Longitude).HasPrecision(9, 6);

            entity.HasIndex(c => c.City);
            entity.HasIndex(c => c.Province);
            entity.HasIndex(c => c.IsActive);
        });
    }
}