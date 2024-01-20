using Microsoft.EntityFrameworkCore;

using PassGuardia.Contracts.Models;
using PassGuardia.Domain.DbContexts;

namespace PassGuardia.Domain.Repositories;

public interface IPasswordRepository
{
    Task<Password> CreatePassword(Password password, CancellationToken cancellationToken = default);
    Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default);
    Task<Password> UpdatePasswordVisibility(Password password, CancellationToken cancellationToken = default);
    Task<Audit> CreateAudit(Audit audit, CancellationToken cancellationToken = default);
    Task<List<Audit>> GetAudits(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
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
        _ = await _dbPassword.Passwords.AddAsync(password, cancellationToken);
        _ = await _dbPassword.SaveChangesAsync(cancellationToken);

        return password;
    }

    public Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbPassword.Passwords.SingleOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<Password> UpdatePasswordVisibility(Password password, CancellationToken cancellationToken = default)
    {
        _ = _dbPassword.Update(password);
        _ = await _dbPassword.SaveChangesAsync(cancellationToken);

        return password;
    }

    public async Task<Audit> CreateAudit(Audit audit, CancellationToken cancellationToken = default)
    {
        _ = await _dbPassword.Audits.AddAsync(audit, cancellationToken);
        _ = await _dbPassword.SaveChangesAsync(cancellationToken);

        return audit;
    }

    public Task<List<Audit>> GetAudits(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return _dbPassword.Audits
                    .OrderByDescending(desc => desc.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
    }
}