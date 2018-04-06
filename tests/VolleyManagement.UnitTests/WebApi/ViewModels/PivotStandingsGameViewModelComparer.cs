using FluentAssertions;

namespace VolleyManagement.UnitTests.WebApi.ViewModels
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Xunit;
    using UI.Areas.WebApi.ViewModels.GameReports;
    using Mvc.ViewModels;

    [ExcludeFromCodeCoverage]
    internal class PivotStandingsGameViewModelComparer : IComparer, IComparer<PivotStandingsGameViewModel>
    {
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

        public int Compare(object x, object y)
        {
            return Compare(x as PivotStandingsGameViewModel, y as PivotStandingsGameViewModel);
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
