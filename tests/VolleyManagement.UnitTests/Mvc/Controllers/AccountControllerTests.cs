using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authentication;
using VolleyManagement.Contracts.Authentication.Models;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Domain.RolesAggregate;
using VolleyManagement.UI.Areas.Mvc.Controllers;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Users;
using VolleyManagement.UI.Infrastructure;
using VolleyManagement.UnitTests.Mvc.Comparers;
using VolleyManagement.UnitTests.Mvc.ViewModels;
using VolleyManagement.UnitTests.Services.UserManager;
using Xunit;

namespace VolleyManagement.UnitTests.Mvc.Controllers
{
    [ExcludeFromCodeCoverage]
    public class AccountControllerTests
    {
        public AccountControllerTests()
        {
            _userManagerMock = new Mock<IVolleyUserManager<UserModel>>();

            _rolesServiceMock = new Mock<IRolesService>();

            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _exceptionFilter = new Mock<VolleyExceptionFilterAttribute>();

            _cacheProviderMock = new Mock<ICacheProvider>();

            _userServiceMock = new Mock<IUserService>();

            _authService = new Mock<IAuthorizationService>();
        }

        private const int USER_ID = 2;
        private const string USER_NAME = "Jack";

        private readonly Role _adminRole = new Role {
            Id = 1,
            Name = "Admin"
        };

        private readonly Mock<IVolleyUserManager<UserModel>> _userManagerMock;
        private readonly Mock<IRolesService> _rolesServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private Mock<VolleyExceptionFilterAttribute> _exceptionFilter;
        private readonly Mock<ICacheProvider> _cacheProviderMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IAuthorizationService> _authService;

        private ControllerContext GetControllerContext()
        {
            var claim = new Claim("id", USER_ID.ToString());
            var identityMock = Mock.Of<ClaimsIdentity>(ci => ci.FindFirst(It.IsAny<string>()) == claim);
            var mockContext = Mock.Of<ControllerContext>(cc => cc.HttpContext.User.Identity.Equals(identityMock));
            return mockContext;
        }

        private void MockFindById()
        {
            _userManagerMock.Setup(um => um.FindByIdAsync(USER_ID))
                .Returns(Task.FromResult(
                    new UserModelBuilder()
                        .WithId(USER_ID)
                        .WithUserName(USER_NAME)
                        .Build()));
        }

        private void MockGetRole()
        {
            _rolesServiceMock.Setup(rs => rs.GetRole(It.IsAny<int>()))
                .Returns(_adminRole);
        }

        private void MockCurrentUser()
        {
            _currentUserServiceMock.Setup(s => s.GetCurrentUserId())
                .Returns(USER_ID);
        }

        private AccountController CreateController()
        {
            return new AccountController(
                _userManagerMock.Object,
                _rolesServiceMock.Object,
                _userServiceMock.Object,
                _cacheProviderMock.Object,
                _currentUserServiceMock.Object,
                _authService.Object);
        }

        /// <summary>
        ///     Test for Details()
        /// </summary>
        [Fact]
        public void Details_UserExists_UserIsReturned()
        {
            // Arrange
            MockCurrentUser();
            MockFindById();
            MockGetRole();

            var expected = new UserMvcViewModelBuilder()
                .WithId(USER_ID)
                .WithUserName(USER_NAME)
                .Build();

            var sut = CreateController();

            // Act
            var actual = TestExtensions.GetModelAsync<UserViewModel>(sut.Details());

            // Assert
            TestHelper.AreEqual(expected, actual, new UserViewModelComparer());
        }

        /// <summary>
        ///     Test for Edit()
        /// </summary>
        /// <returns>Asynchronous operation</returns>
        [Fact]
        public async Task EditPostAction_UserExists_UserUpdated()
        {
            // Arrange
            MockCurrentUser();
            MockFindById();
            MockGetRole();

            var userEditViewModel = new UserEditMvcViewModelBuilder()
                .WithId(USER_ID)
                .WithCellPhone("068-33-44-555")
                .WithEmail("example@ex.ua")
                .WithName("Vasya Petichkin")
                .Build();

            var sut = CreateController();

            // Act
            await sut.Edit(userEditViewModel);

            // Assert
            _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<UserModel>()), Times.Once());
        }

        /// <summary>
        ///     Test() edit method.
        /// </summary>
        /// <returns>Asynchronous operation</returns>
        [Fact]
        public async Task EditPostAction_UserIdPassed_ExceptionThrown()
        {
            // Arrange
            MockCurrentUser();
            MockGetRole();

            _userManagerMock.Setup(um => um.FindByIdAsync(USER_ID))
                .Returns(Task.FromResult<UserModel>(null));

            var userEditViewModel = new UserEditMvcViewModelBuilder()
                .Build();

            var sut = CreateController();
            sut.ControllerContext = GetControllerContext();

            // Act
            await sut.Edit(userEditViewModel);

            // Assert
            _userManagerMock.Verify(um => um.UpdateAsync(It.IsAny<UserModel>()), Times.Never());
        }
    }
}