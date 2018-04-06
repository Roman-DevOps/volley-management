using System.Collections;
using System.Collections.Generic;
using FluentAssertions;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Division;

namespace VolleyManagement.UnitTests.Mvc.ViewModels
{
    internal class GroupViewModelComparer : IComparer, IComparer<GroupViewModel>
    {
        public int Compare(object x, object y)
        {
            return Compare(x as GroupViewModel, y as GroupViewModel);
        }

        public int Compare(GroupViewModel x, GroupViewModel y)
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

        public int CompareInternal(GroupViewModel x, GroupViewModel y)
        {
            x.Id.Should().Be(y.Id, "Id does not match");
            x.Name.Should().Be(y.Name, "Name does not match");

            return 0;
        }
    }
}