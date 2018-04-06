using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VolleyManagement.UI.Areas.Mvc.ViewModels.GameResults;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Tournaments;

namespace VolleyManagement.UnitTests.Mvc.ViewModels
{
    /// <summary>
    ///     Comparer for schedule objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class ScheduleViewModelComparer : IComparer<ScheduleViewModel>, IComparer
    {
        /// <summary>
        ///     Compares two schedules objects (non-generic implementation).
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of schedules.</returns>
        public int Compare(object x, object y)
        {
            var firstSchedule = x as ScheduleViewModel;
            var secondSchedule = y as ScheduleViewModel;

            if (firstSchedule == null)
            {
                return -1;
            }

            if (secondSchedule == null)
            {
                return 1;
            }

            return Compare(firstSchedule, secondSchedule);
        }

        /// <summary>
        ///     Compares two schedules objects.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of schedules.</returns>
        public int Compare(ScheduleViewModel x, ScheduleViewModel y)
        {
            return AreEqual(x, y) ? 0 : 1;
        }

        /// <summary>
        ///     Finds out whether two schedules objects have the same properties.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if given schedules have the same properties.</returns>
        public bool AreEqual(ScheduleViewModel x, ScheduleViewModel y)
        {
            return x.TournamentId == y.TournamentId &&
                   x.TournamentName == y.TournamentName &&
                   x.MaxNumberOfRounds == y.MaxNumberOfRounds &&
                   AreRoundsEqual(x.Rounds, y.Rounds);
        }

        /// <summary>
        ///     Finds out whether two rounds objects have the same properties.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if given rounds have the same properties.</returns>
        public bool AreRoundsEqual(
            Dictionary<byte, List<GameResultViewModel>> x,
            Dictionary<byte, List<GameResultViewModel>> y)
        {
            {
                if (x.Keys.Count != y.Keys.Count)
                {
                    return false;
                }

                for (var i = 0; i < x.Count; i++)
                {
                    if (!x[x.Keys.ElementAt(i)].SequenceEqual(y[y.Keys.ElementAt(i)],
                        new GameResultViewModelEqualityComparer()))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}