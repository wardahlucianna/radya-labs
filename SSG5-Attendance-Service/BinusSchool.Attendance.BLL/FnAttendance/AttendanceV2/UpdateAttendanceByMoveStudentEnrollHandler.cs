using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using System.Linq;
using Microsoft.Extensions.Azure;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Data.Api.School.FnPeriod;
using BinusSchool.Persistence.AttendanceDb.Models;
using Org.BouncyCastle.Asn1.Esf;
using FluentEmail.Core;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class UpdateAttendanceByMoveStudentEnrollHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetime;
        public UpdateAttendanceByMoveStudentEnrollHandler(IAttendanceDbContext dbContext, IMachineDateTime datetime)
        {
            _dbContext = dbContext;
            _datetime = datetime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.GetBody<UpdateAttendanceByMoveStudentEnrollRequest>();
            var startDate = body.StartDate;
            var endDate = _datetime.ServerTime;
            var listIdHomeroomStudent = body.IdHomeroomStudent;

            var listHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                            .Include(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                                            .Where(e => listIdHomeroomStudent.Contains(e.Id))
                                            .Select(e => new
                                            {
                                                e.IdStudent,
                                                e.Homeroom.IdGrade,
                                                e.Homeroom.Grade.IdLevel
                                            })
                                            .Distinct()
                                            .FirstOrDefaultAsync(CancellationToken);

            var listStudentGrade = await _dbContext.Entity<MsStudentGrade>()
                                           .Where(e => e.Grade.Level.IdAcademicYear==body.IdAcademicYear
                                                    && e.IdGrade== listHomeroomStudent.IdGrade)
                                           .Distinct()
                                           .ToListAsync(CancellationToken);

            var idStudentGrade = listStudentGrade.Where(e => e.IdStudent== listHomeroomStudent.IdStudent).Select(e=>e.Id).FirstOrDefault();

            var listAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                                                .Include(e=>e.StudentGrade).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                                .Include(e=>e.AttdAdministrationCancel).ThenInclude(e=>e.ScheduleLesson)
                                                .Where(e => idStudentGrade.Contains(e.IdStudentGrade) 
                                                            && ((e.StartDate >= startDate && e.StartDate<=endDate) 
                                                                    || (e.EndDate >= startDate && e.EndDate<= endDate)
                                                                    ))
                                                .Distinct()
                                                .OrderBy(e => e.DateIn)
                                                .ToListAsync(CancellationToken);

            var idStudents = listAttendanceAdministration.Select(x => x.StudentGrade.IdStudent).Distinct().ToList();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                    .Where(e => e.Grade.Level.IdAcademicYear == body.IdAcademicYear)
                    .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                    .Include(e => e.Lesson)
                                                    .Where(e => idStudents.Contains(e.HomeroomStudent.IdStudent)
                                                                && e.HomeroomStudent.Homeroom.IdAcademicYear == body.IdAcademicYear)
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
                                                         e.HomeroomStudent.Homeroom.Semester,
                                                         e.Lesson.IdSubject,
                                                         e.HomeroomStudent.Student.FirstName,
                                                         e.HomeroomStudent.Student.MiddleName,
                                                         e.HomeroomStudent.Student.LastName,
                                                         e.HomeroomStudent.Student.IdBinusian
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
                                                          FirstName = e.Key.FirstName,
                                                          MiddleName = e.Key.MiddleName,
                                                          LastName = e.Key.LastName,
                                                          BinusianID = e.Key.IdBinusian,
                                                          IsShowHistory = false
                                                      })
                                                    .ToListAsync(CancellationToken);

            listHomeroomStudentEnrollment.ForEach(e =>
            {
                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                                    .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                .Include(e => e.LessonNew)
                                                .Where(x => idStudents.Contains(x.HomeroomStudent.IdStudent)
                                                                && x.LessonOld.IdAcademicYear == body.IdAcademicYear)
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
                                                    Semester = e.HomeroomStudent.Homeroom.Semester,
                                                    EffectiveDate = e.StartDate,
                                                    IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                                                    IsFromMaster = false,
                                                    IdStudent = e.HomeroomStudent.IdStudent,
                                                    IsDelete = e.IsDelete,
                                                    IdSubject = e.IdSubjectNew,
                                                    FirstName = e.HomeroomStudent.Student.FirstName,
                                                    MiddleName = e.HomeroomStudent.Student.MiddleName,
                                                    LastName = e.HomeroomStudent.Student.LastName,
                                                    Datein = e.DateIn.Value,
                                                    BinusianID = e.HomeroomStudent.Student.IdBinusian,
                                                    IsShowHistory = e.IsShowHistory,
                                                })
                                                .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                  .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                  .ToList();

            #region Add Attendance have id lesson
            if (listAttendanceAdministration.Any())
            {
                foreach (var item in listAttendanceAdministration)
                {
                    var listIdLessonByStudent = listStudentEnrollmentUnion.Where(e => e.IdStudent == item.StudentGrade.IdStudent).Select(e=>e.IdLesson).Distinct().ToList();

                    #region Find all lesson by student , date and periode
                    var scheduleLessons = _dbContext.Entity<MsScheduleLesson>()
                        .Include(e=>e.Lesson)
                        .Where(x => x.ScheduleDate.Date >= item.StartDate.Date)
                        .Where(x => x.ScheduleDate.Date <= item.EndDate.Date)
                        .Where(x => x.IdGrade == item.StudentGrade.IdGrade)
                        .Where(x => listIdLessonByStudent.ToList().Contains(x.IdLesson))
                        .AsQueryable();

                    scheduleLessons = scheduleLessons.Where(x => (item.StartTime >= x.StartTime && item.StartTime <= x.EndTime)
                                                                    || (item.EndTime > x.StartTime && item.EndTime <= x.EndTime)
                                                                    || (item.StartTime <= x.StartTime && item.EndTime >= x.EndTime));

                    var scheduleLessonData = await scheduleLessons.Select(x => new
                    {
                        x.Id,
                        x.IdLesson,
                        x.IdDay,
                        x.IdSession,
                        x.IdWeek,
                        x.ScheduleDate,
                        x.Lesson.Semester,
                        StartTime = x.StartTime,
                        x.EndTime,
                        x.IsActive
                    }).ToListAsync(CancellationToken);

                    var listStudentEnrollmentUnionByStudent = listStudentEnrollmentUnion.Where(e => e.IdStudent == item.StudentGrade.IdStudent).ToList();

                    if (item.AttdAdministrationCancel.Any())
                    {
                        List<string> listIdScheduleLessonNew = new List<string>();
                        
                        foreach (var itemCancel in item.AttdAdministrationCancel)
                        {
                            itemCancel.ScheduleLesson.StartTime = itemCancel.ScheduleLesson.StartTime.Subtract(TimeSpan.FromMinutes(-1));

                            var excludeScheduleLesson = scheduleLessonData.Where(e => e.ScheduleDate == itemCancel.ScheduleLesson.ScheduleDate
                                                                 && ((itemCancel.ScheduleLesson.StartTime >= e.StartTime && itemCancel.ScheduleLesson.StartTime <= e.EndTime)
                                                                 || (itemCancel.ScheduleLesson.EndTime > e.StartTime && itemCancel.ScheduleLesson.EndTime < e.EndTime)
                                                                 || (itemCancel.ScheduleLesson.StartTime <= e.StartTime && itemCancel.ScheduleLesson.EndTime > e.EndTime))
                                                                ).ToList();

                            foreach (var itemScheduleLesson in excludeScheduleLesson)
                            {
                                var getStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnionByStudent, itemScheduleLesson.ScheduleDate, itemScheduleLesson.Semester.ToString(), itemScheduleLesson.IdLesson);

                                if (!getStudentEnrollmentMoving.Any())
                                    scheduleLessonData = scheduleLessonData.Where(e => e.Id != itemScheduleLesson.Id).ToList();

                                listIdScheduleLessonNew.Add(itemScheduleLesson.Id);
                            }
                        }

                        scheduleLessonData = scheduleLessonData.Where(e => !listIdScheduleLessonNew.Contains(e.Id)).ToList();

                        var listCancelRemove = item.AttdAdministrationCancel.Where(e => !listIdScheduleLessonNew.Contains(e.IdScheduleLesson)).ToList();
                        listCancelRemove.ForEach(e=>e.IsActive=false);
                        _dbContext.Entity<TrAttdAdministrationCancel>().UpdateRange(listCancelRemove);

                        var listIdScheduleLessonCancel = item.AttdAdministrationCancel.Select(e => e.IdScheduleLesson).ToList();
                        var listCancelAdd = listIdScheduleLessonNew.Where(e => !listIdScheduleLessonCancel.Contains(e)).ToList();

                        foreach (var idScheduleLesson in listCancelAdd)
                        {
                            var newTrAttendanceAdminCancel = new TrAttdAdministrationCancel
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdAttendanceAdministration = item.Id,
                                IdScheduleLesson = idScheduleLesson
                            };
                            _dbContext.Entity<TrAttdAdministrationCancel>().Add(newTrAttendanceAdminCancel);
                        }

                        #region Update Attendance Entry
                        var listIdscheduleLessonData = scheduleLessonData.Select(e => e.Id).ToList();

                        var removeAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                          .Where(e => listIdScheduleLessonCancel.Contains(e.IdScheduleLesson) && e.HomeroomStudent.IdStudent == item.StudentGrade.IdStudent)
                          .IgnoreQueryFilters()
                          .ToListAsync(CancellationToken);

                        var listAttendanceEntryGroup = removeAttendanceEntry
                                                      .GroupBy(e => new { e.IdScheduleLesson })
                                                      .ToList();

                        foreach (var itemAttendance in listAttendanceEntryGroup)
                        {
                            var listAttendanceEntryByItem = itemAttendance.OrderByDescending(e => e.DateIn).ToList();

                            //non aktifkan
                            var getAttendanceEntryNoActive = listAttendanceEntryByItem.FirstOrDefault();
                            if (getAttendanceEntryNoActive != null)
                            {
                                if (getAttendanceEntryNoActive.IsFromAttendanceAdministration)
                                {
                                    getAttendanceEntryNoActive.IsActive = false;
                                    _dbContext.Entity<TrAttendanceEntryV2>().Update(getAttendanceEntryNoActive);
                                }
                            }

                            //aktifkan data yang lama
                            var getAttendanceEntryActive = listAttendanceEntryByItem.Where(e=>e.PositionIn!= "Admin").FirstOrDefault();
                            if (getAttendanceEntryActive != null)
                            {
                                getAttendanceEntryActive.IsActive = true;
                                _dbContext.Entity<TrAttendanceEntryV2>().Update(getAttendanceEntryActive);
                            }
                        }
                        #endregion

                        #region Remove Attendance have id lesson old
                        var listScheduleLessonOld = await _dbContext.Entity<MsScheduleLesson>()
                              .Where(e => e.ScheduleDate.Date >= startDate.Date
                                      && e.ScheduleDate.Date <= endDate.Date
                                      && body.ListLessonMoveStudentEnroll.Select(x => x.IdLessonOld).Contains(e.IdLesson)
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
                                  e.StartTime,
                                  e.EndTime,
                                  e.SessionID,
                                  e.IdGrade
                              })
                              .ToListAsync(CancellationToken);


                        var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                                    .Include(e => e.ScheduleLesson)
                                                    .Where(e => e.IsFromAttendanceAdministration
                                                        && e.IdHomeroomStudent == body.IdHomeroomStudent
                                                        && e.ScheduleLesson.ScheduleDate.Date >= startDate.Date
                                                        && e.ScheduleLesson.ScheduleDate.Date <= endDate.Date
                                                        )
                                                    .Distinct()
                                                    .OrderBy(e => e.DateIn)
                                                    .ToListAsync(CancellationToken);


                        var listIdLesson = listScheduleLessonOld
                              .Where(e => e.ScheduleDate.Date >= item.StartDate.Date
                                      && e.ScheduleDate.Date <= item.EndDate.Date
                                      && ((item.StartTime >= e.StartTime && item.StartTime <= e.EndTime)
                                           || (item.EndTime > e.StartTime && item.EndTime <= e.EndTime)
                                           || (item.StartTime <= e.StartTime && item.EndTime >= e.EndTime))
                                      && e.IdGrade == item.StudentGrade.IdGrade
                                      )
                              .Select(e => e.Id)
                              .ToList();

                        if (!listIdLesson.Any())
                            continue;

                        var listAttendanceEntryByAdmin = listAttendanceEntry.Where(e => listIdLesson.Contains(e.IdScheduleLesson)).ToList();
                        listAttendanceEntryByAdmin = listAttendanceEntryByAdmin.Where(e => !listIdscheduleLessonData.Contains(e.IdScheduleLesson)).ToList();
                        listAttendanceEntryByAdmin.ForEach(e => e.IsActive = false);
                        _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(listAttendanceEntryByAdmin);
                        #endregion

                        await _dbContext.SaveChangesAsync();
                    }

                    var _scheduleLesson = scheduleLessonData.Select(x => x.Id).ToList();
                    #endregion

                    #region Find IdMappingAttendance For Each Student, case each student different level
                    MsAttendanceMappingAttendance mappingAttendance = new MsAttendanceMappingAttendance();
                    if (item.IdAttendance == "2")
                    {
                        mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                       .Include(x => x.MappingAttendance)
                           .ThenInclude(x => x.Level)
                       .Include(x => x.Attendance)
                       .Where(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
                       .Where(x => x.MappingAttendance.IdLevel == listHomeroomStudent.IdLevel)
                       .FirstOrDefaultAsync(CancellationToken);
                    }
                    else
                    {
                        mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                        .Include(x => x.MappingAttendance)
                            .ThenInclude(x => x.Level)
                        .Include(x => x.Attendance)
                        .Where(x => x.IdAttendance == item.IdAttendance)
                        .Where(x => x.MappingAttendance.IdLevel == listHomeroomStudent.IdLevel)
                        .FirstOrDefaultAsync(CancellationToken);
                    }


                    if (mappingAttendance is null)
                    {
                        var selectedMappingAttendance = await _dbContext.Entity<MsAttendance>()
                            .Where(x => x.Id == item.IdAttendance)
                            .Select(x => x.Description)
                            .FirstOrDefaultAsync(CancellationToken);

                        throw new BadRequestException($"Attendance name {selectedMappingAttendance} is not exist in level {listHomeroomStudent.IdLevel}.");
                    }
                    #endregion

                    var dataSchedule = await _dbContext.Entity<MsSchedule>()
                                  .Where(e => scheduleLessonData.Select(x => x.IdLesson).Contains(e.IdLesson))
                                  .ToListAsync(CancellationToken);

                    var staff = await _dbContext.Entity<MsStaff>()
                                      .ToListAsync(CancellationToken);

                    var attendanceEntryByGeneratedScheduleLesson = await _dbContext.Entity<TrAttendanceEntryV2>()
                        .Include(x => x.HomeroomStudent)
                        .Where(x => _scheduleLesson.Any(y => y == x.IdScheduleLesson) && x.HomeroomStudent.IdStudent == listHomeroomStudent.IdStudent).ToListAsync();


                    List<TrAttendanceEntryV2> attendanceEntries = new List<TrAttendanceEntryV2>();
                    // update any existing entries
                    if (attendanceEntryByGeneratedScheduleLesson.Any())
                    {
                        foreach (var newEntrySchedule in scheduleLessonData)
                        {
                            var getStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnionByStudent, Convert.ToDateTime(newEntrySchedule.ScheduleDate), newEntrySchedule.Semester.ToString(), newEntrySchedule.IdLesson);

                            if (!getStudentEnrollmentMoving.Any())
                                continue;

                            var idTeacher = dataSchedule
                              .Where(e => e.IdLesson == newEntrySchedule.IdLesson
                                       && e.IdDay == newEntrySchedule.IdDay
                                       && e.IdSession == newEntrySchedule.IdSession
                                       && e.IdWeek == newEntrySchedule.IdWeek)
                              .Select(e => e.IdUser)
                              .FirstOrDefault();

                            if (string.IsNullOrEmpty(idTeacher))
                            {
                                idTeacher = staff.Where(x => x.IdBinusian == AuthInfo.UserId).Select(x => x.IdBinusian).FirstOrDefault();
                                if (string.IsNullOrEmpty(idTeacher))
                                    throw new BadRequestException($"Your account does not have Binusian ID to enter attendance");
                            }

                            var attendanceEntry = attendanceEntryByGeneratedScheduleLesson.Where(e => e.IdScheduleLesson == newEntrySchedule.Id).FirstOrDefault();
                            attendanceEntry.IsActive = true;
                            _dbContext.Entity<TrAttendanceEntryV2>().Update(attendanceEntry);

                            TrAttendanceEntryV2 trAttendanceEntry = new TrAttendanceEntryV2
                            {
                                IdAttendanceEntry = Guid.NewGuid().ToString(),
                                IdScheduleLesson = newEntrySchedule.Id,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = item.AbsencesFile,
                                Notes = item.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true,
                                IdBinusian = idTeacher,
                                IdHomeroomStudent = getStudentEnrollmentMoving.Select(x => x.IdHomeroomStudent).FirstOrDefault(),
                                PositionIn = "Admin"
                            };
                            _dbContext.Entity<TrAttendanceEntryV2>().Add(trAttendanceEntry);
                        }
                    }
                    else
                    {
                        foreach (var newEntrySchedule in scheduleLessonData)
                        {
                            var getStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnionByStudent, Convert.ToDateTime(newEntrySchedule.ScheduleDate), newEntrySchedule.Semester.ToString(), newEntrySchedule.IdLesson);

                            if (!getStudentEnrollmentMoving.Any())
                                continue;

                            var idTeacher = dataSchedule
                              .Where(e => e.IdLesson == newEntrySchedule.IdLesson
                                       && e.IdDay == newEntrySchedule.IdDay
                                       && e.IdSession == newEntrySchedule.IdSession
                                       && e.IdWeek == newEntrySchedule.IdWeek)
                              .Select(e => e.IdUser)
                              .FirstOrDefault();

                            if (string.IsNullOrEmpty(idTeacher))
                            {
                                idTeacher = staff.Where(x => x.IdBinusian == AuthInfo.UserId).Select(x => x.IdBinusian).FirstOrDefault();
                                if (string.IsNullOrEmpty(idTeacher))
                                    throw new BadRequestException($"Your account does not have Binusian ID to enter attendance");
                            }
                            TrAttendanceEntryV2 trAttendanceEntry = new TrAttendanceEntryV2
                            {
                                IdAttendanceEntry = Guid.NewGuid().ToString(),
                                IdScheduleLesson = newEntrySchedule.Id,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = item.AbsencesFile,
                                Notes = item.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true,
                                IdBinusian = idTeacher,
                                IdHomeroomStudent = getStudentEnrollmentMoving.Select(x => x.IdHomeroomStudent).FirstOrDefault(),
                                PositionIn = "Admin",
                            };
                            _dbContext.Entity<TrAttendanceEntryV2>().Add(trAttendanceEntry);
                        }
                    }
                }
               
                await _dbContext.SaveChangesAsync();
            }
            #endregion

            return Request.CreateApiResult2();
        }
    }

    public class AttendanceEntryDate
    {
        public string IdAttendanceEntry { get; set; }
        public DateTime ScheduleStartDate { get; set; }
        public DateTime ScheduleEndDate { get; set; }
    }
}
