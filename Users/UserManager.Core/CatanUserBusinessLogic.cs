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

        public async Task<bool> RegisterPlayer(UserProfile playerProfile)
        {
            _logger?.LogDebug($"AddPlayer: {playerProfile.Id}");
            UserProfile user = await GetUser(playerProfile.Name, playerProfile.Password);
            if (user != null) return false;

            await _catanGamePersist.UpdateUser(playerProfile);
            return true;
        }

        public async Task UpdatePlayer(UserProfile playerProfile)
        {
            _logger?.LogDebug($"UpdatePlayer: {playerProfile.Name}");
            await _catanGamePersist.UpdateUser(playerProfile);
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetPlayer: {userName}");
            return await _catanGamePersist.GetUser(userName, password);
        }
        
        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser: {userId}");
             await _catanGamePersist.UnRegisterUser(userId);
        }

        public Task<List<UserProfile>> SearchUser(string userName)
        {
            _logger?.LogDebug($"SearchUser: {userName}");
            throw new NotImplementedException();
        } 
    }
}
