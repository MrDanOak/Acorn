using Moffat.EndlessOnline.SDK.Data;

namespace Acorn.Infrastructure;
public class SessionGenerator : ISessionGenerator
{
    private readonly WorldState _worldState;
    private Random _rnd;

    public SessionGenerator(WorldState worldState)
    {
        _rnd = new Random();
        _worldState = worldState;
    }

    public int Generate()
    {
        var sessionId = 0;
        do {
            sessionId = _rnd.Next(1, (int)EoNumericLimits.SHORT_MAX);
        } while (_worldState.Players.Any(x => x.SessionId == sessionId));
        return sessionId;
    }
}

public interface ISessionGenerator
{
    int Generate();
}