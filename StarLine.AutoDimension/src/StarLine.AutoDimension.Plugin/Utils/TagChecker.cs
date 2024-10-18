using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class TagChecker
    {
        private readonly Document _currentDocument;
        private readonly HashSet<int> _taggedIds = new HashSet<int>();
        private readonly HashSet<int> _taggedMaterialIds = new HashSet<int>();
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

        public bool IsMaterialTagged(FamilyInstance fi)
        {
            Init();
            return _taggedMaterialIds.Contains(fi.Id.IntegerValue) ||
                   fi.GetSubComponentIds().Any(x => _taggedMaterialIds.Contains(x.IntegerValue));
        }

        private void Init()
        {
            if (_isInitialized)
            {
                return;
            }

            var tags = new FilteredElementCollector(_currentDocument, _currentDocument.ActiveView.Id)
                .OfClass(typeof(IndependentTag)).OfType<IndependentTag>();

            foreach (var tag in tags)
            {
                if (tag.IsMaterialTag)
                {
                    AddToList(tag, _taggedMaterialIds);
                }
                else if (!tag.IsMulticategoryTag)
                {
                    AddToList(tag, _taggedIds);
                }
            }

            _isInitialized = true; // check this list , tags being duplicated
        }

        private static void AddToList(IndependentTag tag, ISet<int> set)
        {
            foreach (var linkElementId in tag.GetTaggedElementIds())
            {
                set.Add(linkElementId.HostElementId != ElementId.InvalidElementId
                    ? linkElementId.HostElementId.IntegerValue
                    : linkElementId.LinkedElementId.IntegerValue);
            }
        }
    }
}
