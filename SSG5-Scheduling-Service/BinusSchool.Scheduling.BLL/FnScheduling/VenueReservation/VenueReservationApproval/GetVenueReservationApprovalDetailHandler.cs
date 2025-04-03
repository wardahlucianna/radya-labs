using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval.Helper;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueReservationApproval
{
    public class GetVenueReservationApprovalDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _dateTime;

        public GetVenueReservationApprovalDetailHandler(ISchedulingDbContext context, IMachineDateTime dateTime)
        {
            _context = context;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = Request.ValidateParams<GetVenueReservationApprovalDetailRequest>
                (nameof(GetVenueReservationApprovalDetailRequest.IdUser),
                 nameof(GetVenueReservationApprovalDetailRequest.IdBooking));

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.User)
                .Where(a => a.Id == request.IdBooking)
                .FirstOrDefaultAsync(CancellationToken);

            var getStaffName = await _context.Entity<MsUser>()
                .Where(a => a.Id == getVenueReservation.IdUserAction)
                .FirstOrDefaultAsync(CancellationToken);

            var getVenueMappingApproval = await _context.Entity<MsVenueMappingApproval>()
                .Include(a => a.Staff)
                .Where(a => a.IdVenueMapping == getVenueReservation.IdVenueMapping)
                .ToListAsync(CancellationToken);

            var validateApprover = getVenueMappingApproval
                .Where(a => a.IdBinusian == request.IdUser)
                .Count();

            if (validateApprover == 0) throw new BadRequestException("This user is not the Approver");

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Rejected,
                VenueApprovalStatus.Canceled
            }.Cast<int>().ToList();

            var equipmentReservation = new List<GetVenueReservationApprovalDetailResponse_Equipment>();

            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                .Include(a => a.MappingEquipmentReservation)
                .Include(a => a.Equipment.EquipmentType)
                .Where(a => a.MappingEquipmentReservation.IdVenueReservation == request.IdBooking)
                .ToListAsync(CancellationToken);

            var getVenueEquipment = await _context.Entity<MsVenueEquipment>()
                .Include(a => a.Equipment.EquipmentType)
                .Where(a => a.IdVenue == getVenueReservation.VenueMapping.IdVenue)
                .ToListAsync(CancellationToken);

            if (!visibleVenueStatuses.Contains(getVenueReservation.Status))
            {
                var insertEquipmentReservation = getEquipmentReservation
                .Select(a => new GetVenueReservationApprovalDetailResponse_Equipment
                {
                    IdEquipment = a.IdEquipment,
                    EquipmentName = a.Equipment.EquipmentName,
                    EquipmentBorrowingQty = a.EquipmentBorrowingQty,
                    IdEquipmentType = a.Equipment.IdEquipmentType,
                    EquipmentType = a.Equipment.EquipmentType.EquipmentTypeName
                }).ToList();

                equipmentReservation.AddRange(insertEquipmentReservation);
            }
            else
            {
                #region cancelled venue equipment
                var getMappingEquipmentReserveHistory = await _context.Entity<HTrMappingEquipmentReservation>()
                    .Where(a => a.IdVenueReservation == request.IdBooking)
                    .OrderByDescending(a => a.DateIn)
                    .ToListAsync(CancellationToken);

                var equipmentReservations = await _context.Entity<HTrEquipmentReservation>()
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
                #endregion

                var insertEquipmentReservation = topJoinEquipmentHistory
                    .SelectMany(a => a.Equipments)
                    .Select(equipment => new GetVenueReservationApprovalDetailResponse_Equipment
                    {
                        IdEquipment = equipment.IdEquipment,
                        EquipmentName = equipment.EquipmentName,
                        EquipmentBorrowingQty = equipment.EquipmentBorrowingQty,
                        IdEquipmentType = equipment.IdEquipmentType,
                        EquipmentType = equipment.EquipmentTypeName
                    })
                    .ToList();

                equipmentReservation.AddRange(insertEquipmentReservation);
            }

            var insertVenueEquipment = getVenueEquipment
                .Select(a => new GetVenueReservationApprovalDetailResponse_VenueEquipment
                {
                    IdEquipment = a.IdEquipment,
                    EquipmentName = a.Equipment.EquipmentName,
                    EquipmentBorrowingQty = a.EquipmentQty,
                    IdEquipmentType = a.Equipment.IdEquipmentType,
                    EquipmentTypeName = a.Equipment.EquipmentType.EquipmentTypeName
                }).ToList();

            var insertApprovalMapping = getVenueMappingApproval
                .Select(a => new ItemValueVm
                {
                    Id = a.IdBinusian,
                    Description = NameUtil.GenerateFullName(a.Staff.FirstName, a.Staff.LastName)
                }).ToList();

            var response = new GetVenueReservationApprovalDetailResponse()
            {
                IdVenueReservation = getVenueReservation.Id,
                Venue = new ItemValueVm
                {
                    Id = getVenueReservation.VenueMapping.IdVenue,
                    Description = getVenueReservation.VenueMapping.Venue.Description
                },
                Requester = new ItemValueVm
                {
                    Id = getVenueReservation.IdUser,
                    Description = getVenueReservation.User.DisplayName
                },
                EventDescription = getVenueReservation.EventDescription,
                ScheduleDate = getVenueReservation.ScheduleDate,
                StartTime = getVenueReservation.StartTime,
                EndTime = getVenueReservation.EndTime,
                Note = getVenueReservation.Notes,
                FileUpload = new GetVenueReservationApprovalDetailResponse_FileUpload
                {
                    FileName = getVenueReservation.FileName ?? "",
                    FileType = getVenueReservation.FileType ?? "",
                    FileSize = getVenueReservation.FileSize,
                    Url = getVenueReservation.URL
                },
                VenueApprovalStatus = new GetVenueReservationApprovalDetailResponse_VenueApprovalStatus
                {
                    Id = getVenueReservation.Status == 0 ? "4" : getVenueReservation.Status.ToString(),
                    Description = VenueReservationApprovalStatusHelper.ApprovalStatus(getVenueReservation.Status),
                    ModifiedTime = getVenueReservation.DateUp ?? null,
                    RejectionReason = getVenueReservation.RejectionReason,
                    IsOverlapping = getVenueReservation.IsOverlapping,
                    IdUserAction = getVenueReservation.IdUserAction,
                    IdUserActionName = getStaffName?.DisplayName ?? "",
                    VenueApprovalUser = insertApprovalMapping,
                },
                PreparationTime = getVenueReservation.PreparationTime,
                CleanUpTime = getVenueReservation.CleanUpTime,
                VenueEquipments = insertVenueEquipment,
                IdMappingEquipmentReservation = getEquipmentReservation.FirstOrDefault()?.IdMappingEquipmentReservation ?? null,
                Equipments = equipmentReservation
            };

            return Request.CreateApiResult2(response as object);
        }
    }
}
