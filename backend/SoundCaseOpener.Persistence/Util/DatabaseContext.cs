using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using SoundCaseOpener.Persistence.Model;

namespace SoundCaseOpener.Persistence.Util;

public sealed class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
{
    public const string SchemaName = "SoundCaseOpener";

    public DbSet<User> Users => Set<User>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Case> Cases => Set<Case>();
    public DbSet<Sound> Sounds => Set<Sound>();
    public DbSet<ItemTemplate> ItemTemplates => Set<ItemTemplate>();
    public DbSet<CaseTemplate> CaseTemplates => Set<CaseTemplate>();
    public DbSet<SoundTemplate> SoundTemplates => Set<SoundTemplate>();
    public DbSet<CaseItem> CaseItems => Set<CaseItem>();
    public DbSet<SoundFile> SoundFiles => Set<SoundFile>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(SchemaName);
        
        ConfigureUser(modelBuilder.Entity<User>());
        ConfigureItem(modelBuilder.Entity<Item>());
        ConfigureSound(modelBuilder.Entity<Sound>());
        ConfigureCase(modelBuilder.Entity<Case>());
        ConfigureItemTemplate(modelBuilder.Entity<ItemTemplate>());
        ConfigureCaseTemplate(modelBuilder.Entity<CaseTemplate>());
        ConfigureCaseItem(modelBuilder.Entity<CaseItem>());
        ConfigureSoundTemplate(modelBuilder.Entity<SoundTemplate>());
        ConfigureSoundFile(modelBuilder.Entity<SoundFile>());
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Conventions.Remove<TableNameFromDbSetConvention>();
    }

    private static void ConfigureSoundFile(EntityTypeBuilder<SoundFile> builder)
    {
        builder.HasKey(sf => sf.Id);
    }
    
    private static void ConfigureSoundTemplate(EntityTypeBuilder<SoundTemplate> builder)
    {
        builder.HasOne(st => st.SoundFile)
               .WithMany(sf => sf.SoundTemplates)
               .HasForeignKey(st => st.SoundFileId)
               .OnDelete(DeleteBehavior.Cascade);
    }
    
    private static void ConfigureCaseItem(EntityTypeBuilder<CaseItem> builder)
    {
        builder.HasKey(ci => new { ci.CaseTemplateId, ci.ItemTemplateId });
        builder.HasOne(ci => ci.CaseTemplate)
               .WithMany(ct => ct.ItemTemplates)
               .HasForeignKey(ci => ci.CaseTemplateId)
               .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(ci => ci.ItemTemplate)
               .WithMany(it => it.CaseTemplates)
               .HasForeignKey(ci => ci.ItemTemplateId)
               .OnDelete(DeleteBehavior.Cascade);
    }
    
    private static void ConfigureCaseTemplate(EntityTypeBuilder<CaseTemplate> builder) { }
    
    private static void ConfigureItemTemplate(EntityTypeBuilder<ItemTemplate> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();
        builder.HasMany(i => i.Items)
            .WithOne(i => i.Template)
            .HasForeignKey(i => i.TemplateId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.UseTpcMappingStrategy();
    }
    
    private static void ConfigureCase(EntityTypeBuilder<Case> builder) { }
    
    private static void ConfigureSound(EntityTypeBuilder<Sound> builder) { }
    
    private static void ConfigureItem(EntityTypeBuilder<Item> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();
        builder.HasOne(i => i.Owner)
            .WithMany(u => u.Items)
            .HasForeignKey(i => i.OwnerId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.UseTpcMappingStrategy();
    }
    
    private static void ConfigureUser(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).ValueGeneratedOnAdd();
        builder.HasIndex(u => u.Username).IsUnique();
    }
}
