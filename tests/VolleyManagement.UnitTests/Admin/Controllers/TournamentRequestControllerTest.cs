using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Domain.PlayersAggregate;
using VolleyManagement.Domain.TeamsAggregate;
using VolleyManagement.Domain.TournamentRequestAggregate;
using VolleyManagement.Domain.TournamentsAggregate;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UI.Areas.Admin.Controllers;
using VolleyManagement.UI.Areas.Admin.Models;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Players;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Teams;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Tournaments;
using VolleyManagement.UnitTests.Admin.Comparers;
using VolleyManagement.UnitTests.Admin.ViewModels;
using VolleyManagement.UnitTests.Mvc.ViewModels;
using VolleyManagement.UnitTests.Services.PlayerService;
using VolleyManagement.UnitTests.Services.TeamService;
using VolleyManagement.UnitTests.Services.TournamentRequestService;
using VolleyManagement.UnitTests.Services.TournamentService;
using VolleyManagement.UnitTests.Services.UsersService;
using Xunit;

namespace VolleyManagement.UnitTests.Admin.Controllers
{
    [ExcludeFromCodeCoverage]
    public class TournamentRequestControllerTest
    {
        public TournamentRequestControllerTest()
        {
            _requestServiceMock = new Mock<ITournamentRequestService>();
            _tournamentServiceMock = new Mock<ITournamentService>();
            _teamServiceMock = new Mock<ITeamService>();
            _userServiceMock = new Mock<IUserService>();
            _httpContextMock = new Mock<HttpContextBase>();
            _httpRequestMock = new Mock<HttpRequestBase>();
        }

        private const string ACHIEVEMENTS = "TestAchievements";
        private const string TEAM_NAME = "TestName";
        private const string PLAYER_FIRSTNAME = "Test";
        private const string PLAYER_LASTNAME = "Test";
        private const string COACH = "TestCoach";
        private const int SPECIFIED_TEAM_ID = 4;
        private const int SPECIFIED_PLAYER_ID = 4;
        private const int TEST_TEAM_ID = 1;
        private const int TEST_GROUP_ID = 1;
        private const int TEST_TOURNAMENT_ID = 1;
        private const int REQUEST_ID = 1;
        private const int USER_ID = 1;
        private const string TEST_MESSAGE = "Hello, user!";

        private readonly Mock<ITournamentRequestService> _requestServiceMock = new Mock<ITournamentRequestService>();
        private readonly Mock<ITournamentService> _tournamentServiceMock = new Mock<ITournamentService>();
        private readonly Mock<ITeamService> _teamServiceMock = new Mock<ITeamService>();
        private readonly Mock<IUserService> _userServiceMock = new Mock<IUserService>();
        private Mock<HttpContextBase> _httpContextMock = new Mock<HttpContextBase>();
        private Mock<HttpRequestBase> _httpRequestMock = new Mock<HttpRequestBase>();

        private static void AssertValidRedirectResult(ActionResult actionResult, string view)
        {
            var result = (RedirectToRouteResult) actionResult;
            Assert.NotNull(result);
            Assert.False(result.Permanent, "Redirect should not be permanent");
            result.RouteValues.Count.Should()
                .Be(1, string.Format("Redirect should forward to Requests.{0} action", view));
            result.RouteValues["action"].Should()
                .Be(view, string.Format("Redirect should forward to Requests.{0} action", view));
        }

        private TournamentRequestController BuildSUT()
        {
            return new TournamentRequestController(
                _requestServiceMock.Object,
                _userServiceMock.Object,
                _teamServiceMock.Object,
                _tournamentServiceMock.Object);
        }

        private void MockTeamServiceGetTeam(Team team)
        {
            _teamServiceMock.Setup(ts => ts.Get(It.IsAny<int>())).Returns(team);
        }

        private void SetupGet(int teamId, Team team)
        {
            _teamServiceMock.Setup(tr => tr.Get(teamId)).Returns(team);
        }

