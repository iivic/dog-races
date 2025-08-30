using DogRaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogRaces.Infrastructure.Database.Configurations;

/// <summary>
/// Entity configuration for Bet entity
/// </summary>
public class BetConfiguration : IEntityTypeConfiguration<Bet>
{
    public void Configure(EntityTypeBuilder<Bet> builder)
    {
        builder.ToTable("bets");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.RaceId)
            .IsRequired();

        builder.Property(b => b.TicketId)
            .IsRequired();

        builder.Property(b => b.Selection)
            .IsRequired();

        builder.Property(b => b.BetType)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        builder.Property(b => b.Odds)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(b => b.IsWinning); // Nullable bool

        builder.Property(b => b.CreatedAt)
            .IsRequired();

        // Indexes
        builder.HasIndex(b => b.RaceId);
        builder.HasIndex(b => b.TicketId);
    }
}

