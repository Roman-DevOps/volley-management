using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using MSTestExtensions;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Data.Contracts;
using VolleyManagement.Data.Queries.Common;
using VolleyManagement.Data.Queries.User;
using VolleyManagement.Domain.PlayersAggregate;
using VolleyManagement.Domain.RolesAggregate;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UnitTests.Services.PlayerService;
using Xunit;

namespace VolleyManagement.UnitTests.Services.UsersService
{
    [ExcludeFromCodeCoverage]
    public class UserServiceTests : BaseTest
    {
        /// <summary>
        ///     Initializes test data.
        /// </summary>
        public UserServiceTests()
        {
            _authServiceMock = new Mock<IAuthorizationService>();
            _userRepositoryMock = new Mock<IUserRepository>();
            _cacheProviderMock = new Mock<ICacheProvider>();
            _getAllQueryMock = new Mock<IQuery<ICollection<User>, GetAllCriteria>>();
            _getPlayerByIdQueryMock = new Mock<IQuery<Player, FindByIdCriteria>>();
            _getByIdQueryMock = new Mock<IQuery<User, FindByIdCriteria>>();
            _getAdminsListQueryMock = new Mock<IQuery<ICollection<User>, UniqueUserCriteria>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
        }

        private const int EXISTING_ID = 1;

        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ICacheProvider> _cacheProviderMock;
        private readonly Mock<IQuery<ICollection<User>, GetAllCriteria>> _getAllQueryMock;
        private readonly Mock<IQuery<Player, FindByIdCriteria>> _getPlayerByIdQueryMock;
        private readonly Mock<IQuery<User, FindByIdCriteria>> _getByIdQueryMock;
        private readonly Mock<IQuery<ICollection<User>, UniqueUserCriteria>> _getAdminsListQueryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;

        private readonly UserServiceTestFixture _testFixture = new UserServiceTestFixture();

        private VolleyManagement.Services.Authorization.UserService BuildSUT()
        {
            return new VolleyManagement.Services.Authorization.UserService(
                _authServiceMock.Object,
                _getByIdQueryMock.Object,
                _getAllQueryMock.Object,
                _getPlayerByIdQueryMock.Object,
                _cacheProviderMock.Object,
                _getAdminsListQueryMock.Object,
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object);
        }

        private void MockAuthServiceThrowsException(AuthOperation operation)
        {
            _authServiceMock.Setup(tr => tr.CheckAccess(operation)).Throws<AuthorizationException>();
        }

        private void MockGetAllUsersQuery(IEnumerable<User> testData)
        {
            _getAllQueryMock.Setup(tr => tr.Execute(It.IsAny<GetAllCriteria>())).Returns(testData.ToList());
        }

        private void MockGetUserByIdQuery(User testData)
        {
            _getByIdQueryMock.Setup(tr => tr.Execute(It.IsAny<FindByIdCriteria>())).Returns(testData);
        }

        private void MockGetPlayerByIdQuery(Player testData)
        {
            _getPlayerByIdQueryMock.Setup(tr => tr.Execute(It.IsAny<FindByIdCriteria>())).Returns(testData);
        }

        private void MockGetAdminsListQuery(IEnumerable<User> testData)
        {
            _getAdminsListQueryMock.Setup(tr => tr.Execute(It.IsAny<UniqueUserCriteria>())).Returns(testData.ToList());
        }

        [Fact]
        public void GetAdminsList_UsersExist_UsersReturned()
        {
            // Arrange
            var testData = _testFixture.TestUsers().Build();
            MockGetAdminsListQuery(testData);

            var expected = new UserServiceTestFixture()
                .TestUsers()
                .Build();
            var sut = BuildSUT();

            // Act
            var actual = sut.GetAdminsList();

            // Assert
            TestHelper.AreEqual(expected, actual, new UserComparer());
        }

        [Fact]
        public void GetAll_NoViewRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.AllUsers.ViewList);

            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<AuthorizationException>(() => sut.GetAllUsers(), "Requested operation is not allowed");
        }

        [Fact]
        public void GetAll_UsersExist_UsersReturned()
        {
            // Arrange
            var testData = _testFixture.TestUsers().Build();
            MockGetAllUsersQuery(testData);

            var expected = new UserServiceTestFixture()
                .TestUsers()
                .Build();

            var sut = BuildSUT();

            // Act
            var actual = sut.GetAllUsers();

            // Assert
            TestHelper.AreEqual(expected, actual, new UserComparer());
        }

        [Fact]
        public void GetAllActiveUsers_NoViewRights_AuthorizationExceptionThrown()
        {
            // Arrange
            var testData = _testFixture.TestUsers().Build();
            MockAuthServiceThrowsException(AuthOperations.AllUsers.ViewActiveList);

            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<AuthorizationException>(() => sut.GetAllActiveUsers(), "Requested operation is not allowed");
        }

        [Fact]
        public void GetById_UserExists_UserReturned()
        {
            // Arrange
            var expected = new UserBuilder().WithId(EXISTING_ID).Build();
            MockGetUserByIdQuery(expected);

            var sut = BuildSUT();

            // Act
            var actual = sut.GetUser(EXISTING_ID);

            // Assert
            TestHelper.AreEqual(expected, actual, new UserComparer());
        }

        [Fact]
        public void GetUserDetails_NoViewRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.AllUsers.ViewDetails);

            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<AuthorizationException>(() => sut.GetUserDetails(EXISTING_ID),
                "Requested operation is not allowed");
        }

        [Fact]
        public void GetUserDetails_UserExists_UserReturned()
        {
            // Arrange
            var player = new PlayerBuilder().WithId(EXISTING_ID).Build();
            var expected = new UserBuilder().WithId(EXISTING_ID).WithPlayer(player).Build();
            MockGetUserByIdQuery(expected);
            MockGetPlayerByIdQuery(player);
            var sut = BuildSUT();

            // Act
            var actual = sut.GetUserDetails(EXISTING_ID);

            // Assert
            TestHelper.AreEqual(expected, actual, new UserComparer());
        }
    }
}