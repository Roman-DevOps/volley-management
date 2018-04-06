using System.Collections.Generic;
using FluentAssertions;
using VolleyManagement.Domain.GameReportsAggregate;

namespace VolleyManagement.UnitTests.Services.GameReportService
{
    internal class PivotStandingsComparer : IComparer<PivotStandingsDto>
    {
        private bool hasComparerByGames = true;
        private readonly TeamStandingsDtoComparer teamsComparer;

        public PivotStandingsComparer()
        {
            teamsComparer = new TeamStandingsDtoComparer();
        }

        public int Compare(PivotStandingsDto x, PivotStandingsDto y)
        {
            y.DivisionId.Should().Be(x.DivisionId, "Division Ids do not match");
            y.DivisionName.Should().Be(x.DivisionName, $"[DivisionId={x.DivisionId}] Division Names do not match");
            y.LastUpdateTime.Should()
                .Be(x.LastUpdateTime, $"[DivisionId={x.DivisionId}] Last Update time do not match");

            x.Teams.Count.Should().Be(y.Teams.Count,
                $"[DivisionId={x.DivisionId}] Number of team entries does not match.");

            for (var i = 0; i < x.Teams.Count; i++)
            {
                if (teamsComparer.Compare(x.Teams[i], y.Teams[i]) != 0)
                {
                    return 1;
                }
            }

            if (hasComparerByGames)
            {
                x.GameResults.Count.Should().Be(y.GameResults.Count,
                    $"[DivisionId={x.DivisionId}] Number of game result entries does not match.");

                var gameResultComparer = new ShortGameResultDtoComparer();
                for (var i = 0; i < x.GameResults.Count; i++)
                {
                    if (gameResultComparer.Compare(x.GameResults[i], y.GameResults[i]) != 0)
                    {
                        return 1;
                    }
                }
            }

            return 0;
        }

        public void WithPointsComparer()
        {
            CleanComparerFlags();
            teamsComparer.HasComparerByPoints = true;
        }

        public void WithSetsComparer()
        {
            CleanComparerFlags();
            teamsComparer.HasComparerBySets = true;
        }

        public void WithBallsComparer()
        {
            CleanComparerFlags();
            teamsComparer.HasComparerByBalls = true;
        }

        public void WithGamesComparer()
        {
            CleanComparerFlags();
            hasComparerByGames = true;
        }

        private void CleanComparerFlags()
        {
            teamsComparer.HasComparerByPoints = false;
            teamsComparer.HasComparerByBalls = false;
            teamsComparer.HasComparerBySets = false;
            hasComparerByGames = false;
        }
    }
}