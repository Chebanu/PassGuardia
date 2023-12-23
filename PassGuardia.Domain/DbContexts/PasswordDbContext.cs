using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using PassGuardia.Contracts.Models;

namespace PassGuardia.Domain.DbContexts;

public class PasswordDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<Password> Passwords { get; set; }
    public DbSet<Audit> Audits { get; set; }

    public PasswordDbContext(DbContextOptions<PasswordDbContext> dbContextOptions) : base(dbContextOptions)
    {
    }
}