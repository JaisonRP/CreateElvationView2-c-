using StarLine.AutoDimension.Core.Utils;

namespace StarLine.AutoDimension.Core.Domain
{
    public class ReferenceRepresentation
    {
        public ReferenceRepresentation(RefType refType)
        {
            RefType = refType;
        }

        public RefType RefType { get; }

        public string TypeName { get; set; }

        public string PlaneName { get; set; }

        public int ExtractIndex()
        {
            return Extractor.ExtractIndex(TypeName);
        }
    }
}
