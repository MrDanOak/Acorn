using Acorn.Data.Models;
using Acorn.Data.Repository;
using Microsoft.Extensions.Logging;

namespace Acorn.Services;
public class StatsReporter : IStatsReporter
{
    private readonly ILogger<StatsReporter> _logger;
    private readonly IRepository<Account> _accountRepository;
    private readonly IDataRepository _dataRepository;

    public StatsReporter(
        ILogger<StatsReporter> logger,
        IRepository<Account> accountRepository,
        IDataRepository dataRepository
    )
    {
        _logger = logger;
        _accountRepository = accountRepository;
        _dataRepository = dataRepository;
    }

    public async Task Report()
    {
        (await _accountRepository.GetAll()).Switch(accounts =>
        {
            _logger.LogInformation("Loaded {Accounts} account(s)", accounts.Value.ToList().Count);
        }, err => { });

        _logger.LogInformation("Loaded {Items} items", _dataRepository.GetItems().Count());
        _logger.LogInformation("Loaded {Npcs} npcs", _dataRepository.GetNpcs().Count());
        _logger.LogInformation("Loaded {Classes} classes", _dataRepository.GetClasses().Count());
        _logger.LogInformation("Loaded {Maps} maps", _dataRepository.GetMaps().Count());
    }
}

public interface IStatsReporter
{
    Task Report();
}