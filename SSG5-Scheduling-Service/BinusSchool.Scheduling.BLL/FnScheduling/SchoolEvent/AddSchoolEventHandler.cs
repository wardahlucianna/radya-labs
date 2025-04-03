using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Scheduling.FnSchedule.Award.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using NPOI.SS.Formula.Functions;
using Newtonsoft.Json;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using Microsoft.Azure.Documents.SystemFunctions;
using NPOI.XWPF.UserModel;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using System.Security.Policy;
using HandlebarsDotNet.StringUtils;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class AddSchoolEventHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRolePosition _rolePositionService;

        public AddSchoolEventHandler(ISchedulingDbContext dbContext, IRolePosition rolePositionService)
        {
            _dbContext = dbContext;
            _rolePositionService = rolePositionService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddSchoolEventRequest, AddSchoolEventValidator>();

            var ay = await _dbContext.Entity<MsEventType>()
                .Where(x => x.Id == body.IdEventType)
                .Select(x => new { x.IdAcademicYear, x.AcademicYear.IdSchool })
                .FirstOrDefaultAsync(CancellationToken);

            if (ay is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.IdEventType));

            #region Save Event
            var newEvent = new TrEvent
            {
                Id = Guid.NewGuid().ToString(),
                IdAcademicYear = ay.IdAcademicYear,
                IdEventType = body.IdEventType,
                Name = body.EventName,
                IsShowOnCalendarAcademic = body.IsShowOnCalendarAcademic,
                IsShowOnSchedule = body.IsShowOnSchedule,
                Objective = body.EventObjective,
                Place = body.EventPlace,
                EventLevel = (EventLevel)body.EventLevel,
                IdCertificateTemplate = (!string.IsNullOrEmpty(body.IdCertificateTemplate)) ? body.IdCertificateTemplate : null,
                StatusEvent = "On Review (1)",
                StatusEventAward = "Approved",
                DescriptionEvent = "Event Settings is On Review",
            };
            #endregion

            #region Event Detail
            var eventDetails = new List<TrEventDetail>(body.Dates.Count());

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
                    IdEvent = newEvent.Id,
                    StartDate = date.Start,
                    EndDate = date.End
                };
                eventDetails.Add(newEventDetail);
            }
            _dbContext.Entity<TrEventDetail>().AddRange(eventDetails);
            #endregion

            #region Event Intended
            List<TrEventIntendedFor> EventIntendedFor = new List<TrEventIntendedFor>();
            List<TrEventIntendedForDepartment> EventIntendedForDepartement = new List<TrEventIntendedForDepartment>();
            List<TrEventIntendedForPosition> EventIntendedForPosition = new List<TrEventIntendedForPosition>();
            List<TrEventIntendedForPersonal> EventIntendedForPersonal = new List<TrEventIntendedForPersonal>();
            List<TrEventIntendedForPersonalStudent> EventIntendedForPersonalStudent = new List<TrEventIntendedForPersonalStudent>();
            List<TrEventIntendedForGradeStudent> EventIntendedForGradeStudent = new List<TrEventIntendedForGradeStudent>();
            List<TrEventIntendedForLevelStudent> EventIntendedForLevelStudent = new List<TrEventIntendedForLevelStudent>();
            List<TrEventIntendedForPersonalParent> EventIntendedForPersonalParent = new List<TrEventIntendedForPersonalParent>();
            List<TrEventIntendedForGradeParent> EventIntendedForGradeParent = new List<TrEventIntendedForGradeParent>();
            List<TrEventIntendedForGradeParent> EventIntendedForLevelParent = new List<TrEventIntendedForGradeParent>();
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

            body.IntendedFor.ForEach(e => e.IdIntendedFor = e.IdIntendedFor==null?Guid.NewGuid().ToString(): e.IdIntendedFor);
            foreach (var ItemIntendedFor in body.IntendedFor)
            {
                UserRolePersonalOptionRole role = UserRolePersonalOptionRole.ALL;

                switch (ItemIntendedFor.Role)
                {
                    case "ALL":
                        role = UserRolePersonalOptionRole.ALL;
                        break;
                    case "TEACHER":
                        role = UserRolePersonalOptionRole.TEACHER;
                        break;
                    case "STUDENT":
                        role = UserRolePersonalOptionRole.STUDENT;
                        break;
                    case "STAFF":
                        role = UserRolePersonalOptionRole.STAFF;
                        break;
                    case "PARENT":
                        role = UserRolePersonalOptionRole.PARENT;
                        break;
                    default:
                        break;
                }

                UserRolePersonalOptionType type = UserRolePersonalOptionType.None;
                var listPersonal = new List<string>();
                var listLevel = new List<string>();
                var listHomeroom = new List<string>();

                switch (ItemIntendedFor.Option)
                {
                    case EventOptionType.None:
                        type = UserRolePersonalOptionType.None;
                        break;
                    case EventOptionType.All:
                        type = UserRolePersonalOptionType.All;
                        break;
                    case EventOptionType.Grade:
                        type = UserRolePersonalOptionType.Grade;
                        if (ItemIntendedFor.Role == "STUDENT")
                            listHomeroom.AddRange(ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway);
                        else if (ItemIntendedFor.Role == "PARENT")
                            listHomeroom.AddRange(ItemIntendedFor.IntendedForGradeParentIdHomeroomPathway);
                        break;
                    case EventOptionType.Department:
                        type = UserRolePersonalOptionType.Department;
                        break;
                    case EventOptionType.Personal:
                        type = UserRolePersonalOptionType.Personal;
                        if (ItemIntendedFor.Role == "TEACHER" || ItemIntendedFor.Role == "STAFF")
                            listPersonal.AddRange(ItemIntendedFor.IntendedForPersonalIdUser.ToList());
                        else if (ItemIntendedFor.Role == "STUDENT")
                            listPersonal.AddRange(ItemIntendedFor.IntendedForPersonalIdStudent.ToList());
                        else if (ItemIntendedFor.Role == "PARENT")
                            listPersonal.AddRange(ItemIntendedFor.IntendedForPersonalIdParent.ToList());
                        break;
                    case EventOptionType.Position:
                        type = UserRolePersonalOptionType.Position;
                        break;
                    case EventOptionType.Level:
                        type = UserRolePersonalOptionType.Level;
                        if (ItemIntendedFor.Role == "STUDENT")
                            listLevel.AddRange(ItemIntendedFor.IntendedForLevelStudentIdLevel);
                        else if (ItemIntendedFor.Role == "PARENT")
                            listLevel.AddRange(ItemIntendedFor.IntendedForLevelParentIdLevel);
                        break;
                    case EventOptionType.Subject:
                    default:
                        break;
                }

                var newUserRolePositions = new GetUserRolePosition
                {
                    IdUserRolePositions  = ItemIntendedFor.IdIntendedFor,
                    Role = role,
                    Option = type,
                    TeacherPositions = ItemIntendedFor.IntendedForPositionIdTeacherPosition.ToList(),
                    Departemens = ItemIntendedFor.IntendedForDepartemetIdDepartemet.ToList(),
                    Level = listLevel,
                    Homeroom = listHomeroom,
                    Personal = listPersonal
                };

                paramUserRolePosition.UserRolePositions.Add(newUserRolePositions);
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
                    var listUserEvent = getUserRolePosition
                                        .Where(e=>e.IdUserRolePositions==ItemIntendedFor.IdIntendedFor)
                                        .GroupBy(e => new
                                        {
                                            e.IdUser
                                        })
                                        .Select(e => new TrUserEvent
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdEventDetail = eventDetail.Id,
                                            IsApproved = ItemIntendedFor.NeedParentPermission ? false : true,
                                            IsNeedApproval = ItemIntendedFor.NeedParentPermission ? true : false,
                                            IdUser = e.Key.IdUser
                                        })
                                        .Distinct().ToList();
                    _dbContext.Entity<TrUserEvent>().AddRange(listUserEvent);
                }
                #endregion

                if (ItemIntendedFor.Role == "All")
                {
                    NewIntendedFor = ListEventIntendedFor(newEvent, ItemIntendedFor, "No Option", false, false);
                    EventIntendedFor.Add(NewIntendedFor);
                }
                else
                {
                    NewIntendedFor = ListEventIntendedFor
                    (
                        newEvent,
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
                    }
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
                        if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.All)
                        {
                            var NewUserId = await (from a in _dbContext.Entity<MsHomeroomTeacher>()
                                                   join b in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals b.Id
                                                   where b.IdAcademicYear == body.IdAcadyear
                                                   select a.IdBinusian).Distinct().ToListAsync(CancellationToken);
                            GetUserId.AddRange(NewUserId);
                        }
                        else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Level)
                        {
                            var NewUserId = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Homeroom)
                                .ThenInclude(e => e.Grade)
                                .ThenInclude(e => e.Level)
                                .Where(e => ItemIntendedFor.IntendedForLevelStudentIdLevel.Contains(e.Homeroom.Grade.Level.Id))
                                .Select(e => e.IdBinusian).Distinct().ToListAsync(CancellationToken);
                            GetUserId.AddRange(NewUserId);
                        }
                        else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Grade)
                        {
                            var NewUserId = await (from a in _dbContext.Entity<MsHomeroomTeacher>()
                                                   join b in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals b.Id
                                                   where ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway.Contains(b.Id)
                                                   select a.IdBinusian).Distinct().ToListAsync(CancellationToken);
                            GetUserId.AddRange(NewUserId);
                        }
                        else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var NewUserId = await (from a in _dbContext.Entity<MsHomeroomTeacher>()
                                                   join b in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals b.Id
                                                   join c in _dbContext.Entity<MsHomeroomStudent>() on a.IdHomeroom equals c.IdHomeroom
                                                   where ItemIntendedFor.IntendedForPersonalIdStudent.Contains(c.IdStudent)
                                                   select a.IdBinusian).Distinct().ToListAsync(CancellationToken);
                            GetUserId.AddRange(NewUserId);
                        }
                        else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Staff)
                        {
                            var NewUserId = await (from a in _dbContext.Entity<MsStaff>()
                                                   join b in _dbContext.Entity<MsUser>() on a.IdBinusian equals b.Id
                                                   join c in _dbContext.Entity<MsUserSchool>() on b.Id equals c.IdUser
                                                   where c.IdSchool == ay.IdSchool
                                                   select a.IdBinusian).Distinct().ToListAsync(CancellationToken);
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

                    #region Attandance Checked
                    var eachDates = body.Dates.SelectMany(x => DateTimeUtil.ToEachDay(x.Start, x.End));
                    if (body.IsAttendanceRepeat)
                    {
                        foreach (var eachDate in eachDates)
                        {
                            foreach (var attCheck in body.AttandanceCheck.Where(e => e.Startdate.Date == eachDate.start.Date))
                            {
                                var attCheckStudent = new TrEventIntendedForAtdCheckStudent
                                {
                                    Id = Guid.NewGuid().ToString(),
                                    IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                                    StartDate = eachDate.start,
                                    StartTime = eachDate.start.TimeOfDay,
                                    EndTime = eachDate.end.TimeOfDay,
                                    CheckName = attCheck.Name,
                                    Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
                                    IsPrimary = attCheck.IsPrimary
                                };
                                IntendedForAttendanceCheckStudent.Add(attCheckStudent);
                            }
                        }
                    }
                    else
                    {
                        foreach (var attCheck in body.AttandanceCheck)
                        {
                            var eachDate = eachDates.SingleOrDefault(e => e.start == attCheck.Startdate);
                            var attCheckStudent = new TrEventIntendedForAtdCheckStudent
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                                StartDate = eachDate.start,
                                StartTime = eachDate.start.TimeOfDay,
                                EndTime = eachDate.end.TimeOfDay,
                                CheckName = attCheck.Name,
                                Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
                                IsPrimary = attCheck.IsPrimary
                            };
                            IntendedForAttendanceCheckStudent.Add(attCheckStudent);
                        }
                    }
                    #endregion
                }
                else
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

                    if (body.MandatoryType == Common.Model.Enums.EventIntendedForAttendanceStudent.All || body.MandatoryType == Common.Model.Enums.EventIntendedForAttendanceStudent.Excuse)
                    {
                        var eachDates = body.Dates.SelectMany(x => DateTimeUtil.ToEachDay(x.Start, x.End));
                        foreach (var eachDate in eachDates)
                        {
                            var attCheckStudentAll = new TrEventIntendedForAtdCheckStudent
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventIntendedForAttendanceStudent = newEventIntendedForAttendanceStudent.Id,
                                StartDate = eachDate.start,
                                StartTime = eachDate.start.TimeOfDay,
                                EndTime = eachDate.end.TimeOfDay,
                                CheckName = "All Present or All Excuse",
                                Time = DateTime.MinValue.TimeOfDay,
                                IsPrimary = true
                            };
                            IntendedForAttendanceCheckStudent.Add(attCheckStudentAll);
                        }
                    }
                }
                #endregion
            }

            _dbContext.Entity<TrEventIntendedFor>().AddRange(EventIntendedFor);
            _dbContext.Entity<TrEventIntendedForDepartment>().AddRange(EventIntendedForDepartement);
            _dbContext.Entity<TrEventIntendedForPosition>().AddRange(EventIntendedForPosition);
            _dbContext.Entity<TrEventIntendedForPersonal>().AddRange(EventIntendedForPersonal);
            _dbContext.Entity<TrEventIntendedForPersonalParent>().AddRange(EventIntendedForPersonalParent);
            _dbContext.Entity<TrEventIntendedForPersonalStudent>().AddRange(EventIntendedForPersonalStudent);
            _dbContext.Entity<TrEventIntendedForGradeStudent>().AddRange(EventIntendedForGradeStudent);
            _dbContext.Entity<TrEventIntendedForGradeParent>().AddRange(EventIntendedForGradeParent);
            _dbContext.Entity<TrEventIntendedForLevelStudent>().AddRange(EventIntendedForLevelStudent);
            _dbContext.Entity<TrEventIntendedForGradeParent>().AddRange(EventIntendedForLevelParent);
            _dbContext.Entity<TrEventIntendedForAttendanceStudent>().AddRange(EventIntendedForAttendanceStudent);
            _dbContext.Entity<TrEventIntendedForAtdCheckStudent>().AddRange(IntendedForAttendanceCheckStudent);
            _dbContext.Entity<TrEventIntendedForAtdPICStudent>().AddRange(EventIntendedForAttendancePIC);
            #endregion

            #region Event Coordinator
            var newEventCoordinator = new TrEventCoordinator
            {
                Id = Guid.NewGuid().ToString(),
                IdEvent = newEvent.Id,
                IdUser = body.IdUserCoordinator,
            };
            _dbContext.Entity<TrEventCoordinator>().Add(newEventCoordinator);
            #endregion

            #region Event Budget
            List<TrEventBudget> EventBudget = new List<TrEventBudget>();
            var CountNameDistinct = body.Budget.Select(e => e.Name).Distinct().Count();

            if (CountNameDistinct == body.Budget.Count())
            {
                foreach (var ItemBudget in body.Budget)
                {
                    var newEventBudget = new TrEventBudget
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = newEvent.Id,
                        Name = ItemBudget.Name,
                        Amount = ItemBudget.Amount,
                    };
                    EventBudget.Add(newEventBudget);
                }
                _dbContext.Entity<TrEventBudget>().AddRange(EventBudget);
            }
            else
            {
                throw new BadRequestException("The budget name can't be twin");
            }
            #endregion

            #region Event Attachment
            List<TrEventAttachment> EventAttachment = new List<TrEventAttachment>();
            foreach (var ItemAttachment in body.AttachmentBudget)
            {
                var newEventAttachment = new TrEventAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
                    Url = ItemAttachment.Url,
                    Filename = ItemAttachment.Filename,
                    Filetype = ItemAttachment.Filetype,
                    Filesize = ItemAttachment.Filesize,
                };
                EventAttachment.Add(newEventAttachment);
            }

            _dbContext.Entity<TrEventAttachment>().AddRange(EventAttachment);
            #endregion

            #region Event Activity
            List<TrEventActivity> EventActivity = new List<TrEventActivity>();
            List<TrEventActivityRegistrant> EventActivityRegristrant = new List<TrEventActivityRegistrant>();
            List<TrEventActivityPIC> EventActivityPIC = new List<TrEventActivityPIC>();
            List<TrEventActivityAward> EventActivityAward = new List<TrEventActivityAward>();
            List<TrEventActivityAwardTeacher> EventActivityAwardTeacher = new List<TrEventActivityAwardTeacher>();
            foreach (var ItemEventActivity in body.Activity)
            {
                var newEventActivity = new TrEventActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
                    IdActivity = ItemEventActivity.IdActivity,
                };
                EventActivity.Add(newEventActivity);

                #region Event Activity PIC
                foreach (var IdUser in ItemEventActivity.EventActivityPICIdUser)
                {
                    var newEventActivityPIC = new TrEventActivityPIC
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newEventActivity.Id,
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
                        IdEventActivity = newEventActivity.Id,
                        IdUser = IdUser,
                    };
                    EventActivityRegristrant.Add(newEventActivityRegristrant);
                }
                #endregion

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
                                    IdEventActivity = newEventActivity.Id,
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
                    newEvent.StatusEventAward = "On Review (1)";
                    newEvent.DescriptionEventAward = "Record of Involvement is On Review";
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
                                    IdEventActivity = newEventActivity.Id,
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
                    newEvent.StatusEventAward = "On Review (1)";
                    newEvent.DescriptionEventAward = "Record of Involvement is On Review";
                }

                #endregion
            }

            _dbContext.Entity<TrEvent>().Add(newEvent);
            _dbContext.Entity<TrEventActivity>().AddRange(EventActivity);
            _dbContext.Entity<TrEventActivityPIC>().AddRange(EventActivityPIC);
            _dbContext.Entity<TrEventActivityRegistrant>().AddRange(EventActivityRegristrant);
            _dbContext.Entity<TrEventActivityAward>().AddRange(EventActivityAward);
            _dbContext.Entity<TrEventActivityAwardTeacher>().AddRange(EventActivityAwardTeacher);

            #endregion

            #region History Event Approval
            List<HTrEventApproval> EventAproval = new List<HTrEventApproval>();
            var newEventApproval = new HTrEventApproval
            {
                Id = Guid.NewGuid().ToString(),
                IdEvent = newEvent.Id,
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
                    IdEvent = newEvent.Id,
                    Section = "Event",
                    State = 2,
                    IdUser = body.IdUserEventApproval2,
                };
                EventAproval.Add(newEventApproval);
            }
            _dbContext.Entity<HTrEventApproval>().AddRange(EventAproval);
            #endregion

            #region History Event Award Approval
            var EventAwardAproval = new List<HTrEventApproval>();
            var EventAprover = new List<TrEventApprover>();
            var AwardAprover = new List<TrEventAwardApprover>();
            var newEventAwardApproval = new HTrEventApproval();

            if (!string.IsNullOrEmpty(body.IdUserEventApproval1))
            {
                newEventAwardApproval = new HTrEventApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
                    Section = "Event",
                    State = 1,
                    IdUser = body.IdUserEventApproval1,
                };
                EventAwardAproval.Add(newEventAwardApproval);
            }

            if (!string.IsNullOrEmpty(body.IdUserEventApproval2))
            {
                newEventAwardApproval = new HTrEventApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
                    Section = "Event",
                    State = 2,
                    IdUser = body.IdUserEventApproval2,
                };
                EventAwardAproval.Add(newEventAwardApproval);
            }

            if (!string.IsNullOrEmpty(body.IdUserAwardApproval1))
            {
                newEventAwardApproval = new HTrEventApproval
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newEvent.Id,
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
                    IdEvent = newEvent.Id,
                    Section = "Award",
                    State = 2,
                    IdUser = body.IdUserAwardApproval2,
                };
                EventAwardAproval.Add(newEventAwardApproval);
            }
            _dbContext.Entity<HTrEventApproval>().AddRange(EventAwardAproval);
            #endregion

            #region Event Approval
            var EventApprover = new List<TrEventApprover>();
            var ListApprover = EventAwardAproval.Where(e => e.Section == "Event").ToList();
            int OrderApprover = 1;
            foreach (var Approver in ListApprover)
            {
                var NewEventApprover = new TrEventApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = Approver.IdEvent,
                    IdUser = Approver.IdUser,
                    OrderNumber = OrderApprover
                };

                EventApprover.Add(NewEventApprover);
                OrderApprover++;
            }
            _dbContext.Entity<TrEventApprover>().AddRange(EventApprover);
            #endregion

            #region Award Approval
            var AwardApprover = new List<TrEventAwardApprover>();
            ListApprover = EventAwardAproval.Where(e => e.Section == "Award").ToList();

            int OrderAwardApprover = 1;
            foreach (var Approver in ListApprover)
            {
                var NewAwardApprover = new TrEventAwardApprover
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = Approver.IdEvent,
                    IdUser = Approver.IdUser,
                    OrderNumber = OrderAwardApprover
                };

                AwardApprover.Add(NewAwardApprover);
                OrderAwardApprover++;
            }
            _dbContext.Entity<TrEventAwardApprover>().AddRange(AwardApprover);
            #endregion

            #region Event Change
            var DisplayName = _dbContext.Entity<MsUser>()
                .SingleOrDefault(e => e.Id == AuthInfo.UserId).DisplayName;
            var idEventChange = Guid.NewGuid().ToString();
            var newEventChange = new TrEventChange
            {
                Id = idEventChange,
                IdEvent = newEvent.Id,
                ChangeNotes = "Event Created by " + DisplayName
            };
            _dbContext.Entity<TrEventChange>().Add(newEventChange);
            #endregion

            #region History Event
            var newHTrEvent = new HTrEvent
            {
                Id = idEventChange,
                IdAcademicYear = ay.IdAcademicYear,
                IdEventType = body.IdEventType,
                Name = body.EventName,
                IsShowOnCalendarAcademic = body.IsShowOnCalendarAcademic,
                IsShowOnSchedule = body.IsShowOnSchedule,
                Objective = body.EventObjective,
                Place = body.EventPlace,
                EventLevel = (EventLevel)body.EventLevel,
                // IdCertificateTemplate = (body.IdCertificateTemplate != null || body.IdCertificateTemplate != "") ? "" : null,
                StatusEvent = "On Review (1)",
                DescriptionEvent = "Event Settings is On Review",
            };
            _dbContext.Entity<HTrEvent>().Add(newHTrEvent);
            #endregion


            #region History Event Detail
            var htrEventDetails = new List<HTrEventDetail>(body.Dates.Count());

            foreach (var date in body.Dates)
            {
                var intersectEvents = await _dbContext.Entity<HTrEventDetail>()
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
                // if (intersectEvents.Count != 0)
                // {
                //     // get each date of new event
                //     var eachDate = DateTimeUtil.ToEachDay(date.Start, date.End);

                //     foreach (var (start, end) in eachDate)
                //     {
                //         // select event that intersect date & time with day
                //         var dayOfEvents = intersectEvents.Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, start, end));
                //         // select event that intersect time with day
                //         var intersectDayOfEvents = dayOfEvents
                //             .Where(x
                //                 => TimeSpanUtil.IsIntersect(x.StartDate.TimeOfDay, x.EndDate.TimeOfDay, start.TimeOfDay, end.TimeOfDay)
                //                 && (body.IntendedFor.Any(e=>e.Role=="ALL") || body.IntendedFor.Any(e => e.Role.Contains(x.Event.EventIntendedFor.First().IntendedFor)))
                //                 && x.Event.Name == body.EventName);

                //         if (intersectDayOfEvents.Any())
                //             conflictEvents = conflictEvents.Concat(intersectEvents.Select(x => x.Event.Name));
                //     }
                // }

                // if (conflictEvents.Any())
                //     throw new BadRequestException("There is another event with same name, intended for, date and time.");

                var newHTrEventDetail = new HTrEventDetail
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    StartDate = date.Start,
                    EndDate = date.End
                };
                htrEventDetails.Add(newHTrEventDetail);
            }
            _dbContext.Entity<HTrEventDetail>().AddRange(htrEventDetails);
            #endregion

            #region History Event Intended
            List<HTrEventIntendedFor> HTrEventIntendedFor = new List<HTrEventIntendedFor>();
            List<HTrEventIntendedForDepartment> HTrEventIntendedForDepartement = new List<HTrEventIntendedForDepartment>();
            List<HTrEventIntendedForPosition> HTrEventIntendedForPosition = new List<HTrEventIntendedForPosition>();
            List<HTrEventIntendedForPersonal> HTrEventIntendedForPersonal = new List<HTrEventIntendedForPersonal>();
            List<HTrEventIntendedForPersonalStudent> HTrEventIntendedForPersonalStudent = new List<HTrEventIntendedForPersonalStudent>();
            List<HTrEventIntendedForPersonalParent> HTrEventIntendedForPersonalParent = new List<HTrEventIntendedForPersonalParent>();
            List<HTrEventIntendedForGradeParent> HTrEventIntendedForGradeParent = new List<HTrEventIntendedForGradeParent>();
            List<HTrEventIntendedForGradeParent> HTrEventIntendedForLevelParent = new List<HTrEventIntendedForGradeParent>();
            List<HTrEventIntendedForGradeStudent> HTrEventIntendedForGradeStudent = new List<HTrEventIntendedForGradeStudent>();
            List<HTrEventIntendedForLevelStudent> HTrEventIntendedForLevelStudent = new List<HTrEventIntendedForLevelStudent>();
            List<HTrEventIntendedForAtdPICStudent> HTrEventIntendedForAttendancePIC = new List<HTrEventIntendedForAtdPICStudent>();
            List<HTrEvIntendedForAtdCheckStudent> HsIntendedForAttendanceCheckStudent = new List<HTrEvIntendedForAtdCheckStudent>();
            List<HTrEventIntendedForAtdStudent> HTrEventIntendedForAtdStudent = new List<HTrEventIntendedForAtdStudent>();
            HTrEventIntendedFor NewHsIntendedFor = null;
            foreach (var ItemIntendedFor in body.IntendedFor)
            {
                if (ItemIntendedFor.Role == "All")
                {

                    NewHsIntendedFor = ListHTrEventIntendedFor(newHTrEvent, ItemIntendedFor, "No Option", false, false);
                    HTrEventIntendedFor.Add(NewHsIntendedFor);
                }
                else
                {
                    NewHsIntendedFor = ListHTrEventIntendedFor
                    (
                        newHTrEvent,
                        ItemIntendedFor,
                        ItemIntendedFor.Option.ToString(),
                        ItemIntendedFor.Role == RoleConstant.Student ? ItemIntendedFor.SendNotificationToLevelHead : false,
                        ItemIntendedFor.Role == RoleConstant.Student ? ItemIntendedFor.NeedParentPermission : false
                    );
                    HTrEventIntendedFor.Add(NewHsIntendedFor);

                    if (ItemIntendedFor.Role == RoleConstant.Staff || ItemIntendedFor.Role == RoleConstant.Teacher)
                    {
                        if (ItemIntendedFor.Option == EventOptionType.Department)
                        {
                            var NewHsIntendedForDepartement = ListHTrEventIntendedForDepartment(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForDepartement.AddRange(NewHsIntendedForDepartement);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Position)
                        {
                            var NewHsIntendedForPosition = ListHTrEventIntendedForPosition(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForPosition.AddRange(NewHsIntendedForPosition);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var NewHsIntendedForPersonal = ListHTrEventIntendedForPersonal(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForPersonal.AddRange(NewHsIntendedForPersonal);
                        }
                    }
                    else if (ItemIntendedFor.Role == RoleConstant.Parent)
                    {
                        if (ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var NewHsIntendedForPersonalParent = ListHTrEventIntendedForPersonalParent(NewHsIntendedFor, intendedPersonalIdParents);
                            HTrEventIntendedForPersonalParent.AddRange(NewHsIntendedForPersonalParent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Grade)
                        {
                            var NewHsIntendedForGradeParent = ListHTrEventIntendedForGradeParent(NewHsIntendedFor, ItemIntendedFor, intendedGradeByParent);
                            HTrEventIntendedForGradeParent.AddRange(NewHsIntendedForGradeParent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Level)
                        {

                            var NewHsIntendedForLevelParent = ListHTrEventIntendedForLevelParent(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForLevelParent.AddRange(NewHsIntendedForLevelParent);
                        }
                    }
                    else if (ItemIntendedFor.Role == RoleConstant.Student)
                    {
                        if (ItemIntendedFor.Option == EventOptionType.Level)
                        {
                            var NewHsIntendedForLevelStudent = ListHTrEventIntendedForLevelStudent(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForLevelStudent.AddRange(NewHsIntendedForLevelStudent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Grade)
                        {
                            var NewHsIntendedForGradeStudent = ListHTrEventIntendedForGradeStudent(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForGradeStudent.AddRange(NewHsIntendedForGradeStudent);
                        }
                        else if (ItemIntendedFor.Option == EventOptionType.Personal)
                        {
                            var NewHsIntendedForPersonalStudent = ListHTrEventIntendedForPersonalStudent(NewHsIntendedFor, ItemIntendedFor);
                            HTrEventIntendedForPersonalStudent.AddRange(NewHsIntendedForPersonalStudent);
                        }

                        #region Attendent Setting
                        if (body.IsSetAttendance)
                        {
                            var newHTrEventIntendedForAttendanceStudent = new HTrEventIntendedForAtdStudent
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdEventIntendedFor = NewHsIntendedFor.Id,
                                Type = body.MandatoryType,
                                IsSetAttendance = body.IsSetAttendance,
                                IsRepeat = body.IsAttendanceRepeat,
                            };
                            HTrEventIntendedForAtdStudent.Add(newHTrEventIntendedForAttendanceStudent);

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
                                else if (body.AttandancePIC == EventIntendedForAttendancePICStudent.Homeroom && ItemIntendedFor.Option == EventOptionType.Personal)
                                {
                                    var NewUserId = await (from a in _dbContext.Entity<MsHomeroomTeacher>()
                                                           join b in _dbContext.Entity<MsHomeroom>() on a.IdHomeroom equals b.Id
                                                           join c in _dbContext.Entity<MsHomeroomStudent>() on a.IdHomeroom equals c.IdHomeroom
                                                           where ItemIntendedFor.IntendedForPersonalIdStudent.Contains(c.IdStudent)
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
                                    var newHTrEventIntendedForAttendancePIC = new HTrEventIntendedForAtdPICStudent
                                    {
                                        IdHTrEventIntendedForAtdPICStudent = Guid.NewGuid().ToString(),
                                        IdEventIntendedForAttendanceStudent = newHTrEventIntendedForAttendanceStudent.Id,
                                        Type = body.AttandancePIC,
                                        IdUser = IdUser,
                                    };

                                    HTrEventIntendedForAttendancePIC.Add(newHTrEventIntendedForAttendancePIC);
                                }
                            }
                            else
                            {
                                var newHTrEventIntendedForAttendancePIC = new HTrEventIntendedForAtdPICStudent
                                {
                                    IdHTrEventIntendedForAtdPICStudent = Guid.NewGuid().ToString(),
                                    IdEventIntendedForAttendanceStudent = newHTrEventIntendedForAttendanceStudent.Id,
                                    Type = body.AttandancePIC,
                                    IdUser = body.AttandancePIC == EventIntendedForAttendancePICStudent.EventCoordinator ? body.IdUserCoordinator : body.AttandancePICIdUser,
                                };

                                HTrEventIntendedForAttendancePIC.Add(newHTrEventIntendedForAttendancePIC);
                            }
                            #endregion

                            #region Attandance Checked
                            var eachDates = body.Dates.SelectMany(x => DateTimeUtil.ToEachDay(x.Start, x.End));
                            if (body.IsAttendanceRepeat)
                            {
                                foreach (var eachDate in eachDates)
                                {
                                    foreach (var attCheck in body.AttandanceCheck)
                                    {
                                        var HsattCheckStudent = new HTrEvIntendedForAtdCheckStudent
                                        {
                                            Id = Guid.NewGuid().ToString(),
                                            IdHTrEventIntendedForAtdStudent = newHTrEventIntendedForAttendanceStudent.Id,
                                            StartDate = eachDate.start,
                                            StartTime = eachDate.start.TimeOfDay,
                                            EndTime = eachDate.end.TimeOfDay,
                                            CheckName = attCheck.Name,
                                            Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
                                            IsPrimary = attCheck.IsPrimary
                                        };
                                        HsIntendedForAttendanceCheckStudent.Add(HsattCheckStudent);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var attCheck in body.AttandanceCheck)
                                {
                                    var eachDate = eachDates.SingleOrDefault(e => e.start == attCheck.Startdate);
                                    var HsattCheckStudent = new HTrEvIntendedForAtdCheckStudent
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        IdHTrEventIntendedForAtdStudent = newHTrEventIntendedForAttendanceStudent.Id,
                                        StartDate = eachDate.start,
                                        StartTime = eachDate.start.TimeOfDay,
                                        EndTime = eachDate.end.TimeOfDay,
                                        CheckName = attCheck.Name,
                                        Time = TimeSpan.FromMinutes(attCheck.TimeInMinute),
                                        IsPrimary = attCheck.IsPrimary
                                    };
                                    HsIntendedForAttendanceCheckStudent.Add(HsattCheckStudent);
                                }
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
            }

            _dbContext.Entity<HTrEventIntendedFor>().AddRange(HTrEventIntendedFor);
            _dbContext.Entity<HTrEventIntendedForDepartment>().AddRange(HTrEventIntendedForDepartement);
            _dbContext.Entity<HTrEventIntendedForPosition>().AddRange(HTrEventIntendedForPosition);
            _dbContext.Entity<HTrEventIntendedForPersonal>().AddRange(HTrEventIntendedForPersonal);
            _dbContext.Entity<HTrEventIntendedForPersonalParent>().AddRange(HTrEventIntendedForPersonalParent);
            _dbContext.Entity<HTrEventIntendedForGradeParent>().AddRange(HTrEventIntendedForGradeParent);
            _dbContext.Entity<HTrEventIntendedForGradeParent>().AddRange(HTrEventIntendedForLevelParent);
            _dbContext.Entity<HTrEventIntendedForPersonalStudent>().AddRange(HTrEventIntendedForPersonalStudent);
            _dbContext.Entity<HTrEventIntendedForGradeStudent>().AddRange(HTrEventIntendedForGradeStudent);
            _dbContext.Entity<HTrEventIntendedForLevelStudent>().AddRange(HTrEventIntendedForLevelStudent);
            _dbContext.Entity<HTrEventIntendedForAtdStudent>().AddRange(HTrEventIntendedForAtdStudent);
            _dbContext.Entity<HTrEvIntendedForAtdCheckStudent>().AddRange(HsIntendedForAttendanceCheckStudent);
            _dbContext.Entity<HTrEventIntendedForAtdPICStudent>().AddRange(HTrEventIntendedForAttendancePIC);
            #endregion

            #region History Event Coordinator
            var newHTrEventCoordinator = new HTrEventCoordinator
            {
                Id = Guid.NewGuid().ToString(),
                IdEvent = newHTrEvent.Id,
                IdUser = body.IdUserCoordinator,
            };
            _dbContext.Entity<HTrEventCoordinator>().Add(newHTrEventCoordinator);
            #endregion

            #region History Event Budget
            List<HTrEventBudget> HTrEventBudget = new List<HTrEventBudget>();
            var CountHsNameDistinct = body.Budget.Select(e => e.Name).Distinct().Count();

            if (CountHsNameDistinct == body.Budget.Count())
            {
                foreach (var ItemBudget in body.Budget)
                {
                    var newHTrEventBudget = new HTrEventBudget
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEvent = newHTrEvent.Id,
                        Name = ItemBudget.Name,
                        Amount = ItemBudget.Amount,
                    };
                    HTrEventBudget.Add(newHTrEventBudget);
                }
                _dbContext.Entity<HTrEventBudget>().AddRange(HTrEventBudget);
            }
            else
            {
                throw new BadRequestException("The budget name can't be twin");
            }
            #endregion

            #region History Event Attachment
            List<HTrEventAttachment> HTrEventAttachment = new List<HTrEventAttachment>();
            foreach (var ItemAttachment in body.AttachmentBudget)
            {
                var newHTrEventAttachment = new HTrEventAttachment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    Url = ItemAttachment.Url,
                    Filename = ItemAttachment.Filename,
                    Filetype = ItemAttachment.Filetype,
                    Filesize = ItemAttachment.Filesize,
                };
                HTrEventAttachment.Add(newHTrEventAttachment);
            }

            _dbContext.Entity<HTrEventAttachment>().AddRange(HTrEventAttachment);
            #endregion

            #region History Event Activity
            List<HTrEventActivity> HTrEventActivity = new List<HTrEventActivity>();
            List<HTrEventActivityRegistrant> HTrEventActivityRegristrant = new List<HTrEventActivityRegistrant>();
            List<HTrEventActivityPIC> HTrEventActivityPIC = new List<HTrEventActivityPIC>();
            List<HTrEventActivityAward> HTrEventActivityAward = new List<HTrEventActivityAward>();
            List<HTrEventActivityAwardTeacher> HTrEventActivityAwardTeacher = new List<HTrEventActivityAwardTeacher>();
            foreach (var ItemEventActivity in body.Activity)
            {
                var newHTrEventActivity = new HTrEventActivity
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEvent = newHTrEvent.Id,
                    IdActivity = ItemEventActivity.IdActivity,
                };
                HTrEventActivity.Add(newHTrEventActivity);

                #region History Event Activity PIC
                foreach (var IdUser in ItemEventActivity.EventActivityPICIdUser)
                {
                    var newHTrEventActivityPIC = new HTrEventActivityPIC
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdUser = IdUser,
                    };
                    HTrEventActivityPIC.Add(newHTrEventActivityPIC);
                }
                #endregion

                #region History Event Activity Registrant
                foreach (var IdUser in ItemEventActivity.EventActivityRegistrantIdUser)
                {
                    var newHTrEventActivityRegristrant = new HTrEventActivityRegistrant
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdEventActivity = newHTrEventActivity.Id,
                        IdUser = IdUser,
                    };
                    HTrEventActivityRegristrant.Add(newHTrEventActivityRegristrant);
                }
                #endregion

                #region History Activity Award
                foreach (var ItemInvolvmentAward in ItemEventActivity.EventActivityAwardIdUser)
                {
                    foreach (var ItemAward in ItemInvolvmentAward.IdAward)
                    {
                        var newHTrEventActivityAward = new HTrEventActivityAward
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdEventActivity = newHTrEventActivity.Id,
                            IdAward = ItemAward,
                            IdHomeroomStudent = ItemInvolvmentAward.IdHomeroomStudent,
                            Url = ItemInvolvmentAward.Url,
                            Filename = ItemInvolvmentAward.Filename,
                            Filetype = ItemInvolvmentAward.Filetype,
                            Filesize = ItemInvolvmentAward.Filesize,
                            OriginalFilename = ItemInvolvmentAward.OriginalFilename
                        };
                        HTrEventActivityAward.Add(newHTrEventActivityAward);
                    }
                }
                #endregion

                #region History Activity Award Teacher
                foreach (var ItemInvolvmentAward in ItemEventActivity.EventActivityAwardTeacherIdUser)
                {
                    foreach (var ItemAward in ItemInvolvmentAward.IdAward)
                    {
                        var newHTrEventActivityAwardTeacher = new HTrEventActivityAwardTeacher
                        {
                            Id = Guid.NewGuid().ToString(),
                            IdEventActivity = newHTrEventActivity.Id,
                            IdAward = ItemAward,
                            IdStaff = ItemInvolvmentAward.IdStaff,
                            Url = ItemInvolvmentAward.Url,
                            Filename = ItemInvolvmentAward.Filename,
                            Filetype = ItemInvolvmentAward.Filetype,
                            Filesize = ItemInvolvmentAward.Filesize,
                            OriginalFilename = ItemInvolvmentAward.OriginalFilename
                        };
                        HTrEventActivityAwardTeacher.Add(newHTrEventActivityAwardTeacher);
                    }
                }
                #endregion
            }

            _dbContext.Entity<HTrEventActivity>().AddRange(HTrEventActivity);
            _dbContext.Entity<HTrEventActivityPIC>().AddRange(HTrEventActivityPIC);
            _dbContext.Entity<HTrEventActivityRegistrant>().AddRange(HTrEventActivityRegristrant);
            _dbContext.Entity<HTrEventActivityAward>().AddRange(HTrEventActivityAward);
            _dbContext.Entity<HTrEventActivityAwardTeacher>().AddRange(HTrEventActivityAwardTeacher);

            #endregion

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
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

        public HTrEventIntendedFor ListHTrEventIntendedFor(HTrEvent newHTrEvent, SchoolEventIntendedFor ItemIntendedFor, string option, bool SendNotificationToLevelHead, bool NeedParentPermission)
        {
            var NewHTrEventIntendedFor = new HTrEventIntendedFor
            {
                Id = Guid.NewGuid().ToString(),
                IdEvent = newHTrEvent.Id,
                IntendedFor = ItemIntendedFor.Role,
                Option = option,
                SendNotificationToLevelHead = ItemIntendedFor.SendNotificationToLevelHead,
                NeedParentPermission = ItemIntendedFor.NeedParentPermission,
                NoteToParent = ItemIntendedFor.NoteToParent
            };

            return NewHTrEventIntendedFor;
        }

        public List<HTrEventIntendedForDepartment> ListHTrEventIntendedForDepartment(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForDepartement = new List<HTrEventIntendedForDepartment>();
            foreach (var IdDepartment in ItemIntendedFor.IntendedForDepartemetIdDepartemet)
            {
                var NewHTrEventIntendedForDepartement = new HTrEventIntendedForDepartment
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdDepartment = IdDepartment,
                };

                HTrEventIntendedForDepartement.Add(NewHTrEventIntendedForDepartement);
            }
            return HTrEventIntendedForDepartement;
        }

        public List<HTrEventIntendedForPosition> ListHTrEventIntendedForPosition(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForPosition = new List<HTrEventIntendedForPosition>();
            foreach (var IdTeacherPosition in ItemIntendedFor.IntendedForPositionIdTeacherPosition)
            {
                var NewHTrEventIntendedForPosition = new HTrEventIntendedForPosition
                {
                    IdHTrEventIntendedForPosition = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdTeacherPosition = IdTeacherPosition,
                };

                HTrEventIntendedForPosition.Add(NewHTrEventIntendedForPosition);
            }
            return HTrEventIntendedForPosition;
        }

        public List<HTrEventIntendedForPersonal> ListHTrEventIntendedForPersonal(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForPersonal = new List<HTrEventIntendedForPersonal>();
            foreach (var IdUser in ItemIntendedFor.IntendedForPersonalIdUser)
            {
                var NewHTrEventIntendedForPersonal = new HTrEventIntendedForPersonal
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdUser = IdUser,
                };

                HTrEventIntendedForPersonal.Add(NewHTrEventIntendedForPersonal);
            }
            return HTrEventIntendedForPersonal;
        }

        public List<HTrEventIntendedForPersonalParent> ListHTrEventIntendedForPersonalParent(HTrEventIntendedFor NewIntendedFor, List<string> intendedPersonalIdParents)
        {
            var HTrEventIntendedForPersonalParent = new List<HTrEventIntendedForPersonalParent>();
            foreach (var IdParent in intendedPersonalIdParents)
            {
                var NewHTrEventIntendedForPersonalParent = new HTrEventIntendedForPersonalParent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdParent = IdParent,
                };

                HTrEventIntendedForPersonalParent.Add(NewHTrEventIntendedForPersonalParent);
            }
            return HTrEventIntendedForPersonalParent;
        }

        public List<HTrEventIntendedForGradeParent> ListHTrEventIntendedForGradeParent(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor, List<GetGradeByLevelResult> homeroomByLevel)
        {
            var HTrEventIntendedForGradeParent = new List<HTrEventIntendedForGradeParent>();
            foreach (var IdHomeroomPathway in ItemIntendedFor.IntendedForGradeParentIdHomeroomPathway)
            {
                var NewHTrEventIntendedForGradeParent = new HTrEventIntendedForGradeParent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHTrEventIntendedFor = NewIntendedFor.Id,
                    IdHomeroom = IdHomeroomPathway,
                    IdLevel = homeroomByLevel.FirstOrDefault(x => x.IdHomeroom == IdHomeroomPathway).IdLevel
                };

                HTrEventIntendedForGradeParent.Add(NewHTrEventIntendedForGradeParent);
            }
            return HTrEventIntendedForGradeParent;
        }

        public List<HTrEventIntendedForGradeParent> ListHTrEventIntendedForLevelParent(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForLevelParent = new List<HTrEventIntendedForGradeParent>();
            foreach (var IdLevel in ItemIntendedFor.IntendedForLevelParentIdLevel)
            {
                var NewHTrEventIntendedForLevelParent = new HTrEventIntendedForGradeParent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdHTrEventIntendedFor = NewIntendedFor.Id,
                    IdLevel = IdLevel,
                };

                HTrEventIntendedForLevelParent.Add(NewHTrEventIntendedForLevelParent);
            }
            return HTrEventIntendedForLevelParent;
        }

        public List<HTrEventIntendedForPersonalStudent> ListHTrEventIntendedForPersonalStudent(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForPersonalStudent = new List<HTrEventIntendedForPersonalStudent>();
            foreach (var IdStudent in ItemIntendedFor.IntendedForPersonalIdStudent)
            {
                var NewHTrEventIntendedForPersonalStudent = new HTrEventIntendedForPersonalStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdStudent = IdStudent,
                };

                HTrEventIntendedForPersonalStudent.Add(NewHTrEventIntendedForPersonalStudent);
            }
            return HTrEventIntendedForPersonalStudent;
        }

        public List<HTrEventIntendedForGradeStudent> ListHTrEventIntendedForGradeStudent(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForGradeStudent = new List<HTrEventIntendedForGradeStudent>();
            foreach (var IdHomeroomPathway in ItemIntendedFor.IntendedForGradeStudentIdHomeroomPathway)
            {
                var NewHTrEventIntendedForGradeStudent = new HTrEventIntendedForGradeStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdHomeroom = IdHomeroomPathway,
                };

                HTrEventIntendedForGradeStudent.Add(NewHTrEventIntendedForGradeStudent);
            }
            return HTrEventIntendedForGradeStudent;
        }

        public List<HTrEventIntendedForLevelStudent> ListHTrEventIntendedForLevelStudent(HTrEventIntendedFor NewIntendedFor, SchoolEventIntendedFor ItemIntendedFor)
        {
            var HTrEventIntendedForLevelStudent = new List<HTrEventIntendedForLevelStudent>();
            foreach (var IdLevel in ItemIntendedFor.IntendedForLevelStudentIdLevel)
            {
                var NewHTrEventIntendedForLevelStudent = new HTrEventIntendedForLevelStudent
                {
                    Id = Guid.NewGuid().ToString(),
                    IdEventIntendedFor = NewIntendedFor.Id,
                    IdLevel = IdLevel,
                };

                HTrEventIntendedForLevelStudent.Add(NewHTrEventIntendedForLevelStudent);
            }
            return HTrEventIntendedForLevelStudent;
        }
    }
}
