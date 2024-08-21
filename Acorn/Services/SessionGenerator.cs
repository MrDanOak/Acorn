using Moffat.EndlessOnline.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acorn.Services;
public class SessionGenerator : ISessionGenerator
{
    private Random _rnd;

    public SessionGenerator()
    {
        _rnd = new Random();
    }

    public int Generate() => _rnd.Next(1, (int)EoNumericLimits.SHORT_MAX);
}

public interface ISessionGenerator
{
    int Generate();
}