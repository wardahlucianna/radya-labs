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
using NPOI.XSSF.UserModel.Helpers;
using FluentEmail.Core;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetListEquipmentWithAvailableStockHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetListEquipmentWithAvailableStockHandler(
            ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListEquipmentWithAvailableStockRequest>(
                nameof(GetListEquipmentWithAvailableStockRequest.IdSchool),
                nameof(GetListEquipmentWithAvailableStockRequest.Date),
                nameof(GetListEquipmentWithAvailableStockRequest.StartTime),
                nameof(GetListEquipmentWithAvailableStockRequest.EndTime));

            var getEquipmentList = await _dbContext.Entity<MsEquipment>()
                .Include(x => x.EquipmentType).ThenInclude(x => x.ReservationOwner)
                .Where(x => x.EquipmentType.IdSchool == param.IdSchool)
                .ToListAsync(CancellationToken);

            var StartDate = param.Date.Date.Add(param.StartTime);
            var EndDate = param.Date.Date.Add(param.EndTime);

            if(DateTime.Compare(StartDate, EndDate) >= 0)
            {
                throw new BadRequestException("Start Time must be less than End Time");
            }

            var getTransaction = await _dbContext.Entity<TrEquipmentReservation>()
                .Include(x => x.MappingEquipmentReservation)
                .Where(x => getEquipmentList.Contains(x.Equipment))
                .ToListAsync(CancellationToken);

            getTransaction = getTransaction
                .Where(x =>
                    (x.MappingEquipmentReservation.ScheduleStartDate.Subtract(TimeSpan.FromMinutes(x.MappingEquipmentReservation.VenueReservation?.PreparationTime ?? 0)) < EndDate &&
                    x.MappingEquipmentReservation.ScheduleEndDate.Add(TimeSpan.FromMinutes(x.MappingEquipmentReservation.VenueReservation?.CleanUpTime ?? 0)) > StartDate)
                ).ToList();


            var NotUserTransaction = getTransaction.Where(x => param.IdMappingEquipmentReservation == null ? true : (x.IdMappingEquipmentReservation != param.IdMappingEquipmentReservation)).ToList();
            var UserTransaction = getTransaction.Where(x => x.IdMappingEquipmentReservation == param.IdMappingEquipmentReservation).ToList();

            var res = getEquipmentList.Select(x => new ListEquipmentForBookingEquipmentOnly
            {
                IdEquipment = x.Id,
                EquipmentName = x.EquipmentName,
                EquipmentType = x.EquipmentType.EquipmentTypeName,
                Owner = new NameValueVm
                {
                    Id = x.EquipmentType.ReservationOwner?.Id,
                    Name = x.EquipmentType.ReservationOwner?.OwnerName
                },
                MaxBorrowingQty = x.MaxQtyBorrowing,
                EquipmentBorrowingQty = UserTransaction.Where(y => y.Equipment == x).Select(y => y.EquipmentBorrowingQty).FirstOrDefault(),
                CurrentAvailableStock = Math.Max(0, x.TotalStockQty - NotUserTransaction.Where(y => y.Equipment == x).Sum(y => y.EquipmentBorrowingQty))
            });

            return Request.CreateApiResult2(res as object);
        }
    }
}
