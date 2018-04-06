using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VolleyManagement.UI.Areas.Admin.Models;

namespace VolleyManagement.UnitTests.Admin.Comparers
{
    /// <summary>
    ///     Compares message instances
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MessageViewModelComparer : IComparer<MessageViewModel>, IComparer
    {
        public int Compare(object x, object y)
        {
            var firstMessage = x as MessageViewModel;
            var secondMessage = y as MessageViewModel;

            if (firstMessage == null)
            {
                return -1;
            }

            if (secondMessage == null)
            {
                return 1;
            }

            return Compare(firstMessage, secondMessage);
        }

        public int Compare(MessageViewModel x, MessageViewModel y)
        {
            return AreEqual(x, y) ? 0 : 1;
        }

        private bool AreEqual(MessageViewModel x, MessageViewModel y)
        {
            return x.Id == y.Id &&
                   x.Message == y.Message;
        }
    }
}