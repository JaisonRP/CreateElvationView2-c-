using StarLine.AutoDimension.Core.Domain;
using StarLine.AutoDimension.Core.Utils;
using Tortuga.Anchor.Modeling;

namespace StarLine.AutoDimension.Core.ViewModels
{
    public class ReferenceModel : ModelBase
    {
        public RefType RefType
        {
            get => Get<RefType>();
            set => Set(value);
        }

        public string TypeName
        {
            get => Get<string>();
            set => Set(value);
        }

        public string PlaneName
        {
            get => Get<string>();
            set => Set(value);
        }

        public int ExtractIndex()
        {
            return Extractor.ExtractIndex(TypeName);
        }
    }
}
