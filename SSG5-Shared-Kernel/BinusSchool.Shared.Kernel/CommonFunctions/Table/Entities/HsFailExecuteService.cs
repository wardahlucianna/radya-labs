using Microsoft.Azure.Cosmos.Table;

namespace BinusSchool.Common.Functions.Table.Entities
{
    public abstract class HsFailExecuteService : TableEntity
    {
        public string Message { get; set; }
        public string InnerMessage { get; set; }
        public string Value { get; set; }
    }
}
