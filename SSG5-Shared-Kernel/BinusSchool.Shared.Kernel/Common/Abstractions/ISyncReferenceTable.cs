using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;

namespace BinusSchool.Common.Abstractions
{
    public interface ISyncReferenceTable
    {
        Task SendChanges(string dbName, IEnumerable<AuditChangeLog> changeLogs, IEnumerable<string> ignoreTablesToSync = null);
    }
}
