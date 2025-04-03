using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.DailyAttendanceRecap
{
    public class GetDailyAttendanceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAuth _apiAuth;
        private readonly IAttendanceLog _apiAttendanceLog;
        private readonly IMachineDateTime _datetimeNow;

        public GetDailyAttendanceRecapHandler(IAttendanceDbContext dbContext, IAuth apiAuth, IAttendanceLog apiAttendanceLog, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _apiAuth = apiAuth;
            _apiAttendanceLog = apiAttendanceLog;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDailyAttendanceRecapRequest>(nameof(GetDailyAttendanceRecapRequest.IdAcademicYear));

            var result = await GetDailyAttendanceRecap(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<List<GetDailyAttendanceRecapResult>> GetDailyAttendanceRecap(GetDailyAttendanceRecapRequest param)
        {
            var dailyAbsentTerms = await GetDailyAbsentTerm(param);

            var absentTermIds = dailyAbsentTerms.Select(a => a.IdLevel).ToList();

            var scheduleLessons = await GetScheduleLessonHandler(param, absentTermIds);
            var studentEnrollments = await GetHomeroomStudentEnrollmentHandler(param, absentTermIds);
            var studentStatuses = await GetStudentStatusHandler(param);
            var homeroomTeachers = await GetHomeroomTeacherHandler(param, absentTermIds);
            var attendanceEntries = await GetAttendanceEntryHandler(param, absentTermIds);

            var attendanceLessonIds = new HashSet<string>(attendanceEntries.Select(a => a.IdScheduleLesson));

            var filteredScheduleLessons = scheduleLessons
                .Where(sl => !attendanceLessonIds.Contains(sl.IdScheduleLesson))
                .Select(sl => new
                {
                    sl.IdScheduleLesson
                })
                .ToList();

            var groupScheduleLessons = scheduleLessons
                .Where(sl => filteredScheduleLessons.Any(fsl => fsl.IdScheduleLesson == sl.IdScheduleLesson))
                .Select(sl => new
                {
                    sl.IdLesson,
                    sl.IdScheduleLesson,
                    sl.ScheduleDate
                })
                .Join(
                    studentEnrollments,
                    sl => sl.IdLesson,
                    se => se.IdLesson,
                    (sl, se) => new {
                        sl.IdLesson,
                        sl.IdScheduleLesson,
                        sl.ScheduleDate,
                        se.IdStudent,
                        se.Class,
                        se.IdHomeroomStudent
                    }
                )
                .Join(
                    studentStatuses,
                    temp => temp.IdStudent,
                    ss => ss.IdStudent,
                    (temp, ss) => new GroupScheduleLessonResult
                    {
                        IdStudent = ss.IdStudent,
                        IdLesson = temp.IdLesson,
                        IdScheduleLesson = temp.IdScheduleLesson,
                        ScheduleDate = temp.ScheduleDate,
                        Class = temp.Class,
                        IdHomeroomStudent = temp.IdHomeroomStudent,
                        StartDate = ss.StartDate,
                        EndDate = ss.EndDate
                    })
                .ToList();

            var unsubmittedAttendance = groupScheduleLessons
                .Join(
                    homeroomTeachers,
                    gsl => gsl.IdLesson,
                    ht => ht.IdLesson,
                    (gsl, ht) => new {
                        gsl.IdScheduleLesson,
                        gsl.ScheduleDate,
                        gsl.Class,
                        gsl.IdHomeroomStudent,
                        gsl.StartDate,
                        gsl.EndDate,
                        ht.IdBinusian,
                        ht.HomeroomTeacher
                    }
                 )
                .GroupJoin(
                    attendanceEntries,
                    temp => new { temp.IdScheduleLesson, temp.IdHomeroomStudent },
                    ae => new { ae.IdScheduleLesson, ae.IdHomeroomStudent },
                    (temp, ae) => new {
                        temp.IdScheduleLesson,
                        temp.ScheduleDate,
                        temp.Class,
                        temp.StartDate,
                        temp.EndDate,
                        temp.IdBinusian,
                        temp.HomeroomTeacher,
                        ae
                    }
                 )
                .SelectMany(
                    joined => joined.ae.DefaultIfEmpty(),
                    (joined, ae) => new {
                        joined.ScheduleDate,
                        joined.StartDate,
                        joined.EndDate,
                        joined.Class,
                        joined.IdBinusian,
                        joined.HomeroomTeacher,
                        joined.IdScheduleLesson,
                        UnsubmittedDate = joined.ScheduleDate
                    }
                ).Distinct()
                .Where(result => result.ScheduleDate >= result.StartDate &&
                    result.ScheduleDate <= result.EndDate)
                .GroupBy(result => new { result.Class, result.IdBinusian, result.HomeroomTeacher })
                .Select(group => new GetDailyAttendanceRecapResult
                {
                    Class = group.Key.Class,
                    IdBinusian = group.Key.IdBinusian,
                    HomeroomTeacher = group.Key.HomeroomTeacher,
                    UnsubmittedDate = group.OrderBy(x => x.UnsubmittedDate).Select(x => x.UnsubmittedDate.ToString("dd/MM/yyyy")).Distinct().ToList(),
                    TotalUnsubmitted = group.Select(x => x.UnsubmittedDate).Distinct().Count()
                })
                .OrderBy(result => result.Class)
                .ToList();

            return unsubmittedAttendance;
        }

        private async Task<List<DailyAbsentTermResult>> GetDailyAbsentTerm(GetDailyAttendanceRecapRequest param)
        {
            return await _dbContext.Entity<MsMappingAttendance>()
                .Include(a => a.Level)
                .Where(a => a.AbsentTerms == AbsentTerm.Day && a.Level.IdAcademicYear == param.IdAcademicYear &&
                            (string.IsNullOrEmpty(param.IdLevel) || a.IdLevel == param.IdLevel))
                .Select(a => new DailyAbsentTermResult { IdLevel = a.IdLevel })
                .ToListAsync();
        }

        private async Task<List<ScheduleLessonResult>> GetScheduleLessonHandler(GetDailyAttendanceRecapRequest param, List<string> absentTermIds)
        {
            return await _dbContext.Entity<MsScheduleLesson>()
                .Include(a => a.Lesson)
                .Where(a => a.IdAcademicYear == param.IdAcademicYear &&
                            (param.Semester == null || a.Lesson.Semester == param.Semester) &&
                            a.SubjectName == "Homeroom" &&
                            a.ScheduleDate <= DateTime.Now &&
                            absentTermIds.Contains(a.IdLevel) &&
                            (string.IsNullOrEmpty(param.IdGrade) || a.IdGrade == param.IdGrade))
                .Select(a => new ScheduleLessonResult
                {
                    IdScheduleLesson = a.Id,
                    IdLesson = a.Lesson.Id,
                    ScheduleDate = a.ScheduleDate
                })
                .ToListAsync();
        }

        private async Task<List<StudentEnrollmentResult>> GetHomeroomStudentEnrollmentHandler(GetDailyAttendanceRecapRequest param, List<string> absentTermIds)
        {
            return await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(a => a.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom)
                .Include(a => a.HomeroomStudent.Homeroom.Grade.Level)
                .Include(a => a.HomeroomStudent)
                .Include(a => a.Lesson)
                .Where(a => (param.Semester == null || a.HomeroomStudent.Homeroom.Semester == param.Semester) &&
                            (string.IsNullOrEmpty(param.IdHomeroom) || a.HomeroomStudent.Homeroom.Id == param.IdHomeroom) &&
                            a.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear &&
                            absentTermIds.Contains(a.HomeroomStudent.Homeroom.Grade.Level.Id) &&
                            (string.IsNullOrEmpty(param.IdGrade) || a.HomeroomStudent.Homeroom.Grade.Id == param.IdGrade))
                .Select(a => new StudentEnrollmentResult
                {
                    IdLesson = a.Lesson.Id,
                    Class = a.HomeroomStudent.Homeroom.Grade.Code + a.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    IdStudent = a.HomeroomStudent.IdStudent,
                    IdHomeroomStudent = a.IdHomeroomStudent
                })
                .ToListAsync();
        }

        private async Task<List<StudentStatusResult>> GetStudentStatusHandler(GetDailyAttendanceRecapRequest param)
        {
            return await _dbContext.Entity<TrStudentStatus>()
                .Where(a => a.IdStudentStatus == 1 && a.IdAcademicYear == param.IdAcademicYear)
                .Select(a => new StudentStatusResult
                {
                    IdStudent = a.IdStudent,
                    StartDate = a.StartDate,
                    EndDate = a.EndDate ?? DateTime.Now
                })
                .ToListAsync();
        }

        private async Task<List<HomeroomTeacherResult>> GetHomeroomTeacherHandler(GetDailyAttendanceRecapRequest param, List<string> absentTermIds)
        {
            var lessonTeachers = await _dbContext.Entity<MsLessonTeacher>()
                .Include(a => a.Lesson.Subject.Grade)
                .Include(a => a.Staff)
                .Where(a => a.Lesson.IdAcademicYear == param.IdAcademicYear &&
                            (param.Semester == null || a.Lesson.Semester == param.Semester) &&
                            (string.IsNullOrEmpty(param.IdBinusian) || a.Staff.IdBinusian == param.IdBinusian) &&
                            a.Lesson.Subject.Description == "Homeroom" &&
                            absentTermIds.Contains(a.Lesson.Grade.IdLevel) &&
                            (string.IsNullOrEmpty(param.IdGrade) || a.Lesson.Subject.Grade.Id == param.IdGrade))
                .Select(a => new
                {
                    a.IdLesson,
                    a.Staff.IdBinusian
                })
                .ToListAsync();

            var homeroomTeachers = await _dbContext.Entity<MsHomeroomTeacher>()
                .Include(a => a.Homeroom.Grade.Level)
                .Include(a => a.Staff)
                .Where(a => a.TeacherPosition.IdPosition == "5" &&
                            a.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear &&
                            (param.Semester == null || a.Homeroom.Semester == param.Semester) &&
                            (string.IsNullOrEmpty(param.IdHomeroom) || a.IdHomeroom == param.IdHomeroom) &&
                            (string.IsNullOrEmpty(param.IdBinusian) || a.IdBinusian == param.IdBinusian) &&
                            absentTermIds.Contains(a.Homeroom.Grade.Level.Id) &&
                            (string.IsNullOrEmpty(param.IdGrade) || a.Homeroom.Grade.Id == param.IdGrade))
                .Select(a => new
                {
                    a.Staff.IdBinusian,
                    a.Staff.FirstName,
                    a.Staff.LastName,
                    a.IdHomeroom
                })
                .ToListAsync();

            return homeroomTeachers
                .Join(
                    lessonTeachers,
                    ht => ht.IdBinusian,
                    lt => lt.IdBinusian,
                    (ht, lt) => new HomeroomTeacherResult
                    {
                        IdBinusian = lt.IdBinusian,
                        HomeroomTeacher = $"{ht.FirstName} {ht.LastName}",
                        IdHomeroom = ht.IdHomeroom,
                        IdLesson = lt.IdLesson
                    })
                .ToList();
        }

        private async Task<List<AttendanceEntryResult>> GetAttendanceEntryHandler(GetDailyAttendanceRecapRequest param, List<string> absentTermIds)
        {
            return await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(a => a.ScheduleLesson)
                .Where(a => a.ScheduleLesson.IdAcademicYear == param.IdAcademicYear &&
                            (string.IsNullOrEmpty(param.IdBinusian) || a.IdBinusian == param.IdBinusian) &&
                            (param.Semester == null || a.ScheduleLesson.Lesson.Semester == param.Semester) &&
                            (string.IsNullOrEmpty(param.IdHomeroom) || a.HomeroomStudent.IdHomeroom == param.IdHomeroom) &&
                            absentTermIds.Contains(a.ScheduleLesson.IdLevel) &&
                            (string.IsNullOrEmpty(param.IdGrade) || a.ScheduleLesson.IdGrade == param.IdGrade))
                .Select(a => new AttendanceEntryResult
                {
                    IdAttendanceEntry = a.IdAttendanceEntry,
                    IdScheduleLesson = a.IdScheduleLesson,
                    IdHomeroomStudent = a.IdHomeroomStudent
                })
                .ToListAsync();
        }
    }
}
