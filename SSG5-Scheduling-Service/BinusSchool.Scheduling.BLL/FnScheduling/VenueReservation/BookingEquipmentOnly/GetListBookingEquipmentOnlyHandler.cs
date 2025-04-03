using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly
{
    public class GetListBookingEquipmentOnlyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;
        private readonly IMachineDateTime _dateTime;

        public GetListBookingEquipmentOnlyHandler(ISchedulingDbContext dbContext,
            GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetListBookingEquipmentOnlyRequest>(
            nameof(GetListBookingEquipmentOnlyRequest.IdSchool),
            nameof(GetListBookingEquipmentOnlyRequest.GetAllData)
            );

            var userLogin = AuthInfo.UserId;

            var checkUserSpecialRole = await _getVenueReservationUserLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = userLogin
            });

            var checkEquipmentList = false; //Only for Check Equipment Detail
            if (param.GetAllData)
            {
                checkEquipmentList = true;

                checkUserSpecialRole = new GetVenueReservationUserLoginSpecialRoleResponse
                {
                    CanOverrideAnotherReservation = true,
                    AllSuperAccess = true,
                };
            }

            var getVenueReservationRule = await _dbContext.Entity<MsVenueReservationRule>()
                .Where(a => a.IdSchool == param.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var data = await _dbContext.Entity<TrMappingEquipmentReservation>()
                .Include(x => x.EquipmentReservations).ThenInclude(x => x.Equipment)
                .Include(x => x.User)
                .Include(x => x.Venue)
                .Where(x => x.EquipmentReservations.Select(y => y.Equipment).FirstOrDefault().EquipmentType.IdSchool == param.IdSchool)
                .Where(x => (param.StartDate == null && param.EndDate == null) ? true :
                            (x.ScheduleStartDate.Date >= param.StartDate.Value.Date && x.ScheduleStartDate.Date <= param.EndDate.Value.Date &&
                            x.ScheduleEndDate.Date <= param.EndDate.Value.Date && x.ScheduleEndDate.Date >= param.StartDate.Value.Date))
                .Where(x => x.IdVenueReservation == null)
                .ToListAsync(CancellationToken);

            var today = _dateTime.ServerTime;

            var res = new List<GetListBookingEquipmentOnlyResult>();

            if ((checkUserSpecialRole == null || (checkUserSpecialRole != null && checkUserSpecialRole.CanOverrideAnotherReservation == false && checkUserSpecialRole.AllSuperAccess == false)) && checkEquipmentList == false)
            {
                data = data.Where(x => x.IdUser == userLogin  || x.UserIn == userLogin).ToList();

                res = data.Select(x => new GetListBookingEquipmentOnlyResult
                {
                    IdMappingEquipmentReservation = x.Id,
                    ScheduleDate = x.ScheduleStartDate.Date, //Schedule Start and End Date should be the same
                    StartEndTime = new GetListBookingEquipmentOnlyResult_StartEndTime
                    {
                        StartTime = x.ScheduleStartDate.TimeOfDay,
                        EndTime = x.ScheduleEndDate.TimeOfDay
                    },
                    Requester = new NameValueVm
                    {
                        Id = x.IdUser,
                        Name = x.User.DisplayName
                    },
                    EventDescription = x.EventDescription,
                    Venue = new ItemValueVm
                    {
                        Id = x.Venue?.Id,
                        Description = x.Venue?.Description
                    },
                    ListEquipment = x.EquipmentReservations.Select(y => new GetListBookingEquipmentOnlyResult_Equipment
                    {
                        IdEquipment = y.IdEquipment,
                        EquipmentName = y.Equipment.EquipmentName,
                        EquipmentBorrowingQty = y.EquipmentBorrowingQty
                    }).ToList(),
                    CanEdit = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                    {
                        Today = today,
                        ScheduleDate = x.ScheduleStartDate.Date,
                        StartTime = x.ScheduleStartDate.TimeOfDay,
                        MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                        MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                        AllSuperAccess = false,
                        CanOverride = false,
                        IdLoggedUser = userLogin,
                        CreatedBy = x.UserIn,
                        CreatedFor = x.IdUser,
                        ApprovalStatus = 0, //Booking Equipment doesn't have approval status
                        IsOverlapping = false, //Booking Equipment doesn't have overlapping
                        CheckApprovalStatus = false
                    }),
                    CanDelete = BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                    {
                        Today = today,
                        ScheduleDate = x.ScheduleStartDate.Date,
                        StartTime = x.ScheduleStartDate.TimeOfDay,
                        MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                        MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                        AllSuperAccess = false,
                        CanOverride = false,
                        IdLoggedUser = userLogin,
                        CreatedBy = x.UserIn,
                        CreatedFor = x.IdUser,
                        ApprovalStatus = 0,
                        IsOverlapping = false,
                        CheckApprovalStatus = false
                    }),
                    VenueNameinEquipment = x.VenueNameinEquipment
                }).ToList();
            }
            else
            {
                if (checkUserSpecialRole.CanOverrideAnotherReservation == false)
                {
                    data = data.Where(a => a.IdUser == userLogin || a.UserIn == userLogin).ToList();
                }

                res = data.Select(x => new GetListBookingEquipmentOnlyResult
                {
                    IdMappingEquipmentReservation = x.Id,
                    ScheduleDate = x.ScheduleStartDate.Date, //Schedule Start and End Date should be the same
                    StartEndTime = new GetListBookingEquipmentOnlyResult_StartEndTime
                    {
                        StartTime = x.ScheduleStartDate.TimeOfDay,
                        EndTime = x.ScheduleEndDate.TimeOfDay
                    },
                    Requester = new NameValueVm
                    {
                        Id = x.IdUser,
                        Name = x.User.DisplayName
                    },
                    EventDescription = x.EventDescription,
                    Venue = new ItemValueVm
                    {
                        Id = x.Venue?.Id,
                        Description = x.Venue?.Description
                    },
                    ListEquipment = x.EquipmentReservations.Select(y => new GetListBookingEquipmentOnlyResult_Equipment
                    {
                        IdEquipment = y.IdEquipment,
                        EquipmentName = y.Equipment?.EquipmentName,
                        EquipmentBorrowingQty = y.EquipmentBorrowingQty
                    }).ToList(),
                    CanEdit = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                    {
                        Today = today,
                        ScheduleDate = x.ScheduleStartDate.Date,
                        StartTime = x.ScheduleStartDate.TimeOfDay,
                        MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                        MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                        AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                        CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                        IdLoggedUser = userLogin,
                        CreatedBy = x.UserIn,
                        CreatedFor = x.IdUser,
                        ApprovalStatus = 0, //Booking Equipment doesn't have approval status
                        IsOverlapping = false, //Booking Equipment doesn't have overlapping
                        CheckApprovalStatus = false
                    }),
                    CanDelete = BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                    {
                        Today = today,
                        ScheduleDate = x.ScheduleStartDate.Date,
                        StartTime = x.ScheduleStartDate.TimeOfDay,
                        MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                        MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                        AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                        CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                        IdLoggedUser = userLogin,
                        CreatedBy = x.UserIn,
                        CreatedFor = x.IdUser,
                        ApprovalStatus = 0,
                        IsOverlapping = false,
                        CheckApprovalStatus = false
                    }),
                    VenueNameinEquipment = x.VenueNameinEquipment
                }).ToList();
            }

            return Request.CreateApiResult2(res as object);

        }

    }
}
