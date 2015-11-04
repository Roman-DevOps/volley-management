﻿namespace VolleyManagement.UnitTests.Services.TournamentService
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using Data.Queries.Common;
    using Data.Queries.Tournaments;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Ninject;
    using VolleyManagement.Contracts;
    using VolleyManagement.Contracts.Exceptions;
    using VolleyManagement.Crosscutting.Contracts.Providers;
    using VolleyManagement.Data.Contracts;
    using VolleyManagement.Data.Queries.Common;
    using VolleyManagement.Data.Queries.Tournaments;
    using VolleyManagement.Domain.TournamentsAggregate;
    using VolleyManagement.Services;

    /// <summary>
    /// Tests for TournamentService class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class TournamentServiceTests
    {
        private const int MINIMUN_REGISTRATION_PERIOD_MONTH = 3;
        private const int FIRST_TOURNAMENT_ID = 1;

        private const int UPCOMING_TOURNAMENTS_MONTH_LIMIT = 3;

        private readonly TournamentServiceTestFixture _testFixture = new TournamentServiceTestFixture();

        private readonly Mock<ITournamentRepository> _tournamentRepositoryMock = new Mock<ITournamentRepository>();

        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new Mock<IUnitOfWork>();

        private readonly Mock<IQuery<Tournament, UniqueTournamentCriteria>> _uniqueTournamentQueryMock =
            new Mock<IQuery<Tournament, UniqueTournamentCriteria>>();

        private readonly Mock<ITournamentService> _tournamentServiceMock = new Mock<ITournamentService>();
        private readonly Mock<IQuery<List<Tournament>, GetAllCriteria>> _getAllQueryMock =
            new Mock<IQuery<List<Tournament>, GetAllCriteria>>();

        private readonly Mock<IQuery<Tournament, FindByIdCriteria>> _getByIdQueryMock =
            new Mock<IQuery<Tournament, FindByIdCriteria>>();

        private readonly Mock<TimeProvider> _timeMock = new Mock<TimeProvider>();

        private IKernel _kernel;

        /// <summary>
        /// Initializes test data.
        /// </summary>
        [TestInitialize]
        public void TestInit()
        {
            _kernel = new StandardKernel();
            _kernel.Bind<ITournamentRepository>()
                   .ToConstant(_tournamentRepositoryMock.Object);
            _kernel.Bind<IQuery<Tournament, UniqueTournamentCriteria>>()
                   .ToConstant(_uniqueTournamentQueryMock.Object);
            _kernel.Bind<IQuery<List<Tournament>, GetAllCriteria>>()
                   .ToConstant(_getAllQueryMock.Object);
            _kernel.Bind<IQuery<Tournament, FindByIdCriteria>>()
                   .ToConstant(_getByIdQueryMock.Object);

            _tournamentRepositoryMock.Setup(tr => tr.UnitOfWork).Returns(_unitOfWorkMock.Object);
            this._timeMock.SetupGet(tp => tp.UtcNow).Returns(new DateTime(2015, 04, 01));
            TimeProvider.Current = this._timeMock.Object;
        }

        /// <summary>
        /// Cleanup test data
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            TimeProvider.ResetToDefault();
        }

        /// <summary>
        /// Test for FinById method.
        /// </summary>
        [TestMethod]
        public void FindById_Existing_TournamentFound()
        {
            // Arrange
            var sut = _kernel.Get<TournamentService>();

            var tournament = CreateAnyTournament(FIRST_TOURNAMENT_ID);
            MockGetByIdQuery(tournament);

            //// Act
            var actualResult = sut.Get(FIRST_TOURNAMENT_ID);

            // Assert
            TestHelper.AreEqual<Tournament>(tournament, actualResult, new TournamentComparer());
        }

        /// <summary>
        /// Test for FinById method. Null returned.
        /// </summary>
        [TestMethod]
        public void FindById_NotExistingTournament_NullReturned()
        {
            // Arrange
            MockGetByIdQuery(null);
            var sut = _kernel.Get<TournamentService>();

            // Act
            var tournament = sut.Get(1);

            // Assert
            Assert.IsNull(tournament);
        }

        /// <summary>
        /// Test for Get() method. The method should return existing tournaments
        /// (order is important).
        /// </summary>
        [TestMethod]
        public void GetAll_TournamentsExist_TournamentsReturned()
        {
            // Arrange
            var testData = _testFixture.TestTournaments()
                                       .Build();
            MockGetAllTournamentsQuery(testData);
            var sut = _kernel.Get<TournamentService>();
            var expected = new TournamentServiceTestFixture()
                                            .TestTournaments()
                                            .Build()
                                            .ToList();

            // Act
            var actual = sut.Get().ToList();

            // Assert
            CollectionAssert.AreEqual(expected, actual, new TournamentComparer());
        }

        /// <summary>
        /// Test for Edit() method. The method should invoke Update() method of ITournamentRepository
        /// and Commit() method of IUnitOfWork.
        /// </summary>
        [TestMethod]
        public void Edit_TournamentAsParam_TournamentEdited()
        {
            // Arrange
            var testTournament = new TournamentBuilder()
                                        .WithId(1)
                                        .WithName("Test Tournament")
                                        .Build();
            var sut = _kernel.Get<TournamentService>();

            // Act
            sut.Edit(testTournament);

            // Assert
            _tournamentRepositoryMock.Verify(
                tr => tr.Update(It.Is<Tournament>(t => TournamentsAreEqual(t, testTournament))),
                Times.Once());
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Once());
        }

        /// <summary>
        /// Test for Edit() method with null as input parameter. The method should throw NullReferenceException
        /// and shouldn't invoke Commit() method of IUnitOfWork.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void Edit_TournamentNullAsParam_ExceptionThrown()
        {
            // Arrange
            Tournament testTournament = null;
            _tournamentRepositoryMock.Setup(tr => tr.Update(null)).Throws<NullReferenceException>();
            var sut = _kernel.Get<TournamentService>();

            // Act
            sut.Edit(testTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Edit() method where input tournament has non-unique name. The method should
        /// throw TournamentValidationException and shouldn't invoke Commit() method of IUnitOfWork.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException))]
        public void Edit_TournamentWithNonUniqueName_ExceptionThrown()
        {
            // Arrange
            var testData = new TournamentBuilder()
                                        .WithId(1)
                                        .WithName("Non-Unique Tournament")
                                        .Build();

            Tournament nonUniqueNameTournament = new TournamentBuilder()
                                                        .WithId(2)
                                                        .WithName("Non-Unique Tournament")
                                                        .Build();

            MockGetUniqueTournamentQuery(testData);
            var sut = _kernel.Get<TournamentService>();

            // Act
            sut.Edit(nonUniqueNameTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method where input applying end date goes before start applying date
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Начало периода заявок должно быть раньше чем его окнчание")]
        public void Create_TournamentWithInvalidApplyingDate_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            var newTournament = tournamentBuilder
                .WithApplyingPeriodStart(now.AddDays(1))
                .WithApplyingPeriodEnd(now.AddDays(-1))
                .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method where input applying start date goes before now
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Период заявок должен следовать перед началом игр")]
        public void Create_TournamentWithInvalidGameStartDate_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            var newTournament = tournamentBuilder
                .WithGamesStart(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH - 1))
                .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method where input applying start date goes before now
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Окончание трансферного периода должно быть раньше окончания игр")]
        public void Create_TournamentTransferEndGoesAfterGamesEnd_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            int tournamentPeriodMonth = TournamentBuilder.TRANSFER_PERIOD_MONTH;
            var newTournament = tournamentBuilder
                .WithGamesEnd(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + tournamentPeriodMonth))
                .WithTransferEnd(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + tournamentPeriodMonth + 1))
                .Build();
            var sut = _kernel.Get<TournamentService>();

            // Act
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method where input applying start date goes before now
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Начальная дата турнира должна следовать перед ее окончанием")]
        public void Create_TournamentGamesStartGoesAfterGamesEnd_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            var newTournament = tournamentBuilder
                .WithGamesStart(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH))
                .WithGamesEnd(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH))
                .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method. Transfer end before transfer start
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Начало трансферного периода должно быть раньше чем его окнчание")]
        public void Create_TournamentTransferEndBeforeStart_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            var newTournament = tournamentBuilder
                .WithTransferStart(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + 1).AddDays(1))
                .WithTransferEnd(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + 1))
                .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method. Transfer start is before than tournaments start
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Начало трансферного окна должно быть после начала игр")]
        public void Create_TournamentTransferStartBeforeTournamentStart_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            var newTournament = tournamentBuilder
                .WithGamesStart(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + 1))
                .WithTransferStart(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + 1).AddDays(-1))
                .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method. Tournament end is before than tournaments start
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException), "Начальная дата турнира должна следовать перед ее окончанием")]
        public void Create_TournamentTournamentEndBeforeTournamentStart_ExceptionThrown()
        {
            // Arrange
            var tournamentBuilder = new TournamentBuilder();
            var now = TimeProvider.Current.UtcNow;
            var newTournament = tournamentBuilder
                .WithGamesStart(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + 1).AddDays(1))
                .WithGamesEnd(now.AddMonths(MINIMUN_REGISTRATION_PERIOD_MONTH + 1))
                .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method. The method should return a created tournament.
        /// </summary>
        [TestMethod]
        public void Create_TournamentNotExist_TournamentCreated()
        {
            // Arrange
            DateTime applyingPeriodStart = DateTime.UtcNow.AddDays(1);
            DateTime applyingPeriodEnd = applyingPeriodStart.AddDays(1);
            DateTime gamesStart = applyingPeriodEnd.AddDays(1);
            DateTime transferStart = gamesStart.AddDays(1);
            DateTime transferEnd = transferStart.AddDays(1);
            DateTime gamesEnd = transferEnd.AddDays(1);

            var newTournament = new TournamentBuilder().WithApplyingPeriodStart(applyingPeriodStart)
                                                       .WithApplyingPeriodEnd(applyingPeriodEnd)
                                                       .WithGamesStart(gamesStart)
                                                       .WithGamesEnd(gamesEnd)
                                                       .WithTransferStart(transferStart)
                                                       .WithTransferEnd(transferEnd)
                                                       .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(newTournament);

            // Assert
            _tournamentRepositoryMock.Verify(
                tr => tr.Add(It.Is<Tournament>(t => TournamentsAreEqual(t, newTournament))));
            _unitOfWorkMock.Verify(u => u.Commit());
        }

        /// <summary>
        /// Test for Create() method with null as a parameter. The method should throw ArgumentNullException
        /// and shouldn't invoke Commit() method of IUnitOfWork.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Create_TournamentNullAsParam_ExceptionThrown()
        {
            // Arrange
            Tournament testTournament = null;
            _tournamentRepositoryMock.Setup(tr => tr.Add(null)).Throws<InvalidOperationException>();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(testTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// Test for Create() method where tournament name should be unique. The method should throw ArgumentException
        /// and shouldn't invoke Commit() method of IUnitOfWork.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(TournamentValidationException))]
        public void Create_TournamentWithNonUniqueName_ExceptionThrown()
        {
            // Arrange
            var testData = new TournamentServiceTestFixture()
                                    .AddTournament(new TournamentBuilder()
                                                        .WithId(1)
                                                        .WithName("Tournament 5")
                                                        .Build())
                                    .Build();

            Tournament nonUniqueNameTournament = new TournamentBuilder()
                                                        .WithId(2)
                                                        .WithName("Tournament 5")
                                                        .Build();

            // Act
            var sut = _kernel.Get<TournamentService>();
            sut.Create(nonUniqueNameTournament);

            // Assert
            _unitOfWorkMock.Verify(u => u.Commit(), Times.Never());
        }

        /// <summary>
        /// GetActual method test. The method should invoke Find() method of ITournamentRepository
        /// </summary>
        public void GetActual_ActualTournamentsRequest_FindCalled()
        {
            // Act
            var tournamentService = _kernel.Get<TournamentService>();
            tournamentService.GetActual();

            // Assert
            ////_tournamentRepositoryMock.Verify(m => m.Find(), Times.Once());
        }

        /// <summary>
        /// GetActual method test. The method should return actual tournaments
        /// </summary>
        [TestMethod]
        public void GetActual_TournamentsExist_ActualTournamentsReturnes()
        {
            // Arrange
            var sut = _kernel.Get<TournamentService>();
            var testData = _testFixture.TestTournaments()
                                       .Build();
            MockGetAllTournamentsQuery(testData);

            var expected = BuildActualTournamentsList();

            // Act
            var actual = sut.GetActual().ToList();

            // Assert
            CollectionAssert.AreEqual(expected, actual, new TournamentComparer());
        }

        /// <summary>
        /// Find out whether two tournament objects have the same properties.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if given tournaments have the same properties.</returns>
        private bool TournamentsAreEqual(Tournament x, Tournament y)
        {
            TournamentComparer comparer = new TournamentComparer();
            return comparer.Compare(x, y) == 0;
        }

        /// <summary>
        /// Mocks get all tournaments query.
        /// </summary>
        /// <param name="testData">Test data to mock.</param>
        private void MockGetAllTournamentsQuery(IEnumerable<Tournament> testData)
        {
            _getAllQueryMock.Setup(tr => tr.Execute(It.IsAny<GetAllCriteria>())).Returns(testData.ToList());
        }

        /// <summary>
        /// Mocks Execute method for get by ID.
        /// </summary>
        /// <param name="testData">Test data to mock.</param>
        private void MockGetByIdQuery(Tournament testData)
        {
            _getByIdQueryMock.Setup(tr => tr.Execute(It.IsAny<FindByIdCriteria>())).Returns(testData);
        }

        /// <summary>
        /// Mocks Execute method for get by unique criteria.
        /// </summary>
        /// <param name="testData">Test data to mock.</param>
        private void MockGetUniqueTournamentQuery(Tournament testData)
        {
            _uniqueTournamentQueryMock.Setup(tr => tr.Execute(It.IsAny<UniqueTournamentCriteria>())).Returns(testData);
        }

        /// <summary>
        /// Creating any tournament with required ID
        /// </summary>
        /// <param name="id">Required ID</param>
        /// <returns>Created tournament</returns>
        private Tournament CreateAnyTournament(int id)
        {
            return new TournamentBuilder()
                .WithId(id)
                .WithName("Name")
                .WithDescription("Description")
                .WithScheme(TournamentSchemeEnum.One)
                .WithSeason(2014)
                .WithRegulationsLink("link")
                .Build();
        }

        /// <summary>
        /// Builds a list of actual tournaments
        /// </summary>
        /// <returns>List of actual tournaments</returns>
        private List<Tournament> BuildActualTournamentsList()
        {
            return new TournamentServiceTestFixture()
                            .AddTournament(new TournamentBuilder()
                                            .WithId(1)
                                            .WithName("Tournament 1")
                                            .WithDescription("Tournament 1 description")
                                            .WithSeason(2014)
                                            .WithScheme(TournamentSchemeEnum.One)
                                            .WithRegulationsLink("www.Volleyball.dp.ua/Regulations/Tournaments('1')")
                                            .WithApplyingPeriodStart(new DateTime(2015, 02, 20))
                                            .WithApplyingPeriodEnd(new DateTime(2015, 06, 20))
                                            .WithGamesStart(new DateTime(2015, 06, 30))
                                            .WithGamesEnd(new DateTime(2015, 11, 30))
                                            .WithTransferStart(new DateTime(2015, 08, 20))
                                            .WithTransferEnd(new DateTime(2015, 09, 10))
                                            .Build())
                            .AddTournament(new TournamentBuilder()
                                            .WithId(2)
                                            .WithName("Tournament 2")
                                            .WithDescription("Tournament 2 description")
                                            .WithSeason(2014)
                                            .WithScheme(TournamentSchemeEnum.Two)
                                            .WithRegulationsLink("www.Volleyball.dp.ua/Regulations/Tournaments('2')")
                                            .WithApplyingPeriodStart(new DateTime(2015, 02, 20))
                                            .WithApplyingPeriodEnd(new DateTime(2015, 06, 20))
                                            .WithGamesStart(new DateTime(2015, 06, 30))
                                            .WithGamesEnd(new DateTime(2015, 11, 30))
                                            .WithTransferStart(new DateTime(2015, 08, 20))
                                            .WithTransferEnd(new DateTime(2015, 09, 10))
                                            .Build())
                            .AddTournament(new TournamentBuilder()
                                            .WithId(3)
                                            .WithName("Tournament 3")
                                            .WithDescription("Tournament 3 description")
                                            .WithSeason(2014)
                                            .WithScheme(TournamentSchemeEnum.TwoAndHalf)
                                            .WithRegulationsLink("www.Volleyball.dp.ua/Regulations/Tournaments('3')")
                                            .WithApplyingPeriodStart(new DateTime(2015, 02, 20))
                                            .WithApplyingPeriodEnd(new DateTime(2015, 06, 20))
                                            .WithGamesStart(new DateTime(2015, 06, 30))
                                            .WithGamesEnd(new DateTime(2015, 11, 30))
                                            .WithTransferStart(new DateTime(2015, 08, 20))
                                            .WithTransferEnd(new DateTime(2015, 09, 10))
                                            .Build())
                            .Build();
    }
}
