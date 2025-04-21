using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.ExternalAPI.Requests
{
    public class AddPlayerVictoryPointRequest
    {
        public required CatanGame Game {get; set;}
        public required ActivePlayer ActivePlayer { get; set; }
        public required VPType UpdateType { get; set; }
    }
}
