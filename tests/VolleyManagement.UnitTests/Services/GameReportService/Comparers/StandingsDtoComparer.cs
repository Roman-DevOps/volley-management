using FluentAssertions;

namespace VolleyManagement.UnitTests.Services.GameReportService
{
    using System.Collections.Generic;
    using Domain.GameReportsAggregate;
    using Xunit;

    public class StandingsDtoComparer : IComparer<StandingsDto>
    {
        private StandingsEntryComparer standingsComparer;

        public StandingsDtoComparer() : this(new StandingsEntryComparer())
        {
        }

        internal StandingsDtoComparer(StandingsEntryComparer standingsComparer)
        {
            this.standingsComparer = standingsComparer;
        }

        public int Compare(StandingsDto x, StandingsDto y)
        {
            y.DivisionId.Should().Be(x.DivisionId, "Division Ids do not match");
            y.DivisionName.Should().Be(x.DivisionName, $"[DivisionId={x.DivisionId}] Division Names do not match");
            y.LastUpdateTime.Should().Be(x.LastUpdateTime, $"[DivisionId={x.DivisionId}] Last Update time do not match");

            TestHelper.AreEqual(x.Standings, y.Standings, new StandingsEntryComparer());

            return 0;
        }
    }
}