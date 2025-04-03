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
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingEquipmentOnly.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Helper;
using BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.BookingVenueAndEquipment
{
    public class UpdateVenueReservationPrepAndCleanTimeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IMachineDateTime _dateTime;
        private IDbContextTransaction _transaction;
        private readonly GetVenueReservationUserLoginSpecialRoleHandler _userLoginSpecialRoleHandler;
        private readonly SaveEquipmentReservationHandler _saveEquipment;

        public UpdateVenueReservationPrepAndCleanTimeHandler(ISchedulingDbContext context, IMachineDateTime dateTime, GetVenueReservationUserLoginSpecialRoleHandler userLoginSpecialRoleHandler, SaveEquipmentReservationHandler saveEquipment)
        {
            _context = context;
            _dateTime = dateTime;
            _userLoginSpecialRoleHandler = userLoginSpecialRoleHandler;
            _saveEquipment = saveEquipment;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var request = await Request.ValidateBody<UpdateVenueReservationPrepAndCleanTimeRequest, UpdateVenueReservationPrepAndCleanTimeValidator>();

            var idLoggedUser = AuthInfo.UserId;

            List<int> visibleVenueStatuses = new List<VenueApprovalStatus>
            {
                VenueApprovalStatus.Approved,
                VenueApprovalStatus.WaitingForApproval,
                VenueApprovalStatus.NoNeedApproval,
            }.Cast<int>().ToList();

            var saveEquipment = new SaveEquipmentReservationRequest();

            DateTime today = _dateTime.ServerTime;

            var checkUserSpecialRole = await _userLoginSpecialRoleHandler.GetVenueReservationUserLoginSpecialRole(new GetVenueReservationUserLoginSpecialRoleRequest
            {
                IdUser = request.IdUser,
            });

            var idSchool = await _context.Entity<MsUserSchool>()
                    .Where(a => a.IdUser == request.IdUser)
                    .FirstOrDefaultAsync(CancellationToken);

            var getVenueReservationRule = await _context.Entity<MsVenueReservationRule>()
                .Where(a => a.IdSchool == idSchool.IdSchool)
                .FirstOrDefaultAsync(CancellationToken);

            var getVenueReservation = await _context.Entity<TrVenueReservation>()
                .Include(a => a.VenueMapping.Venue)
                .Include(a => a.VenueMapping.AcademicYear)
                .Include(a => a.User)
                .Where(a => a.VenueMapping.AcademicYear.IdSchool == idSchool.IdSchool
                    && a.Id == request.IdVenueReservation)
                .FirstOrDefaultAsync(CancellationToken);

            TimeSpan StartTime = getVenueReservation.StartTime.Subtract(TimeSpan.FromMinutes(request.PreparationTime ?? 0));
            TimeSpan EndTime = getVenueReservation.EndTime.Add(TimeSpan.FromMinutes(request.CleanUpTime ?? 0));

            if (!BookingVenueAndEquipmentValidationHelper.ValidateEdit(new BookingVenueAndEquipmentValidationParams
            {
                Today = today,
                ScheduleDate = getVenueReservation.ScheduleDate,
                StartTime = getVenueReservation.StartTime,
                MaxDayBooking = getVenueReservationRule.MaxDayBookingVenue,
                MaxTimeBooking = getVenueReservationRule.MaxTimeBookingVenue,
                AllSuperAccess = checkUserSpecialRole.AllSuperAccess,
                CanOverride = checkUserSpecialRole.CanOverrideAnotherReservation,
                IdLoggedUser = request.IdUser,
                CreatedBy = getVenueReservation.UserIn,
                CreatedFor = getVenueReservation.IdUser,
                ApprovalStatus = getVenueReservation.Status,
                IsOverlapping = getVenueReservation.IsOverlapping
            }))
                throw new BadRequestException($"Cannot edit {getVenueReservation.EventDescription} preparation and clean up time");

            #region validate overlap
            var getScheduleLesson = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == getVenueReservation.ScheduleDate)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var getScheduleRealization = _context.Entity<TrScheduleRealization2>()
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == getVenueReservation.ScheduleDate)
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            var joinSchedule = from scheduleLesson in getScheduleLesson
                               join scheduleRealization in getScheduleRealization
                               on new { scheduleLesson.ScheduleDate, scheduleLesson.IdLesson, scheduleLesson.SessionID }
                               equals new { scheduleRealization.ScheduleDate, scheduleRealization.IdLesson, scheduleRealization.SessionID }
                               select new
                               {
                                   ScheduleLesson = scheduleLesson,
                                   ScheduleRealization = scheduleRealization
                               };

            var scheduleLessonIds = new List<string>();

            if (joinSchedule.Any())
                scheduleLessonIds = joinSchedule.Select(sl => sl.ScheduleLesson.Id).ToList();

            var getScheduleLeesonRequestFloor = _context.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                    .ThenInclude(lesson => lesson.LessonTeachers)
                    .ThenInclude(lessonTeacher => lessonTeacher.Staff)
                .Include(a => a.Venue.VenueMappings)
                .Where(a => a.ScheduleDate.Date == getVenueReservation.ScheduleDate
                    && a.IdVenue == getVenueReservation.VenueMapping.IdVenue
                    && !scheduleLessonIds.Contains(a.Id))
                .OrderBy(a => a.VenueName.Length)
                    .ThenBy(a => a.VenueName)
                .ToList();

            // check with schedule lesson

            bool checkWithScheduleLesson = getScheduleLeesonRequestFloor
                .Distinct()
                .Where(a => getVenueReservation.ScheduleDate == a.ScheduleDate.Date
                    && StartTime < a.EndTime
                    && EndTime > a.StartTime)
                .Any();

            // check with schedule realization

            bool checkWithScheduleRealization = joinSchedule
                .Distinct()
                .Where(a => getVenueReservation.ScheduleDate == a.ScheduleRealization.ScheduleDate.Date
                    && StartTime < a.ScheduleRealization.EndTime
                    && EndTime > a.ScheduleRealization.StartTime
                    && a.ScheduleRealization.IdVenueChange == getVenueReservation.VenueMapping.IdVenue
                    && a.ScheduleRealization.IsCancel == false)
                .Any();

            // check with other venue reserve

            bool checkWithOtherVenueReservation = _context.Entity<TrVenueReservation>()
                .Where(a => a.ScheduleDate.Date == getVenueReservation.ScheduleDate
                    && a.VenueMapping.IdVenue == getVenueReservation.VenueMapping.IdVenue
                    && visibleVenueStatuses.Contains(a.Status))
                .AsEnumerable()
                .Where(a => StartTime < a.EndTime
                    && EndTime > a.StartTime
                    && a.Id != getVenueReservation.Id)
                .Any();

            if (checkWithScheduleLesson || checkWithScheduleRealization || checkWithOtherVenueReservation)
                throw new BadRequestException($"Preparation or Clean Up time is overlap");
            #endregion

            var getMappingEquipmentReservation = await _context.Entity<TrMappingEquipmentReservation>()
                .Where(a => a.IdVenueReservation == getVenueReservation.Id)
                .FirstOrDefaultAsync(CancellationToken);

            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                .Include(a => a.MappingEquipmentReservation)
                .Include(a => a.Equipment.EquipmentType)
                .Where(a => a.MappingEquipmentReservation.IdVenueReservation == getVenueReservation.Id)
                .ToListAsync(CancellationToken);

            var insertEquipmentReservation = getEquipmentReservation
                .Select(a => new SaveEquipmentReservationRequest_Equipment
                {
                    IdEquipment = a.IdEquipment,
                    EquipmentBorrowingQty = a.EquipmentBorrowingQty
                }).ToList();

            saveEquipment = new SaveEquipmentReservationRequest
            {
                IdSchool = getVenueReservation.VenueMapping.AcademicYear.IdSchool,
                IdUserLogin = idLoggedUser,
                EquipmentReservationMapping = new List<SaveEquipmentReservationRequest_Mapping>
                                {
                                    new SaveEquipmentReservationRequest_Mapping
                                    {
                                        IdMappingEquipmentReservation = getMappingEquipmentReservation?.Id ?? null,
                                        ScheduleStartDate = getVenueReservation.ScheduleDate.Add(StartTime),
                                        ScheduleEndDate = getVenueReservation.ScheduleDate.Add(EndTime),
                                        IdUser = getVenueReservation.IdUser,
                                        IdVenue = getVenueReservation.VenueMapping.IdVenue,
                                        EventDescription = getVenueReservation.EventDescription,
                                        IdVenueReservation = getVenueReservation.Id,
                                        Notes = getVenueReservation.Notes,
                                        ListEquipment = insertEquipmentReservation
                                    }
                                }
            };

            using (_transaction = await _context.BeginTransactionAsync(CancellationToken))
            {
                try
                {
                    var insertHTrVenueReservation = new HTrVenueReservation()
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdVenueReservation = getVenueReservation.Id,
                        IdVenueMapping = getVenueReservation.IdVenueMapping,
                        ScheduleDate = getVenueReservation.ScheduleDate,
                        StartTime = getVenueReservation.StartTime,
                        EndTime = getVenueReservation.EndTime,
                        IdUser = getVenueReservation.IdUser,
                        EventDescription = getVenueReservation.EventDescription,
                        IdRepeatGroup = getVenueReservation.IdRepeatGroup,
                        URL = getVenueReservation.URL ?? null,
                        FileName = getVenueReservation.FileName ?? null,
                        FileType = ("." + getVenueReservation.FileType) ?? null,
                        FileSize = getVenueReservation.FileSize,
                        Notes = getVenueReservation.Notes,
                        PreparationTime = getVenueReservation.PreparationTime,
                        CleanUpTime = getVenueReservation.CleanUpTime,
                        IsOverlapping = getVenueReservation.IsOverlapping,
                        Status = getVenueReservation.Status,
                        RejectionReason = getVenueReservation.RejectionReason,
                        IdUserAction = getVenueReservation.IdUserAction
                    };

                    _context.Entity<HTrVenueReservation>().Add(insertHTrVenueReservation);

                    getVenueReservation.PreparationTime = request.PreparationTime;
                    getVenueReservation.CleanUpTime = request.CleanUpTime;

                    _context.Entity<TrVenueReservation>().Update(getVenueReservation);

                    await _context.SaveChangesAsync();
                    await _transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    _transaction?.Rollback();

                    throw new Exception(ex.Message.ToString());
                }
            }

            // update equipment reserve
            var updateEquipment = await _saveEquipment.SaveEquipmentReservation(saveEquipment);

            return Request.CreateApiResult2();
        }
    }
}
