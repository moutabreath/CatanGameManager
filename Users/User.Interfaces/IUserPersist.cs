using CatanGameManager.CommonObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CatanGameManager.Interfaces.PersistanceInterfaces
{
    public interface ICatanUserPersist
    {
        Task<bool> UpdateUser(UserProfile playerProfile);
        Task<UserProfile> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);

        Task AddPlayerPoints(string userId, int points);

        Task<UserProfile> GetUser(Guid userId);
        Task<List<UserProfile>> SearchUser(string userName);
    }
}
