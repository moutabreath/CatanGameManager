﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CatanGameManager.API.Controllers
{
    /// <summary>
    /// The web interface includes most common game update options.
    /// </summary>
    [Produces("application/json")]
    [Route("[controller]/[action]")]
    public class GameController : Controller
    {
        private readonly ICatanGameBusinessLogic _catanGameBusinessLogic;
        private readonly ILogger<GameController> _logger;
        private static readonly HttpClient _sHttpCient = new HttpClient();// TODO: add authentication token to validate the user exists

        public GameController(ILogger<GameController> logger, ICatanGameBusinessLogic catanGameBusinessLogic)
        {
            _catanGameBusinessLogic = catanGameBusinessLogic;
            _logger = logger;
        }

        #region Game Update

        [HttpGet]
        public async Task UpdateGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"UpdateGame for game: {catanGame.Id}");
            await _catanGameBusinessLogic.UpdateGame(catanGame);
        }

        [HttpGet]
        public async Task<CatanGame> GetGame(Guid catanGameId)
        {
            _logger?.LogInformation($"GetGame for game: {catanGameId}");
            return await _catanGameBusinessLogic.GetGame(catanGameId);
        }

        [HttpGet]
        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(Guid userId)
        {
            _logger?.LogInformation($"GetUserActiveGames for user: {userId}");
            return await _catanGameBusinessLogic.GetUserActiveGames(userId);
        }

        [HttpPost]
        public async Task AddPlayerToGame(CatanGame catanGame, Guid userId, string userName)
        {
            _logger?.LogInformation($"AddPlayerToGame for game: {catanGame.Id} and user: {userId} ");
            // TODO: add authentication token to validate the user exists
            await _catanGameBusinessLogic.AddPlayerToGame(catanGame, userId, userName);
        }

        [HttpPost]
        public async Task RemoveGame(CatanGame catanGame)
        {
            _logger?.LogInformation($"RemoveGame for game: {catanGame.Id}");
            await _catanGameBusinessLogic.RemoveGame(catanGame);
        }

        #endregion Game Update

        #region victory points

  
        [HttpPost]
        public async Task AddPlayerVictoryPoint(CatanGame game, ActivePlayer activePlayer, VPType updateType)
        {
            _logger?.LogInformation($"AddPlayerVictoryPoint for game: {game.Id}, activePlayer {activePlayer.Id} and updateType {updateType}");
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(game, activePlayer, updateType);
        }

        #endregion victory points

        #region game and player knights

        [HttpGet]
        public async Task<int> GetGameTotalActiveKnights(Guid gameId)
        {
            _logger?.LogInformation($"GetGameTotalActiveKnights for game: {gameId}");
            return await _catanGameBusinessLogic.GetGameTotalActiveKnights(gameId);
        }

        [HttpPost]
        public async Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank)
        {
            _logger?.LogInformation($"GetTotalActiveKnights for game: {activePlayerId}");
            await _catanGameBusinessLogic.AddPlayerKnight(gameId, activePlayerId, knightRank);
        }

        [HttpPost]
        public async Task AdvanceBarbarians(Guid gameId)
        {
            _logger?.LogInformation($"AdvanceBarbarians for user: {gameId}");
            await _catanGameBusinessLogic.AdvanceBarbarians(gameId);
        }

        [HttpPost]
        public async Task ActivateAllKnightsForPlayer(Guid gameId, Guid playerId)
        {
            _logger?.LogInformation($"ActiveAllKnightsForPlayer for game: {gameId} and player: {playerId}");
            await _catanGameBusinessLogic.ActivateAllKnightsForPlayer(gameId, playerId);
        }

        [HttpPost]
        public async Task DeactivateAllKnights(Guid gameId)
        {
            _logger?.LogInformation($"DeactivateAllKnights for game: {gameId}");
            await _catanGameBusinessLogic.DeactivateAllKnights(gameId);
        }

        #endregion game and player knights     
  
    }
}