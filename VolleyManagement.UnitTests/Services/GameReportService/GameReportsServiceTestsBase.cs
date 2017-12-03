﻿namespace VolleyManagement.UnitTests.Services.GameReportService
{
    using System.Collections.Generic;
    using Contracts;
    using Data.Contracts;
    using Data.Queries.GameResult;
    using Data.Queries.Team;
    using Data.Queries.Tournament;
    using Domain.GameReportsAggregate;
    using Domain.GamesAggregate;
    using Domain.TeamsAggregate;
    using Domain.TournamentsAggregate;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using VolleyManagement.Services;

    public abstract class GameReportsServiceTestsBase
    {
        protected const int TOURNAMENT_ID = 1;
        protected const int TOP_TEAM_INDEX = 0;

        private Mock<IQuery<List<GameResultDto>, TournamentGameResultsCriteria>> _tournamentGameResultsQueryMock;
        private Mock<IQuery<List<TeamTournamentDto>, FindByTournamentIdCriteria>> _tournamentTeamsQueryMock;
        private Mock<IQuery<TournamentScheduleDto, TournamentScheduleInfoCriteria>> _tournamentScheduleDtoByIdQueryMock;

        protected void InitializeTest()
        {
            _tournamentScheduleDtoByIdQueryMock = new Mock<IQuery<TournamentScheduleDto, TournamentScheduleInfoCriteria>>();
            _tournamentTeamsQueryMock = new Mock<IQuery<List<TeamTournamentDto>, FindByTournamentIdCriteria>>();
            _tournamentGameResultsQueryMock = new Mock<IQuery<List<GameResultDto>, TournamentGameResultsCriteria>>();
        }

        protected IGameReportService BuildSUT()
        {
            return new GameReportService(
                _tournamentGameResultsQueryMock.Object,
                _tournamentTeamsQueryMock.Object,
                _tournamentScheduleDtoByIdQueryMock.Object);
        }

        protected void MockTournamentGameResultsQuery(int tournamentId, List<GameResultDto> testData)
        {
            _tournamentGameResultsQueryMock.Setup(m =>
                    m.Execute(It.Is<TournamentGameResultsCriteria>(c => c.TournamentId == tournamentId)))
                .Returns(testData);
        }

        protected void MockTournamentTeamsQuery(int tournamentId, List<TeamTournamentDto> testData)
        {
            _tournamentTeamsQueryMock.SetupSequence(m =>
                    m.Execute(It.Is<FindByTournamentIdCriteria>(c => c.TournamentId == tournamentId)))
                .Returns(testData);
        }

        protected static List<TeamTournamentDto> TeamsInSingleDivisionSingleGroup()
            => new TeamInTournamentTestFixture().WithTeamsInSingleDivisionSingleGroup().Build();

        protected static List<TeamTournamentDto> TeamsInTwoDivisionTwoGroups()
            => new TeamInTournamentTestFixture().WithTeamsInTwoDivisionTwoGroups().Build();

        protected static void AssertTournamentStandingsAreEqual<T>(
            TournamentStandings<T> expected,
            TournamentStandings<T> actual,
            string message,
            IComparer<T> comparer)
        {
            int compareResult;
            var errorDetails = string.Empty;
            try
            {
                compareResult = new TournamentStandingsComparer<T>(comparer)
                    .Compare(expected, actual);
            }
            catch (AssertFailedException e)
            {
                compareResult = -1;
                errorDetails = $" Error Details: {e.Message}";
            }

            Assert.IsTrue(compareResult == 0, $"{message}{errorDetails}");
        }
    }
}