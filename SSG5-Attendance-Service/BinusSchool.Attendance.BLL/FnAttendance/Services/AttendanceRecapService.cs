using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model;

namespace BinusSchool.Attendance.FnAttendance.Services
{
    public class AttendanceRecapService : IAttendanceRecapService
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceRecapService(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<HomeroomStudentEnrollmentResult>> GetHomeroomStudentEnrollmentAsync(string idAcademicYear, string idLevel, string idStudent, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .ThenInclude(e => e.Level)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom)
                .ThenInclude(e => e.Classroom)
                .Include(e => e.Lesson)
                .Include(e => e.Subject)
                .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == idAcademicYear && e.HomeroomStudent.IdStudent == idStudent)
                .AsQueryable();

            if (!string.IsNullOrEmpty(idLevel))
                queryable =
                    queryable.Where(e =>
                        e.HomeroomStudent.Homeroom.Grade.IdLevel == idLevel);

            var results = await queryable
                .GroupBy(e => new HomeroomStudentEnrollmentResult
                {
                    IdLesson = e.IdLesson,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ClassId = e.Lesson.ClassIdGenerated,
                    IdStudent = e.HomeroomStudent.Student.Id,
                    FirstName = e.HomeroomStudent.Student.FirstName,
                    MiddleName = e.HomeroomStudent.Student.MiddleName,
                    LastName = e.HomeroomStudent.Student.LastName,
                    Homeroom = new ItemValueVm
                    {
                        Id = e.HomeroomStudent.IdHomeroom,
                        Description = e.HomeroomStudent.Homeroom.Grade.Code +
                                      e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                    },
                    IdClassroom = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                    ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    ClassroomName = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                    Grade = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.IdGrade,
                        Code = e.HomeroomStudent.Homeroom.Grade.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                        Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                    },
                    Semester = e.HomeroomStudent.Homeroom.Semester,
                    IdSubject = e.Subject.Id,
                    SubjectCode = e.Subject.Code,
                    SubjectName = e.Subject.Description,
                    SubjectID = e.Subject.SubjectID,
                    IsFromMaster = true,
                    IsDelete = false,
                    IdHomeroomStudentEnrollment = e.Id,
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return results;
        }

        public async Task<List<HomeroomStudentEnrollmentResult>> GetTrHomeroomStudentEnrollmentAsync(string idAcademicYear, string idLevel, string idStudent, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            var queryable = _dbContext.Entity<TrHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .ThenInclude(e => e.Level)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom)
                .ThenInclude(e => e.Classroom)
                .Include(e => e.LessonNew)
                .Include(e => e.SubjectNew)
                .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == idAcademicYear && e.HomeroomStudent.IdStudent == idStudent);

            if (!string.IsNullOrEmpty(idLevel))
                queryable =
                    queryable.Where(e =>
                        e.HomeroomStudent.Homeroom.Grade.IdLevel == idLevel);

            var results = await queryable
                .AsNoTracking()
                .GroupBy(e => new HomeroomStudentEnrollmentResult
                {
                    IdLesson = e.IdLessonNew,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ClassId = e.LessonNew.ClassIdGenerated,
                    IdStudent = e.HomeroomStudent.Student.Id,
                    FirstName = e.HomeroomStudent.Student.FirstName,
                    MiddleName = e.HomeroomStudent.Student.MiddleName,
                    LastName = e.HomeroomStudent.Student.LastName,
                    Homeroom = new ItemValueVm
                    {
                        Id = e.HomeroomStudent.IdHomeroom,
                        Description = e.HomeroomStudent.Homeroom.Grade.Code +
                                      e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code
                    },
                    IdClassroom = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                    ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    ClassroomName = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                    Grade = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.IdGrade,
                        Code = e.HomeroomStudent.Homeroom.Grade.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Description
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                        Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                        Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                    },
                    Semester = e.HomeroomStudent.Homeroom.Semester,
                    IdSubject = e.SubjectNew.Id,
                    SubjectCode = e.SubjectNew.Code,
                    SubjectName = e.SubjectNew.Description,
                    SubjectID = e.SubjectNew.SubjectID,
                    IsDelete = e.IsDelete,
                    IsFromMaster = false,
                    EffectiveDate = e.StartDate,
                    IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                    DateIn = e.DateIn
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return results;
        }

        public async Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear, string idLevel, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<MsPeriod>()
                .AsNoTracking()
                .AsQueryable();

            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (!string.IsNullOrWhiteSpace(idLevel))
                queryable = queryable.Where(e => e.Grade.IdLevel == idLevel);

            queryable = queryable.Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear);

            var listPeriod = await queryable
                .GroupBy(e => new PeriodResult
                {
                    IdPeriod = e.Id,
                    IdGrade = e.IdGrade,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    Semester = e.Semester,
                    IdLevel = e.Grade.IdLevel,
                    AttendanceStartDate = e.AttendanceStartDate,
                    AttendanceEndDate = e.AttendanceEndDate
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listPeriod;
        }

        public async Task<List<ScheduleLessonResult>> GetScheduleLessonAsync(string idAcademicYear, string idLevel, List<string> lessons, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<MsScheduleLesson>()
                .Include(e => e.Subject)
                .Include(e => e.Session)
                .Include(e => e.Lesson)
                .Where(e => e.IdAcademicYear == idAcademicYear &&
                            e.ScheduleDate.Date >= startDate &&
                            e.ScheduleDate.Date <= endDate &&
                            e.IsGenerated == true &&
                            e.IdLevel == idLevel)
                .AsNoTracking();

            var results = await queryable
                .GroupBy(e => new ScheduleLessonResult
                {
                    Id = e.Id,
                    ScheduleDate = e.ScheduleDate,
                    IdLesson = e.IdLesson,
                    ClassID = e.ClassID,
                    Session = new AttendanceSummarySessionResult
                    {
                        Id = e.Session.Id,
                        Name = e.Session.Name,
                        SessionID = e.Session.SessionID.ToString()
                    },
                    IdGrade = e.IdGrade,
                    IdDay = e.IdDay,
                    IdWeek = e.IdWeek,
                    IdAcademicYear = e.IdAcademicYear,
                    IdLevel = e.IdLevel,
                    Subject = new AttendanceSummarySubjectResult
                    {
                        Id = e.Subject.Id,
                        Code = e.Subject.Code,
                        Description = e.Subject.Description,
                        SubjectID = e.Subject.SubjectID,
                    },
                    Semester = e.Lesson.Semester
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            var listIdLesson = results.Select(e => e.IdLesson).Distinct().ToList();

            var lessonPathways = (await _dbContext.Entity<MsLessonPathway>()
                    .Include(e => e.HomeroomPathway)
                    .Where(e => listIdLesson.Contains(e.IdLesson))
                    .Select(e => new { e.IdLesson, e.HomeroomPathway.IdHomeroom })
                    .ToListAsync(cancellationToken))
                .GroupBy(e => e.IdLesson)
                .ToDictionary(e => e.Key, f => f.Select(g => g.IdHomeroom).ToList());

            foreach (var item in results)
                if (lessonPathways.TryGetValue(item.IdLesson, out var items))
                    foreach (var a in items)
                        if (item.LessonPathwayResults.All(e => e.IdHomeroom != a))
                            item.LessonPathwayResults.Add(new ScheduleLessonPathwayResult
                            {
                                IdHomeroom = a
                            });

            return results;
        }
    }
}
