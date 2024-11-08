using Acorn.Data.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using System.Data;

namespace Acorn.Data.Repository;

public class CharacterRepository : BaseDbRepository, IDbRepository<Character>, IDisposable
{
    private readonly IDbConnection _conn;
    private readonly ILogger<AccountRepository> _logger;

    public static class SQLStatements
    {
        public static string Create = "";
        public static string Update = "";
        public static string GetByKey = "";
    }

    public CharacterRepository(
        IDbConnection conn,
        ILogger<AccountRepository> logger,
        IOptions<DatabaseOptions> options,
        IDbInitialiser initialiser
    ) : base(initialiser)
    {
        _conn = conn;
        _logger = logger;

        SQLStatements.Create = File.ReadAllText($"Database/Scripts/{options.Value.Engine}/Character/Create.sql");
        SQLStatements.Update = File.ReadAllText($"Database/Scripts/{options.Value.Engine}/Character/Update.sql");
        SQLStatements.GetByKey = File.ReadAllText($"Database/Scripts/{options.Value.Engine}/Character/GetByKey.sql");

        if (_conn.State != ConnectionState.Open)
        {
            _conn.Open();
        }
    }

    public async Task<OneOf<Success, Error>> CreateAsync(Character entity)
    {
        using var t = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await _conn.ExecuteAsync(SQLStatements.Create, entity);

            t.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError("Error saving character information for {CharacterName}. Exception {Exception}", entity.Name, e.Message);
            t.Rollback();
            return new Error();
        }

        return new Success();
    }

    public Task<OneOf<Success, Error>> DeleteAsync(Character entity)
    {
        throw new NotImplementedException();
    }

    public async Task<OneOf<Success<Character>, NotFound, Error>> GetByKey(string name)
    {
        try
        {
            var character = await _conn.QueryAsync<Character, ItemWithAmount, Character>(
                SQLStatements.GetByKey,
                (character, item) =>
                {
                    if (item != null && character.Inventory.Items.All(x => x.Id != item.Id))
                        character.Inventory.Items.Add(item);

                    return character;
                },
                new { name });

            if (character is null || !character.Any())
            {
                return new NotFound();
            }

            return new Success<Character>(character.Single());
        }
        catch (Exception e)
        {
            _logger.LogError("Error fetching character {Character}. Exception {Exception}", name, e.Message);
        }

        return new Error();
    }

    public async Task<OneOf<Success, Error>> UpdateAsync(Character entity)
    {
        using var t = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await _conn.ExecuteAsync(SQLStatements.Update, entity);

            _logger.LogInformation("Saved character '{Name}'", entity.Name);

            t.Commit();
        }
        catch (Exception e)
        {
            _logger.LogError("Error saving character information for {CharacterName}. Exception {Exception}", entity.Name, e.Message);
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

    public Task<OneOf<Success<IEnumerable<Character>>, Error>> GetAll()
    {
        throw new NotImplementedException();
    }
}
