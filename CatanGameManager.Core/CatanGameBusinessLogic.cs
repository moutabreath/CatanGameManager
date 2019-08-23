using CatanGameManager.CommonObjects;
using CatanGameManager.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.Interfaces.PersistanceInterfaces;

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

        public async Task ActivateAllKnightsForPlayer(Guid gameId, Guid playerId)
        {
            _logger?.LogInformation($"ActivateAllKnightsForPlayer for catanGame: {gameId}, player: {playerId}");
            await _catanGamePersist.ActivateAllKnightsForPlayer(gameId, playerId);
        }

        public async Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogInformation($"AddPlayerKnight for catanGame: {gameId}, player: {activePlayerId}, knightRank: {knightRank.ToString()}");
            await _catanGamePersist.AddPlayerKnight(gameId, activePlayerId, knightRank);
        }

        public async Task AddPlayerToGame(CatanGame catanGame, ActivePlayer playerProfile)
        {
            _logger?.LogInformation($"AddPlayerToGame for catanGame: {catanGame.Id}");
            await _catanGamePersist.UpdatePlayerInGame(catanGame, playerProfile);
        }

        public async Task AddPlayerToGame(CatanGame catanGame, PlayerProfile playerProfile)
        {
            _logger?.LogInformation($"AddPlayerToGame for catanGame: {catanGame.Id}, player: {playerProfile.Id}");

            ActivePlayer activePlayer = new ActivePlayer();
            activePlayer.Id = Guid.NewGuid();
            activePlayer.PlayerProfileId = playerProfile.Id;
            activePlayer.InterChanageableVPs = new List<VPType.InterChanageableVP>();
            activePlayer.NumOfActiveKnights = 0;
            activePlayer.NumOfCities = 0;
            activePlayer.NumOfActiveKnights = 0;
            activePlayer.NumOfContinousRoads = 0;
            activePlayer.NumOfSettlements = 0;
            activePlayer.NumOfTotalKnights = 0;

            catanGame.ActivePlayers.Add(activePlayer);
            await _catanGamePersist.UpdateGame(catanGame);
        }

        public async Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType)
        {
            _logger?.LogInformation($"AddPlayerVictoryPoint for catanGame: {gameId}, player: {activePlayerId}, updateType: {updateType.ToString()}");
            await _catanGamePersist.AddPlayerVictoryPoint(gameId, activePlayerId, updateType);
        }

        public async Task AdvanceBarbarians(Guid gameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians for catanGame: {gameId}");
            await _catanGamePersist.AdvanceBarbarians(gameId);
        }

        public async Task DeactivateAllKnights(Guid gameId)
        {
            _logger?.LogInformation($"DeactivateAllKnights for catanGame: {gameId}");
            await _catanGamePersist.DeactivateAllKnights(gameId);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            _logger?.LogInformation($"GetGame for game: \"{gameId}\"");
            return  await _catanGamePersist.GetGame(gameId);
        }

        public async Task<int> GetGameTotalActiveKnights(Guid gameId)
        {
            _logger?.LogInformation($"GetGameTotalActiveKnights for catanGame: {gameId}");
            return await _catanGamePersist.GetTotalActiveKnights(gameId);
        }

        public async Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId)
        {
            _logger?.LogInformation($"GetPlayerActiveGames for game admin: {playedId}");
            return await _catanGamePersist.GetPlayerActiveGames(playedId);
        }

        public async Task<int> GetPlayerTotalVps(ActivePlayer activePlayer)
        {
            _logger?.LogInformation($"GetPlayerTotalVps for game: {activePlayer.CatanGameId},  player: {activePlayer.Id}");
            return await _catanGamePersist.GetPlayerTotalVps(activePlayer.CatanGameId, activePlayer.Id);
        }

        public async Task<int> GetTotalActiveKnights(Guid gameId)
        {
            _logger?.LogInformation($"GetTotalActiveKnights for catanGame: {gameId}");
            return await _catanGamePersist.GetTotalActiveKnights(gameId);
        }

        public async Task RemoveGame(Guid gameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians for catanGame: {gameId}");
            await _catanGamePersist.RemoveGame(gameId);
        }

        public async Task UpdateGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"UpdateGame for game:  \"{catanGame.Id}\"");
            await _catanGamePersist.UpdateGame(catanGame);
        }
    }
}
