using CatanGameManager.CommonObjects;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.API.Requests
{
    public class AddPlayerVictoryPointRequest
    {
        public CatanGame Game {get; set;}
        public ActivePlayer ActivePlayer { get; set; }
        public VPType UpdateType { get; set; }
    }
}
