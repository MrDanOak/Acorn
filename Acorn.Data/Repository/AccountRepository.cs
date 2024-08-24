using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using System.Data;

namespace Acorn.Data.Repository;
public class AccountRepository : BaseDbRepository, IDbRepository<Account>, IDisposable
{
    private readonly IDbConnection _conn;
    private readonly ILogger<AccountRepository> _logger;
    public static class SQLStatements
    {
        public static string Create = "";
        public static string Update = "";
        public static string GetByKey = "";
        public static string Delete = "";
        public static string GetCharacters = "";
        public static string GetAll = "";
    }

    public AccountRepository(
        IDbConnection conn,
        ILogger<AccountRepository> logger,
        IOptions<DatabaseOptions> options,
        IDbInitialiser initialiser
    ) : base(initialiser)
    {
        _conn = conn;
        _logger = logger;

        SQLStatements.Create = File.ReadAllText($"Database/{options.Value.Engine}/Account/Create.sql");
        SQLStatements.Update = File.ReadAllText($"Database/{options.Value.Engine}/Account/Update.sql");
        SQLStatements.GetByKey = File.ReadAllText($"Database/{options.Value.Engine}/Account/GetByKey.sql");
        SQLStatements.Delete = File.ReadAllText($"Database/{options.Value.Engine}/Account/Delete.sql");
        SQLStatements.GetCharacters = File.ReadAllText($"Database/{options.Value.Engine}/Account/GetCharacters.sql");
        SQLStatements.GetAll = File.ReadAllText($"Database/{options.Value.Engine}/Account/GetAll.sql");

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
            await _conn.ExecuteAsync(SQLStatements.Create, entity);

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
            await _conn.ExecuteAsync(SQLStatements.Delete, new { entity.Username });
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
            var acc = await _conn.QuerySingleOrDefaultAsync<Account>(SQLStatements.GetByKey, new { username });
            if (acc is null)
            {
                return new NotFound();
            }

            acc.Characters = (await _conn.QueryAsync<Character>(SQLStatements.GetCharacters, new { username })).ToList();
            return new Success<Account>(acc);
        }
        catch (Exception e)
        {
            _logger.LogError("Error fetching account {PlayerName}. Exception {Exception}", username, e.Message);
        }

        return new Error();
    }

    public async Task<OneOf<Success<IEnumerable<Account>>, Error>> GetAll()
    {
        try
        {
            var accounts = (await _conn.QueryAsync<Account>(SQLStatements.GetAll)).ToList();
            var withCharacters = accounts.Select(async a =>
            {
                a.Characters = (await _conn.QueryAsync<Character>(SQLStatements.GetCharacters, new { username = a.Username })).ToList();
                return a;
            });

            var results = await Task.WhenAll(withCharacters);
            return new Success<IEnumerable<Account>>(results);
        }
        catch (Exception e)
        {
            _logger.LogError("Error fetching all accounts. Exception {Exception}", e.Message);
        }

        return new Error();
    }

    public async Task<OneOf<Success, Error>> UpdateAsync(Account entity)
    {
        using var t = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await _conn.ExecuteAsync(SQLStatements.Update, entity);
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