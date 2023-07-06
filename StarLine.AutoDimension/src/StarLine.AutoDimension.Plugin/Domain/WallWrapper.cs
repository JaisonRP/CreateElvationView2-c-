using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;
using StarLine.AutoDimension.Core.Services;
using StarLine.AutoDimension.Plugin.Utils;

namespace StarLine.AutoDimension.Plugin.Domain
{
    internal class WallWrapper
    {
        private readonly Document _currentDocument;
        private readonly Wall _wall;
        private readonly SelectionResult _selectionResult;

        private readonly IList<WallPanel> _panels = new List<WallPanel>();
        private readonly IList<Grid> _gridLines = new List<Grid>();

        private WallWrapper(Wall wall, Document currentDocument, SelectionResult selectionResult)
        {
            _wall = wall;
            _currentDocument = currentDocument;
            _selectionResult = selectionResult;
        }

        private Face[] _wallEnds;
        public Face[] WallEnds => _wallEnds ?? (_wallEnds = FindEndFaces());

        public double WallHeight => _wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM)?.AsDouble() ?? 0;

        public static WallWrapper Build(Wall wall, Document document, SelectionResult selectionResult)
        {
            var wallWrapper = new WallWrapper(wall, document, selectionResult);
            var linkedDocument = wall.Document;
            var utils = new RevitUtils(document);
            var options = OptionSerializer.ReadCurrent();
            foreach (var panelId in wall.CurtainGrid.GetPanelIds())
            {
                if (linkedDocument.GetElement(panelId) is FamilyInstance fs)
                {
                    var wallPanel = new WallPanel(panelId.IntegerValue)
                    {
                        IsByPass = fs.LookupParameter("Bypass")?.AsInteger() == 1,
                        Series = fs.LookupParameter("Series")?.AsString() ?? string.Empty
                    };

                    var pairs = utils.GetNamedReferences(fs, selectionResult);
                    foreach (var pair in pairs)
                    {
                        var wr = options.FindReference(pair);
                        if (wr != null)
                        {
                            wallPanel.AddReference(wr);
                        }
                    }

                    wallWrapper._panels.Add(wallPanel);
                }
            }

            return wallWrapper;
        }

        public IList<DimLine> GetDimensionPair(Direction direction)
        {
            var lines = new List<DimLine>();
            if (direction == Direction.Vertical)
            {
                if (WallEnds != null && WallEnds.Length >= 2)
                {
                    if (_gridLines.Count > 0)
                    {
                        var gridDimLine = new DimLine();
                        for (var i = 0; i < 2; i++)
                        {
                            var face = WallEnds[i];
                            gridDimLine.AddReference(FaceToReference(face, _currentDocument));
                        }

                        foreach (var gridLine in _gridLines)
                        {
                            var gridRef = new Reference(gridLine);
                            gridDimLine.AddReference(new WallReference(gridLine.Id.IntegerValue,
                                new ReferenceRepresentation(RefType.Other), Direction.Vertical,
                                gridRef.ConvertToStableRepresentation(_currentDocument)));
                        }

                        lines.Add(gridDimLine);
                    }

                    var wallDimLine = new DimLine();
                    for (var i = 0; i < 2; i++)
                    {
                        var face = WallEnds[i];
                        wallDimLine.AddReference(FaceToReference(face, _currentDocument));
                    }

                    wallDimLine.Segments.Add(new DimSegment(0, string.Empty, "R.O."));
                    lines.Add(wallDimLine);

                    var endPanels = FindEndPanels();
                    if (endPanels.Length >= 2 &&
                        endPanels[0] != ElementId.InvalidElementId &&
                        endPanels[1] != ElementId.InvalidElementId)
                    {
                        var startPanel = _panels.FirstOrDefault(x => x.Id == endPanels[0].IntegerValue);
                        var endPanel = _panels.FirstOrDefault(x => x.Id == endPanels[1].IntegerValue);
                        if (startPanel != null && endPanel != null)
                        {
                            var extJambLeft = startPanel.GetByType(direction, RefType.ExtendJambLeft);
                            var extJambRight = startPanel.GetByType(direction, RefType.ExtendJambRight);
                            var heelLeft = startPanel.GetByType(direction, RefType.HeelLeft);
                            var heelRight = startPanel.GetByType(direction, RefType.HeelRight);
                            var extDimLine = new DimLine();
                            extDimLine.AddReference(FaceToReference(WallEnds[0], _currentDocument));
                            extDimLine.AddReference(extJambLeft);
                            extDimLine.AddReference(heelLeft);
                            extDimLine.AddReference(heelRight);
                            extDimLine.AddReference(extJambRight);
                            extDimLine.AddReference(FaceToReference(WallEnds[1], _currentDocument));
                            if (extDimLine.References.Count > 2)
                            {
                                if (heelLeft != null && heelRight != null)
                                {
                                    var index = extDimLine.References.Count == 5 ? 2 : 1;
                                    var segment = new DimSegment(index, string.Empty, "H.D.");
                                    extDimLine.AddSegment(segment);
                                }

                                lines.Add(extDimLine);
                            }
                        }
                    }
                }
            }

            var tempLines = new List<DimLine>();
            foreach (var wallPanel in _panels)
            {
                tempLines.AddRange(wallPanel.GetDimensionPair(direction));
            }

            if (direction == Direction.Vertical)
            {
                // we need them in 1 line
                var groupId = Guid.NewGuid().ToString();
                foreach (var t in tempLines)
                {
                    t.GroupId = groupId;
                }
            }

            lines.AddRange(tempLines);
            return lines;
        }

