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
        Task RemoveGame(Guid id);
        Task<CatanGame> GetGame(Guid gameId);
        Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId);
        Task AddPlayerToGame(CatanGame catanGame, PlayerProfile playerProfile);

        Task ActivateAllKnightsForPlayer(Guid gameId, Guid playerId);
        Task DeactivateAllKnights(Guid gameId);
        Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank);
        Task AdvanceBarbarians(Guid gameId);
        Task<int> GetGameTotalActiveKnights(Guid gameId);

        Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType);
        Task<int> GetTotalActiveKnights(Guid gameId);

        Task<int> GetPlayerTotalVps(ActivePlayer activePlayer);
    }
}
