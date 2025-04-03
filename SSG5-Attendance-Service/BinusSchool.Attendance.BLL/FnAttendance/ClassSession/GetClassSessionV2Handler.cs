using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Attendance.FnAttendance.ClassSession;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.ClassSession
{
    public class GetClassSessionV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetClassSessionV2Handler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassSessionRequest>(nameof(GetClassSessionRequest.IdUser),
                                                                       nameof(GetClassSessionRequest.IdSchool),
                                                                       nameof(GetClassSessionRequest.Date));

            var result = await GetClassAndSessions(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<IEnumerable<GetClassSessionResult>> GetClassAndSessions(GetClassSessionRequest param)
        {
            var uniqueComparer = new UniqueIdComparer<SessionOfClass>();
            var scheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.IdBinusianSubtitute == param.IdUser && x.ScheduleDate == param.Date)
                    .ToListAsync(CancellationToken);

            var scheduleRealizationChanged = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.IdBinusian == param.IdUser && x.Status == "Subtituted" && x.ScheduleDate == param.Date && x.IdBinusian != x.IdBinusianSubtitute)
                    .ToListAsync(CancellationToken);

            var listIdLessonRealization = scheduleRealization.Select(x => x.IdLesson).ToList();

            #region Check user Homeroom teacher or not for this homeroom
            var isHomeroomTeacher = !string.IsNullOrEmpty(param.IdHomeroom) ? await _dbContext.Entity<MsHomeroomTeacher>()
                .AnyAsync(x => x.IdBinusian == param.IdUser
                          && x.IdHomeroom == param.IdHomeroom, CancellationToken) : false;

            #endregion
            if (isHomeroomTeacher)
            {
                var academic = await _dbContext.Entity<MsHomeroom>()
                                .Where(e => e.Id == param.IdHomeroom)
                                .Select(e => new
                                {
                                    e.IdAcademicYear,
                                    e.Semester
                                })
                                .FirstOrDefaultAsync(CancellationToken);

                var listPeriod = await _dbContext.Entity<MsPeriod>()
                  .Where(e => e.Grade.Level.IdAcademicYear == academic.IdAcademicYear)
                  .ToListAsync(CancellationToken);

                var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent)
                                .Where(e => e.HomeroomStudent.IdHomeroom == param.IdHomeroom)
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
                                    BinusianID = e.Key.IdBinusian
                                })
                            .ToListAsync(CancellationToken);

                listHomeroomStudentEnrollment.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min());
                listHomeroomStudentEnrollment.ForEach(e => e.Datein = e.EffectiveDate);

                var listIdHomeroomEnroll = listHomeroomStudentEnrollment.Select(e => e.IdHomeroomStudentEnrollment).Distinct().ToList();


                var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                                        .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                    .Include(e => e.LessonNew)
                                                    .Where(x => x.StartDate.Date <= param.Date.Date && 
                                                                x.LessonOld.IdAcademicYear == academic.IdAcademicYear && 
                                                                listIdHomeroomEnroll.Contains(x.IdHomeroomStudentEnrollment))
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
                                                        BinusianID = e.HomeroomStudent.Student.IdBinusian
                                                    })
                                                    .ToListAsync(CancellationToken);

                var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                      .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                      .ToList();

                var listUnionIdLesson = listStudentEnrollmentUnion.Select(e => e.IdLesson).Distinct().ToList();
                List<string> listIdLesson = new List<string>();  
                foreach (var item in listUnionIdLesson)
                {
                    var getStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, param.Date.Date, academic.Semester.ToString(), item);
                    if (getStudentEnrollmentMoving.Count() > 0)
                        listIdLesson.Add(item);
                }
                
                var query = await _dbContext.Entity<MsScheduleLesson>()
                    .Include(e => e.Subject)
                    .Where(x => listIdLesson.Contains(x.IdLesson) && x.ScheduleDate.Date == param.Date.Date)
                    .ToListAsync(CancellationToken);

                return query
                .GroupBy(x => x.ClassID)
                .Select(x => new GetClassSessionResult
                {
                    Id = x.First().Id,
                    ClassId = x.Key,
                    Description = x.First().SubjectName,
                    Sessions = x
                        .OrderBy(y => y.StartTime)
                        .Select(y => new SessionOfClass
                        {
                            Id = y.IdSession,
                            SessionId = y.SessionID,
                            StartTime = y.StartTime,
                            EndTime = y.EndTime
                        })
                        .Distinct(uniqueComparer)
                        .OrderBy(x => int.TryParse(x.SessionId, out var sessionNumber) ? sessionNumber : 0)
                });
            }
            else
            {
                var AcademicyearByDate = await _dbContext.Entity<MsPeriod>()
                   .Include(x => x.Grade)
                       .ThenInclude(x => x.Level)
                           .ThenInclude(x => x.AcademicYear)
               .Where(x => param.Date.Date >= x.AttendanceStartDate.Date)
               .Where(x => param.Date.Date <= x.AttendanceEndDate.Date)
               .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
               .GroupBy(x => new
               {
                   x.Grade.Level.IdAcademicYear,
                   x.Semester,
               })
               .Select(x => x.Key)
               .FirstOrDefaultAsync(CancellationToken);

                var listIdLessonByLessonTeacher = AcademicyearByDate != null ? await _dbContext.Entity<MsLessonTeacher>()
                 .Include(x => x.Lesson)
                 .Where(x => x.IdUser == param.IdUser || listIdLessonRealization.Contains(x.IdLesson))
                 .Where(x => x.Lesson.IdAcademicYear == AcademicyearByDate.IdAcademicYear)
                 .Where(x => x.IsAttendance)
                 .Select(x => x.Lesson.Id)
                 .ToListAsync(CancellationToken) : new List<string>();

                var listIdLessonBySchedule = await _dbContext.Entity<MsSchedule>()
                 .Where(x => x.IdUser == param.IdUser || listIdLessonRealization.Contains(x.IdLesson))
                 .Where(x => x.Lesson.IdAcademicYear == AcademicyearByDate.IdAcademicYear)
                 .Where(x => x.Semester == AcademicyearByDate.Semester)
                 .Where(x => listIdLessonByLessonTeacher.Contains(x.IdLesson) && x.Day.Description== param.Date.DayOfWeek.ToString())
                 .Select(x => x.Lesson.Id)
                 .ToListAsync(CancellationToken);

                var querydata = await _dbContext.Entity<MsScheduleLesson>()
                .Where(x => listIdLessonBySchedule.Contains(x.IdLesson) 
                    && EF.Functions.DateDiffDay(x.ScheduleDate, param.Date) == 0)
                .ToListAsync(CancellationToken);

                var query = querydata.Where(x => !scheduleRealization.Any(y => y.IdLesson == x.IdLesson && y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID)).ToList();
                foreach (var item in scheduleRealization)
                {
                    if (querydata.Any(x => x.IdLesson == item.IdLesson && x.SessionID == item.SessionID && x.ScheduleDate == item.ScheduleDate && !item.IsCancel))
                        query.Add(querydata.Where(x => x.IdLesson == item.IdLesson && x.SessionID == item.SessionID && x.ScheduleDate == item.ScheduleDate && !item.IsCancel).FirstOrDefault());
                }
                query = querydata.Where(x => !scheduleRealizationChanged.Any(y => y.IdLesson == x.IdLesson && y.ScheduleDate == x.ScheduleDate && y.SessionID == x.SessionID)).ToList();

                var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Lessons)
                                .Where(e => (listIdLessonBySchedule.Contains(e.IdLesson))
                                                && e.HomeroomStudent.Homeroom.IdAcademicYear == AcademicyearByDate.IdAcademicYear)
                                .GroupBy(e => new GetHomeroom
                                {
                                    IdLesson = e.IdLesson,
                                    IdStudent = e.HomeroomStudent.IdStudent,
                                    IdHomeroomStudent = e.HomeroomStudent.Id,
                                    Homeroom = new ItemValueVm
                                    {
                                        Id = e.HomeroomStudent.IdHomeroom,
                                    },
                                    ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = e.HomeroomStudent.Homeroom.Grade.Id,
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
                                    IdHomeroomStudentEnrollment = e.Id
                                })
                                .Select(e => new GetHomeroom
                                {
                                    IdLesson = e.Key.IdLesson,
                                    IdStudent = e.Key.IdStudent,
                                    IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                    Homeroom = new ItemValueVm
                                    {
                                        Id = e.Key.Homeroom.Id,
                                    },
                                    ClassroomCode = e.Key.ClassroomCode,
                                    Grade = new CodeWithIdVm
                                    {
                                        Id = e.Key.Grade.Id,
                                        Code = e.Key.Grade.Code,
                                        Description = e.Key.Grade.Description
                                    },
                                    Level = new CodeWithIdVm
                                    {
                                        Id = e.Key.Level.Id,
                                        Code = e.Key.Level.Code,
                                        Description = e.Key.Level.Description
                                    },
                                    Semester = e.Key.Semester,
                                    IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                    IsDelete = false,
                                    IsFromMaster = true,
                                    IsShowHistory = false
                                })
                                .ToListAsync(CancellationToken);

                var listIdlevel = listHomeroomStudentEnrollment.Select(e => e.Level.Id).Distinct().ToList();

                var listLevelMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                    .Where(e => listIdlevel.Contains(e.IdLevel) && e.AbsentTerms == AbsentTerm.Session)
                                    .Select(e => e.IdLevel )
                                    .ToListAsync(CancellationToken);

                query = query.Where(x => listLevelMappingAttendance.Contains(x.IdLevel)).ToList();

                return query
                .GroupBy(x => x.ClassID)
                .Select(x => new GetClassSessionResult
                {
                    Id = x.First().Id,
                    ClassId = x.Key,
                    Description = x.First().SubjectName,
                    Sessions = x
                        .OrderBy(y => y.StartTime)
                        .Select(y => new SessionOfClass
                        {
                            Id = y.IdSession,
                            SessionId = y.SessionID,
                            StartTime = y.StartTime,
                            EndTime = y.EndTime
                        })
                        .Distinct(uniqueComparer)
                        .OrderBy(x => int.TryParse(x.SessionId, out var sessionNumber) ? sessionNumber : 0)
                });
            }
        }
    }
}
