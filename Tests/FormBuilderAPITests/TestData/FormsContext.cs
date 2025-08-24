using Microsoft.EntityFrameworkCore;


namespace FormBuilderAPITests.TestData;

public class FormsContext : DbContext
{
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<Domain> Domains => Set<Domain>();
    public DbSet<Forms_Domains> Forms_Domains => Set<Forms_Domains>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=FormBuilderTests.db");
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        DataGenerator.FakeData.Init(10);
        
        modelBuilder.Entity<Forms_Domains>().HasData();
    }
}