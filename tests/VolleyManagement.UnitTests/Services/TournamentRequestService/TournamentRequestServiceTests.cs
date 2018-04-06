using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Moq;
using MSTestExtensions;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Contracts.ExternalResources;
using VolleyManagement.Data.Contracts;
using VolleyManagement.Data.Exceptions;
using VolleyManagement.Data.Queries.Common;
using VolleyManagement.Data.Queries.TournamentRequest;
using VolleyManagement.Domain.RolesAggregate;
using VolleyManagement.Domain.TournamentRequestAggregate;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UnitTests.Services.MailService;
using VolleyManagement.UnitTests.Services.UserManager;
using Xunit;

namespace VolleyManagement.UnitTests.Services.TournamentRequestService
{
    [ExcludeFromCodeCoverage]
    public class TournamentRequestServiceTests : BaseTest
    {
        public TournamentRequestServiceTests()
        {
            _tournamentRequestRepositoryMock
                = new Mock<ITournamentRequestRepository>();

            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _authServiceMock = new Mock<IAuthorizationService>();

            _getAllRequestsQueryMock = new Mock<IQuery<ICollection<TournamentRequest>, GetAllCriteria>>();

            _getRequestByIdQueryMock = new Mock<IQuery<TournamentRequest, FindByIdCriteria>>();

            _getRequestByAllQueryMock = new Mock<IQuery<TournamentRequest, FindByTeamTournamentCriteria>>();

            _tournamentServiceMock = new Mock<ITournamentService>();

            _mailServiceMock = new Mock<IMailService>();

            _userServiceMock = new Mock<IUserService>();

            _tournamentRequestRepositoryMock.Setup(tr => tr.UnitOfWork)
                .Returns(_unitOfWorkMock.Object);
        }

        private const int EXISTING_ID = 1;
        private const int INVALID_REQUEST_ID = -1;

        private readonly TournamentRequestServiceTestFixture _testFixture
            = new TournamentRequestServiceTestFixture();

        private readonly Mock<ITournamentRequestRepository> _tournamentRequestRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthorizationService> _authServiceMock;
        private readonly Mock<IQuery<ICollection<TournamentRequest>, GetAllCriteria>> _getAllRequestsQueryMock;
        private readonly Mock<IQuery<TournamentRequest, FindByIdCriteria>> _getRequestByIdQueryMock;
        private readonly Mock<IQuery<TournamentRequest, FindByTeamTournamentCriteria>> _getRequestByAllQueryMock;
        private readonly Mock<ITournamentService> _tournamentServiceMock = new Mock<ITournamentService>();
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly Mock<IUserService> _userServiceMock;

        private VolleyManagement.Services.TournamentRequestService BuildSUT()
        {
            return new VolleyManagement.Services.TournamentRequestService(
                _tournamentRequestRepositoryMock.Object,
                _authServiceMock.Object,
                _getAllRequestsQueryMock.Object,
                _getRequestByIdQueryMock.Object,
                _getRequestByAllQueryMock.Object,
                _tournamentServiceMock.Object,
                _mailServiceMock.Object,
                _userServiceMock.Object);
        }

        private void VerifyCreateTournamentRequest(
            TournamentRequest request,
            Times times,
            string message)
        {
            _tournamentRequestRepositoryMock
                .Verify(
                    tr => tr.Add(
                        It.Is<TournamentRequest>(p => TournamentRequestAreEquals(p, request))),
                    times,
                    message);
            _unitOfWorkMock.Verify(uow => uow.Commit(), times);
        }

        private bool TournamentRequestAreEquals(TournamentRequest x, TournamentRequest y)
        {
            return new TournamentRequestComparer().Compare(x, y) == 0;
        }

        private void MockRequestServiceThrowsInvalidKeyValueException()
        {
            _tournamentRequestRepositoryMock.Setup(tr => tr.Remove(It.IsAny<int>())).Throws<InvalidKeyValueException>();
        }

        private void MockGetAllTournamentRequestQuery(IEnumerable<TournamentRequest> testData)
        {
            _getAllRequestsQueryMock.Setup(tr => tr.Execute(It.IsAny<GetAllCriteria>())).Returns(testData.ToList());
        }

