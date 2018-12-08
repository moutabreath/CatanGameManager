using CatanGameManager.CommonObjects;
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
        public async Task<bool> RegisterPlayer(PlayerProfile playerProfile)
        {
            _logger?.LogInformation($"AddPlayer:  \"{playerProfile.NickName}\"");
            var user = await GetPlayer(playerProfile.NickName, playerProfile.Password);
            if (user != null) return false;
            await _catanGamePersist.UpdateUser(playerProfile);
            return true;
        }

        public async Task<PlayerProfile> GetPlayer(string userName, string password)
        {
            _logger?.LogInformation($"GetPlayer:  \"{userName}\"");
            return await _catanGamePersist.GetUser(userName, password);
        }
        
        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogInformation($"UnRegisterUser:  \"{userId}\"");
             await _catanGamePersist.UnRegisterUser(userId);
        }

        public Task<List<PlayerProfile>> SearchPlayer(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task UpdatePlayer(PlayerProfile playerProfile)
        {
            _logger?.LogInformation($"UpdatePlayer:  \"{playerProfile.Id}\"");
            await _catanGamePersist.UpdateUser(playerProfile);
        }

     
    }
}
