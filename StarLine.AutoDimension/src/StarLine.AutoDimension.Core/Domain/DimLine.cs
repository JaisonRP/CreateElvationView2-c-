using System;
using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Core.Domain
{
    public class DimLine
    {
        public DimLine()
        {
            Segments = new List<DimSegment>();
            References = new List<WallReference>();
        }

        public IList<DimSegment> Segments { get; }

        public IList<WallReference> References { get; }

        public string GroupId { get; set; } = Guid.NewGuid().ToString();

        public void AddReference(WallReference reference)
        {
            if (reference != null)
            {
                References.Add(reference);
            }
        }

        public void AddSegment(DimSegment segment)
        {
            if (segment.Index > References.Count - 2)
            {
                throw new Exception($"There are {References.Count} references which gives {References.Count - 1} segments");
            }

            var existingSeg = Segments.FirstOrDefault(x => x.Index == segment.Index);
            if (existingSeg != null)
            {
                Segments.Remove(existingSeg);
            }

            Segments.Add(segment);
        }
    }
}
