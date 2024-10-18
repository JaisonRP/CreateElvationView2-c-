using Autodesk.Revit.DB;
using System;

namespace StarLine.AutoDimension.Plugin.Domain
{
    internal class LevelWrapper
    {
        private readonly Document _currentDocument;
        private readonly Level _level;

        public LevelWrapper(ElementId id, Document currentDocument)
        {
            _currentDocument = currentDocument;
            if (_currentDocument.GetElement(id) is Level level)
            {
                _level = level;
            }
            else
            {
                throw new Exception("Error getting level");
            }
        }

        public void AddNote()
        {
            var typeId = _level.GetTypeId();
            if (_currentDocument.GetElement(typeId) is LevelType lt &&
                string.Equals(lt.Name, "Circle Head - Project Datum", StringComparison.InvariantCultureIgnoreCase))
            {
                var textTypeId = _currentDocument.GetDefaultElementTypeId(ElementTypeGroup.TextNoteType);
                var activeView = _currentDocument.ActiveView;
                var box = _level.get_BoundingBox(activeView);
                var insertionPoint = box.Max + (7.0 / 12) * activeView.UpDirection;
                TextNote.Create(_currentDocument, activeView.Id, insertionPoint, "T.O.S",
                    textTypeId);
            }
        }

        public string GetStringRepresentation()
        {
            var reference = _level.GetPlaneReference();
            return reference.ConvertToStableRepresentation(_currentDocument);
        }
    }
}
