using Autodesk.Revit.DB;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class SelectionResult
    {
        public SelectionResult(Document document, ElementId id) : this(document, id, null)
        {
        }

        public SelectionResult(Document document, ElementId id, RevitLinkInstance revitLinkInstance)
        {
            Document = document;
            Id = id;
            RevitLinkInstance = revitLinkInstance;
        }

        public Document Document { get; }

        public ElementId Id { get; }

        public RevitLinkInstance RevitLinkInstance { get; }
    }
}
