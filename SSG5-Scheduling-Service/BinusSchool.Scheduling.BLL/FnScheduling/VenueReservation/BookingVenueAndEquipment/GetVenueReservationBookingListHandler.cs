using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class GetVenueReservationBookingListHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _getVenueReservationUserLoginSpecialRoleHandler;

        public GetVenueReservationBookingListHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime, GetVenueReservationUserLoginSpecialRoleHandler getVenueReservationUserLoginSpecialRoleHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getVenueReservationUserLoginSpecialRoleHandler = getVenueReservationUserLoginSpecialRoleHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueReservationBookingListRequest>
                (nameof(GetVenueReservationBookingListRequest.IdSchool),
                 nameof(GetVenueReservationBookingListRequest.IdUser),
                 nameof(GetVenueReservationBookingListRequest.BookingStartDate),
                 nameof(GetVenueReservationBookingListRequest.BookingEndDate));

            var response = new List<GetVenueReservationBookingListResponse>();
            DateTime today = _dateTime.ServerTime;

            var checkUserSpecialRole = await _getVenueReservationUserLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = request.IdUser
            });

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Rejected,
                VenueApprovalStatus.Canceled
            }.Cast<int>().ToList();

            var getVenueReservation = await _dbContext.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.User)
                .Where(a => a.VenueMapping.AcademicYear.IdSchool == request.IdSchool
                    && request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate
                    && (string.IsNullOrEmpty(request.IdVenue) || a.VenueMapping.IdVenue == request.IdVenue)
                    && (request.BookingStatus == null
                        ? true
                        : (VenueApprovalStatus)a.Status == request.BookingStatus))
                .ToListAsync(CancellationToken);

            var getVenueReservationRule = await _dbContext.Entity<MsVenueReservationRule>()
                .Where(a => a.IdSchool == request.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var getEquipmentReservation = await _dbContext.Entity<TrEquipmentReservation>()
                .Include(a => a.MappingEquipmentReservation)
                .Include(a => a.Equipment.EquipmentType)
                .ToListAsync(CancellationToken);

            var equipmentGrouped = getEquipmentReservation
                .Where(er => er.MappingEquipmentReservation != null && er.MappingEquipmentReservation.IdVenueReservation != null)
                .GroupBy(er => er.MappingEquipmentReservation.IdVenueReservation)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => new GetVenueReservationBookingListResponse_Equipment
                    {
                        IdEquipment = e.IdEquipment,
                        EquipmentName = e.Equipment?.EquipmentName, // Use null-conditional operator here
                        EquipmentBorrowingQty = e.EquipmentBorrowingQty,
                        IdEquipmentType = e.Equipment?.IdEquipmentType, // Use null-conditional operator here
                        EquipmentTypeName = e.Equipment?.EquipmentType?.EquipmentTypeName // Use null-conditional operator here
                    }).ToList()
                );

            var getMappingEquipmentReserveHistory = await _dbContext.Entity<HTrMappingEquipmentReservation>()
                .Where(a => request.BookingStartDate <= a.ScheduleEndDate.Date
                    && request.BookingEndDate >= a.ScheduleStartDate.Date
                    && (string.IsNullOrEmpty(request.IdVenue) || a.IdVenue == request.IdVenue))
                .OrderByDescending(a => a.DateIn)
                .ToListAsync(CancellationToken);

            var equipmentReservations = await _dbContext.Entity<HTrEquipmentReservation>()
                .Include(a => a.Equipment.EquipmentType)
                .ToListAsync(CancellationToken);

            var equipmentGroupedByMapping = equipmentReservations
                .GroupBy(a => a.IdHTrMappingEquipmentReservation)
                .Select(g => new
                {
                    IdHTrMappingEquipmentReservation = g.Key,
                    Equipments = g.Select(e => new
                    {
                        e.IdEquipment,
                        e.Equipment.EquipmentName,
                        e.Equipment.IdEquipmentType,
                        e.Equipment.EquipmentType.EquipmentTypeName,
                        e.EquipmentBorrowingQty
                    }).ToList()
                })
                .ToList();

            // Join antara MappingEquipmentReserveHistory dan EquipmentGroup
            var joinEquipmentHistory = from merh in getMappingEquipmentReserveHistory
                                       join equipmentGroup in equipmentGroupedByMapping
                                           on merh.IdHTrMappingEquipmentReservation equals equipmentGroup.IdHTrMappingEquipmentReservation into equipmentGroup
                                       from equipment in equipmentGroup.DefaultIfEmpty()
                                       select new
                                       {
                                           MappingEquipmentReservationHistory = merh,
                                           Equipments = equipment?.Equipments // Memastikan Equipments tidak null
                                       };

            var topJoinEquipmentHistory = joinEquipmentHistory
                .GroupBy(x => x.MappingEquipmentReservationHistory.IdVenueReservation)
                .Select(g => g.OrderByDescending(x => x.MappingEquipmentReservationHistory.DateIn).FirstOrDefault())
                .ToList();

            // Join dengan VenueReservation dan penggabungan data
            var joinData = from vr in getVenueReservation
                           join erGroup in equipmentGrouped on vr.Id equals erGroup.Key into ergroup
                           from er in ergroup.DefaultIfEmpty()
                           join merh in topJoinEquipmentHistory on vr.Id equals merh.MappingEquipmentReservationHistory.IdVenueReservation into merhgroup
                           from merh in merhgroup.DefaultIfEmpty()
                           select new
                           {
                               VenueReservation = vr,
                               Equipments = !visibleVenueStatuses.Contains(vr.Status)
                                   ? (ergroup != null && ergroup.Any() ? er.Value : new List<GetVenueReservationBookingListResponse_Equipment>())
                                   : merh?.Equipments != null && merh.Equipments.Any()
                                       ? merh.Equipments.Select(e => new GetVenueReservationBookingListResponse_Equipment
                                       {
                                           IdEquipment = e.IdEquipment,
                                           EquipmentName = e.EquipmentName,
                                           EquipmentBorrowingQty = e.EquipmentBorrowingQty,
                                           IdEquipmentType = e.IdEquipmentType,
                                           EquipmentTypeName = e.EquipmentTypeName
                                       }).ToList()
                                       : new List<GetVenueReservationBookingListResponse_Equipment>()
                           };


            if (checkUserSpecialRole == null || (checkUserSpecialRole != null && checkUserSpecialRole.CanOverrideAnotherReservation == false && checkUserSpecialRole.AllSuperAccess == false))
            {
                var filterVenueReservation = joinData
                    .Where(a => a.VenueReservation.IdUser == request.IdUser
                        || a.VenueReservation.UserIn == request.IdUser)
                    .ToList();

                var getData = filterVenueReservation
                    .Distinct()
                    .Select(a => new GetVenueReservationBookingListResponse
                    {
                        IdBooking = a.VenueReservation.Id,
                        ScheduleDate = a.VenueReservation.ScheduleDate,
                        Venue = new ItemValueVm
                        {
                            Id = a.VenueReservation.VenueMapping.IdVenue,
                            Description = a.VenueReservation.VenueMapping.Venue.Description
                        },
                        Time = new GetVenueReservationBookingListResponse_Time
                        {
                            Start = a.VenueReservation.StartTime,
                            End = a.VenueReservation.EndTime
                        },
                        PreparationTime = a.VenueReservation.PreparationTime,
                        CleanUpTime = a.VenueReservation.CleanUpTime,
                        EventName = a.VenueReservation.EventDescription,
                        Requester = new ItemValueVm
                        {
                            Id = a.VenueReservation.IdUser,
                            Description = a.VenueReservation.User.DisplayName
                        },
                        BookingStatus = new GetVenueReservationBookingListResponse_Status
                        {
                            IdBooking = a.VenueReservation.Status,
                            BookingDesc = VenueReservationApprovalStatusHelper.ApprovalStatus(a.VenueReservation.Status),
                            RejectionReason = a.VenueReservation.RejectionReason,
                            IsOverlapping = a.VenueReservation.IsOverlapping
                        },
                        Equipments = a.Equipments,
                        Action = new GetVenueReservationBookingListResponse_Action
                        {
                            CanEdit = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                            {
                                Today = today,
                                ScheduleDate = a.VenueReservation.ScheduleDate,
                                StartTime = a.VenueReservation.StartTime,
                                MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                                MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                                AllSuperAccess = false,
                                CanOverride = false,
                                IdLoggedUser = request.IdUser, 
                                CreatedBy = a.VenueReservation.UserIn, 
                                CreatedFor = a.VenueReservation.IdUser, 
                                ApprovalStatus = a.VenueReservation.Status,
                                IsOverlapping = a.VenueReservation.IsOverlapping
                            }),
                            CanDelete = BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                            {
                                Today = today,
                                ScheduleDate = a.VenueReservation.ScheduleDate,
                                StartTime = a.VenueReservation.StartTime,
                                MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                                MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                                AllSuperAccess = false,
                                CanOverride = false,
                                IdLoggedUser = request.IdUser,
                                CreatedBy = a.VenueReservation.UserIn,
                                CreatedFor = a.VenueReservation.IdUser,
                                ApprovalStatus = a.VenueReservation.Status,
                                IsOverlapping = a.VenueReservation.IsOverlapping
                            }),
                            CanEditPrepTime = new GetVenueReservationBookingListResponse_Action_PrepTime
                            {
                                IsEnabled = false,
                                IsVisible = false
                            }
                        }
                    });

                response.AddRange(getData);
            }
            else
            {
                if (checkUserSpecialRole.CanOverrideAnotherReservation == false)
                    joinData = joinData
                    .Where(a => a.VenueReservation.IdUser == request.IdUser
                        || a.VenueReservation.UserIn == request.IdUser)
                    .ToList();

                var getData = joinData
                    .Select(a => new GetVenueReservationBookingListResponse
                    {
                        IdBooking = a.VenueReservation.Id,
                        ScheduleDate = a.VenueReservation.ScheduleDate,
                        Venue = new ItemValueVm
                        {
                            Id = a.VenueReservation.VenueMapping.IdVenue,
                            Description = a.VenueReservation.VenueMapping.Venue.Description
                        },
                        Time = new GetVenueReservationBookingListResponse_Time
                        {
                            Start = a.VenueReservation.StartTime,
                            End = a.VenueReservation.EndTime
                        },
                        PreparationTime = a.VenueReservation.PreparationTime,
                        CleanUpTime = a.VenueReservation.CleanUpTime,
                        EventName = a.VenueReservation.EventDescription,
                        Requester = new ItemValueVm
                        {
                            Id = a.VenueReservation.IdUser,
                            Description = a.VenueReservation.User.DisplayName
                        },
                        BookingStatus = new GetVenueReservationBookingListResponse_Status
                        {
                            IdBooking = a.VenueReservation.Status,
                            BookingDesc = VenueReservationApprovalStatusHelper.ApprovalStatus(a.VenueReservation.Status),
                            RejectionReason = a.VenueReservation.RejectionReason,
                            IsOverlapping = a.VenueReservation.IsOverlapping
                        },
                        Equipments = a.Equipments,
                        Action = new GetVenueReservationBookingListResponse_Action
                        {
                            CanEdit = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                            {
                                Today = today,
                                ScheduleDate = a.VenueReservation.ScheduleDate,
                                StartTime = a.VenueReservation.StartTime,
                                MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                                MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                                AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                                CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                                IdLoggedUser = request.IdUser,
                                CreatedBy = a.VenueReservation.UserIn,
                                CreatedFor = a.VenueReservation.IdUser,
                                ApprovalStatus = a.VenueReservation.Status,
                                IsOverlapping = a.VenueReservation.IsOverlapping
                            }),
                            CanDelete = BookingVenueAndEquipmentValidationHelper.ValidateDelete(new BookingVenueAndEquipmentValidationParams
                            {
                                Today = today,
                                ScheduleDate = a.VenueReservation.ScheduleDate,
                                StartTime = a.VenueReservation.StartTime,
                                MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                                MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                                AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                                CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                                IdLoggedUser = request.IdUser,
                                CreatedBy = a.VenueReservation.UserIn,
                                CreatedFor = a.VenueReservation.IdUser,
                                ApprovalStatus = a.VenueReservation.Status,
                                IsOverlapping = a.VenueReservation.IsOverlapping
                            }),
                            CanEditPrepTime = 
                            new GetVenueReservationBookingListResponse_Action_PrepTime
                            {
                                IsEnabled = BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
                                {
                                    Today = today,
                                    ScheduleDate = a.VenueReservation.ScheduleDate,
                                    StartTime = a.VenueReservation.StartTime,
                                    MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                                    MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                                    AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                                    CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                                    IdLoggedUser = request.IdUser,
                                    CreatedBy = a.VenueReservation.UserIn,
                                    CreatedFor = a.VenueReservation.IdUser,
                                    ApprovalStatus = a.VenueReservation.Status,
                                    IsOverlapping = a.VenueReservation.IsOverlapping
                                }),
                                IsVisible = BookingVenueAndEquipmentValidationHelper.HasCanOverrideAndAllSuperAccess(checkUserSpecialRole.CanOverrideAnotherReservation, checkUserSpecialRole.AllSuperAccess)
                            }
                        }
                    });

                response.AddRange(getData);
            }

            return Request.CreateApiResult2(response as object);
        }
    }
}
