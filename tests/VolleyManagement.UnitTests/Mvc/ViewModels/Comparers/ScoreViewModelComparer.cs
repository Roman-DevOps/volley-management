using FluentAssertions;

namespace VolleyManagement.UnitTests.Mvc.ViewModels.Comparers
{
    using Xunit;
    using UI.Areas.Mvc.ViewModels.GameResults;

    public static class ScoreViewModelComparer
    {
        public static void AssertAreEqual(ScoreViewModel expected, ScoreViewModel actual, string messagePrefix = "")
        {
            expected.Home.Should().Be(expected.Home, $"{messagePrefix}Home score should be equal.");
            expected.Away.Should().Be(expected.Away, $"{messagePrefix}Away score should be equal.");
            expected.IsTechnicalDefeat.Should().Be(expected.IsTechnicalDefeat, $"{messagePrefix}IsTechnicalDefeat should be equal.");
        }
    }
}