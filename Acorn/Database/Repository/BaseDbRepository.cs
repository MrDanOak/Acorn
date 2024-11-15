namespace Acorn.Database.Repository;

public abstract class BaseDbRepository
{
    public BaseDbRepository(IDbInitialiser initialiser)
    {
        initialiser.Initialise();
    }
}