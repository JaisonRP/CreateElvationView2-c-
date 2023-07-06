namespace StarLine.AutoDimension.Core.Domain
{
    public class DimSegment
    {
        public DimSegment(int index, string prefix, string suffix)
        {
            Index = index;
            Prefix = prefix;
            Suffix = suffix;
        }

        public int Index { get; }

        public string Prefix { get; }

        public string Suffix { get; }
    }
}
