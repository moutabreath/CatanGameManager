using System;
using System.Collections.Generic;
using CatanGameManager.CommonObjects.Enums;

namespace CatanGameManager.CommonObjects.User
{
    public class ActivePlayer: Entity
    {
        public required string UserName { get; set; }
        public int NumOfSettlements { get; set; }
        public int NumOfCities { get; set; }
        public int NumOfContinousRoads { get; set; }
        public int NumOfRoadsLeft { get; set; }
        public int NumOfActiveKnights { get; set; }
        public int NumOfTotalKnights { get; set; }
        public int AmountOfVictoryPoints { get; set; }
        public int SaviorOfCatanVP { get; set; }
        public int NumOfVictoryPoints { get; set; }
        public int SpecialVictoryPoints { get; set; }
        public required IList<VPType.InterChanageableVP> InterChanageableVPs { get; set; } = [];

    }
}
