using DogRaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogRaces.Infrastructure.Database.Configurations;

/// <summary>
/// Entity configuration for Ticket entity
/// </summary>
public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        builder.ToTable("tickets");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .ValueGeneratedOnAdd();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>(); // Store enum as string

        builder.Property(t => t.TotalStake)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.TotalPayout)
            .HasColumnType("decimal(18,2)");

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.Property(t => t.ProcessedAt);

        // Relationships
        builder.HasMany(t => t.Bets)
            .WithOne(b => b.Ticket)
            .HasForeignKey(b => b.TicketId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

