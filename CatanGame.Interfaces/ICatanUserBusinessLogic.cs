using CatanGameManager.CommonObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces
{
    public interface ICatanUserBusinessLogic
    {
        Task UpdatePlayer(PlayerProfile playerProfile);
        Task<PlayerProfile> GetPlayer(string userName, string password);
        Task UnRegisterUser(Guid userId);
        Task<List<PlayerProfile>> SearchPlayer(string userName);
    }
}
