using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using UserManager.API.Requests;

namespace CatanGameManager.Internal_API.Controllers
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
        public async Task<bool> RegisterUser([FromBody] UserProfile user)
        {
            _logger?.LogDebug($"RegisterUser for user: {user.Id}");
            return await _catanUserBusinessLogic.RegisterPlayer(user);
        }

        [HttpPost]
        public async Task UpdateUser([FromBody] UserProfile user)
        {
            _logger?.LogDebug($"UpdatePlayer for user: {user.Id}");
            await _catanUserBusinessLogic.UpdatePlayer(user);
        }

        [HttpPost]
        public async Task<UserProfile> GetUser([FromBody] GetUserRequest getUserRequest) 
         { 
            _logger?.LogDebug($"GetUser: {getUserRequest.UserName}");
            return await _catanUserBusinessLogic.GetUser(getUserRequest.UserName, getUserRequest.Password);
        }

        [HttpGet]
        public async Task<List<UserProfile>> SearchPlayer(string userName)
        {
            _logger?.LogDebug($"GetAvailableUsers for game admin: {userName}");
            return await _catanUserBusinessLogic.SearchUser(userName);
        }

        [HttpPost]
        public async Task UnRegisterUser([FromBody] Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser for user: {userId}");
            await _catanUserBusinessLogic.UnRegisterUser(userId);
        }
       
    }
}