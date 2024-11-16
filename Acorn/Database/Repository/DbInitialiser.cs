using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;

namespace Acorn.Database.Repository;

public class DbInitialiser : IDbInitialiser
{
    private readonly DatabaseOptions _options;

    public DbInitialiser(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public void Initialise()
    {
        if (_options.Engine?.ToLower() != "sqlite")
        {
            return;
        }

        if (File.Exists("Acorn.db"))
        {
            return;
        }

        var sqliteConnection = new SqliteConnection(_options.ConnectionString);
        sqliteConnection.Open();
        using var command = sqliteConnection.CreateCommand();
        command.CommandText = File.ReadAllText($"Database/Scripts/{_options.Engine}/Init.sql");
        command.ExecuteNonQuery();
        sqliteConnection.Close();
    }
}

public interface IDbInitialiser
{
    void Initialise();
}