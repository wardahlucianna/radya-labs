using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetCurrentAvailableEquipmentStockHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbcontext;
        public GetCurrentAvailableEquipmentStockHandler (ISchedulingDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCurrentAvailableEquipmentStockRequest>(
                nameof(GetCurrentAvailableEquipmentStockRequest.BookingStartDate),
                nameof(GetCurrentAvailableEquipmentStockRequest.BookingEndDate),
                nameof(GetCurrentAvailableEquipmentStockRequest.StartTime),
                nameof(GetCurrentAvailableEquipmentStockRequest.EndTime));

            if(param.StartTime > param.EndTime)
            {
                throw new Exception("Start time cannot be later than the end time");
            }

            if(param.BookingStartDate > param.BookingEndDate)
            {
                throw new Exception("Booking start date cannot be later than the booking end date");
            }

            // Generate a list of time windows for each day between BookingStartDate and BookingEndDate
            // The rule here is to treat each day in the range as an independent booking day.
            // For example, if BookingStartDate is November 1, 2024, and BookingEndDate is November 5, 2024,
            // with StartTime at 08:00 and EndTime at 15:00, this means:
            // - November 1, 2024: booking window is 08:00 to 15:00
            // - November 2, 2024: booking window is 08:00 to 15:00
            // - November 3, 2024: booking window is 08:00 to 15:00
            // - ...
            // This does NOT mean a continuous booking from November 1 at 08:00 to November 5 at 15:00.
            var dailyTimeWindows = Enumerable
                .Range(0, (param.BookingEndDate - param.BookingStartDate).Days + 1)
                .Select(offset => new
                {
                    DayStart = param.BookingStartDate.Date.AddDays(offset).Add(param.StartTime),
                    DayEnd = param.BookingStartDate.Date.AddDays(offset).Add(param.EndTime)
                })
                .ToList();

            var equipmentTransactions = await _dbcontext.Entity<TrEquipmentReservation>()
                .Include(x => x.MappingEquipmentReservation)
                .Where(x => (string.IsNullOrEmpty(param.IdEquipmentType) || x.Equipment.IdEquipmentType == param.IdEquipmentType))
                .ToListAsync(CancellationToken);

            // The filter ensures that any equipment reservation that overlaps with these daily windows
            // is included. For example:
            // - A transaction on November 3 from 10:00 to 11:00 will be shown, as it falls within the daily window.
            // - A transaction on November 3 from 16:00 to 17:00 will NOT be shown, as it falls outside the 08:00 to 15:00 window.
            equipmentTransactions = equipmentTransactions
                .Where(x => dailyTimeWindows.Any(window =>
                    x.MappingEquipmentReservation.ScheduleStartDate.Subtract(TimeSpan.FromMinutes(x.MappingEquipmentReservation.VenueReservation?.PreparationTime ?? 0)) < window.DayEnd &&
                    x.MappingEquipmentReservation.ScheduleEndDate.Add(TimeSpan.FromMinutes(x.MappingEquipmentReservation.VenueReservation?.CleanUpTime ?? 0)) > window.DayStart)
                ).ToList();

            var equipmentData = await _dbcontext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType)
                .Where(x => (string.IsNullOrEmpty(param.IdEquipmentType) || x.IdEquipmentType == param.IdEquipmentType))
                .ToListAsync(CancellationToken);

            var result = equipmentData
                .Select(x => new GetCurrentAvailableEquipmentStockResult
                {
                    EquipmentType = new NameValueVm
                    {
                        Id = x.IdEquipmentType,
                        Name = x.EquipmentType.EquipmentTypeName
                    },
                    EquipmentName = x.EquipmentName,
                    CurrentAvailableStock = Math.Max(0, x.TotalStockQty - equipmentTransactions.Where(y => y.IdEquipment == x.Id).Sum(y => y.EquipmentBorrowingQty)),
                    MaxQtyBorrowing = x.MaxQtyBorrowing
                }).ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
