using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces
{
    public interface ICatanUserBusinessLogic
    {
        Task<bool> RegisterPlayer(UserProfile playerProfile);
        Task UpdatePlayer(UserProfile playerProfile);
        Task<UserProfile> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);
        Task<List<UserProfile>> SearchUser(string userName);

        Task ConsumeTopic();
    }
}
