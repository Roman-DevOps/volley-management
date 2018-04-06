using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using FluentAssertions;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Domain.PlayersAggregate;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UI.Areas.Admin.Controllers;
using VolleyManagement.UnitTests.Services.PlayerService;
using VolleyManagement.UnitTests.Services.UsersService;
using Xunit;

namespace VolleyManagement.UnitTests.Mvc.Controllers
{
    [ExcludeFromCodeCoverage]
    public class RequestControllerTests
    {
        public RequestControllerTests()
        {
            _requestServiceMock = new Mock<IRequestService>();
            _userServiceMock = new Mock<IUserService>();
            _playerServiceMock = new Mock<IPlayerService>();
        }

        private const int REQUEST_ID = 1;
        private const int USER_ID = 1;
        private const int PLAYER_ID = 1;

        private readonly Mock<IRequestService> _requestServiceMock = new Mock<IRequestService>();
        private readonly Mock<IUserService> _userServiceMock = new Mock<IUserService>();
        private readonly Mock<IPlayerService> _playerServiceMock = new Mock<IPlayerService>();

        private static void AssertValidRedirectResult(ActionResult actionResult, string view)
        {
            var result = (RedirectToRouteResult) actionResult;
            result.Should().NotBeNull("Method result should be instance of RedirectToRouteResult");
            result.Should().NotBeNull("Method result should be instance of RedirectToRouteResult");
            Assert.False(result.Permanent, "Redirect should not be permanent");
            result.RouteValues.Count.Should()
                .Be(1, string.Format("Redirect should forward to Requests.{0} action", view));
            result.RouteValues["action"].Should()
                .Be(view, string.Format("Redirect should forward to Requests.{0} action", view));
        }

        private User GetUser()
        {
            return new User();
        }

        private Player GetPlayer()
        {
            return new Player();
        }

        private RequestController BuildSUT()
        {
            return new RequestController(
                _requestServiceMock.Object,
                _userServiceMock.Object,
                _playerServiceMock.Object);
        }

        private void SetupUserService(int id)
        {
            _userServiceMock.Setup(m => m.GetUser(id)).Returns(new User());
        }

        private void SetupUserServiceReturnsNullUser(int id)
        {
            User user = null;
            _userServiceMock.Setup(m => m.GetUser(id)).Returns(user);
        }

        private void SetupPlayerService(int id)
        {
            _playerServiceMock.Setup(m => m.Get(id)).Returns(new Player());
        }

        private void SetupPlayerServiceReturnsNullPlayer(int id)
        {
            Player player = null;
            _playerServiceMock.Setup(m => m.Get(id)).Returns(player);
        }

        private void SetupConfirmThrowsMissingEntityException()
        {
            _requestServiceMock.Setup(m => m.Confirm(It.IsAny<int>()))
                .Throws(new MissingEntityException(string.Empty));
        }

        private void SetupDeclineThrowsMissingEntityException()
        {
            _requestServiceMock.Setup(m => m.Decline(It.IsAny<int>()))
                .Throws(new MissingEntityException(string.Empty));
        }

        private void AssertVerifyConfirm(Mock<IRequestService> mock, int requestId)
        {
            mock.Verify(m => m.Confirm(It.Is<int>(id => id == requestId)), Times.Once());
        }

        private void AssertVerifyDecline(Mock<IRequestService> mock, int requestId)
        {
            mock.Verify(m => m.Decline(It.Is<int>(id => id == requestId)), Times.Once());
        }

        [Fact]
        public void Confirm_AnyRequest_RequestConfirmed()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Confirm(REQUEST_ID);

            // Assert
            AssertVerifyConfirm(_requestServiceMock, REQUEST_ID);
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
        public void Decline_AnyRequest_RequestDeclined()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Decline(REQUEST_ID);

            // Assert
            AssertVerifyDecline(_requestServiceMock, REQUEST_ID);
        }

        [Fact]
        public void Decline_AnyRequest_RequestRedirectToIndex()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Decline(REQUEST_ID);

            // Assert
            AssertValidRedirectResult(actionResult, "Index");
        }

        [Fact]
        public void Decline_NonExistentRequest_ThrowsMissingEntityException()
        {
            // Arrange
            SetupDeclineThrowsMissingEntityException();
            var sut = BuildSUT();

            // Act
            var actionResult = sut.Decline(REQUEST_ID);

            // Assert
            Assert.NotNull(actionResult);
        }

        [Fact]
        public void PlayerDetails_ExistingPlayer_PlayerReturned()
        {
            // Arrange
            var expected = GetPlayer();
            SetupPlayerService(PLAYER_ID);
            var sut = BuildSUT();

            // Act
            var actionResult = sut.PlayerDetails(PLAYER_ID);
            var actual = TestExtensions.GetModel<Player>(actionResult);

            // Assert
            TestHelper.AreEqual(expected, actual, new PlayerComparer());
        }

        [Fact]
        public void PlayerDetails_NonExistentUser_HttpNotFoundResultIsReturned()
        {
            // Arrange
            SetupPlayerServiceReturnsNullPlayer(PLAYER_ID);
            var sut = BuildSUT();

            // Act
            var actionResult = sut.PlayerDetails(PLAYER_ID);

            // Assert
            Assert.IsType<HttpNotFoundResult>(actionResult);
        }

        [Fact]
        public void UserDetails_ExistingUser_UserReturned()
        {
            // Arrange
            var expected = GetUser();
            SetupUserService(USER_ID);
            var sut = BuildSUT();

            // Act
            var actionResult = sut.UserDetails(USER_ID);
            var actual = TestExtensions.GetModel<User>(actionResult);

            // Assert
            TestHelper.AreEqual(expected, actual, new UserComparer());
        }

        [Fact]
        public void UserDetails_NonExistentUser_HttpNotFoundResultIsReturned()
        {
            // Arrange
            SetupUserServiceReturnsNullUser(USER_ID);
            var sut = BuildSUT();

            // Act
            var actionResult = sut.UserDetails(USER_ID);

            // Assert
            Assert.IsType<HttpNotFoundResult>(actionResult);
        }
    }
}