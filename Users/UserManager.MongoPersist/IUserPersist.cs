namespace CatanGameManager.Interfaces.PersistanceInterfaces
{
    public interface ICatanUserPersist
    {
        Task UpdateUser(User playerProfile);
        Task<User> GetUser(string userName, string password);
        Task UnRegisterUser(Guid userId);
    }
}
