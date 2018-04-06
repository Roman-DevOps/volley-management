using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using VolleyManagement.UI.Areas.WebApi.ViewModels.GameReports;

namespace VolleyManagement.UnitTests.WebApi.ViewModels
{
    [ExcludeFromCodeCoverage]
    internal class PivotStandingsEntryViewModelComparer : IComparer, IComparer<PivotStandingsTeamViewModel>
    {
        public int Compare(object x, object y)
        {
            return Compare(x as PivotStandingsTeamViewModel, y as PivotStandingsTeamViewModel);
        }

        public int Compare(PivotStandingsTeamViewModel x, PivotStandingsTeamViewModel y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null)
            {
                return -1;
            }

            if (y == null)
            {
                return 1;
            }

            return CompareInternal(x, y);
        }

        private int CompareInternal(PivotStandingsTeamViewModel x, PivotStandingsTeamViewModel y)
        {
            x.TeamName.Should().Be(y.TeamName, "TeamName should match");
            x.TeamId.Should().Be(y.TeamId, $"[TeamName{x.TeamName}] TeamId should match");
            x.Points.Should().Be(y.Points, $"[TeamName{x.TeamName}] Points should match");
            x.SetsRatio.GetValueOrDefault().Should().BeApproximately(y.SetsRatio.GetValueOrDefault(), 0.001f,
                $"[TeamName{x.TeamName}] SetsRatio should match");

            return 0;
        }
    }
}