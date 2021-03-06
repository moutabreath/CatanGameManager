using System;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.Config;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace CatanGamePersistence.MongoDB
{
    public class CatanUserMongoPersist: CatanEntityMongoPersist<UserProfile>, ICatanUserPersist
    {
        private readonly string _playerProfileDoc = "PlayerProfile";
        

        public CatanUserMongoPersist(ILogger<CatanUserMongoPersist> logger, IOptions<CatanManagerConfig> options): base(logger, options, "PlayerProfile")
        {   
        }

        protected override void InitializeClassMap()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(UserProfile)) == false)
            {
                BsonClassMap.RegisterClassMap<UserProfile>(classMap =>
                {
                    classMap.AutoMap();
                    classMap.SetIdMember(classMap.GetMemberMap(playerProfile => playerProfile.Id));
                    classMap.SetIgnoreExtraElements(true);
                });
            }
        }

        public async Task UpdateUser(UserProfile playerProfile)
        {
            if (playerProfile.Id == Guid.Empty) playerProfile.Id = Guid.NewGuid();
            _logger?.LogInformation($"UpdateUser: \"{playerProfile.Id}\" "); 
            await UpdateEntity(playerProfile, 
                Database.GetCollection<UserProfile>(_playerProfileDoc), 
                Builders<UserProfile>.Filter.Where(x => x.Id == playerProfile.Id));
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogInformation($"GetUser: \"{userName}\" ");
            IMongoCollection<UserProfile> playerCollection = Database.GetCollection<UserProfile>(_playerProfileDoc);
            IAsyncCursor<UserProfile> response = await playerCollection.FindAsync(playerProfile => playerProfile.Email == userName && playerProfile.Password == password);
            return response.FirstOrDefault();
        }

        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogInformation($"UnRegisterUser: \"{userId}\" ");
            IMongoCollection<UserProfile> playerCollection = Database.GetCollection<UserProfile>(_playerProfileDoc);
            await playerCollection.DeleteOneAsync(playerProfile => playerProfile.Id == userId);
        }
    }
}
