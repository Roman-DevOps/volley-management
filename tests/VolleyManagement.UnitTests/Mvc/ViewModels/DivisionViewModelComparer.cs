using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using VolleyManagement.UI.Areas.Mvc.ViewModels.Division;

namespace VolleyManagement.UnitTests.Mvc.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class DivisionViewModelComparer : IComparer, IComparer<DivisionViewModel>
    {
        public int Compare(object x, object y)
        {
            return Compare(x as DivisionViewModel, y as DivisionViewModel);
        }

        public int Compare(DivisionViewModel x, DivisionViewModel y)
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

        public int CompareInternal(DivisionViewModel x, DivisionViewModel y)
        {
            x.Id.Should().Be(y.Id, "Id does not match");
            x.Name.Should().Be(y.Name, "Name does not match");

            TestHelper.AreEqual(x.Groups, y.Groups, new GroupViewModelComparer());
            return 0;
        }
    }
}