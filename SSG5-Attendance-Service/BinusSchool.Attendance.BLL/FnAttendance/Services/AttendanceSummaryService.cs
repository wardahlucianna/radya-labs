using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.Abstractions;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Attendance.FnAttendance.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Models;
using DocumentFormat.OpenXml.Drawing;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.Services
{
    public class AttendanceSummaryService : IAttendanceSummaryService
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _machineDateTime;

        public AttendanceSummaryService(IAttendanceDbContext dbContext, IMachineDateTime machineDateTime)
        {
            _dbContext = dbContext;
            _machineDateTime = machineDateTime;
        }

        public async Task<List<HomeroomStudentEnrollmentResult>> GetHomeroomStudentEnrollmentAsync(
            string idAcademicYear,
            string idLevel,
            CancellationToken cancellationToken)
        {
            var periods = await GetPeriodAsync(idAcademicYear, idLevel, cancellationToken);

            var queryable = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .ThenInclude(e => e.Level)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom)
                .ThenInclude(e => e.Classroom)
                .Include(e => e.Lesson)
                .Include(e => e.Subject)
                .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == idAcademicYear)
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

            results.ForEach(e =>
            {
                e.EffectiveDate = periods.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.DateIn = periods.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            return results;
        }

        public async Task<List<HomeroomStudentEnrollmentResult>> GetTrHomeroomStudentEnrollmentAsync(
            string idAcademicYear,
            string idLevel, CancellationToken cancellationToken)
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
                .Where(e => e.HomeroomStudent.Homeroom.Grade.Level.IdAcademicYear == idAcademicYear);

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

        public async Task<List<HomeroomTeacherResult>> GetHomeroomTeacherAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var queryHomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                .Include(e => e.Staff)
                .Where(x => x.Homeroom.IdAcademicYear == idAcademicYear);

            if (!string.IsNullOrEmpty(idLevel))
                queryHomeroomTeacher = queryHomeroomTeacher.Where(e => e.Homeroom.Grade.IdLevel == idLevel);

            var listHomeroomTeacher = await queryHomeroomTeacher
                .GroupBy(e => new HomeroomTeacherResult
                {
                    IdHomeroom = e.Homeroom.Id,
                    IdGrade = e.Homeroom.IdGrade,
                    IdClassroom = e.Homeroom.GradePathwayClassroom.IdClassroom,
                    IsAttendance = e.IsAttendance,
                    Teacher = new TeacherResult
                    {
                        IdUser = e.IdBinusian,
                        FirstName = e.Staff.FirstName,
                        LastName = e.Staff.LastName,
                    },
                    Position = new CodeWithIdVm
                    {
                        Id = e.TeacherPosition.LtPosition.Id,
                        Code = e.TeacherPosition.LtPosition.Code,
                        Description = e.TeacherPosition.LtPosition.Description
                    }
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listHomeroomTeacher;
        }

        public async Task<List<ScheduleResult>> GetScheduleAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            var querySchedule = _dbContext.Entity<MsSchedule>()
                .Include(e => e.User)
                .Include(e => e.Lesson)
                .Where(x => x.Lesson.IdAcademicYear == idAcademicYear);

            if (!string.IsNullOrEmpty(idLevel))
                querySchedule = querySchedule.Where(e => e.Lesson.Grade.IdLevel == idLevel);

            var listSchedule = await querySchedule
                .GroupBy(e => new ScheduleResult
                {
                    IdLesson = e.IdLesson,
                    Teacher = new TeacherResult
                    {
                        IdUser = e.IdUser,
                        FirstName = e.User.FirstName,
                        LastName = e.User.LastName
                    },
                    IdWeek = e.IdWeek,
                    IdDay = e.IdDay,
                    IdSession = e.IdSession,
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listSchedule;
        }

        public async Task<List<NonTeachingLoadResult>> GetNonTeachingLoadResultAsync(string idAcademicYear,
            CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<TrNonTeachingLoad>()
                .Include(e => e.NonTeachingLoad).ThenInclude(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                .Where(x => x.Data != null && x.NonTeachingLoad.IdAcademicYear == idAcademicYear);

            var listNonTeachingLoad = await queryable
                .GroupBy(e => new NonTeachingLoadResult
                {
                    IdUserTeacher = e.IdUser,
                    PositionCode = e.NonTeachingLoad.TeacherPosition.LtPosition.Code,
                    Data = e.Data
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listNonTeachingLoad;
        }

        public async Task<List<DepartmentResult>> GetDepartmentLevelResultAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var queryable = _dbContext.Entity<MsDepartmentLevel>()
                .Include(e => e.Department)
                .Where(e => e.Department.IdAcademicYear == idAcademicYear);

            if (!string.IsNullOrEmpty(idLevel))
                queryable = queryable.Where(e => e.IdLevel == idLevel);

            var listDepartmentLevel = await queryable
                .SelectMany(e => e.Level.Grades.Select(f => new DepartmentResult
                {
                    IdDepartment = e.Id,
                    IdGrade = f.Id
                }))
                .GroupBy(e => new DepartmentResult
                {
                    IdDepartment = e.IdDepartment,
                    IdGrade = e.IdGrade,
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return listDepartmentLevel;
        }

        public Task<StudentStatusResult> GetStudentStatusResultAsync(string idAcademicYear, string idStudent,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<List<AttendanceEntryResult>> GetAttendanceEntryAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<List<AttendanceEntryResult>> GetAttendanceEntryByStudentAsync(string idAcademicYear, string idStudent, DateTime startDate,DateTime endDate,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(idStudent))
                throw new InvalidOperationException();

            var queryAttendanceEntry = _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Session)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Subject)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Where(e => e.ScheduleLesson.IdAcademicYear == idAcademicYear && e.HomeroomStudent.IdStudent == idStudent &&
                            e.ScheduleLesson.ScheduleDate.Date >= startDate && e.ScheduleLesson.ScheduleDate.Date <= endDate &&
                            e.ScheduleLesson.IsGenerated == true);

            var results = await queryAttendanceEntry
                .GroupBy(e => new AttendanceEntryResult
                {
                    IdAttendanceEntry = e.IdAttendanceEntry,
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ScheduleDate = e.ScheduleLesson.ScheduleDate,
                    IdLesson = e.ScheduleLesson.IdLesson,
                    ClassID = e.ScheduleLesson.ClassID,
                    IdGrade = e.ScheduleLesson.IdGrade,
                    IdDay = e.ScheduleLesson.IdDay,
                    IdWeek = e.ScheduleLesson.IdWeek,
                    IdAcademicYear = e.ScheduleLesson.IdAcademicYear,
                    IdLevel = e.ScheduleLesson.IdLevel,
                    IdStudent = e.HomeroomStudent.IdStudent,
                    Session = new AttendanceSummarySessionResult
                    {
                        Id = e.ScheduleLesson.Session.Id,
                        Name = e.ScheduleLesson.Session.Name,
                        SessionID = e.ScheduleLesson.Session.SessionID.ToString()
                    },
                    Subject = new AttendanceSummarySubjectResult
                    {
                        Id = e.ScheduleLesson.Subject.Id,
                        Code = e.ScheduleLesson.Subject.SubjectID,
                        Description = e.ScheduleLesson.Subject.Description,
                        SubjectID = e.ScheduleLesson.Subject.SubjectID,
                    },
                    Status = e.Status,
                    Attendance = new AttendanceSummaryAttendanceResult
                    {
                        Id = e.AttendanceMappingAttendance.Attendance.Id,
                        Code = e.AttendanceMappingAttendance.Attendance.Code,
                        Description = e.AttendanceMappingAttendance.Attendance.Description,
                        AbsenceCategory = e.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                        ExcusedAbsenceCategory = e.AttendanceMappingAttendance.Attendance.ExcusedAbsenceCategory
                    },
                    Notes = e.Notes,
                    IdUserTeacher = e.IdBinusian,
                    Student = new StudentResult
                    {
                        IdStudent = e.HomeroomStudent.Student.Id,
                        FirstName = e.HomeroomStudent.Student.FirstName,
                        LastName = e.HomeroomStudent.Student.LastName,
                        MiddleName = e.HomeroomStudent.Student.MiddleName,
                    },
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    IdUserAttendanceEntry = e.UserIn,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    // AttendanceEntryWorkhabit = e.AttendanceEntryWorkhabitV2s.Select(e =>
                    //     new AttendanceEntryWorkhabitResult
                    //     {
                    //         IdAttendanceEntry = e.IdAttendanceEntry,
                    //         IdMappingAttendanceWorkhabit = e.IdMappingAttendanceWorkhabit
                    //     }).ToList(),
                    Semester = e.ScheduleLesson.Lesson.Semester,
                    GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                    Classroom = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                        Description = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Code = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    }
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            var ids = results.Select(e => e.IdAttendanceEntry).ToList();

            var workHabits = await _dbContext.Entity<TrAttendanceEntryWorkhabitV2>()
                .Where(e => ids.Contains(e.IdAttendanceEntry))
                .Select(e => new AttendanceEntryWorkhabitResult
                {
                    IdAttendanceEntry = e.IdAttendanceEntry,
                    IdMappingAttendanceWorkhabit = e.IdMappingAttendanceWorkhabit
                })
                .ToListAsync(cancellationToken);

            var dict = workHabits.GroupBy(e => e.IdAttendanceEntry)
                .ToDictionary(e => e.Key, y => y.ToList());

            foreach (var item in results)
                item.AttendanceEntryWorkhabit = dict.TryGetValue(item.IdAttendanceEntry, out var value) ? value : new List<AttendanceEntryWorkhabitResult>();


            return results;
        }

        public async Task<List<ScheduleLessonResult>> GetScheduleLessonAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            var dt = _machineDateTime.ServerTime.Date;
            var queryable = _dbContext.Entity<MsScheduleLesson>()
                .Include(e => e.Subject)
                .Include(e => e.Session)
                .Include(e => e.Lesson)
                .Where(e => e.IdAcademicYear == idAcademicYear &&
                            e.ScheduleDate.Date <= dt &&
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

        public async Task<List<MappingAttendanceResult>> GetMappingAttendanceAsync(string idAcademicYear,
            string idLevel,
            CancellationToken cancellationToken)
        {
            if (idAcademicYear == null) throw new InvalidOperationException(nameof(idAcademicYear));
            if (idLevel == null) throw new InvalidOperationException(nameof(idLevel));

            var queryMappingAttendance = _dbContext.Entity<MsMappingAttendance>()
                .Where(e => e.Level.IdAcademicYear == idAcademicYear && e.IdLevel == idLevel);

            var results = await queryMappingAttendance
                .GroupBy(e => new MappingAttendanceResult
                {
                    Id = e.Id,
                    IdLevel = e.IdLevel,
                    AbsentTerms = e.AbsentTerms,
                    IsNeedValidation = e.IsNeedValidation,
                    IsUseWorkhabit = e.IsUseWorkhabit,
                    IsUseDueToLateness = e.IsUseDueToLateness,
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return results;
        }

        public async Task<List<LessonTeacherResult>> GetLessonTeacherAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
        {
            if (idAcademicYear == null) throw new InvalidOperationException(nameof(idAcademicYear));
            if (idLevel == null) throw new InvalidOperationException(nameof(idLevel));

            var queryable = _dbContext.Entity<MsLessonTeacher>()
                .Include(e => e.Lesson)
                .Where(e => e.Lesson.IdAcademicYear == idAcademicYear && e.IsAttendance &&
                            e.Lesson.Grade.IdLevel == idLevel);

            var results = await queryable
                .GroupBy(e => new LessonTeacherResult
                {
                    IdUserTeacher = e.IdUser,
                    IdLesson = e.IdLesson,
                    ClassId = e.Lesson.ClassIdGenerated,
                    IsAttendance = e.IsAttendance
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            return results;
        }

        public async Task<List<AttendanceMappingAttendanceResult>> GetAttendanceMappingAttendanceAsync(
            string idAcademicYear,
            string idLevel, CancellationToken cancellationToken)
        {
            if (idAcademicYear == null) throw new InvalidOperationException(nameof(idAcademicYear));
            if (idLevel == null) throw new InvalidOperationException(nameof(idLevel));

            var queryable = _dbContext.Entity<MsAttendanceMappingAttendance>()
                .Include(x => x.MappingAttendance)
                .Include(x => x.Attendance)
                .Where(x => x.Attendance.IdAcademicYear == idAcademicYear && x.MappingAttendance.IdLevel == idLevel);

            var results = await queryable
                .Select(e => new AttendanceMappingAttendanceResult
                {
                    Id = e.Id,
                    AbsenceCategory = e.Attendance.AbsenceCategory,
                })
                .ToListAsync(cancellationToken);

            return results;
        }

        public Task<List<CodeWithIdVm>> GetMsAttendanceMappingAttendanceAsync(string idLevel,
            CancellationToken cancellationToken)
        {
            return _dbContext.Entity<MsAttendanceMappingAttendance>()
                .Include(x => x.MappingAttendance)
                .Include(x => x.Attendance)
                .Where(x => x.MappingAttendance.IdLevel == idLevel)
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Attendance.Code,
                    Description = x.Attendance.Description
                }).ToListAsync(cancellationToken);
        }

        public Task<List<CodeWithIdVm>> GetMsMappingAttendanceWorkhabitAsync(string idLevel,
            CancellationToken cancellationToken)
        {
            return _dbContext.Entity<MsMappingAttendanceWorkhabit>()
                .Include(x => x.MappingAttendance)
                .Include(x => x.Workhabit)
                .Where(x => x.MappingAttendance.IdLevel == idLevel)
                .Select(x => new CodeWithIdVm
                {
                    Id = x.Id,
                    Code = x.Workhabit.Code,
                    Description = x.Workhabit.Description
                }).ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<int, List<HomeroomResult>>> GetHomeroomsGroupedBySemester(string idGrade,
            CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<MsHomeroom>()
                .Where(e => e.IdGrade == idGrade)
                .OrderBy(e => e.DateIn)
                .Select(e => new HomeroomResult
                {
                    IdHomeroom = e.Id,
                    Semester = e.Semester
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.Semester)
                .ToDictionary(e => e.Key, g => g.ToList());
        }

        public async Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesGroupedAsync(
            string[] idSchedules, CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Where(e => idSchedules.Contains(e.IdScheduleLesson))
                .Select(e => new AttendanceEntryResult
                {
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    Status = e.Status,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    //PositionIn = e.PositionIn,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    DateIn = e.DateIn.Value,
                    ScheduleDate = e.ScheduleLesson.ScheduleDate,
                    IdStudent = e.HomeroomStudent.IdStudent,
                    AttendanceEntryWorkhabit = e.AttendanceEntryWorkhabitV2s.Select(f =>
                        new AttendanceEntryWorkhabitResult
                        {
                            IdAttendanceEntry = f.IdAttendanceEntry,
                            IdAttendanceEntryWorkhabit = f.Id,
                            IdMappingAttendanceWorkhabit = f.IdMappingAttendanceWorkhabit
                        }).ToList()
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.IdScheduleLesson).ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<Dictionary<string, List<AttendanceEntryResult>>> GetAttendanceEntriesPendingGroupedAsync(
            string[] idSchedules, CancellationToken cancellationToken)
        {
            var results = await _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Include(e => e.ScheduleLesson)
                .Where(e => idSchedules.Contains(e.IdScheduleLesson) && e.ScheduleLesson.IsGenerated == true && e.Status == Common.Model.Enums.AttendanceEntryStatus.Pending)
                .Select(e => new AttendanceEntryResult
                {
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    Status = e.Status,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    //PositionIn = e.PositionIn,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    DateIn = e.DateIn.Value,
                    AttendanceEntryWorkhabit = e.AttendanceEntryWorkhabitV2s.Select(f =>
                        new AttendanceEntryWorkhabitResult
                        {
                            IdAttendanceEntry = f.IdAttendanceEntry,
                            IdAttendanceEntryWorkhabit = f.Id,
                            IdMappingAttendanceWorkhabit = f.IdMappingAttendanceWorkhabit
                        }).ToList()
                }).ToListAsync(cancellationToken);

            return results.GroupBy(e => e.IdScheduleLesson).ToDictionary(g => g.Key, g => g.ToList());
        }

        public async Task<List<StudentEnrollmentDto2>> GetStudentEnrolledAsync(string idHomeroom,
            DateTime startAttendanceDt,
            CancellationToken cancellationToken)
        {
            var students = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(e => e.Student)
                .Include(e => e.Homeroom)
                .ThenInclude(e => e.Grade)
                .Include(e => e.Homeroom)
                .ThenInclude(e => e.GradePathwayClassroom)
                .ThenInclude(e => e.Classroom)
                .Where(e => e.IdHomeroom == idHomeroom)
                .Select(e => new MsHomeroomStudent
                {
                    Id = e.Id,
                    IdStudent = e.IdStudent,
                    Student = new MsStudent
                    {
                        Id = e.Student.Id,
                        FirstName = e.Student.FirstName,
                        MiddleName = e.Student.MiddleName,
                        LastName = e.Student.LastName
                    },
                    Homeroom = new MsHomeroom
                    {
                        Id = e.IdHomeroom,
                        GradePathwayClassroom = new MsGradePathwayClassroom
                        {
                            Classroom = new MsClassroom
                            {
                                Id = e.Homeroom.GradePathwayClassroom.Classroom.Id,
                                Code = e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                Description = e.Homeroom.GradePathwayClassroom.Classroom.Description,
                            }
                        },
                        Grade = new MsGrade
                        {
                            Code = e.Homeroom.Grade.Code
                        }
                    },
                })
                .ToListAsync(cancellationToken);

            var idHomeroomStudents = students.Select(e => e.Id).Distinct().ToList();

            var allHomeroomStudentEnrollments = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Where(e => idHomeroomStudents.Contains(e.IdHomeroomStudent))
                .Select(e => new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = e.Id,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdLesson = e.IdLesson,
                    IdLessonOld = null,
                    Date = startAttendanceDt,
                    DateIn = e.DateIn.Value,
                    IsDeleted = false
                })
                .ToListAsync(cancellationToken);

            var allTrHomeroomStudentEnrollments = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                .Where(e => idHomeroomStudents.Contains(e.IdHomeroomStudent))
                .Select(e => new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdLessonOld = e.IdLessonOld,
                    IdLesson = e.IdLessonNew,
                    Date = e.StartDate,
                    DateIn = e.DateIn.Value,
                    IsDeleted = e.IsDelete,
                    Flag = 1
                })
                .ToListAsync(cancellationToken);

            var allUnionList = allHomeroomStudentEnrollments.Union(allTrHomeroomStudentEnrollments)
                .GroupBy(e => e.IdHomeroomStudent)
                .ToDictionary(g => g.Key, g => g.ToList());

            var list = new List<StudentEnrollmentDto2>();

            foreach (var item in students)
            {
                if (!allUnionList.ContainsKey(item.Id))
                    continue;

                var vmItem = new StudentEnrollmentDto2
                {
                    IdHomeroomStudent = item.Id,
                    IdStudent = item.IdStudent,
                    FirstName = item.Student.FirstName,
                    MiddleName = item.Student.MiddleName,
                    LastName = item.Student.LastName,
                    ClassroomCode = item.Homeroom.GradePathwayClassroom.Classroom.Code,
                    GraceCode = item.Homeroom.Grade.Code,
                    IdHomeroom = item.Homeroom.Id
                };

                var fixedList = allUnionList[item.Id]
                    .GroupBy(e => e.IdHomeroomStudentEnrollment)
                    .Select(e => new
                    {
                        e.Key, Items =
                            e.OrderBy(y => y.Flag)
                                .ThenBy(y => y.Date)
                                .ToList()
                    })
                    .ToList();

                foreach (var item2 in fixedList)
                {
                    //tidak pernah di moving
                    if (item2.Items.Count == 1)
                    {
                        vmItem.Items.Add(new StudentEnrollmentItemDto
                        {
                            IdLesson = item2.Items[0].IdLesson,
                            StartDt = startAttendanceDt,
                            Ignored = false
                        });
                        continue;
                    }

                    //logic moving
                    var fixedItems = RecalculateHomeroomStudentEnroll(item2.Items);

                    for (var i = 0; i < fixedItems.Count; i++)
                    {
                        var vmChildItem = new StudentEnrollmentItemDto
                        {
                            IdLesson = fixedItems[i].IdLesson,
                            StartDt = fixedItems[i].Date,
                        };

                        if (i + 1 < fixedItems.Count)
                            vmChildItem.EndDt = fixedItems[i + 1].Date;

                        vmItem.Items.Add(vmChildItem);
                    }
                }

                list.Add(vmItem);
            }

            return list;
        }

        public async Task<List<StudentStatusDto>> GetStudentStatusesAsync(string[] studentIds,
            string idAcademicYear,
            CancellationToken cancellationToken)
        {
            var list = new List<StudentStatusDto>();

            var queryable = _dbContext.Entity<TrStudentStatus>()
                .AsQueryable();

            var studentStatuses = await queryable
                .Select(e => new TrStudentStatus
                {
                    IdTrStudentStatus = e.IdTrStudentStatus,
                    IdAcademicYear = e.IdAcademicYear,
                    IdStudent = e.IdStudent,
                    StartDate = e.StartDate,
                    EndDate = e.EndDate,
                    ActiveStatus = e.ActiveStatus
                })
                .Where(e => studentIds.Contains(e.IdStudent) && e.ActiveStatus && e.IdAcademicYear == idAcademicYear && e.StartDate <= _machineDateTime.ServerTime.Date)
                .ToListAsync(cancellationToken);

            foreach (var item in studentStatuses)
            {
                var vmItem = new StudentStatusDto
                {
                    IdStudent = item.IdStudent,
                    IsActive = item.ActiveStatus,
                    StartDt = item.StartDate,
                    EndDt = DateTime.MaxValue
                };

                if (item.EndDate.HasValue)
                    vmItem.EndDt = item.EndDate.Value;

                list.Add(vmItem);
            }

            return list;
        }

        private List<HomeroomStudentEnrollDto> RecalculateHomeroomStudentEnroll(List<HomeroomStudentEnrollDto> data)
        {
            var list = new List<HomeroomStudentEnrollDto>();
            var fixedList = new List<HomeroomStudentEnrollDto>();
            //construct data to a single row
            for (var i = 1; i < data.Count; i++)
            {
                if (i == 1)
                {
                    list.Add(new HomeroomStudentEnrollDto
                    {
                        IdHomeroomStudentEnrollment = data[0].IdHomeroomStudentEnrollment,
                        IdHomeroomStudent = data[0].IdHomeroomStudent,
                        IdLesson = data[i].IdLessonOld,
                        Date = data[0].Date,
                        DateIn = data[0].DateIn,
                        IsDeleted = data[0].IsDeleted
                    });

                    list.Add(new HomeroomStudentEnrollDto
                    {
                        IdHomeroomStudentEnrollment = data[i].IdHomeroomStudentEnrollment,
                        IdHomeroomStudent = data[i].IdHomeroomStudent,
                        IdLesson = data[i].IdLesson,
                        Date = data[i].Date,
                        DateIn = data[i].DateIn,
                        IsDeleted = data[i].IsDeleted
                    });

                    continue;
                }

                list.Add(new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = data[i].IdHomeroomStudentEnrollment,
                    IdHomeroomStudent = data[i].IdHomeroomStudent,
                    IdLesson = data[i].IdLesson,
                    Date = data[i].Date,
                    DateIn = data[i].DateIn,
                    IsDeleted = data[i].IsDeleted
                });
            }

            var index = 0;
            foreach (var item in list)
            {
                if (index == 0)
                {
                    fixedList.Add(item);
                    index++;
                    continue;
                }

                if (fixedList.Last().IdLesson == item.IdLesson)
                {
                    fixedList.Last().Date = item.Date;
                    fixedList.Last().DateIn = item.DateIn;
                    index++;
                    continue;
                }

                fixedList.Add(item);
                index++;
            }

            fixedList.Reverse();
            list.Clear();

            foreach (var t in fixedList)
                if (t.Date.Date <= _machineDateTime.ServerTime.Date)
                {
                    if (list.Any())
                    {
                        if (t.Date.Date < list.Last().Date.Date)
                            list.Add(t);
                    }
                    else
                        list.Add(t);
                }

            list.Reverse();

            return list;
        }

        public async Task<List<PeriodResult>> GetPeriodAsync(string idAcademicYear, string idLevel,
            CancellationToken cancellationToken)
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

        public async Task<List<StudentEnrollmentDto2>> GetStudentEnrolledByStudentAsync(string idAcademicYear, string idStudent,
        DateTime startAttendanceDt,
        CancellationToken cancellationToken)
        {
            var students = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(e => e.Student)
                .Include(e => e.Homeroom)
                .ThenInclude(e => e.Grade)
                .Include(e => e.Homeroom)
                .ThenInclude(e => e.GradePathwayClassroom)
                .ThenInclude(e => e.Classroom)
                .Where(e => e.IdStudent == idStudent && e.Homeroom.IdAcademicYear == idAcademicYear)
                .Select(e => new MsHomeroomStudent
                {
                    Id = e.Id,
                    IdStudent = e.IdStudent,
                    Student = new MsStudent
                    {
                        Id = e.Student.Id,
                        FirstName = e.Student.FirstName,
                        MiddleName = e.Student.MiddleName,
                        LastName = e.Student.LastName
                    },
                    Homeroom = new MsHomeroom
                    {
                        Id = e.IdHomeroom,
                        GradePathwayClassroom = new MsGradePathwayClassroom
                        {
                            Classroom = new MsClassroom
                            {
                                Id = e.Homeroom.GradePathwayClassroom.Classroom.Id,
                                Code = e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                Description = e.Homeroom.GradePathwayClassroom.Classroom.Description,
                            }
                        },
                        Grade = new MsGrade
                        {
                            Code = e.Homeroom.Grade.Code,
                            Id = e.Homeroom.Grade.Id
                            
                        }
                    },
                })
                .ToListAsync(cancellationToken);

            var idHomeroomStudents = students.Select(e => e.Id).ToList();

            var allHomeroomStudentEnrollments = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                .Where(e => idHomeroomStudents.Contains(e.IdHomeroomStudent))
                .Select(e => new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = e.Id,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdLesson = e.IdLesson,
                    IdLessonOld = null,
                    Date = startAttendanceDt,
                    DateIn = e.DateIn.Value,
                    IsDeleted = false
                })
                .ToListAsync(cancellationToken);

            var allTrHomeroomStudentEnrollments = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                .Where(e => idHomeroomStudents.Contains(e.IdHomeroomStudent))
                .Select(e => new HomeroomStudentEnrollDto
                {
                    IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    IdLessonOld = e.IdLessonOld,
                    IdLesson = e.IdLessonNew,
                    Date = e.StartDate,
                    DateIn = e.DateIn.Value,
                    IsDeleted = e.IsDelete,
                    Flag = 1
                })
                .ToListAsync(cancellationToken);

            var allUnionList = allHomeroomStudentEnrollments.Union(allTrHomeroomStudentEnrollments)
                .GroupBy(e => e.IdHomeroomStudent)
                .ToDictionary(g => g.Key, g => g.ToList());

            var list = new List<StudentEnrollmentDto2>();

            foreach (var item in students)
            {
                if (!allUnionList.ContainsKey(item.Id))
                    continue;

                var vmItem = new StudentEnrollmentDto2
                {
                    IdHomeroomStudent = item.Id,
                    IdStudent = item.IdStudent,
                    FirstName = item.Student.FirstName,
                    MiddleName = item.Student.MiddleName,
                    LastName = item.Student.LastName,
                    ClassroomCode = item.Homeroom.GradePathwayClassroom.Classroom.Code,
                    GraceCode = item.Homeroom.Grade.Code,
                    GraceId = item.Homeroom.Grade.Id,
                    IdHomeroom = item.Homeroom.Id
                };

                var fixedList = allUnionList[item.Id]
                    .GroupBy(e => e.IdHomeroomStudentEnrollment)
                    .Select(e => new
                    {
                        e.Key,
                        Items =
                            e.OrderBy(y => y.Flag)
                                .ThenBy(y => y.Date)
                                .ToList()
                    })
                    .ToList();

                foreach (var item2 in fixedList)
                {
                    //tidak pernah di moving
                    if (item2.Items.Count == 1)
                    {
                        vmItem.Items.Add(new StudentEnrollmentItemDto
                        {
                            IdLesson = item2.Items[0].IdLesson,
                            StartDt = startAttendanceDt,
                            Ignored = false
                        });
                        continue;
                    }

                    //logic moving
                    var fixedItems = RecalculateHomeroomStudentEnroll(item2.Items);

                    for (var i = 0; i < fixedItems.Count; i++)
                    {
                        var vmChildItem = new StudentEnrollmentItemDto
                        {
                            IdLesson = fixedItems[i].IdLesson,
                            StartDt = fixedItems[i].Date,
                        };

                        if (i + 1 < fixedItems.Count)
                            vmChildItem.EndDt = fixedItems[i + 1].Date;

                        vmItem.Items.Add(vmChildItem);
                    }
                }

                list.Add(vmItem);
            }

            return list;
        }

        public async Task<MsFormula> GetFormulaAsync(string idAcademicYear, string idLevel, CancellationToken cancellationToken)
        {
            if (idAcademicYear == null) throw new InvalidOperationException(nameof(idAcademicYear));
            if (idLevel == null) throw new InvalidOperationException(nameof(idLevel));

            var queryFormula = _dbContext.Entity<MsFormula>()
                                          .Where(x => x.Level.IdAcademicYear == idAcademicYear && x.IdLevel == idLevel);

            var results = await queryFormula.FirstOrDefaultAsync(cancellationToken);

            return results;
        }

        public async Task<List<ScheduleLessonResult>> GetScheduleLessonByGradeAsync(string idAcademicYear, string idGrade,
                    CancellationToken cancellationToken)
        {
            var dt = _machineDateTime.ServerTime.Date;
            var queryable = _dbContext.Entity<MsScheduleLesson>()
                .Include(e => e.Subject)
                .Include(e => e.Session)
                .Include(e => e.Lesson)
                .Where(e => e.IdAcademicYear == idAcademicYear &&
                            e.ScheduleDate.Date <= dt &&
                            e.IsGenerated == true &&
                            e.IdGrade == idGrade)
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

        public async Task<List<AttendanceEntryResult>> GetAttendanceEntryUnexcusedAbsenceAsync(string idAcademicYear, string idLevel, string idGrade, 
            List<string> ListStudent, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(idAcademicYear))
                throw new InvalidOperationException();

            if (string.IsNullOrWhiteSpace(idLevel))
                throw new InvalidOperationException();

            if (ListStudent.Count == 0)
                throw new InvalidOperationException();

            var queryAttendanceEntry = _dbContext.Entity<TrAttendanceEntryV2>()
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Session)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Subject)
                .Include(e => e.ScheduleLesson).ThenInclude(e => e.Lesson)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                .Include(e => e.AttendanceEntryWorkhabitV2s)
                .Where(e => e.ScheduleLesson.IdAcademicYear == idAcademicYear && ListStudent.Contains(e.HomeroomStudent.IdStudent) &&
                            e.ScheduleLesson.IdLevel == idLevel &&
                            e.AttendanceMappingAttendance.Attendance.AbsenceCategory == AbsenceCategory.Unexcused &&
                            e.Status == AttendanceEntryStatus.Submitted &&
                            e.ScheduleLesson.IsGenerated == true);

            if (!string.IsNullOrWhiteSpace(idGrade))
                queryAttendanceEntry = queryAttendanceEntry.Where(e => e.ScheduleLesson.IdGrade == idGrade);

            if (startDate.Date != DateTime.Parse("1999-01-01") && endDate.Date != DateTime.Parse("1999-01-01"))
                queryAttendanceEntry = queryAttendanceEntry.Where(e => e.ScheduleLesson.ScheduleDate.Date >= startDate && e.ScheduleLesson.ScheduleDate.Date <= endDate);

            var results = await queryAttendanceEntry
                .GroupBy(e => new AttendanceEntryResult
                {
                    IdAttendanceEntry = e.IdAttendanceEntry,
                    IdScheduleLesson = e.IdScheduleLesson,
                    IdHomeroomStudent = e.IdHomeroomStudent,
                    ScheduleDate = e.ScheduleLesson.ScheduleDate,
                    IdLesson = e.ScheduleLesson.IdLesson,
                    ClassID = e.ScheduleLesson.ClassID,
                    IdGrade = e.ScheduleLesson.IdGrade,
                    IdDay = e.ScheduleLesson.IdDay,
                    IdWeek = e.ScheduleLesson.IdWeek,
                    IdAcademicYear = e.ScheduleLesson.IdAcademicYear,
                    IdLevel = e.ScheduleLesson.IdLevel,
                    IdStudent = e.HomeroomStudent.IdStudent,
                    Session = new AttendanceSummarySessionResult
                    {
                        Id = e.ScheduleLesson.Session.Id,
                        Name = e.ScheduleLesson.Session.Name,
                        SessionID = e.ScheduleLesson.Session.SessionID.ToString()
                    },
                    Subject = new AttendanceSummarySubjectResult
                    {
                        Id = e.ScheduleLesson.Subject.Id,
                        Code = e.ScheduleLesson.Subject.SubjectID,
                        Description = e.ScheduleLesson.Subject.Description,
                        SubjectID = e.ScheduleLesson.Subject.SubjectID,
                    },
                    Status = e.Status,
                    Attendance = new AttendanceSummaryAttendanceResult
                    {
                        Id = e.AttendanceMappingAttendance.Attendance.Id,
                        Code = e.AttendanceMappingAttendance.Attendance.Code,
                        Description = e.AttendanceMappingAttendance.Attendance.Description,
                        AbsenceCategory = e.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                        ExcusedAbsenceCategory = e.AttendanceMappingAttendance.Attendance.ExcusedAbsenceCategory
                    },
                    Notes = e.Notes,
                    IdUserTeacher = e.IdBinusian,
                    Student = new StudentResult
                    {
                        IdStudent = e.HomeroomStudent.Student.Id,
                        FirstName = e.HomeroomStudent.Student.FirstName,
                        LastName = e.HomeroomStudent.Student.LastName,
                        MiddleName = e.HomeroomStudent.Student.MiddleName,
                    },
                    IdAttendanceMappingAttendance = e.IdAttendanceMappingAttendance,
                    IdUserAttendanceEntry = e.UserIn,
                    IsFromAttendanceAdministration = e.IsFromAttendanceAdministration,
                    // AttendanceEntryWorkhabit = e.AttendanceEntryWorkhabitV2s.Select(e =>
                    //     new AttendanceEntryWorkhabitResult
                    //     {
                    //         IdAttendanceEntry = e.IdAttendanceEntry,
                    //         IdMappingAttendanceWorkhabit = e.IdMappingAttendanceWorkhabit
                    //     }).ToList(),
                    Semester = e.ScheduleLesson.Lesson.Semester,
                    GradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                    Classroom = new CodeWithIdVm
                    {
                        Id = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Id,
                        Description = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Description,
                        Code = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                    },
                    IdHomeroom = e.HomeroomStudent.Homeroom.Id
                })
                .Select(e => e.Key)
                .ToListAsync(cancellationToken);

            var ids = results.Select(e => e.IdAttendanceEntry).ToList();

            var workHabits = await _dbContext.Entity<TrAttendanceEntryWorkhabitV2>()
                .Where(e => ids.Contains(e.IdAttendanceEntry))
                .Select(e => new AttendanceEntryWorkhabitResult
                {
                    IdAttendanceEntry = e.IdAttendanceEntry,
                    IdMappingAttendanceWorkhabit = e.IdMappingAttendanceWorkhabit
                })
                .ToListAsync(cancellationToken);

            var dict = workHabits.GroupBy(e => e.IdAttendanceEntry)
                .ToDictionary(e => e.Key, y => y.ToList());

            foreach (var item in results)
                item.AttendanceEntryWorkhabit = dict.TryGetValue(item.IdAttendanceEntry, out var value) ? value : new List<AttendanceEntryWorkhabitResult>();


            return results;
        }
    }
}
