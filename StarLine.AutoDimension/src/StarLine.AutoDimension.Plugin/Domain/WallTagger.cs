using System.Linq;
using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;
using StarLine.AutoDimension.Plugin.Utils;
using Options = StarLine.AutoDimension.Core.Domain.Options;

namespace StarLine.AutoDimension.Plugin.Domain
{
    internal class WallTagger
    {
        private readonly Document _currentDocument;
        private readonly Wall _wall;
        private readonly SelectionResult _selectionResult;
        private readonly TagChecker _tagChecker;
        private readonly RevitFilter _filter;

        public WallTagger(Document currentDocument, Wall wall,
            SelectionResult selectionResult)
        {
            _currentDocument = currentDocument;
            _wall = wall;
            _selectionResult = selectionResult;
            _filter = new RevitFilter(currentDocument);
            _tagChecker = new TagChecker(currentDocument);
        }

        public void Tag(Options options)
        {
            if (_wall.CurtainGrid != null)
            {
                TagWall();
                var doc = _wall.Document;
                foreach (var elementId in _wall.CurtainGrid.GetPanelIds().Where(x => !_tagChecker.IsTagged(x)))
                {
                    var element = doc.GetElement(elementId);
                    if (element is Panel panel)
                    {
                        TagPanel(panel, options);
                        foreach (var subComponentId in panel.GetSubComponentIds().Where(x => !_tagChecker.IsTagged(x)))
                        {
                            var subElement = doc.GetElement(subComponentId);
                            if (subElement is FamilyInstance fs)
                            {
                                if (subElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Doors)
                                {
                                    TagDoor(fs, options);
                                }
                                else if (subElement.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows)
                                {
                                    TagDoor(fs, options);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TagDoor(FamilyInstance fs, Options options)
        {
            var reference = new Reference(fs);
            if (_selectionResult.RevitLinkInstance != null)
            {
                reference = reference.CreateLinkReference(_selectionResult.RevitLinkInstance);
            }

            if (fs.Location is LocationPoint locationPoint)
            {
                var types = _filter.GetDoorTagSymbols();
                var tagType = IdNamePair.GetByIdOrName(types, options.CurtainPanelTagDoors);
                if (tagType != null)
                {
                    var tagLocation = locationPoint.Point + options.CurtainTagOffset * XYZ.BasisY;
                    var tag = IndependentTag.Create(_currentDocument, _currentDocument.ActiveView.Id, reference, true,
                        TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagLocation);
                    tag.ChangeTypeId(new ElementId(tagType.Id));
                }

                var materialTagType = GetMaterialType(options);
                if (materialTagType != null)
                {
                    var materialTagLocation = locationPoint.Point + options.MaterialTagOffset * XYZ.BasisY;
                    var tag = IndependentTag.Create(_currentDocument, _currentDocument.ActiveView.Id, reference, true,
                        TagMode.TM_ADDBY_MATERIAL, TagOrientation.Horizontal, materialTagLocation);
                    tag.ChangeTypeId(materialTagType.GetElementId());
                }
            }
        }

        private void TagPanel(Panel panel, Options options)
        {
            var reference = new Reference(panel);
            if (_selectionResult.RevitLinkInstance != null)
            {
                reference = reference.CreateLinkReference(_selectionResult.RevitLinkInstance);
            }

            var types = _filter.GetPanelTagSymbols();
            var tagType = IdNamePair.GetByIdOrName(types, options.CurtainPanelTag);
            if (tagType != null)
            {
                var tagLocation = panel.Transform.Origin + options.CurtainTagOffset * XYZ.BasisY;
                var tag = IndependentTag.Create(_currentDocument, _currentDocument.ActiveView.Id, reference, true,
                        TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, tagLocation);
                tag.ChangeTypeId(new ElementId(tagType.Id));
            }

            var materialTagType = GetMaterialType(options);
            if (materialTagType != null)
            {
                var materialTagLocation = panel.Transform.Origin + options.MaterialTagOffset * XYZ.BasisY;
                var tag = IndependentTag.Create(_currentDocument, _currentDocument.ActiveView.Id, reference, true,
                            TagMode.TM_ADDBY_MATERIAL, TagOrientation.Horizontal, materialTagLocation);
                tag.ChangeTypeId(materialTagType.GetElementId());
            }
        }

        private void TagWall()
        {
            var reference = new Reference(_wall);
            if (_selectionResult.RevitLinkInstance != null)
            {
                reference = reference.CreateLinkReference(_selectionResult.RevitLinkInstance);
            }

            if (_wall.Location is LocationCurve location)
            {
                var tag = IndependentTag.Create(_currentDocument, _currentDocument.ActiveView.Id, reference, true,
                    TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, location.Curve.Evaluate(0.5, true));
            }
        }

        private RevitIdNamePair GetMaterialType(Options options)
        {
            var types = _filter.GetMaterialTagSymbols();
            var tagType = IdNamePair.GetByIdOrName(types, options.MaterialTag);
            return new RevitIdNamePair(tagType);
        }
    }
}
