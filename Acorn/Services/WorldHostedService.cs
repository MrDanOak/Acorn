using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Acorn.Services;
internal class WorldHostedService : IHostedService
{
    public WorldHostedService(IConfiguration configuration)
    {
        
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // todo: world tick events, npcs moving etc...
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
