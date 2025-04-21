using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Config;
using CatanGameManager.ExternalAPI.Requests;
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
        
        private static readonly HttpClient _sHttpCient = new HttpClient();
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
            _logger?.LogDebug($"UpdateGame for game: {catanGame.Id}");
            await _catanGameBusinessLogic.UpdateGame(catanGame);
        }

        [HttpGet("{catanGameId}")]
        public async Task<CatanGame> GetGame(Guid catanGameId)
        {
            _logger?.LogDebug($"GetGame for game: {catanGameId}");
            return await _catanGameBusinessLogic.GetGame(catanGameId);
        }

        [HttpGet("{userName}")]
        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName)
        {
            _logger?.LogDebug($"GetUserActiveGames for user: {userName}");
            return await _catanGameBusinessLogic.GetUserActiveGames(userName);
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayerToGame([FromBody] AddPlayerToGameRequest addPlayerToGameRequest)
        {
            _logger?.LogDebug($"AddPlayerToGame for game: {addPlayerToGameRequest.CatanGame.Id} and user: {addPlayerToGameRequest.UserName} ");
            // TODO: add authentication token when validating user exists
            try
            {
                HttpResponseMessage userResponse = await _sHttpCient.GetAsync($"{_usersEndpoint}" + $"account/SearchPlayer?userName={addPlayerToGameRequest.UserName}");
                if (!userResponse.IsSuccessStatusCode)
                {                 
                    _logger?.LogError($"AddPlayerToGame Unexpected response from 'Players' endpoint. User: {addPlayerToGameRequest.UserName}. Game: {addPlayerToGameRequest.CatanGame.Id}");
                    return StatusCode(500);
                }
            }
            catch(Exception ex)
            {
                _logger?.LogError($"AddPlayerToGame Error while tring to reach 'Players' endpoint for user {addPlayerToGameRequest.UserName} and game {addPlayerToGameRequest.CatanGame.Id}", ex);
                return Problem(statusCode: 400, title: "Invalid user");
            }
            await _catanGameBusinessLogic.AddPlayerToGame(addPlayerToGameRequest.CatanGame, addPlayerToGameRequest.UserName);
            return Ok();
        }

        [HttpPost]
        public async Task RemoveGame([FromBody] CatanGame catanGame)
        {
            _logger?.LogDebug($"RemoveGame for game: {catanGame.Id}");
            await _catanGameBusinessLogic.RemoveGame(catanGame);
        }

        #endregion Game Update

        #region victory points

  
        [HttpPost]
        public async Task AddPlayerVictoryPoint([FromBody] AddPlayerVictoryPointRequest addPlayerVictoryPointRequest)
        {
            _logger?.LogDebug($"AddPlayerVictoryPoint for game: {addPlayerVictoryPointRequest.Game.Id}, activePlayer {addPlayerVictoryPointRequest.ActivePlayer.Id} and updateType {addPlayerVictoryPointRequest.UpdateType}");
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(addPlayerVictoryPointRequest.Game, addPlayerVictoryPointRequest.ActivePlayer, addPlayerVictoryPointRequest.UpdateType);
        }

        #endregion victory points

        #region game and player knights

        [HttpGet("{catanGameId}")]
        public async Task<int> GetGameTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"GetGameTotalActiveKnights for game: {catanGameId}");
            return await _catanGameBusinessLogic.GetGameTotalActiveKnights(catanGameId);
        }

        [HttpPost]
        public async Task AddPlayerKnight([FromBody] AddPlayerKnightRequest addPlayerKnightRequest)
        {
            _logger?.LogDebug($"AddPlayerKnight: GameId {addPlayerKnightRequest.GameId}, ActivePlayerId {addPlayerKnightRequest.ActivePlayerId}, KnightRank:{addPlayerKnightRequest.KnightRank}");
            await _catanGameBusinessLogic.AddPlayerKnight(addPlayerKnightRequest.GameId, addPlayerKnightRequest.ActivePlayerId, addPlayerKnightRequest.KnightRank);
        }

        [HttpPost("{catanGameId}")]
        public async Task AdvanceBarbarians(Guid catanGameId)
        {
            _logger?.LogDebug($"AdvanceBarbarians for user: {catanGameId}");
            await _catanGameBusinessLogic.AdvanceBarbarians(catanGameId);
        }

        [HttpPost]
        public async Task ActivateAllKnightsForPlayer(ActivateAllKnightsForPlayerRequest activateAllKnightsForPlayer)
        {
            _logger?.LogDebug($"ActiveAllKnightsForPlayer for game: {activateAllKnightsForPlayer.GameId} and player: {activateAllKnightsForPlayer.PlayerId}");
            await _catanGameBusinessLogic.ActivateAllKnightsForPlayer(activateAllKnightsForPlayer.GameId, activateAllKnightsForPlayer.PlayerId);
        }

        [HttpPost("{catanGameId}")]
        public async Task DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"DeactivateAllKnights for game: {catanGameId}");
            await _catanGameBusinessLogic.DeactivateAllKnights(catanGameId);
        }

        #endregion game and player knights     
  
    }
}