using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PassGuardia.Contracts.Models;


namespace PassGuardia.Domain.DbContexts;

public class PasswordDbContext : DbContext
{
    private readonly IConfiguration Configuration;

    public PasswordDbContext(IConfiguration configuration,DbContextOptions dbContextOptions) : base(dbContextOptions)
    {
        Configuration = configuration;
    }    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Password>().HasKey(k => k.Id);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Configuration.GetConnectionString("DefaultConnection"));
    }

    public DbSet<Password> Passwords { get; set; }
}