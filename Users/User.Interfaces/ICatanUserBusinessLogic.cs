using CatanGameManager.CommonObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatanGameManager.Interfaces
{
    public interface ICatanUserBusinessLogic
    {
        Task<bool> RegisterPlayer(UserProfile playerProfile);
        Task<bool> UpdatePlayer(UserProfile playerProfile);
        Task<UserProfile> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);
        Task<List<UserProfile>> SearchUser(string userName);

        Task ConsumeTopic();
        Task<bool> ValidateUser(Guid userId);
    }
}
