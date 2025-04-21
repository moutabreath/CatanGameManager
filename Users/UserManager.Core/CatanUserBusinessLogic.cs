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
    public class CatanUserBusinessLogic : ICatanUserBusinessLogic
    {
        private readonly ILogger<CatanUserBusinessLogic> _logger;
        private readonly ICatanUserPersist catanUserPersist;

        public CatanUserBusinessLogic(ILogger<CatanUserBusinessLogic> logger, ICatanUserPersist catanUserPersist)
        {
            _logger = logger;
            this.catanUserPersist = catanUserPersist;
        }

        public async Task<bool> RegisterPlayer(UserProfile playerProfile)
        {
            _logger?.LogDebug($"AddPlayer: {playerProfile.Id}");
            UserProfile user = await GetUser(playerProfile.Name, playerProfile.Password);
            if (user != null) return false;

            await catanUserPersist.UpdateUser(playerProfile);
            return true;
        }

        public async Task UpdatePlayer(UserProfile playerProfile)
        {
            _logger?.LogDebug($"UpdatePlayer: {playerProfile.Name}");
            await catanUserPersist.UpdateUser(playerProfile);
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetPlayer: {userName}");
            return await catanUserPersist.GetUser(userName, password);
        }
        
        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser: {userId}");
             await catanUserPersist.UnRegisterUser(userId);
        }

        public async Task<List<UserProfile>> SearchUser(string userName)
        {
            _logger?.LogDebug($"SearchUser: {userName}");
            return await catanUserPersist.SearchUser(userName);
        }

        public async Task ConsumeTopic()
        {
            _logger?.LogDebug($"ConsumeTopic");
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
            _logger?.LogDebug($"ValidateUser: {userId}");
            UserProfile userProfile =  await catanUserPersist.GetUser(userId);
            return userProfile != null;
        }
    }
    
}
