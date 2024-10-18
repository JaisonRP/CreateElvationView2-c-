using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Core.Domain
{
    public class WallPanel : WallPanelBase
    {
        public WallPanel(int id) : base(id)
        {
            References = new List<WallReference>();
        }

        public IList<WallReference> References { get; }

        public string FamilyName { get; set; }

        public double RoHeight { get; set; }
        

        public void AddReference(WallReference reference)
        {
            References.Add(reference);
        }

        public WallReference GetByType(Direction direction, RefType refType)
        {
            return References.FirstOrDefault(x =>
                x.Direction == direction && x.ReferenceRepresentation.RefType == refType);
        }

        public IEnumerable<WallReference> GetAllByType(Direction direction, RefType refType)
        {
            return References.Where(x =>
                x.Direction == direction && x.ReferenceRepresentation.RefType == refType);
        }

        public IList<DimLine> GetDimensionPair(Direction direction)
        {
            var lines = new List<DimLine>();
            var queryable = References.AsQueryable().Where(x => x.Direction == direction);
            // Check if the direction is horizontal
            if (direction == Direction.Horizontal)  
            {
                // Get references for various components
                var rt = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.RoughTop);
                var rb = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.RoughBottom);
                var hb = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelBottom);
                var ht = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelTop);
                var bars = queryable.Where(x => x.ReferenceRepresentation.RefType == RefType.ClBar).ToList();

                if (IsByPass) // Main outside
                {

                    // Create dimension line for tbar dimensions
                    if (bars.Count > 0)
                    {
                        var barDimLine = new DimLine();
                        barDimLine.AddReference(ht);
                        foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                        {
                            barDimLine.AddReference(wallReference);
                        }

                        barDimLine.AddReference(hb);
                        if (barDimLine.IsDrawable)
                        {
                            lines.Add(barDimLine);
                        }
                    }

                    // Create dimension line for HD dimensions
                    var heelDimLine = new DimLine();
                    heelDimLine.AddReference(rt);
                    heelDimLine.AddReference(ht);
                    heelDimLine.AddReference(hb);
                    heelDimLine.AddReference(rb);
                    if (heelDimLine.IsDrawable)
                    {
                        if (heelDimLine.References.Count == 4)
                        {
                            heelDimLine.AddSegment(new DimSegment(1, string.Empty, "H.D."));
                        }

                        lines.Add(heelDimLine);
                    }

                    // Create dimension line for RO dimensions
                    var outDimLine = new DimLine();
                    outDimLine.AddReference(rt);
                    outDimLine.AddReference(rb);
                    if (outDimLine.IsDrawable)
                    {
                        outDimLine.AddSegment(new DimSegment(0, string.Empty, "R.O. (Outside Dimension)"));
                        lines.Add(outDimLine);
                    }
                }
                else if (IsDoor) // Door
                {
                    {
                        var dimLine = new DimLine();
                        dimLine.AddReference(rt);
                        dimLine.AddReference(rb);
                        if (dimLine.IsDrawable)
                        {
                            dimLine.AddSegment(new DimSegment(0, string.Empty, "DOOR R.O."));
                            lines.Add(dimLine);
                        }
                    }

                    {
                        var dimLine = new DimLine();
                        dimLine.AddReference(rt);
                        dimLine.AddReference(ht);
                        dimLine.AddReference(hb);
                        dimLine.AddReference(rb);
                        if (dimLine.IsDrawable)
                        {
                            if (dimLine.References.Count == 4)
                            {
                                dimLine.AddSegment(new DimSegment(1, string.Empty, "DOOR H.D."));
                            }

                            lines.Add(dimLine);
                        }

                        if (bars.Count > 0)
                        {
                            var barDimLine = new DimLine();
                            barDimLine.AddReference(ht);
                            foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                            {
                                barDimLine.AddReference(wallReference);
                            }

                            barDimLine.AddReference(hb);
                            if (barDimLine.IsDrawable)
                            {
                                lines.Add(barDimLine);
                            }
                        }
                    }
                }
                else // Main inside
                {
                    if (bars.Count > 0)
                    {
                        var barDimLine = new DimLine();
                        barDimLine.AddReference(ht);
                        foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                        {
                            barDimLine.AddReference(wallReference);
                        }

                        barDimLine.AddReference(hb);
                        if (barDimLine.IsDrawable)
                        {
                            lines.Add(barDimLine);
                        }
                    }

                    var heelDimLine = new DimLine();
                    heelDimLine.AddReference(rt);
                    heelDimLine.AddReference(ht);
                    heelDimLine.AddReference(hb);
                    heelDimLine.AddReference(rb);
                    if (heelDimLine.IsDrawable)
                    {
                        if (heelDimLine.References.Count == 4)
                        {
                            heelDimLine.AddSegment(new DimSegment(1, string.Empty, "INT. H.D."));
                        }

                        lines.Add(heelDimLine);
                    }

                    var outDimLine = new DimLine();
                    outDimLine.AddReference(rt);
                    outDimLine.AddReference(rb);
                    if (outDimLine.IsDrawable)
                    {
                        outDimLine.AddSegment(new DimSegment(0, string.Empty, "R.O. (Inside Dimension)"));
                        lines.Add(outDimLine);
                    }

                    
                }
            }
            // Check if the direction is Vertical
            else
            {
                var hl = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelLeft);
                var hr = queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelRight);
                var bars = queryable.Where(x => x.ReferenceRepresentation.RefType == RefType.ClBarVertical).ToList();
                if (hl != null && hr != null)
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

            // Annotation: The following block of code is intended to append the FrameTag to the Prefix property
            // of each DimSegment within each DimLine in the lines collection. It can be uncommented if the
            // FrameTag information needs to be included in the segment prefixes.

            // Uncomment the block below to append FrameTag to the Prefix property of each DimSegment
            /*
            foreach (var dimLine in lines)
            {
                foreach (var dimLineSegment in dimLine.Segments)
                {
                    // Append FrameTag to the Prefix property of each DimSegment
                    dimLineSegment.Prefix += $" ({FrameTag})";
                }
            }
            */


            return lines;
        }

        public void AppendToDim(DimLine dimLine, Direction direction)
        {
            var queryable = References.AsQueryable().Where(x => x.Direction == direction);
            if (direction == Direction.Vertical)
            {
                var bars = queryable.Where(x => x.ReferenceRepresentation.RefType == RefType.ClBarVertical).ToList();
                foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                {
                    dimLine.AddReference(wallReference);
                }

                dimLine.AddReference(queryable.FirstOrDefault(x => x.ReferenceRepresentation.RefType == RefType.HeelRight));
            }
        }
    }
}
