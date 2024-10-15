using CatanGameManager.CommonObjects;

namespace CatanGameManager.API.Requests
{
    public class AddPlayerToGameRequest
    {
        public string UserName { get; set; }
        public CatanGame CatanGame { get; set; }
    }
}
