using Acorn.Data.Models;
using OneOf;
using OneOf.Types;

namespace Acorn.Data.Repository.SQLite;

public class CharacterRepository : IRepository<Character>
{
    public Task<OneOf<Success, Error>> CreateAsync(Character entity)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<Success, Error>> DeleteAsync(Character entity)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<Success<Character>, NotFound, Error>> GetByKey(string key)
    {
        throw new NotImplementedException();
    }

    public Task<OneOf<Success, Error>> UpdateAsync(Character entity)
    {
        throw new NotImplementedException();
    }
}
