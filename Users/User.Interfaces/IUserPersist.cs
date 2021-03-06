using System;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces.PersistanceInterfaces
{
    public interface ICatanUserPersist
    {
        Task UpdateUser(UserProfile playerProfile);
        Task<UserProfile> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);
    }
}
