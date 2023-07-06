using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Core.Domain
{
    public class Options
    {
        public IList<ReferenceRepresentation> VerticalReferences { get; set; } = new List<ReferenceRepresentation>();

        public IList<ReferenceRepresentation> HorizontalReferences { get; set; } = new List<ReferenceRepresentation>();

        public IdNamePair CurtainPanelTag { get; set; }

        public IdNamePair CurtainPanelTagDoors { get; set; }

        public IdNamePair MaterialTag { get; set; }

        public IdNamePair GenericTag { get; set; }

        public IdNamePair GenericAnnotation { get; set; }

        public IdNamePair DimensionStyle { get; set; }

        public bool SuppressCornerPostTag { get; set; }

        public bool HorizontalDimensionBottom { get; set; }

        public bool AutoAlignDimension { get; set; }

        public double CurtainTagOffset { get; set; }

        public double DoorTagOffset { get; set; }

        public double MaterialTagOffset { get; set; }

        public double CornerPostTagOffset { get; set; }

        public double MoveSegmentsLessThan { get; set; }

        public double MoveSegmentsBy { get; set; }

        public double StackSegmentsCloserThan { get; set; }

        public double StackingDistance { get; set; }

        public double DimLinesDistance { get; set; }

        public double FirstDimLineHorizontal { get; set; }

        public double FirstDimLineVertical { get; set; }

        public WallReference FindReference(ReferencePair pair)
        {
            var reference = HorizontalReferences.FirstOrDefault(x => x.PlaneName == pair.Name);
            if (reference != null)
            {
                return new WallReference(pair.Id, reference, Direction.Horizontal, pair.ReferenceRepresentation);
            }

            reference = VerticalReferences.FirstOrDefault(x => x.PlaneName == pair.Name);
            if (reference != null)
            {
                return new WallReference(pair.Id, reference, Direction.Vertical, pair.ReferenceRepresentation);
            }

            return null;
        }
    }
}
