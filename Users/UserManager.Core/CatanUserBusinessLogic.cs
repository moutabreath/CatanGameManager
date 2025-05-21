using CatanGameManager.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using Confluent.Kafka;
using CatanGameManager.CommonObjects;

namespace CatanGameManager.Core
{
    public class CatanUserBusinessLogic(ILogger<CatanUserBusinessLogic> logger, ICatanUserPersist catanUserPersist) : ICatanUserBusinessLogic
    {
        public async Task<bool> RegisterPlayer(UserProfile playerProfile)
        {
            logger?.LogDebug($"AddPlayer: {playerProfile.Id}");
            UserProfile user = await GetUser(playerProfile.Email, playerProfile.Password);
            if (user != null)
            {
                logger?.LogWarning($"AddPlayer: Tried to re-register an existing user {playerProfile.Id}");
                return false;
            }

            return await catanUserPersist.UpdateUser(playerProfile);
        }

        public async Task<bool> UpdatePlayer(UserProfile playerProfile)
        {
            logger?.LogDebug($"UpdatePlayer: {playerProfile.Name}");
            return await catanUserPersist.UpdateUser(playerProfile);
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            logger?.LogDebug($"GetPlayer: {userName}");
            return await catanUserPersist.GetUser(userName, password);
        }
        
        public async Task<bool> UnRegisterUser(Guid userId)
        {
            logger?.LogDebug($"UnRegisterUser: {userId}");
             return await catanUserPersist.UnRegisterUser(userId);
        }

        public async Task<List<UserProfile>> SearchUser(string userName)
        {
            logger?.LogDebug($"SearchUser: {userName}");
            return await catanUserPersist.SearchUser(userName);
        }

        public async Task ConsumeTopic()
        {
            logger?.LogDebug($"ConsumeTopic");
            var config = new ConsumerConfig
            {
                BootstrapServers = "kafka-server:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            IList<string> topics =
            [
                "player-points"
            ];
            using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
            consumer.Subscribe(topics);
            var consumeResult = consumer.Consume(5000);
            await catanUserPersist.AddPlayerPoints(consumeResult.Message.Value, 50);
        }

        public async Task<bool> ValidateUser(Guid userId)
        {
            logger?.LogDebug($"ValidateUser: {userId}");
            UserProfile userProfile =  await catanUserPersist.GetUser(userId);
            return userProfile != null;
        }
    }
    
}
