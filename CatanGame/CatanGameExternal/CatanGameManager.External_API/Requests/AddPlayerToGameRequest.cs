using CatanGameManager.CommonObjects;

namespace CatanGameManager.ExternalAPI.Requests
{
    public class AddPlayerToGameRequest
    {
        public required string UserName { get; set; }
        public required CatanGame CatanGame { get; set; }
    }
}
