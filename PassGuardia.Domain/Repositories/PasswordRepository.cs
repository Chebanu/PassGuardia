using Microsoft.EntityFrameworkCore;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.DbContexts;

namespace PassGuardia.Domain.Repositories;

public interface IPasswordRepository
{
    Task<Password> CreatePassword(Password password, CancellationToken cancellationToken = default);
    Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default);
    Task<Audit> CreateAudit(Audit audit, CancellationToken cancellationToken = default);
    Task<List<Audit>> GetAudits(CancellationToken cancellationToken = default);
}

public class PasswordRepository : IPasswordRepository
{
    private readonly PasswordDbContext _dbPassword;

    public PasswordRepository(PasswordDbContext dbPassword)
    {
        _dbPassword = dbPassword;
    }

    public async Task<Password> CreatePassword(Password password, CancellationToken cancellationToken = default)
    {
        await _dbPassword.Passwords.AddAsync(password, cancellationToken);
        await _dbPassword.SaveChangesAsync(cancellationToken);

        return password;
    }

    public Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbPassword.Passwords.SingleOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Audit> CreateAudit(Audit audit, CancellationToken cancellationToken = default)
    {
        await _dbPassword.Audits.AddAsync(audit, cancellationToken);
        await _dbPassword.SaveChangesAsync(cancellationToken);

        return audit;
    }

    public Task<List<Audit>> GetAudits(CancellationToken cancellationToken = default)
    {
        return _dbPassword.Audits.ToListAsync(cancellationToken);
    }
}