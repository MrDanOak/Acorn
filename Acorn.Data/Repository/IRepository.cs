using OneOf;
using OneOf.Types;

namespace Acorn.Data.Repository;
public interface IRepository<T> where T : class
{
    Task<OneOf<Success<T>, NotFound, Error>> GetByKey(string key);
    Task<OneOf<Success, Error>> CreateAsync(T entity);
    Task<OneOf<Success, Error>> UpdateAsync(T entity);
    Task<OneOf<Success, Error>> DeleteAsync(T entity);
}