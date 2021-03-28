using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.CommonObjects.Enums;
using System.Linq;
using Confluent.Kafka;
using System.Net;
using static Confluent.Kafka.ConfigPropertyNames;

namespace CatanGameManager.Core
{
    public class CatanGameBusinessLogic : ICatanGameBusinessLogic
    {
        private ILogger<CatanGameBusinessLogic> _logger;
        private ICatanGamePersist _catanGamePersist;

        public CatanGameBusinessLogic(ILogger<CatanGameBusinessLogic> logger, ICatanGamePersist catanGamePersist)
        {
            _logger = logger;
            _catanGamePersist = catanGamePersist;
        }

        #region Game Update

        public async Task UpdateGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"UpdateGame for game: {catanGame.Id}");
            await _catanGamePersist.UpdateGame(catanGame);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogInformation($"GetGame for catanGame: {gameId}");
            return await _catanGamePersist.GetGame(gameId);
        }

        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(Guid activePlayerId)
        {
            _logger?.LogInformation($"GetUserActiveGames for game admin: {activePlayerId}");
            return await _catanGamePersist.GetUserActiveGames(activePlayerId);
        }

        public async Task AddPlayerToGame(CatanGame catanGame, Guid userId, string userName)
        {
            _logger?.LogInformation($"AddPlayerToGame for catanGame: {catanGame.Id}, player: {userId}: {userName}");

            ActivePlayer activePlayer = new ActivePlayer
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                InterChanageableVPs = new List<VPType.InterChanageableVP>(),
                NumOfActiveKnights = 0,
                NumOfCities = 0,
                NumOfContinousRoads = 0,
                NumOfSettlements = 0,
                NumOfTotalKnights = 0,
                Name = userName
            };

            catanGame.ActivePlayers.Add(activePlayer);
            await _catanGamePersist.UpdateGame(catanGame);
        }
        public async Task UpdatePlayerInGame(CatanGame catanGame, ActivePlayer playerToUpdate)
        {
            _logger?.LogInformation($"UpdatePlayerInGame for catanGame: {catanGame.Id}, player to update: {playerToUpdate.Id}");
            await _catanGamePersist.UpdatePlayerInGame(catanGame, playerToUpdate);
        }

        public async Task RemoveGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"RemoveGame for catanGame: {catanGame.Id}");
            await _catanGamePersist.RemoveGame(catanGame);
        }

        #endregion Game Update

        #region victory points

        /// <summary>
        /// This serves a utility to avoid repeating this calculation on the client.
        /// </summary>
        /// <param name="activePlayer"></param>
        /// <returns></returns>
        private async Task <int> GetPlayerTotalVps(ActivePlayer activePlayer)
        {
            int interchangeabelCounter = 0;
            if (activePlayer.InterChanageableVPs != null)
            {
                foreach (VPType.InterChanageableVP interChanageableVp in activePlayer.InterChanageableVPs)
                {
                    switch (interChanageableVp)
                    {
                        case VPType.InterChanageableVP.Merchant:
                            interchangeabelCounter++;
                            break;
                        case VPType.InterChanageableVP.LongestRoad:
                        case VPType.InterChanageableVP.MetropolisCloth:
                        case VPType.InterChanageableVP.MetropolisCoin:
                        case VPType.InterChanageableVP.MetropolisPaper:
                            interchangeabelCounter += 2;
                            break;
                    }
                }
            }

            int numberOfVps = interchangeabelCounter + activePlayer.NumOfCities * 2 + activePlayer.NumOfSettlements + activePlayer.SaviorOfCatanVP + activePlayer.SpecialVictoryPoints;
            if (numberOfVps >= 13)
            {
                var config = new ProducerConfig { BootstrapServers = "localhost:9092" };

                // If serializers are not specified, default serializers from
                // `Confluent.Kafka.Serializers` will be automatically used where
                // available. Note: by default strings are encoded as UTF8.
                using (IProducer<Null, string> producer = new ProducerBuilder<Null, string>(config).Build())
                {
                    try
                    {
                        DeliveryResult<Null, string> directoryResult = await producer.ProduceAsync("player-points", new Message<Null, string> { Value = $"{activePlayer.UserId}" });
                        _logger?.LogInformation($"Delivered '{directoryResult.Value}' to '{directoryResult.TopicPartitionOffset}'");
                    }
                    catch (ProduceException<Null, string> ex)
                    {
                        _logger?.LogError("Error producing ", ex);
                    }
                }
            }
            return numberOfVps;
        }

        public async Task AddPlayerVictoryPoint(CatanGame catanGame, ActivePlayer activePlayer, VPType updateType)
        {
            _logger?.LogInformation($"AddPlayerVictoryPoint for catanGame: {catanGame.Id}, player: {activePlayer.Id}, updateType: {updateType}");
            if (updateType.TypeToUpdate == VPType.UpdateType.Interchangeable)
            {
                IEnumerable<ActivePlayer> activePlayers = catanGame?.ActivePlayers.Where(player => player.InterChanageableVPs.Contains(updateType.TypeOfInterchangeable));
                ActivePlayer reduceVPCandidate = activePlayers?.FirstOrDefault();
                if (reduceVPCandidate != null)
                {
                    reduceVPCandidate.InterChanageableVPs.Remove(updateType.TypeOfInterchangeable);
                    reduceVPCandidate.NumOfVictoryPoints = await GetPlayerTotalVps(reduceVPCandidate);
                }

                activePlayer.InterChanageableVPs.Add(updateType.TypeOfInterchangeable);
                activePlayer.NumOfVictoryPoints = await GetPlayerTotalVps(activePlayer);
                await _catanGamePersist.UpdateGame(catanGame);
                return;
            }
            await AddNonInterchaneableVPs(catanGame, activePlayer, updateType);
        }

        private async Task AddNonInterchaneableVPs(CatanGame catanGame, ActivePlayer activePlayer, VPType updateType)
        {
            _logger?.LogInformation($"AddVPsToSelectedPlayer game: {catanGame.Id}, player {activePlayer.Id}, updateType: {updateType.TypeToUpdate}");            
            switch (updateType.TypeToUpdate)
            {
                //TODO: Update remaining settlements / cities
                case VPType.UpdateType.City:
                    catanGame.BanditsStrength++;
                    activePlayer.NumOfCities++;
                    break;
                case VPType.UpdateType.Settlment:
                    activePlayer.NumOfSettlements++;
                    break;
                case VPType.UpdateType.SaviorOfCatan:
                    activePlayer.SaviorOfCatanVP++;
                    break;
                case VPType.UpdateType.Constitution:
                case VPType.UpdateType.Printer:
                    activePlayer.SpecialVictoryPoints++;
                    break;
            }
            activePlayer.NumOfVictoryPoints = await GetPlayerTotalVps(activePlayer);
            await _catanGamePersist.UpdateGame(catanGame);
        }



        #endregion victory points

        #region game and player knights

        public async Task<int> GetGameTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogInformation($"GetTotalActiveKnights for catanGame: {catanGameId}");
            return await _catanGamePersist.GetGameTotalActiveKnights(catanGameId);
        }

        public async Task AddPlayerKnight(Guid catanGameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogInformation($"AddPlayerKnight for catanGame: {catanGameId}, player: {activePlayerId}, knightRank: {knightRank}");
            await _catanGamePersist.AddPlayerKnight(catanGameId, activePlayerId, knightRank);
        }

        public async Task AdvanceBarbarians(Guid catanGameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians for catanGame: {catanGameId}");
            await _catanGamePersist.AdvanceBarbarians(catanGameId);
        }

        public async Task ActivateAllKnightsForPlayer(Guid catanGameId, Guid activePlayerId)
        {
            _logger?.LogInformation($"ActivateAllKnightsForPlayer for catanGame: {catanGameId}, player: {activePlayerId}");
            await _catanGamePersist.ActivateAllKnightsForPlayer(catanGameId, activePlayerId);
        }

        public async Task DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogInformation($"DeactivateAllKnights for catanGame: {catanGameId}");
            await _catanGamePersist.DeactivateAllKnights(catanGameId);
        }
      

        #endregion  game and player knights
    }
}
