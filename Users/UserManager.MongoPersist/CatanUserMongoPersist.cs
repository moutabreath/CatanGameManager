using System;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.Config;
using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Text.RegularExpressions;

namespace CatanGamePersistence.MongoDB
{
    public class CatanUserMongoPersist: CatanEntityMongoPersist<UserProfile>, ICatanUserPersist
    {       

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
            _logger?.LogDebug($"UpdateUser: {playerProfile.Id}"); 
            await UpdateEntity(playerProfile,
                MongoCollection, 
                Builders<UserProfile>.Filter.Where(x => x.Id == playerProfile.Id));
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetUser: {userName}");
            IAsyncCursor<UserProfile> response = await MongoCollection.FindAsync(playerProfile => playerProfile.Email == userName && playerProfile.Password == password);
            return response.FirstOrDefault();
        }

        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser: {userId}");
            await MongoCollection.DeleteOneAsync(playerProfile => playerProfile.Id == userId);
        }

        public async Task AddPlayerPoints(string userId, int points)
        {
            _logger?.LogDebug($"UnReAddPlayerPointsgisterUser: {userId}, points: {points}");
            FilterDefinition<UserProfile> filter = Builders<UserProfile>.Filter.Where(userProfile => userProfile.Id.ToString() == userId);
            UpdateDefinition<UserProfile> update = Builders<UserProfile>.Update.Set(userProfile => userProfile.TotalPoints, points);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task<UserProfile> GetUser(Guid userId)
        {
            _logger?.LogDebug($"GetUser: {userId}");
            IAsyncCursor<UserProfile> user = await MongoCollection.FindAsync(user => user.Id == userId);
            return user.FirstOrDefault();            
        }

        public async Task<List<UserProfile>> SearchUser(string userName)
        {
            var queryExpr = new BsonRegularExpression(new Regex(userName, RegexOptions.None));
            var builder = Builders<UserProfile>.Filter;
            FilterDefinition<UserProfile> filter = builder.Regex("UserName", queryExpr);
            IAsyncCursor<UserProfile> filterResult = await MongoCollection.FindAsync<UserProfile>(filter);
            List<UserProfile> users = await filterResult.ToListAsync();
            return users;
        }
    }
}
