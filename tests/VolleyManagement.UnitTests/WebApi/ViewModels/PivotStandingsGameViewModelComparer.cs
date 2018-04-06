using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using VolleyManagement.UI.Areas.WebApi.ViewModels.GameReports;
using VolleyManagement.UnitTests.Mvc.ViewModels;

namespace VolleyManagement.UnitTests.WebApi.ViewModels
{
    [ExcludeFromCodeCoverage]
    internal class PivotStandingsGameViewModelComparer : IComparer, IComparer<PivotStandingsGameViewModel>
    {
        public int Compare(object x, object y)
        {
            return Compare(x as PivotStandingsGameViewModel, y as PivotStandingsGameViewModel);
        }

        public int Compare(PivotStandingsGameViewModel x, PivotStandingsGameViewModel y)
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

        private int CompareInternal(PivotStandingsGameViewModel x, PivotStandingsGameViewModel y)
        {
            y.AwayTeamId.Should().Be(x.AwayTeamId, $" AwayTeamId should match");
            y.HomeTeamId.Should().Be(x.HomeTeamId, $" HomeTeamId should match");

            TestHelper.AreEqual(x.Results, y.Results, new ShortGameResultViewModelComparer());

            return 0;
        }
    }
}