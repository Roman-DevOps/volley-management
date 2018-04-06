using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Contracts.Exceptions;
using VolleyManagement.Contracts.ExternalResources;
using VolleyManagement.Crosscutting.Contracts.Providers;
using VolleyManagement.Data.Contracts;
using VolleyManagement.Data.Exceptions;
using VolleyManagement.Data.Queries.Common;
using VolleyManagement.Domain.FeedbackAggregate;
using VolleyManagement.Domain.RolesAggregate;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UnitTests.Services.MailService;
using VolleyManagement.UnitTests.Services.UserManager;
using Xunit;

namespace VolleyManagement.UnitTests.Services.FeedbackService
{
    [ExcludeFromCodeCoverage]
    public class FeedbackServiceTests : IDisposable
    {
        public FeedbackServiceTests()
        {
            _feedbackRepositoryMock = new Mock<IFeedbackRepository>();
            _feedbackServiceMock = new Mock<IFeedbackService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _timeMock = new Mock<TimeProvider>();
            _getAllFeedbacksQueryMock = new Mock<IQuery<ICollection<Feedback>, GetAllCriteria>>();
            _getFeedbackByIdQueryMock = new Mock<IQuery<Feedback, FindByIdCriteria>>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _userServiceMock = new Mock<IUserService>();
            _mailServiceMock = new Mock<IMailService>();
            _authServiceMock = new Mock<IAuthorizationService>();

            _feedbackRepositoryMock.Setup(fr => fr.UnitOfWork).Returns(_unitOfWorkMock.Object);
            TimeProvider.Current = _timeMock.Object;

            _timeMock.Setup(tp => tp.UtcNow).Returns(_feedbackTestDate);
        }

        // TestCleanup
        public void Dispose()
        {
            TimeProvider.ResetToDefault();
        }

        public const string ERROR_FOR_FEEDBACK_REPOSITORY_VERIFY
            = "Parameter feedback is not equal to Instance of feedback";

        public const string ERROR_FOR_UNIT_OF_WORK_VERIFY
            = "Can't save feedback to database";

        public const int SPECIFIC_FEEDBACK_ID = 2;
        private const int EXISTING_ID = 1;
        private const int INVALID_FEEDBACK_ID = -1;
        private const int VALID_USER_ID = 1;
        private const string TEST_NAME = "Nick";
        private const string MESSAGE = "Test reply message";

        private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock;

        private Mock<IFeedbackService> _feedbackServiceMock = new Mock<IFeedbackService>();

        private readonly Mock<IUnitOfWork> _unitOfWorkMock
            = new Mock<IUnitOfWork>();

        private readonly Mock<TimeProvider> _timeMock = new Mock<TimeProvider>();

        private readonly Mock<IQuery<ICollection<Feedback>, GetAllCriteria>> _getAllFeedbacksQueryMock;
        private readonly Mock<IQuery<Feedback, FindByIdCriteria>> _getFeedbackByIdQueryMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<IMailService> _mailServiceMock;
        private readonly Mock<IAuthorizationService> _authServiceMock;

        private readonly FeedbackServiceTestFixture _testFixture = new FeedbackServiceTestFixture();

        private readonly DateTime _feedbackTestDate = new DateTime(2007, 05, 03);

        private VolleyManagement.Services.FeedbackService BuildSUT()
        {
            return new VolleyManagement.Services.FeedbackService(
                _feedbackRepositoryMock.Object,
                _userServiceMock.Object,
                _mailServiceMock.Object,
                _currentUserServiceMock.Object,
                _authServiceMock.Object,
                _getFeedbackByIdQueryMock.Object,
                _getAllFeedbacksQueryMock.Object);
        }

        private void VerifyCheckAccess(AuthOperation operation, Times times)
        {
            _authServiceMock.Verify(tr => tr.CheckAccess(operation), times);
        }

        private void SetupEditMissingEntityException(Feedback feedback)
        {
            _feedbackRepositoryMock.Setup(m =>
                    m.Update(It.Is<Feedback>(grs => FeedbacksAreEqual(grs, feedback))))
                .Throws(new ConcurrencyException());
        }

