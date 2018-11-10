using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces.PersistanceInterfaces;

namespace CatanGameManager.Core
{
    public class CatanGameBusinessLogic : ICatanGameBusinessLogic
    {
        private readonly ILogger<CatanGameBusinessLogic> _logger;
        private readonly ICatanGamePersist _catanGamePersist;
        public CatanGameBusinessLogic(ILogger<CatanGameBusinessLogic> logger, ICatanGamePersist catanGamePersist)
        {
            _logger = logger;
            _catanGamePersist = catanGamePersist;
        }
        public async Task AddPlayerToGame(CatanGame catanGame, PlayerProfile playerProfile)
        {
            _logger?.LogInformation($"AddPlayerToGame for catanGame: {catanGame.Id}");
            ActivePlayer activePlayer = new ActivePlayer
            {
                Email = playerProfile.Email,
                FirstName = playerProfile.FirstName,
                LastName = playerProfile.LastName

            };
            await _catanGamePersist.UpdatePlayerInGame(catanGame, activePlayer);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogInformation($"GetGame for game: {gameId}");
            return  await _catanGamePersist.GetGame(gameId);
        }

        public async Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId)
        {
            _logger?.LogInformation($"GetPlayerActiveGames for game admin: {playedId}");
            return await _catanGamePersist.GetPlayerActiveGames(playedId);
        }

        public async Task UpdateGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"UpdateGame for game: {catanGame.Id}");
            await _catanGamePersist.UpdateGame(catanGame);
        }
      
        public async Task AdvanceBarbarians(Guid gameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians for game: {gameId}");
            await _catanGamePersist.AdvanceBarbarians(gameId);
        }

        public async Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType)
        {
            _logger?.LogInformation($"AddPlayerVictoryPoint for game: {gameId} ,player: {activePlayerId} and update type: {updateType}");
            await _catanGamePersist.AddPlayerVictoryPoint(gameId, activePlayerId, updateType);
        }

        public async Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogInformation($"AddPlayerKnight for game: {gameId}, player: {activePlayerId}");
            await _catanGamePersist.AddPlayerKnight(gameId, activePlayerId, knightRank);
        }

        public async Task RemoveGame(Guid id)
        {
            _logger?.LogInformation($"RemoveGame: {id}");
            await _catanGamePersist.RemoveGame(id);
        }

        public async Task ActivateAllKnightsForPlayer(Guid gameId, Guid playerId)
        {
            _logger?.LogInformation($"ActivateAllKnightsForPlayer for game: {gameId}, player: {playerId}");
            await _catanGamePersist.ActivateAllKnightsForPlayer(gameId, playerId);
        }

        public async Task DeactivateAllKnights(Guid gameId)
        {
            _logger?.LogInformation($"DeactiveateAllKnights  game: {gameId}");
            await _catanGamePersist.DeactivateAllKnights(gameId);
        }

        public async Task<int> GetTotalActiveKnights(Guid gameId)
        {
            _logger?.LogInformation($"GetTotalActiveKnights  game: {gameId}");
            return await _catanGamePersist.GetTotalActiveKnights(gameId);
        }

        public async Task<int> GetGameTotalActiveKnights(Guid gameId)
        {
            _logger?.LogInformation($"GetGameTotalActiveKnights  game: {gameId}");
            return await _catanGamePersist.GetTotalActiveKnights(gameId);
        }

        public async Task<int> GetPlayerTotalVps(ActivePlayer activePlayer)
        {
            CatanGame game = await GetGame(activePlayer.CatanGameId);
            activePlayer = game.ActivePlayers.FirstOrDefault(activeP => activeP.Id == activePlayer.Id);

            int interchangeabelCounter = 0;
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
            return interchangeabelCounter + activePlayer.NumOfCities*2 + activePlayer.NumOfSettlements + activePlayer.SaviorOfCatanVP;
        }
    }
}