        public Line GetBaseLine(Direction direction)
        {
            var view = _currentDocument.ActiveView;
            if (_wall.Location is LocationCurve loc && loc.Curve is Line line)
            {
                if (direction == Direction.Vertical)
                {
                    return line;
                }
                else
                {
                    var dir = line.Direction;
                    var point = line.GetEndPoint(1);
                    if (dir.AngleOnPlaneTo(view.RightDirection, view.ViewDirection) > Math.PI / 2)
                    {
                        point = line.GetEndPoint(0);
                    }

                    return Line.CreateBound(point, point.Add(view.UpDirection));
                }
            }

            throw new Exception("Error retrieving wall baseline");
        }

        public void AddGridLine(ElementId id)
        {
            if (_currentDocument.GetElement(id) is Grid grid && !grid.IsCurved)
            {
                if (grid.Curve is Line line)
                {
                    // Only add if vertical
                    var isVertical = XYZ.BasisY.IsAlmostEqualTo(line.Direction) ||
                                     XYZ.BasisY.IsAlmostEqualTo(line.Direction.Negate());
                    if (isVertical)
                    {
                        _gridLines.Add(grid);
                    }
                }
            }
        }

        private WallReference FaceToReference(Face face, Document doc)
        {
            var refRep = new ReferenceRepresentation(RefType.Other);
            var reference = face.Reference;
            var util = new RevitUtils(doc);
            var rep = util.GetRepresentation(reference, _selectionResult, _wall.Document);
            return new WallReference(face.Id, refRep, Direction.Vertical, rep);
        }

        private Face[] FindEndFaces()
        {
            if (_wall.Location is LocationCurve loc && loc.Curve is Line line)
            {
                var doc = _wall.Document;
                var opt = doc.Application.Create.NewGeometryOptions();
                opt.ComputeReferences = true;
                opt.IncludeNonVisibleObjects = true;
                var geometryElement = _wall.get_Geometry(opt);
                var faces = new List<Face>();
                foreach (var geometryObject in geometryElement)
                {
                    if (geometryObject is Solid solid)
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace pf)
                            {
                                // Check if face is perpendicular to wall line
                                var d = line.Direction.DotProduct(pf.FaceNormal);
                                if (Math.Abs(Math.Abs(d) - 1) < .001)
                                {
                                    faces.Add(face);
                                }
                            }
                        }
                    }
                }

                return faces.ToArray();
            }

            return null;
        }

        private ElementId[] FindEndPanels()
        {
            var minDist = double.MaxValue;
            var maxDist = double.MinValue;
            var startPanel = ElementId.InvalidElementId;
            var endPanel = ElementId.InvalidElementId;
            var doc = _wall.Document;
            if (_wall.Location is LocationCurve loc && loc.Curve is Line line)
            {
                foreach (var panelId in _wall.CurtainGrid.GetPanelIds())
                {
                    if (doc.GetElement(panelId) is FamilyInstance fs)
                    {
                        if (fs.Location is LocationCurve loc1 && loc1.Curve is Line line1)
                        {
                            var d = line.Origin.DistanceTo(line1.Origin);
                            if (d < minDist)
                            {
                                minDist = d;
                                startPanel = panelId;
                            }

                            if (d > maxDist)
                            {
                                maxDist = d;
                                endPanel = panelId;
                            }
                        }
                    }
                }
            }

            return new[] { startPanel, endPanel };
        }
    }
}
