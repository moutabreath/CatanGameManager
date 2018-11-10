using System;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces.PersistanceInterfaces
{
    public interface ICatanUserPersist
    {
        Task UpdateUser(PlayerProfile playerProfile);
        Task<PlayerProfile> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);
    }
}
