using Microsoft.EntityFrameworkCore;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.DbContexts;

namespace PassGuardia.Domain.Repositories;

public interface IRepository
{
    Task<Password> CreatePassword(string hashedPassword, string iv, CancellationToken cancellationToken = default);
    Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default);
}

public class Repository : IRepository
{
    private readonly PasswordDbContext _dbPassword;

    public Repository(PasswordDbContext dbPassword)
    {
        _dbPassword = dbPassword;
    }

    public async Task<Password> CreatePassword(string hashedPassword, string iv, CancellationToken cancellationToken = default)
    {
        var password = new Password
        {
            Id = Guid.NewGuid(),
            EncryptedPassword = hashedPassword,
            IV = iv
        };

        await _dbPassword.Passwords.AddAsync(password, cancellationToken);
        await _dbPassword.SaveChangesAsync(cancellationToken);

        return password;
    }

    public Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default)
    {
        return _dbPassword.Passwords.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }
}