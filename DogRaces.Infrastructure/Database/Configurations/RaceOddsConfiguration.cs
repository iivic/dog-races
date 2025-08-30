using DogRaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogRaces.Infrastructure.Database.Configurations;

/// <summary>
/// Entity configuration for RaceOdds entity
/// </summary>
public class RaceOddsConfiguration : IEntityTypeConfiguration<RaceOdds>
{
    public void Configure(EntityTypeBuilder<RaceOdds> builder)
    {
        builder.ToTable("race_odds");

        builder.HasKey(ro => ro.Id);
        builder.Property(ro => ro.Id)
            .ValueGeneratedOnAdd();

        builder.Property(ro => ro.RaceId)
            .IsRequired();

        builder.Property(ro => ro.Selection)
            .IsRequired();

        builder.Property(ro => ro.Odds)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(ro => ro.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(ro => new { ro.RaceId, ro.Selection })
            .IsUnique();
    }
}

