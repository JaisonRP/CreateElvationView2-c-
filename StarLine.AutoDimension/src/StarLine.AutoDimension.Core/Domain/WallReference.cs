namespace StarLine.AutoDimension.Core.Domain
{
    public class WallReference
    {
        public WallReference(int id, ReferenceRepresentation referenceRepresentation,
            Direction direction, string stringRepresentation)
        {
            Id = id;
            ReferenceRepresentation = referenceRepresentation;
            Direction = direction;
            StringRepresentation = stringRepresentation;
        }

        public int Id { get; }

        public ReferenceRepresentation ReferenceRepresentation { get; }

        public Direction Direction { get; }

        public string StringRepresentation { get; }
    }
}
