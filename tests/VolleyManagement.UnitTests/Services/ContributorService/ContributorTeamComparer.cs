﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VolleyManagement.Domain.ContributorsAggregate;

namespace VolleyManagement.UnitTests.Services.ContributorService
{
    /// <summary>
    ///     Comparer for contributor team objects.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class ContributorTeamComparer : IComparer<ContributorTeam>, IComparer
    {
        /// <summary>
        ///     Compares two contributors team objects (non-generic implementation).
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of contributors.</returns>
        public int Compare(object x, object y)
        {
            var firstTeamContributor = x as ContributorTeam;
            var secondTeamContributor = y as ContributorTeam;

            if (firstTeamContributor == null)
            {
                return -1;
            }

            if (secondTeamContributor == null)
            {
                return 1;
            }

            return Compare(firstTeamContributor, secondTeamContributor);
        }

        /// <summary>
        ///     Compares two contributor team objects.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of contributors team.</returns>
        public int Compare(ContributorTeam x, ContributorTeam y)
        {
            return AreEqual(x, y) ? 0 : 1;
        }

        /// <summary>
        ///     Finds out whether two contributors team objects have the same properties.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>True if given contributors team have the same properties.</returns>
        public bool AreEqual(ContributorTeam x, ContributorTeam y)
        {
            return x.Id == y.Id &&
                   x.Name == y.Name &&
                   x.CourseDirection == y.CourseDirection &&
                   x.Contributors.SequenceEqual(y.Contributors, new ContributorEqualityComparer());
        }
    }
}