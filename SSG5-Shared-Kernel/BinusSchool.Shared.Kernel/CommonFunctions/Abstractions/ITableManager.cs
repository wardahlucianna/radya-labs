using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace BinusSchool.Common.Functions.Abstractions
{
    public interface ITableManager
    {
        CloudTableClient TableClient { get; }
        
        Task InsertAndSave<T>(IEnumerable<T> entries, CancellationToken ct = default) where T : ITableEntity;
        Task InsertAndSave<T>(T entry, CancellationToken ct = default) where T : ITableEntity;
    }
}
