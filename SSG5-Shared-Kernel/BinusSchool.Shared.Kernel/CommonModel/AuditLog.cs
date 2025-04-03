using System;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Common.Model
{
    public class AuditLog
    {
        public AuditLog() {}
        
        public AuditLog(DateTime time, string database, string table, string column, string id, AuditAction action, string executor, string oldValue, string newValue)
        {
            Time = time;
            Database = database;
            Table = table;
            Column = column;
            Id = id;
            Action = action;
            Executor = executor;
            OldValue = oldValue;
            NewValue = newValue;
        }

        public DateTime Time { get; set; }
        public string Database { get; set; }
        public string Table { get; set; }
        public string Column { get; set; }
        public string Id { get; set; }
        public AuditAction Action { get; set; }
        public string Executor { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}