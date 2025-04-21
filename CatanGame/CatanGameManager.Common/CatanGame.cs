using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.CommonObjects
{
    public class CatanGame: Entity
    {
        public int BanditsDistance { get; set; }
        public required List<ActivePlayer> ActivePlayers { get; set; } = [];
        public required List<Tuple<int, int>> RecentDiceRolls { get; set; } = [];
        public int BanditsStrength = 0;
    }
}
