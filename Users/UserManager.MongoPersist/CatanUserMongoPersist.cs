using System;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using System.Collections.Generic;
using MongoDB.Bson;
using System.Text.RegularExpressions;
using CommonLib.Config;

namespace CatanGamePersistence.MongoDB
{
    public class CatanUserMongoPersist(ILogger<CatanUserMongoPersist> logger, IOptions<MongoConfig> options) : 
        CatanEntityMongoPersist<UserProfile>(logger, options, options.Value.MongoPlayerDocumentName), ICatanUserPersist
    {
        protected override void InitializeClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(UserProfile)))
            {
                BsonClassMap.RegisterClassMap<UserProfile>(classMap =>
                {
                    classMap.AutoMap();
                    classMap.SetIdMember(classMap.GetMemberMap(playerProfile => playerProfile.Id));
                    classMap.SetIgnoreExtraElements(true);
                });
            }
        }

        public async Task<bool> UpdateUser(UserProfile playerProfile)
        {
            if (playerProfile.Id == Guid.Empty) playerProfile.Id = Guid.NewGuid();

            _logger?.LogDebug($"UpdateUser: {playerProfile.Id}"); 

            return await UpdateEntity(playerProfile,
                MongoCollection, 
                Builders<UserProfile>.Filter.Where(userProfile => userProfile.Id == playerProfile.Id));
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetUser: {userName}");
            var debug = MongoCollection.Find(playerProfile => playerProfile.Email == userName && playerProfile.Password == password).FirstOrDefault();
            return await
                (await MongoCollection.FindAsync(playerProfile => playerProfile.Email == userName && playerProfile.Password == password))
                .FirstOrDefaultAsync();
        }

        public async Task<bool> UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser: {userId}");
            DeleteResult deleteResult = await MongoCollection.DeleteOneAsync(playerProfile => playerProfile.Id == userId);
            if (deleteResult != null)
            {
                return deleteResult.IsAcknowledged;
            }
            return false;
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
            return await (await MongoCollection.FindAsync(user => user.Id == userId)).FirstOrDefaultAsync();
        }

        public async Task<List<UserProfile>> SearchUser(string userName)
        {
            var queryExpr = new BsonRegularExpression(new Regex(userName, RegexOptions.None));
            var builder = Builders<UserProfile>.Filter;
            FilterDefinition<UserProfile> filter = builder.Regex("UserName", queryExpr);
            return await (await MongoCollection.FindAsync<UserProfile>(filter)).ToListAsync();
        }
    }
}
