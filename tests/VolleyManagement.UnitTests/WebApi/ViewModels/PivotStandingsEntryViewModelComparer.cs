﻿using FluentAssertions;

namespace VolleyManagement.UnitTests.WebApi.ViewModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Xunit;
    using UI.Areas.WebApi.ViewModels.GameReports;

    [ExcludeFromCodeCoverage]
    internal class PivotStandingsEntryViewModelComparer : IComparer, IComparer<PivotStandingsTeamViewModel>
    {
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

        public int Compare(object x, object y)
        {
            return Compare(x as PivotStandingsTeamViewModel, y as PivotStandingsTeamViewModel);
        }

        private int CompareInternal(PivotStandingsTeamViewModel x, PivotStandingsTeamViewModel y)
        {
            var result = x.TeamName.CompareTo(y.TeamName);
            if (result != 0)
            {
                Assert.Fail($"TeamName should match");
            }

            result = x.TeamId.CompareTo(y.TeamId);
            if (result != 0)
            {
                Assert.Fail($"[TeamName{x.TeamName}] TeamId should match");
            }

            result = x.Points.CompareTo(y.Points);
            if (result != 0)
            {
                Assert.Fail($"[TeamName{x.TeamName}] Points should match");
            }

            x.SetsRatio.GetValueOrDefault().Should().BeApproximately(y.SetsRatio.GetValueOrDefault(), 0.001f, $"[TeamName{x.TeamName}] SetsRatio should match");

            return 0;
        }

    }
}
