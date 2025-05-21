using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using CatanGamePersistence.MongoDB;
using CommonLib.Config;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CatanGameManger.Persistence.MongoDB
{
    public class CatanGameMongoPersist(ILogger<CatanGameMongoPersist> logger, IOptions<MongoConfig> options) :
        CatanEntityMongoPersist<CatanGame>(logger, options, options.Value.MongoGameDocumentName), ICatanGamePersist
    {
        protected override void InitializeClassMap()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(CatanGame)))
            {
                BsonClassMap.RegisterClassMap<CatanGame>(classMap =>
                {
                    classMap.AutoMap();
                    classMap.SetIdMember(classMap.GetMemberMap(catanGame => catanGame.Id));
                    classMap.SetIgnoreExtraElements(true);
                });
            }
        }


        public async Task<bool> UpdatePlayerInGame(CatanGame catanGame, ActivePlayer playerToUpdate)
        {
            _logger?.LogDebug($"UpdatePlayerInGame, game: {catanGame.Id}, player: {playerToUpdate.Id}");

            bool doesPlayerExist = catanGame.ActivePlayers.Select(activePlayer => activePlayer.Id == playerToUpdate.Id).
                FirstOrDefault();
            if (!doesPlayerExist)
            {
                _logger?.LogError($"UpdatePlayerInGame. Player is not added into the game. game: {catanGame.Id}, player: {playerToUpdate.Id}");
                return false;
            }
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(catanGame => catanGame.Id == catanGame.Id
                                                          && catanGame.ActivePlayers.Any(activePlayer => activePlayer.Id == playerToUpdate.Id));
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(catanGame => catanGame.ActivePlayers[-1], playerToUpdate);
            UpdateResult result = await MongoCollection.UpdateOneAsync(filter, update);
            if (result != null)
            {
                return result.IsAcknowledged;
            }
            return false;
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogDebug($"GetGame: {gameId}");
            return (await MongoCollection.FindAsync(game => game.Id == gameId)).First();
        }

        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName)
        {
            _logger?.LogDebug($"GetPlayerActiveGames for player: {userName}");
            return (await MongoCollection.FindAsync(game => game.ActivePlayers
                                                                 .Any(activePlayer => activePlayer.UserName == userName)))
                                                                 .ToList();
        }

        public async Task<bool> UpdateGame(CatanGame catanGame)
        {
            if (catanGame.Id == Guid.Empty)
            {
                catanGame.Id = Guid.NewGuid();
                catanGame.BanditsDistance = 7;
                catanGame.BanditsStrength = 0;
                catanGame.RecentDiceRolls = [];
            }
            _logger?.LogDebug($"UpdateGame game: {catanGame.Id}");
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(game => game.Id == catanGame.Id);
            return await UpdateEntity(catanGame, MongoCollection, filter);
        }

        public async Task<bool> RemoveGame(CatanGame catanGame)
        {
            _logger?.LogDebug($"RemoveGame: {catanGame.Id}");
            DeleteResult result = await MongoCollection.DeleteOneAsync(game => game.Id == catanGame.Id);
            if (result != null)
            {
                return result.IsAcknowledged;
            }
            return false;
        }

        public async Task<bool> DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"DeactivateAllKnights game: {catanGameId}");

            var filter = Builders<CatanGame>.Filter.And(
                Builders<CatanGame>.Filter.Eq(game => game.Id, catanGameId)
            );
        
            var update = Builders<CatanGame>.Update.Inc("ActivePlayers.$[player].NumOfActiveKnights", 0);
            var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument
                    {
                        { "player.NumOfActiveKnights",new BsonDocument("$gt", 0) }
                    }
                )
            };
            var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
            var result = await MongoCollection.UpdateOneAsync(filter, update, updateOptions);
            return result != null && result.IsAcknowledged;
        }

        public async Task<bool> ActivateAllKnightsForPlayer(Guid catanGameId, Guid playerId)
        {
            _logger?.LogDebug($"ActivateAllKnightsForPlayer, game: {catanGameId}, player: {playerId}");

            // 1. Retrieve the current NumOfTotalKnights for the player
            var game = await (await MongoCollection.FindAsync(g => g.Id == catanGameId)).FirstOrDefaultAsync();
            var player = game?.ActivePlayers.FirstOrDefault(activePlayer => activePlayer.Id == playerId);
            if (player == null)
                return false;

            int totalKnights = player.NumOfTotalKnights;

            // 2. Update NumOfActiveKnights for that player
            var filter = Builders<CatanGame>.Filter.And(
                Builders<CatanGame>.Filter.Eq(g => g.Id, catanGameId),
                Builders<CatanGame>.Filter.ElemMatch(g => g.ActivePlayers, activePlayer => activePlayer.Id == playerId)
            );

            var update = Builders<CatanGame>.Update.Set("ActivePlayers.$[player].NumOfActiveKnights", totalKnights);

            var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument("player._id", new BsonBinaryData(playerId, GuidRepresentation.Standard))
                )
            };
            var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };

            var result = await MongoCollection.UpdateOneAsync(filter, update, updateOptions);
            return result != null && result.IsAcknowledged;
        }

        public async Task<bool> AdvanceBarbarians(Guid catanGameId)
        {
            _logger?.LogDebug($"AdvanceBarbarians, game: {catanGameId}");
            CatanGame game = (await MongoCollection.FindAsync(game => game.Id == catanGameId)).FirstOrDefault();
            game.BanditsDistance--;
            game.BanditsDistance %= 7;
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(catanGame => catanGame.Id == catanGameId);
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(catanGame => catanGame.BanditsDistance, game.BanditsDistance);

            UpdateResult result = await MongoCollection.UpdateOneAsync(filter, update);
            if (result != null)
            {
                return result.IsAcknowledged;
            }
            return false;
        }

        public async Task<bool> AddPlayerKnight(Guid catanGameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogDebug($"AddPlayerKnight: {catanGameId}, activePlayerId: {activePlayerId}, knightRank: {knightRank}");
            int knightsNumberToAdd = knightRank switch
            {
                KnightRank.Basic => 1,
                KnightRank.Strong => 2,
                KnightRank.Mighty => 3,
                _ => 0
            };

            var filter = Builders<CatanGame>.Filter.And(
                Builders<CatanGame>.Filter.Eq(game => game.Id, catanGameId),
                Builders<CatanGame>.Filter.ElemMatch(game => game.ActivePlayers, activePlayer => activePlayer.Id == activePlayerId)
            );
            var update = Builders<CatanGame>.Update.Inc("ActivePlayers.$[player].NumOfTotalKnights", knightsNumberToAdd);
            var arrayFilters = new List<ArrayFilterDefinition>
            {
                new BsonDocumentArrayFilterDefinition<BsonDocument>(
                    new BsonDocument
                    {
                        { "player._id", new BsonBinaryData(activePlayerId, GuidRepresentation.Standard) }
                    }
                )
            };
            var updateOptions = new UpdateOptions { ArrayFilters = arrayFilters };
            UpdateResult result = await MongoCollection.UpdateOneAsync(filter, update, updateOptions);
            return result != null && result.IsAcknowledged;
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
            CatanGame catanGame = (await MongoCollection.FindAsync(game => game.Id == catanGameId)).First();
            if (catanGame == null)
            {
                _logger.LogError($"GetGameTotalActiveKnights: couldn't find game at this id:{catanGameId}");
            }
            return catanGame.ActivePlayers.Sum(activePlayer => activePlayer.NumOfActiveKnights);
        }
    }
}
