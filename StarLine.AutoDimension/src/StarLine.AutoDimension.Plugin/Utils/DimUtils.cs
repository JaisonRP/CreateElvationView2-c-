using System;
using System.Collections.Generic;
using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;
using System.Linq;
using System.Text;
using StarLine.AutoDimension.Plugin.Domain;
using View = Autodesk.Revit.DB.View;
using System.Windows.Controls;

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
            var offset = wallWrapper.WallOffset;
            var dimType = GetDimType(options);
            var horizontalLines = wallWrapper.GetDimensionPair(Direction.Horizontal);
            var horizontalBaseline = wallWrapper.GetBaseLine(Direction.Horizontal);
            var verticalDimDir = view.RightDirection;
            if (options.VerticalDimensionLeft)
            {
                verticalDimDir = view.RightDirection.Negate();
                horizontalBaseline =
                    (Line)horizontalBaseline.CreateTransformed(
                        Transform.CreateTranslation(wallWrapper.WallLength * verticalDimDir));
            }

            horizontalBaseline =
                (Line)horizontalBaseline.CreateTransformed(
                    Transform.CreateTranslation(options.FirstDimLineVertical * verticalDimDir));
            var existingSignatures = new List<string>();
            var i = 0;




            //horizontalLines.OrderBy(dimLine => dimLine.References.FirstOrDefault()?.ReferenceRepresentation.RefType).ToList(); // sort the lines based on referencetypr


            //update here for ordering tbars . 
            if (!options.SupressVerticalDimension)
            {
                foreach (var dimLine in horizontalLines)
                {
                    var line = (Line)horizontalBaseline.CreateTransformed(
                        Transform.CreateTranslation(i++ * options.DimLinesDistance * verticalDimDir));
                    var dim = PlaceDimension(dimLine, view, line, dimType);
                    if (dim != null)
                    {
                        AddPrefixSuffix(dimLine, dim,options);
                        var signature = GetDimSignature(dim);
                        if (existingSignatures.Contains(signature))
                        {
                            _currentDocument.Delete(dim.Id);
                            i--;
                            continue;
                        }

                        existingSignatures.Add(signature);
                        MoveDim(dim, options);
                    }
                }
            }
            var verticalLines = wallWrapper.GetDimensionPair(Direction.Vertical);
            var verticalBaseline = wallWrapper.GetBaseLine(Direction.Vertical);
            
            var horizontalDimDir = view.UpDirection.Negate();

            
            if (!options.HorizontalDimensionBottom)
            {
                verticalBaseline =
                    (Line)verticalBaseline.CreateTransformed(Transform.CreateTranslation((height + offset) * view.UpDirection));
                horizontalDimDir = view.UpDirection;
            }
            else
            {
                verticalBaseline =
                    (Line)verticalBaseline.CreateTransformed(Transform.CreateTranslation(offset * view.UpDirection));
            }

            verticalBaseline =
                (Line)verticalBaseline.CreateTransformed(
                    Transform.CreateTranslation(options.FirstDimLineHorizontal * horizontalDimDir));
            i = -1;
            var lastGroupId = string.Empty;

            if (!options.SupressHorizontalDimension) // new vs 1.5.0.6
            {
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
                            Transform.CreateTranslation(i * options.DimLinesDistance * horizontalDimDir));
                        var dim = PlaceDimension(dimLine, view, line, dimType);
                        if (dim != null)
                        {
                            foreach (var segment in dim.Segments)
                            {
                                var T= segment;
                            }
                                AddPrefixSuffix(dimLine, dim,options);
                            MoveDim(dim, options);
                        }
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

        private static void AddPrefixSuffix(DimLine dimLine, Dimension dim, Core.Domain.Options options)
        {
            foreach (var segment in dimLine.Segments)
            {
                var segment1 = segment;
                if (dim.Segments.Size > segment.Index)
                    {
                        
                    dim.Segments.get_Item(segment.Index).Prefix = segment.Prefix;
                    //dim.Segments.get_Item(segment.Index).Suffix = segment.Suffix;

                    //String t = dim.Segments.get_Item(segment.Index).ValueString;
                    //if (dim.ValueString == "0" || dim.ValueString == null)
                    //{
                    //    dim.ValueOverride = ".";
                    //}
                    //// check for revit api dimension replace with text dim.ValueOverride = "Your Text Here" //v 1.5.0.7;

                    if ((segment.Suffix == "R.O." || segment.Suffix == " O/A H.D.") && options.ReplaceWithText)
                    {
                        String str = dim.ValueString;
                        dim.Segments.get_Item(segment.Index).ValueOverride = dim.Segments.get_Item(segment.Index).ValueString;
                    }
                    else
                    {
                        dim.Segments.get_Item(segment.Index).Suffix = segment.Suffix;
                    }


                }
                    else if (segment.Index == 0)
                    {
                        dim.Prefix = segment.Prefix;
                        dim.Suffix = segment.Suffix;
                    if (segment.Suffix == "R.O." && options.ReplaceWithText)
                    {
                        String str = dim.ValueString +" R.O.";
                        dim.ValueOverride = str;
                        //dim.Suffix = dim.Segments.get_Item(segment.Index).ValueString;
                    }
                    //else
                    //{
                    //    dim.Suffix = segment.Suffix;
                    //}
                }
            }
        }

        private static void MoveDim(Dimension dim, Core.Domain.Options options)
        {
            if (dim.Curve is Line line)
            {
                var view = dim.Document.ActiveView;
                var midPoint = GetDimMidPoint(dim);
                var totalLen = GetDimLength(dim);
                var currentLen = 0d;
                foreach (DimensionSegment dimSegment in dim.Segments)
                {
                    currentLen += dimSegment.Value ?? 0;
                    if (dimSegment.Value < options.MoveSegmentsLessThan && dimSegment.IsTextPositionAdjustable() && dimSegment.Value !=0)
                    {
                        var rotAxis = currentLen <= totalLen / 2 ? view.ViewDirection : view.ViewDirection.Negate();
                        if (line.Direction.DotProduct(view.UpDirection) <= double.Epsilon) // is horizontal
                        {
                            rotAxis = rotAxis.Negate();
                        }

                        var transform = Transform.CreateRotationAtPoint(rotAxis, 45 * Math.PI / 180,
                            dimSegment.TextPosition);
                        var baseVec = (midPoint - dimSegment.Origin).Normalize();
                        var rotatedVec = transform.OfVector(baseVec);
                        dimSegment.TextPosition = dimSegment.TextPosition.Add(options.MoveSegmentsBy * rotatedVec);
                    }
                }
            }
        }

        private static XYZ GetDimMidPoint(Dimension dim)
        {
            if (dim.NumberOfSegments == 0)
            {
                return dim.Origin;
            }

            if (dim.Curve is Line line)
            {
                var totalLen = GetDimLength(dim);

                var firstSeg = dim.Segments.get_Item(0);
                return firstSeg.Origin - 0.5 * (firstSeg.Value ?? 0) * line.Direction + 0.5 * totalLen * line.Direction;
            }

            throw new Exception("Error locating Dim midpoint");
        }

        private static double GetDimLength(Dimension dim)
        {
            if (dim.NumberOfSegments == 0)
            {
                return dim.Value ?? 0;
            }

            var totalLen = 0d;
            foreach (DimensionSegment dimSegment in dim.Segments)
            {
                totalLen += dimSegment.Value ?? 0;
            }

            return totalLen;
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

        private string GetDimSignature(Dimension dim)
        {
            if (dim.NumberOfSegments == 0)
            {
                return $"{dim.Value:F4}_{dim.Suffix}";
            }

            var sb = new StringBuilder();
            foreach (DimensionSegment dimSegment in dim.Segments)
            {
                sb.Append($"{dimSegment.Value:F4}_{dimSegment.Suffix}_");
            }

            return sb.ToString();
        }
    }
}
