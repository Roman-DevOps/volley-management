using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Data.Contracts;
using VolleyManagement.Data.Queries.Common;
using VolleyManagement.Data.Queries.User;
using VolleyManagement.Domain.PlayersAggregate;
using VolleyManagement.Domain.UsersAggregate;
using Xunit;

namespace VolleyManagement.UnitTests.Services.UserService
{
    [ExcludeFromCodeCoverage]
    public class BlockUserServiceTests
    {
        public BlockUserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _authorizationServiceMock = new Mock<IAuthorizationService>();
            _userServiceMock = new Mock<IUserService>();
            _cacheProviderMock = new Mock<ICacheProvider>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _getUserByIdQueryMock = new Mock<IQuery<User, FindByIdCriteria>>();
            _getPlayerByIdQueryMock = new Mock<IQuery<Player, FindByIdCriteria>>();
            _getAllUserQueryMock = new Mock<IQuery<ICollection<User>, GetAllCriteria>>();
            _getUserListQueryMock = new Mock<IQuery<List<User>, UniqueUserCriteria>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();

            _userRepositoryMock.Setup(fr => fr.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);
        }

        private const int INVALID_USER_ID = -1;
        private const int VALID_USER_ID = 1;
        private const string TEST_NAME = "Test Name";
        private const bool USER_STATUS_IS_BLOCKED = true;
        private const bool USER_STATUS_IS_UNBLOCKED = false;

        private readonly Mock<IUserRepository> _userRepositoryMock;

        private readonly Mock<IAuthorizationService> _authorizationServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ICacheProvider> _cacheProviderMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IQuery<User, FindByIdCriteria>> _getUserByIdQueryMock;
        private readonly Mock<IQuery<Player, FindByIdCriteria>> _getPlayerByIdQueryMock;
        private readonly Mock<IQuery<ICollection<User>, GetAllCriteria>> _getAllUserQueryMock;
        private readonly Mock<IQuery<List<User>, UniqueUserCriteria>> _getUserListQueryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;

        private VolleyManagement.Services.Authorization.UserService BuildSUT()
        {
            return new VolleyManagement.Services.Authorization.UserService(
                _authorizationServiceMock.Object,
                _getUserByIdQueryMock.Object,
                _getAllUserQueryMock.Object,
                _getPlayerByIdQueryMock.Object,
                _cacheProviderMock.Object,
                _getUserListQueryMock.Object,
                _userRepositoryMock.Object,
                _currentUserServiceMock.Object);
        }

        private void MockCurrentUser(User testData, int userId)
        {
            _getUserByIdQueryMock
                .Setup(tr => tr.Execute(It.IsAny<FindByIdCriteria>()))
                .Returns(testData);
        }

        private void VerifyEditUser(User user, Times times)
        {
            _userRepositoryMock.Verify(
                ur => ur.Update(It.Is<User>(u => UsersAreEqual(u, user))),
                times);
            _unitOfWorkMock.Verify(uow => uow.Commit(), times);
        }

        private bool UsersAreEqual(User x, User y)
        {
            return new BlockUserComparer().Compare(x, y) == 0;
        }

        private void VerifyExceptionThrown(
            Exception exception,
            string expectedMessage)
        {
            exception.Should().NotBeNull("There is no exception thrown");
            exception.Message.Should().Be(expectedMessage, "Expected and actual exceptions messages aren't equal");
        }

        private void MockUserServiceThrowsException(int userId)
        {
            _userServiceMock
                .Setup(tr => tr.GetUser(userId))
                .Throws<MissingEntityException>();
        }

        [Fact]
        public void SetUserBlocked_UserDoesNotExist_ExceptionThrown()
        {
            // Arrange
            Exception exception = null;
            MockUserServiceThrowsException(INVALID_USER_ID);
            var sut = BuildSUT();

            // Act
            try
            {
                sut.ChangeUserBlocked(INVALID_USER_ID, true);
            }
            catch (MissingEntityException ex)
            {
                exception = ex;
            }

            // Assert
            VerifyExceptionThrown(
                exception,
                "A user with specified identifier was not found");
        }

        [Fact]
        public void SetUserBlocked_UserExist_UpdatedUserReturned()
        {
            // Arrange
            var user = new BlockUserBuilder().WithId(VALID_USER_ID).Build();
            MockCurrentUser(user, VALID_USER_ID);
            var sut = BuildSUT();

            // Act
            sut.ChangeUserBlocked(VALID_USER_ID, true);

            // Assert
            VerifyEditUser(user, Times.Once());
        }

        [Fact]
        public void SetUserBlocked_UserExist_UserStatusIsBlocked()
        {
            // Arrange
            var user = new BlockUserBuilder().WithBlockStatus(USER_STATUS_IS_BLOCKED).Build();
            MockCurrentUser(user, VALID_USER_ID);
            var sut = BuildSUT();

            // Act
            sut.ChangeUserBlocked(VALID_USER_ID, true);

            // Assert
            VerifyEditUser(user, Times.Once());
        }

        [Fact]
        public void SetUserUnblocked_UserDoesNotExist_ExceptionThrown()
        {
            // Arrange
            Exception exception = null;
            MockUserServiceThrowsException(INVALID_USER_ID);
            var sut = BuildSUT();

            // Act
            try
            {
                sut.ChangeUserBlocked(INVALID_USER_ID, false);
            }
            catch (MissingEntityException ex)
            {
                exception = ex;
            }

            // Assert
            VerifyExceptionThrown(
                exception,
                "A user with specified identifier was not found");
        }

        [Fact]
        public void SetUserUnblocked_UserExist_UpdatedUserReturned()
        {
            // Arrange
            var user = new BlockUserBuilder().WithId(VALID_USER_ID).Build();
            MockCurrentUser(user, VALID_USER_ID);
            var sut = BuildSUT();

            // Act
            sut.ChangeUserBlocked(VALID_USER_ID, false);

            // Assert
            VerifyEditUser(user, Times.Once());
        }

        [Fact]
        public void SetUserUnblocked_UserExist_UserStatusIsUnblocked()
        {
            // Arrange
            var user = new BlockUserBuilder().WithBlockStatus(USER_STATUS_IS_UNBLOCKED).Build();
            MockCurrentUser(user, VALID_USER_ID);
            var sut = BuildSUT();

            // Act
            sut.ChangeUserBlocked(VALID_USER_ID, false);

            // Assert
            VerifyEditUser(user, Times.Once());
        }
    }
}