        private void MockCurrentUser(int userId)
        {
            SetupCurrentUserServiceGetCurrentUserId(VALID_USER_ID);

            _userServiceMock.Setup(
                us => us.GetUser(userId)).Returns(
                new User { PersonName = TEST_NAME });
        }

        private bool FeedbacksAreEqual(Feedback x, Feedback y)
        {
            return new FeedbackComparer().Compare(x, y) == 0;
        }

        private void AssertFeedbackAreEquals(Feedback expected, Feedback actual)
        {
            TestHelper.AreEqual(expected, actual, new FeedbackComparer());
        }

        private void VerifyCreateFeedback(Feedback feedback, Times times, string message)
        {
            _feedbackRepositoryMock.Verify(
                pr => pr.Add(
                    It.Is<Feedback>(f =>
                        FeedbacksAreEqual(f, feedback))),
                times,
                message);
        }

        private void VerifySaveFeedback(Feedback feedback, Times times, string message)
        {
            _unitOfWorkMock.Verify(
                uow => uow.Commit(),
                times,
                message);
        }

        private void VerifyExceptionThrown(Exception exception, Exception expected)
        {
            Assert.NotNull(exception);
            Assert.Equal(exception.Message, expected.Message);
        }

        private void VerifyEditFeedback(Feedback feedback, Times times)
        {
            _feedbackRepositoryMock.Verify(tr => tr.Update(It.Is<Feedback>(t => FeedbacksAreEqual(t, feedback))),
                times);
            _unitOfWorkMock.Verify(uow => uow.Commit(), times);
        }

        private void VerifyAdminName(Feedback feedback, string expectedAdminName)
        {
            feedback.AdminName.Should().NotBeNullOrEmpty("Admin's name didn't set");
            Assert.True(feedback.AdminName.Equals(expectedAdminName), "Expected and actual admin names aren't equal");
        }

        private void MockAuthServiceThrowsException(AuthOperation operation)
        {
            _authServiceMock.Setup(tr => tr.CheckAccess(operation)).Throws<AuthorizationException>();
        }

        private void MockGetAllFeedbacksQuery(IEnumerable<Feedback> testData)
        {
            _getAllFeedbacksQueryMock.Setup(tr => tr.Execute(It.IsAny<GetAllCriteria>())).Returns(testData.ToList());
        }

        private void MockGetFeedbackByIdQuery(Feedback testData)
        {
            _getFeedbackByIdQueryMock.Setup(tr => tr.Execute(It.IsAny<FindByIdCriteria>())).Returns(testData);
        }

        private void SetupCurrentUserServiceGetCurrentUserId(int id)
        {
            _currentUserServiceMock.Setup(cs => cs.GetCurrentUserId())
                .Returns(id);
        }

        private void MockMailService(EmailMessage message)
        {
            _mailServiceMock.Setup(tr => tr.Send(message));
        }

        private void MockUserService()
        {
            var user = new UserBuilder().Build();
            var userList = new List<User> { user };
            _userServiceMock.Setup(tr => tr.GetAdminsList()).Returns(userList);
        }

        [Fact]
        public void Close_AnsweredFeedback_LastUpdateInfoSet()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Answered)
                .Build();

            MockGetFeedbackByIdQuery(feedback);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Close(EXISTING_ID);

