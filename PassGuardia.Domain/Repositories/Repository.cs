using Microsoft.EntityFrameworkCore;
using PassGuardia.Contracts.Models;
using PassGuardia.Domain.DbContexts;

namespace PassGuardia.Domain.Repositories;

public interface IRepository
{
    Task<Password> CreatePassword(Password password, CancellationToken cancellationToken = default);
    Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default);
}

public class Repository : IRepository
{
    private readonly PasswordDbContext _dbPassword;

    public Repository(PasswordDbContext dbPassword)
    {
        _dbPassword = dbPassword;
    }

    public async Task<Password> CreatePassword(Password password, CancellationToken cancellationToken = default)
    {
        await _dbPassword.Passwords.AddAsync(password, cancellationToken);
        await _dbPassword.SaveChangesAsync(cancellationToken);

        return password;
    }

    public async Task<Password> GetPasswordById(Guid id, CancellationToken cancellationToken = default)
    {
        var password= await _dbPassword.Passwords.FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

        if(password == null)
        {
            throw new NullReferenceException($"Password doesn't exist by id: {id}");
        }

        return password;
    }
}