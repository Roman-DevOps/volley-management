﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VolleyManagement.UI.Areas.Admin.Models;

namespace VolleyManagement.UnitTests.Mvc.Comparers
{
    /// <summary>
    ///     Compares Role instances
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class RoleViewModelComparer : IComparer<RoleViewModel>, IComparer
    {
        public int Compare(object x, object y)
        {
            return Compare(x as RoleViewModel, y as RoleViewModel);
        }

        public int Compare(RoleViewModel x, RoleViewModel y)
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

        private int CompareInternal(RoleViewModel x, RoleViewModel y)
        {
            var result = y.Id - x.Id;
            if (result != 0)
            {
                return result;
            }

            result = string.CompareOrdinal(x.Name, y.Name);
            return result;
        }
    }
}