using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Shapes;
using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;
using StarLine.AutoDimension.Plugin.Utils;
using Tortuga.Anchor;

namespace StarLine.AutoDimension.Plugin.Domain
{
    internal class PanelTagger : WallPanelBase
    {
        private readonly Panel _panel;
        private readonly Document _currentDocument;
        private readonly SelectionResult _selectionResult;
        private readonly TagChecker _tagChecker;

        public PanelTagger(int id, Panel panel, Document currentDocument,
            SelectionResult selectionResult, TagChecker tagChecker) : base(id)
        {
            _panel = panel;
            _currentDocument = currentDocument;
            _selectionResult = selectionResult;
            _tagChecker = tagChecker;
        }

        public double Height => _panel.get_Parameter(BuiltInParameter.CURTAIN_WALL_PANELS_HEIGHT)?.AsDouble() ?? 0;

        public void TagPanel(ElementId tagType, double offset)
        {
            if (!_tagChecker.IsTagged(_panel.Id)) // this is checcker  add to generic tags too
            {
                var reference = new Reference(_panel);
                if (_selectionResult.RevitLinkInstance != null)
                {
                    reference = reference.CreateLinkReference(_selectionResult.RevitLinkInstance);
                }

                if (tagType != null)
                {
                    // Extract X and Y values from _panel.Transform.Origin
                    double xPosition = _panel.Transform.Origin.X;
                    double yPosition = _panel.Transform.Origin.Y;
                    LocationCurve locationCurve = _panel.Host.Location as LocationCurve;
                    Curve line = locationCurve.Curve;
                    XYZ startPoint = line.GetEndPoint(0);
                    XYZ endPoint = line.GetEndPoint(1);

                    XYZ midPoint = new XYZ(
                        (startPoint.X + endPoint.X) / 2.0,
                        (startPoint.Y + endPoint.Y) / 2.0,
                        (startPoint.Z + endPoint.Z) / 2.0);

                    double zValue = midPoint.Z;
                    // Set the Z component of the tagLocation to a specific value (fixedZPosition)
                    var tagLocation = new XYZ(xPosition, yPosition, zValue + offset * _currentDocument.ActiveView.UpDirection.Z);

                   // var tagLocation = _panel.Transform.Origin + offset * _currentDocument.ActiveView.UpDirection;

                    IndependentTag.Create(_currentDocument, tagType, _currentDocument.ActiveView.Id,
                        reference, false, TagOrientation.Horizontal, tagLocation);
                }
            }
        }

        public void TagDoor(ElementId tagType, double offset)
        {
            if (IsDoor && !_tagChecker.IsTagged(_panel.Id))
            {
                var items = GetSubItem(x =>
                    x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows &&
                    x.Symbol.FamilyName.Contains("Slider")).ToArray();

                if (items.All(x => !_tagChecker.IsTagged(x.Id)))
                {
                    foreach (var fi in items)
                    {
                        if (TagInstance(fi, tagType, offset, true))
                        {
                            break;
                        }
                    }
                }
            }
        }

        public void TagCorner(ElementId typeId, double offset)
        {
            if (!_tagChecker.IsTagged(_panel.Id))
            {
                foreach (var fi in GetSubItem(x =>
                         x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel &&
                         x.Symbol.GetParameters("AssemblyType").Any(p =>
                             p.HasValue && string.Equals(p.AsValueString(), "CornerPost")) &&
                         !_tagChecker.IsTagged(x.Id)))
                {
                    var center = GetCenterPoint(fi) + offset * _currentDocument.ActiveView.UpDirection;
                    TagInstance(fi, typeId, center, false);
                }
            }
        }

