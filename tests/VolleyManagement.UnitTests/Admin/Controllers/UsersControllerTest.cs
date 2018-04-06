using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Web.Mvc;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UI.Areas.Admin.Controllers;
using VolleyManagement.UI.Areas.Admin.Models;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Players;
using VolleyManagement.UnitTests.Admin.Comparers;
using VolleyManagement.UnitTests.Admin.ViewModels;
using VolleyManagement.UnitTests.Mvc.ViewModels;
using VolleyManagement.UnitTests.Services.UsersService;
using Xunit;

namespace VolleyManagement.UnitTests.Admin.Controllers
{
    [ExcludeFromCodeCoverage]
    public class UsersControllerTest
    {
        public UsersControllerTest()
        {
            _userServiceMock = new Mock<IUserService>();
            _currentUserService = new Mock<ICurrentUserService>();
        }

        private const int EXISTING_ID = 1;

        private readonly Mock<IUserService> _userServiceMock;
        private Mock<ICurrentUserService> _currentUserService;

        private UsersController BuildSUT()
        {
            return new UsersController(_userServiceMock.Object);
        }

        private User CreateUser()
        {
            return new UserBuilder()
                .WithId(EXISTING_ID)
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

        private void MockUserServiceGetUserDetails(User user)
        {
            _userServiceMock.Setup(ts => ts.GetUser(It.IsAny<int>())).Returns(user);
        }

        private void SetupGetUserDetails(int userId, User user)
        {
            _userServiceMock.Setup(tr => tr.GetUserDetails(userId)).Returns(user);
        }

        private void SetupGetAllUsers(List<User> users)
        {
            _userServiceMock.Setup(tr => tr.GetAllUsers()).Returns(users);
        }

        private List<User> MakeTestUsers()
        {
            return new UserServiceTestFixture().TestUsers().Build();
        }

        [Fact]
        public void UserDetails_ExistingUser_UserViewModelIsReturned()
        {
            // Arrange
            var user = CreateUser();
            SetupGetUserDetails(EXISTING_ID, user);

            var sut = BuildSUT();
            var expected = CreateUserViewModel();

            // Act
            var actual = TestExtensions.GetModel<UserViewModel>(sut.UserDetails(EXISTING_ID));

            // Assert
            TestHelper.AreEqual(expected, actual, new UserViewModelComparer());
        }

        [Fact]
        public void UserDetails_NonExistentUser_HttpNotFoundResultIsReturned()
        {
            // Arrange
            SetupGetUserDetails(EXISTING_ID, null);
            var sut = BuildSUT();

            // Act
            var result = sut.UserDetails(EXISTING_ID);

            // Assert
            Assert.IsType<HttpNotFoundResult>(result);
        }
    }
}