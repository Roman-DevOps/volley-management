﻿namespace VolleyManagement.UnitTests.Services.TeamService
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Ninject;
    using VolleyManagement.Contracts;
    using VolleyManagement.Contracts.Exceptions;
    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.Exceptions;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Domain.PlayersAggregate;
    using VolleyManagement.Domain.TeamsAggregate;
    using VolleyManagement.Services;
    using VolleyManagement.UnitTests.Services.PlayerService;
    using Data.Queries.Team;
    using Data.Queries.Player;

    /// <summary>
    /// Tests for TournamentService class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TeamServiceTests
    {
        private const int SPECIFIC_PLAYER_ID = 2;

        private const int SPECIFIC_TEAM_ID = 2;

        private const int UNASSIGNED_ID = 0;

        private readonly TeamServiceTestFixture _testFixture = new TeamServiceTestFixture();

        private readonly Mock<ITeamRepository> _teamRepositoryMock = new Mock<ITeamRepository>();

        private readonly Mock<IPlayerRepository> _playerRepositoryMock = new Mock<IPlayerRepository>();

        private readonly Mock<IQuery<Team, FindByIdCriteria>> _getTeamByIdQueryMock = new Mock<IQuery<Team, FindByIdCriteria>>();

        private readonly Mock<IQuery<Player, FindByIdCriteria>> _getPlayerByIdQueryMock = new Mock<IQuery<Player, FindByIdCriteria>>();

        private readonly Mock<IQuery<Team, FindByCaptainIdCriteria>> _getTeamByCaptainQueryMock = new Mock<IQuery<Team, FindByCaptainIdCriteria>>();

        private readonly Mock<IQuery<List<Team>, GetAllCriteria>> _getAllTeamsQueryMock = new Mock<IQuery<List<Team>, GetAllCriteria>>();

        private readonly Mock<IQuery<List<Player>, TeamPlayersCriteria>> _getTeamRosterQueryMock = new Mock<IQuery<List<Player>, TeamPlayersCriteria>>();

        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>();



        private IKernel _kernel;

        /// <summary>
        /// Initializes test data.
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            _kernel = new StandardKernel();
            _kernel.Bind<ITeamRepository>()
                   .ToConstant(_teamRepositoryMock.Object);
            _kernel.Bind<IPlayerRepository>()
                   .ToConstant(_playerRepositoryMock.Object);
            _kernel.Bind<IQuery<Team, FindByIdCriteria>>()
                   .ToConstant(_getTeamByIdQueryMock.Object);
            _kernel.Bind<IQuery<Player, FindByIdCriteria>>()
                   .ToConstant(_getPlayerByIdQueryMock.Object);
            _kernel.Bind<IQuery<Team, FindByCaptainIdCriteria>>()
                   .ToConstant(_getTeamByCaptainQueryMock.Object);
            _kernel.Bind<IQuery<List<Team>, GetAllCriteria>>()
                   .ToConstant(_getAllTeamsQueryMock.Object);
            _kernel.Bind<IQuery<List<Player>, TeamPlayersCriteria>>()
                   .ToConstant(_getTeamRosterQueryMock.Object);

            _teamRepositoryMock.Setup(tr => tr.UnitOfWork).Returns(_unitOfWorkMock.Object);
            _playerRepositoryMock.Setup(pr => pr.UnitOfWork).Returns(_unitOfWorkMock.Object);
        }

        /// <summary>
        /// Test for Get() method. The method should return existing teams
        /// </summary>
        [TestMethod]
        public void GetAll_TeamsExist_TeamsReturned()
        {
            // Arrange
            var testData = _testFixture.TestTeams().Build();
            MockRepositoryFindAll(testData);

            var expected = new TeamServiceTestFixture()
                                            .TestTeams()
                                            .Build()
                                            .ToList();

            // Act
            var sut = _kernel.Get<TeamService>();
            var actual = sut.Get().ToList();

            // Assert
            CollectionAssert.AreEqual(expected, actual, new TeamComparer());
        }

        /// <summary>
        /// Test for Create() method. The method should create a new team.
        /// </summary>
        [TestMethod]
        public void Create_TeamPassed_TeamCreated()
        {
            // Arrange
            var newTeam = new TeamBuilder().WithId(UNASSIGNED_ID).WithCaptain(SPECIFIC_PLAYER_ID).Build();
            _teamRepositoryMock.Setup(tr => tr.Add(It.Is<Team>(t => t.Id == UNASSIGNED_ID)))
                .Callback<Team>(t => t.Id = SPECIFIC_TEAM_ID);

            int? captainTeamId = null;
            var captain = (new PlayerBuilder()).WithId(SPECIFIC_PLAYER_ID).WithTeamId(captainTeamId).Build();
            _getPlayerByIdQueryMock.Setup(pr => 
                                          pr.Execute(It.Is<FindByIdCriteria>(cr => 
                                                                             cr.Id == captain.Id)))
                                    .Returns(captain);

            // Act
            var sut = _kernel.Get<TeamService>();
            sut.Create(newTeam);

            // Assert
            _teamRepositoryMock.Verify(
                tr => tr.Add(It.Is<Team>(t => TeamsAreEqual(t, newTeam))), Times.Once());
            Assert.AreNotEqual(newTeam.Id, UNASSIGNED_ID);
            _unitOfWorkMock.Verify(u => u.Commit());
        }

        /// <summary>
        /// Test for Create() method. The method check case when captain id is invalid.
        /// Should throw MissingEntityException
        /// </summary>
        [TestMethod]
        public void Create_InvalidCaptainId_MissingEntityExceptionThrown()
        {
            // Arrange
            var newTeam = new TeamBuilder().Build();
            Player testPlayer = new Player() { Id = 2 };
            _getPlayerByIdQueryMock.Setup(pr => pr.Execute(It.Is<FindByIdCriteria>(cr => cr.Id == testPlayer.Id))).Returns(testPlayer);

            // Act
            var sut = _kernel.Get<TeamService>();
            bool gotException = false;
            try
            {
                sut.Create(newTeam);
            }
            catch (MissingEntityException)
            {
                gotException = true;
            }

            // Assert
            _teamRepositoryMock.Verify(tr => tr.Add(It.IsAny<Team>()), Times.Never());
            Assert.IsTrue(gotException);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method. The method check case when captain is captain of another team.
        /// Should throw ValidationException
        /// </summary>
        [TestMethod]
        public void Create_PlayerIsCaptainOfExistingTeam_ValidationExceptionThrown()
        {
            // Arrange
            var newTeam = new TeamBuilder().Build();
            var captain = (new PlayerBuilder()).WithTeamId(SPECIFIC_TEAM_ID).Build();

            var captainLeadTeam = new TeamBuilder().WithId(SPECIFIC_TEAM_ID).Build();
            var testTeams = new TeamServiceTestFixture().AddTeam(captainLeadTeam).Build();
            _getPlayerByIdQueryMock.Setup(pr =>
                            pr.Execute(It.Is<FindByIdCriteria>(cr =>
                                            cr.Id == captain.Id))).
                                            Returns(captain);
            _getTeamByCaptainQueryMock.Setup(tm =>
                            tm.Execute(It.Is<FindByCaptainIdCriteria>(cr =>
                                                                    cr.CaptainId == captain.Id)))
                            .Returns(testTeams.Where(tm => tm.Id == captain.TeamId).FirstOrDefault());

            // Act
            var sut = _kernel.Get<TeamService>();
            bool gotException = false;
            try
            {
                sut.Create(newTeam);
            }
            catch (ValidationException)
            {
                gotException = true;
            }

            // Assert
            _teamRepositoryMock.Verify(tr => tr.Add(It.IsAny<Team>()), Times.Never());
            Assert.IsTrue(gotException);
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method. The method check that captain's teamId
        /// was updated after creating team in DB
        /// </summary>
        [TestMethod]
        public void Create_TeamPassed_CaptainUpdated()
        {
            // Arrange
            var newTeam = new TeamBuilder().WithCaptain(SPECIFIC_PLAYER_ID).Build();
            _teamRepositoryMock.Setup(tr => tr.Add(It.IsAny<Team>()))
                .Callback<Team>(t => t.Id = SPECIFIC_TEAM_ID);

            int? captainTeamId = null;
            var captain = (new PlayerBuilder())
                                        .WithId(SPECIFIC_PLAYER_ID)
                                        .WithTeamId(captainTeamId)
                                        .Build();

            _getPlayerByIdQueryMock.Setup(pr =>
                                          pr.Execute(It.Is<FindByIdCriteria>(
                                              cr =>
                                              cr.Id == captain.Id)))
                                    .Returns(captain);

            // Act
            var sut = _kernel.Get<TeamService>();
            sut.Create(newTeam);

            // Assert
            _teamRepositoryMock.Verify(tr => tr.Add(It.IsAny<Team>()), Times.Once());
            _playerRepositoryMock.Verify(pr => pr.Update(It.Is<Player>(p => p.Id == SPECIFIC_PLAYER_ID)), Times.Once());
            Assert.AreEqual(captain.TeamId, SPECIFIC_TEAM_ID);
        }

        /// <summary>
        /// Test for Delete() method. The method should delete specified team.
        /// </summary>
        [TestMethod]
        public void Delete_TeamPassed_CorrectIdPostedToDatabaseLayer()
        {
            // Act
            var ts = _kernel.Get<TeamService>();
            ts.Delete(SPECIFIC_TEAM_ID);

            // Assert
            _teamRepositoryMock.Verify(tr => tr.Remove(It.Is<int>(teamId => teamId == SPECIFIC_TEAM_ID)), Times.Once());
            _unitOfWorkMock.Verify(tr => tr.Commit());
        }

        /// <summary>
        /// Test for Delete() method. The method check case when team id is invalid.
        /// Should throw MissingEntityException
        /// </summary>
        [TestMethod]
        public void Delete_InvalidTeamId_MissingEntityExceptionThrown()
        {
            // Arrange
            _teamRepositoryMock.Setup(tr => tr.Remove(It.IsAny<int>()))
                .Throws(new InvalidKeyValueException());

            // Act
            var ts = _kernel.Get<TeamService>();
            bool gotException = false;
            try
            {
                ts.Delete(SPECIFIC_TEAM_ID);
            }
            catch (MissingEntityException)
            {
                gotException = true;
            }

            // Assert
            _teamRepositoryMock.Verify(tr => tr.Remove(It.Is<int>(teamId => teamId == SPECIFIC_TEAM_ID)), Times.Once());
            Assert.IsTrue(gotException);
            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Never());
        }

        /// <summary>
        /// Successful test for Delete() method.
        /// </summary>
        [TestMethod]
        public void Delete_TeamPassed_TeamDeleted()
        {
            // Act
            var ts = _kernel.Get<TeamService>();
            ts.Delete(SPECIFIC_TEAM_ID);

            // Assert
            _teamRepositoryMock.Verify(tr => tr.Remove(It.Is<int>(teamId => teamId == SPECIFIC_TEAM_ID)), Times.Once());
            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Once());
        }

        /// <summary>
        /// Successful test for Delete() method.
        /// </summary>
        [TestMethod]
        public void Delete_TeamPassed_RosterPlayersUpdated()
        {
            // Arrange
            IQueryable<Player> expectedRoster = new PlayerServiceTestFixture().TestPlayers().Build().AsQueryable<Player>();
            int expectedCountOfPlayers = expectedRoster.Count();
            ////_playerRepositoryMock.Setup(pr => pr.FindWhere(It.IsAny<Expression<Func<Player, bool>>>()))
            ////    .Returns(expectedRoster);

            // Act
            var ts = _kernel.Get<TeamService>();
            ts.Delete(SPECIFIC_TEAM_ID);

            // Assert
            _playerRepositoryMock.Verify(
                                         pr => pr.Update(It.Is<Player>(player => expectedRoster.Contains(player))),
                                         Times.Exactly(expectedCountOfPlayers));

            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Once());
        }

        /// <summary>
        /// Test for SetPlayerTeam() method.
        /// Case when specified player isn't exist. Throw MissingEntityException
        /// </summary>
        [TestMethod]
        public void UpdatePlayerTeam_InvalidPlayerId_MissingEntityExceptionThrown()
        {
            // Arrange
            MockPlayerRepositoryToFindPlayer(null);

            // Act
            var ts = _kernel.Get<TeamService>();
            bool gotException = false;
            try
            {
                ts.UpdatePlayerTeam(SPECIFIC_PLAYER_ID, SPECIFIC_TEAM_ID);
            }
            catch (MissingEntityException)
            {
                gotException = true;
            }

            // Assert
            _playerRepositoryMock.Verify(pr => pr.Update(It.IsAny<Player>()), Times.Never());
            Assert.IsTrue(gotException);
            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for SetPlayerTeam() method.
        /// Case when specified team isn't exist. Throw MissingEntityException
        /// </summary>
        [TestMethod]
        public void UpdatePlayerTeam_InvalidTeamId_MissingEntityExceptionThrown()
        {
            // Arrange
            MockPlayerRepositoryToFindPlayer(new PlayerBuilder().WithTeamId(null).Build());
            var emptyTeamList = new TeamServiceTestFixture().Build().AsQueryable<Team>();
            ////_teamRepositoryMock.Setup(tr => tr.FindWhere(It.IsAny<Expression<Func<Team, bool>>>()))
            ////                   .Returns(emptyTeamList);

            // Act
            var ts = _kernel.Get<TeamService>();
            bool gotException = false;
            try
            {
                ts.UpdatePlayerTeam(SPECIFIC_PLAYER_ID, SPECIFIC_TEAM_ID);
            }
            catch (MissingEntityException)
            {
                gotException = true;
            }

            // Assert
            _playerRepositoryMock.Verify(
                                         pr => pr.Update(It.Is<Player>(player => player.Id == SPECIFIC_PLAYER_ID)),
                                         Times.Never());
            Assert.IsTrue(gotException);
            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for SetPlayerTeam() method.
        /// Case when edited player's is captain of existing team and
        /// new teamId is null or not equal Id of existing team
        /// The method should throw InvalidOperationException.
        /// </summary>
        [TestMethod]
        public void UpdatePlayerTeam_PlayerIsCaptainOfExistingTeam_ValidationExceptionThrown()
        {
            // Arrange
            MockPlayerRepositoryToFindPlayer(new PlayerBuilder()
                                                .WithId(SPECIFIC_PLAYER_ID)
                                                .WithTeamId(SPECIFIC_TEAM_ID)
                                                .Build());

            var existingTeam = new TeamBuilder().WithId(SPECIFIC_TEAM_ID).WithCaptain(SPECIFIC_PLAYER_ID).Build();

            // next operation guarantee that new teamId will distinct from SPECIFIC_TEAM_ID
            int newTeamId = SPECIFIC_TEAM_ID + 1;
            var teamToSet = new TeamBuilder().WithId(newTeamId).Build();

            var teamList = new TeamServiceTestFixture().AddTeam(existingTeam).AddTeam(teamToSet).Build().AsQueryable();

            ////_teamRepositoryMock.Setup(tr => tr.FindWhere(It.IsAny<Expression<Func<Team, bool>>>()))
            ////                   .Returns((Expression<Func<Team, bool>> whereExpr) => teamList.Where(whereExpr));

            // Act
            var ts = _kernel.Get<TeamService>();
            bool gotException = false;
            try
            {
                ts.UpdatePlayerTeam(SPECIFIC_PLAYER_ID, newTeamId);
            }
            catch (ValidationException)
            {
                gotException = true;
            }

            // Assert
            _playerRepositoryMock.Verify(pr => pr.Update(It.IsAny<Player>()), Times.Never());
            Assert.IsTrue(gotException);
            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Never());
        }

        /// <summary>
        /// Successful Test for SetPlayerTeam() method.
        /// </summary>
        [TestMethod]
        public void UpdatePlayerTeam_PlayerAndTeamPassed_PlayerUpdated()
        {
            // Arrange
            MockPlayerRepositoryToFindPlayer(new PlayerBuilder().WithId(SPECIFIC_PLAYER_ID).WithTeamId(null).Build());
            var teamToSet = new TeamBuilder().WithId(SPECIFIC_TEAM_ID).Build();

            var teamList = new TeamServiceTestFixture().AddTeam(teamToSet).Build().AsQueryable();
            ////_teamRepositoryMock.Setup(tr => tr.FindWhere(It.IsAny<Expression<Func<Team, bool>>>()))
            ////                   .Returns(teamList);

            // Act
            var ts = _kernel.Get<TeamService>();
            ts.UpdatePlayerTeam(SPECIFIC_PLAYER_ID, SPECIFIC_TEAM_ID);

            // Assert
            _playerRepositoryMock.Verify(
                    pr => pr.Update(It.Is<Player>(p => p.Id == SPECIFIC_PLAYER_ID && p.TeamId == SPECIFIC_TEAM_ID)),
                    Times.Once());

            _unitOfWorkMock.Verify(tr => tr.Commit(), Times.Once());
        }

        private bool TeamsAreEqual(Team x, Team y)
        {
            return new TeamComparer().Compare(x, y) == 0;
        }

        private void MockRepositoryFindAll(IEnumerable<Team> testData)
        {
            ////_teamRepositoryMock.Setup(tr => tr.Find()).Returns(testData.AsQueryable());
        }

        private void MockPlayerRepositoryToFindPlayer(Player player)
        {
            IQueryable<Player> listOfPlayers = (new PlayerServiceTestFixture())
                                                .AddPlayer(player).Build()
                                                .AsQueryable();

            //     _playerRepositoryMock.Setup(pr => pr.FindWhere(It.IsAny<Expression<Func<Player, bool>>>()))
            //                                       .Returns(listOfPlayers);
        }
    }
}
