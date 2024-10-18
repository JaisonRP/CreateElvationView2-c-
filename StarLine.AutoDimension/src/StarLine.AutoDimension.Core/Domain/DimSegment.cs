using Tortuga.Anchor.Modeling;

namespace StarLine.AutoDimension.Core.Domain
{
    public class DimSegment
    {
        public DimSegment(int index, string prefix, string suffix) //double value
        {
            Index = index;
            Prefix = prefix;
            Suffix = suffix;
            //Value = value;
        }

        public int Index { get; }
        //public double Value { get; }

        public string Prefix { get; }

        public string Suffix { get; }
    }
}
