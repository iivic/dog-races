using DogRaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogRaces.Infrastructure.Database.Configurations;

/// <summary>
/// Entity configuration for Race entity
/// </summary>
public class RaceConfiguration : IEntityTypeConfiguration<Race>
{
    public void Configure(EntityTypeBuilder<Race> builder)
    {
        builder.ToTable("Races");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .ValueGeneratedOnAdd();

        builder.Property(r => r.StartTime)
            .IsRequired();

        builder.Property(r => r.EndTime)
            .IsRequired();

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(r => r.Result)
            .HasConversion(
                v => v != null ? string.Join(",", v) : null,
                v => !string.IsNullOrEmpty(v) ? v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToArray() : null
            );

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.ResultDeterminedAt).IsRequired(false);
        builder.Property(r => r.ResultPublishedAt).IsRequired(false);

        // Relationships
        builder.HasMany(r => r.Bets)
            .WithOne(b => b.Race)
            .HasForeignKey(b => b.RaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(r => r.RaceOdds)
            .WithOne(ro => ro.Race)
            .HasForeignKey(ro => ro.RaceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

