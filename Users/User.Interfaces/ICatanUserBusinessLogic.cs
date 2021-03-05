using CatanGameManager.CommonObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces
{
    public interface ICatanUserBusinessLogic
    {
        Task<bool> RegisterPlayer(User playerProfile);
        Task UpdatePlayer(User playerProfile);
        Task<User> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);
        Task<List<User>> SearchUser(string userName);
        
    }
}
