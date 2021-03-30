using System.Threading;
using System.Threading.Tasks;
using CatanGameManager.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace UserManager.API.Controllers
{
    public class UserHostedService : BackgroundService
    {

        private readonly ICatanUserBusinessLogic _catanUserBusinessLogic;
        private readonly ILogger<UserHostedService> _logger;

        public UserHostedService(ILogger<UserHostedService> logger, ICatanUserBusinessLogic catanUserBusinessLogic)
        {
            _logger = logger;
            _catanUserBusinessLogic = catanUserBusinessLogic;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug("ExecuteAsync");
            while (!stoppingToken.IsCancellationRequested)
            {
                await _catanUserBusinessLogic.ConsumeTopic().ConfigureAwait(false);
            }         
        }
    }
}
