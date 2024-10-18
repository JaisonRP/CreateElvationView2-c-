using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using StarLine.AutoDimension.Core.Domain;

namespace StarLine.AutoDimension.Plugin.Utils
{
    internal class RevitUtils
    {
        private readonly Document _document;

        public RevitUtils(Document document)
        {
            _document = document;
        }

        public string GetLengthUnit()
        {
            var docUnits = _document.GetUnits();
            var spec = SpecTypeId.Distance;
            var formatOptions = docUnits.GetFormatOptions(spec);
            var unitTypeId = formatOptions.GetUnitTypeId();
            return LabelUtils.GetLabelForUnit(unitTypeId);
        }

        public double GetLengthConversion()
        {
            var docUnits = _document.GetUnits();
            var spec = SpecTypeId.Distance;
            var formatOptions = docUnits.GetFormatOptions(spec);
            var unitTypeId = formatOptions.GetUnitTypeId();
            return UnitUtils.ConvertFromInternalUnits(1, unitTypeId);
        }

        public IList<ReferencePair> GetNamedReferences(FamilyInstance fs, SelectionResult selectionResult)
        {
            var pairs = new List<ReferencePair>();
            var types = Enum.GetValues(typeof(FamilyInstanceReferenceType)).Cast<FamilyInstanceReferenceType>();
            foreach (var familyInstanceReferenceType in types)
            {
                ExtractRef(fs, pairs, familyInstanceReferenceType, selectionResult);
            }

            return pairs;
        }

        private void ExtractRef(FamilyInstance fs, ICollection<ReferencePair> pairs,
            FamilyInstanceReferenceType familyInstanceReferenceType, SelectionResult selectionResult)
        {
            var refs = fs.GetReferences(familyInstanceReferenceType);
            foreach (var reference in refs)
            {
                var n = fs.GetReferenceName(reference);
                if (!string.IsNullOrWhiteSpace(n))
                {
                    var representation = GetRepresentation(reference, selectionResult, fs.Document);
                    pairs.Add(new ReferencePair(reference.ElementId.IntegerValue, n, representation));
                }
            }
        }

        public string GetRepresentation(Reference reference, SelectionResult selectionResult, Document ownerDocument)
        {
            var isElementInDoc = selectionResult.RevitLinkInstance == null;
            var representation = isElementInDoc ? reference.ConvertToStableRepresentation(ownerDocument) :
                MakeLinkedReference4Dimension(_document, reference.CreateLinkReference(selectionResult.RevitLinkInstance));
            return representation;
        }

        private string MakeLinkedReference4Dimension(Document doc, Reference reference)
        {
            if (reference.LinkedElementId == ElementId.InvalidElementId) return null;
            var ss = reference.ConvertToStableRepresentation(doc).Split(':');
            var res = string.Empty;
            var first = true;
            for (var i = 0; i < ss.Length; i++)
            {
                var t = ss[i];
                if (t.Contains("RVTLINK"))
                {
                    if (i + 1 < ss.Length && 0 == string.Compare(ss[i + 1], "0", StringComparison.InvariantCultureIgnoreCase))
                    {
                        t = "RVTLINK";
                    }
                    else
                    {
                        t = "0:RVTLINK";
                    }
                }

                if (!first)
                {
                    res = string.Concat(res, ":", t);
                }
                else
                {
                    res = t;
                    first = false;
                }
            }

            return res;
        }
    }
}
