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

        public async Task AddPlayerToGame(CatanGame catanGame, User user)
        {
            _logger?.LogInformation($"AddPlayerToGame for catanGame: {catanGame.Id}, player: {user.Id}: {user.Name}");

            ActivePlayer activePlayer = new ActivePlayer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                InterChanageableVPs = new List<VPType.InterChanageableVP>(),
                NumOfActiveKnights = 0,
                NumOfCities = 0
            };
            activePlayer.NumOfActiveKnights = 0;
            activePlayer.NumOfContinousRoads = 0;
            activePlayer.NumOfSettlements = 0;
            activePlayer.NumOfTotalKnights = 0;
            activePlayer.Name = user.Name;

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
        public async Task<int> GetPlayerTotalVps(ActivePlayer activePlayer)
        {
            int interchangeabelCounter = 0;
            await Task.Run(() =>
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
            });
            return interchangeabelCounter + activePlayer.NumOfCities * 2 + activePlayer.NumOfSettlements + activePlayer.SaviorOfCatanVP;
        }

        public async Task AddPlayerVictoryPoint(Guid catanGameId, Guid activePlayerId, VPType updateType)
        {
            _logger?.LogInformation($"AddPlayerVictoryPoint for catanGame: {activePlayerId}, player: {activePlayerId}, updateType: {updateType.ToString()}");
            await _catanGamePersist.AddPlayerVictoryPoint(catanGameId, activePlayerId, updateType);
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
            _logger?.LogInformation($"AddPlayerKnight for catanGame: {catanGameId}, player: {activePlayerId}, knightRank: {knightRank.ToString()}");
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
