using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Configurations;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnSchedule.CalendarEvent;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace BinusSchool.Scheduling.FnSchedule.CalendarSchedule
{
    public class GetCalendarScheduleHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<IReadOnlyList<CalendarData>> _emptyCalendarData = new Lazy<IReadOnlyList<CalendarData>>(new List<CalendarData>(0));
        private static readonly Lazy<IReadOnlyList<TrClassDiaryAttachment>> _emptyClassDiaryAttachment = new Lazy<IReadOnlyList<TrClassDiaryAttachment>>(new List<TrClassDiaryAttachment>(0));
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetCalendarScheduleRequest.IdSchool),
            nameof(GetCalendarScheduleRequest.StartDate), nameof(GetCalendarScheduleRequest.EndDate),
            nameof(GetCalendarScheduleRequest.IdUser), nameof(GetCalendarScheduleRequest.Role)
        });

        private readonly ISchedulingDbContext _dbContext;
        //private readonly GetCalendarEvent2Handler _calendarEventHandler;

        public GetCalendarScheduleHandler(ISchedulingDbContext dbContext
            //, GetCalendarEvent2Handler calendarEventHandler
            )
        {
            _dbContext = dbContext;
            //_calendarEventHandler = calendarEventHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCalendarScheduleRequest>(_requiredParams.Value);
            var (schedules, props) = await GetCalendarSchedules(param, Localizer);

            return Request.CreateApiResult2(schedules as object, props);
        }

        public async Task<(IEnumerable<GetCalendarScheduleResult> schedules, IDictionary<string, object> props)> GetCalendarSchedules(GetCalendarScheduleRequest param, IStringLocalizer localizer)
        {
            if (!param.Role.IsConstantOf<RoleConstant>())
                throw new BadRequestException(string.Format(localizer["ExNotExist"], localizer["Role"], "Code", param.Role));

            // convert param date to start until end day
            param.StartDate = new DateTime(param.StartDate.Year, param.StartDate.Month, param.StartDate.Day, 0, 0, 0);
            param.EndDate = new DateTime(param.EndDate.Year, param.EndDate.Month, param.EndDate.Day, 23, 59, 59);

            // fetch evets from CalendarEventHandler
            // var events = await _calendarEventHandler.GetCalendarEvents(
            //     new GetCalendarEvent2Request
            //     {
            //         Ids = param.Ids,
            //         IdSchool = param.IdSchool,
            //         Search = param.Search,
            //         Page = param.Page,
            //         Size = param.Size,
            //         GetAll = true,
            //         StartDate = param.StartDate,
            //         EndDate = param.EndDate,
            //         Role = param.Role,
            //         IdUser = param.IdUser,
            //         IdAcadyear = param.IdAcadyear,
            //         IdLevel = param.IdLevel,
            //         IdGrade = param.IdGrade,
            //         ExcludeOptionMetadata = true
            //     });
            // var eventAsSchedules = (events.items as IReadOnlyList<GetCalendarEvent2Result>)
            //     .SelectMany(x => x.Dates.Select(y => new GetCalendarScheduleResult
            //     {
            //         Id = x.Id,
            //         Name = x.Name,
            //         Start = y.Start,
            //         End = y.End,
            //         EventType = x.EventType
            //     }))
            //     .ToArray();
            // var results = eventAsSchedules.ToList();

            var results = new List<GetCalendarScheduleResult>();

            //Add Read setting Schedule Publish Date
            //var configSettingSchedulePublishDate = _dbContext.Entity<MsSettingSchedulePublishDate>()
            //    .Where(x => x.IdAcademicYear == param.IdAcadyear && x.IdSchool == param.IdSchool.First())
            //    .FirstOrDefault();

            //if (configSettingSchedulePublishDate != null)
            //{
            //    if (DateTime.Now.Date < configSettingSchedulePublishDate.PublishDate.Date)
            //        return (results.OrderBy(x => x.Start).SetPagination(param), param.CreatePaginationProperty(results.Count));
            //}

            var listSchedule = await GetGeneratedSchedules(param);
            var listCancelSchedule = await GetCancelSchedules(param);
            var listClassDiary = await GetClassDiaries(param);
            var ListPersonalInvitation = await GetPersonalInvitation(param);
            var ListInvitationBooking = await GetInvitationBooking(param);

            var listClassDiaryAttachment = listClassDiary.Count != 0
                ? await (from ClassDiaryAttachment in _dbContext.Entity<TrClassDiaryAttachment>()
                         where listClassDiary.Select(e => e.Id).Contains(ClassDiaryAttachment.IdClassDiary)
                         select ClassDiaryAttachment).ToListAsync(CancellationToken)
                : _emptyClassDiaryAttachment.Value;

            var listSchoolEvent = await GetSchoolEvents(param);
            var query = listSchedule.Union(listCancelSchedule).Union(listClassDiary).Union(listSchoolEvent).Union(ListInvitationBooking).Union(ListPersonalInvitation);

            //query = param.Role switch
            //{
            //    RoleConstant.Teacher => query.Where(x => x.IdUser == param.IdUser),
            //    RoleConstant.Student => query.Where(x => x.IdStudent == param.IdUser),
            //    _ => query
            //};

            //query = query.Where(x => param.IdSchool.Contains(x.IdSchool));

            //if (param.Ids?.Any() ?? false)
            //    query = query.Where(x => param.Ids.Contains(x.Id));
            //if (!string.IsNullOrWhiteSpace(param.Search))
            //    query = query.Where(x => EF.Functions.Like(x.SubjectName, param.SearchPattern()));
            //if (!string.IsNullOrEmpty(param.IdHomeroom))
            //    query = query.Where(x => x.IdHomeroom == param.IdHomeroom);
            //if (!string.IsNullOrEmpty(param.IdSubject))
            //    query = query.Where(x => x.IdSubject == param.IdSubject);

            var schedules = query
                //.OrderBy(x => x.ScheduleDate)
                //.Where(e => e.EventType.Description == "Class Diary")
                .Select(x => new GetCalendarScheduleResult
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

            //==========================================
            //var predicateSchedule = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
            //    => x.ScheduleDate == param.StartDate || x.ScheduleDate == param.EndDate
            //    || (x.ScheduleDate < param.StartDate
            //        ? (x.ScheduleDate.Date > param.StartDate && x.ScheduleDate < param.EndDate) || x.ScheduleDate > param.EndDate
            //        : (param.EndDate > x.ScheduleDate && param.EndDate < x.ScheduleDate) || param.EndDate > x.ScheduleDate));
            //predicateSchedule = param.Role switch
            //{
            //    RoleConstant.Teacher => predicateSchedule.And(x => x.IdUser == param.IdUser),
            //    RoleConstant.Student => predicateSchedule.And(x => x.GeneratedScheduleStudent.IdStudent == param.IdUser),
            //    _ => predicateSchedule
            //};

            //predicateSchedule = predicateSchedule.And(x => param.IdSchool.Contains(x.Homeroom.Grade.Level.AcademicYear.IdSchool));

            //if (param.Ids?.Any() ?? false)
            //    predicateSchedule = predicateSchedule.And(x => param.Ids.Contains(x.Id));
            //if (!string.IsNullOrWhiteSpace(param.Search))
            //    predicateSchedule = predicateSchedule.And(x => EF.Functions.Like(x.SubjectName, param.SearchPattern()));
            //if (!string.IsNullOrEmpty(param.IdHomeroom))
            //    predicateSchedule = predicateSchedule.And(x => x.IdHomeroom == param.IdHomeroom);
            //if (!string.IsNullOrEmpty(param.IdSubject))
            //    predicateSchedule = predicateSchedule.And(x => x.IdSubject == param.IdSubject);

            //var schedules = await _dbContext.Entity<TrGeneratedScheduleLesson>()
            //    .Where(predicateSchedule)
            //    .OrderBy(x => x.ScheduleDate)
            //    .Select(x => new GetCalendarScheduleResult
            //    {
            //        Id = x.Id,
            //        Name = x.SubjectName,
            //        Start = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.StartTime.Hours, x.StartTime.Minutes, 0),
            //        End = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.EndTime.Hours, x.EndTime.Minutes, 0),
            //        EventType = new CalendarEventTypeVm
            //        {
            //            Id = Guid.Empty.ToString(),
            //            Code = "Schedule",
            //            Description = "Class Schedule"
            //        },
            //        Teacher = new NameValueVm(x.IdUser, x.TeacherName),
            //        Venue = new ItemValueVm(x.IdVenue, x.VenueName),
            //        Homeroom = new ItemValueVm(x.IdHomeroom, x.HomeroomName),
            //        Department = new ItemValueVm(x.IdSubject) // temporary store IdSubject to get department later
            //    })
            //    .ToListAsync(CancellationToken);

            if (schedules.Count != 0)
            {
                // remove redundant schedule
                var dupComparer = new InlineComparer<GetCalendarScheduleResult>(
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

        private async Task<IReadOnlyList<CalendarData>> GetGeneratedSchedules(GetCalendarScheduleRequest param)
        {
            // only role TEACHER & STUDENT that have TrGeneratedScheduleLesson
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

            var predicateSchedule = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.IsGenerated
                && x.ScheduleDate.Date >= param.StartDate.Date
                && x.ScheduleDate.Date <= param.EndDate.Date);

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                predicateSchedule = predicateSchedule.And(x => x.IdUser == param.IdUser || x.IdBinusianOld == param.IdUser);
            else if (RoleConstant.Student == param.Role)
                predicateSchedule = predicateSchedule.And(x => x.GeneratedScheduleStudent.IdStudent == param.IdUser);

            // chained filter, acadyear to homeroom
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicateSchedule = predicateSchedule.And(x => x.IdHomeroom == param.IdHomeroom);
            else if (!string.IsNullOrEmpty(param.IdGrade))
                predicateSchedule = predicateSchedule.And(x => x.Homeroom.IdGrade == param.IdGrade);
            else if (!string.IsNullOrEmpty(param.IdLevel))
                predicateSchedule = predicateSchedule.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            else if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicateSchedule = predicateSchedule.And(x => x.Homeroom.Grade.Level.IdAcademicYear == param.IdAcadyear);

            if (!string.IsNullOrEmpty(param.IdSubject))
                predicateSchedule = predicateSchedule.And(x => x.IdSubject == param.IdSubject);

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                // .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                // .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear).ThenInclude(x => x.School)
                .Where(predicateSchedule)
                .OrderBy(x => x.ScheduleDate);
            var results = await query
                .Select(x => new CalendarData
                {
                    IdUser = x.IdUser,
                    IdStudent = x.GeneratedScheduleStudent.IdStudent,
                    IdSchool = x.Homeroom.Grade.Level.AcademicYear.IdSchool,
                    Id = x.Id,
                    SubjectName = x.SubjectName,
                    Start = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.StartTime.Hours, x.StartTime.Minutes, 0),
                    End = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.EndTime.Hours, x.EndTime.Minutes, 0),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = Guid.Empty.ToString(),
                        Code = "Schedule",
                        Description = "Class Schedule"
                    },
                    IdHomeroom = x.IdHomeroom,
                    IdSubject = x.IdSubject,
                    Teacher = new NameValueVm(x.IdUser, x.TeacherName),
                    Venue = new ItemValueVm(x.IdVenue, x.VenueName),
                    Homeroom = new ItemValueVm(x.IdHomeroom, x.HomeroomName),
                    Department = new ItemValueVm(x.IdSubject), // temporary store IdSubject to get department later
                    ScheduleDate = x.ScheduleDate,
                    CreatedBy = "",
                    ClassId = x.ClassID,
                    Description = "",
                    IsChange = x.IsSetScheduleRealization == true && x.IsCancelScheduleRealization == false ? true : false,
                    IsCancel = x.IsCancelScheduleRealization
                })
                .ToListAsync(CancellationToken);

            return results;
        }

        private async Task<IReadOnlyList<CalendarData>> GetCancelSchedules(GetCalendarScheduleRequest param)
        {
            // only role TEACHER & STUDENT that have TrGeneratedScheduleLesson
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

            var predicateSchedule = PredicateBuilder.Create<TrGeneratedScheduleLesson>(x
                => x.IsCancelScheduleRealization
                && x.ScheduleDate.Date >= param.StartDate.Date
                && x.ScheduleDate.Date <= param.EndDate.Date);

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                predicateSchedule = predicateSchedule.And(x => x.IdUser == param.IdUser || x.IdBinusianOld == param.IdUser);
            else if (RoleConstant.Student == param.Role)
                predicateSchedule = predicateSchedule.And(x => x.GeneratedScheduleStudent.IdStudent == param.IdUser);

            // chained filter, acadyear to homeroom
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicateSchedule = predicateSchedule.And(x => x.IdHomeroom == param.IdHomeroom);
            else if (!string.IsNullOrEmpty(param.IdGrade))
                predicateSchedule = predicateSchedule.And(x => x.Homeroom.IdGrade == param.IdGrade);
            else if (!string.IsNullOrEmpty(param.IdLevel))
                predicateSchedule = predicateSchedule.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            else if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicateSchedule = predicateSchedule.And(x => x.Homeroom.Grade.Level.IdAcademicYear == param.IdAcadyear);

            if (!string.IsNullOrEmpty(param.IdSubject))
                predicateSchedule = predicateSchedule.And(x => x.IdSubject == param.IdSubject);

            var query = _dbContext.Entity<TrGeneratedScheduleLesson>()
                // .Include(x => x.GeneratedScheduleStudent).ThenInclude(x => x.Student)
                // .Include(x => x.Homeroom).ThenInclude(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear).ThenInclude(x => x.School)
                .Where(predicateSchedule)
                .OrderBy(x => x.ScheduleDate);
            var results = await query
                .Select(x => new CalendarData
                {
                    IdUser = x.IdUser,
                    IdStudent = x.GeneratedScheduleStudent.IdStudent,
                    IdSchool = x.Homeroom.Grade.Level.AcademicYear.IdSchool,
                    Id = x.Id,
                    SubjectName = x.SubjectName,
                    Start = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.StartTime.Hours, x.StartTime.Minutes, 0),
                    End = new DateTime(x.ScheduleDate.Year, x.ScheduleDate.Month, x.ScheduleDate.Day, x.EndTime.Hours, x.EndTime.Minutes, 0),
                    EventType = new CalendarEventTypeVm
                    {
                        Id = Guid.Empty.ToString(),
                        Code = "Schedule",
                        Description = "Class Schedule"
                    },
                    IdHomeroom = x.IdHomeroom,
                    IdSubject = x.IdSubject,
                    Teacher = new NameValueVm(x.IdUser, x.TeacherName),
                    Venue = new ItemValueVm(x.IdVenue, x.VenueName),
                    Homeroom = new ItemValueVm(x.IdHomeroom, x.HomeroomName),
                    Department = new ItemValueVm(x.IdSubject), // temporary store IdSubject to get department later
                    ScheduleDate = x.ScheduleDate,
                    CreatedBy = "",
                    ClassId = x.ClassID,
                    Description = "",
                    IsChange = x.IsSetScheduleRealization == true && x.IsCancelScheduleRealization == false ? true : false,
                    IsCancel = x.IsCancelScheduleRealization
                })
                .ToListAsync(CancellationToken);

            return results;
        }

        private async Task<IReadOnlyList<CalendarData>> GetClassDiaries(GetCalendarScheduleRequest param)
        {
            // only role TEACHER & STUDENT that have TrClassDiary
            if (param.Role != RoleConstant.Teacher && param.Role != RoleConstant.Student)
                return _emptyCalendarData.Value;

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
                where ClassDiary.ClassDiaryDate.Date >= param.StartDate.Date && ClassDiary.ClassDiaryDate.Date <= param.EndDate.Date
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
                };

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                query = query.Where(x => x.IdUser == param.IdUser);
            else if (RoleConstant.Student == param.Role)
                query = query.Where(x => x.IdStudent == param.IdUser);
            else
                query = query.Where(x => x.IdUser == param.IdUser);

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

        private async Task<IReadOnlyList<CalendarData>> GetSchoolEvents(GetCalendarScheduleRequest param)
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
                param.IdSchool.Contains(School.Id) &&
                Event.StatusEvent == "Approved" &&
                (Event.IsShowOnSchedule == true) &&
                //(!string.IsNullOrEmpty(param.IdAcadyear) ? Event.IdAcademicYear == param.IdAcadyear : true) &&
                //(!string.IsNullOrEmpty(param.IdLevel) ? subGrade.Id == param.IdGrade || subEventIntendedForLevelStudent.IdLevel == param.IdLevel : true) &&
                (param.StartDate.Date == param.EndDate.Date
                    ? param.StartDate.Date >= EventDetail.StartDate.Date && param.StartDate.Date <= EventDetail.EndDate.Date
                    : EventDetail.StartDate.Date >= param.StartDate.Date && EventDetail.EndDate.Date <= param.EndDate.Date)
                //(EventDetail.StartDate.Date == param.EndDate.Date ||
                // (EventDetail.StartDate.Date < param.StartDate.Date
                //  ? (EventDetail.StartDate.Date > param.StartDate.Date && EventDetail.StartDate.Date < param.EndDate.Date)
                //  || EventDetail.StartDate.Date > param.EndDate.Date
                //  : (param.EndDate.Date > EventDetail.StartDate.Date && param.EndDate.Date < EventDetail.StartDate) || param.EndDate.Date > EventDetail.StartDate.Date))
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
                    IdSubgrade = subGrade.Id,
                    IdsubEventIntendedForLevelStudent = subEventIntendedForLevelStudent.IdLevel,
                    IntendedFor = subEventIntendedFor.IntendedFor
                };

            // filter by role
            if (RoleConstant.Teacher == param.Role)
                query = query.Where(x => x.IdUser == param.IdUser);
            else if (RoleConstant.Student == param.Role)
                query = query.Where(x => x.IdUser == param.IdUser);
            else
                query = query.Where(x => x.IdUser == param.IdUser);

            // chained filter
            if (!string.IsNullOrEmpty(param.IdLevel))
                query = query.Where(x => x.IdSubgrade == param.IdGrade || x.IdsubEventIntendedForLevelStudent == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdAcadyear))
                query = query.Where(x => x.IdAcademicYear == param.IdAcadyear || x.IntendedFor == "All");

            if (!string.IsNullOrEmpty(param.IdSubject))
                query = query.Where(x => x.IdSubject == param.IdSubject);

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

        private async Task<IReadOnlyList<CalendarData>> GetInvitationBooking(GetCalendarScheduleRequest param)
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
                    StudentBooking = e.StudentBooking
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            return results;
        }

        private async Task<IReadOnlyList<CalendarData>> GetPersonalInvitation(GetCalendarScheduleRequest param)
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
                select new //CalendarData
                {
                    IdUser = PersonalInvitation.IdUserTeacher,
                    IdStudent = PersonalInvitation.IdStudent,
                    IdSchool = AcademicYear.IdSchool,
                    Id = PersonalInvitation.Id,
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
            public List<StudentBooking> StudentBooking { get; set; }
            public bool IsChange { get; set; }
            public bool IsCancel { get; set; }
        }
    }
}
