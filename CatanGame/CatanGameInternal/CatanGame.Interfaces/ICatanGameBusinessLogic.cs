using CatanGameManager.CommonObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces
{
    public interface ICatanGameBusinessLogic
    {
        Task UpdateGame(CatanGame catanGame);
        Task<CatanGame> GetGame(Guid gameId);
        Task RemoveGame(CatanGame catanGame);        
        Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName);
        Task AddPlayerToGame(CatanGame catanGame, string userName);

        Task AddPlayerVictoryPoint(CatanGame catanGame, ActivePlayer activePlayer, VPType updateType);
        

        Task<int> GetGameTotalActiveKnights(Guid gameId);
        Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank);
        Task AdvanceBarbarians(Guid gameId);
        Task ActivateAllKnightsForPlayer(Guid gameId, Guid playerId);
        Task DeactivateAllKnights(Guid gameId);
        
        

        
    }
}
