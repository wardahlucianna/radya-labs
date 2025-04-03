using BinusSchool.Common.Model.Enums;
using Microsoft.Azure.Cosmos.Table;

namespace BinusSchool.Persistence.AuditDb.TableEntities
{
    public class HsAuditTrail : TableEntity
    {
        public string Database { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public string Executor { get; set; }
        public AuditAction Action { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}