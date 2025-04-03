using System;
using Microsoft.Azure.Cosmos.Table;

namespace BinusSchool.Common.Functions.Table.Entities
{
    public class HsFailSyncRefTable : HsFailExecuteService
    {
        public string AffectTable { get; set; }
        public Guid InvocationId { get; set; }
        public string OperationId { get; set; }
    }
}
