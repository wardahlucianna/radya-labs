using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BinusSchool.Scheduling.FnLongRun.Interfaces
{
    public interface IVenueAndEquipmentReservationAutoApproveService
    {
        Task<string> VenueAndEquipmentReservationAutoApprove(string idSchool, CancellationToken cancellationToken);
    }
}
