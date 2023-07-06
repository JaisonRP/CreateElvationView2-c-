using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class PanelFilter : ISelectionFilter
    {
        private readonly Document _doc;

        public PanelFilter(Document document)
        {
            _doc = document;
        }

        public bool AllowElement(Element elem)
        {
            return elem is RevitLinkInstance || elem is Panel;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            var elem = _doc.GetElement(reference);
            if (elem is RevitLinkInstance link)
            {
                var linkElem = link.GetLinkDocument().GetElement(reference.LinkedElementId);
                return linkElem is Panel;
            }

            return elem is Panel;
        }
    }
}
