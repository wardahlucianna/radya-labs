using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.FeatureManagement;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class AllPresentEventAttendanceV2Handler : FunctionsHttpSingleHandler
    {
        protected IDbContextTransaction Transaction;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IFeatureManagerSnapshot _featureManager;
        private readonly IMachineDateTime _datetimeNow;
        public AllPresentEventAttendanceV2Handler(
            IAttendanceDbContext dbContext, IFeatureManagerSnapshot featureManager, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _featureManager = featureManager;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<AllPresentEventAttendanceV2Request>();

            if (await _featureManager.IsEnabledAsync(FeatureFlags.AttendanceEventV2))
            {
                var attendanceStudents = await _dbContext.Entity<TrEventIntendedFor>()
                    .Include(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents).ThenInclude(x => x.UserEventAttendance2s)
                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedForAtdCheckStudents)
                    .Include(x => x.EventIntendedForAttendanceStudents)
                    .Include(x => x.EventIntendedForAttendanceStudents).ThenInclude(x => x.EventIntendedFor).ThenInclude(x => x.Event).ThenInclude(x => x.EventDetails).ThenInclude(x => x.UserEvents).ThenInclude(x => x.UserEventAttendance2s)
                    .Where(x
                        => x.IdEvent == body.IdEvent
                        && x.IntendedFor == RoleConstant.Student
                        && x.EventIntendedForAttendanceStudents.Any(y
                            => y.IsSetAttendance
                            && y.EventIntendedForAtdPICStudents.Any(z => z.IdUser == body.IdUser)
                            )
                        )
                    .SelectMany(x => x.EventIntendedForAttendanceStudents)
                    .ToListAsync(CancellationToken);

                if (attendanceStudents.Count == 0)
                    throw new NotFoundException("Event is not found or you're not PIC of this event");

                var presentAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>().Include(x => x.MappingAttendance).ThenInclude(x => x.Level)
                    .Where(x => x.Attendance.AttendanceCategory == AttendanceCategory.Present && x.Attendance.Code == "PR")
                    .ToListAsync(CancellationToken);

                var idAcademicYear = attendanceStudents.Select(e=>e.EventIntendedFor.Event.IdAcademicYear).FirstOrDefault();

                var listPeriod = await _dbContext.Entity<MsPeriod>()
                    .Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear)
                    .ToListAsync(CancellationToken);
                foreach (var checkStudent in attendanceStudents.SelectMany(x => x.EventIntendedForAtdCheckStudents.Where(z=> z.StartDate.Date == body.Date.Date)))
                {
                    var GetUserEventAttendance2 = await _dbContext.Entity<TrUserEventAttendance2>()
                                              .Where(x => x.IdEventIntendedForAtdCheckStudent== checkStudent.Id)
                                              .ToListAsync(CancellationToken);

                    if (GetUserEventAttendance2.Any())
                    {
                        GetUserEventAttendance2.ForEach(x => x.IsActive = false);
                        _dbContext.Entity<TrUserEventAttendance2>().UpdateRange(GetUserEventAttendance2);
                    }

                    var userEvents = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails
                        .SelectMany(x => x.UserEvents).Where(x => x.UserEventAttendance2s.Any(y => y.IdEventIntendedForAtdCheckStudent == checkStudent.Id)).ToList();

                    if (!userEvents.Any()) 
                    {
                        userEvents = checkStudent.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails
                        .SelectMany(x => x.UserEvents).ToList();

                        var attendanceStudentsPIC = await _dbContext.Entity<TrEventIntendedForAttendanceStudent>()
                            .Include(x => x.EventIntendedForAtdPICStudents)
                            .Where(x => x.EventIntendedFor.IdEvent == userEvents.Select(x => x.EventDetail.IdEvent).FirstOrDefault())
                            .ToListAsync(CancellationToken);

                        var isHomeroomTeacher = attendanceStudentsPIC.Any(x => x.EventIntendedForAtdPICStudents.Any(y => y.IdUser == body.IdUser && (y.Type == EventIntendedForAttendancePICStudent.Homeroom || y.Type == EventIntendedForAttendancePICStudent.UserTeacher)));
                        var homerooms = isHomeroomTeacher ? await _dbContext.Entity<MsHomeroomTeacher>()
                                  .Include(x => x.Homeroom)
                                  .Where(x => x.IdBinusian == body.IdUser && x.Homeroom.IdAcademicYear == idAcademicYear)
                                  .Select(x => new { x.IdHomeroom, x.Homeroom.Semester })
                                  .ToListAsync() : null;

                        var attendanceEventDate = await _dbContext.Entity<TrEventDetail>()
                        .Where(x => x.IdEvent == userEvents.Select(x=> x.EventDetail.IdEvent).FirstOrDefault())
                        .ToListAsync(CancellationToken);

                        var startEventDate = attendanceEventDate.Select(x => x.StartDate).Min();
                        var endEventDate = attendanceEventDate.Select(x => x.EndDate).Max();

                        var semester = await _dbContext.Entity<MsPeriod>()
                                    .Where(x => x.StartDate <= startEventDate && x.EndDate >= endEventDate)
                                    .Select(x => x.Semester)
                                    .FirstOrDefaultAsync();

                        if (semester == null)
                            semester = 1;

                        if (homerooms != null)
                        {
                            if (homerooms.Count > 0)
                            {
                                var idHomeroom = homerooms.Where(x => x.Semester == semester).Select(x => x.IdHomeroom).FirstOrDefault();

                                var idStudens = await _dbContext.Entity<MsStudent>()
                                    .Where(x=> x.HomeroomStudents.Any(y => y.IdHomeroom == idHomeroom))
                                    .Select(x => x.Id)
                                    .ToListAsync(CancellationToken);

                                userEvents = userEvents.Where(x => idStudens.Contains(x.IdUser)).ToList();
                            }
                        }
                    }

                    var idUsers = userEvents.Select(x => x.IdUser);
                    var students = await _dbContext.Entity<MsStudent>()
                        .Where(x => idUsers.Contains(x.Id))
                        .Select(x => new { x.Id, x.StudentGrades.FirstOrDefault().Grade.Level })
                        .ToListAsync(CancellationToken);

                    var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                .Include(e => e.Student)
                                .Where(e => e.IdAcademicYear == idAcademicYear && e.ActiveStatus)
                                .Select(e => new
                                {
                                    e.IdStudent,
                                    e.StartDate,
                                    EndDate = e.EndDate == null
                                               ? listPeriod.Select(e => e.AttendanceEndDate).Max()
                                               : Convert.ToDateTime(e.EndDate),
                                    e.Student.IdBinusian
                                })
                                .ToListAsync(CancellationToken);

                    var listAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                     .Include(e => e.Attendance)
                                    .Where(e => e.Attendance.IdAcademicYear == idAcademicYear)
                                    .ToListAsync(CancellationToken);

                    var listMappingAttendanceAbsent = await _dbContext.Entity<MsListMappingAttendanceAbsent>()
                      .Include(e => e.MsAttendance)
                     .Where(e => e.MsAttendance.IdAcademicYear == idAcademicYear)
                     .ToListAsync(CancellationToken);

                    var listEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                        .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                        .Where(e => idUsers.Contains(e.HomeroomStudent.IdStudent)
                                                       && e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
                                        .GroupBy(e => new
                                        {
                                           e.IdLesson,
                                           e.IdHomeroomStudent,
                                           e.HomeroomStudent.Homeroom.Semester,
                                           IdHomeroomStudentEnrollment = e.Id,
                                           e.HomeroomStudent.IdStudent,
                                           e.HomeroomStudent.Homeroom.IdGrade
                                        })
                                        .Select(e => new GetHomeroom
                                        {
                                           IdLesson = e.Key.IdLesson,
                                           IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                           Semester = e.Key.Semester,
                                           IsFromMaster = true,
                                           IsDelete = false,
                                           IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                           IdStudent = e.Key.IdStudent,
                                           Grade = new CodeWithIdVm
                                           {
                                               Id = e.Key.IdGrade
                                           },
                                           IsShowHistory = false,
                                        })
                                        .ToListAsync(CancellationToken);

                    listEnrollment.ForEach(e =>
                    {
                        e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                        e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                    });

                    var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                         .Include(e => e.SubjectNew)
                         .Include(e => e.LessonNew)
                         .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                         .Where(x => x.StartDate.Date <= checkStudent.StartDate.Date
                                   && x.LessonNew.IdAcademicYear == idAcademicYear
                                   && idUsers.Contains(x.HomeroomStudent.IdStudent))
                         .Select(e => new GetHomeroom
                         {
                             IdLesson = e.IdLessonNew,
                             IdHomeroomStudent = e.IdHomeroomStudent,
                             Semester = e.HomeroomStudent.Homeroom.Semester,
                             IsFromMaster = true,
                             IsDelete = false,
                             IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                             EffectiveDate = e.StartDate,
                             IdStudent = e.HomeroomStudent.IdStudent,
                             Grade = new CodeWithIdVm
                             {
                                 Id = e.HomeroomStudent.Homeroom.IdGrade
                             },
                             IsShowHistory = e.IsShowHistory,
                             Datein = e.DateIn.Value
                         })
                         .ToListAsync(CancellationToken);

                    var listStudentEnrollmentUnionData = listEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                           .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                           .ToList();

                    var listScheduleLessons = await _dbContext.Entity<MsScheduleLesson>()
                         .Include(e => e.Lesson).ThenInclude(e => e.Subject)
                         .Where(x
                             => listStudentEnrollmentUnionData.Select(x=> x.IdLesson).ToList().Contains(x.IdLesson)
                             && x.ScheduleDate.Date == checkStudent.StartDate.Date
                             && (
                                 (checkStudent.StartTime >= x.StartTime || checkStudent.StartTime <= x.EndTime)
                                 && (checkStudent.EndTime >= x.StartTime || checkStudent.EndTime <= x.EndTime)
                                 )
                             )
                         .Select(x => new
                         {
                             x.Id,
                             x.IdLesson,
                             x.IdDay,
                             x.IdSession,
                             x.IdWeek,
                             x.IdLevel,
                             x.StartTime,
                             x.EndTime,
                             x.Lesson.Semester,
                             x.IdAcademicYear,
                             x.ScheduleDate,
                             x.Lesson.Subject.Description
                         })
                         .ToListAsync(CancellationToken);

                    var idLevels = listScheduleLessons.Select(e => e.IdLevel).Distinct().ToList();

                    var mappingAttendances = await _dbContext.Entity<MsMappingAttendance>()
                    .Where(e => idLevels.Contains(e.IdLevel))
                    .Select(e => new
                    {
                        e.IdLevel,
                        e.AbsentTerms,
                        e.IsNeedValidation,
                        e.IsUseWorkhabit
                    })
                    .ToListAsync(CancellationToken);

                    var schedules = await _dbContext.Entity<MsSchedule>()
                                .Where(e => listScheduleLessons.Select(x=> x.IdLesson).ToList().Contains(e.IdLesson)
                                            && listScheduleLessons.Select(x => x.IdSession).ToList().Contains(e.IdSession)
                                            && listScheduleLessons.Select(x => x.IdDay).ToList().Contains(e.IdDay))
                                .Select(e => new
                                {
                                    e.IdUser,
                                    e.IdLesson,
                                    e.IdSession,
                                    e.IdDay,
                                    e.IdWeek
                                })
                                .ToListAsync(CancellationToken);

                    var listAttendanceEntrys = await _dbContext.Entity<TrAttendanceEntryV2>()
                    .Include(e => e.HomeroomStudent)
                    .Where(x => listScheduleLessons.Select(x => x.Id).ToList().Contains(x.IdScheduleLesson) && x.IsFromAttendanceAdministration == false)
                    .ToListAsync(CancellationToken);

                    foreach (var userEvent in userEvents)
                    {
                        var student = students.Where(x => x.Id == userEvent.IdUser);
                        var mapping = presentAttendance.Find(x => x.MappingAttendance.IdLevel == student.FirstOrDefault().Level.Id);

                        if (mapping is null)
                            throw new BadRequestException($"Present attendance on level {student.FirstOrDefault().Level.Description} is not mapped yet");

                        if (student is { })
                        {
                            var hasConflict = checkStudent.IsPrimary && await _dbContext.Entity<TrEventIntendedForAtdCheckStudent>()
                                .Where(x
                                    => x.StartDate.Date == checkStudent.StartDate.Date
                                    && x.EventIntendedForAttendanceStudent.EventIntendedFor.Event.EventDetails.Any(y => y.UserEvents.Any(z => z.IdUser == userEvent.Id)))
                                .AnyAsync(CancellationToken);

                            var userEventAttendance = new TrUserEventAttendance2
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdUserEvent = userEvent.Id,
                                IdEventIntendedForAtdCheckStudent = checkStudent.Id,
                                IdAttendanceMappingAttendance = mapping.Id,
                                DateEvent = checkStudent.StartDate,
                                HasBeenResolved = !hasConflict
                            };
                            _dbContext.Entity<TrUserEventAttendance2>().Add(userEventAttendance);

                            // action for replace attendance entry
                            if (!hasConflict && checkStudent.IsPrimary)
                            {
                                var listStudentEnrollmentUnion = listStudentEnrollmentUnionData
                                .Where(e => e.IdStudent == userEvent.IdUser).ToList();                                

                                var listIdLessonEnrollment = listStudentEnrollmentUnion.Select(e=>e.IdLesson).ToList();
                                var listScheduleLesson = listScheduleLessons
                                    .Where(x
                                        => listIdLessonEnrollment.Contains(x.IdLesson)
                                        && x.ScheduleDate.Date == checkStudent.StartDate.Date
                                        && (
                                            (checkStudent.StartTime >= x.StartTime || checkStudent.StartTime <= x.EndTime)
                                            && (checkStudent.EndTime >= x.StartTime || checkStudent.EndTime <= x.EndTime)
                                            )
                                        )
                                    .ToList();

                                var listIdLesson = listScheduleLesson.Select(e => e.IdLesson).ToList();
                                var listIdSession = listScheduleLesson.Select(e => e.IdSession).ToList();
                                var listIdDay = listScheduleLesson.Select(e => e.IdDay).ToList();
                                var listIdScheduleLesson = listScheduleLesson.Select(e => e.Id).ToList();
                                var idLevel = listScheduleLesson.Select(e => e.IdLevel).FirstOrDefault();

                                var mappingAttendance = mappingAttendances
                                .Where(e => e.IdLevel == idLevel)
                                .Select(e => new
                                {
                                    e.AbsentTerms,
                                    e.IsNeedValidation,
                                    e.IsUseWorkhabit
                                })
                                .FirstOrDefault();

                                var schedule = schedules
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                            && listIdSession.Contains(e.IdSession)
                                            && listIdDay.Contains(e.IdDay))
                                .ToList();

                                var listAttendanceEntry = listAttendanceEntrys
                                        .Where(x => listIdScheduleLesson.Contains(x.IdScheduleLesson) && x.IsFromAttendanceAdministration == false)
                                        .ToList();

                                foreach (var scheduleLesson in listScheduleLesson)
                                {
                                    //overwrite data in attendance entry if exist
                                    var lessonExistInEntry = listAttendanceEntry.Where(e => e.IdScheduleLesson == scheduleLesson.Id && e.HomeroomStudent.IdStudent==userEvent.IdUser).ToList();

                                    var IdTecher = schedule
                                                    .Where(e => e.IdLesson == scheduleLesson.IdLesson
                                                       && e.IdDay == scheduleLesson.IdDay
                                                       && e.IdSession == scheduleLesson.IdSession
                                                       && e.IdWeek == scheduleLesson.IdWeek
                                                        )
                                                .Select(e => e.IdUser)
                                                .FirstOrDefault();

                                    foreach (var lesson in lessonExistInEntry)
                                    {
                                        lesson.IsActive = false;
                                    }
                                    _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(lessonExistInEntry);

                                    //moving
                                    var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, scheduleLesson.ScheduleDate, scheduleLesson.Semester.ToString(), scheduleLesson.IdLesson);

                                    var listIdStudent = listStudentStatus
                                            .Where(e => e.StartDate.Date <= scheduleLesson.ScheduleDate.Date
                                                        && e.EndDate.Date >= scheduleLesson.ScheduleDate.Date && e.IdStudent == userEvent.IdUser)
                                            .Select(e => e.IdStudent).ToList();

                                    var studentEnrollmentMoving = listStudentEnrollmentMoving
                                                                    .Where(e => listIdStudent.Contains(e.IdStudent))
                                                                    .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                                    .ToList();

                                    var listIdHomeroomStudent = studentEnrollmentMoving
                                                                .Where(e => e.IdLesson == scheduleLesson.IdLesson)
                                                                .Select(e => e.IdHomeroomStudent)
                                                                .Distinct().ToList();

                                    foreach (var idHomeroomStudent in listIdHomeroomStudent)
                                    {
                                        var IsNeedValidation = UpdateAttendanceEntryV2Handler.GetNeedValidation(listAttendanceMappingAttendance, listMappingAttendanceAbsent, mapping.Id);

                                        var attendanceEntry = new TrAttendanceEntryV2
                                        {
                                            IdAttendanceEntry = Guid.NewGuid().ToString(),
                                            IdScheduleLesson = scheduleLesson.Id,
                                            IdAttendanceMappingAttendance = mapping.Id,
                                            IdHomeroomStudent = idHomeroomStudent,
                                            IdBinusian = IdTecher,
                                            Status = IsNeedValidation
                                                ? AttendanceEntryStatus.Pending
                                                : AttendanceEntryStatus.Submitted,
                                            IsFromAttendanceAdministration = false,
                                            PositionIn = "PIC"
                                        };

                                        _dbContext.Entity<TrAttendanceEntryV2>().Add(attendanceEntry);
                                    }
                                }
                            }
                        }
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                return Request.CreateApiResult2();
            }

            return Request.CreateApiResult2();
        }
    }
}
