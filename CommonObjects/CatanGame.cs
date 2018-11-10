using System;
using System.Collections.Generic;
using CatanGameManager.CommonObjects.User;

namespace CatanGameManager.CommonObjects
{
    public class CatanGame: Entity
    {
        public int BanditsDistance { get; set; }
        public List<ActivePlayer> ActivePlayers { get; set; }

        public List <Tuple<int, int>> RecentDiceRolls { get; set; }
        public int NumberOfTotalKnights = 0;
        public int BanditsStrength = 0;
    }
}