        private List<PlayerNameViewModel> CreateRoster()
        {
            const int FIRST_PLAYER_ID = 1;
            const string FIRST_PLAYER_FIRSTNAME = "FirstNameA";
            const string FIRST_PLAYER_LASTNAME = "LastNameA";
            const int SECOND_PLAYER_ID = 2;
            const string SECOND_PLAYER_FIRSTNAME = "FirstNameB";
            const string SECOND_PLAYER_LASTNAME = "LastNameB";
            const int THIRD_PLAYER_ID = 3;
            const string THIRD_PLAYER_FIRSTNAME = "FirstNameC";
            const string THIRD_PLAYER_LASTNAME = "LastNameC";
            var roster = new List<PlayerNameViewModel> {
                CreatePlayerNameModel(FIRST_PLAYER_FIRSTNAME, FIRST_PLAYER_LASTNAME, FIRST_PLAYER_ID),
                CreatePlayerNameModel(SECOND_PLAYER_FIRSTNAME, SECOND_PLAYER_LASTNAME, SECOND_PLAYER_ID),
                CreatePlayerNameModel(THIRD_PLAYER_FIRSTNAME, THIRD_PLAYER_LASTNAME, THIRD_PLAYER_ID),
                CreatePlayerNameModel(PLAYER_FIRSTNAME, PLAYER_LASTNAME, SPECIFIED_PLAYER_ID)
            };

            return roster;
        }

        private TeamViewModel CreateTeamViewModel()
        {
            var cap = CreatePlayerNameModel(PLAYER_FIRSTNAME, PLAYER_LASTNAME, SPECIFIED_PLAYER_ID);
            var players = CreateRoster();
            return new TeamMvcViewModelBuilder()
                .WithId(SPECIFIED_TEAM_ID)
                .WithCoach(COACH)
                .WithAchievements(ACHIEVEMENTS)
                .WithName(TEAM_NAME)
                .WithCaptain(cap)
                .WithRoster(players)
                .Build();
        }

        private PlayerNameViewModel CreatePlayerNameModel(string firstname, string lastname, int id)
        {
            return new PlayerNameViewModel {
                FirstName = firstname,
                LastName = lastname,
                Id = id
            };
        }

        private Team CreateTeam()
        {
            return new TeamBuilder()
                .WithId(SPECIFIED_TEAM_ID)
                .WithCaptain(SPECIFIED_PLAYER_ID)
                .WithCoach(COACH)
                .WithAchievements(ACHIEVEMENTS)
                .WithName(TEAM_NAME)
                .Build();
        }

        private Player CreatePlayer(int id)
        {
            return new PlayerBuilder()
                .WithId(id)
                .WithFirstName(PLAYER_FIRSTNAME)
                .WithLastName(PLAYER_LASTNAME)
                .WithTeamId(SPECIFIED_TEAM_ID)
                .Build();
        }

        private Tournament MakeTestTournament(int tournamentId)
        {
            return new TournamentBuilder().WithId(tournamentId).Build();
        }

        private TournamentViewModel MakeTestTournamentViewModel(int tournamentId)
        {
            return new TournamentMvcViewModelBuilder().WithId(tournamentId).Build();
        }

        private List<TournamentRequest> MakeTestTournamentsRequests()
        {
            return new TournamentRequestServiceTestFixture().TestRequests().Build();
        }

        private void MockTournamentServiceGetTournament(int tournamentId, Tournament tournament)
        {
            _tournamentServiceMock.Setup(tr => tr.Get(tournamentId)).Returns(tournament);
        }

        private void AssertVerifyConfirm(int requestId)
        {
            _requestServiceMock.Verify(m => m.Confirm(It.Is<int>(id => id == requestId)), Times.Once());
        }

        private void SetupConfirmThrowsMissingEntityException()
        {
            _requestServiceMock.Setup(m => m.Confirm(It.IsAny<int>()))
                .Throws(new MissingEntityException(string.Empty));
        }

