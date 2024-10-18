using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
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
        private readonly IList<LevelWrapper> _levels = new List<LevelWrapper>();

        private WallWrapper(Wall wall, Document currentDocument, SelectionResult selectionResult)
        {
            _wall = wall;
            _currentDocument = currentDocument;
            _selectionResult = selectionResult;
        }

        private Face[] _wallEnds;
        public Face[] WallEnds => _wallEnds ?? (_wallEnds = FindEndFaces());

        public double WallHeight => _wall.get_Parameter(BuiltInParameter.WALL_USER_HEIGHT_PARAM)?.AsDouble() ?? 0;

        public double WallOffset => _wall.get_Parameter(BuiltInParameter.WALL_BASE_OFFSET)?.AsDouble() ?? 0;

        public double WallLength
        {
            get
            {
                if (_wall.Location is LocationCurve loc)
                {
                    return loc.Curve.Length;
                }

                return 0;
            }
        }

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
                        Series = fs.LookupParameter("Series")?.AsString() ?? string.Empty,
                        FrameTag = fs.LookupParameter("FrameTag")?.AsString() ?? string.Empty,
                        RoHeight = fs.LookupParameter("RO Height")?.AsDouble() ?? 0,
                        IsHeelLeft = fs.Symbol.LookupParameter("Comp Channel Left Visibility")?.AsInteger() == 0  && fs.Symbol.LookupParameter("Corner Post Left")?.AsInteger() == 0,
                        IsHeelRight = fs.Symbol.LookupParameter("Comp Channel Right Visibility")?.AsInteger() == 0 && fs.Symbol.LookupParameter("Corner Post Right")?.AsInteger() == 0,
                        FamilyName = fs.Symbol.FamilyName
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
                        // Add the grid dimension line to the list
                        lines.Add(gridDimLine);
                    }
                    // Create a dimension line for the wall
                    var wallDimLine = new DimLine();
                    for (var i = 0; i < 2; i++)
                    {
                        var face = WallEnds[i];
                        wallDimLine.AddReference(FaceToReference(face, _currentDocument));
                    }

                    wallDimLine.Segments.Add(new DimSegment(0, string.Empty, "R.O."));
                    lines.Add(wallDimLine);

                    if (_panels.Count > 1)
                    {
                        var orderedPanels = _panels.OrderBy(x => x.FrameTag).ToArray();
                        var startPanel = orderedPanels[0];
                        var endPanel = orderedPanels[orderedPanels.Length - 1];
                        //var extJambLeft = startPanel.GetByType(direction, RefType.ExtendJambLeft);
                        //var extJambRight = endPanel.GetByType(direction, RefType.ExtendJambRight);
                        
                        var heelLeft = startPanel.GetByType(direction, RefType.HeelLeft);

                        var heelRight = endPanel.GetByType(direction, RefType.HeelRight);
                        var extDimLine = new DimLine();
                        extDimLine.AddReference(FaceToReference(WallEnds[0], _currentDocument));
                        //extDimLine.AddReference(extJambLeft);
                        
                        extDimLine.AddReference(heelLeft);
                        extDimLine.AddReference(heelRight);
                        // extDimLine.AddReference(extJambRight);
                        extDimLine.AddReference(FaceToReference(WallEnds[1], _currentDocument));
                        if (extDimLine.References.Count > 2)
                        {
                            if (heelLeft != null && heelRight != null)
                            {
                                var index = extDimLine.References.Count == 5 ? 2 : 1;
                                var segment = new DimSegment(index, string.Empty, " O/A H.D.");
                                extDimLine.AddSegment(segment);
                            }

                            lines.Add(extDimLine);
                        }

                    }
                }
                
                var dimLine = new DimLine();
                var isFirst = true;

                // Iterate through panels, ordered by FrameTag
                foreach (var panel in _panels.OrderBy(x => x.FrameTag))
                {
                    if (isFirst) // Check if it is the first panel
                    {
                        var heelLeft = panel.GetByType(direction, RefType.HeelLeft);
                        dimLine.AddReference(heelLeft);
                        isFirst = false;
                    }
                    // Append the panel geometry to the dimension line based on the specified direction
                    panel.AppendToDim(dimLine, direction);
                }

                lines.Add(dimLine);
            }
            else // Check if the direction is not vertical or  is horizontal
            {
                var tempLines = new List<DimLine>();
                foreach (var wallPanel in _panels.OrderBy(x => x.FrameTag))
                {
                    tempLines.AddRange(wallPanel.GetDimensionPair(direction));
                }

                tempLines.OrderBy(line => line.References.Count).ToList(); // sort the lines based on reference counts
                
                // Add all dimension lines from the temporary list to the main list of lines
                lines.AddRange(tempLines);

                // Before sorting
                Console.WriteLine("Before Sorting:");
                foreach (var line in lines)
                {
                    Console.WriteLine($"References count for line: {line.References.Count}");
                }


                //lines.OrderBy(line => line.References.Count).ToList(); // sort the lines based on reference counts

                // Sorting
                //var sortedLines = lines.OrderBy(line => line.References.Count).ToList();

                // After sorting
                //Console.WriteLine("After Sorting:");
                //foreach (var line in sortedLines)
                //{
                   // Console.WriteLine($"References count for line: {line.References.Count}");
                //}



                // Define a function to get a reference for the RoughBottom of a wall panel
                Func<WallPanel, WallReference> func = x => x.GetByType(direction, RefType.RoughBottom);
                var rb = _panels.Where(x => func(x) != null).Select(x => func(x)).FirstOrDefault();
                if (rb != null)
                {
                    var bars = new List<WallReference>(); 
                    foreach (var panel in _panels)
                    {
                        foreach (var wallReference in panel.GetAllByType(direction, RefType.ClBar))
                        {
                            if (bars.FirstOrDefault(x => x.ReferenceRepresentation.TypeName ==
                                                         wallReference.ReferenceRepresentation.TypeName) == null)
                            {
                                bars.Add(wallReference);
                            }
                        }
                    }

                    //foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                    foreach (var wallReference in bars.OrderBy(x => x.ReferenceRepresentation.ExtractIndex()))
                    {
                        var barLine = new DimLine();
                        // Add ClBar reference and RoughBottom reference to the dimension line
                        
                        barLine.AddReference(rb);
                        barLine.AddReference(wallReference);
                        if (barLine.IsDrawable) // Check if the dimension line is drawable
                        {
                            lines.Add(barLine); // Add the dimension line to the main list of lines
                        }
                    }
                }
                

                var levelsDimLine = new DimLine();
                foreach (var level in _levels)
                {
                    levelsDimLine.AddReference(new WallReference(0, new ReferenceRepresentation(RefType.Other),
                        direction, level.GetStringRepresentation()));
                }

                if (levelsDimLine.IsDrawable)
                {
                    lines.Add(levelsDimLine);
                }
            }
            
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

        public void AddLevel(LevelWrapper level)
        {
            _levels.Add(level);
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
    }
}