            // Assert
            VerifyAdminName(feedback, TEST_NAME);
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Close_AnsweredFeedback_UpdatedFeedbackStatusToClose()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Answered)
                .Build();

            MockGetFeedbackByIdQuery(feedback);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Close(EXISTING_ID);

            // Assert
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Close_ClosedFeedback_InvalidOperationExceptionThrown()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Closed)
                .Build();
            MockGetFeedbackByIdQuery(feedback);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            Action act = () => sut.Close(EXISTING_ID);

            // Assert
            act.Should().Throw<InvalidOperationException>("Feedback status can't be changed to this status");
        }

        [Fact]
        public void Close_FeedbackDoesNotExist_ExceptionThrown()
        {
            // Arrange
            var sut = BuildSUT();

            // Act 
            Action act = () => sut.Close(INVALID_FEEDBACK_ID);

            // Assert
            act.Should().Throw<MissingEntityException>("A feedback with specified identifier was not found");
        }

        [Fact]
        public void Close_NewFeedback_LastUpdateInfoSet()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .Build();

            MockGetFeedbackByIdQuery(feedback);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Close(EXISTING_ID);

            // Assert
            VerifyAdminName(feedback, TEST_NAME);
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Close_NewFeedback_UpdatedFeedbackStatusToClose()
        {
            // Arrange
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockGetFeedbackByIdQuery(feedback);

            MockCurrentUser(VALID_USER_ID);
            var sut = BuildSUT();

            // Act
            sut.Close(EXISTING_ID);

            // Assert
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Close_NoCloseRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Close);
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Close(EXISTING_ID);

            // Assert
            act.Should().Throw<AuthorizationException>("Requested operation is not allowed");
        }

        [Fact]
        public void Close_NoCloseRights_DbNotChanged()
        {
            // Arrange
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Close);

            var sut = BuildSUT();

            // Act
            Action act = () => sut.Close(EXISTING_ID);

            // Assert
            act.Should().Throw<AuthorizationException>();
            VerifyEditFeedback(feedback, Times.Never());
            VerifyCheckAccess(AuthOperations.Feedbacks.Close, Times.Once());
        }

        [Fact]
        public void Close_ReadFeedback_UpdatedFeedbackStatusToClose()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Read)
                .Build();

            MockGetFeedbackByIdQuery(feedback);

            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Close(EXISTING_ID);

            // Assert
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Create_FeedbackPassed_FeedbackCreated()
        {
            // Arrange
            var newFeedback = new FeedbackBuilder()
                .Build();

            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockUserService();

            var sut = BuildSUT();

            // Act
            sut.Create(newFeedback);

            // Assert
            VerifyCreateFeedback(
                newFeedback,
                Times.Once(),
                "Parameter feedback is not equal to Instance of feedback");
            VerifySaveFeedback(
                newFeedback,
                Times.Once(),
                "DB should not be updated");
        }

        [Fact]
        public void Create_InvalidNullFeedback_ArgumentNullExceptionIsThrown()
        {
            // Arrange
            Exception exception = null;
            Feedback newFeedback = null;

            var sut = BuildSUT();

            // Act
            try
            {
                sut.Create(newFeedback);
            }
            catch (ArgumentNullException ex)
            {
                exception = ex;
            }

            // Assert
            VerifyExceptionThrown(
                exception,
                new ArgumentNullException("feedbackToCreate"));
        }

        [Fact]
        public void Get_NoReadRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Read);
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Get(EXISTING_ID);

            // Assert
            act.Should().Throw<AuthorizationException>();
        }

        [Fact]
        public void GetAll_FeedbacksExist_FeedbacksReturned()
        {
            // Arrange
            var testData = _testFixture.TestFeedbacks().Build();
            MockGetAllFeedbacksQuery(testData);

            var expected = new FeedbackServiceTestFixture()
                .TestFeedbacks()
                .Build();

            var sut = BuildSUT();

            // Act
            var actual = sut.Get();

            // Assert
            TestHelper.AreEqual(expected, actual, new FeedbackComparer());
        }

        [Fact]
        public void GetAll_NoReadRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Read);
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Get();

            // Assert
            act.Should().Throw<AuthorizationException>();
        }

        [Fact]
        public void GetById_FeedbackExists_FeedbackReturned()
        {
            // Arrange
            var expected = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockGetFeedbackByIdQuery(expected);

            var sut = BuildSUT();

            // Act
            var actual = sut.Get(EXISTING_ID);

            // Assert
            TestHelper.AreEqual(expected, actual, new FeedbackComparer());
        }

        [Fact]
        public void GetDetails_FeedbackDoesNotExist_ExceptionThrown()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            Action act = () => sut.GetDetails(INVALID_FEEDBACK_ID);

            // Assert
            act.Should().Throw<MissingEntityException>("A feedback with specified identifier was not found");
        }

        [Fact]
        public void GetDetails_NewFeedback_UpdatedFeedbackStatusToRead()
        {
            // Arrange
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockGetFeedbackByIdQuery(feedback);

            var sut = BuildSUT();

            // Act
            sut.GetDetails(EXISTING_ID);

            // Assert
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void GetDetails_NoReadRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Read);
            var sut = BuildSUT();

            // Act
            Action act = () => sut.GetDetails(EXISTING_ID);

            // Assert
            act.Should().Throw<AuthorizationException>();
        }

        [Fact]
        public void GetDetails_NoReadRights_DbNotChanged()
        {
            // Arrange
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Read);

            var sut = BuildSUT();

            // Act
            Action act = () => sut.GetDetails(EXISTING_ID);

            // Assert
            act.Should().Throw<AuthorizationException>();
            VerifyEditFeedback(feedback, Times.Never());
            VerifyCheckAccess(AuthOperations.Feedbacks.Read, Times.Once());
        }

        [Fact]
        public void Reply_ClosedFeedback_DbNotChanged()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Closed)
                .Build();

            MockGetFeedbackByIdQuery(feedback);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            Action act = () => sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            VerifyEditFeedback(feedback, Times.Never());
        }

        [Fact]
        public void Reply_ClosedFeedback_InvalidOperationExceptionThrown()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Closed)
                .Build();

            MockGetFeedbackByIdQuery(feedback);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            Action act = () => sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            act.Should().Throw<InvalidOperationException>("Feedback status can't be changed to this status");
        }

        [Fact]
        public void Reply_FeedbackDoesNotExist_ExceptionThrown()
        {
            // Arrange
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            act.Should().Throw<MissingEntityException>("A feedback with specified identifier was not found");
        }

        [Fact]
        public void Reply_NewFeedback_LastUpdateInfoSet()
        {
            // Arrange
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockGetFeedbackByIdQuery(feedback);
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            VerifyAdminName(feedback, TEST_NAME);
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Reply_NewFeedback_UpdatedFeedbackStatusToAnswered()
        {
            // Arrange
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();
            MockGetFeedbackByIdQuery(feedback);
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Reply_NoReplyRights_AuthorizationExceptionThrown()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Reply);
            var sut = BuildSUT();

            // Act
            Action act = () => sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            act.Should().Throw<AuthorizationException>();
        }

        [Fact]
        public void Reply_NoReplyRights_DbNotChanged()
        {
            // Arrange
            MockAuthServiceThrowsException(AuthOperations.Feedbacks.Reply);
            var feedback = new FeedbackBuilder().WithId(EXISTING_ID).Build();

            var sut = BuildSUT();

            // Act
            Action act = () => sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            act.Should().Throw<AuthorizationException>();
            VerifyEditFeedback(feedback, Times.Never());
            VerifyCheckAccess(AuthOperations.Feedbacks.Reply, Times.Once());
        }

        [Fact]
        public void Reply_ReadFeedback_UpdatedFeedbackStatusToAnswered()
        {
            // Arrange
            var feedback = new FeedbackBuilder()
                .WithId(EXISTING_ID)
                .WithStatus(FeedbackStatusEnum.Read)
                .Build();
            MockGetFeedbackByIdQuery(feedback);
            var emailMessage = new EmailMessageBuilder().Build();
            MockMailService(emailMessage);
            MockCurrentUser(VALID_USER_ID);

            var sut = BuildSUT();

            // Act
            sut.Reply(EXISTING_ID, MESSAGE);

            // Assert
            VerifyEditFeedback(feedback, Times.Once());
        }

        [Fact]
        public void Update_FeedbackPassed_FeedbackUpdated()
        {
            // Arrange
            var actual = new FeedbackBuilder()
                .Build();
            var expected = new FeedbackBuilder()
                .WithDate(_feedbackTestDate)
                .Build();

            MockUserService();

            var sut = BuildSUT();

            // Act
            sut.Create(actual);

            // Assert
            actual.Should().BeEquivalentTo(actual);
        }
    }
}