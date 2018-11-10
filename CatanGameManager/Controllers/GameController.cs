using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatanGameManager.API.Controllers
{
    [Produces("application/json")]
    [Route("api/Game")]
    public class GameController : Controller
    {

        private readonly ICatanGameBusinessLogic _catanGameBuisnessLogic;

        public GameController(ILogger<GameController> logger, ICatanGameBusinessLogic catanGameBusinessLogic)
        {
            _catanGameBuisnessLogic = catanGameBusinessLogic;
        }

        #region Game Update
        public async Task UpdateGame(CatanGame catanGame)
        {
            await _catanGameBuisnessLogic.UpdateGame(catanGame);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            return await _catanGameBuisnessLogic.GetGame(gameId);
        }

        public async Task RemoveGame(Guid id)
        {
            await _catanGameBuisnessLogic.RemoveGame(id);
        }

        public async Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId)
        {
            return await _catanGameBuisnessLogic.GetPlayerActiveGames(playedId);
        }

        public async Task AddPlayerToGame(CatanGame catanGame, ActivePlayer playerProfile)
        {
            await _catanGameBuisnessLogic.AddPlayerToGame(catanGame, playerProfile);
        }

        #endregion Game Update

        #region game and player knights
        public async Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType)
        {
            await _catanGameBuisnessLogic.AddPlayerVictoryPoint(gameId, activePlayerId, updateType);
        }

        public async Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank)
        {
            await _catanGameBuisnessLogic.AddPlayerKnight(gameId, activePlayerId, knightRank);
        }

        public async Task AdvanceBarbarians(Guid gameId)
        {
            await _catanGameBuisnessLogic.AdvanceBarbarians(gameId);
        }       

        public async Task ActiveAllKnightsForPlayer(Guid gameId, Guid playerId)
        {
            await _catanGameBuisnessLogic.ActivateAllKnightsForPlayer(gameId, playerId);
        }

        public async Task DeactivateAllKnights(Guid gameId)
        {
            await _catanGameBuisnessLogic.DeactivateAllKnights(gameId);
        }

        public async Task<int> GetTotalActiveKnights(Guid gameId)
        {
            return await _catanGameBuisnessLogic.GetTotalActiveKnights(gameId);
        }
        #endregion game and player knights

        public async Task<int> GetPlayerTotalVps(ActivePlayer activePlayer)
        {
            return  await _catanGameBuisnessLogic.GetPlayerTotalVps(activePlayer);
        }
    }
}