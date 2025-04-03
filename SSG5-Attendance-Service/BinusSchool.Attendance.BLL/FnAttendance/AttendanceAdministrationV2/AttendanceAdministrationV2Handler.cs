using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceEntry;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.Teaching;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using static BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2.AttendanceAdministrationV2Handler;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministrationV2
{
    public class AttendanceAdministrationV2Handler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceAdministrationV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new Exception();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var dataPeriode = _dbContext.Entity<MsPeriod>();
            var data = await _dbContext.Entity<TrAttendanceAdministration>()
                .Include(x => x.StudentGrade)
                        .ThenInclude(x => x.Grade)
                            .ThenInclude(x => x.Level)
                                .ThenInclude(x => x.AcademicYear)
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.HomeroomStudents)
                            .ThenInclude(x => x.Homeroom)
                                .ThenInclude(x => x.GradePathwayClassroom)
                                    .ThenInclude(x => x.Classroom)
                .Include(x => x.Attendance)
                .Include(x => x.AttdAdministrationCancel).ThenInclude(e => e.ScheduleLesson)
                .Where(x => x.Id == id)
                .Select(x => new GetAttendanceAdministrationDetailV2Result
                {
                    Student = new CodeWithIdVm
                    {
                        Id = x.StudentGrade.Student.Id,
                        Code = string.Format("{0} {1} {2}",
                            x.StudentGrade.Student.FirstName,
                            x.StudentGrade.Student.MiddleName,
                            x.StudentGrade.Student.LastName
                        )
                    },
                    AcademicYear = new CodeWithIdVm
                    {
                        Id = x.StudentGrade.Grade.Level.IdAcademicYear,
                        Code = x.StudentGrade.Grade.Level.AcademicYear.Code,
                        Description = x.StudentGrade.Grade.Level.AcademicYear.Description,
                    },
                    Level = new CodeWithIdVm
                    {
                        Id = x.StudentGrade.Grade.IdLevel,
                        Code = x.StudentGrade.Grade.Level.Code,
                        Description = x.StudentGrade.Grade.Level.Description,
                    },
                    Grade = new CodeWithIdVm
                    {
                        Id = x.StudentGrade.Grade.Id,
                        Code = x.StudentGrade.Grade.Code,
                        Description = x.StudentGrade.Grade.Description,
                    },
                    Semester = dataPeriode.Where(z => z.IdGrade == x.StudentGrade.Grade.Id && z.StartDate <= x.StartDate && z.EndDate >= x.EndDate).First().Semester,
                    ClassHomeroom = new CodeWithIdVm
                    {
                        Id = x.StudentGrade.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == x.StudentGrade.Grade.Level.IdAcademicYear).IdHomeroom,
                        Code = x.StudentGrade.Grade.Code,
                        Description = x.StudentGrade.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == x.StudentGrade.Grade.Level.IdAcademicYear && y.Homeroom.Semester == dataPeriode.Where(z => z.StartDate >= x.StartDate && z.EndDate <= z.EndDate).First().Semester).Homeroom.GradePathwayClassroom.Classroom.Description,
                    },
                    Attendance = new CodeWithIdVm
                    {
                        Id = x.Attendance.Id,
                        Code = x.Attendance.Code,
                        Description = x.Attendance.Description
                    },
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    StartPeriod = x.StartTime,
                    EndPeriod = x.EndTime,
                    AttendanceCategory = x.Attendance.AttendanceCategory,
                    AbsenceCategory = x.Attendance.AbsenceCategory,
                    AttendanceName = x.Attendance.Description,
                    ExcusedAbsenceCategory = x.Reason,
                    Status = x.StatusApproval == 1 ? "Approved" : x.StatusApproval == 2 ? "Declined" : "On Review",
                    CanApprove = x.NeedValidation == false ? false : x.NeedValidation == true && x.StatusApproval == 0 ? true : false,
                    CancelAttendances = x.AttdAdministrationCancel
                    .Select(e => new CancelAttendance
                    {
                        Date = e.ScheduleLesson.ScheduleDate,
                        StartDate = e.ScheduleLesson.StartTime,
                        EndDate = e.ScheduleLesson.EndTime,
                        SessionID = e.ScheduleLesson.SessionID
                    }).OrderBy(e => e.SessionID).ToList(),
                    Reason = x.Reason

                }).FirstOrDefaultAsync(CancellationToken);

            data.Homeroom = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.GradePathwayClassroom)
                        .ThenInclude(x => x.Classroom)
                    .ThenInclude(x => x.GradePathwayClassrooms)
                        .ThenInclude(x => x.GradePathway)
                            .ThenInclude(x => x.Grade)
                .Include(x => x.Student)
                    .ThenInclude(x => x.StudentGrades)
                .Include(x => x.Homeroom)
                    .ThenInclude(x => x.HomeroomPathways)
                        .ThenInclude(x => x.GradePathwayDetail)
                            .ThenInclude(x => x.Pathway)
                .Where(x => x.IdStudent == data.Student.Id && x.Homeroom.Grade.Id == data.Grade.Id)
                .Select(x => new StudentHomeroomAttendanceAdministrationV2
                {
                    Id = x.Id,
                    Code = $"{x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                    Description = $"{x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                    Semester = data.Semester,
                    Pathway = x.Homeroom.HomeroomPathways.FirstOrDefault().GradePathwayDetail.Pathway.Description
                })
                .FirstOrDefaultAsync();

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAttendanceAdministrationV2Request>(new string[] { nameof(GetAttendanceAdministrationRequest.IdSchool) });
            var columns = new[] { "acadyear", "level", "grade", "homeroom", "studentId", "attendanceStatus", "detailStatus", "SubmittedDate" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0]   , "TrAttendanceAdministration.MsStudentGrade.MsGrade.MsLevel.MsAcademicYear.Description" },
                { columns[1]   , "TrAttendanceAdministration.MsStudentGrade.MsGrade.MsLevel.Description" },
                { columns[2]   , "TrAttendanceAdministration.MsStudentGrade.MsGrade.Description" },
                { columns[3]   , "TrAttendanceAdministration.MsStudentGrade.MsStudent.MsHomeroomStudent.MsHomeroom.Id" },
                { columns[4]   , "TrAttendanceAdministration.MsStudentGrade.MsStudent.Id"},
                { columns[5]   , "MsAttendanceMappingAttendance.MsAttendance.Description"},
                { columns[6]   , "Reason"},
                { columns[7]   , "DateIn"}
            };
            var predicate = PredicateBuilder.Create<TrAttendanceAdministration>(x => 1 == 1);
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.StudentGrade.Grade.Level.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.StudentGrade.Grade.Level.Id == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.StudentGrade.Grade.Id == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.StudentGrade.Student.HomeroomStudents.Any(e => e.IdHomeroom == param.IdHomeroom));

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                EF.Functions.Like(x.StudentGrade.Student.Id, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.FirstName, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.LastName, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.MiddleName, param.SearchPattern())
                );

            var dataPeriode = _dbContext.Entity<MsPeriod>();
            var query = _dbContext.Entity<TrAttendanceAdministration>()
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.HomeroomStudents)
                .Include(x => x.Attendance)
               .Where(predicate)
               .Where(x => param.IdSchool.Contains(x.StudentGrade.Grade.Level.AcademicYear.IdSchool));
            //.OrderByDynamic(param, aliasColumns);
            query = param.OrderBy switch
            {
                "acadyear" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length).ThenBy(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length)
                        : query.OrderByDescending(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length).ThenByDescending(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length),
                "level" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.Grade.Level.Description.Length).ThenBy(x => x.StudentGrade.Grade.Level.Description.Length)
                        : query.OrderByDescending(x => x.StudentGrade.Grade.Level.Description.Length).ThenByDescending(x => x.StudentGrade.Grade.Level.Description.Length),
                "grade" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.Grade.Description.Length).ThenBy(x => x.StudentGrade.Grade.Description.Length)
                        : query.OrderByDescending(x => x.StudentGrade.Grade.Description.Length).ThenByDescending(x => x.StudentGrade.Grade.Description.Length),
                "homeroom" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.Student.HomeroomStudents.FirstOrDefault().Homeroom.GradePathwayClassroom.Classroom.Description.Length).ThenBy(x => x.StudentGrade.Student.HomeroomStudents.FirstOrDefault().Homeroom.GradePathwayClassroom.Classroom.Description.Length)
                        : query.OrderByDescending(x => x.StudentGrade.Student.HomeroomStudents.FirstOrDefault().Homeroom.GradePathwayClassroom.Classroom.Description.Length).ThenByDescending(x => x.StudentGrade.Student.HomeroomStudents.FirstOrDefault().Homeroom.GradePathwayClassroom.Classroom.Description.Length),
                "studentId" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.IdStudent.Length).ThenBy(x => x.StudentGrade.IdStudent)
                        : query.OrderByDescending(x => x.StudentGrade.IdStudent.Length).ThenByDescending(x => x.StudentGrade.IdStudent),
                "attendanceStatus" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Attendance.Description)
                        : query.OrderByDescending(x => x.Attendance.Description),
                "detailStatus" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.Reason)
                        : query.OrderByDescending(x => x.Reason),
                "SubmittedDate" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.DateIn)
                        : query.OrderByDescending(x => x.DateIn),
                _ => query.OrderByDynamic(param, aliasColumns)
            };
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.StudentGrade.Student.FirstName))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetAttendanceAdministrationV2Result
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.Level.IdAcademicYear,
                            Code = x.StudentGrade.Grade.Level.AcademicYear.Code,
                            Description = x.StudentGrade.Grade.Level.AcademicYear.Description,
                        },
                        Level = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.IdLevel,
                            Code = x.StudentGrade.Grade.Level.Code,
                            Description = x.StudentGrade.Grade.Level.Description,
                        },
                        Grade = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.Id,
                            Code = x.StudentGrade.Grade.Code,
                            Description = x.StudentGrade.Grade.Description,
                        },
                        Semester = dataPeriode.Where(z => z.StartDate >= x.StartDate && z.EndDate <= z.EndDate).First().Semester,
                        Homeroom = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == x.StudentGrade.Grade.Level.IdAcademicYear).IdHomeroom,
                            Code = x.StudentGrade.Grade.Code,
                            Description = $"{x.StudentGrade.Grade.Code}{x.StudentGrade.Student.HomeroomStudents.First(y => y.Homeroom.IdAcademicYear == x.StudentGrade.Grade.Level.IdAcademicYear).Homeroom.GradePathwayClassroom.Classroom.Description}",
                        },
                        Student = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Student.Id,
                            Code = string.Format("{0} {1} {2}",
                                x.StudentGrade.Student.FirstName,
                                x.StudentGrade.Student.MiddleName,
                                x.StudentGrade.Student.LastName
                            )
                        },
                        Attendance = new CodeWithIdVm
                        {
                            Id = x.Attendance.Id,
                            Code = x.Attendance.Code,
                            Description = x.Attendance.Description
                        },
                        Detail = x.Reason,
                        SubmittedDate = x.DateIn
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : query.Select(x => x.Id).Count();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<PostAttendanceAdministrationRequest, PostAttendanceAdministratorValidator>();
            List<TrAttendanceAdministration> trAttendanceAdministrations = new List<TrAttendanceAdministration>();
            List<TrAttendanceEntryV2> trAttendanceEntries = new List<TrAttendanceEntryV2>();

            var idStudents = body.Students.Select(x => x.IdStudent).Distinct();
            var idGrades = body.Students.Select(x => x.IdGrade).Distinct();
            var gradeStudents = await _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                    .Where(x => idStudents.Contains(x.IdStudent))
                    .Where(x => idGrades.Contains(x.IdGrade))
                    .Select(x => new
                    {
                        x.Id,
                        x.Grade.IdLevel,
                        Level = x.Grade.Level.Description,
                        IdStudent = x.Student.Id,
                        IdAcademicYear = x.Grade.Level.IdAcademicYear
                    })
                    .ToListAsync();

            var idAcademicYear = gradeStudents.Select(e => e.IdAcademicYear).FirstOrDefault();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                    .Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear)
                    .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                    .Include(e => e.Lesson)
                                                    .Where(e => idStudents.Contains(e.HomeroomStudent.IdStudent)
                                                                && e.HomeroomStudent.Homeroom.IdAcademicYear == idAcademicYear)
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
                                                                && x.LessonOld.IdAcademicYear == idAcademicYear)
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

            var schoolStudent = _dbContext.Entity<MsSchool>().Where(x => x.Id == body.IdSchool).FirstOrDefault();
            List<GetUserPosition> listUserPosition = new List<GetUserPosition>();
            List<string> listIdLevel = new List<string>();
            foreach (var studentRequest in body.Students)
            {
                var gradeStudent = gradeStudents.Find(x => x.IdStudent == studentRequest.IdStudent);
                if (gradeStudent == null)
                    throw new BadRequestException($"Student not yet mapping to any grade in current academic year");

                listIdLevel.Add(gradeStudent.IdLevel);

                var listIdLessonByStudent = listStudentEnrollmentUnion.Where(e => e.IdStudent == studentRequest.IdStudent).Select(e => e.IdLesson).Distinct().ToList();

                var scheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                        .Where(e => e.ScheduleDate.Date >= studentRequest.StartDate.Date
                                && e.ScheduleDate.Date <= studentRequest.EndDate.Date
                                && ((studentRequest.StartPeriod >= e.StartTime && studentRequest.StartPeriod <= e.EndTime)
                                     || (studentRequest.EndPeriod > e.StartTime && studentRequest.EndPeriod <= e.EndTime)
                                     || (studentRequest.StartPeriod <= e.StartTime && studentRequest.EndPeriod >= e.EndTime))
                                && e.IdGrade == studentRequest.IdGrade
                                && listIdLessonByStudent.Contains(e.IdLesson)
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
                        })
                        .ToListAsync(CancellationToken);

                if (scheduleLesson.Count == 0)
                    throw new BadRequestException($"Session for student not found");

                var idHomeroom = listStudentEnrollmentUnion.Select(e => e.Homeroom.Id).FirstOrDefault();

                var homeRoomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                    .Include(e => e.TeacherPosition).ThenInclude(e => e.LtPosition)
                    .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                    .Where(x => x.IdHomeroom == idHomeroom)
                    .ToList();

                if (homeRoomTeacher.Any())
                {
                    var listHomeroomTeacherByCa = homeRoomTeacher
                        .Where(e => e.TeacherPosition.LtPosition.Code == PositionConstant.ClassAdvisor)
                        .Select(e => new GetUserPosition
                        {
                            IdUser = e.IdBinusian,
                            PositionCode = PositionConstant.ClassAdvisor,
                            IdLevel = e.Homeroom.Grade.IdLevel
                        }).ToList();

                    listUserPosition.AddRange(listHomeroomTeacherByCa);
                }

                var _totalSessionWillUsed = scheduleLesson.Count();

                TrAttendanceAdministration trAttendance = new TrAttendanceAdministration
                {
                    Id = Guid.NewGuid().ToString(),
                    IdStudentGrade = gradeStudent.Id,
                    IdAttendance = studentRequest.IdAttendance,
                    AbsencesFile = studentRequest.AbsencesFile,
                    IncludeElective = studentRequest.IncludeElective,
                    NeedValidation = studentRequest.NeedValidation,
                    Reason = studentRequest.Reason,
                    StartDate = studentRequest.StartDate,
                    EndDate = studentRequest.EndDate,
                    StartTime = studentRequest.StartPeriod,
                    EndTime = studentRequest.EndPeriod,
                    SessionUsed = _totalSessionWillUsed
                };
                trAttendanceAdministrations.Add(trAttendance);

                if (!studentRequest.NeedValidation)
                {
                    #region Find all lesson by student , date and periode
                    var queryScheduleLessons = _dbContext.Entity<MsScheduleLesson>()
                        .Include(e => e.Lesson)
                        .Where(x => x.ScheduleDate.Date >= studentRequest.StartDate.Date)
                        .Where(x => x.ScheduleDate.Date <= studentRequest.EndDate.Date)
                        .Where(x => x.IdGrade == studentRequest.IdGrade)
                        .Where(x => listIdLessonByStudent.ToList().Contains(x.IdLesson))
                        .AsQueryable();
                    if (!body.IsAllDay)
                    {
                        queryScheduleLessons = queryScheduleLessons.Where(x => (studentRequest.StartPeriod >= x.StartTime && studentRequest.StartPeriod <= x.EndTime)
                                                                     || (studentRequest.EndPeriod > x.StartTime && studentRequest.EndPeriod <= x.EndTime)
                                                                     || (studentRequest.StartPeriod <= x.StartTime && studentRequest.EndPeriod >= x.EndTime));
                    }
                    var scheduleLessonData = await queryScheduleLessons.Select(x => new
                    {
                        x.Id,
                        x.IdLesson,
                        x.IdDay,
                        x.IdSession,
                        x.IdWeek,
                        x.ScheduleDate,
                        x.Lesson.Semester
                    }).ToListAsync();
                    var _scheduleLesson = scheduleLessonData.Select(x => x.Id).ToList();
                    #endregion

                    #region Find IdMappingAttendance For Each Student, case each student different level
                    MsAttendanceMappingAttendance mappingAttendance = new MsAttendanceMappingAttendance();
                    if (studentRequest.IdAttendance == "2")
                    {
                        mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                       .Include(x => x.MappingAttendance)
                           .ThenInclude(x => x.Level)
                       .Include(x => x.Attendance)
                       .Where(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
                       .Where(x => x.MappingAttendance.IdLevel == gradeStudent.IdLevel)
                       .FirstOrDefaultAsync(CancellationToken);
                    }
                    else
                    {
                        mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                        .Include(x => x.MappingAttendance)
                            .ThenInclude(x => x.Level)
                        .Include(x => x.Attendance)
                        .Where(x => x.IdAttendance == studentRequest.IdAttendance)
                        .Where(x => x.MappingAttendance.IdLevel == gradeStudent.IdLevel)
                        .FirstOrDefaultAsync(CancellationToken);
                    }


                    if (mappingAttendance is null)
                    {
                        var selectedMappingAttendance = await _dbContext.Entity<MsAttendance>()
                            .Where(x => x.Id == studentRequest.IdAttendance)
                            .Select(x => x.Description)
                            .FirstOrDefaultAsync(CancellationToken);

                        throw new BadRequestException($"Attendance name {selectedMappingAttendance} is not exist in level {gradeStudent.Level}.");
                    }
                    #endregion

                    var dataSchedule = await _dbContext.Entity<MsSchedule>()
                                  .Where(e => listIdLessonByStudent.Contains(e.IdLesson))
                                  .ToListAsync(CancellationToken);

                    var staff = await _dbContext.Entity<MsStaff>()
                                      .ToListAsync(CancellationToken);

                    var listStudentEnrollmentUnionByStudent = listStudentEnrollmentUnion.Where(e => e.IdStudent == gradeStudent.IdStudent).ToList();

                    var attendanceEntryByGeneratedScheduleLesson = await _dbContext.Entity<TrAttendanceEntryV2>()
                        .Include(x => x.HomeroomStudent)
                        .Where(x => _scheduleLesson.Any(y => y == x.IdScheduleLesson) && x.HomeroomStudent.IdStudent == studentRequest.IdStudent).ToListAsync();
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

                            if (attendanceEntry != null)
                            {
                                attendanceEntry.IsActive = false;
                                _dbContext.Entity<TrAttendanceEntryV2>().Update(attendanceEntry);
                            }

                            TrAttendanceEntryV2 trAttendanceEntry = new TrAttendanceEntryV2
                            {
                                IdAttendanceEntry = Guid.NewGuid().ToString(),
                                IdScheduleLesson = newEntrySchedule.Id,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = studentRequest.AbsencesFile,
                                Notes = studentRequest.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true,
                                IdBinusian = idTeacher,
                                IdHomeroomStudent = getStudentEnrollmentMoving.Select(x => x.IdHomeroomStudent).FirstOrDefault(),
                                PositionIn = "Admin"
                            };
                            trAttendanceEntries.Add(trAttendanceEntry);
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
                                FileEvidence = studentRequest.AbsencesFile,
                                Notes = studentRequest.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true,
                                IdBinusian = idTeacher,
                                IdHomeroomStudent = getStudentEnrollmentMoving.Select(x => x.IdHomeroomStudent).FirstOrDefault(),
                                PositionIn = "Admin",
                            };
                            trAttendanceEntries.Add(trAttendanceEntry);
                        }
                    }
                }
                else
                {
                    var dataStudent = await _dbContext.Entity<MsStudent>().FirstOrDefaultAsync(x => x.Id == studentRequest.IdStudent);
                    var dataAttendance = await _dbContext.Entity<MsAttendance>().FirstOrDefaultAsync(x => x.Id == studentRequest.IdAttendance);
                }
            }

            if (trAttendanceAdministrations.Count > 0)
                _dbContext.Entity<TrAttendanceAdministration>().AddRange(trAttendanceAdministrations);
            if (trAttendanceEntries.Count > 0)
                _dbContext.Entity<TrAttendanceEntryV2>().AddRange(trAttendanceEntries);

            await _dbContext.SaveChangesAsync(CancellationToken);

            #region Send Email
            var listIdAttendanceAdmin = trAttendanceAdministrations.Select(e => e.Id).ToList();

            #region ATD11
            if (KeyValues.ContainsKey("listIdAttendanceAdmin"))
                KeyValues.Remove("listIdAttendanceAdmin");

            KeyValues.Add("listIdAttendanceAdmin", listIdAttendanceAdmin);
            var NotificationATD11 = ATDNotification(KeyValues, AuthInfo, "ATD11");
            #endregion

            #region ATD12V2Notification
            if (KeyValues.ContainsKey("listIdAttendanceAdmin"))
                KeyValues.Remove("listIdAttendanceAdmin");

            KeyValues.Add("listIdAttendanceAdmin", listIdAttendanceAdmin);
            var NotificationATD12 = ATDNotification(KeyValues, AuthInfo, "ATD12");
            #endregion
            #endregion


            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<PutAttendanceAdministrationV2Request, PutAttendanceAdministratorValidator>();
            List<TrAttendanceAdministration> trAttendanceAdministrations = new List<TrAttendanceAdministration>();
            List<TrAttendanceEntryV2> trAttendanceEntries = new List<TrAttendanceEntryV2>();

            var dataAttendanceAdministration = await _dbContext.Entity<TrAttendanceAdministration>()
                                              .Include(x => x.StudentGrade)
                                              .Where(x => x.Id == body.IdAttendanceAdministration).FirstOrDefaultAsync(CancellationToken);

            var idStudents = dataAttendanceAdministration.StudentGrade.IdStudent;
            var idGrades = dataAttendanceAdministration.StudentGrade.IdGrade;
            var gradeStudents = await _dbContext.Entity<MsStudentGrade>()
                    .Include(x => x.Grade)
                        .ThenInclude(x => x.Level)
                    .Where(x => idStudents.Contains(x.IdStudent))
                    .Where(x => idGrades.Contains(x.IdGrade))
                    .Select(x => new
                    {
                        x.Id,
                        x.Grade.IdLevel,
                        Level = x.Grade.Level.Description,
                        IdStudent = x.Student.Id,
                        IdAcademicYear = x.Grade.Level.IdAcademicYear
                    })
                    .ToListAsync();

            var gradeStudent = gradeStudents.Find(x => x.IdStudent == idStudents);

            if (gradeStudent == null)
                throw new BadRequestException($"Student not yet mapping to any grade in current academic year");

            var listPeriod = await _dbContext.Entity<MsPeriod>()
            .Where(e => idGrades.Contains(e.IdGrade) && (e.StartDate.Date <= dataAttendanceAdministration.StartDate.Date && e.EndDate.Date >= dataAttendanceAdministration.EndDate.Date))
            .Select(e => e.Semester)
            .Distinct()
            .ToListAsync(CancellationToken);

            var homeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
            .Include(x => x.Homeroom)
            .Where(x => x.IdStudent == gradeStudent.IdStudent)
            .Where(x => x.Homeroom.IdAcademicYear == gradeStudent.IdAcademicYear && listPeriod.Contains(x.Homeroom.Semester))
            .Select(x => new { x.IdHomeroom, x.Id })
            .ToListAsync(CancellationToken);

            var listStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                            .Where(e => homeroomStudent.Select(x => x.IdHomeroom).Contains(e.HomeroomStudent.IdHomeroom)
                                        && e.HomeroomStudent.IdStudent == idStudents
                                        && e.HomeroomStudent.Homeroom.IdGrade == idGrades
                                        )
                            .GroupBy(e => new
                            {
                                e.IdHomeroomStudent,
                                e.HomeroomStudent.Homeroom.Grade.IdLevel,
                                e.IdLesson
                            })
                            .Select(e => new
                            {
                                e.Key.IdHomeroomStudent,
                                e.Key.IdLevel,
                                e.Key.IdLesson
                            })
                            .ToListAsync(CancellationToken);

            var scheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                    .Where(e => e.ScheduleDate.Date >= dataAttendanceAdministration.StartDate.Date
                            && e.ScheduleDate.Date <= dataAttendanceAdministration.EndDate.Date
                            && e.StartTime >= dataAttendanceAdministration.StartTime
                            && e.StartTime <= dataAttendanceAdministration.EndTime
                            && e.IdGrade == idGrades
                            && listStudentEnrollment.Select(x => x.IdLesson).Contains(e.IdLesson)
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
                        e.SessionID
                    })
                    .ToListAsync(CancellationToken);

            listStudentEnrollment = listStudentEnrollment.Where(x => scheduleLesson.Select(y => y.IdLesson).Contains(x.IdLesson)).ToList();

            if (scheduleLesson.Count == 0)
                throw new BadRequestException($"Session for student not found");

            var _totalSessionWillUsed = scheduleLesson.Count();

            if (!dataAttendanceAdministration.NeedValidation)
            {
                #region Find all lesson by student , date and periode
                var scheduleLessons = _dbContext.Entity<MsScheduleLesson>()
                    .Where(x => x.ScheduleDate.Date >= dataAttendanceAdministration.StartDate.Date)
                    .Where(x => x.ScheduleDate.Date <= dataAttendanceAdministration.EndDate.Date)
                    .Where(x => x.IdGrade == idGrades)
                    .Where(x => listStudentEnrollment.Select(x => x.IdLesson).ToList().Contains(x.IdLesson))
                    .Where(x => (dataAttendanceAdministration.StartTime >= x.StartTime && dataAttendanceAdministration.StartTime <= x.EndTime)
                                                                 || (dataAttendanceAdministration.EndTime > x.StartTime && dataAttendanceAdministration.EndTime <= x.EndTime)
                                                                 || (dataAttendanceAdministration.StartTime <= x.StartTime && dataAttendanceAdministration.EndTime >= x.EndTime))
                    .AsQueryable();
                var scheduleLessonData = await scheduleLessons.Select(x => new
                {
                    x.Id,
                    x.IdLesson,
                    x.IdDay,
                    x.IdSession,
                    x.IdWeek
                }).ToListAsync();
                var _scheduleLesson = scheduleLessonData.Select(x => x.Id).ToList();
                #endregion

                #region Find IdMappingAttendance For Each Student, case each student different level
                MsAttendanceMappingAttendance mappingAttendance = new MsAttendanceMappingAttendance();
                if (dataAttendanceAdministration.IdAttendance == "2")
                {
                    mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                   .Include(x => x.MappingAttendance)
                       .ThenInclude(x => x.Level)
                   .Include(x => x.Attendance)
                   .Where(x => x.Attendance.AbsenceCategory.Value == AbsenceCategory.Unexcused)
                   .Where(x => x.MappingAttendance.IdLevel == gradeStudent.IdLevel)
                   .FirstOrDefaultAsync(CancellationToken);
                }
                else
                {
                    mappingAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                    .Include(x => x.MappingAttendance)
                        .ThenInclude(x => x.Level)
                    .Include(x => x.Attendance)
                    .Where(x => x.IdAttendance == dataAttendanceAdministration.IdAttendance)
                    .Where(x => x.MappingAttendance.IdLevel == gradeStudent.IdLevel)
                    .FirstOrDefaultAsync(CancellationToken);
                }


                if (mappingAttendance is null)
                {
                    var selectedMappingAttendance = await _dbContext.Entity<MsAttendance>()
                        .Where(x => x.Id == dataAttendanceAdministration.IdAttendance)
                        .Select(x => x.Description)
                        .FirstOrDefaultAsync(CancellationToken);

                    throw new BadRequestException($"Attendance name {selectedMappingAttendance} is not exist in level {gradeStudent.Level}.");
                }
                #endregion

                var dataSchedule = await _dbContext.Entity<MsSchedule>()
                                  .Where(e => scheduleLessonData.Select(x => x.IdLesson).Contains(e.IdLesson))
                                  .ToListAsync(CancellationToken);

                var staff = await _dbContext.Entity<MsStaff>()
                                  .ToListAsync(CancellationToken);

                var attendanceEntryByGeneratedScheduleLesson = await _dbContext.Entity<TrAttendanceEntryV2>()
                    .Include(x => x.HomeroomStudent)
                    .Where(x => _scheduleLesson.Any(y => y == x.IdScheduleLesson) && x.HomeroomStudent.IdStudent == idStudents).ToListAsync();
                List<TrAttendanceEntryV2> attendanceEntries = new List<TrAttendanceEntryV2>();
                // update any existing entries
                if (attendanceEntryByGeneratedScheduleLesson.Any())
                {
                    foreach (var newEntryScheduleId in _scheduleLesson.Where(scheduleId => !attendanceEntryByGeneratedScheduleLesson.Any(y => y.IdScheduleLesson == scheduleId)))
                    {
                        var schedulelesson = scheduleLessonData.Where(x => x.Id == newEntryScheduleId).FirstOrDefault();
                        var idTeacher = dataSchedule
                          .Where(e => e.IdLesson == schedulelesson.IdLesson
                                   && e.IdDay == schedulelesson.IdDay
                                   && e.IdSession == schedulelesson.IdSession
                                   && e.IdWeek == schedulelesson.IdWeek)
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
                            IdScheduleLesson = newEntryScheduleId,
                            IdAttendanceMappingAttendance = mappingAttendance.Id,
                            FileEvidence = dataAttendanceAdministration.AbsencesFile,
                            Notes = dataAttendanceAdministration.Reason,
                            Status = AttendanceEntryStatus.Submitted,
                            IsFromAttendanceAdministration = true,
                            IdBinusian = idTeacher,
                            IdHomeroomStudent = listStudentEnrollment.Select(x => x.IdHomeroomStudent).FirstOrDefault(),
                            UserIn = dataAttendanceAdministration.UserIn,
                            PositionIn = "Admin"
                        };
                        trAttendanceEntries.Add(trAttendanceEntry);
                    }
                    foreach (var item in attendanceEntryByGeneratedScheduleLesson)
                    {
                        item.IdAttendanceMappingAttendance = mappingAttendance.Id;
                        item.FileEvidence = dataAttendanceAdministration.AbsencesFile;
                        item.Notes = dataAttendanceAdministration.Reason;
                        item.Status = AttendanceEntryStatus.Submitted;
                        item.IsFromAttendanceAdministration = true;
                        item.UserUp = dataAttendanceAdministration.UserIn;
                        item.PositionIn = "Admin";
                    }
                    _dbContext.Entity<TrAttendanceEntryV2>().UpdateRange(attendanceEntryByGeneratedScheduleLesson);
                }
                else
                {
                    foreach (var newEntryScheduleId in _scheduleLesson)
                    {
                        var schedulelesson = scheduleLessonData.Where(x => x.Id == newEntryScheduleId).FirstOrDefault();
                        var idTeacher = dataSchedule
                          .Where(e => e.IdLesson == schedulelesson.IdLesson
                                   && e.IdDay == schedulelesson.IdDay
                                   && e.IdSession == schedulelesson.IdSession
                                   && e.IdWeek == schedulelesson.IdWeek)
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
                            IdScheduleLesson = newEntryScheduleId,
                            IdAttendanceMappingAttendance = mappingAttendance.Id,
                            FileEvidence = dataAttendanceAdministration.AbsencesFile,
                            Notes = dataAttendanceAdministration.Reason,
                            Status = AttendanceEntryStatus.Submitted,
                            IsFromAttendanceAdministration = true,
                            IdBinusian = idTeacher,
                            IdHomeroomStudent = listStudentEnrollment.Select(x => x.IdHomeroomStudent).FirstOrDefault(),
                            UserIn = dataAttendanceAdministration.UserIn,
                            PositionIn = "Admin"
                        };
                        trAttendanceEntries.Add(trAttendanceEntry);
                    }
                }
            }

            if (trAttendanceAdministrations.Count > 0)
                _dbContext.Entity<TrAttendanceAdministration>().AddRange(trAttendanceAdministrations);
            if (trAttendanceEntries.Count > 0)
                _dbContext.Entity<TrAttendanceEntryV2>().AddRange(trAttendanceEntries);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        public static string ATDNotification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdScenario)
        {
            // send notification
            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, IdScenario)
                {
                    IdRecipients = null,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }

        public class GetUserPosition
        {
            public string IdUser { get; set; }
            public string PositionCode { get; set; }
            public string IdLevel { get; set; }
        }
    }
}

