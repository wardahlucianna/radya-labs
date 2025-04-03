using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetDetailBookingEquipmentOnlyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetDetailBookingEquipmentOnlyHandler(
            ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDetailBookingEquipmentOnlyRequest>(
                nameof(GetDetailBookingEquipmentOnlyRequest.IdMappingEquipmentReservation)
                );

            var getEquipmentTransaction = await _dbContext.Entity<TrMappingEquipmentReservation>()
                .Include(x => x.User)
                .Include(x => x.Venue)
                .Include(x => x.EquipmentReservations)
                    .ThenInclude(x => x.Equipment)
                    .ThenInclude(x => x.EquipmentType)
                    .ThenInclude(x => x.ReservationOwner)
                .ToListAsync(CancellationToken);

            var filteredEquipmentTransaction = getEquipmentTransaction
                .Where(x => x.Id == param.IdMappingEquipmentReservation)
                .Where(x => x.VenueReservation == null)
                .FirstOrDefault();

            if(filteredEquipmentTransaction == null)
            {
                throw new Exception("Data not found");
            }

            var listEquipment = filteredEquipmentTransaction.EquipmentReservations
                .Select(x => x.IdEquipment).ToList();

            var getTransaction = await _dbContext.Entity<TrEquipmentReservation>()
                .Include(x => x.MappingEquipmentReservation)
                .Where(x =>
                    (x.MappingEquipmentReservation.ScheduleStartDate <= filteredEquipmentTransaction.ScheduleEndDate && x.MappingEquipmentReservation.ScheduleEndDate >= filteredEquipmentTransaction.ScheduleStartDate)
                )
                .Where(x => listEquipment.Contains(x.IdEquipment))
                .ToListAsync(CancellationToken);


            var NotUserTransaction = getTransaction.Where(x => param.IdMappingEquipmentReservation == null ? true : (x.IdMappingEquipmentReservation != param.IdMappingEquipmentReservation)).ToList();
            //var UserTransaction = getTransaction.Where(x => x.IdMappingEquipmentReservation == param.IdMappingEquipmentReservation).ToList();

            var res = new GetDetailBookingEquipmentOnlyResult
            {
                IdMappingEquipmentReservation = filteredEquipmentTransaction.Id,
                ScheduleDate = filteredEquipmentTransaction.ScheduleStartDate.Date, // End and Start Date should be the same
                StartEndTime = new GetDetailBookingEquipmentOnlyResult_StartEndTime
                {
                    StartTime = filteredEquipmentTransaction.ScheduleStartDate.TimeOfDay,
                    EndTime = filteredEquipmentTransaction.ScheduleEndDate.TimeOfDay
                },
                Requester = new NameValueVm
                {
                    Id = filteredEquipmentTransaction.IdUser,
                    Name = filteredEquipmentTransaction.User.DisplayName
                },
                EventDescription = filteredEquipmentTransaction.EventDescription,
                Venue = new ItemValueVm
                {
                    Id = filteredEquipmentTransaction.IdVenue,
                    Description = filteredEquipmentTransaction.Venue?.Description
                },
                Notes = filteredEquipmentTransaction.Notes,
                ListEquipment = filteredEquipmentTransaction.EquipmentReservations
                    .Select(y => new ListEquipmentForBookingEquipmentOnly
                    {
                        IdEquipment = y.IdEquipment,
                        EquipmentName = y.Equipment.EquipmentName,
                        EquipmentType = y.Equipment.EquipmentType.EquipmentTypeName,
                        Owner = new NameValueVm
                        {
                            Id = y.Equipment.EquipmentType.ReservationOwner?.Id,
                            Name = y.Equipment.EquipmentType.ReservationOwner?.OwnerName
                        },
                        CurrentAvailableStock = Math.Max(0, y.Equipment.TotalStockQty - NotUserTransaction.Where(z => z.IdEquipment == y.IdEquipment).Sum(z => z.EquipmentBorrowingQty)),
                        MaxBorrowingQty = y.Equipment.MaxQtyBorrowing,
                        EquipmentBorrowingQty = y.EquipmentBorrowingQty
                    }).ToList(),
                VenueNameinEquipment = filteredEquipmentTransaction.VenueNameinEquipment
            };

            return Request.CreateApiResult2(res as object);

        }
    }
}
