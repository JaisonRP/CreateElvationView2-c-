using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class TagChecker
    {
        private readonly Document _currentDocument;
        private readonly HashSet<int> _taggedIds = new HashSet<int>();
        private bool _isInitialized;

        public TagChecker(Document currentDocument)
        {
            _currentDocument = currentDocument;
        }

        public bool IsTagged(ElementId id)
        {
            Init();
            return _taggedIds.Contains(id.IntegerValue);
        }

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            var collector = new FilteredElementCollector(_currentDocument, _currentDocument.ActiveView.Id)
                .OfClass(typeof(IndependentTag)).OfType<IndependentTag>().Select(x => x.GetTaggedElementIds());

            foreach (var item in collector)
            {
                foreach (var linkElementId in item)
                {
                    if (linkElementId.HostElementId != ElementId.InvalidElementId)
                    {
                        _taggedIds.Add(linkElementId.HostElementId.IntegerValue);
                    }
                    else
                    {
                        _taggedIds.Add(linkElementId.LinkedElementId.IntegerValue);
                    }
                }
            }

            _isInitialized = true;
        }
    }
}
