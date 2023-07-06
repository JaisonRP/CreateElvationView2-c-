using System.Collections.Generic;
using System.Linq;

namespace StarLine.AutoDimension.Core.Domain
{
    public class IdNamePair
    {
        public IdNamePair(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public static IdNamePair GetByIdOrName(IEnumerable<IdNamePair> list, IdNamePair filter)
        {
            return list.FirstOrDefault(x => x.Id == filter?.Id || string.Equals(x.Name, filter?.Name));
        }
    }
}
