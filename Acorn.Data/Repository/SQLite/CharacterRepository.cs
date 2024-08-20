using Acorn.Data.Models;
using Dapper;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System;
using System.Data;
using System.Diagnostics;
using System.Reflection.Emit;
using System.Reflection;
using System.Security.Claims;
using System.Xml.Linq;

namespace Acorn.Data.Repository.SQLite;

public class CharacterRepository : IRepository<Character>, IDisposable
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
                Sitting,
                Hidden,
                NoInteract,
                BankMax,
                GoldBank,
                Usage,
                Inventory,
                Bank,
                Paperdoll,
                Spells,
                Guild,
                GuildRank,
                GuildRankString,
                Quest
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
                @Sitting,
                @Hidden,
                @NoInteract,
                @BankMax,
                @GoldBank,
                @Usage,
                @Inventory,
                @Bank,
                @Paperdoll,
                @Spells,
                @Guild,
                @GuildRank,
                @GuildRankString,
                @Quest
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

    public Task<OneOf<Success, Error>> UpdateAsync(Character entity)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        if (_conn.State == ConnectionState.Open)
            _conn.Close();
        _conn.Dispose();
    }
}
