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
        Task<bool> UpdateGame(CatanGame catanGame);
        Task<CatanGame> GetGame(Guid gameId);
        Task <bool> RemoveGame(CatanGame catanGame);
        Task<IEnumerable<CatanGame>> GetUserActiveGames(string userName);
        
        Task<bool> UpdatePlayerInGame(CatanGame catanGame, ActivePlayer playerToUpdate);

        Task<int> GetGameTotalActiveKnights(Guid gameId);
        Task<bool> AddPlayerKnight(Guid gameId, Guid activePlayerId, KnightRank knightRank);
        Task<bool> AdvanceBarbarians(Guid gameId);
        Task<bool> ActivateAllKnightsForPlayer(Guid gameId, Guid playerId);
        Task<bool> DeactivateAllKnights(Guid gameId);
        
    }
}
