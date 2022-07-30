using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Bancey.Web
{
    public class DocumentComparer : IComparer<IDocument>
    {
        public int Compare(IDocument x, IDocument y)
        {
            var xOrder = (int)x.Get("order");
            var yOrder = (int)y.Get("order");
            return xOrder.CompareTo(yOrder);
        }
    }
}