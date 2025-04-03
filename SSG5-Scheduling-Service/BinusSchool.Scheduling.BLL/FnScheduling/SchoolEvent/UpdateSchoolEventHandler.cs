using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class UpdateSchoolEventHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRolePosition _rolePositionService;

        public UpdateSchoolEventHandler(ISchedulingDbContext dbContext, IRolePosition rolePositionService)
        {
            _dbContext = dbContext;
            _rolePositionService = rolePositionService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateSchoolEventRequest, UpdateSchoolEventValidator>();

            var ay = await _dbContext.Entity<MsEventType>()
                .Include(x => x.AcademicYear)
                    .ThenInclude(x => x.School)
                .Where(x => x.Id == body.IdEventType)
                .Select(x => new { x.IdAcademicYear, x.AcademicYear.IdSchool })
                .FirstOrDefaultAsync(CancellationToken);

            if (ay is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.IdEventType));

            var existEvent = await _dbContext.Entity<TrEvent>()
                .FirstOrDefaultAsync(x => x.Id == body.IdEvent, CancellationToken);

            if (existEvent is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Event"], "Id", body.IdEvent));

            #region Event Change
            var DisplayName = _dbContext.Entity<MsUser>()
                .SingleOrDefault(e => e.Id == AuthInfo.UserId).DisplayName;
            var idEventChange = Guid.NewGuid().ToString();
            var newEventChange = new TrEventChange
            {
                Id = idEventChange,
                IdEvent = existEvent.Id,
                ChangeNotes = "Event Edited by " + DisplayName
            };
            _dbContext.Entity<TrEventChange>().Add(newEventChange);
            #endregion

            #region History Event
            var newHTrEvent = new HTrEvent
            {
                Id = idEventChange,
                IdEventType = existEvent.IdEventType,
                IdAcademicYear = existEvent.IdAcademicYear,
                Name = existEvent.Name,
                IsShowOnCalendarAcademic = existEvent.IsShowOnCalendarAcademic,
                IsShowOnSchedule = existEvent.IsShowOnSchedule,
                Objective = existEvent.Objective,
                Place = existEvent.Place,
                EventLevel = existEvent.EventLevel,
                StatusEvent = existEvent.StatusEvent,
                DescriptionEvent = existEvent.DescriptionEvent,
                StatusEventAward = existEvent.StatusEventAward,
                DescriptionEventAward = existEvent.DescriptionEventAward,
                IdCertificateTemplate = (!string.IsNullOrEmpty(existEvent.IdCertificateTemplate)) ? existEvent.IdCertificateTemplate : null,
            };
            _dbContext.Entity<HTrEvent>().Add(newHTrEvent);
            #endregion

            #region History Event Detail
            var UpdateEventDetail = await _dbContext.Entity<TrEventDetail>()
                                    .Include(e=>e.UserEvents)
                                    .Where(e => e.IdEvent == existEvent.Id)
                                    .ToListAsync(CancellationToken);

            foreach (var ItemDetail in UpdateEventDetail)
            {
                var newHTrEventDetail = new HTrEventDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    StartDate = ItemDetail.StartDate,
                    EndDate = ItemDetail.EndDate,
                    //StartTime = ItemDetail.StartTime,
                    //EndTime = ItemDetail.EndTime,
                };
                _dbContext.Entity<HTrEventDetail>().Add(newHTrEventDetail);
            }
            #endregion

            #region Event
            var listUserEvent = await _dbContext.Entity<TrUserEvent>()
                                    .Include(e=>e.EventDetail)
                                    .IgnoreQueryFilters()
                                   .Where(e => e.EventDetail.IdEvent == existEvent.Id && e.IsActive)
                                   .ToListAsync(CancellationToken);

            var eventDetails = new List<TrEventDetail>(body.Dates.Count());
            if (existEvent.StatusEvent == "Declined")
            {
                #region update Event
                existEvent.IdAcademicYear = ay.IdAcademicYear;
                existEvent.IdEventType = body.IdEventType;
                existEvent.Name = body.EventName;
                existEvent.IsShowOnCalendarAcademic = body.IsShowOnCalendarAcademic;
                existEvent.IsShowOnSchedule = body.IsShowOnSchedule;
                existEvent.Objective = body.EventObjective;
                existEvent.Place = body.EventPlace;
                existEvent.EventLevel = body.EventLavel;
                existEvent.IdCertificateTemplate = (!string.IsNullOrEmpty(body.IdCertificateTemplate)) ? body.IdCertificateTemplate : null;
                existEvent.StatusEvent = "On Review (1)";
                existEvent.DescriptionEvent = "Event Settings is On Review";
                #endregion

                #region update Event Detail
                UpdateEventDetail.ForEach(x => x.IsActive = false);
                _dbContext.Entity<TrEventDetail>().UpdateRange(UpdateEventDetail);
                #endregion

                #region update User Event
                listUserEvent.ForEach(x => x.IsActive = false);
                _dbContext.Entity<TrUserEvent>().UpdateRange(listUserEvent);
                #endregion

                #region Create Event Detail
                foreach (var date in body.Dates)
                {
                    var intersectEvents = await _dbContext.Entity<TrEventDetail>()
                       .Include(x => x.Event).ThenInclude(x => x.EventIntendedFor)
                       .Where(x
                           => x.Event.EventType.AcademicYear.IdSchool == ay.IdSchool
                           && (x.StartDate == date.Start || x.EndDate == date.End
                           || (x.StartDate < date.Start
                               ? (x.EndDate > date.Start && x.EndDate < date.End) || x.EndDate > date.End
                               : (date.End > x.StartDate && date.End < x.EndDate) || date.End > x.EndDate)))
                       .ToListAsync(CancellationToken);

                    // check date & time conflict with existing intersect event
                    var conflictEvents = Enumerable.Empty<string>();
                    if (intersectEvents.Count != 0)
                    {
                        // get each date of new event
                        var eachDate = DateTimeUtil.ToEachDay(date.Start, date.End);

                        foreach (var (start, end) in eachDate)
                        {
                            // select event that intersect date & time with day
                            var dayOfEvents = intersectEvents.Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, start, end));
                            // select event that intersect time with day
                            var intersectDayOfEvents = dayOfEvents
                                .Where(x
                                    => TimeSpanUtil.IsIntersect(x.StartDate.TimeOfDay, x.EndDate.TimeOfDay, start.TimeOfDay, end.TimeOfDay)
                                    && (body.IntendedFor.Any(e => e.Role == "ALL") || body.IntendedFor.Any(e => e.Role.Contains(x.Event.EventIntendedFor.First().IntendedFor)))
                                    && x.Event.Name == body.EventName);

                            if (intersectDayOfEvents.Any())
                                conflictEvents = conflictEvents.Concat(intersectEvents.Select(x => x.Event.Name));
                        }
                    }

                    if (conflictEvents.Any())
                        throw new BadRequestException("There is another event with same name, intended for, date and time.");

                    var newEventDetail = new TrEventDetail
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        StartDate = date.Start,
                        EndDate = date.End
                    };
                    eventDetails.Add(newEventDetail);
                }
                _dbContext.Entity<TrEventDetail>().AddRange(eventDetails);
                #endregion
            }
            else
            {
                #region update Event
                existEvent.IsShowOnCalendarAcademic = body.IsShowOnCalendarAcademic;
                existEvent.IsShowOnSchedule = body.IsShowOnSchedule;
                existEvent.Objective = body.EventObjective;
                existEvent.Place = body.EventPlace;
                existEvent.EventLevel = body.EventLavel;
                // existEvent.StatusEvent = "On Review (1)";
                #endregion

                eventDetails = UpdateEventDetail;

                #region update User Event
                listUserEvent.Where(e=>!e.EventDetail.IsActive).ToList().ForEach(x => x.IsActive = false);
                _dbContext.Entity<TrUserEvent>().UpdateRange(listUserEvent);
                #endregion
            }
            #endregion

            #region IntendedFor
            var GetEventIntendedFor = await _dbContext.Entity<TrEventIntendedFor>()
                   .Where(e => e.IdEvent == existEvent.Id)
                   .ToListAsync(CancellationToken);

            var GetEventIntendedForDepartment = await _dbContext.Entity<TrEventIntendedForDepartment>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPosition = await _dbContext.Entity<TrEventIntendedForPosition>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPersonal = await _dbContext.Entity<TrEventIntendedForPersonal>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForLevelStudent = await _dbContext.Entity<TrEventIntendedForLevelStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForGradeStudent = await _dbContext.Entity<TrEventIntendedForGradeStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPersonalStudent = await _dbContext.Entity<TrEventIntendedForPersonalStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
                .ToListAsync(CancellationToken);

            var GetEventIntendedForPersonalParent = await _dbContext.Entity<TrEventIntendedForPersonalParent>()
               .Include(e => e.EventIntendedFor)
               .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            var GetEventIntendedForAttendanceStudent = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                .Include(e => e.EventIntendedFor)
                .Where(e => e.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            var GetEventIntendedForAttendancePIC = await _dbContext.Entity<TrEventIntendedForAtdPICStudent>()
               .Include(e => e.EventIntendedForAttendanceStudent).ThenInclude(e => e.EventIntendedFor)
               .Where(e => e.EventIntendedForAttendanceStudent.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            var GetEventIntendedForAttendanceCheck = await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
               .Include(e => e.EventIntendedForAttendanceStudent).ThenInclude(e => e.EventIntendedFor)
               .Where(e => e.EventIntendedForAttendanceStudent.EventIntendedFor.IdEvent == existEvent.Id)
               .ToListAsync(CancellationToken);

            #region History IntendedFor
            foreach (var ItemIntendedFor in GetEventIntendedFor)
            {
                var newHTrEventIntededFor = new HTrEventIntendedFor
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IntendedFor = ItemIntendedFor.IntendedFor,
                    Option = ItemIntendedFor.Option,
                    SendNotificationToLevelHead = ItemIntendedFor.SendNotificationToLevelHead,
                    NeedParentPermission = ItemIntendedFor.NeedParentPermission,
                    NoteToParent = ItemIntendedFor.NoteToParent,
                };
                _dbContext.Entity<HTrEventIntendedFor>().AddRange(newHTrEventIntededFor);

                #region History IntededForDept
                var dataIndetdedForDept = GetEventIntendedForDepartment.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForDept in dataIndetdedForDept)
                {
                    var newHTrEventIntededForDept = new HTrEventIntendedForDepartment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdDepartment = ItemIntendedForDept.IdDepartment,
                    };
                    _dbContext.Entity<HTrEventIntendedForDepartment>().Add(newHTrEventIntededForDept);
                }
                #endregion

                #region History IntededForPosition
                var dataIndetdedForPosition = GetEventIntendedForPosition.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPositon in GetEventIntendedForPosition)
                {
                    var newHTrEventIntededForPosition = new HTrEventIntendedForPosition
                    {
                        IdHTrEventIntendedForPosition = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdTeacherPosition = ItemIntendedForPositon.IdTeacherPosition,
                    };
                    _dbContext.Entity<HTrEventIntendedForPosition>().Add(newHTrEventIntededForPosition);
                }
                #endregion

                #region History IntededForPersonal
                var dataIndetdedForPersonal = GetEventIntendedForPersonal.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPersonal in dataIndetdedForPersonal)
                {
                    var newHTrEventIntededForPersonal = new HTrEventIntendedForPersonal
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdUser = ItemIntendedForPersonal.IdUser,
                    };
                    _dbContext.Entity<HTrEventIntendedForPersonal>().Add(newHTrEventIntededForPersonal);
                }
                #endregion

                #region History IntendedForLevelStudent
                var dataIndetdedForLevel = GetEventIntendedForLevelStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForLevel in dataIndetdedForLevel)
                {
                    var newHTrEventIntededForLevel = new HTrEventIntendedForLevelStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdLevel = ItemIntendedForLevel.IdLevel,
                    };
                    _dbContext.Entity<HTrEventIntendedForLevelStudent>().Add(newHTrEventIntededForLevel);
                }
                #endregion

                #region History IntendedForGradeStudent
                var dataIndetdedForGrade = GetEventIntendedForGradeStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForGrade in dataIndetdedForGrade)
                {
                    var newHTrEventIntededForGrade = new HTrEventIntendedForGradeStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdHomeroom = ItemIntendedForGrade.IdHomeroom,
                    };
                    _dbContext.Entity<HTrEventIntendedForGradeStudent>().Add(newHTrEventIntededForGrade);
                }
                #endregion

                #region History IntendedForPersonalStudent
                var dataIndetdedForPersonalStudent = GetEventIntendedForPersonalStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPersonalStudent in dataIndetdedForPersonalStudent)
                {
                    var newHTrEventIntededForPersonalStudent = new HTrEventIntendedForPersonalStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdStudent = ItemIntendedForPersonalStudent.IdStudent,
                    };
                    _dbContext.Entity<HTrEventIntendedForPersonalStudent>().Add(newHTrEventIntededForPersonalStudent);
                }
                #endregion

                #region History IntendedForPersonalParent
                var dataIndetdedForPersonalPersonalParent = GetEventIntendedForPersonalParent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();

                foreach (var ItemIntendedForPersonalParent in dataIndetdedForPersonalPersonalParent)
                {
                    var newHTrEventIntededForPersonalParent = new HTrEventIntendedForPersonalParent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        IdParent = ItemIntendedForPersonalParent.IdParent,
                    };
                    _dbContext.Entity<HTrEventIntendedForPersonalParent>().Add(newHTrEventIntededForPersonalParent);
                }
                #endregion

                #region History IntendedForAttendanceStudent
                var DataIntendedForAttendanceStudent = GetEventIntendedForAttendanceStudent.Where(e => e.IdEventIntendedFor == ItemIntendedFor.Id).ToList();
                foreach (var ItemIntendedForAttendanceStudent in DataIntendedForAttendanceStudent)
                {
                    var newHTrEventIntededForAttendanceStudent = new HTrEventIntendedForAtdStudent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventIntendedFor = newHTrEventIntededFor.Id,
                        Type = ItemIntendedForAttendanceStudent.Type,
                        IsSetAttendance = ItemIntendedForAttendanceStudent.IsSetAttendance,
                        IsRepeat = ItemIntendedForAttendanceStudent.IsRepeat,
                    };
                    _dbContext.Entity<HTrEventIntendedForAtdStudent>().Add(newHTrEventIntededForAttendanceStudent);

                    #region IntendedForAttendancePIC
                    var DataIntendedForAttendancePIC = GetEventIntendedForAttendancePIC.Where(e => e.IdEventIntendedForAttendanceStudent == ItemIntendedForAttendanceStudent.Id).ToList();
                    foreach (var ItemIntendedForAttendancePIC in DataIntendedForAttendancePIC)
                    {
                        var newHTrEventIntededForAttendancePIC = new HTrEventIntendedForAtdPICStudent
                        {
                            IdHTrEventIntendedForAtdPICStudent = Guid.NewGuid().ToString(),
                            IdEventIntendedForAttendanceStudent = newHTrEventIntededForAttendanceStudent.Id,
                            Type = ItemIntendedForAttendancePIC.Type,
                            IdUser = ItemIntendedForAttendancePIC.IdUser,
                        };
                        _dbContext.Entity<HTrEventIntendedForAtdPICStudent>().Add(newHTrEventIntededForAttendancePIC);
                    }
                    #endregion

                    #region IntendedForAttendanceCheck
                    var DataIntendedForAttendanceCheck = GetEventIntendedForAttendanceCheck.Where(e => e.IdEventIntendedForAttendanceStudent == ItemIntendedForAttendanceStudent.Id).ToList();
                    foreach (var ItemIntendedForAttendanceCheck in DataIntendedForAttendanceCheck)
                    {
                        var newHTrEventIntededForAttendanceCheck = new HTrEvIntendedForAtdCheckStudent
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdHTrEventIntendedForAtdStudent = newHTrEventIntededForAttendanceStudent.Id,
                            CheckName = ItemIntendedForAttendanceCheck.CheckName,
                            Time = ItemIntendedForAttendanceCheck.Time,
                            IsPrimary = ItemIntendedForAttendanceCheck.IsPrimary,
                            StartDate = ItemIntendedForAttendanceCheck.StartDate,
                            StartTime = ItemIntendedForAttendanceCheck.StartTime,
                            EndTime = ItemIntendedForAttendanceCheck.EndTime,
                        };
                        _dbContext.Entity<HTrEvIntendedForAtdCheckStudent>().Add(newHTrEventIntededForAttendanceCheck);
                    }
                    #endregion
                }
                #endregion
            }
            #endregion


            // if (existEvent.StatusEvent == "Declined")
            // {
            #region Update All Intended For
            #region Intended For Depatement
            GetEventIntendedForDepartment.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForDepartment>().UpdateRange(GetEventIntendedForDepartment);
            #endregion

            #region Intended For Position
            GetEventIntendedForPosition.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForPosition>().UpdateRange(GetEventIntendedForPosition);
            #endregion

            #region Intended For Personal Staff
            GetEventIntendedForPersonal.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForPersonal>().UpdateRange(GetEventIntendedForPersonal);
            #endregion

            #region Intended For Level
            GetEventIntendedForLevelStudent.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForLevelStudent>().UpdateRange(GetEventIntendedForLevelStudent);
            #endregion

            #region Intended For Grade
            GetEventIntendedForGradeStudent.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForGradeStudent>().UpdateRange(GetEventIntendedForGradeStudent);
            #endregion

            #region Intended For Personal Student
            GetEventIntendedForPersonalStudent.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForPersonalStudent>().UpdateRange(GetEventIntendedForPersonalStudent);
            #endregion

            #region Intended For Personal Parent
            GetEventIntendedForPersonalParent.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForPersonalParent>().UpdateRange(GetEventIntendedForPersonalParent);
            #endregion

            #region Intended For Attendance Student PIC
            GetEventIntendedForAttendancePIC.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedForAtdPICStudent>().UpdateRange(GetEventIntendedForAttendancePIC);
            #endregion

            #region Intended For Attendance Check
            GetEventIntendedForAttendanceCheck.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrEventIntendedForAtdCheckStudent>().UpdateRange(GetEventIntendedForAttendanceCheck);
            #endregion

            #region Intended For Attendance Student
            GetEventIntendedForAttendanceStudent.ForEach(x => x.IsActive = false);
            _dbContext.Entity<TrEventIntendedForAttendanceStudent>().UpdateRange(GetEventIntendedForAttendanceStudent);
            #endregion

            #region IntendedFor
            GetEventIntendedFor.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrEventIntendedFor>().UpdateRange(GetEventIntendedFor);
            #endregion

            #endregion

            #region Create All Intended For
            List<TrEventIntendedFor> EventIntendedFor = new List<TrEventIntendedFor>();
            List<TrEventIntendedForDepartment> EventIntendedForDepartement = new List<TrEventIntendedForDepartment>();
            List<TrEventIntendedForPosition> EventIntendedForPosition = new List<TrEventIntendedForPosition>();
            List<TrEventIntendedForPersonal> EventIntendedForPersonal = new List<TrEventIntendedForPersonal>();
            List<TrEventIntendedForPersonalStudent> EventIntendedForPersonalStudent = new List<TrEventIntendedForPersonalStudent>();
            List<TrEventIntendedForPersonalParent> EventIntendedForPersonalParent = new List<TrEventIntendedForPersonalParent>();
            List<TrEventIntendedForGradeParent> EventIntendedForGradeParent = new List<TrEventIntendedForGradeParent>();
            List<TrEventIntendedForGradeParent> EventIntendedForLevelParent = new List<TrEventIntendedForGradeParent>();
            List<TrEventIntendedForGradeStudent> EventIntendedForGradeStudent = new List<TrEventIntendedForGradeStudent>();
            List<TrEventIntendedForLevelStudent> EventIntendedForLevelStudent = new List<TrEventIntendedForLevelStudent>();
            List<TrEventIntendedForAtdPICStudent> EventIntendedForAttendancePIC = new List<TrEventIntendedForAtdPICStudent>();
            List<TrEventIntendedForAtdCheckStudent> IntendedForAttendanceCheckStudent = new List<TrEventIntendedForAtdCheckStudent>();
            List<TrEventIntendedForAttendanceStudent> EventIntendedForAttendanceStudent = new List<TrEventIntendedForAttendanceStudent>();
            TrEventIntendedFor NewIntendedFor = null;
            // list of parent id when intended for personal parent is selected
            var intendedPersonalIdParents = new List<string>();
            // list of homeroom and grade when intended for grade parent is selected
            List<GetGradeByLevelResult> intendedGradeByParent = new List<GetGradeByLevelResult>();

            #region Get User By Intended For
            var paramUserRolePosition = new GetUserRolePositionRequest
            {
                IdAcademicYear = ay.IdAcademicYear,
                IdSchool = ay.IdSchool,
                UserRolePositions = new List<GetUserRolePosition>()
            };

            foreach (var ItemIntendedFor in body.IntendedFor)
            {
                UserRolePersonalOptionRole role = UserRolePersonalOptionRole.ALL;
                if (ItemIntendedFor.Role == "ALL")
                    role = UserRolePersonalOptionRole.ALL;
                else if (ItemIntendedFor.Role == "TEACHER")
                    role = UserRolePersonalOptionRole.TEACHER;
                else if (ItemIntendedFor.Role == "STUDENT")
                    role = UserRolePersonalOptionRole.STUDENT;
                else if (ItemIntendedFor.Role == "STAFF")
                    role = UserRolePersonalOptionRole.STAFF;
                else if (ItemIntendedFor.Role == "PARENT")
                    role = UserRolePersonalOptionRole.PARENT;

                UserRolePersonalOptionType type = UserRolePersonalOptionType.None;
                var listPersonal = new List<string>();
                var listLevel = new List<string>();
                var listHomeroom = new List<string>();

                if (ItemIntendedFor.Option == EventOptionType.None)
                    type = UserRolePersonalOptionType.None;
                else if (ItemIntendedFor.Option == EventOptionType.All)
                    type = UserRolePersonalOptionType.All;
                else if (ItemIntendedFor.Option == EventOptionType.Grade)
                    type = UserRolePersonalOptionType.Grade;
                    if (ItemIntendedFor.Role == "STUDENT")
                        listHomeroom.AddRange(ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway);
                    else if (ItemIntendedFor.Role == "PARENT")
                        listHomeroom.AddRange(ItemIntendedFor.IntendedForGradeParentIdHomeroomPathway);
                else if (ItemIntendedFor.Option == EventOptionType.Department)
                    type = UserRolePersonalOptionType.Department;
                else if (ItemIntendedFor.Option == EventOptionType.Department)
                {
                    type = UserRolePersonalOptionType.Personal;
                    if (ItemIntendedFor.Role == "TEACHER" || ItemIntendedFor.Role == "STAFF")
                        listPersonal.AddRange(ItemIntendedFor.IntendedForPersonalIdUser.ToList());
                    else if (ItemIntendedFor.Role == "STUDENT")
                        listPersonal.AddRange(ItemIntendedFor.IntendedForPersonalIdStudent.ToList());
                    else if (ItemIntendedFor.Role == "PARENT")
                        listPersonal.AddRange(ItemIntendedFor.IntendedForPersonalIdParent.ToList());
                }
                else if (ItemIntendedFor.Role == "Position")
                    type = UserRolePersonalOptionType.Position;
                else if (ItemIntendedFor.Role == "Level")
                    type = UserRolePersonalOptionType.Level;
                    if (ItemIntendedFor.Role == "STUDENT")
                        listLevel.AddRange(ItemIntendedFor.IntendedForLevelStudentIdLevel);
                    else if (ItemIntendedFor.Role == "PARENT")
                        listLevel.AddRange(ItemIntendedFor.IntendedForLevelParentIdLevel);
                var newUserRolePosition = new GetUserRolePosition
                {
                    IdUserRolePositions = ItemIntendedFor.IdIntendedFor,
                    Role = role,
                    Option = type,
                    TeacherPositions = ItemIntendedFor.IntendedForPositionIdTeacherPosition.ToList(),
                    Departemens = ItemIntendedFor.IntendedForDepartemetIdDepartemet.ToList(),
                    Level = ItemIntendedFor.IntendedForLevelStudentIdLevel.ToList(),
                    Homeroom = ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway.ToList(),
                    Personal = listPersonal
                };

                paramUserRolePosition.UserRolePositions.Add(newUserRolePosition);
            }

            var getUserRolePositionService = await _rolePositionService.GetUserRolePosition(paramUserRolePosition);
            var getUserRolePosition = getUserRolePositionService.IsSuccess ? getUserRolePositionService.Payload : null;

            if (!getUserRolePosition.Any())
                throw new BadRequestException("User by intended for is not found");
            #endregion

            var lisIdUserByUserRolePosition = getUserRolePosition.Select(e => e.IdUser).Distinct().ToList();
            var listUser = await _dbContext.Entity<MsUser>()
                                .Where(e => lisIdUserByUserRolePosition.Contains(e.Id))
                                .Select(e => e.Id)
                                .ToListAsync(CancellationToken);
            getUserRolePosition = getUserRolePosition.Where(e => listUser.Contains(e.IdUser)).ToList();

            foreach (var ItemIntendedFor in body.IntendedFor)
            {
                #region User Event
                foreach (var eventDetail in eventDetails)
                {
                    var isApproved = ItemIntendedFor.NeedParentPermission ? false : true;
                    var isNeedApproval = ItemIntendedFor.NeedParentPermission ? true : false;

                    var listUserEventDb = listUserEvent.Where(e => e.IdEventDetail==eventDetail.Id).Distinct().ToList();
                    var listIdUserDb = listUserEventDb.Select(e => e.IdUser).Distinct().ToList();
                    var listIdUserByIntended  = getUserRolePosition
                                                    .Where(e=>e.IdUserRolePositions==ItemIntendedFor.IdIntendedFor)
                                                    .Select(e=>e.IdUser)
                                                    .Distinct().ToList();

                    //remove TrUserEvent
                    var listRemoveUserEvent = listUserEventDb.Where(e=> !listIdUserByIntended.Contains(e.IdUser)).ToList();
                    listRemoveUserEvent.ForEach(e=>e.IsActive = false);
                    _dbContext.Entity<TrUserEvent>().UpdateRange(listRemoveUserEvent);

                    //active TrUserEvent
                    var listUpdateUserEvent = listUserEventDb.Where(e => listIdUserByIntended.Contains(e.IdUser)).ToList();
                    listUpdateUserEvent.ForEach(e =>
                    {
                        e.IsNeedApproval = isNeedApproval;
                        e.IsApproved = isApproved;
                        e.IsActive = true;
                    });
                    _dbContext.Entity<TrUserEvent>().UpdateRange(listUpdateUserEvent);


                    //add TrUserEvent
                    var listNewUserEvent = listIdUserByIntended.Where(e=> !listIdUserDb.Contains(e))
                                        .Select(e => new TrUserEvent
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdEventDetail = eventDetail.Id,
                                            IsApproved = isApproved,
                                            IsNeedApproval = isNeedApproval,
                                            IdUser = e
                                        })
                                        .Distinct().ToList();
                    _dbContext.Entity<TrUserEvent>().AddRange(listNewUserEvent);
                }
                #endregion

                if (ItemIntendedFor.Role == "All")
                {

                    NewIntendedFor = ListEventIntendedFor(existEvent, ItemIntendedFor, "No Option", false, false);
                    EventIntendedFor.Add(NewIntendedFor);
                }
                else
                {
                    NewIntendedFor = ListEventIntendedFor
                    (
                        existEvent,
                        ItemIntendedFor,
                        ItemIntendedFor.Option.ToString(),
                        ItemIntendedFor.Role == RoleConstant.Student ? ItemIntendedFor.SendNotificationToLevelHead : false,
                        ItemIntendedFor.Role == RoleConstant.Student ? ItemIntendedFor.NeedParentPermission : false
                    );
                    EventIntendedFor.Add(NewIntendedFor);

                    if (ItemIntendedFor.Role == RoleConstant.Staff || ItemIntendedFor.Role == RoleConstant.Teacher)
                    {
                        if (ItemIntendedFor.Option == EventOptionType.Department)
                        {
                            var NewIntendedForDepartement = ListEventIntendedForDepartment(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForDepartement.AddRange(NewIntendedForDepartement);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Position)
                        {
                            var NewIntendedForPosition = ListEventIntendedForPosition(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForPosition.AddRange(NewIntendedForPosition);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var NewIntendedForPersonal = ListEventIntendedForPersonal(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForPersonal.AddRange(NewIntendedForPersonal);
                        }
                    }
                    else if (ItemIntendedFor.Role == RoleConstant.Parent)
                    {
                        if (ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var substringedIdParents = new List<string>();
                            foreach (var item in ItemIntendedFor.IntendedForPersonalIdParent)
                            {
                                substringedIdParents.Add(item.Substring(1));
                            }

                            intendedPersonalIdParents = await _dbContext.Entity<MsStudentParent>().Where(x => substringedIdParents.Contains(x.IdStudent)).Select(x => x.IdParent).ToListAsync(CancellationToken);

                            if (!intendedPersonalIdParents.Any())
                                throw new NotFoundException("Parent for event intended personal not found");

                            var NewIntendedForPersonalParent = ListEventIntendedForPersonalParent(NewIntendedFor, intendedPersonalIdParents);
                            EventIntendedForPersonalParent.AddRange(NewIntendedForPersonalParent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Level)
                        {
                            var NewIntendedForLevelParent = ListEventIntendedForLevelParent(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForLevelParent.AddRange(NewIntendedForLevelParent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Grade)
                        {
                            intendedGradeByParent = await _dbContext.Entity<MsHomeroom>()
                                .Include(x => x.Grade)
                                .Where(x => ItemIntendedFor.IntendedForGradeParentIdHomeroomPathway.Contains(x.Id))
                                .Select(x => new GetGradeByLevelResult
                                {
                                    IdHomeroom = x.Id,
                                    IdLevel = x.Grade.IdLevel
                                })
                                .ToListAsync(CancellationToken);


                            var NewIntendedForGradeParent = ListEventIntendedForGradeParent(NewIntendedFor, ItemIntendedFor, intendedGradeByParent);
                            EventIntendedForGradeParent.AddRange(NewIntendedForGradeParent);
                        }
                    }
                    else if (ItemIntendedFor.Role == RoleConstant.Student)
                    {
                        if (ItemIntendedFor.Option == EventOptionType.Level)
                        {
                            var NewIntendedForLevelStudent = ListEventIntendedForLevelStudent(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForLevelStudent.AddRange(NewIntendedForLevelStudent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Grade)
                        {
                            var NewIntendedForGradeStudent = ListEventIntendedForGradeStudent(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForGradeStudent.AddRange(NewIntendedForGradeStudent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var NewIntendedForPersonalStudent = ListEventIntendedForPersonalStudent(NewIntendedFor, ItemIntendedFor);
                            EventIntendedForPersonalStudent.AddRange(NewIntendedForPersonalStudent);
                        }

                        #region Attendent Setting
                        if (body.IsSetAttendance)
                        {
                            var newEventIntendedForAttendanceStudent = new TrEventIntendedForAttendanceStudent
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventIntendedFor = NewIntendedFor.Id,
                                Type = body.MandatoryType,
                                IsSetAttendance = body.IsSetAttendance,
                                IsRepeat = body.IsAttendanceRepeat,
                            };
                            EventIntendedForAttendanceStudent.Add(newEventIntendedForAttendanceStudent);

                            #region Attandance PIC
                            List<string> GetUserId = new List<string>();

                            if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom || body.AttandancePIC == EventIntendedForAttendancePICStudent.Staff)
                            {
                                if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Level)
                                {
                                    var NewUserId = await _dbContext.Entity<MsHomeroomTeacher>()
                                        .Include(e => e.Homeroom)
                                        .ThenInclude(e => e.Grade)
                                        .ThenInclude(e => e.Level)
                                        .Where(e => ItemIntendedFor.IntendedForLevelStudentIdLevel.Contains(e.Homeroom.Grade.Level.Id))
                                        .Select(e => e.IdBinusian).ToListAsync(CancellationToken);
                                    GetUserId.AddRange(NewUserId);
                                }
                                if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Grade)
                                {
                                    var NewUserId = await (from a in _dbContext.Entity<MsHomeroomTeacher>()
                                                           join b in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals b.Id
                                                           join c in _dbContext.Entity<MsHomeroomPathway>() on a.IdHomeroom equals c.Id
                                                           where ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway.Contains(c.Id)
                                                           select a.IdBinusian).ToListAsync(CancellationToken);
                                    GetUserId.AddRange(NewUserId);
                                }
                                else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Staff)
                                {
                                    var NewUserId = await (from a in _dbContext.Entity<MsStaff>()
                                                           join b in _dbContext.Entity<MsUser>() on a.IdBinusian equals b.Id
                                                           join c in _dbContext.Entity<MsUserSchool>() on b.Id equals c.IdUser
                                                           where c.IdSchool == ay.IdSchool
                                                           select a.IdBinusian).ToListAsync(CancellationToken);
                                    GetUserId.AddRange(NewUserId);
                                }

                                foreach (var IdUser in GetUserId)
                                {
                                    var newEventIntendedForAttendancePIC = new TrEventIntendedForAtdPICStudent
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                                        Type = body.AttandancePIC,
                                        IdUser = IdUser,
                                    };

                                    EventIntendedForAttendancePIC.Add(newEventIntendedForAttendancePIC);
                                }
                            }
                            else
                            {
                                var newEventIntendedForAttendancePIC = new TrEventIntendedForAtdPICStudent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                                    Type = body.AttandancePIC,
                                    IdUser = body.AttandancePIC == EventIntendedForAttendancePICStudent.EventCoordinator ? body.IdUserCoordinator : body.AttandancePICIdUser,
                                };

                                EventIntendedForAttendancePIC.Add(newEventIntendedForAttendancePIC);
                            }
                            #endregion

                            // #region Attandance Checked
                            // var eachDates = body.Dates.SelectMany(x => DateTimeUtil.ToEachDay(x.Start, x.End));
                            // if (body.IsAttendanceRepeat)
                            // {
                            //     foreach (var eachDate in eachDates)
                            //     {
                            //         foreach (var attCheck in body.AttandanceCheck)
                            //         {
                            //             var attCheckStudent = new TrEventIntendedForAttendanceCheckStudent
                            //             {
                            //                 Id = Guid.NewGuid().ToString(),
                            //                 IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                            //                 StartDate = eachDate.start,
                            //                 StartTime = eachDate.start.TimeOfDay,
                            //                 EndTime = eachDate.end.TimeOfDay,
                            //                 CheckName = attCheck.Name,
                            //                 Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
                            //                 IsPrimary = attCheck.IsPrimary
                            //             };
                            //             IntendedForAttendanceCheckStudent.Add(attCheckStudent);
                            //         }
                            //     }
                            // }
                            // else
                            // {
                            //     foreach (var attCheck in body.AttandanceCheck)
                            //     {
                            //         var eachDate = eachDates.SingleOrDefault(e => e.start == attCheck.Startdate);
                            //         var attCheckStudent = new TrEventIntendedForAttendanceCheckStudent
                            //         {
                            //             Id = Guid.NewGuid().ToString(),
                            //             IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                            //             StartDate = eachDate.start,
                            //             StartTime = eachDate.start.TimeOfDay,
                            //             EndTime = eachDate.end.TimeOfDay,
                            //             CheckName = attCheck.Name,
                            //             Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
                            //             IsPrimary = attCheck.IsPrimary
                            //         };
                            //         IntendedForAttendanceCheckStudent.Add(attCheckStudent);
                            //     }
                            // }
                            // #endregion
                        }
                        #endregion
                    }


                }
            }

            _dbContext.Entity<TrEventIntendedFor>().AddRange(EventIntendedFor);
            _dbContext.Entity<TrEventIntendedForDepartment>().AddRange(EventIntendedForDepartement);
            _dbContext.Entity<TrEventIntendedForPosition>().AddRange(EventIntendedForPosition);
            _dbContext.Entity<TrEventIntendedForPersonal>().AddRange(EventIntendedForPersonal);
            _dbContext.Entity<TrEventIntendedForPersonalParent>().AddRange(EventIntendedForPersonalParent);
            _dbContext.Entity<TrEventIntendedForPersonalStudent>().AddRange(EventIntendedForPersonalStudent);
            _dbContext.Entity<TrEventIntendedForGradeStudent>().AddRange(EventIntendedForGradeStudent);
            _dbContext.Entity<TrEventIntendedForLevelStudent>().AddRange(EventIntendedForLevelStudent);
            _dbContext.Entity<TrEventIntendedForAttendanceStudent>().AddRange(EventIntendedForAttendanceStudent);
            // _dbContext.Entity<TrEventIntendedForAttendanceCheckStudent>().AddRange(IntendedForAttendanceCheckStudent);
            _dbContext.Entity<TrEventIntendedForAtdPICStudent>().AddRange(EventIntendedForAttendancePIC);
            #endregion
            // }
            #endregion

            #region Event Coordinator
            var GetCoordinator = _dbContext.Entity<TrEventCoordinator>()
            .SingleOrDefault(e => e.IdEvent == existEvent.Id);

            #region History Coordinator
            if (GetCoordinator != null)
            {
                var newHTrEventCoordinator = new HTrEventCoordinator
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdUser = GetCoordinator.IdUser,
                };
                _dbContext.Entity<HTrEventCoordinator>().Add(newHTrEventCoordinator);
            }
            #endregion

            #region Update Coordinator
            // if (existEvent.StatusEvent == "Declined")
            // {
            if (GetCoordinator == null)
            {
                var newEventCoordinator = new TrEventCoordinator
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = existEvent.Id,
                    IdUser = body.IdUserCoordinator,
                };
                _dbContext.Entity<TrEventCoordinator>().Add(newEventCoordinator);
            }
            if (GetCoordinator.IdUser != body.IdUserCoordinator)
            {
                GetCoordinator.IdUser = body.IdUserCoordinator;
                _dbContext.Entity<TrEventCoordinator>().Update(GetCoordinator);
            }
            // }
            #endregion
            #endregion

            #region Event Budget
            var GetBudget = await _dbContext.Entity<TrEventBudget>()
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            #region History Budget
            foreach (var ItemBudget in GetBudget)
            {
                var newHTrEventBudget = new HTrEventBudget
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    Name = ItemBudget.Name,
                    Amount = ItemBudget.Amount,
                };
                _dbContext.Entity<HTrEventBudget>().Add(newHTrEventBudget);
            }
            #endregion

            #region Update & Create Budget
            // if (existEvent.StatusEvent == "Declined")
            // {
            foreach (var ItemBudget in GetBudget)
            {
                var GetBodyBudget = body.Budget.Where(e => e.Name == ItemBudget.Name).SingleOrDefault();
                if (GetBodyBudget != null)
                {
                    ItemBudget.Amount = GetBodyBudget.Amount;
                }
                else
                {
                    ItemBudget.IsActive = false;
                }
                _dbContext.Entity<TrEventBudget>().Update(ItemBudget);
            }

            List<TrEventBudget> EventBudget = new List<TrEventBudget>();
            foreach (var ItemBudget in body.Budget)
            {
                var GetDbBudget = GetBudget.Any(e => e.Name == ItemBudget.Name);
                if (!GetDbBudget)
                {
                    var newEventBudget = new TrEventBudget
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        Name = ItemBudget.Name,
                        Amount = ItemBudget.Amount,
                    };
                    EventBudget.Add(newEventBudget);
                }
            }
            _dbContext.Entity<TrEventBudget>().AddRange(EventBudget);
            // }
            #endregion
            #endregion

            #region Event Attachmant
            var GetAttachmant = await _dbContext.Entity<TrEventAttachment>()
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            #region History Attachmant
            foreach (var ItemAttachmant in GetAttachmant)
            {
                var newHTrEventAttachment = new HTrEventAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    Url = ItemAttachmant.Url,
                    Filename = ItemAttachmant.Filename,
                    Filetype = ItemAttachmant.Filetype,
                };
                _dbContext.Entity<HTrEventAttachment>().Add(newHTrEventAttachment);
            }
            #endregion

            #region Update & Create Attachment
            // if (existEvent.StatusEvent == "Declined")
            // {
            foreach (var ItemAttachmant in GetAttachmant)
            {
                var ExsisBodyAttachmant = body.AttachmentBudget.Any(e => e.IdAttachmant == ItemAttachmant.Id);
                if (!ExsisBodyAttachmant)
                {
                    ItemAttachmant.IsActive = false;
                    _dbContext.Entity<TrEventAttachment>().Update(ItemAttachmant);
                }
            }

            List<TrEventAttachment> EventAttachment = new List<TrEventAttachment>();
            foreach (var ItemAttach in body.AttachmentBudget)
            {
                var GetDbAttach = GetAttachmant.Any(e => e.Id == ItemAttach.IdAttachmant);
                if (!GetDbAttach)
                {
                    var newEventAttachment = new TrEventAttachment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        Url = ItemAttach.Url,
                        Filename = ItemAttach.Filename,
                        Filetype = ItemAttach.Filetype,
                        Filesize = ItemAttach.Filesize,
                    };
                    EventAttachment.Add(newEventAttachment);
                }
            }
            _dbContext.Entity<TrEventAttachment>().AddRange(EventAttachment);
            // }
            #endregion
            #endregion

            #region Event Activity
            var GetActivity = await _dbContext.Entity<TrEventActivity>()
                .Include(e => e.EventActivityRegistrants)
                .Include(e => e.EventActivityPICs)
                .Include(e => e.EventActivityAwards)
                .Include(e => e.EventActivityAwardTeachers)
                .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemActivity in GetActivity)
            {
                #region History Activity
                var newHTrEventActivity = new HTrEventActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdActivity = ItemActivity.IdActivity,
                };
                _dbContext.Entity<HTrEventActivity>().Add(newHTrEventActivity);
                #endregion

                #region History PIC
                foreach (var ItemActivityPIC in ItemActivity.EventActivityPICs)
                {
                    var newHTrEventActivityPIC = new HTrEventActivityPIC
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdUser = ItemActivityPIC.IdUser,
                    };
                    _dbContext.Entity<HTrEventActivityPIC>().Add(newHTrEventActivityPIC);
                }
                #endregion

                #region History Regristant
                foreach (var ItemActivityRegistrants in ItemActivity.EventActivityRegistrants)
                {
                    var newHTrEventActivityRegristrant = new HTrEventActivityRegistrant
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdUser = ItemActivityRegistrants.IdUser,
                    };
                    _dbContext.Entity<HTrEventActivityRegistrant>().Add(newHTrEventActivityRegristrant);
                }
                #endregion

                #region History Award
                foreach (var ItemActivityAward in ItemActivity.EventActivityAwards)
                {
                    var newHTrEventActivityAward = new HTrEventActivityAward
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdAward = ItemActivityAward.IdAward,
                        IdHomeroomStudent = ItemActivityAward.IdHomeroomStudent,
                        Url = ItemActivityAward.Url,
                        Filename = ItemActivityAward.Filename,
                        Filetype = ItemActivityAward.Filetype,
                        Filesize = ItemActivityAward.Filesize,
                        OriginalFilename = ItemActivityAward.OriginalFilename
                    };

                    _dbContext.Entity<HTrEventActivityAward>().Add(newHTrEventActivityAward);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }
                #endregion

                #region History Award Teacher
                foreach (var ItemActivityAwardTeacher in ItemActivity.EventActivityAwardTeachers)
                {
                    var newHTrEventActivityAwardTeacher = new HTrEventActivityAwardTeacher
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdAward = ItemActivityAwardTeacher.IdAward,
                        IdStaff = ItemActivityAwardTeacher.IdStaff,
                        Url = ItemActivityAwardTeacher.Url,
                        Filename = ItemActivityAwardTeacher.Filename,
                        Filetype = ItemActivityAwardTeacher.Filetype,
                        Filesize = ItemActivityAwardTeacher.Filesize,
                        OriginalFilename = ItemActivityAwardTeacher.OriginalFilename
                    };

                    _dbContext.Entity<HTrEventActivityAwardTeacher>().Add(newHTrEventActivityAwardTeacher);
                    await _dbContext.SaveChangesAsync(CancellationToken);
                }
                #endregion

                #region Update Activity
                var ExsisBodyActivity = body.Activity.Any(e => e.Id == ItemActivity.Id);
                if (!ExsisBodyActivity)
                {
                    ItemActivity.IsActive = false;
                    _dbContext.Entity<TrEventActivity>().Update(ItemActivity);

                    var GetActivityRegristrant = ItemActivity.EventActivityRegistrants.ToList();
                    GetActivityRegristrant.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityRegistrant>().UpdateRange(GetActivityRegristrant);

                    var GetActivityPIC = ItemActivity.EventActivityPICs.ToList();
                    GetActivityPIC.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityPIC>().UpdateRange(GetActivityPIC);

                    var GetActivityAward = ItemActivity.EventActivityAwards.ToList();
                    GetActivityAward.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityAward>().UpdateRange(GetActivityAward);

                    var GetActivityAwardTeacher = ItemActivity.EventActivityAwardTeachers.ToList();
                    GetActivityAwardTeacher.ForEach(e => e.IsActive = false);
                    _dbContext.Entity<TrEventActivityAwardTeacher>().UpdateRange(GetActivityAwardTeacher);
                }
                else
                {
                    foreach (var itemAcyivityPIC in ItemActivity.EventActivityPICs)
                    {
                        var ExsisPIC = body.Activity.Any(e => e.EventActivityPICIdUser.Contains(itemAcyivityPIC.IdUser));
                        if (!ExsisPIC)
                        {
                            itemAcyivityPIC.IsActive = false;
                            _dbContext.Entity<TrEventActivityPIC>().Update(itemAcyivityPIC);
                        }
                    }

                    foreach (var itemAcyivityRegristrant in ItemActivity.EventActivityRegistrants)
                    {
                        var ExsisRegristrant = body.Activity.Any(e => e.EventActivityRegistrantIdUser.Contains(itemAcyivityRegristrant.IdUser));
                        if (!ExsisRegristrant)
                        {
                            itemAcyivityRegristrant.IsActive = false;
                            _dbContext.Entity<TrEventActivityRegistrant>().Update(itemAcyivityRegristrant);
                        }
                    }

                    foreach (var itemAcyivityAward in ItemActivity.EventActivityAwards)
                    {
                        itemAcyivityAward.IsActive = false;
                        _dbContext.Entity<TrEventActivityAward>().Update(itemAcyivityAward);
                    }

                    foreach (var itemAcyivityAwardTeacher in ItemActivity.EventActivityAwardTeachers)
                    {
                        itemAcyivityAwardTeacher.IsActive = false;
                        _dbContext.Entity<TrEventActivityAwardTeacher>().Update(itemAcyivityAwardTeacher);
                    }

                }
                #endregion
            }

            #region Create Event Activity
            List<TrEventActivity> EventActivity = new List<TrEventActivity>();
            List<TrEventActivityRegistrant> EventActivityRegristrant = new List<TrEventActivityRegistrant>();
            List<TrEventActivityPIC> EventActivityPIC = new List<TrEventActivityPIC>();
            List<TrEventActivityAward> EventActivityAward = new List<TrEventActivityAward>();
            List<TrEventActivityAwardTeacher> EventActivityAwardTeacher = new List<TrEventActivityAwardTeacher>();

            foreach (var ItemEventActivity in body.Activity)
            {
                var GetEventActivity = GetActivity.SingleOrDefault(e => e.Id == ItemEventActivity.Id);
                if (GetEventActivity == null)
                {
                    var newEventActivity = new TrEventActivity
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        IdActivity = ItemEventActivity.IdActivity,
                    };
                    EventActivity.Add(newEventActivity);
                    ItemEventActivity.Id = newEventActivity.Id;

                    #region Event Activity PIC
                    foreach (var IdUser in ItemEventActivity.EventActivityPICIdUser)
                    {
                        var newEventActivityPIC = new TrEventActivityPIC
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdEventActivity = ItemEventActivity.Id,
                            IdUser = IdUser,
                        };
                        EventActivityPIC.Add(newEventActivityPIC);
                    }
                    #endregion

                    #region Event Activity Registrant
                    foreach (var IdUser in ItemEventActivity.EventActivityRegistrantIdUser)
                    {
                        var newEventActivityRegristrant = new TrEventActivityRegistrant
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdEventActivity = ItemEventActivity.Id,
                            IdUser = IdUser,
                        };
                        EventActivityRegristrant.Add(newEventActivityRegristrant);
                    }
                    #endregion


                }
                else
                {
                    GetEventActivity.IdActivity = ItemEventActivity.IdActivity;
                    _dbContext.Entity<TrEventActivity>().Update(GetEventActivity);

                    foreach (var ItemEventActivityPIC in ItemEventActivity.EventActivityPICIdUser)
                    {
                        var ActivityPICUserId = GetEventActivity.EventActivityPICs.Where(e => e.IdUser == ItemEventActivityPIC).SingleOrDefault();

                        if (ActivityPICUserId == null)
                        {
                            var newEventActivityPIC = new TrEventActivityPIC
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventActivity = ItemEventActivity.Id,
                                IdUser = ItemEventActivityPIC,
                            };
                            EventActivityPIC.Add(newEventActivityPIC);
                        }

                    }

                    foreach (var ItemEventActivityRegristant in ItemEventActivity.EventActivityRegistrantIdUser)
                    {
                        var ActivityRegristantUserId = GetEventActivity.EventActivityRegistrants.Where(e => e.IdUser == ItemEventActivityRegristant).SingleOrDefault();

                        if (ActivityRegristantUserId == null)
                        {
                            var newEventActivityRegristrant = new TrEventActivityRegistrant
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventActivity = ItemEventActivity.Id,
                                IdUser = ItemEventActivityRegristant,
                            };
                            EventActivityRegristrant.Add(newEventActivityRegristrant);
                        }

                    }
                }

                #region Activity Award
                string[] fileAward = { ".pdf", ".jpg", ".png", ".docx", ".jpeg" };
                if (ItemEventActivity.EventActivityAwardIdUser.Count > 0)
                {
                    foreach (var ItemInvolvmentAward in ItemEventActivity.EventActivityAwardIdUser)
                    {
                        foreach (var ItemAward in ItemInvolvmentAward.IdAward)
                        {
                            if (!string.IsNullOrEmpty(ItemInvolvmentAward.Filetype) && !fileAward.Any(x => x == ItemInvolvmentAward.Filetype))
                            {
                                throw new BadRequestException("Invalid file type. Allowed types: .pdf, .jpg, .png, .docx, .jpeg");
                            }
                            else
                            {
                                var newEventActivityAward = new TrEventActivityAward
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdEventActivity = ItemEventActivity.Id,
                                    IdAward = ItemAward,
                                    IdHomeroomStudent = ItemInvolvmentAward.IdHomeroomStudent,
                                    Url = ItemInvolvmentAward.Url,
                                    Filename = ItemInvolvmentAward.Filename,
                                    Filetype = ItemInvolvmentAward.Filetype,
                                    Filesize = ItemInvolvmentAward.Filesize,
                                    OriginalFilename = ItemInvolvmentAward.OriginalFilename
                                };
                                EventActivityAward.Add(newEventActivityAward);
                            }
                        }
                    }
                    existEvent.StatusEventAward = "On Review (1)";
                    existEvent.DescriptionEventAward = "Record of Involvement is On Review";
                }

                if (ItemEventActivity.EventActivityAwardTeacherIdUser.Count > 0)
                {
                    foreach (var ItemInvolvmentAwardTeacher in ItemEventActivity.EventActivityAwardTeacherIdUser)
                    {
                        foreach (var ItemAward in ItemInvolvmentAwardTeacher.IdAward)
                        {
                            if (!string.IsNullOrEmpty(ItemInvolvmentAwardTeacher.Filetype) && !fileAward.Any(x => x == ItemInvolvmentAwardTeacher.Filetype))
                            {
                                throw new BadRequestException("Invalid file type. Allowed types: .pdf, .jpg, .png, .docx, .jpeg");
                            }
                            else
                            {
                                var newEventActivityAwardTeacher = new TrEventActivityAwardTeacher
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdEventActivity = ItemEventActivity.Id,
                                    IdAward = ItemAward,
                                    IdStaff = ItemInvolvmentAwardTeacher.IdStaff,
                                    Url = ItemInvolvmentAwardTeacher.Url,
                                    Filename = ItemInvolvmentAwardTeacher.Filename,
                                    Filetype = ItemInvolvmentAwardTeacher.Filetype,
                                    Filesize = ItemInvolvmentAwardTeacher.Filesize,
                                    OriginalFilename = ItemInvolvmentAwardTeacher.OriginalFilename
                                };
                                EventActivityAwardTeacher.Add(newEventActivityAwardTeacher);
                            }
                        }
                    }
                    existEvent.StatusEventAward = "On Review (1)";
                    existEvent.DescriptionEventAward = "Record of Involvement is On Review";
                }
                #endregion
            }
            _dbContext.Entity<TrEvent>().Update(existEvent);
            _dbContext.Entity<TrEventActivity>().AddRange(EventActivity);
            _dbContext.Entity<TrEventActivityPIC>().AddRange(EventActivityPIC);
            _dbContext.Entity<TrEventActivityRegistrant>().AddRange(EventActivityRegristrant);
            _dbContext.Entity<TrEventActivityAward>().AddRange(EventActivityAward);
            _dbContext.Entity<TrEventActivityAwardTeacher>().AddRange(EventActivityAwardTeacher);
            #endregion
            #endregion

            #region Approval Event
            var GetApprovalEvent = await _dbContext.Entity<TrEventApprover>()
                    .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            #region History Approver Event
            foreach (var ItemApprovalEvent in GetApprovalEvent)
            {
                var newEventApprover = new HTrEventApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdUser = ItemApprovalEvent.IdUser,
                };
                _dbContext.Entity<HTrEventApprover>().Add(newEventApprover);
            }
            #endregion

            #region Update Approval Event
            if (existEvent.StatusEvent == "Declined")
            {
                List<TrEventApprover> EventAprover = new List<TrEventApprover>();
                GetApprovalEvent.ForEach(e => e.IsActive = false);
                GetApprovalEvent.ForEach(e => e.IsActive = false);

                var newEventApprover = new TrEventApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = existEvent.Id,
                    IdUser = body.IdUserEventApproval1,
                    OrderNumber = 1
                };
                EventAprover.Add(newEventApprover);

                if (!string.IsNullOrEmpty(body.IdUserEventApproval2))
                {
                    newEventApprover = new TrEventApprover
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        IdUser = body.IdUserEventApproval2,
                        OrderNumber = 2
                    };
                    EventAprover.Add(newEventApprover);
                }
                _dbContext.Entity<TrEventApprover>().AddRange(newEventApprover);

            }
            #endregion
            #endregion

            #region Approval Award

            #region History Approver Event
            var GetApprovalAward = await _dbContext.Entity<TrEventAwardApprover>()
                    .Where(e => e.IdEvent == existEvent.Id).ToListAsync(CancellationToken);

            foreach (var ItemApprovalAward in GetApprovalAward)
            {
                var newAwardApprover = new HTrEventAwardApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdUser = ItemApprovalAward.IdUser,
                };
                _dbContext.Entity<HTrEventAwardApprover>().Add(newAwardApprover);
            }
            #endregion

            // #region Update Approval Award
            // if (existEvent.StatusEvent == "Declined")
            // {
            GetApprovalAward.ForEach(e => e.IsActive = false);
            List<TrEventAwardApprover> AwardAproval = new List<TrEventAwardApprover>();

            if (!string.IsNullOrEmpty(body.IdUserAwardApproval1))
            {
                var newAwardApproval = new TrEventAwardApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = existEvent.Id,
                    IdUser = body.IdUserAwardApproval1,
                    OrderNumber = 1

                };
                AwardAproval.Add(newAwardApproval);
            }


            if (!string.IsNullOrEmpty(body.IdUserAwardApproval2))
            {
                var newAwardApproval = new TrEventAwardApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = existEvent.Id,
                    IdUser = body.IdUserAwardApproval2,
                    OrderNumber = 2
                };
                AwardAproval.Add(newAwardApproval);
            }

            _dbContext.Entity<TrEventAwardApprover>().AddRange(AwardAproval);
            // }
            // #endregion
            #endregion

            #region History Event Approval
            if (existEvent.StatusEvent == "Declined")
            {
                List<HTrEventApproval> EventAproval = new List<HTrEventApproval>();

                var newEventApproval = new HTrEventApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = existEvent.Id,
                    Section = "Event",
                    State = 1,
                    IdUser = body.IdUserEventApproval1,
                };
                EventAproval.Add(newEventApproval);

                if (!string.IsNullOrEmpty(body.IdUserEventApproval2))
                {
                    newEventApproval = new HTrEventApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        Section = "Event",
                        State = 2,
                        IdUser = body.IdUserEventApproval2,
                    };
                    EventAproval.Add(newEventApproval);
                }
                _dbContext.Entity<HTrEventApproval>().AddRange(EventAproval);
            }

            #endregion

            #region History Event Award Approval
            if (existEvent.StatusEvent == "Declined")
            {
                var EventAwardAproval = new List<HTrEventApproval>();
                var newEventAwardApproval = new HTrEventApproval();
                if (!string.IsNullOrEmpty(body.IdUserAwardApproval1))
                {
                    newEventAwardApproval = new HTrEventApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        Section = "Award",
                        State = 1,
                        IdUser = body.IdUserAwardApproval1,
                    };
                    EventAwardAproval.Add(newEventAwardApproval);
                }

                if (!string.IsNullOrEmpty(body.IdUserAwardApproval2))
                {
                    newEventAwardApproval = new HTrEventApproval
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = existEvent.Id,
                        Section = "Event",
                        State = 2,
                        IdUser = body.IdUserAwardApproval2,
                    };
                    EventAwardAproval.Add(newEventAwardApproval);
                }
                _dbContext.Entity<HTrEventApproval>().AddRange(EventAwardAproval);
            }
            #endregion

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
        public async Task<List<string>> GetIdUserPIC(UpdateSchoolEventRequest body, SchoolEventIntendedFor ItemIntendedFor, string IdSchool)
        {
            List<string> GetUserId = null;

            if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Level)
            {
                var NewUserId = await _dbContext.Entity<MsHomeroomTeacher>()
                    .Include(e => e.Homeroom)
                    .ThenInclude(e => e.Grade)
                    .ThenInclude(e => e.Level)
                    .Where(e => ItemIntendedFor.IntendedForLevelStudentIdLevel.Contains(e.Homeroom.Grade.Level.Id))
                    .Select(e => e.IdBinusian).ToListAsync(CancellationToken);
                GetUserId.AddRange(NewUserId);
            }
            if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Grade)
            {
                var NewUserId = await (from a in _dbContext.Entity<MsHomeroomTeacher>()
                                       join b in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals b.Id
                                       join c in _dbContext.Entity<MsHomeroomPathway>() on a.IdHomeroom equals c.Id
                                       where ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway.Contains(c.Id)
                                       select a.IdBinusian).ToListAsync(CancellationToken);
                GetUserId.AddRange(NewUserId);
            }
            else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Staff)
            {
                var NewUserId = await (from a in _dbContext.Entity<MsStaff>()
                                       join b in _dbContext.Entity<MsUser>() on a.IdBinusian equals b.Id
                                       join c in _dbContext.Entity<MsUserSchool>() on b.Id equals c.IdUser
                                       where c.IdSchool == IdSchool
                                       select a.IdBinusian).ToListAsync(CancellationToken);
                GetUserId.AddRange(NewUserId);
            }

            return GetUserId;
        }
        public TrEventIntendedFor ListEventIntendedFor(TrEvent newEvent, SchoolEventIntendedFor ItemIntendedFor, string option, bool SendNotificationToLevelHead, bool NeedParentPermission)
        {
            var NewEventIntendedFor = new TrEventIntendedFor
            {
                Id = Guid.NewGuid().ToString(),
                IdEvent = newEvent.Id,
                IntendedFor = ItemIntendedFor.Role,
                Option = option,
                SendNotificationToLevelHead = ItemIntendedFor.SendNotificationToLevelHead,
                NeedParentPermission = ItemIntendedFor.NeedParentPermission,
                NoteToParent = ItemIntendedFor.NoteToParent
            };

            return NewEventIntendedFor;
        }
        public List<TrEventIntendedForDepartment> ListEventIntendedForDepartment(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForDepartement = new List<TrEventIntendedForDepartment>();
            foreach (var IdDepartment in ItemIntendedFor.IntendedForDepartemetIdDepartemet)
            {
                var NewEventIntendedForDepartement = new TrEventIntendedForDepartment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdDepartment = IdDepartment,
                };

                EventIntendedForDepartement.Add(NewEventIntendedForDepartement);
            }
            return EventIntendedForDepartement;
        }
        public List<TrEventIntendedForPosition> ListEventIntendedForPosition(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForPosition = new List<TrEventIntendedForPosition>();
            foreach (var IdTeacherPosition in ItemIntendedFor.IntendedForPositionIdTeacherPosition)
            {
                var NewEventIntendedForPosition = new TrEventIntendedForPosition
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdTeacherPosition = IdTeacherPosition,
                };

                EventIntendedForPosition.Add(NewEventIntendedForPosition);
            }
            return EventIntendedForPosition;
        }
        public List<TrEventIntendedForPersonal> ListEventIntendedForPersonal(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForPersonal = new List<TrEventIntendedForPersonal>();
            foreach (var IdUser in ItemIntendedFor.IntendedForPersonalIdUser)
            {
                var NewEventIntendedForPersonal = new TrEventIntendedForPersonal
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdUser = IdUser,
                };

                EventIntendedForPersonal.Add(NewEventIntendedForPersonal);
            }
            return EventIntendedForPersonal;
        }
        public List<TrEventIntendedForPersonalParent> ListEventIntendedForPersonalParent(TrEventIntendedFor NewIntendedFor, List<string> idParents)
        {
            var EventIntendedForPersonalParent = new List<TrEventIntendedForPersonalParent>();
            foreach (var idParent in idParents)
            {
                var NewEventIntendedForPersonalParent = new TrEventIntendedForPersonalParent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdParent = idParent,
                };

                EventIntendedForPersonalParent.Add(NewEventIntendedForPersonalParent);

            }
            return EventIntendedForPersonalParent;
        }

        public List<TrEventIntendedForGradeParent> ListEventIntendedForLevelParent(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForLevelParent = new List<TrEventIntendedForGradeParent>();
            foreach (var IdLevel in ItemIntendedFor.IntendedForLevelParentIdLevel)
            {
                var NewEventIntendedForLevelParent = new TrEventIntendedForGradeParent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdLevel = IdLevel
                };

                EventIntendedForLevelParent.Add(NewEventIntendedForLevelParent);
            }
            return EventIntendedForLevelParent;
        }

        public List<TrEventIntendedForGradeParent> ListEventIntendedForGradeParent(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor, List<GetGradeByLevelResult> levelByHomeroom)
        {
            var EventIntendedForGradeParent = new List<TrEventIntendedForGradeParent>();
            foreach (var IdHomeroomPathway in ItemIntendedFor.IntendedForGradeParentIdHomeroomPathway)
            {
                var NewEventIntendedForGradeParent = new TrEventIntendedForGradeParent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdHomeroom = IdHomeroomPathway,
                    IdLevel = levelByHomeroom.FirstOrDefault(x => x.IdHomeroom == IdHomeroomPathway).IdLevel
                };

                EventIntendedForGradeParent.Add(NewEventIntendedForGradeParent);
            }
            return EventIntendedForGradeParent;
        }

        public List<TrEventIntendedForPersonalStudent> ListEventIntendedForPersonalStudent(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForPersonalStudent = new List<TrEventIntendedForPersonalStudent>();
            foreach (var IdStudent in ItemIntendedFor.IntendedForPersonalIdStudent)
            {
                var NewEventIntendedForPersonalStudent = new TrEventIntendedForPersonalStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdStudent = IdStudent,
                };

                EventIntendedForPersonalStudent.Add(NewEventIntendedForPersonalStudent);
            }
            return EventIntendedForPersonalStudent;
        }
        public List<TrEventIntendedForGradeStudent> ListEventIntendedForGradeStudent(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForGradeStudent = new List<TrEventIntendedForGradeStudent>();
            foreach (var IdHomeroomPathway in ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway)
            {
                var NewEventIntendedForGradeStudent = new TrEventIntendedForGradeStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdHomeroom = IdHomeroomPathway,
                };

                EventIntendedForGradeStudent.Add(NewEventIntendedForGradeStudent);
            }
            return EventIntendedForGradeStudent;
        }
        public List<TrEventIntendedForLevelStudent> ListEventIntendedForLevelStudent(TrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var EventIntendedForLevelStudent = new List<TrEventIntendedForLevelStudent>();
            foreach (var IdLevel in ItemIntendedFor.IntendedForLevelStudentIdLevel)
            {
                var NewEventIntendedForLevelStudent = new TrEventIntendedForLevelStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdLevel = IdLevel,
                };

                EventIntendedForLevelStudent.Add(NewEventIntendedForLevelStudent);
            }
            return EventIntendedForLevelStudent;
        }
    }
}
