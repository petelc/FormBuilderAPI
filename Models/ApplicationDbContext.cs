using System;
using Microsoft.EntityFrameworkCore;

namespace FormBuilderAPI.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // LEC Define DbSets for your entities here
    // LEC Example:
    // LEC public DbSet<YourEntity> YourEntities { get; set; }
    public DbSet<Form> Forms => Set<Form>();
    public DbSet<Domain> Domains => Set<Domain>();
    public DbSet<Forms_Domains> Forms_Domains => Set<Forms_Domains>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Configure your entity mappings here if needed

        modelBuilder.Entity<Forms_Domains>()
            .HasKey(fd => new { fd.FormId, fd.DomainId });

        modelBuilder.Entity<Forms_Domains>()
            .HasOne(fd => fd.Form)
            .WithMany(f => f.Forms_Domains)
            .HasForeignKey(fd => fd.FormId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Forms_Domains>()
            .HasOne(fd => fd.Domain)
            .WithMany(d => d.Forms_Domains)
            .HasForeignKey(fd => fd.DomainId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);
    }
}

