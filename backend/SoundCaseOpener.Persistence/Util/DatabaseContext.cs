using SoundCaseOpener.Persistence.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace SoundCaseOpener.Persistence.Util;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public const string SchemaName = "SoundCaseOpener";
    
    public DbSet<Rocket> Rockets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);

        ConfigureRocket(modelBuilder);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }

    private static void ConfigureRocket(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Rocket> rocket = modelBuilder.Entity<Rocket>();
        rocket.HasKey(r => r.Id);
        rocket.Property(r => r.Id).ValueGeneratedOnAdd();
    }
}
