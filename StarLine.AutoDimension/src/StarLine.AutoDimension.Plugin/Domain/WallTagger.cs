using System.Collections.Generic;
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
        private readonly IList<PanelTagger> _panels = new List<PanelTagger>();

        public WallTagger(Document currentDocument, Wall wall,
            SelectionResult selectionResult)
        {
            _currentDocument = currentDocument;
            _wall = wall;
            _selectionResult = selectionResult;
            _filter = new RevitFilter(currentDocument);
            _tagChecker = new TagChecker(currentDocument);

            if (_wall.CurtainGrid != null)
            {
                var doc = _wall.Document;
                foreach (var elementId in _wall.CurtainGrid.GetPanelIds())
                {
                    var element = doc.GetElement(elementId);
                    if (element is Panel panel)
                    {
                        _panels.Add(new PanelTagger(panel.Id.IntegerValue, panel,
                            _currentDocument, _selectionResult, _tagChecker)
                        {
                            Series = panel.LookupParameter("Series")?.AsString() ?? string.Empty
                        });
                    }
                }
            }
        }

        public void Tag(Options options)
        {
            // Tag Panels
            var curtainPanelTagTypes = _filter.GetPanelTagSymbols();
            var curtainPanelTagType = IdNamePair.GetByIdOrName(curtainPanelTagTypes, options.CurtainPanelTag);
            if (curtainPanelTagType != null)
            {
                var typeId = new ElementId(curtainPanelTagType.Id);
                foreach (var panelTagger in _panels)
                {
                    panelTagger.TagPanel(typeId, options.CurtainTagOffset);
                }
            }

            // Tag Doors
            var doorTagTypes = _filter.GetDoorTagSymbols();
            var doorTagType = IdNamePair.GetByIdOrName(doorTagTypes, options.CurtainPanelTagDoors);
            if (doorTagType != null)
            {
                var typeId = new ElementId(doorTagType.Id);
                foreach (var panelTagger in _panels)
                {
                    panelTagger.TagDoor(typeId, options.DoorTagOffset);
                }
            }

            // Tag Post corner
            if (!options.SuppressCornerPostTag)
            {
                var genericTagTypes = _filter.GetGenericModelTagSymbols();
                var genericTagType = IdNamePair.GetByIdOrName(genericTagTypes, options.GenericTag);
                if (genericTagType != null)
                {
                    var typeId = new ElementId(genericTagType.Id);
                    foreach (var panelTagger in _panels)
                    {
                        panelTagger.TagCorner(typeId, options.CornerPostTagOffset);
                    }
                }
            }

            // Tag Generic
            if (!options.SuppressGenericTag)
            {
                var genericTagTypes = _filter.GetGenericModelTagSymbols();
                var genericTagType = IdNamePair.GetByIdOrName(genericTagTypes, options.GenericTag2);
                if (genericTagType != null)
                {
                    var typeId = new ElementId(genericTagType.Id);
                    foreach (var panelTagger in _panels)
                    {
                        panelTagger.TagGeneric(typeId, options.CornerPostTagOffset);
                    }
                }
            }

            // Tag material
            var materialTagTypes = _filter.GetMaterialTagSymbols();
            var materialTagType = IdNamePair.GetByIdOrName(materialTagTypes, options.MaterialTag);
            if (materialTagType != null)
            {
                var typeId = new ElementId(materialTagType.Id);
                foreach (var panelTagger in _panels)
                {
                    panelTagger.TagMaterial(typeId, options.MaterialTagOffset);
                }
            }

            // Tag annotation
            if (!options.SuppressAnnotation)
            {
                var annotationSymbols = _filter.GetAnnotationSymbols();
                var annotationType = IdNamePair.GetByIdOrName(annotationSymbols, options.GenericAnnotation);
                if (annotationType != null)
                {
                    var typeId = new ElementId(annotationType.Id);
                    foreach (var panelTagger in _panels)
                    {
                        panelTagger.TagAnnotation(typeId);
                    }
                }
            }
        }
        private void MoveTagsToLowestZ(List<IndependentTag> tags)
        {
            if (tags.Count > 0)
            {
                // Find the tag with the lowest Z position
                var lowestZTag = tags.OrderBy(tag => tag.TagHeadPosition.Z).FirstOrDefault();

                // Move all tags to have the same Z position
                var lowestZ = lowestZTag.TagHeadPosition.Z;
                foreach (var tag in tags)
                {
                    var newPosition = new XYZ(tag.TagHeadPosition.X, tag.TagHeadPosition.Y, lowestZ);
                    tag.TagHeadPosition = newPosition;
                }
            }
        }
        private void TagWall()
        {
            if (!_tagChecker.IsTagged(_wall.Id))
            {
                var reference = new Reference(_wall);
                if (_selectionResult.RevitLinkInstance != null)
                {
                    reference = reference.CreateLinkReference(_selectionResult.RevitLinkInstance);
                }

                if (_wall.Location is LocationCurve location)
                {
                    IndependentTag.Create(_currentDocument, _currentDocument.ActiveView.Id, reference, true,
                        TagMode.TM_ADDBY_CATEGORY, TagOrientation.Horizontal, location.Curve.Evaluate(0.5, true));
                } 
            }
        }
    }
}
