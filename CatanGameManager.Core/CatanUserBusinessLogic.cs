using CatanGameManager.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces.PersistanceInterfaces;

namespace CatanGameManager.Core
{
    public class CatanUserBusinessLogic : ICatanUserBusinessLogic
    {
        private readonly ILogger<CatanUserBusinessLogic> _logger;
        private readonly ICatanUserPersist _catanGamePersist;

        public CatanUserBusinessLogic(ILogger<CatanUserBusinessLogic> logger, ICatanUserPersist catanGamePersist)
        {
            _logger = logger;
            _catanGamePersist = catanGamePersist;
        }

        public async Task<bool> RegisterPlayer(User playerProfile)
        {
            _logger?.LogInformation($"AddPlayer: {playerProfile.Id}");
            User user = await GetUser(playerProfile.Name, playerProfile.Password);
            if (user != null) return false;

            await _catanGamePersist.UpdateUser(playerProfile);
            return true;
        }

        public async Task UpdatePlayer(User playerProfile)
        {
            _logger?.LogInformation($"UpdatePlayer: {playerProfile.Name}");
            await _catanGamePersist.UpdateUser(playerProfile);
        }

        public async Task<User> GetUser(string userName, string password)
        {
            _logger?.LogInformation($"GetPlayer: {userName}");
            return await _catanGamePersist.GetUser(userName, password);
        }
        
        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogInformation($"UnRegisterUser: {userId}");
             await _catanGamePersist.UnRegisterUser(userId);
        }

        public Task<List<User>> SearchUser(string userName)
        {
            _logger?.LogInformation($"SearchUser: {userName}");
            throw new NotImplementedException();
        } 
    }
}
