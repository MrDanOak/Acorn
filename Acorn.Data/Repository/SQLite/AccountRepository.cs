using Acorn.Data.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Data;

namespace Acorn.Data.Repository.SQLite;
public class AccountRepository: IRepository<Account>, IDisposable
{
    private readonly IDbConnection _conn;
    private readonly ILogger<AccountRepository> _logger;

    public AccountRepository(
        IDbConnection conn,
        ILogger<AccountRepository> logger
    )
    {
        _conn = conn;
        _logger = logger;

        if (_conn.State != ConnectionState.Open)
        {
            _conn.Open();
        }
    }

    public async Task<OneOf<Success, Error>> CreateAsync(Account entity)
    {
        using var t = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await _conn.ExecuteAsync("""
            INSERT INTO Accounts
            (
                Username,
                Password,
                FullName,
                Location,
                Email,
                Country,
                Created,
                LastUsed
            )
            VALUES
            (
                @Username,
                @Password,
                @FullName,
                @Location,
                @Email,
                @Country,
                @Created,
                @LastUsed
            )
            """, entity);

            t.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError("Error saving account information for {PlayerName}. Exception {Exception}", entity.Username, e.Message);
            t.Rollback();
            return new Error();
        }

        return new Success();
    }

    public async Task<OneOf<Success, Error>> DeleteAsync(Account entity)
    {
        using var t = _conn.BeginTransaction();
        try
        {
            await _conn.ExecuteAsync("""
            DELETE FROM Accounts WHERE Username = @Username
            """, new { entity.Username });
        }
        catch (Exception e)
        {
            _logger.LogError("Error deleting account {PlayerName}. Exception {Exception}", entity.Username, e.Message);
            t.Rollback();
            return new Error();
        }

        return new Success();
    }

    public async Task<OneOf<Success<Account>, NotFound, Error>> GetByKey(string username)
    {
        try
        {
            return await _conn.QuerySingleOrDefaultAsync<Account>("SELECT * FROM Accounts WHERE Username = @username", new { username }) switch
            {
                Account account when account is not null => new Success<Account>(account),
                _ => new NotFound()
            };
        }
        catch (Exception e)
        {
            _logger.LogError("Error fetching account {PlayerName}. Exception {Exception}", username, e.Message);
        }

        return new Error();
    }

    public async Task<OneOf<Success, Error>> UpdateAsync(Account entity)
    {
        using var t = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await _conn.ExecuteAsync("""
            UPDATE Accounts
            SET
                Password = @Password,
                FullName = @FullName,
                Location = @Location,
                Email = @Email,
                Country = @Country,
                Created = @Created,
                LastUsed = @LastUsed
            WHERE Username = @Username
            """, entity);

            t.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError("Error saving account information for {PlayerName}. Exception {Exception}", entity.Username, e.Message);
            t.Rollback();
            return new Error();
        }

        return new Success();
    }

    public void Dispose()
    {
        if (_conn.State == ConnectionState.Open)
            _conn.Close();
        _conn.Dispose();
    }
}