        private User CreateUser()
        {
            return new UserBuilder()
                .WithId(USER_ID)
                .Build();
        }

        private UserViewModel CreateUserViewModel()
        {
            var player = CreatePlayerViewModel();

            return new UserAdminViewModelBuilder().Build();
        }

        private PlayerViewModel CreatePlayerViewModel()
        {
            return new PlayerMvcViewModelBuilder().Build();
        }

        private void MockUserServiceGetUserDetails(int userId, User user)
        {
            _userServiceMock.Setup(tr => tr.GetUserDetails(userId)).Returns(user);
        }

        private MessageViewModel CreateMessageViewModel()
        {
            return new MessageViewModel {
                Id = REQUEST_ID,
                Message = TEST_MESSAGE
            };
        }

        private MessageViewModel SetupMessageViewModel()
        {
            return new MessageViewModel {
                Id = REQUEST_ID
            };
        }

        private void SetupDeclineThrowsMissingEntityException()
        {
            _requestServiceMock.Setup(m => m.Decline(It.IsAny<int>(), TEST_MESSAGE))
                .Throws(new MissingEntityException(string.Empty));
        }

        private void AssertVerifyDecline(int requestId)
        {
            _requestServiceMock.Verify(m => m.Decline(It.Is<int>(id => id == requestId), TEST_MESSAGE), Times.Once());
        }

        private void MockTournamentRequestServiceGet(List<TournamentRequest> requests)
        {
            _requestServiceMock.Setup(tr => tr.Get()).Returns(requests);
        }

        private List<TournamentRequestViewModel> CreateTournamentRequestViewModelList()
        {
            return new TournamentRequestsViewModelsBuilder()
                .TestTournamentsRequests()
                .Build();
        }

        [Fact]
        public void Confirm_AnyRequest_RequestConfirmed()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Confirm(REQUEST_ID);

            // Assert
            AssertVerifyConfirm(REQUEST_ID);
        }

        [Fact]
        public void Confirm_AnyRequest_RequestRedirectToIndex()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Confirm(REQUEST_ID);

