using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using VolleyManagement.Contracts;
using VolleyManagement.Contracts.Authorization;
using VolleyManagement.Domain.FeedbackAggregate;
using VolleyManagement.Domain.UsersAggregate;
using VolleyManagement.UI.Areas.Mvc.Controllers;
using VolleyManagement.UI.Areas.Mvc.ViewModels.FeedbackViewModel;
using VolleyManagement.UnitTests.Mvc.ViewModels;
using VolleyManagement.UnitTests.Services.FeedbackService;
using Xunit;

namespace VolleyManagement.UnitTests.Mvc.Controllers
{
    /// <summary>
    ///     Feedbacks controller tests.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class FeedbacksControllerTests
    {
        /// <summary>
        ///     Initializes test data.
        /// </summary>
        public FeedbacksControllerTests()
        {
            _feedbackServiceMock = new Mock<IFeedbackService>();
            _userServiceMock = new Mock<IUserService>();
            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _captchaManagerMock = new Mock<ICaptchaManager>();

            _captchaManagerMock.Setup(m => m.ValidateUserCaptchaAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(true));
        }

        private const int TEST_ID = 1;
        private const int ANONYM_ID = -1;
        private const string CREATE_VIEW = "Create";
        private const string FEEDBACK_SENT_MESSAGE = "FeedbackSentMessage";
        private const string TEST_MAIL = "test@gmail.com";
        private const string TEST_CONTENT = "Test content";
        private const string TEST_ENVIRONMENT = "Test environment";
        private const string EXCEPTION_MESSAGE = "ValidationMessage";
        private const string CHECK_DATA_MESSAGE = "Data is not valid.";
        private const string CHECK_CAPTCHA_MESSAGE = "Please verify that you are not a robot.";
        private const string SUCCESS_SENT_MESSAGE = "Your Feedback has been sent successfully.";

        private readonly Mock<IFeedbackService> _feedbackServiceMock;
        private readonly Mock<IUserService> _userServiceMock;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly Mock<ICaptchaManager> _captchaManagerMock;

        private FeedbackViewModel CreateValidFeedback()
        {
            return
                new FeedbackMvcViewModelBuilder()
                    .WithId(TEST_ID)
                    .WithEmail(TEST_MAIL)
                    .WithContent(TEST_CONTENT)
                    .WithEnvironment(TEST_ENVIRONMENT)
                    .Build();
        }

        private Feedback CreateExpectedFeedback()
        {
            return
                new FeedbackBuilder()
                    .WithId(TEST_ID)
                    .WithEmail(TEST_MAIL)
                    .WithContent(TEST_CONTENT)
                    .WithEnvironment(TEST_ENVIRONMENT)
                    .WithDate(DateTime.MinValue)
                    .Build();
        }

        private FeedbackViewModel CreateInvalidFeedback()
        {
            return
                new FeedbackMvcViewModelBuilder()
                    .WithId(TEST_ID)
                    .WithEmail(string.Empty)
                    .WithContent(string.Empty)
                    .WithEnvironment(string.Empty)
                    .Build();
        }

        private FeedbacksController BuildSUT()
        {
            return new FeedbacksController(
                _feedbackServiceMock.Object,
                _userServiceMock.Object,
                _currentUserServiceMock.Object,
                _captchaManagerMock.Object);
        }

        private void SetInvalidModelState(FeedbacksController controller)
        {
            controller.ModelState.AddModelError("Content", "FieldRequired");
        }

        private void SetupCurrentUserGetId(int id)
        {
            _currentUserServiceMock.Setup(cs => cs.GetCurrentUserId())
                .Returns(id);
        }

        /// <summary>
        ///     Test for create GET method.
        ///     User is authenticated. User email returned.
        /// </summary>
        [Fact]
        public void CreateGetAction_UserIsAuthentificated_UsersEmailPrepolulated()
        {
            // Arrange
            SetupCurrentUserGetId(TEST_ID);

            _userServiceMock.Setup(us => us.GetUser(TEST_ID))
                .Returns(new User {Email = TEST_MAIL});

            var sut = BuildSUT();

            // Act
            var feedback = TestExtensions
                .GetModel<FeedbackViewModel>(sut.Create());

            // Assert
            feedback.UsersEmail.Should().Be(TEST_MAIL);
        }

        /// <summary>
        ///     Test for create GET method.
        ///     User email is empty if user is not authenticated.
        /// </summary>
        [Fact]
        public void
            CreateGetAction_UserIsNotAuthentificated_FeedbackHasEmptyEmailField()
        {
            // Arrange
            var feedback = CreateInvalidFeedback();
            SetupCurrentUserGetId(ANONYM_ID);

            var sut = BuildSUT();

            // Act
            sut.Create();

            // Assert
            feedback.UsersEmail.Should().BeEmpty();
        }

        /// <summary>
        ///     Test for Create POST method.
        ///     While calling IFeedbackService method Create()
        ///     argument exception is thrown, ModelState has changed.
        /// </summary>
        [Fact]
        public void CreatePostAction_ArgumentExceptionThrown_ModelChanged()
        {
            // Arrange
            var feedback = CreateValidFeedback();
            _feedbackServiceMock.Setup(f => f.Create(It.IsAny<Feedback>()))
                .Throws(new ArgumentException(EXCEPTION_MESSAGE));

            var sut = BuildSUT();

            // Act
            sut.Create(feedback).Wait();
            var res = sut.ModelState[EXCEPTION_MESSAGE].Errors;

            // Assert
            sut.ModelState.IsValid.Should().BeFalse();
            res[0].ErrorMessage.Should().Be(EXCEPTION_MESSAGE);
        }

        [Fact]
        public void CreatePostAction_CaptchaIsNotApproved_CheckCaptchaMessageReturned()
        {
            // Arrange
            var feedback = CreateValidFeedback();
            var sut = BuildSUT();

            // Act
            _captchaManagerMock
                .Setup(cm => cm.ValidateUserCaptchaAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(false));
            var res = sut.Create(feedback).Result;
            var returnedDataResult = res.Data as FeedbackMessageViewModel;

            // Assert
            returnedDataResult.ResultMessage.Should().Be(CHECK_CAPTCHA_MESSAGE);
        }

        /// <summary>
        ///     Test for create POST method.
        ///     Feedback is incorrect, Create view is returned.
        /// </summary>
        [Fact]
        public void CreatePostAction_ModelIsInvalid_CheckDataMessageReturned()
        {
            // Arrange
            var feedback = CreateInvalidFeedback();

            var sut = BuildSUT();
            SetInvalidModelState(sut);

            // Act
            var result = sut.Create(feedback).Result;
            var returnedDataResult = result.Data as FeedbackMessageViewModel;

            // Assert
            returnedDataResult.ResultMessage.Should().Be(CHECK_DATA_MESSAGE);
        }

        /// <summary>
        ///     Test for Create POST method.
        ///     Valid model passed. Feedback created.
        /// </summary>
        [Fact]
        public void CreatePostAction_ModelIsValid_FeedbackCreated()
        {
            // Arrange
            var sut = BuildSUT();
            var feedback = CreateValidFeedback();
            var expectedFeedback = CreateExpectedFeedback();

            Feedback actualFeedback = null;
            _feedbackServiceMock.Setup(f => f.Create(It.IsAny<Feedback>()))
                .Callback<Feedback>(a => actualFeedback = a);

            // Act
            sut.Create(feedback).Wait();

            // Assert
            TestHelper.AreEqual(
                expectedFeedback,
                actualFeedback,
                new FeedbackComparer());
        }

        /// <summary>
        ///     Test for create POST method.
        ///     Feedback is correct, feedback sent message returned.
        /// </summary>
        [Fact]
        public void CreatePostAction_ModelIsValid_SuccessSentMessageReturned()
        {
            // Arrange
            var sut = BuildSUT();
            var feedback = CreateValidFeedback();

            // Act
            var result = sut.Create(feedback).Result;
            var returnedDataResult = result.Data as FeedbackMessageViewModel;

            // Assert
            returnedDataResult.ResultMessage.Should().Be(SUCCESS_SENT_MESSAGE);
        }
    }
}