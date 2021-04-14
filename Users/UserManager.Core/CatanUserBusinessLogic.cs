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
        private readonly ICatanUserPersist _catanGamePersist;

        public CatanUserBusinessLogic(ILogger<CatanUserBusinessLogic> logger, ICatanUserPersist catanGamePersist)
        {
            _logger = logger;
            _catanGamePersist = catanGamePersist;
        }

        public async Task<bool> RegisterPlayer(UserProfile playerProfile)
        {
            _logger?.LogDebug($"AddPlayer: {playerProfile.Id}");
            UserProfile user = await GetUser(playerProfile.Name, playerProfile.Password);
            if (user != null) return false;

            await _catanGamePersist.UpdateUser(playerProfile);
            return true;
        }

        public async Task UpdatePlayer(UserProfile playerProfile)
        {
            _logger?.LogDebug($"UpdatePlayer: {playerProfile.Name}");
            await _catanGamePersist.UpdateUser(playerProfile);
        }

        public async Task<UserProfile> GetUser(string userName, string password)
        {
            _logger?.LogDebug($"GetPlayer: {userName}");
            return await _catanGamePersist.GetUser(userName, password);
        }
        
        public async Task UnRegisterUser(Guid userId)
        {
            _logger?.LogDebug($"UnRegisterUser: {userId}");
             await _catanGamePersist.UnRegisterUser(userId);
        }

        public async Task<List<UserProfile>> SearchUser(string userName)
        {
            _logger?.LogDebug($"SearchUser: {userName}");
            return await _catanGamePersist.SearchUser(userName);
        }

        public async Task ConsumeTopic()
        {
            _logger?.LogDebug($"ConsumeTopic");
            var config = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "foo",
                AutoOffsetReset = AutoOffsetReset.Earliest
            };
            IList<string> topics = new List<string>
            {
                "player-points"
            };
            using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
            {
                consumer.Subscribe(topics);
                var consumeResult = consumer.Consume(5000);
                await _catanGamePersist.AddPlayerPoints(consumeResult.Message.Value, 50);
                
            }
        }

        public async Task<bool> ValidateUser(Guid userId)
        {
            _logger?.LogDebug($"ValidateUser: {userId}");
            UserProfile userProfile =  await _catanGamePersist.GetUser(userId);
            return userProfile != null;
        }
    }
    
}
