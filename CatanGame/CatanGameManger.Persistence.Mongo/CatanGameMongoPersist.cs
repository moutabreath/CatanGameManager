using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using CommonLib.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace CatanGamePersistence.MongoDB
{
    public class CatanGameMongoPersist : CatanEntityMongoPersist<CatanGame>, ICatanGamePersist
    {
        public CatanGameMongoPersist(ILogger<CatanGameMongoPersist> logger, IOptions<MongoConfig> options) : base(logger, options, "CatanGame")
        {
        }

        protected override void InitializeClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(CatanGame)))
            {
                BsonClassMap.RegisterClassMap<CatanGame>(bsonClassMap =>
                {
                    bsonClassMap.AutoMap();
                    bsonClassMap.SetIdMember(bsonClassMap.GetMemberMap(catanGame => catanGame.Id));
                    bsonClassMap.SetIgnoreExtraElements(true);
                });
            }     
        }  

       
        public async Task UpdatePlayerInGame(CatanGame catanGame, ActivePlayer playerToUpdate)
        {
            _logger?.LogDebug($"UpdatePlayerInGame, game: {catanGame.Id}, player: {playerToUpdate.Id}");
            bool doesPlayerExist = catanGame.ActivePlayers.Select(activePlayer => activePlayer.Id == playerToUpdate.Id).
                FirstOrDefault();
            if (!doesPlayerExist)
            {
                _logger?.LogError($"UpdatePlayerInGame. Player is not added into the game. game: {catanGame.Id}, player: {playerToUpdate.Id}");
                return;
            }
            FilterDefinition<CatanGame> filter =  Builders<CatanGame>.Filter.Where(catanGame => catanGame.Id == catanGame.Id 
                                                          && catanGame.ActivePlayers.Any(activePlayer => activePlayer.Id == playerToUpdate.Id));
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(catanGame => catanGame.ActivePlayers[-1], playerToUpdate);
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogDebug($"GetGame: {gameId}");
            IAsyncCursor<CatanGame> catanGameCursor = await MongoCollection.FindAsync(game => game.Id == gameId);
            return catanGameCursor.First();
        }

        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName)
        {
            _logger?.LogDebug($"GetPlayerActiveGames for player: {userName}");
            IAsyncCursor<CatanGame> playerGames = await MongoCollection.FindAsync(game => game.ActivePlayers.Any(activePlayer => activePlayer.UserName == userName));
            return playerGames.ToList();
        }

        public async Task UpdateGame(CatanGame catanGame)
        {
            if (catanGame.Id == Guid.Empty)
            {
                catanGame.Id = Guid.NewGuid();
                catanGame.BanditsDistance = 7;
                catanGame.BanditsStrength = 0;
                catanGame.RecentDiceRolls = new List<Tuple<int, int>>();
            }
            _logger?.LogDebug($"UpdateGame game: {catanGame.Id}");
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(game => game.Id == catanGame.Id);
            await UpdateEntity(catanGame, MongoCollection, filter);
        }

        public async Task RemoveGame(CatanGame catanGame)
        {
            _logger?.LogDebug($"RemoveGame: {catanGame.Id}");
            await MongoCollection.DeleteOneAsync(game => game.Id == catanGame.Id);
        }    

        public async Task DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"DeactivateAllKnights game: {catanGameId}");
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter
                            .Where(catanGame => catanGame.Id == catanGameId // Select the parent document first by its ID
                            && catanGame.ActivePlayers.Any(activePlayer => activePlayer != null));  // Now filter the matching items in the nested array to be updated ONLY

            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update
                .Set(x => x.ActivePlayers[-1].NumOfActiveKnights, 0); // The "-1" index matches ALL the items matching the filter

            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task ActivateAllKnightsForPlayer(Guid catanGameId, Guid playerId)
        {
            _logger?.LogDebug($"ActivateAllKnightsForPlayer, game: {catanGameId}, player: {playerId}");

            ActivePlayer activePlayerToUpdate = MongoCollection.AsQueryable().Where(game => game.Id == catanGameId).FirstOrDefault().
               ActivePlayers.Where(activePlayer => activePlayer.Id == playerId).FirstOrDefault();

            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter
                        .Where(x => x.Id == catanGameId // Select the parent document first by its ID
                        && x.ActivePlayers.Any(y => y != null));  // Now filter the matching items in the nested array to be updated ONLY

            var update = Builders<CatanGame>.Update.Set(x => x.ActivePlayers[-1].NumOfActiveKnights, activePlayerToUpdate.NumOfTotalKnights); // The "-1" index matches ALL the items matching the filter
            await MongoCollection.UpdateOneAsync(filter, update);
        }

        public async Task AdvanceBarbarians(Guid catanGameId)
        {
            _logger?.LogDebug($"AdvanceBarbarians, game: {catanGameId}");
            IAsyncCursor<CatanGame> catanGameCursor = await MongoCollection.FindAsync(game => game.Id == catanGameId);
            CatanGame game = catanGameCursor.FirstOrDefault();
            game.BanditsDistance--;
            game.BanditsDistance %= 7;
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(catanGame => catanGame.Id == catanGameId);            
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(catanGame => catanGame.BanditsDistance, game.BanditsDistance);

            MongoCollection.UpdateOne(filter, update); 
        }     

        public async Task AddPlayerKnight(Guid catanGameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogDebug($"AddPlayerKnight: {catanGameId}, activePlayerId: {activePlayerId}, knightRank: {knightRank}");
            int knightsNumberToAdd = 0;
            switch (knightRank)
            {
                case KnightRank.Basic:
                    knightsNumberToAdd = 1;
                    break;
                case KnightRank.Strong:
                    knightsNumberToAdd = 2;
                    break;
                case KnightRank.Mighty:
                    knightsNumberToAdd = 3;
                    break;
            }

            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(game => game.Id == catanGameId 
                                                                  && game.ActivePlayers.Any(activePlayer => activePlayer.Id == activePlayerId));

            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Inc(x => x.ActivePlayers[-1].NumOfTotalKnights, knightsNumberToAdd);

            await MongoCollection.UpdateOneAsync(filter, update);
        }

     
        public async Task<int> GetTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"GetTotalActiveKnights: {catanGameId}");
            IAsyncCursor<CatanGame> catanGameCursor = await MongoCollection.FindAsync(game => game.Id == catanGameId);
            int totalNumberOfActiveKnights = catanGameCursor.FirstOrDefault().ActivePlayers.Sum(activePlayer => activePlayer.NumOfTotalKnights);
            return totalNumberOfActiveKnights;
        }

        public async Task<int> GetGameTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"GetGameTotalActiveKnights: {catanGameId}");
            IAsyncCursor<CatanGame> catanGameCursor = await MongoCollection.FindAsync(game => game.Id == catanGameId);
            CatanGame catanGame = catanGameCursor.First();
            if (catanGame == null)
            {
                _logger.LogError($"GetGameTotalActiveKnights: couldn't find game at this id:{catanGameId}");
            }
            return catanGame.ActivePlayers.Sum(activePlayer => activePlayer.NumOfActiveKnights);
        }
    }
}
