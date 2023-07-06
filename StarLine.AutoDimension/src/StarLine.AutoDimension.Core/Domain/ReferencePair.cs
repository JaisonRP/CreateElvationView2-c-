namespace StarLine.AutoDimension.Core.Domain
{
    public class ReferencePair : IdNamePair
    {
        public ReferencePair(int id, string name, string referenceRepresentation) : base(id, name)
        {
            ReferenceRepresentation = referenceRepresentation;
        }

        public string ReferenceRepresentation { get; }
    }
}