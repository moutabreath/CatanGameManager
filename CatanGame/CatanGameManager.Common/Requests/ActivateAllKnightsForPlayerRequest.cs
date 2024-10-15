using System;

namespace CatanGameManager.API.Requests
{
    public class ActivateAllKnightsForPlayerRequest
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
    }
}
