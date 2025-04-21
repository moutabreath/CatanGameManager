using System;

namespace CatanGameManager.ExternalAPI.Requests
{
    public class ActivateAllKnightsForPlayerRequest
    {
        public Guid GameId { get; set; }
        public Guid PlayerId { get; set; }
    }
}
