using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.Config;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.CommonObjects;
using CatanGameManager.Core;
using CatanGameManager.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatanGamePersistence.MongoDB;

namespace CatanGameManager.Tests
{
    [TestClass]
    public class GameManagerTests
    {
        private ICatanUserBusinessLogic _catanPlayerBusinessLogic;
        private ICatanGameBusinessLogic _catanGameBusinessLogic;

        private const string PlayerEmail1 = "player1@email.com";
        private const string PlayerEmail2 = "player2@email.com";

        private const string Password = "pass";


        [TestInitialize]
        public async Task Init()
        {
            CatanManagerConfig config = new CatanManagerConfig
            {
                MongoConnectionString = "mongodb://myAdmin:simplePassword@localhost/catanHelperTest?authSource=admin",
                MongoDbName = "CatanGameTest"
            };            
            IOptions<CatanManagerConfig> someOptions = Options.Create(config);
            _catanPlayerBusinessLogic = new CatanUserBusinessLogic(null, new CatanUserMongoPersist(null, someOptions));
            _catanGameBusinessLogic = new CatanGameBusinessLogic(null, new CatanGameMongoPersist(null, someOptions));

            await Cleanup();
            await AddNewPlayer();
            await UpdateGameAndAddPlayer();
        }

        private async Task<CatanGame> GetGame()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Id);
            return playerGames.FirstOrDefault();
        }

        private async Task AddNewPlayer()
        {
            UserProfile playerProfile = new UserProfile
            {
                Email = PlayerEmail1,
                FirstName = "Some",
                LastName = "Some",
                Name = "someSomeone",
                Password = Password
            };
            await _catanPlayerBusinessLogic.RegisterPlayer(playerProfile);
        }       

        private async Task UpdateGameAndAddPlayer()
        {
            List<ActivePlayer> players = new List<ActivePlayer>();
            CatanGame catanGame = new CatanGame
            {
                ActivePlayers = players,
                BanditsDistance = 7,
                BanditsStrength = 4

            };
            await _catanGameBusinessLogic.UpdateGame(catanGame);
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);

            UserProfile playerProfile2 = new UserProfile
            {
                Email = PlayerEmail2,
                FirstName = "Some",
                LastName = "Some",
                Name = "someSomeone2",
                Password = Password
            };
            await _catanPlayerBusinessLogic.UpdatePlayer(playerProfile2);
            await _catanGameBusinessLogic.AddPlayerToGame(catanGame, playerProfile2.Id, playerProfile2.Email);
            await _catanGameBusinessLogic.AddPlayerToGame(catanGame, playerProfile.Id, playerProfile.Email);
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            if (playerProfile == null) return;
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Id);
            foreach (CatanGame playerGame in playerGames)
            {
                await _catanGameBusinessLogic.RemoveGame(playerGame);
            }

            await _catanPlayerBusinessLogic.UnRegisterUser(playerProfile.Id);
        }

        [TestMethod]
        public async Task TestGetPlayer()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            Assert.IsNotNull(playerProfile);
            Assert.AreEqual(playerProfile.Email, PlayerEmail1);
        }

        [TestMethod]
        public async Task TestGetPlayerActiveGames()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Id);
            CatanGame theCatanGame = playerGames.FirstOrDefault();
            Assert.IsNotNull(theCatanGame);
            Assert.IsTrue(theCatanGame.ActivePlayers.Select(activePlayer => activePlayer.UserId).Contains(playerProfile.Id));
        }        
      
        [TestMethod]
        public async Task TestAddPlayerKnight()
        {
            CatanGame theCatanGame = await GetGame();
            ActivePlayer activePlayer = theCatanGame.ActivePlayers.FirstOrDefault();

            await _catanGameBusinessLogic.AddPlayerKnight(theCatanGame.Id, activePlayer.Id, KnightRank.Basic);
            theCatanGame = await GetGame();
            activePlayer = theCatanGame.ActivePlayers.FirstOrDefault();
            Assert.AreEqual(1, activePlayer.NumOfTotalKnights);

            await _catanGameBusinessLogic.AddPlayerKnight(theCatanGame.Id, activePlayer.Id, KnightRank.Strong);
            theCatanGame = await GetGame();
            activePlayer = theCatanGame.ActivePlayers.FirstOrDefault();
            Assert.AreEqual(3, activePlayer.NumOfTotalKnights);

            await _catanGameBusinessLogic.AddPlayerKnight(theCatanGame.Id, activePlayer.Id, KnightRank.Mighty);
            theCatanGame = await GetGame();
            activePlayer = theCatanGame.ActivePlayers.FirstOrDefault();
            Assert.AreEqual(6, activePlayer.NumOfTotalKnights);
        }

        [TestMethod]
        public async Task TestActivateAllKnightsForPlayer()
        {
            CatanGame theCatanGame = await GetGame();
            ActivePlayer activePlayer = theCatanGame.ActivePlayers.FirstOrDefault();
            await TestAddPlayerKnight();
            await _catanGameBusinessLogic.ActivateAllKnightsForPlayer(theCatanGame.Id, activePlayer.Id);

            theCatanGame = await GetGame();
            activePlayer = theCatanGame.ActivePlayers.FirstOrDefault();

            Assert.AreEqual(6, activePlayer.NumOfActiveKnights);
        }

        [TestMethod]
        public async Task TestDeactivateAllKnights()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Id);
            CatanGame theCatanGame = playerGames.FirstOrDefault();
            await _catanGameBusinessLogic.DeactivateAllKnights(theCatanGame.Id);

            playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Id);
            theCatanGame = playerGames.FirstOrDefault();

            foreach (ActivePlayer activePlayer in theCatanGame.ActivePlayers)
            {
                Assert.AreEqual(0, activePlayer.NumOfActiveKnights);
            }
        }

        [TestMethod]
        public async Task TestAdvanceBarbarians()
        {
            CatanGame theGame = await GetGame();
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            theGame = await GetGame();
            Assert.AreEqual(6, theGame.BanditsDistance);
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            await _catanGameBusinessLogic.AdvanceBarbarians(theGame.Id);
            theGame = await GetGame();
            Assert.AreEqual(0, theGame.BanditsDistance);
        }

        [TestMethod]
        public async Task TestAddPlayerVictoryPoint()
        {
            CatanGame theGame = await GetGame();
            int firstPlayerExpectedVPs = 0;
            int secondPlayerExpectedVPs = 0;

            VPType vpType = new VPType(VPType.UpdateType.City);
            firstPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            int numOfCities = theGame.ActivePlayers.FirstOrDefault().NumOfCities;
            Assert.AreEqual(1, numOfCities);
            Assert.AreEqual(0, theGame.ActivePlayers.FirstOrDefault().NumOfSettlements);
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.Settlment);
            firstPlayerExpectedVPs ++;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.LongestRoad);
            firstPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.Merchant);
            firstPlayerExpectedVPs++;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);


            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.MetropolisCloth);
            firstPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);


            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.MetropolisCoin);
            firstPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.MetropolisPaper);
            firstPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.SaviorOfCatan);
            firstPlayerExpectedVPs++;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.FirstOrDefault(), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            // Replace Interchangeable
            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.LongestRoad);
            firstPlayerExpectedVPs -= 2;
            secondPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.ElementAt(1), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.Merchant);
            firstPlayerExpectedVPs--;
            secondPlayerExpectedVPs++;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.ElementAt(1), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);


            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.MetropolisCloth);
            firstPlayerExpectedVPs -= 2;
            secondPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.ElementAt(1), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);


            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.MetropolisCoin);
            firstPlayerExpectedVPs -= 2;
            secondPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.ElementAt(1), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.Interchangeable, VPType.InterChanageableVP.MetropolisPaper);
            firstPlayerExpectedVPs -= 2;
            secondPlayerExpectedVPs += 2;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.ElementAt(1), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);

            vpType = new VPType(VPType.UpdateType.SaviorOfCatan);
            secondPlayerExpectedVPs++;
            await _catanGameBusinessLogic.AddPlayerVictoryPoint(theGame, theGame.ActivePlayers.ElementAt(1), vpType);
            theGame = await GetGame();
            Assert.AreEqual(firstPlayerExpectedVPs, theGame.ActivePlayers.FirstOrDefault().NumOfVictoryPoints);
            Assert.AreEqual(secondPlayerExpectedVPs, theGame.ActivePlayers.ElementAt(1).NumOfVictoryPoints);
        }
    }
}