        private void MockGetAllTournamentRequestQuery(TournamentRequest testData)
        {
            _getRequestByAllQueryMock.Setup(tr => tr.Execute(It.IsAny<FindByTeamTournamentCriteria>()))
                .Returns(testData);
        }

        private void VerifyAddedTeamToTournament(Times times)
        {
            _tournamentServiceMock.Verify(tr => tr.AddTeamsToTournament(It.IsAny<List<TeamTournamentAssignmentDto>>()),
                times);
        }

        private void MockAuthServiceThrownException(AuthOperation operation)
        {
            _authServiceMock.Setup(tr => tr.CheckAccess(operation)).Throws<AuthorizationException>();
        }

        private void MockGetRequestByIdQuery(TournamentRequest request)
        {
            _getRequestByIdQueryMock.Setup(tr => tr.Execute(It.IsAny<FindByIdCriteria>())).Returns(request);
        }

        private void MockMailService(EmailMessage message)
        {
            _mailServiceMock.Setup(tr => tr.Send(message));
        }

        private void MockUserService()
        {
            var user = new UserBuilder().Build();
            var userList = new List<User> {user};
            _userServiceMock.Setup(tr => tr.GetAdminsList()).Returns(userList);
        }

        private void MockGetUser()
        {
            var user = new UserBuilder().Build();
            _userServiceMock.Setup(tr => tr.GetUser(It.IsAny<int>())).Returns(user);
        }

        private void VerifyCheckAccess(AuthOperation operation, Times times)
        {
            _authServiceMock.Verify(tr => tr.CheckAccess(operation), times);
        }

        private void VerifyDeleteRequest(int requestid, Times repositoryTimes)
        {
            _tournamentRequestRepositoryMock.Verify(tr => tr.Remove(It.Is<int>(id => id == requestid)),
                repositoryTimes);
            _unitOfWorkMock.Verify(uow => uow.Commit(), repositoryTimes);
        }

        private void VerifyDeleteRequest(int requestid, Times repositoryTimes, Times unitOfWorkTimes)
        {
            _tournamentRequestRepositoryMock.Verify(tr => tr.Remove(It.Is<int>(id => id == requestid)),
                repositoryTimes);
            _unitOfWorkMock.Verify(uow => uow.Commit(), unitOfWorkTimes);
        }

        private void MockRemoveTournamentRequest()
        {
            _tournamentRequestRepositoryMock.Setup(tr => tr.Remove(It.IsAny<int>()));
        }

        [Fact]
        public void Confirm_NoConfirmRights_AuthorizationExceptionThrows()
        {
            // Arrange
            MockAuthServiceThrownException(AuthOperations.TournamentRequests.Confirm);
            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<AuthorizationException>(
                () =>
                    sut.Confirm(EXISTING_ID),
                "Requested operation is not allowed");
        }

        [Fact]
        public void Confirm_RequestDoesNotExist_ExceptionThrown()
        {
            // Arrange
            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<MissingEntityException>(
                () =>
                    sut.Confirm(INVALID_REQUEST_ID),
                "A tournament request with specified identifier was not found");
        }

        [Fact]
        public void Confirm_RequestExists_TeamAdded()
        {
            // Arrange
            var expected = new TournamentRequestBuilder().WithId(EXISTING_ID).Build();
            MockGetRequestByIdQuery(expected);

            _tournamentServiceMock.Setup(tr => tr.AddTeamsToTournament(It.IsAny<List<TeamTournamentAssignmentDto>>()));

            var emailMessage = new EmailMessageBuilder().Build();
            MockGetUser();
            MockRemoveTournamentRequest();
            MockMailService(emailMessage);
            var sut = BuildSUT();

            // Act
            sut.Confirm(EXISTING_ID);

            // Assert
            VerifyAddedTeamToTournament(Times.Once());
        }

        [Fact]
        public void Create_InvalidUserId_ExceptionThrows()
        {
            var newTournamentRequest = new TournamentRequestBuilder()
                .Build();
            _tournamentRequestRepositoryMock.Setup(
                    tr => tr.Add(
                        newTournamentRequest))
                .Callback<TournamentRequest>(t => t.UserId = -1);

            // Arrange
            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<ArgumentException>(
                () =>
                    sut.Create(newTournamentRequest),
                "User's id is wrong");
        }

