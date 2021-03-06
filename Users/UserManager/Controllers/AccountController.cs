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
      
        public async Task<bool> RegisterUser(UserProfile user)
        {
            _logger?.LogInformation($"RegisterUser for user:  \"{user.Id}\"");
            return await _catanUserBusinessLogic.RegisterPlayer(user);
        }

        public async Task UpdateUser(UserProfile user)
        {
            _logger?.LogInformation($"UpdatePlayer for user:  \"{user.Id}\"");
            await _catanUserBusinessLogic.UpdatePlayer(user);
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogInformation($"GetUser:  \"{userName}\"");
            return await _catanUserBusinessLogic.GetUser(userName, password);
        }

        public async Task<List<UserProfile>> SearchPlayer(string userName)
        {
            _logger?.LogInformation($"GetAvailableUsers for game admin:  \"{userName}\"");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        public async Task<List<UserProfile>> GetUserActiveGames(string userName)
        {
            _logger?.LogInformation($"GetUserActiveGames for  user:  \"{userName}\"");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogInformation($"UnRegisterUser for user:  \"{userId}\"");
            await _catanUserBusinessLogic.UnRegisterUser(userId);
        }
    }
}