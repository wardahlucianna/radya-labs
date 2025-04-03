using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnLongRun.Services
{
    public class UpdateVenueReservationOverlapStatusService : IUpdateVenueReservationOverlapStatusService
    {
        private readonly ISchedulingDbContext _context;
        private readonly ILogger<UpdateVenueReservationOverlapStatusService> _logger;
        private readonly IVenueReservation _venueReservation;
        private readonly Dictionary<string, string> _dictSchool;

        public UpdateVenueReservationOverlapStatusService(ISchedulingDbContext context, ILogger<UpdateVenueReservationOverlapStatusService> logger, IVenueReservation venueReservation, Dictionary<string, string> dictSchool)
        {
            _context = context;
            _logger = logger;
            _venueReservation = venueReservation;
            _dictSchool = new Dictionary<string, string>();
        }

        public async Task<string> UpdateVenueReservationOverlapStstus(string idSchool, CancellationToken cancellationToken)
        {
            if (_dictSchool.ContainsKey(idSchool))
                return _dictSchool[idSchool];

            var result = await _context.Entity<MsSchool>()
                .Where(a => a.Id == idSchool)
                .Select(a => a.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return null;

            var request = new UpdateVenueReservationOverlapStatusRequest
            {
                IdSchool = idSchool,
                IdUser = "00000000-0000-0000-0000-000000000000"
            };

            var executeUpdateOverlap = await _venueReservation.UpdateVenueReservationOverlapStatus(request);

            _dictSchool.Add(idSchool, result);

            return _dictSchool[idSchool];
        }
    }
}
