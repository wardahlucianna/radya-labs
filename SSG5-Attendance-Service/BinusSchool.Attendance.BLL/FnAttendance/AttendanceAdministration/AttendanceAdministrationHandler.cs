using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceAdministration.validator;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration
{
    public class AttendanceAdministrationHandler : FunctionsHttpCrudHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public AttendanceAdministrationHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
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
                .Where(x => x.Id == id)
                .Select(x => new GetAttendanceAdministrationDetailResult
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
                    Semester = dataPeriode.Where(z => z.StartDate >= x.StartDate && z.EndDate <= z.EndDate).First().Semester,
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
                    CanApprove = x.NeedValidation == false ? false : x.NeedValidation == true && x.StatusApproval == 0 ? true : false

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
                .Select(x => new StudentHomeroomAttendanceAdministration
                {
                    Id = x.Id,
                    Code = $"{x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                    Description = $"{x.Homeroom.GradePathwayClassroom.GradePathway.Grade.Code}{x.Homeroom.GradePathwayClassroom.Classroom.Code}",
                    Semester = x.Homeroom.Semester,
                    Pathway = x.Homeroom.HomeroomPathways.FirstOrDefault().GradePathwayDetail.Pathway.Description
                })
                .FirstOrDefaultAsync();

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetAttendanceAdministrationRequest>(new string[] { nameof(GetAttendanceAdministrationRequest.IdSchool) });
            var columns = new[] { "acadyear", "studentId", "attendanceStatus", "detailStatus", "SubmittedDate" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0]   , "TrGeneratedScheduleLesson.MsHomeroom.GradePathwayClassroom.MsGradePathway.MsGrade.MsLevel.MsAcademicYear.Description" },
                { columns[1]   , "TrGeneratedScheduleLesson.TrGeneratedScheduleStudent.MsStudent.Id"},
                { columns[2]   , "MsAttendanceMappingAttendance.MsAttendance.Description"},
                { columns[3]   , "Reason"},
                { columns[4]   , "DateIn"}
            };
            var predicate = PredicateBuilder.Create<TrAttendanceAdministration>(x => 1 == 1);
            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.StudentGrade.Grade.Level.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x =>
                EF.Functions.Like(x.StudentGrade.Student.Id, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.FirstName, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.LastName, param.SearchPattern())
                || EF.Functions.Like(x.StudentGrade.Student.MiddleName, param.SearchPattern())
                );



            var query = _dbContext.Entity<TrAttendanceAdministration>()
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                            .ThenInclude(x => x.AcademicYear)
                .Include(x => x.StudentGrade)
                    .ThenInclude(x => x.Student)
                .Include(x => x.Attendance)
               .Where(predicate)
               .Where(x => param.IdSchool.Contains(x.StudentGrade.Grade.Level.AcademicYear.IdSchool));
            //.OrderByDynamic(param, aliasColumns);
            query = param.OrderBy switch
            {
                "acadyear" => param.OrderType == OrderType.Asc
                        ? query.OrderBy(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length).ThenBy(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length)
                        : query.OrderByDescending(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length).ThenByDescending(x => x.StudentGrade.Grade.Level.AcademicYear.Description.Length),
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
                    .Select(x => new GetAttendanceAdministrationResult
                    {
                        Id = x.Id,
                        AcademicYear = new CodeWithIdVm
                        {
                            Id = x.StudentGrade.Grade.Level.IdAcademicYear,
                            Code = x.StudentGrade.Grade.Level.AcademicYear.Code,
                            Description = x.StudentGrade.Grade.Level.AcademicYear.Description,
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
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<PostAttendanceAdministrationRequest, PostAttendanceAdministratorValidator>();
            List<TrAttendanceAdministration> trAttendanceAdministrations = new List<TrAttendanceAdministration>();
            List<TrAttendanceEntry> trAttendanceEntries = new List<TrAttendanceEntry>();
            List<ATD11NotificationModel> aTD11NotificationModels = new List<ATD11NotificationModel>();
            List<ATD12NotificationModel> aTD12NotificationModels = new List<ATD12NotificationModel>();
            var dataSessionUsed2 = new EmailATD12Result();

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
            foreach (var studentRequest in body.Students)
            {
                var gradeStudent = gradeStudents.Find(x => x.IdStudent == studentRequest.IdStudent);
                if (gradeStudent == null)
                    throw new BadRequestException($"Student not yet mapping to any grade in current academic year");
                var homeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                .Where(x => x.IdStudent == gradeStudent.IdStudent)
                .Where(x => x.Homeroom.IdAcademicYear == gradeStudent.IdAcademicYear)
                .Select(x => x.IdHomeroom)
                .ToListAsync(CancellationToken);
                var totalSessionStudent = await _dbContext.Entity<TrGeneratedScheduleLesson>()
                .Include(x => x.GeneratedScheduleStudent)
                    .ThenInclude(x => x.Student)
                        .ThenInclude(x => x.StudentGrades)
                            .ThenInclude(x => x.Grade)
                                .ThenInclude(x => x.Level)
                .OrderBy(x => x.ScheduleDate)
                .Where(x => x.GeneratedScheduleStudent.IdStudent == gradeStudent.IdStudent)
                .Where(x => homeroomStudent.Contains(x.IdHomeroom))
                .ToListAsync();
                if (totalSessionStudent.Count == 0)
                    throw new Exception($"Session for student not found");
                var totalSessionWillUsed = totalSessionStudent
                .Where(x => x.ScheduleDate.Date >= studentRequest.StartDate.Date)
                .Where(x => x.ScheduleDate.Date <= studentRequest.EndDate.Date);
                if (!body.IsAllDay)
                {
                    totalSessionWillUsed = totalSessionWillUsed
                    //.Where(x => x.StartTime >= body.StartPeriod)
                    //.Where(x => body.EndPeriod <= x.EndTime);
                    .Where(x => (studentRequest.StartPeriod >= x.StartTime && studentRequest.StartPeriod <= x.EndTime)
                                || (studentRequest.EndPeriod > x.StartTime && studentRequest.EndPeriod <= x.EndTime)
                                || (studentRequest.StartPeriod <= x.StartTime && studentRequest.EndPeriod >= x.EndTime));
                }
                var _totalSessionWillUsed = totalSessionWillUsed.Count();
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
                    var scheduleLessons = _dbContext.Entity<TrGeneratedScheduleLesson>()
                        .Include(x => x.GeneratedScheduleStudent)
                        .Where(x => x.GeneratedScheduleStudent.IdStudent == studentRequest.IdStudent)
                        .Where(x => x.ScheduleDate.Date >= studentRequest.StartDate.Date)
                        .Where(x => x.ScheduleDate.Date <= studentRequest.EndDate.Date)
                        .AsQueryable();
                    if (!body.IsAllDay)
                    {
                        //scheduleLessons = scheduleLessons.Where(x => x.StartTime >= studentRequest.StartPeriod)
                        //                                 .Where(x => studentRequest.EndPeriod <= x.EndTime);
                        scheduleLessons = scheduleLessons.Where(x => (studentRequest.StartPeriod >= x.StartTime && studentRequest.StartPeriod <= x.EndTime)
                                                                     || (studentRequest.EndPeriod > x.StartTime && studentRequest.EndPeriod <= x.EndTime)
                                                                     || (studentRequest.StartPeriod <= x.StartTime && studentRequest.EndPeriod >= x.EndTime));
                    }
                    var _scheduleLesson = await scheduleLessons.Select(x => x.Id).ToListAsync();
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


                    var attendanceEntryByGeneratedScheduleLesson = await _dbContext.Entity<TrAttendanceEntry>()
                        .Where(x => _scheduleLesson.Any(y => y == x.IdGeneratedScheduleLesson)).ToListAsync();
                    List<TrAttendanceEntry> attendanceEntries = new List<TrAttendanceEntry>();
                    // update any existing entries
                    if (attendanceEntryByGeneratedScheduleLesson.Any())
                    {
                        // foreach (var attendanceEntry in attendanceEntryByGeneratedScheduleLesson)
                        // {
                        //     attendanceEntry.IsActive = false;
                        //     TrAttendanceEntry trAttendanceEntry = new TrAttendanceEntry
                        //     {
                        //         Id = Guid.NewGuid().ToString(),
                        //         IdGeneratedScheduleLesson = attendanceEntry.IdGeneratedScheduleLesson,
                        //         IdAttendanceMappingAttendance = mappingAttendance.Id,
                        //         FileEvidence = studentRequest.AbsencesFile,
                        //         Notes = studentRequest.Reason,
                        //         Status = AttendanceEntryStatus.Submitted,
                        //         IsFromAttendanceAdministration = true
                        //     };
                        //     attendanceEntries.Add(trAttendanceEntry);
                        // }


                        attendanceEntryByGeneratedScheduleLesson
                            .ForEach(e => 
                            {
                                e.IdAttendanceMappingAttendance = mappingAttendance.Id;
                                e.PositionIn = "ADMIN";
                                e.Status = AttendanceEntryStatus.Submitted;
                            });

                        // var addedScheduleLesson = attendanceEntryByGeneratedScheduleLesson.Select(x => x.IdGeneratedScheduleLesson).ToList();
                        foreach (var newEntryScheduleId in _scheduleLesson.Where(scheduleId => !attendanceEntryByGeneratedScheduleLesson.Any(y => y.IdGeneratedScheduleLesson == scheduleId)))
                        {
                            TrAttendanceEntry trAttendanceEntry = new TrAttendanceEntry
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdGeneratedScheduleLesson = newEntryScheduleId,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = studentRequest.AbsencesFile,
                                Notes = studentRequest.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true,
                                PositionIn = "ADMIN"
                            };
                            trAttendanceEntries.Add(trAttendanceEntry);
                        }
                        


                        _dbContext.Entity<TrAttendanceEntry>().UpdateRange(attendanceEntryByGeneratedScheduleLesson);
                    }
                    else
                    {
                        foreach (var newEntryScheduleId in _scheduleLesson)
                        {
                            TrAttendanceEntry trAttendanceEntry = new TrAttendanceEntry
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdGeneratedScheduleLesson = newEntryScheduleId,
                                IdAttendanceMappingAttendance = mappingAttendance.Id,
                                FileEvidence = studentRequest.AbsencesFile,
                                Notes = studentRequest.Reason,
                                Status = AttendanceEntryStatus.Submitted,
                                IsFromAttendanceAdministration = true,
                                PositionIn = "ADMIN"
                            };
                            trAttendanceEntries.Add(trAttendanceEntry);
                        }
                        // trAttendanceEntries = _scheduleLesson.Select(scheduleId => new TrAttendanceEntry
                        // {
                        //     Id = Guid.NewGuid().ToString(),
                        //     IdGeneratedScheduleLesson = scheduleId,
                        //     IdAttendanceMappingAttendance = mappingAttendance.Id,
                        //     FileEvidence = studentRequest.AbsencesFile,
                        //     Notes = studentRequest.Reason,
                        //     Status = AttendanceEntryStatus.Submitted,
                        //     IsFromAttendanceAdministration = true
                        // }).ToList();
                    }
                }
                else
                {
                    var dataStudent = await _dbContext.Entity<MsStudent>().FirstOrDefaultAsync(x => x.Id == studentRequest.IdStudent);
                    var dataAttendance = await _dbContext.Entity<MsAttendance>().FirstOrDefaultAsync(x => x.Id == studentRequest.IdAttendance);
                    aTD11NotificationModels.Add(new ATD11NotificationModel() { 
                        BinussianId = dataStudent.Id,
                        StudentName = string.Join(" ", (new string[] {
                            dataStudent.FirstName,
                            dataStudent.MiddleName,
                            dataStudent.LastName }).Where(s => !string.IsNullOrWhiteSpace(s))),
                        StudentGrade = gradeStudent.Level,
                        StartDate = studentRequest.StartDate,
                        EndDate = studentRequest.EndDate,
                        StartSession = totalSessionWillUsed.OrderBy(x => x.StartTime).First().SessionID,
                        EndSession = totalSessionWillUsed.OrderBy(x => x.StartTime).Last().SessionID,
                        Category = dataAttendance.AbsenceCategory.Value.GetDescription(),
                        Reason = studentRequest.Reason
                    });
                }

                // Keperluan model untuk notification ATD12
                var dataSessionUsed = new List<SessionUsed>(); 
                foreach (var sessionUsed in totalSessionWillUsed)
                {
                    dataSessionUsed.Add(new SessionUsed()
                    {
                        IdTeacher = sessionUsed.IdUser,
                        TeacherName = sessionUsed.TeacherName,
                        ClassID = sessionUsed.TeacherName,
                        SessionID = sessionUsed.SessionID,
                        Date = sessionUsed.ScheduleDate
                    });

                    dataSessionUsed2 = new EmailATD12Result()
                    {
                        IdStudent = studentRequest.IdStudent,
                        Reason = studentRequest.Reason,
                        IdTeacher = sessionUsed.IdUser,
                        TeacherName = sessionUsed.TeacherName,
                        ClassID = sessionUsed.ClassID,
                        SessionID = sessionUsed.SessionID,
                        Date = sessionUsed.ScheduleDate
                    };
                }

                var aTD12NotificationModel = new ATD12NotificationModel()
                {
                    IdStudent = studentRequest.IdStudent,
                    Reason = studentRequest.Reason,
                    SessionUseds = dataSessionUsed
                };

                aTD12NotificationModels.Add(aTD12NotificationModel);
            }
            if (trAttendanceAdministrations.Count > 0)
                _dbContext.Entity<TrAttendanceAdministration>().AddRange(trAttendanceAdministrations);
            if (trAttendanceEntries.Count > 0)
                _dbContext.Entity<TrAttendanceEntry>().AddRange(trAttendanceEntries);

            await _dbContext.SaveChangesAsync(CancellationToken);

            var getIdSchoolFromAttendance = _dbContext.Entity<MsAttendance>().Include(x => x.AcademicYear).Where(x => x.Id == body.Students.First().IdAttendance).Select(x => x.AcademicYear.IdSchool).FirstOrDefault();

            if(aTD11NotificationModels.Count > 0)
                ATD11Notification(aTD11NotificationModels, getIdSchoolFromAttendance);

            if (KeyValues.ContainsKey("DataATD12"))
            {
                KeyValues.Remove("DataATD12");
            }
            KeyValues.Add("DataATD12", aTD12NotificationModels);
            var NotificationHomeroom = ATD12Notification(KeyValues, AuthInfo, getIdSchoolFromAttendance);

            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }

        private void ATD11Notification(List<ATD11NotificationModel> aTD11NotificationModel, string IdSchool)
        {
            IDictionary<string, object> paramTemplateNotifikasi = new Dictionary<string, object>();
            paramTemplateNotifikasi.Add("ListData", aTD11NotificationModel);
            paramTemplateNotifikasi.Add("SchoolName", AuthInfo.Tenants.First().Name);

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(IdSchool, "ATD11")
                {
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
        }

        public static string ATD12Notification(IDictionary<string, object> KeyValues, AuthenticationInfo AuthInfo, string IdSchool)
        {

            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(IdSchool, "ATD12")
                {
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }

            return "";
        }
    }
}
