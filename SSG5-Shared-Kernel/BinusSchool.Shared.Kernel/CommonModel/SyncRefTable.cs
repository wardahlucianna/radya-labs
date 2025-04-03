using System.Collections.Generic;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model
{
    public class SyncRefTable
    {
        public SyncRefTable()
        {            
        }
        
        public SyncRefTable(string table, IDictionary<string, string> id, AuditAction action, IDictionary<string, string> value)
        {
            Table = table;
            Id = id;
            Action = action;
            Value = value;    
        }

        public string Table { get; set; }
        public IDictionary<string, string> Id { get; set; }
        public AuditAction Action { get; set; }
        public IDictionary<string, string> Value { get; set; }
    }
}
