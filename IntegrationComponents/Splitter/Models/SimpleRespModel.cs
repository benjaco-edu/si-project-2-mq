using System;
using System.Diagnostics.CodeAnalysis;

namespace Splitter
{
    public class SimpleRespModel : IComparable<SimpleRespModel>
    {
        public int id { get; set; }
        public string amount { get; set; }
        public string stock { get; set; }
        public string broker { get; set; }
        public double totalPrice { get; set; }


        public int CompareTo([AllowNull] SimpleRespModel other)
        {
            if (other == null)
                return 1;
            return this.totalPrice.CompareTo(other.totalPrice);
        }
    }
}