using CatanGameManager.API.Requests;
using CatanGameManager.CommonObjects;
using CatanGameManager.External_API.Config;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ILogger<GameController> _logger;
        
        private static readonly HttpClient _sHttpCient = new HttpClient();
        private string _gameEndpoint;

        public GameController(ILogger<GameController> logger, IOptions<ExternalGameManagerConfig> options)
        {
            _logger = logger;
            _gameEndpoint = $"{options.Value.ServiceHost}/Game";
        }

        #region Game Update

        [HttpPost]
        public async Task<IActionResult> UpdateGame([FromBody] CatanGame catanGame)
        {
            _logger?.LogDebug($"UpdateGame for game: {catanGame.Id}");
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsJsonAsync($"{_gameEndpoint}/UpdateGame", catanGame);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"GetGame exception on response for game: {catanGame.Id}", ex);
                return StatusCode(500);
            }
            return Ok();

        }

        [HttpGet("{catanGameId}")]
        public async Task<CatanGame> GetGame(Guid catanGameId)
        {
            _logger?.LogDebug($"GetGame for game: {catanGameId}");            
            CatanGame? catanGame = null;
            try
            {
                HttpResponseMessage response = await _sHttpCient.GetAsync($"{_gameEndpoint}/{_gameEndpoint}?catanGameId={catanGameId}");
                catanGame = await response.Content.ReadFromJsonAsync<CatanGame>();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"GetGame exception on response catanGameId: {catanGameId}", ex);
            }
            if (catanGame == null)
            {
                _logger?.LogError($"GetGame Unexpected response catanGameId: {catanGameId}");
                return new CatanGame();
            }
            return catanGame;
        }

        [HttpGet("{userName}")]
        public async Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName)
        {
            _logger?.LogDebug($"GetUserActiveGames for user: {userName}");
            IEnumerable<CatanGame>? userActiveGames = null;
            try
            {
                HttpResponseMessage response = await _sHttpCient.GetAsync($"{_gameEndpoint}/GetUserActiveGames?userName={userName}");
                userActiveGames = await response.Content.ReadFromJsonAsync<IEnumerable<CatanGame>>();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"GetGame Unexpected response for user: {userName}", ex);
            }
            if (userActiveGames == null)
            {
                _logger?.LogError($"GetUserActiveGames Unexpected response userName: {userName}");
                return new LinkedList<CatanGame>();
            }
            return userActiveGames;

        }

        [HttpPost]
        public async Task<IActionResult> AddPlayerToGame([FromBody] AddPlayerToGameRequest addPlayerToGameRequest)
        {
            _logger?.LogDebug($"AddPlayerToGame for game: {addPlayerToGameRequest.CatanGame.Id} and user: {addPlayerToGameRequest.UserName} ");
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsJsonAsync($"{_gameEndpoint}/AddPlayerToGame", addPlayerToGameRequest);
                return StatusCode(500);
            }
            catch (Exception ex)
            {
                _logger?.LogError(message: $"AddPlayerToGame Exception on response userName: {addPlayerToGameRequest.CatanGame.Id} and user: {addPlayerToGameRequest.UserName}", ex);            
            }
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveGame([FromBody] CatanGame catanGame)
        {
            _logger?.LogDebug($"RemoveGame for game: {catanGame.Id}");            
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsJsonAsync($"{_gameEndpoint}/AddPlayerToGame", catanGame);
                await response.Content.ReadFromJsonAsync<IEnumerable<CatanGame>>();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"RemoveGame Exception for game: {catanGame.Id}", ex);
                return StatusCode(500);
            }
            return Ok();

        }

        #endregion Game Update

        #region victory points


        [HttpPost]
        public async Task<IActionResult> AddPlayerVictoryPoint([FromBody] AddPlayerVictoryPointRequest addPlayerVictoryPointRequest)
        {
            _logger?.LogDebug($"AddPlayerVictoryPoint for game: {addPlayerVictoryPointRequest.Game.Id}, activePlayer {addPlayerVictoryPointRequest.ActivePlayer.Id} and updateType {addPlayerVictoryPointRequest.UpdateType}");
            try
            {
                await _sHttpCient.PostAsJsonAsync($"{_gameEndpoint}/AddPlayerToGame", addPlayerVictoryPointRequest);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"RemoveGame Exception for game:  {addPlayerVictoryPointRequest.Game.Id}, activePlayer {addPlayerVictoryPointRequest.ActivePlayer.Id} and updateType {addPlayerVictoryPointRequest.UpdateType}", ex);
                return StatusCode(500);
            }
            return Ok();
        }

        #endregion victory points

        #region game and player knights

        [HttpGet("{catanGameId}")]
        public async Task<int> GetGameTotalActiveKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"GetGameTotalActiveKnights for game: {catanGameId}");
            int totalActiveKnights = 0;
            try
            {
                HttpResponseMessage response = await _sHttpCient.GetAsync($"{_gameEndpoint}/GetGameTotalActiveKnights?catanGameId={catanGameId}");
                totalActiveKnights = await response.Content.ReadFromJsonAsync<int>();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"GetGame Unexpected response for game: {catanGameId}", ex);
                return -1;
            }
            return totalActiveKnights;
        }

        [HttpPost]
        public async Task<IActionResult> AddPlayerKnight([FromBody] AddPlayerKnightRequest addPlayerKnightRequest)
        {
            _logger?.LogDebug($"AddPlayerKnight: GameId {addPlayerKnightRequest.GameId}, ActivePlayerId {addPlayerKnightRequest.ActivePlayerId}, KnightRank:{addPlayerKnightRequest.KnightRank}");
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsJsonAsync($"{_gameEndpoint}/AddPlayerKnight", addPlayerKnightRequest);
                await response.Content.ReadFromJsonAsync<IEnumerable<CatanGame>>();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"AddPlayerKnight Exception GameId {addPlayerKnightRequest.GameId}, ActivePlayerId {addPlayerKnightRequest.ActivePlayerId}, KnightRank:{addPlayerKnightRequest.KnightRank}", ex);
                return StatusCode(500);
            }
        }

        [HttpPost("{catanGameId}")]
        public async Task<IActionResult> AdvanceBarbarians(Guid catanGameId)
        {
            _logger?.LogDebug($"AdvanceBarbarians for game: {catanGameId}");
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsync($"{_gameEndpoint}/AdvanceBarbarians/{catanGameId}", null);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"AdvanceBarbarians Exception for game: {catanGameId}", ex);
                return StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ActivateAllKnightsForPlayer(ActivateAllKnightsForPlayerRequest activateAllKnightsForPlayer)
        {
            _logger?.LogDebug($"ActiveAllKnightsForPlayer for game: {activateAllKnightsForPlayer.GameId} and player: {activateAllKnightsForPlayer.PlayerId}");
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsJsonAsync($"{_gameEndpoint}/ActivateAllKnightsForPlayer", activateAllKnightsForPlayer);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"ActivateAllKnightsForPlayer Exception for game: {activateAllKnightsForPlayer.GameId} and player: {activateAllKnightsForPlayer.PlayerId}", ex);
                return StatusCode(500);
            }
        }

        [HttpPost("{catanGameId}")]
        public async Task<IActionResult> DeactivateAllKnights(Guid catanGameId)
        {
            _logger?.LogDebug($"DeactivateAllKnights for game: {catanGameId}");
            try
            {
                HttpResponseMessage response = await _sHttpCient.PostAsync($"{_gameEndpoint}/DeactivateAllKnights/{catanGameId}", null);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger?.LogError($"DeactivateAllKnights for game: {catanGameId}", ex);
                return StatusCode(500);
            }
        }

        #endregion game and player knights     
  
    }
}