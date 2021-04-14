using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatanGameManager.API.Controllers
{
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly ICatanUserBusinessLogic _catanUserBusinessLogic;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger, ICatanUserBusinessLogic catanUserBusinessLogic)
        {
            _logger = logger;
            _catanUserBusinessLogic = catanUserBusinessLogic;
        }     

        [HttpPost]
        public async Task<bool> RegisterUser(UserProfile user)
        {
            _logger?.LogDebug($"RegisterUser for user: {user.Id}");
            return await _catanUserBusinessLogic.RegisterPlayer(user);
        }

        [HttpPost]
        public async Task UpdateUser(UserProfile user)
        {
            _logger?.LogDebug($"UpdatePlayer for user: {user.Id}");
            await _catanUserBusinessLogic.UpdatePlayer(user);
        }

        [HttpGet]
        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetUser: {userName}");
            return await _catanUserBusinessLogic.GetUser(userName, password);
        }

        [HttpGet]
        public async Task<List<UserProfile>> SearchPlayer(string userName)
        {
            _logger?.LogDebug($"GetAvailableUsers for game admin: {userName}");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        [HttpGet]
        public async Task<List<UserProfile>> GetUserActiveGames(string userName)
        {
            _logger?.LogDebug($"GetUserActiveGames for user: {userName}");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        [HttpPost]
        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser for user: {userId}");
            await _catanUserBusinessLogic.UnRegisterUser(userId);
        }

        public async Task<bool> ValidateUser(Guid userId)
        {
            _logger?.LogDebug($"ValidateUser for user: {userId}");
            return await _catanUserBusinessLogic.ValidateUser(userId);
        }
    }
}