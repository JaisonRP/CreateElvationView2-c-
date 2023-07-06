using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Exceptions;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class RevitSelector
    {
        private readonly UIDocument _uiDocument;

        public RevitSelector(UIDocument uiDocument)
        {
            _uiDocument = uiDocument;
        }

        public ElementId Select()
        {
            try
            {
                var reference =
                    _uiDocument.Selection.PickObject(ObjectType.PointOnElement, new PanelFilter(_uiDocument.Document));
                return reference.LinkedElementId == ElementId.InvalidElementId
                    ? reference.ElementId
                    : reference.LinkedElementId;
            }
            catch (OperationCanceledException)
            {
                return ElementId.InvalidElementId;
            }
        }

        public SelectionResult GetCurrentSelection()
        {
            try
            {
                var id = _uiDocument.Selection.GetElementIds().FirstOrDefault();
                if (id != null)
                {
                    if (_uiDocument.Document.GetElement(id) is Panel panel)
                    {
                        return new SelectionResult(_uiDocument.Document, panel.Host.Id);
                    }

                    if (_uiDocument.Document.GetElement(id) is Wall)
                    {
                        return new SelectionResult(_uiDocument.Document, id);
                    }

                    if (_uiDocument.Document.GetElement(id) is RevitLinkInstance revitLinkInstance)
                    {
                        var reference = _uiDocument.Selection.PickObject(ObjectType.LinkedElement, new LinkedWallFilter(), 
                            "Please select a wall");
                        var docLink = revitLinkInstance.GetLinkDocument();
                        return new SelectionResult(docLink, reference.LinkedElementId, revitLinkInstance);
                    }
                }

                return new SelectionResult(_uiDocument.Document, ElementId.InvalidElementId);
            }
            catch (OperationCanceledException)
            {
                return new SelectionResult(_uiDocument.Document, ElementId.InvalidElementId);
            }
        }

        public ICollection<ElementId> CollectGrid()
        {
            var collector = new FilteredElementCollector(_uiDocument.Document, _uiDocument.Document.ActiveView.Id)
                .WhereElementIsNotElementType().OfCategory(BuiltInCategory.OST_Grids);
            return collector.ToElementIds();
        }
    }
}
