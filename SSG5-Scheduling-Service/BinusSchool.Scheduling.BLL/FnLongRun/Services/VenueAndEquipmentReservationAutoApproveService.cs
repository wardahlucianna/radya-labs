using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnLongRun.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Scheduling.FnLongRun.Services
{
    public class VenueAndEquipmentReservationAutoApproveService : IVenueAndEquipmentReservationAutoApproveService
    {
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<VenueAndEquipmentReservationAutoApproveService> _logger;
        private readonly IVenueReservation _venueReservation;
        private readonly Dictionary<string, string> _dictSchool;

        public VenueAndEquipmentReservationAutoApproveService(ISchedulingDbContext context, IMachineDateTime dateTime, ILogger<VenueAndEquipmentReservationAutoApproveService> logger, IVenueReservation venueReservation, Dictionary<string, string> dictSchool)
        {
            _context = context;
            _dateTime = dateTime;
            _logger = logger;
            _venueReservation = venueReservation;
            _dictSchool = new Dictionary<string, string>();
        }

        public async Task<string> VenueAndEquipmentReservationAutoApprove(string idSchool, CancellationToken cancellationToken)
        {
            if (_dictSchool.ContainsKey(idSchool))
                return _dictSchool[idSchool];

            var result = await _context.Entity<MsSchool>()
                .Where(a => a.Id == idSchool)
                .Select(a => a.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (result == null)
                return null;

            DateTime date = _dateTime.ServerTime;

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Where(a => date.Date <= a.ScheduleDate
                    && date.Date.AddDays(1) >= a.ScheduleDate
                    && a.Status == 3
                    && a.VenueMapping.AcademicYear.IdSchool == idSchool)
                .ToListAsync();

            if (!getVenueReservation.Any())
                return null;

            var insertVenueData = getVenueReservation.Select(a => new ChangeVenueReservationApprovalStatusRequest
            {
                IdBooking = a.Id,
                IdUser = "00000000-0000-0000-0000-000000000000",
                ApprovalStatus = Common.Model.Enums.VenueApprovalStatus.Approved,
                RejectionReason = null,
            }).ToList();

            var executeApproval = await _venueReservation.ChangeVenueReservationApprovalStatus(insertVenueData);

            _dictSchool.Add(idSchool, result);

            return _dictSchool[idSchool];
        }
    }
}
