using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using PassGuardia.Contracts.Models;


namespace PassGuardia.Domain.DbContexts;

public class PasswordDbContext : DbContext
{
    public DbSet<Password> Passwords { get; set; }
    public DbSet<Audit> Audits { get; set; }

    public PasswordDbContext(DbContextOptions<PasswordDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            modelBuilder.Entity(entityType.Name).ToTable(entityType.ClrType.Name);
        }

        modelBuilder.Entity<Password>().HasKey(k => k.Id);
        modelBuilder.Entity<Audit>().HasKey(k => k.Id);
    }
}