using DogRaces.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DogRaces.Infrastructure.Database.Configurations;

/// <summary>
/// EF Core configuration for GlobalConfiguration entity
/// </summary>
public class GlobalConfigurationConfiguration : IEntityTypeConfiguration<GlobalConfiguration>
{
    public void Configure(EntityTypeBuilder<GlobalConfiguration> builder)
    {
        builder.ToTable("global_configuration");

        // Single row configuration - always ID = 1
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
               .HasColumnName("id")
               .ValueGeneratedOnAdd();

        builder.Property(c => c.MinTicketStake)
               .HasColumnName("min_ticket_stake")
               .HasColumnType("decimal(18,2)")
               .HasDefaultValue(1.00m)
               .IsRequired();

        builder.Property(c => c.MaxTicketWin)
               .HasColumnName("max_ticket_win")
               .HasColumnType("decimal(18,2)")
               .HasDefaultValue(10000.00m)
               .IsRequired();

        builder.Property(c => c.MinNumberOfActiveRounds)
               .HasColumnName("min_number_of_active_rounds")
               .HasDefaultValue(7)
               .IsRequired();

        // Seed default configuration data (single row with ID = 1)
        builder.HasData(new
        {
            Id = 1,
            MinTicketStake = 1.00m,
            MaxTicketWin = 10000.00m,
            MinNumberOfActiveRounds = 7
        });
    }
}