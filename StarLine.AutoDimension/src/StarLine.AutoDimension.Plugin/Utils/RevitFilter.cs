using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class RevitFilter
    {
        private readonly Document _document;

        public RevitFilter(Document document)
        {
            _document = document;
        }

        public List<RevitIdNamePair> GetDoorTagSymbols()
        {
            return GetFamilySymbols(BuiltInCategory.OST_WindowTags);
        }

        public List<RevitIdNamePair> GetPanelTagSymbols()
        {
            return GetFamilySymbols(BuiltInCategory.OST_CurtainWallPanelTags);
        }

        public List<RevitIdNamePair> GetMaterialTagSymbols()
        {
            return GetFamilySymbols(BuiltInCategory.OST_MaterialTags);
        }

        public List<RevitIdNamePair> GetGenericModelTagSymbols()
        {
            return GetFamilySymbols(BuiltInCategory.OST_GenericModelTags);
        }

        public List<RevitIdNamePair> GetAnnotationSymbols()
        {
            return GetFamilySymbols(BuiltInCategory.OST_GenericAnnotation);
        }

        public List<RevitIdNamePair> GetFamilySymbols(BuiltInCategory category)
        {
            var collector = new FilteredElementCollector(_document).OfCategory(category).OfClass(typeof(FamilySymbol));

            var symbols = collector.Select(x => new RevitIdNamePair(x)).ToList();
            return symbols;
        }

        public List<RevitIdNamePair> GetDimensionStyles(DimensionStyleType styleType)
        {
            //https://forums.autodesk.com/t5/revit-api-forum/filter-out-internal-dimension-styles/td-p/11912151
            var symbols = new FilteredElementCollector(_document)
              .OfClass(typeof(DimensionType)).Cast<DimensionType>()
              .Where(x => x.StyleType == styleType && x.Parameters.Size > 5)
              .Select(x => new RevitIdNamePair(x)).ToList();

            return symbols;
        }
    }
}
