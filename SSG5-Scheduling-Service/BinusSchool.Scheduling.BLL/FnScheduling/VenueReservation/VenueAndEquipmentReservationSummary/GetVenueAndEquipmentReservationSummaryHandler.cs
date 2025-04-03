using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class GetVenueAndEquipmentReservationSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;

        public GetVenueAndEquipmentReservationSummaryHandler(ISchedulingDbContext context)
        {
            _context = context;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueAndEquipmentReservationSummaryRequest>
                (nameof(GetVenueAndEquipmentReservationSummaryRequest.BookingStartDate),
                 nameof(GetVenueAndEquipmentReservationSummaryRequest.BookingEndDate));

            var response = new List<GetVenueAndEquipmentReservationSummaryResponse>();

            var idLoggedUser = AuthInfo.UserId;

            var idSchool = await _context.Entity<MsUserSchool>()
                .Where(a => a.IdUser == idLoggedUser)
                .FirstOrDefaultAsync(CancellationToken);

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.VenueMapping.Floor.Building)
                .Include(a => a.User)
                .Include(a => a.VenueMapping.AcademicYear)
                .Where(a => request.BookingStartDate <= a.ScheduleDate
                    && request.BookingEndDate >= a.ScheduleDate
                    && (string.IsNullOrEmpty(request.IdBuilding) ? true : a.VenueMapping.Floor.IdBuilding == request.IdBuilding)
                    && (string.IsNullOrEmpty(request.IdVenue) ? true : a.VenueMapping.IdVenue == request.IdVenue)
                    && (request.ApprovalStatus == null ? true : (VenueApprovalStatus)a.Status == request.ApprovalStatus)
                    && a.VenueMapping.AcademicYear.IdSchool == idSchool.IdSchool)
                .ToListAsync(CancellationToken);

            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                .Include(a => a.MappingEquipmentReservation)
                .Include(a => a.Equipment.EquipmentType)
                .ToListAsync(CancellationToken);

            var equipmentGrouped = getEquipmentReservation
                .Where(er => er.MappingEquipmentReservation != null && er.MappingEquipmentReservation.IdVenueReservation != null)
                .GroupBy(er => er.MappingEquipmentReservation.IdVenueReservation)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => new GetVenueAndEquipmentReservationSummaryResponse_Equipment
                    {
                        IdEquipment = e.IdEquipment,
                        EquipmentName = e.Equipment?.EquipmentName, 
                        EquipmentBorrowingQty = e.EquipmentBorrowingQty,
                        IdEquipmentType = e.Equipment?.IdEquipmentType, 
                        EquipmentTypeName = e.Equipment?.EquipmentType?.EquipmentTypeName
                    }).ToList()
                );

            var getVenueMappingApproval = await _context.Entity<MsVenueMappingApproval>()
                .Include(a => a.Staff)
                .ToListAsync(CancellationToken);

            var venueApproverGrouped = getVenueMappingApproval
                .Where(a => a.IdVenueMapping != null)
                .GroupBy(a => a.IdVenueMapping)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(a => new ItemValueVm
                    {
                        Id = a.IdBinusian,
                        Description = NameUtil.GenerateFullName(a.Staff.FirstName, a.Staff.LastName)
                    }).ToList()
                );

            var joinReservationData = from vr in getVenueReservation
                                      join erGroup in equipmentGrouped on vr.Id equals erGroup.Key into ergroup
                                      from er in ergroup.DefaultIfEmpty()
                                      join vmaGroup in venueApproverGrouped on vr.IdVenueMapping equals vmaGroup.Key into vmagroup
                                      from vma in vmagroup.DefaultIfEmpty()
                                      select new
                                      {
                                          vr = vr,
                                          er = er.Value,
                                          vma = vma.Value
                                      };

            var getReservationData = joinReservationData
                .Select(a => new GetVenueAndEquipmentReservationSummaryResponse
                {
                    IdBooking = a.vr.Id,
                    ScheduleDate = a.vr.ScheduleDate,
                    Time = new GetVenueAndEquipmentReservationSummaryResponse_Time
                    {
                        Start = a.vr.StartTime,
                        End = a.vr.EndTime,
                    },
                    Building = new ItemValueVm
                    {
                        Id = a.vr.VenueMapping.Floor.IdBuilding,
                        Description = a.vr.VenueMapping.Floor.Building.Description
                    },
                    Venue = new ItemValueVm
                    {
                        Id = a.vr.VenueMapping.IdVenue,
                        Description = a.vr.VenueMapping.Venue.Description
                    },
                    Requester = new ItemValueVm
                    {
                        Id = a.vr.IdUser,
                        Description = a.vr.User.DisplayName.Trim()
                    },
                    Event = a.vr.EventDescription,
                    Equipments = a.er,
                    VenueApprovalUsers = a.vma,
                    BookingStatus = new GetVenueAndEquipmentReservationSummaryResponse_BookingStatus
                    {
                        IdBooking = a.vr.Status,
                        BookingDesc = VenueReservationApprovalStatusHelper.ApprovalStatus(a.vr.Status),
                        RejectionReason = a.vr.RejectionReason,
                        IsOverlapping = a.vr.IsOverlapping
                    },
                    Note = a.vr.Notes,
                    PreparationTime = a.vr.PreparationTime,
                    CleanUpTime = a.vr.CleanUpTime,
                    FileUpload = new GetVenueAndEquipmentReservationSummaryResponse_FileUpload
                    {
                        FileName = a.vr.FileName,
                        FileType = a.vr.FileType,
                        FileSize = a.vr.FileSize,
                        Url = a.vr.URL
                    }
                });

            response.AddRange(getReservationData);

            return Request.CreateApiResult2(response as object);
        }
    }
}
