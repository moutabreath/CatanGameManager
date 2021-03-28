using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces;
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
            _logger?.LogDebug($"RegisterUser for user: {user.Id}");
            return await _catanUserBusinessLogic.RegisterPlayer(user);
        }

        public async Task UpdateUser(UserProfile user)
        {
            _logger?.LogDebug($"UpdatePlayer for user: {user.Id}");
            await _catanUserBusinessLogic.UpdatePlayer(user);
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetUser: {userName}");
            return await _catanUserBusinessLogic.GetUser(userName, password);
        }

        public async Task<List<UserProfile>> SearchPlayer(string userName)
        {
            _logger?.LogDebug($"GetAvailableUsers for game admin: {userName}");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        public async Task<List<UserProfile>> GetUserActiveGames(string userName)
        {
            _logger?.LogDebug($"GetUserActiveGames for user: {userName}");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser for user: {userId}");
            await _catanUserBusinessLogic.UnRegisterUser(userId);
        }

    }
}