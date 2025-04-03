using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Scheduling.FnLongRun.Interfaces
{
    public interface IUpdateVenueReservationOverlapStatusService
    {
        Task<string> UpdateVenueReservationOverlapStstus(string idSchool, CancellationToken cancellationToken);
    }
}
