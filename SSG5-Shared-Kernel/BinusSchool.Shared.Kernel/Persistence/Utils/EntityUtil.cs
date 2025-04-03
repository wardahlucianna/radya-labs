using System.Collections.Generic;
using System.Linq;
using BinusSchool.Domain.Abstractions;
using BinusSchool.Persistence.Abstractions;

namespace BinusSchool.Persistence.Utils
{
    public class EntityUtil
    {
        private static readonly IDictionary<int, string> _refEntities = new Dictionary<int, string>();

        public static string GetRefEntityNameOf<T>() where T : IEntity, IKindDb
        {
            var hashCode = typeof(T).GetHashCode();
            if (_refEntities.TryGetValue(hashCode, out var refEntity))
                return refEntity;

            var kinds = typeof(T).GetInterfaces().Where(x => typeof(IKindDb).IsAssignableFrom(x) && x != typeof(IKindDb));
            if (kinds.Count() > 2)
                throw new System.Exception("Can only implement one kind database.");
            var tableName = string.Format("{0}[{1}]", typeof(T).Name, kinds.First().Name[1..^2].ToUpper());

            if (!_refEntities.TryAdd(hashCode, tableName))
                throw new System.Exception($"Failed to add {tableName} to entity name collections.");

            return tableName;
        }
    }
}