        public void TagGeneric(ElementId typeId, double offset)
        {
            double offset1 = offset*6;
            if (!_tagChecker.IsTagged(_panel.Id))
            {
                var elements = _panel.GetDependentElements(new ElementClassFilter(typeof(FamilyInstance)));
                foreach (var elementId in elements)
                {
                    if (_panel.Document.GetElement(elementId) is FamilyInstance fi)
                    {
                        if (fi.Category.Id.IntegerValue == (int)BuiltInCategory.OST_GenericModel)
                        {
                            // Check the AssemblyType parameter of the element.Symbol
                            Parameter assemblyTypeParam = fi.Symbol.LookupParameter("AssemblyType");
                            if (assemblyTypeParam==null)
                            {assemblyTypeParam = fi.LookupParameter("AssemblyType"); }
                            string assemblyType = assemblyTypeParam.AsString();
                           
                            // Check the index
                            string index = fi.LookupParameter("Index").AsValueString();
                            string profcode = fi.Symbol.LookupParameter("ProfCode").AsValueString();
                            // Set the direction based on the index
                            XYZ direction;
                            if (index == "1") // bottom
                            {
                                if (assemblyType == "Flashing")
                                { direction = -_currentDocument.ActiveView.UpDirection - _currentDocument.ActiveView.RightDirection;
                                    //offset1 = offset+2;
                                }
                                else if (assemblyType == "InstallationAngle")
                                {
                                    direction = _currentDocument.ActiveView.UpDirection - _currentDocument.ActiveView.RightDirection ;
                                  //offset1 = offset + 50;
                                }
                                else { direction = -_currentDocument.ActiveView.UpDirection;
                                   // offset1 = offset / 2;
                                }
                                }
                            else if (index == "2") // right
                            {
                                if (assemblyType == "ParentExtrusion") { direction = _currentDocument.ActiveView.UpDirection - _currentDocument.ActiveView.RightDirection; 
                                    //offset1 = offset / 2; 
                                }
                                else { direction = _currentDocument.ActiveView.UpDirection + _currentDocument.ActiveView.RightDirection; 
                                    //offset1 = offset / 2;
                                }
                            }
                            else if (index == "3") // top
                            {
                                if (assemblyType == "ParentExtrusion") {// direction =2*_currentDocument.ActiveView.RightDirection + _currentDocument.ActiveView.UpDirection ;
                                                                        direction = new XYZ(20, 0, 20);
                                    offset1 = offset*12; 
                                }

                                else
                                {
                                    direction = +2 * _currentDocument.ActiveView.UpDirection;
                                }
                            }
                            else if (index == "4")
                            {
                                if (assemblyType == "ParentExtrusion") 
                                { 
                                    direction =  _currentDocument.ActiveView.RightDirection*1; 
                                    //offset1 = offset / 2; 
                                }
                                else { direction = _currentDocument.ActiveView.UpDirection - _currentDocument.ActiveView.RightDirection; }
                            }
                            else
                            {
                                direction = _currentDocument.ActiveView.UpDirection;
                            }
                            var v = offset1 * direction;
                            var center = GetCenterPoint(fi) + v;
                            TagInstance(fi, typeId, center, true);
                        }
                    }
                }
            }
        }

        public void TagMaterial(ElementId typeId, double offset)
        {
            if (!_tagChecker.IsTagged(_panel.Id))
            {
                
                foreach (var fi in GetSubItem(x =>
                             x.Category.Id.IntegerValue == (int)BuiltInCategory.OST_Windows &&
                             !_tagChecker.IsMaterialTagged(x)))
                    

                {

                    // Check the Panel series parameter of the element.Symbol or instance
                    Parameter seriesparam = _panel.Symbol.LookupParameter("Series");
                    if (seriesparam == null)
                    {
                        seriesparam = _panel.LookupParameter("Series");
                    }

                    string series = seriesparam.AsString();

                    // Set the direction based on the index
                    XYZ direction = _currentDocument.ActiveView.RightDirection;
                    var center = GetCenterPoint(fi) - 3.5 * _currentDocument.ActiveView.UpDirection + offset * direction;

                    if (series == "D4600" || series == "D4500R" || series == "D4540" || series == "D4540") {  
                        center = GetCenterPoint(fi)+ offset * _currentDocument.ActiveView.UpDirection- 0.8 * _currentDocument.ActiveView.RightDirection;
                    }
                    else
                    {
                         center = GetCenterPoint(fi) - 3.5 * _currentDocument.ActiveView.UpDirection + offset * direction;
                    }
                    TagInstance(fi, typeId, center, false);
                }
            }
        }

        public void TagAnnotation(ElementId typeId)
        {
            if (IsDoor && !_tagChecker.IsTagged(_panel.Id))
            {
                var doc = _panel.Document;
                var elem = doc.GetElement(typeId);
                if (elem is FamilySymbol fs)
                {
                    var location = _panel.Transform.Origin + (Height / 2) * _currentDocument.ActiveView.UpDirection;
                    _currentDocument.Create.NewFamilyInstance(location, fs, _currentDocument.ActiveView);
                }
            }
        }

        private IEnumerable<FamilyInstance> GetSubItem(Func<FamilyInstance, bool> predicate)
        {
            var doc = _panel.Document;
            foreach (var subComponentId in _panel.GetSubComponentIds())
            {
                var subElement = doc.GetElement(subComponentId);
                if (subElement is FamilyInstance fs)
                {
                    if (predicate(fs))
                    {
                        yield return fs;
                    }
                }
            }
        }

        private bool TagInstance(FamilyInstance fs, ElementId tagType, double offset, bool addLeader)
        {
            if (fs.Location is LocationPoint locationPoint)
            {
                var tagLocation = locationPoint.Point + offset * _currentDocument.ActiveView.UpDirection;
                return TagInstance(fs, tagType, tagLocation, addLeader);
            }

            return false;
        }

        private bool TagInstance(FamilyInstance fs, ElementId tagType, XYZ insertionPoint, bool addLeader)
        {
            var reference = new Reference(fs);
            if (_selectionResult.RevitLinkInstance != null)
            {
                reference = reference.CreateLinkReference(_selectionResult.RevitLinkInstance);
            }

            IndependentTag.Create(_currentDocument, tagType, _currentDocument.ActiveView.Id,
                reference, addLeader, TagOrientation.Horizontal, insertionPoint);
            return true;
        }

        private XYZ GetCenterPoint(Element element)
        {
            var boundingBox = element.get_BoundingBox(_currentDocument.ActiveView);
            var min = boundingBox.Min;
            var max = boundingBox.Max;
            var vec = max - min;
            return min + 0.5 * vec;
        }
    }
}
