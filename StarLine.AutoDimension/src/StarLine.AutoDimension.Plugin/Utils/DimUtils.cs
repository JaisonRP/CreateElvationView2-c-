using System;
using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;
using System.Linq;
using StarLine.AutoDimension.Plugin.Domain;
using View = Autodesk.Revit.DB.View;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class DimUtils
    {
        private readonly Document _currentDocument;

        public DimUtils(Document currentDocument)
        {
            _currentDocument = currentDocument;
        }

        public void AddDimensions(WallWrapper wallWrapper, Core.Domain.Options options)
        {
            var view = _currentDocument.ActiveView;
            var height = wallWrapper.WallHeight;
            var dimType = GetDimType(options);
            var horizontalLines = wallWrapper.GetDimensionPair(Direction.Horizontal);
            var horizontalBaseline = wallWrapper.GetBaseLine(Direction.Horizontal);
            horizontalBaseline =
                (Line)horizontalBaseline.CreateTransformed(
                    Transform.CreateTranslation(options.FirstDimLineVertical * view.RightDirection));
            var i = 0;
            foreach (var dimLine in horizontalLines.Reverse())
            {
                var line = (Line)horizontalBaseline.CreateTransformed(
                    Transform.CreateTranslation(i++ * options.DimLinesDistance * view.RightDirection));
                var dim = PlaceDimension(dimLine, view, line, dimType);
                if (dim != null)
                {
                    AddPrefixSuffix(dimLine, dim);
                    MoveDim(dim, options);
                }
            }

            var verticalLines = wallWrapper.GetDimensionPair(Direction.Vertical);
            var verticalBaseline = wallWrapper.GetBaseLine(Direction.Vertical);
            
            var dimDir = view.UpDirection.Negate();
            if (!options.HorizontalDimensionBottom)
            {
                verticalBaseline =
                    (Line)verticalBaseline.CreateTransformed(Transform.CreateTranslation(height * view.UpDirection));
                dimDir = view.UpDirection;
            }

            verticalBaseline =
                (Line)verticalBaseline.CreateTransformed(
                    Transform.CreateTranslation(options.FirstDimLineHorizontal * dimDir));
            i = -1;
            var lastGroupId = string.Empty;
            foreach (var dimLine in verticalLines.Reverse())
            {
                if (verticalBaseline != null)
                {
                    if (!string.Equals(dimLine.GroupId, lastGroupId))
                    {
                        i++;
                        lastGroupId = dimLine.GroupId;
                    }

                    var line = (Line)verticalBaseline.CreateTransformed(
                        Transform.CreateTranslation(i * options.DimLinesDistance * dimDir));
                    var dim = PlaceDimension(dimLine, view, line, dimType);
                    if (dim != null)
                    {
                        AddPrefixSuffix(dimLine, dim);
                        MoveDim(dim, options);
                    }
                }
            }
        }

        private Dimension PlaceDimension(DimLine dimLine, View view, Line line, DimensionType dimType)
        {
            var ra = new ReferenceArray();
            foreach (var wallReference in dimLine.References)
            {
                var reference = Reference.ParseFromStableRepresentation(_currentDocument, wallReference.StringRepresentation);
                ra.Append(reference);
            }


            if (ra.Size >= 2)
            {
                var dim = _currentDocument.Create.NewDimension(view, line, ra, dimType);
                return dim;
            }

            return null;
        }

        private static void AddPrefixSuffix(DimLine dimLine, Dimension dim)
        {
            foreach (var segment in dimLine.Segments)
            {
                if (dim.Segments.Size > segment.Index)
                {
                    dim.Segments.get_Item(segment.Index).Prefix = segment.Prefix;
                    dim.Segments.get_Item(segment.Index).Suffix = segment.Suffix;
                }
            }
        }

        private static void MoveDim(Dimension dim, Core.Domain.Options options)
        {
            if (dim.Curve is Line line)
            {
                var view = dim.Document.ActiveView;
                foreach (DimensionSegment dimSegment in dim.Segments)
                {
                    if (dimSegment.Value < options.MoveSegmentsLessThan && dimSegment.IsTextPositionAdjustable())
                    {
                        var transform = Transform.CreateRotationAtPoint(view.ViewDirection, 45 * Math.PI / 180,
                            dimSegment.TextPosition);
                        var rotatedVec = transform.OfVector(line.Direction);
                        dimSegment.TextPosition = dimSegment.TextPosition.Add(options.MoveSegmentsBy * rotatedVec);
                    }
                }
            }
        }

        private DimensionType GetDimType(Core.Domain.Options options)
        {
            var filter = new RevitFilter(_currentDocument);
            var dimTypes = filter.GetDimensionStyles(DimensionStyleType.Linear);
            var dimTypePair = IdNamePair.GetByIdOrName(dimTypes, options.DimensionStyle);
            var dimType = dimTypePair != null
                ? _currentDocument.GetElement(new ElementId(dimTypePair.Id)) as DimensionType
                : _currentDocument.GetElement(_currentDocument.GetDefaultElementTypeId(ElementTypeGroup.LinearDimensionType)) as DimensionType;
            return dimType;
        }
    }
}
