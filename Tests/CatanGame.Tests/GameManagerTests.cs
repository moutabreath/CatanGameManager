using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CatanGameManager.CommonObjects.Enums;
using CatanGameManager.CommonObjects.User;
using CatanGameManager.CommonObjects;
using CatanGameManager.Core;
using CatanGameManager.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatanGamePersistence.MongoDB;
using CommonLib.Config;
using Mongo2Go;
using Microsoft.Extensions.DependencyInjection;
using CatanGameManager.Interfaces.PersistanceInterfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization;
using CatanGameManger.Persistence.MongoDB;

namespace CatanGameManager.Tests
{
    [TestClass]
    public class GameManagerTests
    {
        private static ServiceProvider _serviceProvider;

        private static MongoDbRunner _mongoRunner;

        private ICatanUserBusinessLogic _catanPlayerBusinessLogic;
        private ICatanGameBusinessLogic _catanGameBusinessLogic;

        private const string PlayerEmail1 = "player1@email.com";
        private const string PlayerEmail2 = "player2@email.com";

        private const string Password = "pass";


        [ClassInitialize]
        public static void ConfigureIoC(TestContext testContext)
        {
            var services = new ServiceCollection();

            services.AddLogging();

            InitMongo(services);

            services.AddScoped<ICatanUserBusinessLogic, CatanUserBusinessLogic>();
            services.AddScoped<ICatanGameBusinessLogic, CatanGameBusinessLogic>();
            services.AddScoped<ICatanUserPersist, CatanUserMongoPersist>();
            services.AddScoped<ICatanGamePersist, CatanGameMongoPersist>();

            _serviceProvider = services.BuildServiceProvider();

        }

        private static void InitMongo(ServiceCollection services)
        {
            _mongoRunner = MongoDbRunner.Start();
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
            var config = new MongoConfig
            {
                MongoConnectionString = _mongoRunner.ConnectionString,
                MongoDatabaseName = "CatanGameTest",
                MongoGameDocumentName = "CatanGame",
                MongoPlayerDocumentName = "PlayerProfile"
            };


            IOptions<MongoConfig> someOptions = Options.Create(config);
            services.AddSingleton(Options.Create(someOptions.Value));
        }

        [TestCleanup]
        public async Task Cleanup()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            if (playerProfile == null) return;

            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Email);
            foreach (CatanGame playerGame in playerGames)
            {
                await _catanGameBusinessLogic.RemoveGame(playerGame);
            }

            await _catanPlayerBusinessLogic.UnRegisterUser(playerProfile.Id);
        }

        [TestInitialize]
        public async Task PopulateMockData()
        {
            _catanPlayerBusinessLogic = _serviceProvider.GetRequiredService<ICatanUserBusinessLogic>();
            _catanGameBusinessLogic = _serviceProvider.GetRequiredService<ICatanGameBusinessLogic>();
            await AddNewPlayer();
            await UpdateGameAndAddPlayer();
        }

        private async Task<CatanGame> GetGame()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Email);
            return playerGames.FirstOrDefault();
        }

        private async Task AddNewPlayer()
        {
            UserProfile playerProfile = new()
            {
                Email = PlayerEmail1,
                FirstName = "Some",
                LastName = "Some",
                Name = "someSomeone",
                Password = Password
            };
            bool result = await _catanPlayerBusinessLogic.RegisterPlayer(playerProfile);
        }

        private async Task UpdateGameAndAddPlayer()
        {
            List<ActivePlayer> players = [];
            CatanGame catanGame = new()
            {
                ActivePlayers = players,
                BanditsDistance = 7,
                BanditsStrength = 4,
                RecentDiceRolls = []
            };
            await _catanGameBusinessLogic.UpdateGame(catanGame);
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);

            UserProfile playerProfile2 = new()
            {
                Email = PlayerEmail2,
                FirstName = "Some",
                LastName = "Some",
                Name = "someSomeone2",
                Password = Password
            };
            bool playerUpdated = await _catanPlayerBusinessLogic.UpdatePlayer(playerProfile2);
            bool playerAddedToGame = await _catanGameBusinessLogic.AddPlayerToGame(catanGame, playerProfile2.Email);
            bool player2AddedToGame = await _catanGameBusinessLogic.AddPlayerToGame(catanGame, playerProfile.Email);
        }


        [TestMethod]
        public async Task TestGetPlayer()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            Assert.IsNotNull(playerProfile);
            Assert.AreEqual(PlayerEmail1, playerProfile.Email);
        }

        [TestMethod]
        public async Task TestGetPlayerActiveGames()
        {
            UserProfile playerProfile = await _catanPlayerBusinessLogic.GetUser(PlayerEmail1, Password);
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Email);
            CatanGame theCatanGame = playerGames.FirstOrDefault();
            Assert.IsNotNull(theCatanGame);
            Assert.IsTrue(theCatanGame.ActivePlayers.Select(activePlayer => activePlayer.UserName).Contains(playerProfile.Email));
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
            IEnumerable<CatanGame> playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Email);
            CatanGame theCatanGame = playerGames.FirstOrDefault();
            await _catanGameBusinessLogic.DeactivateAllKnights(theCatanGame.Id);

            playerGames = await _catanGameBusinessLogic.GetUserActiveGames(playerProfile.Email);
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
            firstPlayerExpectedVPs++;
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