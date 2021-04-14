using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Config;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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
        private string _usersEndpoint;

        public GameController(ILogger<GameController> logger, IOptions<GameManagerConfig> options, ICatanGameBusinessLogic catanGameBusinessLogic)
        {
            _catanGameBusinessLogic = catanGameBusinessLogic;
            _logger = logger;
            _usersEndpoint = options.Value.UsersEndpoint;
        }

        #region Game Update

        [HttpPost]
        public async Task UpdateGame([FromBody] CatanGame catanGame)
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
        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName)
        {
            _logger?.LogInformation($"GetUserActiveGames for user: {userName}");
            return await _catanGameBusinessLogic.GetUserActiveGames(userName);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayerToGame(string userName, [FromBody] CatanGame catanGame)
        {
            _logger?.LogInformation($"AddPlayerToGame for game: {catanGame.Id} and user: {userName} ");
            // TODO: add authentication token to validate the user exists
            try
            {
                HttpResponseMessage userResponse = await _sHttpCient.GetAsync($"{_usersEndpoint}" + $"api/SearchPlayer?userName={userName}");
            }
            catch(Exception ex)
            {
                _logger?.LogError($"AddPlayerToGame Error while try to reach 'Players' endpoint for userId {userName} and game {catanGame.Id}", ex);
                return Problem(statusCode: 400, title: "Invalid user");
            }
            await _catanGameBusinessLogic.AddPlayerToGame(catanGame, userName);
            return Ok();
        }

        [HttpPost]
        public async Task RemoveGame([FromBody] CatanGame catanGame)
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
        public async Task DeactivateAllKnights([FromBody] Guid gameId)
        {
            _logger?.LogInformation($"DeactivateAllKnights for game: {gameId}");
            await _catanGameBusinessLogic.DeactivateAllKnights(gameId);
        }

        #endregion game and player knights     
  
    }
}