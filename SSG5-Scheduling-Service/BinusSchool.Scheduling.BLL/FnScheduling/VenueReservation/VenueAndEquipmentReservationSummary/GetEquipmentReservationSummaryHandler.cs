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
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetEquipmentReservationSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;

        public GetEquipmentReservationSummaryHandler(ISchedulingDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetEquipmentReservationSummaryRequest>
                (nameof(GetEquipmentReservationSummaryRequest.BookingStartDate),
                 nameof(GetEquipmentReservationSummaryRequest.BookingEndDate));

            var response = new List<GetEquipmentReservationSummaryResponse>();

            var idLoggedUser = AuthInfo.UserId;

            var idSchool = await _context.Entity<MsUserSchool>()
                .Where(a => a.IdUser == idLoggedUser)
                .FirstOrDefaultAsync(CancellationToken);

            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                .Include(a => a.Equipment.EquipmentType.ReservationOwner)
                .Include(a => a.MappingEquipmentReservation.User)
                .Include(a => a.MappingEquipmentReservation.Venue)
                .Where(a => request.BookingStartDate <= a.MappingEquipmentReservation.ScheduleEndDate.Date
                    && request.BookingEndDate >= a.MappingEquipmentReservation.ScheduleStartDate.Date
                    && (string.IsNullOrEmpty(request.IdEquipmentType) ? true : a.Equipment.IdEquipmentType == request.IdEquipmentType)
                    && (string.IsNullOrEmpty(request.IdEquipment) ? true : a.IdEquipment == request.IdEquipment)
                    && a.Equipment.EquipmentType.IdSchool == idSchool.IdSchool)
                .ToListAsync(CancellationToken);

            var insertData = getEquipmentReservation
                .Select(a => new GetEquipmentReservationSummaryResponse
                {
                    IdEquipmentReservation = a.Id,
                    ScheduleDate = a.MappingEquipmentReservation.ScheduleStartDate.Date,
                    Time = new GetEquipmentReservationSummaryResponse_Time
                    {
                        Start = a.MappingEquipmentReservation.ScheduleStartDate.TimeOfDay,
                        End = a.MappingEquipmentReservation.ScheduleEndDate.TimeOfDay
                    },
                    Requester = new ItemValueVm
                    {
                        Id = a.MappingEquipmentReservation?.IdUser,
                        Description = a.MappingEquipmentReservation?.User?.DisplayName.Trim()
                    },
                    Venue = new ItemValueVm
                    {
                        Id = a.MappingEquipmentReservation?.IdVenue ?? a.MappingEquipmentReservation?.VenueNameinEquipment,
                        Description = a.MappingEquipmentReservation?.Venue?.Description ?? a.MappingEquipmentReservation?.VenueNameinEquipment
                    },
                    Event = a.MappingEquipmentReservation?.EventDescription,
                    Notes = a.MappingEquipmentReservation?.Notes,
                    Equipment = new GetEquipmentReservationSummaryResponse_Equipment
                    {
                        Name = a.Equipment?.EquipmentName,
                        Type = a.Equipment?.EquipmentType?.EquipmentTypeName,
                        Owner = a.Equipment?.EquipmentType?.ReservationOwner?.OwnerName,
                        BorrowingQty = a.EquipmentBorrowingQty
                    }
                });

            response.AddRange(insertData);

            return Request.CreateApiResult2(response as object);
        }
    }
}
