using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent;
using FluentEmail.Core;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BinusSchool.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Scheduling.FnSchedule.SchoolEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Threading;
using BinusSchool.Common.Model.Enums;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleV2Handler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<IReadOnlyList<CalendarData>> _emptyCalendarData = new Lazy<IReadOnlyList<CalendarData>>(new List<CalendarData>(0));
        private static readonly Lazy<IReadOnlyList<TrClassDiaryAttachment>> _emptyClassDiaryAttachment = new Lazy<IReadOnlyList<TrClassDiaryAttachment>>(new List<TrClassDiaryAttachment>(0));
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetCalendarScheduleV2Request.IdSchool),
            nameof(GetCalendarScheduleV2Request.StartDate), nameof(GetCalendarScheduleV2Request.EndDate), nameof(GetCalendarScheduleV2Request.Role)
        });

        private readonly ISchedulingDbContext _dbContext;
        //private readonly GetCalendarEvent2Handler _calendarEventHandler;
        private readonly IMachineDateTime _datetimeNow;
        private readonly IRedisCache _redisCache;

        public GetCalendarScheduleV2Handler(ISchedulingDbContext dbContext, IMachineDateTime datetimeNow, IRedisCache redisCache
             //, GetCalendarEvent2Handler calendarEventHandler
             )
        {
            _dbContext = dbContext;
            _datetimeNow = datetimeNow;
            _redisCache = redisCache;
            //_calendarEventHandler = calendarEventHandler;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCalendarScheduleV2Request>(_requiredParams.Value);
            var (schedules, props) = await GetCalendarSchedules(param, Localizer);

            return Request.CreateApiResult2(schedules as object, props);
        }

        public async Task<(IEnumerable<GetCalendarScheduleV2Result> schedules, IDictionary<string, object> props)> GetCalendarSchedules(GetCalendarScheduleV2Request param, IStringLocalizer localizer)
        {
            var roleWithSettingPublishDate = new List<string> { RoleConstant.Student, RoleConstant.Parent };

            if (!param.Role.IsConstantOf<RoleConstant>())
                throw new BadRequestException(string.Format(localizer["ExNotExist"], localizer["Role"], "Code", param.Role));

            // convert param date to start until end day
            param.StartDate = new DateTime(param.StartDate.Year, param.StartDate.Month, param.StartDate.Day, 0, 0, 0);
            param.EndDate = new DateTime(param.EndDate.Year, param.EndDate.Month, param.EndDate.Day, 23, 59, 59);

            var results = new List<GetCalendarScheduleV2Result>();

            //Add Read setting Schedule Publish Date
            var configSettingSchedulePublishDate = _dbContext.Entity<MsSettingSchedulePublishDate>()
                .Where(x => x.IdAcademicYear == param.IdAcadyear && x.IdSchool == param.IdSchool.First())
                .FirstOrDefault();

            if (configSettingSchedulePublishDate != null && roleWithSettingPublishDate.Any(x => x == param.Role))
            {
                //Add validate If the current date is smaller than the Schedule Publish Date setting date, then no Schedule will be displayed
                if (_datetimeNow.ServerTime < configSettingSchedulePublishDate.PublishDate)
                    return (results.OrderBy(x => x.Start).SetPagination(param), param.CreatePaginationProperty(results.Count));
            }

            var listSchedule = await GetGeneratedSchedules(param);

            var listClassDiary = await GetClassDiaries(param);

            var ListPersonalInvitation = await GetPersonalInvitation(param);

            var ListInvitationBooking = await GetInvitationBooking(param);

            var listClassDiaryAttachment = listClassDiary.Count != 0
                ? await (from ClassDiaryAttachment in _dbContext.Entity<TrClassDiaryAttachment>()
                         where listClassDiary.Select(e => e.Id).Contains(ClassDiaryAttachment.IdClassDiary)
                         select ClassDiaryAttachment).ToListAsync(CancellationToken)
                : _emptyClassDiaryAttachment.Value;

            var listSchoolEvent = await GetSchoolEvents(param);

            var query = listSchedule.Union(listClassDiary).Union(listSchoolEvent).Union(ListInvitationBooking).Union(ListPersonalInvitation);

            var schedules = query
                //.OrderBy(x => x.ScheduleDate)
                //.Where(e => e.EventType.Description == "Class Diary")
                .Select(x => new GetCalendarScheduleV2Result
                {
                    Id = x.Id,
                    Name = x.SubjectName,
                    Start = x.Start,
                    End = x.End,
                    EventType = x.EventType,
                    Teacher = x.Teacher,
                    Venue = x.Venue,
                    Homeroom = x.Homeroom,
                    Department = x.Department, // temporary store IdSubject to get department later
                    CreateBy = x.CreatedBy,
                    ClassId = x.ClassId,
                    Description = x.Description,
                    Attachment = listClassDiaryAttachment.Where(e => e.IdClassDiary == x.Id).Select(e => new AttachmantClassDiary
                    {
                        Id = e.Id,
                        Url = e.Url,
                        OriginalFilename = e.OriginalFilename,
                        FileName = e.Filename,
                        FileSize = e.Filesize,
                        FileType = e.Filetype,
                    }).ToList(),
                    StudentBooking = x.StudentBooking,
                    IsChange = x.IsChange,
                    IsCancel = x.IsCancel
                })
                .ToList();

            if (schedules.Count != 0)
            {
                // remove redundant schedule
                var dupComparer = new InlineComparer<GetCalendarScheduleV2Result>(
                    (x, y) => DateTimeUtil.IsIntersect(new DateTimeRange(x.Start, x.End), new DateTimeRange(y.Start, y.End)) && x.ClassId == y.ClassId,
                    x => (x.Start, x.End, x.ClassId).GetHashCode());
                schedules = schedules.Distinct(dupComparer).ToList();

                var idSubjects = schedules.Select(x => x.Department.Id).Distinct(); // get temporary stored IdSubject
                var departments = await _dbContext.Entity<MsSubject>()
                    .Where(x => idSubjects.Contains(x.Id))
                    .Select(x => new { x.IdDepartment, x.Department.Description, IdSubject = x.Id })
                    .ToListAsync(CancellationToken);

                foreach (var schedule in schedules)
                {

                    var department = departments.Find(x => x.IdSubject == schedule.Department.Id); // find department with temp IdSubject
                    schedule.Department.Id = department?.IdDepartment;
                    schedule.Department.Description = department?.Description;

                    // prioritize event over schedule if the time was intersected
                    // var intersectsEvent = eventAsSchedules.Where(x => DateTimeUtil.IsIntersect(x.Start, x.End, schedule.Start, schedule.End));
                    // if (!intersectsEvent.Any())
                    results.Add(schedule);


                }

            }

            return (results.OrderBy(x => x.Start).SetPagination(param), param.CreatePaginationProperty(results.Count));
        }

        private async Task<IReadOnlyList<CalendarData>> GetGeneratedSchedules(GetCalendarScheduleV2Request param)
        {
            // only role TEACHER & STUDENT that have MsScheduleLesson
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

            var predicateSchedule = PredicateBuilder.Create<MsScheduleLesson>(x
                => x.IsGenerated
                && x.Lesson.AcademicYear.IdSchool == param.IdSchool.FirstOrDefault()
                && x.ScheduleDate.Date >= param.StartDate.Date
                && x.ScheduleDate.Date <= param.EndDate.Date);

            var predicateScheduleLesson = PredicateBuilder.Create<MsSchedule>(x => x.Lesson.AcademicYear.IdSchool == param.IdSchool.FirstOrDefault());
            var predicateLessonTeacher = PredicateBuilder.Create<MsLessonTeacher>(e => true);
            var predicateSubtitute = PredicateBuilder.Create<TrScheduleRealization2>(x => x.ScheduleDate >= param.StartDate && x.ScheduleDate <= param.EndDate && x.AcademicYear.IdSchool == param.IdSchool.FirstOrDefault());

            if (!string.IsNullOrEmpty(param.IdAcadyear))
            {
                predicateSchedule = predicateSchedule.And(x => x.Lesson.IdAcademicYear == param.IdAcadyear);
                predicateScheduleLesson = predicateScheduleLesson.And(x => x.Lesson.IdAcademicYear == param.IdAcadyear);
                predicateLessonTeacher = predicateLessonTeacher.And(x => x.Lesson.IdAcademicYear == param.IdAcadyear);
                predicateSubtitute = predicateSubtitute.And(x => x.IdAcademicYear == param.IdAcadyear);
            }

            if (param.IdUser != null && param.Role == RoleConstant.Teacher)
            {
                predicateLessonTeacher = predicateLessonTeacher.And(e => param.IdUser.Contains(e.IdUser));
                predicateScheduleLesson = predicateScheduleLesson.And(e => param.IdUser.Contains(e.IdUser));
            }

            if (param.IdGrade != null)
            {
                predicateLessonTeacher = predicateLessonTeacher.And(e => param.IdGrade.Contains(e.Lesson.IdGrade));
                predicateScheduleLesson = predicateScheduleLesson.And(e => param.IdGrade.Contains(e.Lesson.IdGrade));
            }

            var dataTeacher = _dbContext.Entity<MsLessonTeacher>()
                          .Include(e => e.Staff)
                          .Where(e => e.Lesson.IdAcademicYear == param.IdAcadyear)
                          .Where(predicateLessonTeacher)
                          .Select(e => new
                          {
                              e.Staff.IdBinusian,
                              Name = NameUtil.GenerateFullName(e.Staff.FirstName, e.Staff.LastName),
                              e.IdLesson,
                              e.IsPrimary
                          })
                          .ToList();

            var listIdLesson = await _dbContext.Entity<MsSchedule>()
                          .Include(e => e.Lesson)
                          .Where(e => e.Lesson.IdAcademicYear == param.IdAcadyear)
                          .Where(predicateScheduleLesson)
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                          .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcadyear)
                          .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await GetListHomeroomStudentEnrollmentQuery(param, listPeriod);

            var getTrHomeroomStudentEnrollment = RoleConstant.Teacher == param.Role
                                                ? new List<GetHomeroom>()
                                                :
                        await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                              .Include(e => e.SubjectNew)
                              .Include(e => e.LessonNew)
                              .Include(e => e.HomeroomStudent)
                              .Where(x => x.StartDate.Date <= param.EndDate.Date && x.HomeroomStudent.IdStudent == param.IdUser && x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcadyear)
                              .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                              .Select(e => new GetHomeroom
                              {
                                  IdLesson = e.IdLessonNew,
                                  Homeroom = new ItemValueVm
                                  {
                                      Id = e.HomeroomStudent.IdHomeroom,
                                  },
                                  Grade = new CodeWithIdVm
                                  {
                                      Id = e.HomeroomStudent.Homeroom.Grade.Id,
                                      Code = e.HomeroomStudent.Homeroom.Grade.Code,
                                  },
                                  ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                  ClassId = e.LessonNew.ClassIdGenerated,
                                  IdHomeroomStudent = e.IdHomeroomStudent,
                                  Semester = e.HomeroomStudent.Semester,
                                  EffectiveDate = e.StartDate,
                                  IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                  IsFromMaster = false,
                                  IdStudent = e.HomeroomStudent.IdStudent,
                                  IsDelete = e.IsDelete,
                                  IdSubject = e.IdSubjectNew,
                                  SubjectName = e.SubjectNew.Description,
                                  IsShowHistory = e.IsShowHistory,
                              })
                              .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                        .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                        .ToList();

            var listIdLessonStudent = listStudentEnrollmentUnion
                       .Select(e => e.IdLesson)
                       .ToList();

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                predicateSchedule = predicateSchedule.And(x => listIdLesson.Contains(x.IdLesson) && listIdLessonStudent.Contains(x.IdLesson));
            else if (RoleConstant.Student == param.Role)
                predicateSchedule = predicateSchedule.And(x => listIdLessonStudent.Contains(x.IdLesson));

            if (!string.IsNullOrWhiteSpace(param.IdLevel))
            {
                predicateSchedule = predicateSchedule.And(x => x.IdLevel == param.IdLevel);
                predicateSubtitute = predicateSubtitute.And(x => x.IdLevel == param.IdLevel);
            }

            if (param.IdGrade != null)
            {
                predicateSchedule = predicateSchedule.And(x => param.IdGrade.Contains(x.IdGrade));
                predicateSubtitute = predicateSubtitute.And(x => param.IdGrade.Contains(x.IdGrade));
            }

            if (!string.IsNullOrEmpty(param.IdSubject))
                predicateSchedule = predicateSchedule.And(x => x.IdSubject == param.IdSubject);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicateSchedule = predicateSchedule.And(x => x.Lesson.LessonPathways.Any(s => s.HomeroomPathway.IdHomeroom == param.IdHomeroom));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicateSchedule = predicateSchedule.And(x
                    => EF.Functions.Like(x.VenueName.ToUpper(), $"%{param.Search.ToUpper()}%")
                    || x.Lesson.LessonTeachers.Any(y => (y.Staff.FirstName + " " + y.Staff.LastName).ToUpper().Contains(param.Search.ToUpper()))
                    );

            var listScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                 .Include(x => x.Venue)
                                 .Include(x => x.AcademicYear)
                                 .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                                 .Where(predicateSchedule)
                                 .OrderBy(x => x.ScheduleDate).ToListAsync(CancellationToken);

            var getDataSubtituteTeacher = _dbContext.Entity<TrScheduleRealization2>()
                                 .Include(x => x.Staff)
                                 .Include(x => x.StaffSubtitute)
                                 .Where(predicateSubtitute)
                                 .ToList();

            List<GetStudentHomeroom> listHomeroomStudentUnion = new List<GetStudentHomeroom>();
            if (!string.IsNullOrEmpty(param.IdUser) && param.Role == RoleConstant.Student)
            {
                var listHTrMoveStudentHomeroom = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                                              .Include(e => e.HomeroomStudent)
                                              .Include(e => e.HomeroomNew).ThenInclude(e => e.Grade)
                                              .Include(e => e.HomeroomNew).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                              .Where(x => x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcadyear && x.HomeroomStudent.IdStudent == param.IdUser)
                                              .Select(e => new GetStudentHomeroom
                                              {
                                                  Homeroom = new ItemValueVm
                                                  {
                                                      Id = e.HomeroomNew.Id,
                                                      Description = e.HomeroomNew.Grade.Code + e.HomeroomNew.GradePathwayClassroom.Classroom.Code
                                                  },
                                                  IdStudent = e.HomeroomStudent.IdStudent,
                                                  EffectiveDate = e.StartDate,
                                                  IsFromMaster = false,
                                                  IdGrade = e.HomeroomNew.Grade.Id,
                                                  Semester = e.HomeroomNew.Semester,
                                                  DateIn = e.DateIn,
                                              })
                                              .ToListAsync(CancellationToken);

                var listMsHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                                   .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                   .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                   .Where(x => x.Homeroom.IdAcademicYear == param.IdAcadyear && x.IdStudent == param.IdUser)
                                                   .Select(e => new GetStudentHomeroom
                                                   {
                                                       Homeroom = new ItemValueVm
                                                       {
                                                           Id = e.Homeroom.Id,
                                                           Description = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                                                       },
                                                       IdStudent = e.IdStudent,
                                                       IsFromMaster = true,
                                                       IdGrade = e.Homeroom.Grade.Id,
                                                       Semester = e.Homeroom.Semester,
                                                       DateIn = e.DateIn
                                                   })
                                                   .ToListAsync(CancellationToken);

                listMsHomeroomStudent.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.IdGrade && f.Semester == e.Semester).Select(f => f.AttendanceStartDate).Min());
                listHomeroomStudentEnrollment.ForEach(e => e.Datein = e.EffectiveDate);

                listHomeroomStudentUnion = listMsHomeroomStudent.Union(listHTrMoveStudentHomeroom)
                                                    .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate).ThenBy(e => e.DateIn)
                                                    .ToList();
            }

            var listScheduleNotFix = listScheduleLesson
                .Select(x => new CalendarData
                {
                    //IdUser = param.IdUser,
                    //IdStudent = param.IdUser,
                    IdSchool = x.AcademicYear.IdSchool,
                    Id = x.Id,
                    SubjectName = x.Lesson.Subject.Description,
                    Start = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.StartTime.Hours, x.StartTime.Minutes, 0),
                    End = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.EndTime.Hours, x.EndTime.Minutes, 0),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = Guid.Empty.ToString(),
                        Code = "Schedule",
                        Description = "Class Schedule"
                    },
                    IdSubject = x.IdSubject,
                    Venue = new ItemValueVm(x.IdVenue, x.VenueName),
                    Department = new ItemValueVm(x.IdSubject), // temporary store IdSubject to get department later
                    ScheduleDate = x.ScheduleDate,
                    CreatedBy = "",
                    ClassId = x.ClassID,
                    Description = "",
                    IsChange = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID && y.ClassID == x.ClassID && (y.Status == "Subtituted" || y.Status == "Venue Change" || y.Status == "Subtituted & Venue Change")).FirstOrDefault() != null ? true : false,
                    IsCancel = getDataSubtituteTeacher.Where(y => y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID && y.ClassID == x.ClassID && y.IsCancel).FirstOrDefault() != null ? true : false,
                    IdLesson = x.IdLesson,
                    IsRemove = false,
                    Semester = x.Lesson.Semester.ToString(),
                    SessionID = x.SessionID,
                    IdGrade = x.IdGrade
                })
                .ToList();

            List<CalendarData> results = new List<CalendarData>();
            foreach (var item in listScheduleNotFix)
            {
                var semester = listPeriod
                               .Where(e => e.IdGrade == item.IdGrade
                                           && e.StartDate.Date <= Convert.ToDateTime(item.ScheduleDate).Date
                                           && e.EndDate >= Convert.ToDateTime(item.ScheduleDate).Date)
                               .Select(e => e.Semester)
                               .FirstOrDefault();

                var listStudentEnrollment = listHomeroomStudentEnrollment
                                                .Where(e => e.IdLesson == item.IdLesson && e.Semester == semester)
                                                .ToList();

                var idHomeroom = listHomeroomStudentEnrollment
                            .Where(e => e.IdLesson == item.IdLesson && e.Semester.ToString() == item.Semester)
                            .Select(e => e.Homeroom.Id)
                            .FirstOrDefault();

                var homeroom = listHomeroomStudentEnrollment
                            .Where(e => e.IdLesson == item.IdLesson && e.Semester.ToString() == item.Semester)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Homeroom.Id,
                                Description = e.Grade.Code + e.ClassroomCode
                            }).FirstOrDefault();

                if (!string.IsNullOrEmpty(param.IdUser) && param.Role == RoleConstant.Student)
                {
                    homeroom = listHomeroomStudentUnion
                                    .Where(e => e.EffectiveDate.Date <= Convert.ToDateTime(item.ScheduleDate))
                                    .Select(e => new ItemValueVm
                                    {
                                        Id = e.Homeroom.Id,
                                        Description = e.Homeroom.Description
                                    })
                                    .LastOrDefault();

                    if (!item.IsChange)
                    {
                        var idLesson = listStudentEnrollmentUnion
                                .Where(e => e.EffectiveDate.Date <= Convert.ToDateTime(item.ScheduleDate) && e.Semester.ToString()==item.Semester && e.IdSubject==item.IdSubject) 
                                .Select(e => e.IdLesson)
                                .LastOrDefault();

                        if (idLesson != item.IdLesson)
                            continue;

                        item.IdLesson = idLesson;
                    }
                }

                if (homeroom == null)
                    continue;

                item.IdHomeroom = homeroom.Id;
                item.Homeroom = homeroom;

                if (!item.IsChange)
                {
                    item.Teacher = dataTeacher
                        .Where(e => e.IdLesson == item.IdLesson && e.IsPrimary)
                        .Select(e => new NameValueVm
                        {
                            Id = e.IdBinusian,
                            Name = e.Name
                        }).FirstOrDefault();

                    if(item.Teacher==null)
                        item.Teacher = dataTeacher
                       .Where(e => e.IdLesson == item.IdLesson)
                       .Select(e => new NameValueVm
                       {
                           Id = e.IdBinusian,
                           Name = e.Name
                       }).FirstOrDefault();
                }
                else
                {
                    item.Teacher = getDataSubtituteTeacher
                        .Where(e => e.ClassID == item.ClassId && e.ScheduleDate == item.ScheduleDate && e.IdLesson == item.IdLesson
                        && item.SessionID == e.SessionID)
                        .Select(e => new NameValueVm
                        {
                            Id = e.IdBinusianSubtitute,
                            Name = e.TeacherNameSubtitute
                        }).FirstOrDefault();

                    item.Venue = getDataSubtituteTeacher
                        .Where(e => e.ClassID == item.ClassId && e.ScheduleDate == item.ScheduleDate && e.IdLesson == item.IdLesson
                        && item.SessionID == e.SessionID)
                        .Select(e => new ItemValueVm
                        {
                            Id = e.IdVenueChange,
                            Description = e.VenueNameChange
                        }).FirstOrDefault();
                }

                if (param.IdUser != null && RoleConstant.Student == param.Role)
                {
                    item.IdUser = param.IdUser;
                    item.IdStudent = param.IdUser;
                    results.Add(item);
                }
                else
                {
                    foreach (var itemStudentEnroll in listStudentEnrollment)
                    {
                        item.IdUser = itemStudentEnroll.IdStudent;
                        item.IdStudent = itemStudentEnroll.IdStudent;
                        results.Add(item);
                    }
                }
            }

            var dateScheduleEnd = _datetimeNow.ServerTime.Date;
            if (results.Count > 0)
            {
                dateScheduleEnd = results.OrderByDescending(x => x.ScheduleDate).Select(x => x.ScheduleDate.Value).FirstOrDefault();
            }

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                              .Where(e => e.IdAcademicYear == param.IdAcadyear && e.ActiveStatus)
                              .Select(e => new
                              {
                                  e.IdStudent,
                                  e.StartDate,
                                  EndDate = e.EndDate == null
                                              ? listPeriod.Select(e => e.AttendanceEndDate).Max()
                                              : Convert.ToDateTime(e.EndDate)
                              })
                              .ToListAsync(CancellationToken);

            foreach (var item in results)
            {
                var semester = listPeriod
                                .Where(e => e.IdGrade == item.IdGrade
                                            && e.StartDate.Date <= Convert.ToDateTime(item.ScheduleDate).Date
                                            && e.EndDate >= Convert.ToDateTime(item.ScheduleDate).Date)
                                .Select(e => e.Semester)
                                .FirstOrDefault();
                //item.Teacher = dataTeacher
                //            .Where(e => e.IdLesson == item.IdLesson && e.IsPrimary)
                //            .Select(e => new NameValueVm
                //            {
                //                Id = e.IdBinusian,
                //                Name = e.Name
                //            }).FirstOrDefault();

                if (param.Role != RoleConstant.Student)
                    continue;

                //student status
                var IsStudentActive = listStudentStatus
                                          .Where(e => e.StartDate.Date <= Convert.ToDateTime(item.ScheduleDate).Date
                                                      && e.EndDate.Date >= Convert.ToDateTime(item.ScheduleDate).Date
                                                      && e.IdStudent == item.IdStudent)
                                          .Select(e => e.IdStudent).Any();
               
                //moving
                var listStudentEnrollmentUnionByStudent = listStudentEnrollmentUnion.Where(e => e.IdStudent == item.IdStudent && e.Semester == semester).ToList();
                var listStudentEnrollmentMoving = SetSchoolEventApprovalStatusHandler.GetMovingStudent(listStudentEnrollmentUnionByStudent, Convert.ToDateTime(item.ScheduleDate), item.Semester.ToString(), item.IdLesson, true);

                var StudentEnrollmentMoving = listStudentEnrollmentMoving
                                                .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                .LastOrDefault();

                if (StudentEnrollmentMoving == null || !IsStudentActive || item.Homeroom == null)
                {
                    item.IsRemove = true;
                    continue;
                }

                if (StudentEnrollmentMoving.IsDelete)
                {
                    item.IsRemove = true;
                }
                else
                {
                    item.IdSubject = StudentEnrollmentMoving.IdSubject;
                    item.IdLesson = StudentEnrollmentMoving.IdLesson;
                    item.ClassId = StudentEnrollmentMoving.ClassId;
                    item.SubjectName = StudentEnrollmentMoving.SubjectName;
                    //item.Teacher = dataTeacher
                    //        .Where(e => e.IdLesson == item.IdLesson && e.IsPrimary)
                    //        .Select(e => new NameValueVm
                    //        {
                    //            Id = e.IdBinusian,
                    //            Name = e.Name
                    //        }).FirstOrDefault();
                }

            }

            results = results.Where(e => !e.IsRemove).ToList();

            return results;
        }

        private async Task<List<GetHomeroom>> GetListHomeroomStudentEnrollmentQuery(GetCalendarScheduleV2Request param, List<MsPeriod> listPeriod)
        {
            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                          .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                          .Include(e => e.TrHomeroomStudentEnrollments)
                          .Include(e => e.HomeroomStudent).ThenInclude(x => x.Homeroom).ThenInclude(x => x.Grade)
                          .Include(e => e.HomeroomStudent).ThenInclude(x => x.Homeroom).ThenInclude(x => x.GradePathwayClassroom).ThenInclude(x => x.Classroom)
                          .Where(e => e.Lesson.AcademicYear.IdSchool == param.IdSchool.FirstOrDefault());

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.Lesson.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.IdUser) && param.Role == RoleConstant.Student)
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.IdStudent == param.IdUser);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.Homeroom.Semester == param.Semester);

            var data = await queryHomeroomStudentEnrollment
                .GroupBy(e => new
                {
                    e.IdLesson,
                    e.IdHomeroomStudent,
                    e.HomeroomStudent.IdHomeroom,
                    e.HomeroomStudent.IdStudent,
                    idGrade = e.HomeroomStudent.Homeroom.Grade.Id,
                    gradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                    classroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    e.Id,
                    classId = e.Lesson.ClassIdGenerated,
                    e.HomeroomStudent.Semester,
                    e.Lesson.IdSubject,
                    subjectName = e.Lesson.Subject.Description
                })
                .Select(e => new GetHomeroom
                {
                    IdLesson = e.Key.IdLesson,
                    Homeroom = new ItemValueVm
                    {
                        Id = e.Key.IdHomeroom,
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = e.Key.idGrade,
                        Code = e.Key.gradeCode,
                    },
                    ClassroomCode = e.Key.classroomCode,
                    ClassId = e.Key.classId,
                    IdHomeroomStudent = e.Key.IdHomeroomStudent,
                    IdStudent = e.Key.IdStudent,
                    Semester = e.Key.Semester,
                    IdHomeroomStudentEnrollment = e.Key.Id,
                    IsFromMaster = true,
                    IsDelete = false,
                    IdSubject = e.Key.IdSubject,
                    SubjectName = e.Key.subjectName,
                    IsShowHistory = false,
                })
                .ToListAsync(CancellationToken);

            data.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).DefaultIfEmpty().Min());

            return data;
        }

        private async Task<IReadOnlyList<CalendarData>> GetClassDiaries(GetCalendarScheduleV2Request param)
        {
            // only role TEACHER & STUDENT that have TrClassDiary
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

            //var listLessonByUser = await GetLessonByUserClassDiary(_dbContext, CancellationToken, param.IdUserLogin, param.IdAcadyear);

            var query =
                from ClassDiary in _dbContext.Entity<TrClassDiary>()
                join Homeroom in _dbContext.Entity<MsHomeroom>() on ClassDiary.IdHomeroom equals Homeroom.Id
                join LessonTeacher in _dbContext.Entity<MsLessonTeacher>() on ClassDiary.IdLesson equals LessonTeacher.IdLesson
                join UserTeacher in _dbContext.Entity<MsUser>() on LessonTeacher.IdUser equals UserTeacher.Id into JoinedUserTeacher
                from UserTeacher in JoinedUserTeacher.DefaultIfEmpty()
                join UserCreate in _dbContext.Entity<MsUser>() on ClassDiary.ClassDiaryUserCreate equals UserCreate.Id into JoinedUserCreate
                from UserCreate in JoinedUserCreate.DefaultIfEmpty()
                join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on Homeroom.Id equals HomeroomStudent.IdHomeroom into JoinedHomeroomStudent
                from HomeroomStudent in JoinedHomeroomStudent.DefaultIfEmpty()
                join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on Homeroom.IdAcademicYear equals AcademicYear.Id
                join ClassDiaryTypeSetting in _dbContext.Entity<MsClassDiaryTypeSetting>() on ClassDiary.IdClassDiaryTypeSetting equals ClassDiaryTypeSetting.Id
                join Lesson in _dbContext.Entity<MsLesson>() on ClassDiary.IdLesson equals Lesson.Id
                join Subject in _dbContext.Entity<MsSubject>() on Lesson.IdSubject equals Subject.Id
                where ClassDiary.ClassDiaryDate.Date >= param.StartDate.Date && ClassDiary.ClassDiaryDate.Date <= param.EndDate.Date && AcademicYear.Id == param.IdAcadyear
                select new //CalendarData
                {
                    IdUser = LessonTeacher.IdUser,
                    IdStudent = RoleConstant.Student == param.Role ? HomeroomStudent.IdStudent : "",
                    IdSchool = AcademicYear.IdSchool,
                    Id = ClassDiary.Id,
                    SubjectName = ClassDiary.ClassDiaryTopic,
                    Start = new DateTime(ClassDiary.ClassDiaryDate.Year, ClassDiary.ClassDiaryDate.Month, ClassDiary.ClassDiaryDate.Day, 0, 0, 0),
                    End = new DateTime(ClassDiary.ClassDiaryDate.Year, ClassDiary.ClassDiaryDate.Month, ClassDiary.ClassDiaryDate.Day, 0, 0, 0),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = Guid.Empty.ToString(),
                        Color = RoleConstant.Student == param.Role ? "#7B2183" : "#8BB26A",
                        Description = "Class Diary"
                    },
                    IdHomeroom = Homeroom.Id,
                    IdSubject = Subject.Id,
                    Teacher = new NameValueVm(LessonTeacher.IdUser, UserTeacher.DisplayName),
                    Venue = new ItemValueVm(ClassDiaryTypeSetting.Id, ClassDiaryTypeSetting.TypeName),
                    Homeroom = new ItemValueVm(Homeroom.Id, Grade.Code + Classroom.Code),
                    Department = new ItemValueVm(Subject.Id), // temporary store IdSubject to get department later
                    ScheduleDate = ClassDiary.DateUp,
                    CreatedBy = UserCreate.DisplayName,
                    ClassId = Lesson.ClassIdGenerated,
                    Description = ClassDiary.ClassDiaryDescription,
                    IdLevel = Grade.IdLevel,
                    IdGrade = Grade.Id,
                    IdAcademicYear = AcademicYear.Id,
                    IdLesson = ClassDiary.IdLesson,
                };

            var coba = query.ToList();

            // filter by role
            //var listIdLesson = listLessonByUser.Select(e => e.IdLesson).Distinct().ToList();
            var listIdLesson = await GetListIdLessonByUserRedis(_dbContext, CancellationToken, param.IdUserLogin, param.IdAcadyear);

            if (RoleConstant.Student == param.Role)
            {
                query = query.Where(x => x.IdStudent == param.IdUser || x.CreatedBy == param.IdUser);
            }
            else
            {
                if (RoleConstant.Parent != param.Role)
                {
                    if (listIdLesson.Any())
                    {
                        query = query.Where(x => listIdLesson.Contains(x.IdLesson) || x.CreatedBy == param.IdUser);
                        var coba4 = query.ToList();

                    }
                    else
                        query = query.Where(x => x.IdUser == param.IdUser || x.CreatedBy == param.IdUser);
                }
                else
                    query = query.Where(x => x.IdUser == param.IdUser);

            }
            var coba2 = query.ToList();

            // chained filter, acadyear to homeroom
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            else if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            else if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            else if (!string.IsNullOrEmpty(param.IdAcadyear))
                query = query.Where(x => x.IdAcademicYear == param.IdAcadyear);

            if (!string.IsNullOrEmpty(param.IdSubject))
                query = query.Where(x => x.IdSubject == param.IdSubject);

            var coba1 = query.ToList();

            var results = await query
                .Select(e => new CalendarData
                {
                    IdUser = e.IdUser,
                    IdStudent = e.IdStudent,
                    IdSchool = e.IdSchool,
                    Id = e.Id,
                    SubjectName = e.SubjectName,
                    Start = e.Start,
                    End = e.End,
                    EventType = e.EventType,
                    IdHomeroom = e.IdHomeroom,
                    IdSubject = e.IdSubject,
                    Teacher = e.Teacher,
                    Venue = e.Venue,
                    Homeroom = e.Homeroom,
                    Department = e.Department,
                    ScheduleDate = e.ScheduleDate,
                    CreatedBy = e.CreatedBy,
                    ClassId = e.ClassId,
                    Description = e.Description,
                    IdAcademicYear = e.IdAcademicYear,
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            if (RoleConstant.Student == param.Role)
            {
                if (results.Count != 0)
                {
                    results = results.GroupBy(x => x.Id).Select(x => x.First()).ToList();
                }
            }

            return results;
        }

        private async Task<List<string>> GetListIdLessonByUserRedis(ISchedulingDbContext dbContext, CancellationToken cancellationToken, string idUserLogin, string idAcadyear)
        {
            var key = $"{GetType().FullName}.{nameof(GetListIdLessonByUserRedis)}-{idUserLogin}-{idAcadyear}";

            var data = await _redisCache.GetListByPatternAsync<string>(key);

            if (data == null)
            {
                var list = await GetLessonByUserClassDiary(_dbContext, CancellationToken, idUserLogin, idAcadyear);
                data = list.Select(e => e.IdLesson).Distinct().ToList();

                await _redisCache.SetListAsync(key, data, TimeSpan.FromMinutes(5));
            }

            return data;
        }

        private async Task<IReadOnlyList<CalendarData>> GetSchoolEvents(GetCalendarScheduleV2Request param)
        {
            var query =
                from Event in _dbContext.Entity<TrEvent>()
                join User in _dbContext.Entity<MsUser>() on Event.UserIn equals User.Id
                join EventCoordinator in _dbContext.Entity<TrEventCoordinator>() on Event.Id equals EventCoordinator.IdEvent
                join UserCoordinator in _dbContext.Entity<MsUser>() on EventCoordinator.IdUser equals UserCoordinator.Id
                join EventType in _dbContext.Entity<MsEventType>() on Event.IdEventType equals EventType.Id
                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on EventType.IdAcademicYear equals AcademicYear.Id
                join School in _dbContext.Entity<MsSchool>() on AcademicYear.IdSchool equals School.Id
                join EventDetail in _dbContext.Entity<TrEventDetail>() on Event.Id equals EventDetail.IdEvent
                join UserEvent in _dbContext.Entity<TrUserEvent>() on EventDetail.Id equals UserEvent.IdEventDetail
                join EventIntendedFor in _dbContext.Entity<TrEventIntendedFor>() on Event.Id equals EventIntendedFor.IdEvent into leftEventIntendedFor
                from subEventIntendedFor in leftEventIntendedFor.DefaultIfEmpty()
                join EventIntendedForGradeStudent in _dbContext.Entity<TrEventIntendedForGradeStudent>() on subEventIntendedFor.Id equals EventIntendedForGradeStudent.IdEventIntendedFor into leftEventIntendedForGradeStudent
                from subEventIntendedForGradeStudent in leftEventIntendedForGradeStudent.DefaultIfEmpty()
                join Homeroom in _dbContext.Entity<MsHomeroom>() on subEventIntendedForGradeStudent.IdHomeroom equals Homeroom.Id into leftHomeroom
                from subHomeroom in leftHomeroom.DefaultIfEmpty()
                join Grade in _dbContext.Entity<MsGrade>() on subHomeroom.IdGrade equals Grade.Id into leftGrade
                from subGrade in leftGrade.DefaultIfEmpty()
                join EventIntendedForLevelStudent in _dbContext.Entity<TrEventIntendedForLevelStudent>() on subEventIntendedFor.Id equals EventIntendedForLevelStudent.IdEventIntendedFor into leftEventIntendedForLevelStudent
                from subEventIntendedForLevelStudent in leftEventIntendedForLevelStudent.DefaultIfEmpty()
                where
                Event.IdAcademicYear == param.IdAcadyear &&
                Event.StatusEvent == "Approved" &&
                Event.IsShowOnSchedule == true &&
                (
                    (param.StartDate.Date <= EventDetail.StartDate.Date || param.StartDate.Date <= EventDetail.EndDate.Date) &&
                    (param.EndDate.Date >= EventDetail.StartDate.Date || param.EndDate.Date >= EventDetail.EndDate.Date)
                )
                select new /*CalendarData*/
                {
                    IsShowOnSchedule = Event.IsShowOnSchedule,
                    IdAcademicYear = Event.IdAcademicYear,
                    IdUser = UserEvent.IdUser,
                    IdStudent = string.Empty,
                    IdSchool = AcademicYear.IdSchool,
                    Id = Event.Id,
                    SubjectName = Event.Name,
                    Start = new DateTime(EventDetail.StartDate.Year, EventDetail.StartDate.Month, EventDetail.StartDate.Day, EventDetail.StartDate.Hour, EventDetail.StartDate.Minute, 0),
                    End = new DateTime(EventDetail.EndDate.Year, EventDetail.EndDate.Month, EventDetail.EndDate.Day, EventDetail.EndDate.Hour, EventDetail.EndDate.Minute, 0),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = EventType.Id,
                        Code = EventType.Code,
                        Description = EventType.Description,
                        Color = EventType.Color
                    },
                    IdHomeroom = string.Empty,
                    IdSubject = string.Empty,
                    Teacher = new NameValueVm(UserCoordinator.Id, UserCoordinator.DisplayName),
                    Venue = new ItemValueVm(Event.Place, Event.Place),
                    Homeroom = new ItemValueVm(Event.Place),
                    Department = new ItemValueVm(Event.Place), // temporary store IdSubject to get department later
                    ScheduleDate = EventDetail.StartDate,
                    CreatedBy = User.DisplayName,
                    ClassId = string.Empty,
                    Description = string.Empty,
                    IdSubgrade = subGrade.Id == null ? param.IdGrade : subGrade.Id,
                    IdsubEventIntendedForLevelStudent = subEventIntendedForLevelStudent.IdLevel == null
                                                            ? param.IdLevel : subEventIntendedForLevelStudent.IdLevel,
                    IntendedFor = subEventIntendedFor.IntendedFor
                };

            var coba = query.ToList();
            if (!string.IsNullOrEmpty(param.IdUser))
                query = query.Where(x => x.IdUser == param.IdUser);
            var coba1 = query.ToList();
            // chained filter
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdsubEventIntendedForLevelStudent == param.IdLevel);
            var coba2 = query.ToList();
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdSubgrade == param.IdGrade);
            var coba3 = query.ToList();
            if (!string.IsNullOrEmpty(param.IdSubject))
                query = query.Where(x => x.IdSubject == param.IdSubject);
            var coba4 = query.ToList();
            var results = await query
                .Select(x => new CalendarData
                {
                    IsShowOnSchedule = x.IsShowOnSchedule,
                    IdAcademicYear = x.IdAcademicYear,
                    IdUser = x.IdUser,
                    IdStudent = string.Empty,
                    IdSchool = x.IdSchool,
                    Id = x.Id,
                    SubjectName = x.SubjectName,
                    Start = x.Start,
                    End = x.End,
                    EventType = x.EventType,
                    IdHomeroom = string.Empty,
                    IdSubject = string.Empty,
                    Teacher = x.Teacher,
                    Venue = x.Venue,
                    Homeroom = x.Homeroom,
                    Department = x.Department,
                    ScheduleDate = x.ScheduleDate,
                    CreatedBy = x.CreatedBy,
                    ClassId = string.Empty,
                    Description = string.Empty,
                })
                .Distinct().ToListAsync(CancellationToken);

            return results;
        }

        private async Task<IReadOnlyList<CalendarData>> GetInvitationBooking(GetCalendarScheduleV2Request param)
        {
            // only role TEACHER & STUDENT that have TrClassDiary
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

            var query =
                from InvitationBookingDetail in _dbContext.Entity<TrInvitationBookingDetail>()
                join InvitationBooking in _dbContext.Entity<TrInvitationBooking>() on InvitationBookingDetail.IdInvitationBooking equals InvitationBooking.Id
                join UserTeacher in _dbContext.Entity<MsUser>() on InvitationBooking.IdUserTeacher equals UserTeacher.Id
                join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on InvitationBookingDetail.IdHomeroomStudent equals HomeroomStudent.Id
                join InvitationBookingSetting in _dbContext.Entity<TrInvitationBookingSetting>() on InvitationBooking.IdInvitationBookingSetting equals InvitationBookingSetting.Id
                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on InvitationBookingSetting.IdAcademicYear equals AcademicYear.Id
                join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                join Venue in _dbContext.Entity<MsVenue>() on InvitationBooking.IdVenue equals Venue.Id
                join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                join UserCreate in _dbContext.Entity<MsUser>() on InvitationBooking.UserIn equals UserCreate.Id into JoinedUserCreate
                from UserCreate in JoinedUserCreate.DefaultIfEmpty()
                where (InvitationBooking.StartDateInvitation.Date >= param.StartDate.Date && InvitationBooking.StartDateInvitation.Date <= param.EndDate.Date)
                        && (InvitationBooking.EndDateInvitation.Date >= param.StartDate.Date && InvitationBooking.EndDateInvitation.Date <= param.EndDate.Date)
                select new //CalendarData
                {
                    IdUser = InvitationBooking.IdUserTeacher,
                    IdStudent = HomeroomStudent.IdStudent,
                    IdParent = $"P{HomeroomStudent.IdStudent}",
                    IdSchool = AcademicYear.IdSchool,
                    Id = InvitationBookingDetail.Id,
                    SubjectName = InvitationBookingSetting.InvitationName,
                    Start = InvitationBooking.StartDateInvitation,
                    End = InvitationBooking.EndDateInvitation,
                    EventType = new CalendarEventTypeVm
                    {
                        Id = "",
                        Color = "",
                        Description = "Invitation Booking"
                    },
                    IdHomeroom = Homeroom.Id,
                    IdSubject = "",
                    Teacher = new NameValueVm(UserTeacher.Id, UserTeacher.DisplayName),
                    Venue = new ItemValueVm(Venue.Id, Venue.Description),
                    Homeroom = new ItemValueVm(Homeroom.Id, Grade.Code + Classroom.Code),
                    Department = new ItemValueVm("", ""),
                    ScheduleDate = InvitationBooking.StartDateInvitation,
                    CreatedBy = UserCreate.DisplayName,
                    ClassId = "",
                    Description = InvitationBooking.Description,
                    IdLevel = Grade.IdLevel,
                    IdGrade = Grade.Id,
                    IdAcademicYear = AcademicYear.Id,
                    StudentBooking = InvitationBooking.InvitationBookingDetails.Select(x => new StudentBooking
                    {
                        IdBinusian = x.HomeroomStudent.IdStudent,
                        StudentName = x.HomeroomStudent.Student.FirstName + (x.HomeroomStudent.Student.MiddleName == null ? "" : " " + x.HomeroomStudent.Student.MiddleName) + (x.HomeroomStudent.Student.LastName == null ? "" : " " + x.HomeroomStudent.Student.LastName),
                        Date = x.InvitationBooking.StartDateInvitation.Date,
                        Time = x.InvitationBooking.StartDateInvitation.TimeOfDay,
                        IdInvitationBooking = x.IdInvitationBooking,
                    }).ToList(),
                };

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                query = query.Where(x => x.IdUser == param.IdUser);
            else if (RoleConstant.Student == param.Role)
                query = query.Where(x => x.IdStudent == param.IdUser);
            else if (RoleConstant.Parent == param.Role)
                query = query.Where(x => x.IdParent == param.IdUser);

            // chained filter, acadyear to homeroom
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                query = query.Where(x => x.IdAcademicYear == param.IdAcadyear);

            if (!string.IsNullOrEmpty(param.IdSubject))
                query = query.Where(x => x.IdSubject == param.IdSubject);

            var listInvitationBooking = await query
                .Select(e => new CalendarData
                {
                    IdUser = e.IdUser,
                    IdStudent = e.IdStudent,
                    IdSchool = e.IdSchool,
                    Id = e.Id,
                    SubjectName = e.SubjectName,
                    Start = e.Start,
                    End = e.End,
                    EventType = e.EventType,
                    IdHomeroom = e.IdHomeroom,
                    IdSubject = e.IdSubject,
                    Teacher = e.Teacher,
                    Venue = e.Venue,
                    Homeroom = e.Homeroom,
                    Department = e.Department,
                    ScheduleDate = e.ScheduleDate,
                    CreatedBy = e.CreatedBy,
                    ClassId = e.ClassId,
                    Description = e.Description,
                    IdAcademicYear = e.IdAcademicYear,
                    StudentBooking = e.StudentBooking
                })
                .ToListAsync(CancellationToken);

            var results = listInvitationBooking
                .Select(e => new CalendarData
                {
                    IdUser = e.IdUser,
                    IdStudent = e.IdStudent,
                    IdSchool = e.IdSchool,
                    Id = e.Id,
                    SubjectName = e.SubjectName,
                    Start = e.Start,
                    End = e.End,
                    EventType = e.EventType,
                    IdHomeroom = e.IdHomeroom,
                    IdSubject = e.IdSubject,
                    Teacher = e.Teacher,
                    Venue = e.Venue,
                    Homeroom = e.Homeroom,
                    Department = e.Department,
                    ScheduleDate = e.ScheduleDate,
                    CreatedBy = e.CreatedBy,
                    ClassId = e.ClassId,
                    Description = e.Description,
                    IdAcademicYear = e.IdAcademicYear,
                    StudentBooking = e.StudentBooking
                })
                .Distinct()
                .ToList();

            return results;
        }

        private async Task<IReadOnlyList<CalendarData>> GetPersonalInvitation(GetCalendarScheduleV2Request param)
        {
            // only role TEACHER & STUDENT that have TrClassDiary
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

            var query =
                from PersonalInvitation in _dbContext.Entity<TrPersonalInvitation>()
                join UserTeacher in _dbContext.Entity<MsUser>() on PersonalInvitation.IdUserTeacher equals UserTeacher.Id
                join AcademicYear in _dbContext.Entity<MsAcademicYear>() on PersonalInvitation.IdAcademicYear equals AcademicYear.Id
                join HomeroomStudent in _dbContext.Entity<MsHomeroomStudent>() on PersonalInvitation.IdStudent equals HomeroomStudent.IdStudent
                join Homeroom in _dbContext.Entity<MsHomeroom>() on HomeroomStudent.IdHomeroom equals Homeroom.Id
                join Venue in _dbContext.Entity<MsVenue>() on PersonalInvitation.IdVenue equals Venue.Id into JoinedVenue
                from Venue in JoinedVenue.DefaultIfEmpty()
                join Grade in _dbContext.Entity<MsGrade>() on Homeroom.IdGrade equals Grade.Id
                join GradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on Homeroom.IdGradePathwayClassRoom equals GradePathwayClassroom.Id
                join Classroom in _dbContext.Entity<MsClassroom>() on GradePathwayClassroom.IdClassroom equals Classroom.Id
                join UserCreate in _dbContext.Entity<MsUser>() on PersonalInvitation.UserIn equals UserCreate.Id into JoinedUserCreate
                from UserCreate in JoinedUserCreate.DefaultIfEmpty()
                where (PersonalInvitation.InvitationDate.Date >= param.StartDate.Date && PersonalInvitation.InvitationDate.Date <= param.EndDate.Date) 
                        && (PersonalInvitation.Status==PersonalInvitationStatus.Approved || PersonalInvitation.Status == PersonalInvitationStatus.NoApproval)
                select new //CalendarData
                {
                    IdUser = PersonalInvitation.IdUserTeacher,
                    IdStudent = PersonalInvitation.IdStudent,
                    IdSchool = AcademicYear.IdSchool,
                    Id = PersonalInvitation.Id,
                    IdParent = PersonalInvitation.IsMother || PersonalInvitation.IsFather ? $"P{PersonalInvitation.IdStudent}" : null,
                    SubjectName = "Personal Invitation",
                    Start = PersonalInvitation.InvitationDate + PersonalInvitation.InvitationStartTime,
                    End = PersonalInvitation.InvitationDate + PersonalInvitation.InvitationEndTime,
                    EventType = new CalendarEventTypeVm
                    {
                        Id = "",
                        Color = "",
                        Description = "Personal Invitation"
                    },
                    IdHomeroom = Homeroom.Id,
                    IdSubject = "",
                    Teacher = new NameValueVm(UserTeacher.Id, UserTeacher.DisplayName),
                    Venue = new ItemValueVm(Venue.Id, Venue.Description),
                    Homeroom = new ItemValueVm(Homeroom.Id, Grade.Code + Classroom.Code),
                    Department = new ItemValueVm("", ""),
                    ScheduleDate = PersonalInvitation.InvitationDate,
                    CreatedBy = UserCreate.DisplayName,
                    ClassId = "",
                    Description = PersonalInvitation.Description,
                    IdLevel = Grade.IdLevel,
                    IdGrade = Grade.Id,
                    IdAcademicYear = AcademicYear.Id,
                    Semester = Homeroom.Semester,
                };

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                query = query.Where(x => x.IdUser == param.IdUser);
            else if (RoleConstant.Student == param.Role)
                query = query.Where(x => x.IdStudent == param.IdUser);
            else if (RoleConstant.Parent == param.Role)
                query = query.Where(x => x.IdParent == param.IdUser);

            // chained filter, acadyear to homeroom
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            if (string.IsNullOrEmpty(param.IdHomeroom))
                query = query.Where(x => x.Semester == 1);
            if (!string.IsNullOrEmpty(param.IdGrade))
                query = query.Where(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                query = query.Where(x => x.IdAcademicYear == param.IdAcadyear);

            if (!string.IsNullOrEmpty(param.IdSubject))
                query = query.Where(x => x.IdSubject == param.IdSubject);

            var results = await query
                .Select(e => new CalendarData
                {
                    IdUser = e.IdUser,
                    IdStudent = e.IdStudent,
                    IdSchool = e.IdSchool,
                    Id = e.Id,
                    SubjectName = e.SubjectName,
                    Start = e.Start,
                    End = e.End,
                    EventType = e.EventType,
                    IdHomeroom = e.IdHomeroom,
                    IdSubject = e.IdSubject,
                    Teacher = e.Teacher,
                    Venue = e.Venue,
                    Homeroom = e.Homeroom,
                    Department = e.Department,
                    ScheduleDate = e.ScheduleDate,
                    CreatedBy = e.CreatedBy,
                    ClassId = e.ClassId,
                    Description = e.Description,
                    IdAcademicYear = e.IdAcademicYear,
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            return results;
        }

        public static async Task<List<GetIdLessonByUserResult>> GetLessonByUserClassDiary(ISchedulingDbContext _dbContext, System.Threading.CancellationToken CancellationToken, string IdUser, string IdAcademicYear)
        {
            List<GetIdLessonByUserResult> listLessonByUser = new List<GetIdLessonByUserResult>();
            List<string> listCodePosition = new List<string>()
                                        {
                                            PositionConstant.HeadOfDepartment,
                                            PositionConstant.SubjectHead,
                                            PositionConstant.LevelHead,
                                            PositionConstant.ClassAdvisor,
                                            PositionConstant.AffectiveCoordinator,
                                            PositionConstant.SubjectTeacher,
                                            PositionConstant.VicePrincipal,
                                            PositionConstant.Principal
                                        };

            var listLessonUser = await GetLessonByUser(_dbContext, CancellationToken, IdUser, IdAcademicYear);
            var listLessonByPositionCode = listLessonUser.Where(e => listCodePosition.Contains(e.PositionCode)).ToList();

            return listLessonByPositionCode;
        }

        public static async Task<List<GetIdLessonByUserResult>> GetLessonByUser(ISchedulingDbContext _dbContext, System.Threading.CancellationToken CancellationToken, string IdUser, string IdAcademicYear)
        {
            List<GetIdLessonByUserResult> listLessonByUser = new List<GetIdLessonByUserResult>();

            var idSchool = await _dbContext.Entity<MsAcademicYear>()
                                   .Where(x => x.Id == IdAcademicYear)
                                   .Select(e => e.IdSchool)
                                   .FirstOrDefaultAsync(CancellationToken);

            var listTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                       .Include(e => e.Position)
                                       .Where(x => x.IdSchool == idSchool)
                                       .Select(e => new
                                       {
                                           Id = e.Id,
                                           PositionCode = e.Position.Code,
                                       })
                                       .ToListAsync(CancellationToken);


            #region Get Lesson
            List<GetIdLessonByUserResult> listSubjectByUser = new List<GetIdLessonByUserResult>();
            var listLesson = await _dbContext.Entity<MsLesson>()
                                .Include(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Subject)
                                .Include(e => e.Schedules).ThenInclude(e => e.User)
                                .Include(e => e.LessonPathways).ThenInclude(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.LessonPathways).ThenInclude(e => e.HomeroomPathway).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(x => x.Grade.Level.IdAcademicYear == IdAcademicYear && x.HomeroomStudentEnrollments.Any() && x.ScheduleLesson.Any())
                                .ToListAsync(CancellationToken);

            List<GetIdLessonByUserResult> listLessonFullSet = new List<GetIdLessonByUserResult>();
            foreach (var item in listLesson)
            {
                var listEnroll = item.LessonPathways
                                    .Where(e => e.IdLesson == item.Id)
                                    .GroupBy(e => new
                                    {
                                        IdLevel = e.HomeroomPathway.Homeroom.Grade.Level.Id,
                                        Level = e.HomeroomPathway.Homeroom.Grade.Level.Description,
                                        IdGrade = e.HomeroomPathway.Homeroom.Grade.Id,
                                        Grade = e.HomeroomPathway.Homeroom.Grade.Description,
                                        GradeCode = e.HomeroomPathway.Homeroom.Grade.Code,
                                        IdHomeroom = e.HomeroomPathway.Homeroom.Id,
                                        ClassroomCode = e.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                                    })
                                    .Select(e => e.Key)
                                    .ToList();

                var listSchedule = item.Schedules
                                    .Where(e => e.IdLesson == item.Id)
                                    .GroupBy(e => new
                                    {
                                        IdTeacher = e.IdUser,
                                        Teacher = NameUtil.GenerateFullName(e.User.FirstName, e.User.LastName),
                                    })
                                    .Select(e => e.Key)
                                    .ToList();

                foreach (var itemEnroll in listEnroll)
                {
                    foreach (var itemSchedule in listSchedule)
                    {
                        GetIdLessonByUserResult newLessonFullSet = new GetIdLessonByUserResult
                        {
                            IdLesson = item.Id,
                            ClassId = item.ClassIdGenerated,
                            Semester = item.Semester,
                            Level = new ItemValueVm
                            {
                                Id = item.Grade.IdLevel,
                                Description = item.Grade.Level.Description
                            },
                            Grade = new ItemValueVm
                            {
                                Id = item.Grade.Id,
                                Description = item.Grade.Description
                            },
                            Homeroom = new CodeWithIdVm
                            {
                                Id = itemEnroll.IdHomeroom,
                                Code = itemEnroll.ClassroomCode,
                                Description = itemEnroll.GradeCode + itemEnroll.ClassroomCode,
                            },
                            Subject = new ItemValueVm
                            {
                                Id = item.IdSubject,
                                Description = item.Subject.Description
                            },
                            Teacher = new ItemValueVm
                            {
                                Id = itemSchedule.IdTeacher,
                                Description = itemSchedule.Teacher
                            },

                        };

                        listLessonFullSet.Add(newLessonFullSet);
                    }
                }
            }

            listLessonFullSet = listLessonFullSet
                        .GroupBy(e => new
                        {
                            IdLesson = e.IdLesson,
                            ClassId = e.ClassId,
                            Semester = e.Semester,
                            IdLevel = e.Level.Id,
                            Level = e.Level.Description,
                            IdGrade = e.Grade.Id,
                            Grade = e.Grade.Description,
                            IdHomeroom = e.Homeroom.Id,
                            HomeroomCode = e.Homeroom.Code,
                            Homeroom = e.Homeroom.Description,
                            IdSubject = e.Subject.Id,
                            Subject = e.Subject.Description,
                            IdTeacher = e.Teacher.Id,
                            Teacher = e.Teacher.Description,
                        })
                        .Select(e => new GetIdLessonByUserResult
                        {
                            IdLesson = e.Key.IdLesson,
                            ClassId = e.Key.ClassId,
                            Semester = e.Key.Semester,
                            Level = new ItemValueVm
                            {
                                Id = e.Key.IdLevel,
                                Description = e.Key.Level
                            },
                            Grade = new ItemValueVm
                            {
                                Id = e.Key.IdGrade,
                                Description = e.Key.Grade
                            },
                            Homeroom = new CodeWithIdVm
                            {
                                Id = e.Key.IdHomeroom,
                                Code = e.Key.HomeroomCode,
                                Description = e.Key.Homeroom,
                            },
                            Subject = new ItemValueVm
                            {
                                Id = e.Key.IdSubject,
                                Description = e.Key.Subject
                            },
                            Teacher = new ItemValueVm
                            {
                                Id = e.Key.IdTeacher,
                                Description = e.Key.Teacher
                            }
                        }).ToList();
            #endregion

            #region ST
            var positionCodeBySubjectTeacher = listTeacherPosition
                                                    .Where(e => e.PositionCode == PositionConstant.SubjectTeacher)
                                                    .Select(e => e.PositionCode)
                                                    .ToList();

            foreach (var itemPositionCode in positionCodeBySubjectTeacher)
            {
                var listIdLesson = listLessonByUser.Select(f => f.IdLesson).Distinct().ToList();
                var listLessonBySt = listLessonFullSet.Where(e => e.Teacher.Id == IdUser && !listIdLesson.Contains(e.IdLesson)).ToList();
                listLessonBySt.ForEach(e => e.PositionCode = itemPositionCode);
                listLessonByUser.AddRange(listLessonBySt);
            }
            #endregion

            #region CA
            var listHomeroomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
               .Include(e => e.TeacherPosition).ThenInclude(e => e.Position)
               .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
               .Where(x => x.IdBinusian == IdUser && x.Homeroom.IdAcademicYear == IdAcademicYear)
               .Select(e => new
               {
                   e.IdHomeroom,
                   PositionCode = e.TeacherPosition.Position.Code
               })
               .Distinct().ToListAsync(CancellationToken);

            foreach (var itemHomeroomTeacher in listHomeroomTeacher)
            {
                var listIdLesson = listLessonByUser.Select(f => f.IdLesson).Distinct().ToList();
                var listLessonCaByHomeroom = listLessonFullSet.Where(e => e.Homeroom.Id == itemHomeroomTeacher.IdHomeroom && !listIdLesson.Contains(e.IdLesson)).ToList();
                listLessonCaByHomeroom.ForEach(e => e.PositionCode = itemHomeroomTeacher.PositionCode);
                listLessonByUser.AddRange(listLessonCaByHomeroom);
            }
            #endregion

            #region non teaching load
            var listTeacherNonTeaching = await _dbContext.Entity<TrNonTeachingLoad>()
                                .Include(e => e.MsNonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.Position)
                                 .Where(x => x.IdUser == IdUser && x.MsNonTeachingLoad.IdAcademicYear == IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            var listDepartmentLevel = await _dbContext.Entity<MsDepartmentLevel>()
                                .Include(e => e.Level).ThenInclude(e => e.MsGrades)
                                 .Where(x => x.Level.IdAcademicYear == IdAcademicYear)
                                 .ToListAsync(CancellationToken);

            foreach (var item in listTeacherNonTeaching)
            {
                if (item.Data == null)
                    continue;

                var _dataNewPosition = JsonConvert.DeserializeObject<IDictionary<string, ItemValueVm>>(item.Data);
                _dataNewPosition.TryGetValue("Department", out var _DepartemenPosition);
                _dataNewPosition.TryGetValue("Grade", out var _GradePosition);
                _dataNewPosition.TryGetValue("Level", out var _LevelPosition);
                _dataNewPosition.TryGetValue("Subject", out var _SubjectPosition);

                var listIdLesson = listLessonByUser.Select(f => f.IdLesson).Distinct().ToList();
                if (_SubjectPosition == null && _GradePosition == null && _LevelPosition == null && _DepartemenPosition != null)
                {
                    var getDepartmentLevelbyIdLevel = listDepartmentLevel.Where(e => e.IdDepartment == _DepartemenPosition.Id).Select(e => e.IdLevel).ToList();
                    var listLessonByIdGarde = listLessonFullSet.Where(e => getDepartmentLevelbyIdLevel.Contains(e.Level.Id) && !listIdLesson.Contains(e.IdLesson)).ToList();

                    if (listLessonByIdGarde.Any())
                    {
                        listLessonByIdGarde.ForEach(e => e.PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code);
                        listLessonByUser.AddRange(listLessonByIdGarde);
                    }

                }
                else if (_SubjectPosition != null && _GradePosition != null && _LevelPosition != null && _DepartemenPosition != null)
                {
                    var listLessonByIdSubject = listLessonFullSet.Where(e => e.Subject.Id == _SubjectPosition.Id && !listIdLesson.Contains(e.IdLesson)).ToList();

                    if (listLessonByIdSubject.Any())
                    {
                        listLessonByIdSubject.ForEach(e => e.PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code);
                        listLessonByUser.AddRange(listLessonByIdSubject);
                    }
                }
                else if (_SubjectPosition == null && _GradePosition != null && _LevelPosition != null)
                {
                    var listLessonByIdGrade = listLessonFullSet.Where(e => e.Grade.Id == _GradePosition.Id && !listIdLesson.Contains(e.IdLesson)).ToList();

                    if (listLessonByIdGrade.Any())
                    {
                        listLessonByIdGrade.ForEach(e => e.PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code);
                        listLessonByUser.AddRange(listLessonByIdGrade);
                    }
                }
                else if (_SubjectPosition == null && _GradePosition == null && _LevelPosition != null)
                {
                    var listLessonByIdLevel = listLessonFullSet.Where(e => e.Level.Id == _LevelPosition.Id && !listIdLesson.Contains(e.IdLesson)).ToList();

                    if (listLessonByIdLevel.Any())
                    {
                        listLessonByIdLevel.ForEach(e => e.PositionCode = item.MsNonTeachingLoad.TeacherPosition.Position.Code);
                        listLessonByUser.AddRange(listLessonByIdLevel);
                    }
                }
            }
            #endregion

            #region Staff
            if (!listHomeroomTeacher.Any() && !listHomeroomTeacher.Any() && !listTeacherNonTeaching.Any())
            {
                var listUserRole = await _dbContext.Entity<MsUserRole>()
                                       .Where(x =>
                                            (x.Role.RoleGroup.Code == RoleConstant.SuperAdmin
                                             || x.Role.RoleGroup.Code == RoleConstant.Admin
                                             || x.Role.RoleGroup.Code == RoleConstant.Staff
                                            )
                                            && x.IdUser == IdUser)
                                       .ToListAsync(CancellationToken);

                if (listUserRole.Count > 0)
                {
                    var listIdLesson = listLessonByUser.Select(f => f.IdLesson).Distinct().ToList();
                    var listLessonStaff = listLessonFullSet.Where(e => !listIdLesson.Contains(e.IdLesson)).ToList();
                    listLessonByUser.AddRange(listLessonStaff);
                }
            }
            #endregion

            var listLessonByUserFix = listLessonByUser
                                    .GroupBy(e => new
                                    {
                                        IdLesson = e.IdLesson,
                                        ClassId = e.ClassId,
                                        Semester = e.Semester,
                                        IdLevel = e.Level.Id,
                                        Level = e.Level.Description,
                                        IdGrade = e.Grade.Id,
                                        Grade = e.Grade.Description,
                                        IdHomeroom = e.Homeroom.Id,
                                        HomeroomCode = e.Homeroom.Code,
                                        Homeroom = e.Homeroom.Description,
                                        IdSubject = e.Subject.Id,
                                        Subject = e.Subject.Description,
                                        IdTeacher = e.Teacher.Id,
                                        Teacher = e.Teacher.Description,
                                        PositionCode = e.PositionCode
                                    })
                                    .Select(e => new GetIdLessonByUserResult
                                    {
                                        IdLesson = e.Key.IdLesson,
                                        ClassId = e.Key.ClassId,
                                        Semester = e.Key.Semester,
                                        Level = new ItemValueVm
                                        {
                                            Id = e.Key.IdLevel,
                                            Description = e.Key.Level
                                        },
                                        Grade = new ItemValueVm
                                        {
                                            Id = e.Key.IdGrade,
                                            Description = e.Key.Grade
                                        },
                                        Homeroom = new CodeWithIdVm
                                        {
                                            Id = e.Key.IdHomeroom,
                                            Code = e.Key.HomeroomCode,
                                            Description = e.Key.Homeroom,
                                        },
                                        Subject = new ItemValueVm
                                        {
                                            Id = e.Key.IdSubject,
                                            Description = e.Key.Subject
                                        },
                                        Teacher = new ItemValueVm
                                        {
                                            Id = e.Key.IdTeacher,
                                            Description = e.Key.Teacher
                                        },
                                        PositionCode = e.Key.PositionCode
                                    }).ToList();

            return listLessonByUserFix;
        }

        private class CalendarData
        {
            public string IdUser { get; set; }
            public string IdStudent { get; set; }
            public string IdSchool { get; set; }
            public string Id { get; set; }
            public string SubjectName { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public CalendarEventTypeVm EventType { get; set; }
            public string IdHomeroom { get; set; }
            public string IdSubject { get; set; }
            public NameValueVm Teacher { get; set; }
            public ItemValueVm Venue { get; set; }
            public ItemValueVm Homeroom { get; set; }
            public ItemValueVm Department { get; set; }
            public DateTime? ScheduleDate { get; set; }
            public string CreatedBy { get; set; }
            public string ClassId { get; set; }
            public string Description { get; set; }
            public bool IsShowOnSchedule { get; set; }
            public string IdAcademicYear { get; set; }
            public string IdLesson { get; set; }
            public string Semester { get; set; }
            public List<StudentBooking> StudentBooking { get; set; }
            public bool IsChange { get; set; }
            public bool IsCancel { get; set; }
            public bool IsRemove { get; set; }
            public string SessionID { get; set; }
            public string IdGrade { get; set; }
        }
    }
}
