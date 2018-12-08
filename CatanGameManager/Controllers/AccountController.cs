using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatanGameManager.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Account")]
    public class AccountController : Controller
    {

        private readonly ICatanUserBusinessLogic _catanUserBusinessLogic;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, ICatanUserBusinessLogic catanUserBusinessLogic)
        {
            _logger = logger;
            _catanUserBusinessLogic = catanUserBusinessLogic;
        }
      
        public async Task<bool> RegisterUser(PlayerProfile playerProfile)
        {
            _logger?.LogInformation($"RegisterUser for user:  \"{playerProfile.Id}\"");
            return await _catanUserBusinessLogic.RegisterPlayer(playerProfile);
        }

        public async Task UpdatePlayer(PlayerProfile playerProfile)
        {
            _logger?.LogInformation($"UpdatePlayer for user:  \"{playerProfile.Id}\"");
            await _catanUserBusinessLogic.UpdatePlayer(playerProfile);
        }

        public async Task<PlayerProfile> GetUser(string userName, string password)
        {
            _logger?.LogInformation($"GetUser:  \"{userName}\"");
            return await _catanUserBusinessLogic.GetPlayer(userName, password);
        }

        public async Task<List<PlayerProfile>> SearchPlayer(string userName)
        {
            _logger?.LogInformation($"GetAvailableUsers for game admin:  \"{userName}\"");
            return await _catanUserBusinessLogic.SearchPlayer(userName);
        }

        public async Task<List<PlayerProfile>> GetUserActiveGames(string userName)
        {
            _logger?.LogInformation($"GetUserActiveGames for  user:  \"{userName}\"");
            return await _catanUserBusinessLogic.SearchPlayer(userName);
        }

        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogInformation($"UnRegisterUser for user:  \"{userId}\"");
            await _catanUserBusinessLogic.UnRegisterUser(userId);
        }
    }
}