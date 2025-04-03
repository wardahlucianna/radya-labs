using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.Validator;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using System.Linq;
using BinusSchool.Persistence.AttendanceDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using Microsoft.OData;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class GetCancelAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetCancelAttendanceHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCancelAttendanceRequest>();

            var getAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                .Include(e=>e.StudentGrade).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                .Include(e=>e.AttdAdministrationCancel)
                .Where(e => e.Id== param.IdAttendanceAdministration)
                .Distinct()
                .FirstOrDefaultAsync(CancellationToken);

            if (getAttendanceAdministration == null)
                throw new BadRequestException($"attendance administration is not found");

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                 .Where(e => e.Grade.Level.IdAcademicYear == getAttendanceAdministration.StudentGrade.Grade.Level.IdAcademicYear)
                 .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                 .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                 .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                 .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                 .Include(e => e.Lesson)
                                                 .Where(e => e.HomeroomStudent.IdStudent==getAttendanceAdministration.StudentGrade.IdStudent
                                                             && e.HomeroomStudent.Homeroom.IdAcademicYear == getAttendanceAdministration.StudentGrade.Grade.Level.IdAcademicYear)
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
                                                .Where(e => e.HomeroomStudent.IdStudent == getAttendanceAdministration.StudentGrade.IdStudent
                                                             && e.HomeroomStudent.Homeroom.IdAcademicYear == getAttendanceAdministration.StudentGrade.Grade.Level.IdAcademicYear)
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

            var scheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                    .Include(e=>e.Lesson)
                    .Where(e => e.ScheduleDate.Date >= getAttendanceAdministration.StartDate.Date
                            && e.ScheduleDate.Date <= getAttendanceAdministration.EndDate.Date
                            && ((getAttendanceAdministration.StartTime >= e.StartTime && getAttendanceAdministration.StartTime <= e.EndTime)
                                 || (getAttendanceAdministration.EndTime > e.StartTime && getAttendanceAdministration.EndTime <= e.EndTime)
                                 || (getAttendanceAdministration.StartTime <= e.StartTime && getAttendanceAdministration.EndTime >= e.EndTime))
                            && e.IdGrade == getAttendanceAdministration.StudentGrade.IdGrade
                            && listStudentEnrollmentUnion.Select(x => x.IdLesson).Contains(e.IdLesson)
                            )
                    .Select(e => new
                    {
                        e.Id,
                        e.ScheduleDate,
                        e.StartTime,
                        e.EndTime,
                        e.SessionID,
                        e.Lesson.Semester,
                        e.IdLesson
                    })
                    .OrderBy(e=>e.ScheduleDate).ThenBy(e=>e.StartTime)
                    .ToListAsync(CancellationToken);

            List<ScheduleLessonCancel> listScheduleLessonNew = new List<ScheduleLessonCancel>();
            foreach (var item in scheduleLesson)
            {
                var getStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion,item.ScheduleDate, item.Semester.ToString(), item.IdLesson);

                if (!getStudentEnrollmentMoving.Any())
                    continue;


                listScheduleLessonNew.Add(new ScheduleLessonCancel
                {
                    Date = item.ScheduleDate,
                    IdScheduleLesson = item.Id,
                    SessionId = item.SessionID,
                    IsSessionDisabled = getAttendanceAdministration.AttdAdministrationCancel
                                                                        .Where(w => w.IdScheduleLesson == item.Id)
                                                                        .Any(),
                    StartTime = item.StartTime,
                    EndTime = item.EndTime,
                });
            }

            var listDate = listScheduleLessonNew
                            .GroupBy(e => new
                            {
                                e.Date,
                            })
                            .ToList();

            List<GetCancelAttendanceResult> items = new List<GetCancelAttendanceResult>();
            foreach(var item in listDate)
            {
                var listScheduleLesson = item
                                        .GroupBy(e => new
                                        {
                                            IdScheduleLesson = e.IdScheduleLesson,
                                            SessionId = e.SessionId,
                                            IsSessionDisabled = e.IsSessionDisabled,
                                            StartTime = e.StartTime,
                                            EndTime = e.EndTime
                                        })
                                        .Select(e => new ScheduleLessonCancel
                                        {
                                            IdScheduleLesson = e.Key.IdScheduleLesson,
                                            SessionId = e.Key.SessionId,
                                            IsSessionDisabled = getAttendanceAdministration.AttdAdministrationCancel
                                                                        .Where(w => w.IdScheduleLesson == e.Key.IdScheduleLesson)
                                                                        .Any(),
                                            StartTime = e.Key.StartTime,
                                            EndTime = e.Key.EndTime,
                                        })
                                        .ToList();

                var countScheduleLesson = listScheduleLesson.Count();
                var countSessionDisabled = listScheduleLesson.Where(e => e.IsSessionDisabled).Count();

                var newCancelAttendance = new GetCancelAttendanceResult
                {
                    Date = item.Key.Date,
                    IsDayDisabled = countScheduleLesson == countSessionDisabled ? true : false,
                    ScheduleLessonCancels = listScheduleLesson
                };

                items.Add(newCancelAttendance);
            }

            return Request.CreateApiResult2(items as object);
        }
    }
}
