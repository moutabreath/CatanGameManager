using CatanGameManager.CommonObjects.Enums;
using System;

namespace CatanGameManager.ExternalAPI.Requests
{
    public class AddPlayerKnightRequest
    {
        public Guid GameId { get; set; }
        public Guid ActivePlayerId { get; set; }
        public KnightRank KnightRank { get; set; }
    }
}
