﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Config;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace CatanGamePersistence.MongoDB
{
    public class CatanGameMongoPersist : CatanEntityMongoPersist<CatanGame>, ICatanGamePersist
    {
        public CatanGameMongoPersist(ILogger<CatanGameMongoPersist> logger, IOptions<CatanManagerConfig> options) : base(logger, options, "CatanGame")
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
            _logger?.LogInformation($"UpdatePlayerInGame, game: {catanGame.Id}, player: {playerToUpdate.Id}");
            bool doesPlayerExist = catanGame.ActivePlayers.Select(activePlayer => activePlayer.Id == playerToUpdate.Id).
                FirstOrDefault();
            if (!doesPlayerExist)
            {
                _logger?.LogError($"UpdatePlayerInGame. Player is not added into the game. game: {catanGame.Id}, player: {playerToUpdate.Id}");
                return;
            }
            FilterDefinition<CatanGame> filter =  Builders<CatanGame>.Filter.Where(catanGame => catanGame.Id == catanGame.Id 
                                                          && catanGame.ActivePlayers.Any(activePlayer => activePlayer.Id == playerToUpdate.Id));
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(x => x.ActivePlayers[-1], playerToUpdate);
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            await gameCollection.UpdateOneAsync(filter, update);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogInformation($"GetGame: {gameId}");
            IMongoCollection<CatanGame> collection = Database.GetCollection<CatanGame>(_documentName);
            IAsyncCursor<CatanGame> catanGameCursor = await collection.FindAsync(game => game.Id == gameId);
            return catanGameCursor.First();
        }

        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(Guid userId)
        {
            _logger?.LogInformation($"GetPlayerActiveGames for player: {userId}");
            IMongoCollection<CatanGame> collection = Database.GetCollection<CatanGame>(_documentName);
            IAsyncCursor<CatanGame> playerGames = await collection.FindAsync(game => game.ActivePlayers.Any(activePlayer => activePlayer.UserId == userId));
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
            _logger?.LogInformation($"UpdateGame game: {catanGame.Id}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(game => game.Id == catanGame.Id);
            await UpdateEntity(catanGame, gameCollection, filter);
        }

        public async Task RemoveGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"RemoveGame: {catanGame.Id}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            await gameCollection.DeleteOneAsync(game => game.Id == catanGame.Id);
        }    

        public async Task DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogInformation($"DeactivateAllKnights game: {catanGameId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter
                            .Where(x => x.Id == catanGameId // Select the parent document first by its ID
                            && x.ActivePlayers.Any(activePlayer => activePlayer != null));  // Now filter the matching items in the nested array to be updated ONLY

            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update
                .Set(x => x.ActivePlayers[-1].NumOfActiveKnights, 0); // The "-1" index matches ALL the items matching the filter

            await gameCollection.UpdateOneAsync(filter, update);
        }

        public async Task ActivateAllKnightsForPlayer(Guid catanGameId, Guid playerId)
        {
            _logger?.LogInformation($"ActivateAllKnightsForPlayer, game: {catanGameId}, player: {playerId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);

            ActivePlayer activePlayerToUpdate = gameCollection.AsQueryable().Where(game => game.Id == catanGameId).FirstOrDefault().
               ActivePlayers.Where(activePlayer => activePlayer.Id == playerId).FirstOrDefault();

            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter
    .Where(x => x.Id == catanGameId // Select the parent document first by its ID
    && x.ActivePlayers.Any(y => y != null));  // Now filter the matching items in the nested array to be updated ONLY

            var update = Builders<CatanGame>.Update
                .Set(x => x.ActivePlayers[-1].NumOfActiveKnights, activePlayerToUpdate.NumOfTotalKnights); // The "-1" index matches ALL the items matching the filter
            await gameCollection.UpdateOneAsync(filter, update);
        }

        public async Task AdvanceBarbarians(Guid catanGameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians, game: {catanGameId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            IAsyncCursor<CatanGame> catanGameCursor = await gameCollection.FindAsync(game => game.Id == catanGameId);
            CatanGame game = catanGameCursor.FirstOrDefault();
            game.BanditsDistance--;
            game.BanditsDistance %= 7;
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(catanGame => catanGame.Id == catanGameId);            
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(catanGame => catanGame.BanditsDistance, game.BanditsDistance);

            gameCollection.UpdateOne(filter, update); 
        }     

        public async Task AddPlayerKnight(Guid catanGameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogInformation($"AddPlayerKnight: {catanGameId}, activePlayerId: {activePlayerId}, knightRank: {knightRank}");
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

            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(game => game.Id == catanGameId 
                                                                  && game.ActivePlayers.Any(activePlayer => activePlayer.Id == activePlayerId));

            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Inc(x => x.ActivePlayers[-1].NumOfTotalKnights, knightsNumberToAdd);

            await gameCollection.UpdateOneAsync(filter, update);
        }

     
        public async Task<int> GetTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogInformation($"GetTotalActiveKnights: {catanGameId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            IAsyncCursor<CatanGame> catanGameCursor = await gameCollection.FindAsync(game => game.Id == catanGameId);
            int totalNumberOfActiveKnights = catanGameCursor.FirstOrDefault().ActivePlayers.Sum(activePlayer => activePlayer.NumOfTotalKnights);
            return totalNumberOfActiveKnights;
        }

        public async Task<int> GetGameTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogInformation($"GetGameTotalActiveKnights: {catanGameId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_documentName);
            IAsyncCursor<CatanGame> catanGameCursor = await gameCollection.FindAsync(game => game.Id == catanGameId);
            CatanGame catanGame = catanGameCursor.First();
            if (catanGame == null)
            {
                _logger.LogError($"GetGameTotalActiveKnights: couldn't find game at this id:{catanGameId}");
            }
            return catanGame.ActivePlayers.Sum(activePlayer => activePlayer.NumOfActiveKnights);
        }



    }
}