            // Assert
            AssertValidRedirectResult(actionResult, "Index");
        }

        [Fact]
        public void Confirm_NonExistentRequest_ThrowsMissingEntityException()
        {
            // Arrange
            SetupConfirmThrowsMissingEntityException();
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Confirm(REQUEST_ID);

            // Assert
            Assert.NotNull(actionResult);
        }

        [Fact]
        public void Decline_AnyRequest_MessageViewModelIsReturned()
        {
            // Arrange
            var expected = SetupMessageViewModel();
            var sut = BuildSUT();

            // Act
            var actual = TestExtensions.GetModel<MessageViewModel>(sut.Decline(REQUEST_ID));

            // Assert
            TestHelper.AreEqual(expected, actual, new MessageViewModelComparer());
        }

        [Fact]
        public void Decline_AnyRequest_RequestDeclined()
        {
            // Arrange
            var messageViewModel = CreateMessageViewModel();
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Decline(messageViewModel);

            // Assert
            AssertVerifyDecline(REQUEST_ID);
        }

        [Fact]
        public void Decline_AnyRequest_RequestRedirectToIndex()
        {
            // Arrange
            var messageViewModel = CreateMessageViewModel();
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Decline(messageViewModel);

            // Assert
            AssertValidRedirectResult(actionResult, "Index");
        }

        [Fact]
        public void Decline_NonExistentRequest_ThrowsMissingEntityException()
        {
            // Arrange
            SetupDeclineThrowsMissingEntityException();
            var messageViewModel = CreateMessageViewModel();
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Decline(messageViewModel);

            // Assert
            Assert.NotNull(actionResult);
        }

        [Fact]
        public void Index_ExistingRequests_TournamentRequestCollectionViewModelIsReturned()
        {
            // Arrange
            var requests = MakeTestTournamentsRequests();
            var team = CreateTeam();
            var tournament = MakeTestTournament(TEST_TOURNAMENT_ID);
            var user = CreateUser();

            MockTournamentRequestServiceGet(requests);
            MockTeamServiceGetTeam(team);
            MockUserServiceGetUserDetails(USER_ID, user);
            _tournamentServiceMock.Setup(tr => tr.GetTournamentByGroup(TEST_GROUP_ID)).Returns(tournament);

            var sut = BuildSUT();
            var expected = CreateTournamentRequestViewModelList();

            // Act
            var actual = TestExtensions
                .GetModel<TournamentRequestCollectionViewModel>(sut.Index())
                .Requests.ToList();

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void TeamDetails_ExistingTeam_TeamViewModelIsReturned()
        {
            // Arrange
            var team = CreateTeam();
            var captain = CreatePlayer(SPECIFIED_PLAYER_ID);
            var roster = new PlayerServiceTestFixture()
                .TestPlayers()
                .AddPlayer(captain)
                .Build();

            MockTeamServiceGetTeam(team);
            _teamServiceMock.Setup(ts => ts.GetTeamCaptain(It.IsAny<Team>())).Returns(captain);
            _teamServiceMock.Setup(ts => ts.GetTeamRoster(It.IsAny<int>())).Returns(roster.ToList());

            var sut = BuildSUT();
            var expected = CreateTeamViewModel();

            // Act
            var actual = TestExtensions.GetModel<TeamViewModel>(sut.TeamDetails(SPECIFIED_TEAM_ID));

            // Assert
            TestHelper.AreEqual(expected, actual, new TeamViewModelComparer());
        }

        [Fact]
        public void TeamDetails_NonExistentTeam_HttpNotFoundResultIsReturned()
        {
            // Arrange
            SetupGet(TEST_TEAM_ID, null);
            var sut = BuildSUT();

            // Act
            var result = sut.TeamDetails(TEST_TEAM_ID);

            // Assert
            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public void TournamentDetails_ExistingTournament_TournamentViewModelIsReturned()
        {
            // Arrange
            var testData = MakeTestTournament(TEST_TOURNAMENT_ID);
            var expected = MakeTestTournamentViewModel(TEST_TOURNAMENT_ID);
            MockTournamentServiceGetTournament(TEST_TOURNAMENT_ID, testData);
            var sut = BuildSUT();

            // Act
            var actual = TestExtensions.GetModel<TournamentViewModel>(sut.TournamentDetails(TEST_TOURNAMENT_ID));

            // Assert
            TournamentViewModelComparer.AssertAreEqual(expected, actual);
        }

        [Fact]
        public void TournamentDetails_NonExistentTournament_HttpNotFoundResultIsReturned()
        {
            // Arrange
            MockTournamentServiceGetTournament(TEST_TOURNAMENT_ID, null);
            var sut = BuildSUT();

            // Act
            var result = sut.TournamentDetails(TEST_TOURNAMENT_ID);

            // Assert
            Assert.IsType<HttpNotFoundResult>(result);
        }

        [Fact]
        public void UserDetails_ExistingUser_UserViewModelIsReturned()
        {
            // Arrange
            var user = CreateUser();
            MockUserServiceGetUserDetails(USER_ID, user);

            var sut = BuildSUT();
            var expected = CreateUserViewModel();

            // Act
            var actual = TestExtensions.GetModel<UserViewModel>(sut.UserDetails(USER_ID));

            // Assert
            TestHelper.AreEqual(expected, actual, new UserViewModelComparer());
        }

        [Fact]
        public void UserDetails_NonExistentUser_HttpNotFoundResultIsReturned()
        {
            // Arrange
            MockUserServiceGetUserDetails(USER_ID, null);
            var sut = BuildSUT();

            // Act
            var result = sut.UserDetails(USER_ID);

            // Assert
            Assert.IsType<HttpNotFoundResult>(result);
        }
    }
}