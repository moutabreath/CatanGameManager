using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.Interfaces.PersistanceInterfaces
{
    public interface ICatanGamePersist
    {
        Task UpdateGame(CatanGame catanGame);

        Task<CatanGame> GetGame(Guid gameId);

        Task<IEnumerable<CatanGame>> GetPlayerActiveGames(Guid playedId);

        Task UpdatePlayerInGame(CatanGame catanGame, ActivePlayer playerToUpdate);

        Task DeactivateAllKnights(Guid catanGameId);

        Task ActivateAllKnightsForPlayer(Guid catanGameId, Guid activePlayerId);
        Task AdvanceBarbarians(Guid gameId);
        Task AddPlayerVictoryPoint(Guid gameId, Guid activePlayerId, VPType updateType);
        Task AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank);
        Task RemoveGame(Guid id);
        Task<int> GetTotalActiveKnights(Guid gameId);
    }
}
