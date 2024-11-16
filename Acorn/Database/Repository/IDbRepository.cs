using OneOf;
using OneOf.Types;

namespace Acorn.Database.Repository;

public interface IDbRepository<T> : IDbRepository<T, string> where T : class
{
}

public interface IDbRepository<T, TKey> where T : class
{
    Task<OneOf<Success<IEnumerable<T>>, Error>> GetAll();
    Task<OneOf<Success<T>, NotFound, Error>> GetByKey(TKey key);
    Task<OneOf<Success, Error>> CreateAsync(T entity);
    Task<OneOf<Success, Error>> UpdateAsync(T entity);
    Task<OneOf<Success, Error>> DeleteAsync(T entity);
}