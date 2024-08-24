namespace Acorn.Data.Repository;
public abstract class BaseDbRepository
{
    public BaseDbRepository(IDbInitialiser initialiser)
    {
        initialiser.Initialise();
    }
}
