using System.Collections.Generic;
using FluentAssertions;
using VolleyManagement.Domain.GameReportsAggregate;

namespace VolleyManagement.UnitTests.Services.GameReportService
{
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
            
            (x == null || y == null).Should().BeFalse("One instance is null");

            TestHelper.AreEqual(x.Divisions, y.Divisions, _groupItemComparer);

            return 0;
        }
    }
}