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

        private readonly ICatanGameBusinessLogic _catanGameBusinessLogic;

        public GameController(ILogger<GameController> logger, ICatanGameBusinessLogic catanGameBusinessLogic)
        {
            _catanGameBusinessLogic = catanGameBusinessLogic;
        }

        #region Game Update
        public async Task UpdateGame(CatanGame catanGame)
        {
            await _catanGameBusinessLogic.UpdateGame(catanGame);
        }

        public async Task<CatanGame> GetGame(Guid gameId)
        {
            return await _catanGameBusinessLogic.GetGame(gameId);
        }

        public async Task RemoveGame(Guid id)
        {
            await _catanGameBusinessLogic.RemoveGame(id);
        }

        public async Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId)
        {
            return await _catanGameBusinessLogic.GetPlayerActiveGames(playedId);
        }

        public async Task AddPlayerToGame(CatanGame catanGame, ActivePlayer playerProfile)
        {
            await _catanGameBusinessLogic.AddPlayerToGame(catanGame, playerProfile);
        }

        #endregion Game Update

        #region game and player knights
        public async Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType)
        {
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(gameId, activePlayerId, updateType);
        }

        public async Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank)
        {
            await _catanGameBusinessLogic.AddPlayerKnight(gameId, activePlayerId, knightRank);
        }

        public async Task AdvanceBarbarians(Guid gameId)
        {
            await _catanGameBusinessLogic.AdvanceBarbarians(gameId);
        }       

        public async Task ActiveAllKnightsForPlayer(Guid gameId, Guid playerId)
        {
            await _catanGameBusinessLogic.ActivateAllKnightsForPlayer(gameId, playerId);
        }

        public async Task DeactivateAllKnights(Guid gameId)
        {
            await _catanGameBusinessLogic.DeactivateAllKnights(gameId);
        }

        public async Task<int> GetTotalActiveKnights(Guid gameId)
        {
            return await _catanGameBusinessLogic.GetTotalActiveKnights(gameId);
        }
        #endregion game and player knights

        public async Task<int> GetPlayerTotalVps(ActivePlayer activePlayer)
        {
            return  await _catanGameBusinessLogic.GetPlayerTotalVps(activePlayer);
        }
    }
}