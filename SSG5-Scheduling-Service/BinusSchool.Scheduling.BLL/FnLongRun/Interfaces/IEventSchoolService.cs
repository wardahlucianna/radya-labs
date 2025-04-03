using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Scheduling.FnLongRun.Interfaces
{
    public interface IEventSchoolService
    {
        Task<string> GetEventSchool(string idSchool,bool halfDay, CancellationToken cancellationToken);
    }
}
