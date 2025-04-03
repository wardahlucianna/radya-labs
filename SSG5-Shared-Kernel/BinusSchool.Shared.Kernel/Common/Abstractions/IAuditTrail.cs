using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;

namespace BinusSchool.Common.Abstractions
{
    public interface IAuditTrail
    {
        Task SaveChangeLog(string dbName, string executor, DateTime time, IEnumerable<AuditChangeLog> changeLogs);
    }
}