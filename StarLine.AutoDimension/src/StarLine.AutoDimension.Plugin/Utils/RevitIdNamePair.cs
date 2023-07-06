using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class RevitIdNamePair : IdNamePair
    {
        public RevitIdNamePair(Element element) : base(element.Id.IntegerValue, element.Name)
        {
        }

        public RevitIdNamePair(IdNamePair pair) : base(pair.Id, pair.Name)
        {
        }

        public ElementId GetElementId()
        {
            return new ElementId(Id);
        }

        public Element GetElement(Document doc)
        {
            return doc.GetElement(GetElementId());
        }
    }
}
