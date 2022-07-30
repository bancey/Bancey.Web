using System;
using System.Collections.Generic;
using Statiq.Common;

namespace Bancey.Web
{
    public class DocumentComparer : IComparer<IDocument>
    {
        public int Compare(IDocument x, IDocument y)
        {
            Console.WriteLine($"x: {x.Get("name")} y: {y.Get("name")}");
            var xOrder = (int)x.Get("order");
            var yOrder = (int)y.Get("order");
            return xOrder.CompareTo(yOrder);
        }
    }
}