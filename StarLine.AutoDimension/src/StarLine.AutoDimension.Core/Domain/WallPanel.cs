using System;
using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Core.Domain
{
    public class WallPanel
    {
        public WallPanel(int id)
        {
            Id = id;
            References = new List<WallReference>();
        }

        public int Id { get; }

        public bool IsByPass { get; set; }

        public string Series { get; set; }

        public bool IsDoor => !string.IsNullOrEmpty(Series) &&
                              Series.StartsWith("D", StringComparison.InvariantCultureIgnoreCase);

        public IList<WallReference> References { get; }

        public void AddReference(WallReference reference)
        {
            References.Add(reference);
        }

        public WallReference GetByType(Direction direction, RefType refType)
        {
            return References.FirstOrDefault(x =>
                x.Direction == direction && x.ReferenceRepresentation.RefType == refType);
        }

        public IList<DimLine> GetDimensionPair(Direction direction)
        {
            var lines = new List<DimLine>();
            var queryable = References.AsQueryable().Where(x => x.Direction == direction);
            if (direction == Direction.Horizontal)
            {
                var ts = !IsByPass
                    ? queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.TopSlab)
                    : queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.InteriorRoughBottom);
                var bs = !IsByPass
                    ? queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.BottomSlab)
                    : queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.InteriorHeelBottom);
                var rt = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.RoughTop);
                var rb = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.RoughBottom);
                var hb = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelBottom);
                var ht = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelTop);
                var bars = queryable.Where(x => x.ReferenceRepresentation.RefType == RefType.ClBar).ToList();

                if (ts != null && bs != null)
                {
                    var dimLine = new DimLine();
                    dimLine.AddReference(ts);
                    dimLine.AddReference(bs);
                    var prefix = IsByPass ? "Int" : string.Empty;
                    if (IsDoor)
                    {
                        prefix += " Door";
                        prefix = prefix.Trim();
                    }

                    dimLine.AddSegment(new DimSegment(0, prefix, "R.O."));
                    lines.Add(dimLine);
                }

                if (rt != null && rb != null && hb != null && ht != null)
                {
                    var dimLine = new DimLine();
                    dimLine.AddReference(ht);
                    dimLine.AddReference(rt);
                    dimLine.AddReference(rb);
                    dimLine.AddReference(hb);
                    var prefix = IsDoor ? "Door" : string.Empty;
                    dimLine.AddSegment(new DimSegment(1, prefix, "H.D."));
                    lines.Add(dimLine);
                }

                if (hb != null && ht != null && bars.Count > 0)
                {
                    var dimLine = new DimLine();
                    dimLine.AddReference(ht);
                    foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                    {
                        dimLine.AddReference(wallReference);
                    }

                    dimLine.AddReference(hb);
                    lines.Add(dimLine);
                }
            }
            else
            {
                var hl = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelLeft);
                var hr = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelRight);
                if (hl != null && hr != null)
                {
                    var dimLine = new DimLine();
                    dimLine.AddReference(hl);
                    dimLine.AddReference(hr);
                    lines.Add(dimLine);
                }

                var bars = queryable.Where(x => x.ReferenceRepresentation.RefType == RefType.ClBarVertical).ToList();
                if (hl != null && hr != null && bars.Count > 0)
                {
                    var dimLine = new DimLine();
                    dimLine.AddReference(hl);
                    foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                    {
                        dimLine.AddReference(wallReference);
                    }

                    dimLine.AddReference(hr);
                    lines.Add(dimLine);
                }
            }

            return lines;
        }
    }
}
