using OneOf;
using OneOf.Types;

namespace Acorn.Data.Repository;

public interface IRepository<T> : IRepository<T, string> where T : class
{ }

public interface IRepository<T, TKey> where T : class
{
    Task<OneOf<Success<IEnumerable<T>>, Error>> GetAll();
    Task<OneOf<Success<T>, NotFound, Error>> GetByKey(TKey key);
    Task<OneOf<Success, Error>> CreateAsync(T entity);
    Task<OneOf<Success, Error>> UpdateAsync(T entity);
    Task<OneOf<Success, Error>> DeleteAsync(T entity);
}