        [Fact]
        public void Create_TournamentRequesExist_RequestNotAdded()
        {
            // Arrange
            var newTournamentRequest = new TournamentRequestBuilder()
                .WithId(EXISTING_ID)
                .WithTeamId(EXISTING_ID)
                .WithGroupId(EXISTING_ID)
                .WithUserId(EXISTING_ID)
                .Build();
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockUserService();
            _tournamentRequestRepositoryMock.Setup(
                    tr => tr.Add(
                        It.IsAny<TournamentRequest>()))
                .Callback<TournamentRequest>(t => t.Id = EXISTING_ID);
            MockGetAllTournamentRequestQuery(newTournamentRequest);
            var sut = BuildSUT();

            // Act
            sut.Create(newTournamentRequest);

            // Assert
            VerifyCreateTournamentRequest(newTournamentRequest, Times.Never(),
                "Parameter request is not equal to Instance of request");
        }

        [Fact]
        public void Create_ValidTournamentRequest_RequestAdded()
        {
            // Arrange
            var newTournamentRequest = new TournamentRequestBuilder()
                .WithId(EXISTING_ID)
                .WithTeamId(EXISTING_ID)
                .WithGroupId(EXISTING_ID)
                .WithUserId(EXISTING_ID)
                .Build();
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockUserService();
            _tournamentRequestRepositoryMock.Setup(
                    tr => tr.Add(
                        It.IsAny<TournamentRequest>()))
                .Callback<TournamentRequest>(t => t.Id = EXISTING_ID);
            var sut = BuildSUT();

            // Act
            sut.Create(newTournamentRequest);

            // Assert
            VerifyCreateTournamentRequest(newTournamentRequest, Times.Once(),
                "Parameter request is not equal to Instance of request");
        }

        [Fact]
        public void Decline_RequestDoesNotExist_DbNotChanged()
        {
            // Arrange
            MockRequestServiceThrowsInvalidKeyValueException();
            Exception exception = null;
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            var expected = new TournamentRequestBuilder().Build();
            MockGetRequestByIdQuery(expected);
            MockGetUser();

            var sut = BuildSUT();

            // Act
            try
            {
                sut.Decline(INVALID_REQUEST_ID, emailMessage.Body);
            }
            catch (MissingEntityException ex)
            {
                exception = ex;
            }

            // Assert
            VerifyDeleteRequest(INVALID_REQUEST_ID, Times.Once(), Times.Never());
        }

        [Fact]
        public void Decline_RequestExist_RequestDeleted()
        {
            // Arrange
            var expected = new TournamentRequestBuilder().Build();
            MockGetRequestByIdQuery(expected);
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockGetUser();
            var sut = BuildSUT();

            // Act
            sut.Decline(EXISTING_ID, emailMessage.Body);

            // Assert
            VerifyDeleteRequest(EXISTING_ID, Times.Once());
        }

        [Fact]
        public void GetAll_NoViewListRights_AuthorizationExceptionThrows()
        {
            // Arrange
            MockAuthServiceThrownException(AuthOperations.TournamentRequests.ViewList);
            var sut = BuildSUT();

            // Act => Assert
            Assert.Throws<AuthorizationException>(
                () =>
                    sut.Get(),
                "Requested operation is not allowed");
        }

        [Fact]
        public void GetAll_RequestsExist_RequestsReturned()
        {
            // Arrange
            var expected = _testFixture.TestRequests().Build();

            MockGetAllTournamentRequestQuery(expected);

            var sut = BuildSUT();

            // Act
            var actual = sut.Get();

            // Assert
            TestHelper.AreEqual(expected, actual, new TournamentRequestComparer());
        }

        [Fact]
        public void GetById_RequestExists_RequestReturned()
        {
            // Arrange
            var expected = new TournamentRequestBuilder().WithId(EXISTING_ID).Build();
            MockGetRequestByIdQuery(expected);

            var sut = BuildSUT();

            // Act
            var actual = sut.Get(EXISTING_ID);

            // Assert
            TestHelper.AreEqual(expected, actual, new TournamentRequestComparer());
        }
    }
}