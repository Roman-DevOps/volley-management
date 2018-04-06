using FluentAssertions;
using VolleyManagement.UI.Areas.Mvc.ViewModels.GameResults;

namespace VolleyManagement.UnitTests.Mvc.ViewModels.Comparers
{
    public static class ScoreViewModelComparer
    {
        public static void AssertAreEqual(ScoreViewModel expected, ScoreViewModel actual, string messagePrefix = "")
        {
            expected.Home.Should().Be(expected.Home, $"{messagePrefix}Home score should be equal.");
            expected.Away.Should().Be(expected.Away, $"{messagePrefix}Away score should be equal.");
            expected.IsTechnicalDefeat.Should().Be(expected.IsTechnicalDefeat,
                $"{messagePrefix}IsTechnicalDefeat should be equal.");
        }
    }
}