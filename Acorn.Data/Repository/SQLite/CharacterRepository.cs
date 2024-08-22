using Dapper;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Data;

namespace Acorn.Data.Repository.SQLite;

public class CharacterRepository : IDbRepository<Character>, IDisposable
{
    private readonly IDbConnection _conn;
    private readonly ILogger<AccountRepository> _logger;

    public CharacterRepository(
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

    public async Task<OneOf<Success, Error>> CreateAsync(Character entity)
    {
        using var t = _conn.BeginTransaction(IsolationLevel.ReadCommitted);
        try
        {
            await _conn.ExecuteAsync("""
            INSERT INTO Characters
            (
                Accounts_Username,
                Name,
                Title,
                Home,
                Fiance,
                Partner,
                Admin,
                Class,
                Gender,
                Race,
                HairStyle,
                HairColor,
                Map,
                X,
                Y,
                Direction,
                Level,
                Exp,
                Hp,
                Tp,
                Str,
                Wis,
                Agi,
                Con,
                Cha,
                StatPoints,
                SkillPoints,
                Karma,
                SitState,
                Hidden,
                NoInteract,
                BankMax,
                GoldBank,
                Usage
            )
            VALUES
            (
                @Accounts_Username,
                @Name,
                @Title,
                @Home,
                @Fiance,
                @Partner,
                @Admin,
                @Class,
                @Gender,
                @Race,
                @HairStyle,
                @HairColor,
                @Map,
                @X,
                @Y,
                @Direction,
                @Level,
                @Exp,
                @Hp,
                @Tp,
                @Str,
                @Wis,
                @Agi,
                @Con,
                @Cha,
                @StatPoints,
                @SkillPoints,
                @Karma,
                @SitState,
                @Hidden,
                @NoInteract,
                @BankMax,
                @GoldBank,
                @Usage
            )
            """, entity);

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
            var character = await _conn.QuerySingleOrDefaultAsync<Character>("SELECT * FROM Characters WHERE Name = @name", new { name });
            if (character is null)
            {
                return new NotFound();
            }
            return new Success<Character>(character);
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
            await _conn.ExecuteAsync("""
            UPDATE Characters
            SET
                Title = @Title,
                Home = @Home,
                Fiance = @Fiance,
                Partner = @Partner,
                Admin = @Admin,
                Class = @Class,
                Gender = @Gender,
                Race = @Race,
                HairStyle = @HairStyle,
                HairColor = @HairColor,
                Map = @Map,
                X = @X,
                Y = @Y,
                Direction = @Direction,
                Level = @Level,
                Exp = @Exp,
                Hp = @Hp,
                Tp = @Tp,
                Str = @Str,
                Wis = @Wis,
                Agi = @Agi,
                Con = @Con,
                Cha = @Cha,
                StatPoints = @StatPoints,
                SkillPoints = @SkillPoints,
                Karma = @Karma,
                SitState = @SitState,
                Hidden = @Hidden,
                NoInteract = @NoInteract,
                BankMax = @BankMax,
                GoldBank = @GoldBank,
                Usage = @Usage
            WHERE Name = @Name
            """, entity);

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
