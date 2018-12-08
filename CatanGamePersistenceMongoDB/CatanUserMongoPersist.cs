﻿using System;
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
    public class CatanUserMongoPersist: CatanEntityMongoPersist<PlayerProfile>, ICatanUserPersist
    {
        private readonly ILogger<CatanUserMongoPersist> _logger;
        private readonly string _playerProfileDoc = "PlayerProfile";
        

        public CatanUserMongoPersist(ILogger<CatanUserMongoPersist> logger, IOptions<CatanManagerConfig> options)
        {
            _logger = logger;
            CatanManagerConfig configuration = options.Value;
            Client = new MongoClient(configuration.MongoConnectionString);
            Database = Client.GetDatabase(configuration.MongoCatanPlayerDbName);
            InitializeClassMap();
        }

        private static void InitializeClassMap()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(PlayerProfile)) == false)
            {
                BsonClassMap.RegisterClassMap<PlayerProfile>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }

        public async Task UpdateUser(PlayerProfile playerProfile)
        {
            if (playerProfile.Id == Guid.Empty) playerProfile.Id = Guid.NewGuid();
            _logger?.LogInformation($"UpdateUser: \"{playerProfile.Id}\" "); 
            await UpdateEntity(playerProfile, 
                Database.GetCollection<PlayerProfile>(_playerProfileDoc), 
                Builders<PlayerProfile>.Filter.Where(x => x.NickName == playerProfile.NickName));
        }

        public async Task<PlayerProfile> GetUser(string userName, string password)
        {
            _logger?.LogInformation($"GetUser:  \"{userName}\" ");
            IMongoCollection<PlayerProfile> playerCollection = Database.GetCollection<PlayerProfile>(_playerProfileDoc);
            var response = await playerCollection.FindAsync(playerProfile => playerProfile.Email == userName && playerProfile.Password == password);
            return response.FirstOrDefault();
        }


        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogInformation($"UnRegisterUser:  \"{userId}\" ");
            IMongoCollection<PlayerProfile> playerCollection = Database.GetCollection<PlayerProfile>(_playerProfileDoc);
            await playerCollection.DeleteOneAsync(playerProfile => playerProfile.Id == userId);
        }
    }
}