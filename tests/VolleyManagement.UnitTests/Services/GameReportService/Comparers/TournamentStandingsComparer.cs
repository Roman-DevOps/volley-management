using FluentAssertions;

namespace VolleyManagement.UnitTests.Services.GameReportService
{
    using System.Collections.Generic;
    using Domain.GameReportsAggregate;
    using Xunit;

    public class TournamentStandingsComparer<T> : IComparer<TournamentStandings<T>>
    {
        private readonly IComparer<T> _groupItemComparer;

        public TournamentStandingsComparer(IComparer<T> groupItemComparer)
        {
            _groupItemComparer = groupItemComparer;
        }

        public int Compare(TournamentStandings<T> x, TournamentStandings<T> y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            (x == null || y == null).Should().BeTrue("One instance is null");

            TestHelper.AreEqual(x.Divisions, y.Divisions, _groupItemComparer);

            return 0;
        }
    }
}