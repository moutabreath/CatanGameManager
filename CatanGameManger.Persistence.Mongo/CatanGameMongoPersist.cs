using System;
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
        private readonly ILogger _logger;
        private readonly string _catanGameDoc = "CatanGame";

        public CatanGameMongoPersist(ILogger<CatanGameMongoPersist> logger, IOptions<CatanManagerConfig> options)
        {
            _logger = logger;
            CatanManagerConfig configuration = options.Value;
            Client = new MongoClient(configuration.MongoConnectionString);
            Database = Client.GetDatabase(configuration.MongoCatanGameDbName);
            InitializeClassMap();
            MongoCollection = Database.GetCollection<CatanGame> (_catanGameDoc);
        }

        private static void InitializeClassMap()
        {
            if (BsonClassMap.IsClassMapRegistered(typeof(CatanGame)) == false)
            {
                BsonClassMap.RegisterClassMap<CatanGame>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.SetIgnoreExtraElements(true);
                });
            }
     
            if (BsonClassMap.IsClassMapRegistered(typeof(ActivePlayer)) == false)
            {
                BsonClassMap.RegisterClassMap<ActivePlayer>(cm =>
                {
                    cm.AutoMap();
                    cm.SetIdMember(cm.GetMemberMap(c => c.Id));
                    cm.SetIgnoreExtraElements(true);
                });
            }
        }


        public async Task UpdatePlayerInGame(CatanGame catanGame, ActivePlayer playerToUpdate)
        {
            _logger?.LogInformation($"UpdatePlayerInGame, game: {catanGame.Id}, player: {playerToUpdate.Id}");
            bool doesPlayerExist = catanGame.ActivePlayers.Select(activePlayer => activePlayer.NickName == playerToUpdate.NickName).
                FirstOrDefault();
            if (!doesPlayerExist)
            {
                _logger?.LogError($"UpdatePlayerInGame. Player is not added into the game. game: {catanGame.Id}, player: {playerToUpdate.Id}");
                return;
            }
            FilterDefinition<CatanGame> filter =  Builders<CatanGame>.Filter.Where(x => x.Id == catanGame.Id 
                                                          && x.ActivePlayers.Any(activePlayer => activePlayer.NickName == playerToUpdate.NickName));
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(x => x.ActivePlayers[-1], playerToUpdate);
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);
            await gameCollection.UpdateOneAsync(filter, update);

        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogInformation($"GetGame: {gameId}");
            IMongoQueryable<CatanGame> collection = Database.GetCollection<CatanGame>(_catanGameDoc).AsQueryable();
            CatanGame catanGame = await collection.FirstAsync(game => game.Id == gameId);
            return catanGame;
        }

        public async Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId)
        {
            _logger?.LogInformation($"GetPlayerActiveGames for player: {playedId}");
            IMongoCollection<CatanGame> collection = Database.GetCollection<CatanGame>(_catanGameDoc);
            IAsyncCursor<CatanGame> playerGames = await collection.FindAsync(game => game.ActivePlayers.Any(activePlayer => activePlayer.Id ==playedId));
            return playerGames.ToList();
        }

        public async Task UpdateGame(CatanGame catanGame)
        {
            if (catanGame.Id == Guid.Empty) catanGame.Id = Guid.NewGuid();
            _logger?.LogInformation($"UpdateGame game: {catanGame.Id}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);
            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(x => x.Id == catanGame.Id);
            await UpdateEntity(catanGame, gameCollection, filter);
        }


        public async Task DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogInformation($"DeactivateAllKnights game:  {catanGameId}");
            


            //        var filter = Builders<CatanGame>.Filter
            //.Where(x => x.Id == catanGameId // Select the parent document first by its ID
            //&& x.ActivePlayers.Any(y => y != null));  // Now filter the matching items in the nested array to be updated ONLY

            //        var update = Builders<CatanGame>.Update
            //            .Set(x => x.ActivePlayers[-1].NumOfActiveKnights, 0); // The "-1" index matches ALL the items matching the filter

            //        await gameCollection.UpdateOneAsync(filter, update);
            await UpdateKnightsStatus(catanGameId, y => y != null, 0);
        }

        private async Task UpdateKnightsStatus(Guid catanGameId, Func<ActivePlayer, bool> predicate, int newValue)
        {
            
            _logger?.LogInformation($"UpdateKnightsStatus:  \"{catanGameId}\"");

            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);

            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(x => x.Id == catanGameId
                                                                  && x.ActivePlayers.Any(predicate));
            
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(x => x.ActivePlayers[-1].NumOfActiveKnights, newValue);

            await gameCollection.UpdateOneAsync(filter, update);
        }

        public async Task ActivateAllKnightsForPlayer(Guid catanGameId, Guid activePlayerId)
        {
            _logger?.LogInformation($"ActivateAllKnightsForPlayer, game: {catanGameId}, player: {activePlayerId}");
            IMongoQueryable<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc).AsQueryable();
            ActivePlayer activePlayerToUpdate = gameCollection.Where(game => game.Id == catanGameId).FirstOrDefault().
                ActivePlayers.Where(activePlayer => activePlayer.Id == activePlayerId).FirstOrDefault();
            await UpdateKnightsStatus(catanGameId, y => y.Id == activePlayerId, activePlayerToUpdate.NumOfTotalKnights);
        }

        public async Task AdvanceBarbarians(Guid gameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians, game: {gameId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);
            IAsyncCursor<CatanGame> catanGame = await gameCollection.FindAsync(game => game.Id == gameId);

            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(x => x.Id == gameId);            
            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(x => x.BanditsDistance, catanGame.FirstOrDefault().BanditsDistance-- %7);

            gameCollection.UpdateOne(filter, update); 
        }

        public async Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType)
        {
            _logger?.LogInformation($"AddPlayerVictoryPoint game: {gameId}, player: {activePlayerId}, updateType: {updateType.TypeToUpdate} {updateType.TypeOfInterchangeable}");

            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);
            
            await ReduceVPsForPrevPlayer(gameId, updateType, gameCollection);
            await AddVPsToSelectedPlayer(gameCollection, gameId, activePlayerId, updateType);
        }

        private async Task AddVPsToSelectedPlayer(IMongoCollection<CatanGame> gameCollection, Guid gameId, Guid activePlayerId, VPType updateType)
        {
            _logger?.LogInformation($"AddVPsToSelectedPlayer game: {gameId}, player {activePlayerId}, updateType: {updateType.TypeToUpdate} {updateType.TypeOfInterchangeable}");

            IAsyncCursor<CatanGame> playerGameCursor = await gameCollection.FindAsync(game => game.Id == gameId);
            CatanGame playerGame = playerGameCursor.FirstOrDefault();

            int oldPlayerVPs = playerGame.ActivePlayers.FirstOrDefault(player => player.Id == activePlayerId).AmountOfVictoryPoints;
            switch (updateType.TypeToUpdate)
            {
                //TODO: Update remaining settlements / cities
                case VPType.UpdateType.City:
                    var updateDef = Builders<CatanGame>.Update.Set(game => game.BanditsStrength, ++playerGame.BanditsStrength);
                    gameCollection.UpdateOne(game => game.Id == playerGame.Id, updateDef);
                    break;
                case VPType.UpdateType.Constitution:
                case VPType.UpdateType.Printer:
                case VPType.UpdateType.SaviorOfCatan:
                    FilterDefinition<CatanGame> activePlayerFilter = Builders<CatanGame>.Filter.Where(x =>
                        x.Id == gameId 
                        && x.ActivePlayers.Any(activePlayer => activePlayer.Id == activePlayerId));
                    UpdateDefinition<CatanGame> updateActivePlayer = Builders<CatanGame>.Update
                        .Set(x => x.ActivePlayers[-1].AmountOfVictoryPoints, ++oldPlayerVPs);
                    await gameCollection.UpdateOneAsync(activePlayerFilter, updateActivePlayer);
                    break;
            }
        }

        private async Task ReduceVPsForPrevPlayer(Guid gameId, VPType updateType, IMongoCollection<CatanGame> gameCollection)
        {
            if (updateType.TypeToUpdate != VPType.UpdateType.Interchangeable) return;

            IAsyncCursor<CatanGame> cursor = await gameCollection.FindAsync(game => game.Id == gameId);
            IEnumerable<ActivePlayer> playerOwningInterchangeable = cursor.FirstOrDefault()
                ?.ActivePlayers.Where(player => player.InterChanageableVPs.Contains(updateType.TypeOfInterchangeable));

            ActivePlayer activePlayerToReduceVp = playerOwningInterchangeable?.FirstOrDefault();
            if (activePlayerToReduceVp != null)
            {
                _logger?.LogInformation( $"ReduceVPsForPrevPlayer: \"{gameId}\" found interchangeable VP for player {activePlayerToReduceVp.Id}" +
                    $" updateType: {updateType.TypeToUpdate} {updateType.TypeOfInterchangeable}");

                UpdateInterchangeableForPrevPlayer(gameId, updateType, gameCollection, activePlayerToReduceVp);

                int amountOfPointsToReduce = 0;
                switch (updateType.TypeOfInterchangeable)
                {
                    case VPType.InterChanageableVP.Merchant:
                        amountOfPointsToReduce = 1;                       
                        break;
                    case VPType.InterChanageableVP.MetropolisPaper:
                    case VPType.InterChanageableVP.LongestRoad:
                    case VPType.InterChanageableVP.MetropolisCloth:
                    case VPType.InterChanageableVP.MetropolisCoin:
                        amountOfPointsToReduce = 2;                        
                        break;
                }
                gameCollection.FindOneAndUpdate(game => game.Id == gameId &&
                                                                game.ActivePlayers.Any(activePlayer => activePlayer.Id == activePlayerToReduceVp.Id),
                            Builders<CatanGame>.Update.Set(c => c.ActivePlayers[-1].AmountOfVictoryPoints,
                                activePlayerToReduceVp.AmountOfVictoryPoints - amountOfPointsToReduce));
            }
        }

        private static void UpdateInterchangeableForPrevPlayer(Guid gameId, VPType updateType, IMongoCollection<CatanGame> gameCollection,
            ActivePlayer activePlayerToReduceVp)
        {
            IList<VPType.InterChanageableVP> newInterChanageableVps = new List<VPType.InterChanageableVP>();
            foreach (VPType.InterChanageableVP interChanageableVP in activePlayerToReduceVp.InterChanageableVPs)
            {
                if (interChanageableVP != updateType.TypeOfInterchangeable)
                {
                    newInterChanageableVps.Add(interChanageableVP);
                }
            }

            gameCollection.FindOneAndUpdate(game => game.Id == gameId &&
                                                    game.ActivePlayers.Any(activePlayer => activePlayer.Id == activePlayerToReduceVp.Id), // find this match
                Builders<CatanGame>.Update.Set(c => c.ActivePlayers[-1].InterChanageableVPs, newInterChanageableVps));
        }

        public async Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogInformation($"AddPlayerKnight: {gameId}, activePlayerId: {activePlayerId}, knightRank: {knightRank}");
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

            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);

            FilterDefinition<CatanGame> filter = Builders<CatanGame>.Filter.Where(game => game.Id == gameId 
                                                                  && game.ActivePlayers.Any(activePlayer => activePlayer.Id == activePlayerId));

            UpdateDefinition<CatanGame> update = Builders<CatanGame>.Update.Set(x => x.ActivePlayers[-1].NumOfTotalKnights, knightsNumberToAdd);

            await gameCollection.UpdateOneAsync(filter, update);
        }

        public async Task RemoveGame(Guid id)
        {
            _logger?.LogInformation($"RemoveGame: {id}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);
            await gameCollection.DeleteOneAsync(game => game.Id == id);
        }

        public async Task<int> GetTotalActiveKnights(Guid gameId)
        {
            _logger?.LogInformation($"GetTotalActiveKnights: {gameId}");
            IMongoCollection<CatanGame> gameCollection = Database.GetCollection<CatanGame>(_catanGameDoc);
            IAsyncCursor<CatanGame> catanGame = await gameCollection.FindAsync(game => game.Id == gameId);
            int totalNumberOfActiveKnights = catanGame.FirstOrDefault().ActivePlayers.Sum(activePlayer => activePlayer.NumOfTotalKnights);
            return totalNumberOfActiveKnights;
        }
    }
}
