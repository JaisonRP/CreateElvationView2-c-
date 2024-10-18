using System;
using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Core.Domain
{
    public class DimLine : IEquatable<DimLine>
    {
        public IList<DimSegment> Segments { get; } = new List<DimSegment>();

        public IList<WallReference> References { get; } = new List<WallReference>();

        public string GroupId { get; set; } = Guid.NewGuid().ToString();

        public bool IsDrawable => References.Count >= 2;

        public void AddReference(WallReference reference)
        {
            if (reference != null)
            {
                References.Add(reference);
            }
        }

        public void AddSegment(DimSegment segment)
        {
            if (segment.Index > References.Count - 2)
            {
                throw new Exception($"There are {References.Count} references which gives {References.Count - 1} segments");
            }

            var existingSeg = Segments.FirstOrDefault(x => x.Index == segment.Index);
            if (existingSeg != null)
            {
                Segments.Remove(existingSeg);
            }

            Segments.Add(segment);
        }

        public bool Equals(DimLine other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            var isEqual = References.Count == other.References.Count;

            // If the number of references is equal, proceed with detailed comparison
            if (isEqual)
            {
                // Iterate through each reference and compare RefType and PlaneName
                for (var i = 0; i < References.Count; i++)
                {
                    isEqual = References[i].ReferenceRepresentation.RefType ==
                              other.References[i].ReferenceRepresentation.RefType &&
                              References[i].ReferenceRepresentation.PlaneName ==
                              other.References[i].ReferenceRepresentation.PlaneName;

                    // If segments are present, compare Suffix
                    if (i < Segments.Count && i < other.Segments.Count)
                    {
                        isEqual = isEqual && Segments[i].Suffix == other.Segments[i].Suffix;
                                  //        &&
                                  //Segments[i].Prefix == other.Segments[i].Prefix;
                    }
                    // If any inequality is found, break out of the loop
                    if (!isEqual)
                    {
                        break;
                    }
                }
            }

            return isEqual;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DimLine)obj);
        }

        public override int GetHashCode()
        {
            return (References != null ? References.Count.GetHashCode() : 0);
        }

        public static bool operator ==(DimLine left, DimLine right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(DimLine left, DimLine right)
        {
            return !Equals(left, right);
        }
    }
}
