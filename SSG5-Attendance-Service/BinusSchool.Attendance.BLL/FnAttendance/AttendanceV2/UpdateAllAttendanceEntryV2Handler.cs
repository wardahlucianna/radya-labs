using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
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
using Google.Apis.Util;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateAllAttendanceEntryV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetimeNow;
        public UpdateAllAttendanceEntryV2Handler(IAttendanceDbContext dbContext, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateAllAttendanceEntryV2Request>();

            var scheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                        .Include(e=>e.Lesson)
                        .Where(e => e.ScheduleDate.Date == body.Date.Date
                                && e.ClassID == body.ClassId
                                && e.IdSession == body.IdSession
                                && e.Id == body.IdScheduleLesson
                                )
                        .Select(e => new
                        {
                            e.Id,
                            e.ScheduleDate,
                            e.ClassID,
                            e.IdAcademicYear,
                            e.IdLesson,
                            e.IdDay,
                            e.IdWeek,
                            e.Lesson.Semester
                        })
                        .FirstOrDefaultAsync(CancellationToken);

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                               .Include(e => e.Student)
                               .Where(e => e.IdAcademicYear == scheduleLesson.IdAcademicYear
                                       && e.ActiveStatus
                                       && e.StartDate.Date <= scheduleLesson.ScheduleDate.Date
                                       && (e.EndDate == null || e.EndDate >= scheduleLesson.ScheduleDate.Date)
                                       )
                               .Select(e => new
                               {
                                   e.IdStudent,
                                   e.StartDate,
                                   EndDate = e.EndDate == null
                                              ? body.Date.Date
                                              : Convert.ToDateTime(e.EndDate),
                                   e.Student.IdBinusian
                               })
                               .ToListAsync(CancellationToken);

            var listIdStudent = listStudentStatus.Select(r => r.IdStudent).ToList();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                     .Where(e => e.Grade.Level.IdAcademicYear == scheduleLesson.IdAcademicYear)
                     .ToListAsync(CancellationToken);

            var listStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                             .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                             .Where(e => (e.IdLesson == scheduleLesson.IdLesson) && e.HomeroomStudent.Homeroom.IdAcademicYear== scheduleLesson.IdAcademicYear)
                             .GroupBy(e => new
                             {
                                 e.IdHomeroomStudent,
                                 e.HomeroomStudent.Homeroom.Grade.IdLevel,
                                 e.IdLesson,
                                 IdHomeroomStudentEnrollment = e.Id,
                                 e.HomeroomStudent.IdStudent,
                                 e.HomeroomStudent.Homeroom.Semester,
                                 idGrade = e.HomeroomStudent.Homeroom.Grade.Id
                             })
                             .Select(e => new GetHomeroom
                             {
                                 IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                 Level = new CodeWithIdVm
                                 {
                                     Id = e.Key.IdLevel
                                 },
                                 Grade = new CodeWithIdVm
                                 {
                                     Id = e.Key.idGrade
                                 },
                                 IdLesson = e.Key.IdLesson,
                                 IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                 IsFromMaster = true,
                                 IdStudent = e.Key.IdStudent,
                                 IsDelete=false,
                                 Semester = e.Key.Semester,
                                 IsShowHistory = false
                             })
                             .ToListAsync(CancellationToken);

            listStudentEnrollment.ForEach(e => 
            {
                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                            .Include(e => e.LessonNew)
                            .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                            .Where(x => x.StartDate.Date <= scheduleLesson.ScheduleDate.Date && x.LessonNew.IdAcademicYear == scheduleLesson.IdAcademicYear)
                            .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                             .Select(e => new GetHomeroom
                             {
                                 IdHomeroomStudent = e.IdHomeroomStudent,
                                 Level = new CodeWithIdVm
                                 {
                                     Id = e.HomeroomStudent.Homeroom.Grade.IdLevel
                                 },
                                 IdLesson = e.IdLessonNew,
                                 IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                 IsFromMaster = false,
                                 EffectiveDate = e.StartDate,
                                 IdStudent = e.HomeroomStudent.IdStudent,
                                 IsDelete = e.IsDelete,
                                 Semester = e.HomeroomStudent.Homeroom.Semester,
                                 IsShowHistory = e.IsShowHistory,
                                 Datein = e.DateIn.Value
                             })
                            .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                 .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                 .ToList();

            //moving
            var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, scheduleLesson.ScheduleDate, scheduleLesson.Semester.ToString(), scheduleLesson.IdLesson);

            var studentEnrollmentMoving = listStudentEnrollmentMoving
                                          .Where(e => listIdStudent.Contains(e.IdStudent))
                                          .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                          .ToList();

            var idLevel = listStudentEnrollmentMoving.Select(e => e.Level.Id).FirstOrDefault();

            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                .Where(e => e.IdLevel == idLevel)
                                .Select(e => new
                                {
                                    e.AbsentTerms,
                                    e.IsUseWorkhabit,
                                    e.Id
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            var listMappingAttendanceAbsent = await _dbContext.Entity<MsListMappingAttendanceAbsent>()
                               .Include(e => e.MsAttendance)
                              .Where(e => e.MsAttendance.IdAcademicYear == scheduleLesson.IdAcademicYear)
                              .ToListAsync(CancellationToken);

            var listAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                .Include(e => e.Attendance)
                               .Where(e => e.Attendance.IdAcademicYear == scheduleLesson.IdAcademicYear)
                               .ToListAsync(CancellationToken);

            var idTeacher = await _dbContext.Entity<MsSchedule>()
                                .Where(e => e.IdLesson == scheduleLesson.IdLesson
                                        && e.IdDay == scheduleLesson.IdDay
                                        && e.IdSession == body.IdSession
                                        && e.IdWeek == scheduleLesson.IdWeek
                                        )
                                .Select(e => e.IdUser)
                                .FirstOrDefaultAsync(CancellationToken);

            var idAttendanceMappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                //.Include(e=>e.att)
                                .Where(e => e.Attendance.IdAcademicYear== scheduleLesson.IdAcademicYear
                                        && e.Attendance.Code == "PR"
                                        && e.IdMappingAttendance== mappingAttendance.Id)
                                .Select(e => e.Id)
                                .FirstOrDefaultAsync(CancellationToken);

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                               .Where(e => e.IdScheduleLesson == scheduleLesson.Id && e.IsFromAttendanceAdministration == false)
                               .ToListAsync(CancellationToken);

            var listAttendanceEntryAdm = await _dbContext.Entity<TrAttendanceEntryV2>()
                   .Where(e => e.IdScheduleLesson == scheduleLesson.Id && e.IsFromAttendanceAdministration)
                   .Select(e => e.IdHomeroomStudent)
                   .ToListAsync(CancellationToken);

            studentEnrollmentMoving = studentEnrollmentMoving.Where(e => !listAttendanceEntryAdm.Contains(e.IdHomeroomStudent)).ToList();

            listAttendanceEntry.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(listAttendanceEntry);

            var listAttendanceWorkhabit = await _dbContext.Entity<TrAttendanceEntryWorkhabitV2>()
                                .Include(e => e.AttendanceEntry)
                               .Where(e => e.AttendanceEntry.IdScheduleLesson == scheduleLesson.Id)
                               .ToListAsync(CancellationToken);

            listAttendanceWorkhabit.ForEach(e => e.IsActive = false);
            _dbContext.Entity<TrAttendanceEntryWorkhabitV2>().UpdateRange(listAttendanceWorkhabit);

            List<TrAttendanceEntryV2> newAttendanceEntry = new List<TrAttendanceEntryV2>();
            List<TrAttendanceEntryWorkhabitV2> newAttendanceEntryWorkhabit = new List<TrAttendanceEntryWorkhabitV2>();

            foreach (var studentEnroolment in studentEnrollmentMoving)
            {
                var IsNeedValidation = UpdateAttendanceEntryV2Handler.GetNeedValidation(listAttendanceMappingAttendance, listMappingAttendanceAbsent, idAttendanceMappingAttendance);

                var IdAttendanceEntry = Guid.NewGuid().ToString();
                newAttendanceEntry.Add(new TrAttendanceEntryV2
                {
                    IdAttendanceEntry = IdAttendanceEntry,
                    IdScheduleLesson = scheduleLesson.Id,
                    IdAttendanceMappingAttendance = idAttendanceMappingAttendance,
                    Status = IsNeedValidation
                                    ? AttendanceEntryStatus.Pending
                                    : AttendanceEntryStatus.Submitted,
                    IsFromAttendanceAdministration = false,
                    PositionIn = PositionConstant.SubjectTeacher,
                    IdBinusian = idTeacher,
                    IdHomeroomStudent = studentEnroolment.IdHomeroomStudent,
                });
            }
            _dbContext.Entity<TrAttendanceEntryV2>().AddRange(newAttendanceEntry);
            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
