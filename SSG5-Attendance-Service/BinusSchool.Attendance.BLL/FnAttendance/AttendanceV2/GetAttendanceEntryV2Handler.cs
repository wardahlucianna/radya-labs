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
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Models.Binusian.BinusSchool.AttendanceLog;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetAttendanceEntryV2Handler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAuth _apiAuth;
        private readonly IAttendanceLog _apiAttendanceLog;
        private readonly IMachineDateTime _datetimeNow;
        public GetAttendanceEntryV2Handler(IAttendanceDbContext dbContext, IAttendanceLog apiAttendanceLog, IAuth apiAuth, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _apiAttendanceLog = apiAttendanceLog;
            _apiAuth = apiAuth;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceEntryV2Request>();

            GetHomeroom homeroom = default;
            List<string> listIdLesson = new List<string>();
            if (PositionConstant.ClassAdvisor == param.CurrentPosition)
            {
                var listHomeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                            .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                            .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Where(e => e.IdBinusian == param.IdUser
                                    && e.Homeroom.IdAcademicYear == param.IdAcademicYear
                                    && e.IdHomeroom == param.IdHomeroom
                                    )
                            .Select(e => new GetHomeroom
                            {
                                Homeroom = new ItemValueVm
                                {
                                    Id = e.IdHomeroom,
                                    Description = $"{e.Homeroom.Grade.Code}{e.Homeroom.GradePathwayClassroom.Classroom.Code}"
                                },
                                Level = new CodeWithIdVm
                                {
                                    Id = e.Homeroom.Grade.Level.Id,
                                    Code = e.Homeroom.Grade.Level.Code,
                                    Description = e.Homeroom.Grade.Level.Description
                                },
                                Grade = new CodeWithIdVm
                                {
                                    Id = e.Homeroom.Grade.Id,
                                    Code = e.Homeroom.Grade.Code,
                                    Description = e.Homeroom.Grade.Description
                                },
                                Semester = e.Homeroom.Semester
                            })
                            .ToListAsync(CancellationToken);

                //var IdGrade = listHomeroom.Select(e => e.grade.Id).FirstOrDefault();
                //var semester = await _dbContext.Entity<MsPeriod>()
                //             .Where(e => e.IdGrade== IdGrade
                //                        && (param.Date.Date >= e.StartDate && param.Date.Date <= e.EndDate))
                //             .Select(e => e.Semester)
                //             .FirstOrDefaultAsync(CancellationToken);

                homeroom = listHomeroom.FirstOrDefault();

                if (homeroom == null)
                    throw new BadRequestException("You are not Homeroom Teacher");

                var listEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                    .Include(e => e.Lesson)
                    .Where(e => e.HomeroomStudent.Homeroom.Id == homeroom.Homeroom.Id)
                    .Select(e => new { e.IdLesson , e.Id })
                    .ToListAsync(CancellationToken);

                var listIdEnrollment = listEnrollment.Select(x => x.Id).Distinct().ToList();

                var idlessonInMoving = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom)
                                                    .ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                                .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                                                .Include(e => e.LessonNew)
                                                .Where(x => x.StartDate.Date <= param.Date.Date && x.LessonOld.IdAcademicYear == param.IdAcademicYear
                                                            && listIdEnrollment.Contains(x.IdHomeroomStudentEnrollment))
                                                .GroupBy(x=> x.IdLessonNew)
                                                .Select(x=> x.Key)
                                                .ToListAsync(CancellationToken);

                listIdLesson = listEnrollment.Select(x=> x.IdLesson).Distinct().ToList().Union(idlessonInMoving)
                                                      .ToList();
            }
            var listIdLessonRealization = new List<string>();
            if (PositionConstant.SubjectTeacher == param.CurrentPosition)
            {
                var scheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                    .Where(x => x.IdBinusianSubtitute == param.IdUser && x.ScheduleDate == param.Date)
                    .ToListAsync(CancellationToken);

                listIdLessonRealization = scheduleRealization.Select(x => x.IdLesson).ToList();

                listIdLesson = await _dbContext.Entity<MsSchedule>()
                          .Include(e => e.Lesson)
                          .Where(e => e.IdUser == param.IdUser
                                  && e.Lesson.IdAcademicYear == param.IdAcademicYear
                                  && e.Lesson.ClassIdGenerated == param.ClassId || listIdLessonRealization.Contains(e.IdLesson))
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

                homeroom = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                    .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                    .Include(e => e.Lesson)
                                    .Where(e => listIdLesson.Contains(e.IdLesson) && e.Lesson.ClassIdGenerated == param.ClassId)
                                    .GroupBy(e => new GetHomeroom
                                    {
                                        Homeroom = new ItemValueVm
                                        {
                                            Id = e.HomeroomStudent.IdHomeroom,
                                        },
                                        ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                        Grade = new CodeWithIdVm
                                        {
                                            Id = e.HomeroomStudent.Homeroom.Id,
                                            Code = e.HomeroomStudent.Homeroom.Grade.Code,
                                            Description = e.HomeroomStudent.Homeroom.Grade.Description
                                        },
                                        Level = new CodeWithIdVm
                                        {
                                            Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                                            Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                                            Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                                        },
                                        Semester = e.HomeroomStudent.Homeroom.Semester
                                    })
                                    .Select(e => new GetHomeroom
                                    {
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
                                        Semester = e.Key.Semester
                                    })
                                    .FirstOrDefaultAsync(CancellationToken);

                if (homeroom == null)
                    throw new BadRequestException("You are not subject teacher");
            }

            NameValueVm Teacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                    .Include(e => e.TeacherPosition).ThenInclude(x => x.LtPosition)
                                    .Include(e => e.Staff)
                                    .Where(e => e.IdHomeroom == param.IdHomeroom && e.TeacherPosition.LtPosition.Code == "CA")
                                    .Select(e => new NameValueVm
                                    {
                                        Id = e.IdBinusian,
                                        Name = (e.Staff.FirstName == null ? "" : e.Staff.FirstName) + (e.Staff.LastName == null ? "" : " " + e.Staff.LastName)
                                    })
                                    .FirstOrDefaultAsync(CancellationToken);

            var mappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                               .Where(e => e.IdLevel == homeroom.Level.Id)
                               .Select(e => new
                               {
                                   e.UsingCheckboxAttendance,
                                   e.IsUseDueToLateness,
                                   e.RenderAttendance,
                                   e.IsUseWorkhabit,
                                   e.IsNeedValidation,
                                   e.AbsentTerms
                               })
                               .FirstOrDefaultAsync(CancellationToken);

            var queryScheduleLesson = _dbContext.Entity<MsScheduleLesson>()
                               .Include(e => e.Lesson)
                               .Include(e => e.Subject)
                               .Include(e => e.Session)
                               .Include(e => e.AcademicYear)
                               .Where(e => e.IdAcademicYear == param.IdAcademicYear
                                            && e.ScheduleDate.Date == param.Date.Date
                                            && listIdLesson.Contains(e.IdLesson)
                                       );

            if (!string.IsNullOrEmpty(param.ClassId))
                queryScheduleLesson = queryScheduleLesson.Where(e => e.ClassID == param.ClassId);

            if (!string.IsNullOrEmpty(param.IdSession))
                queryScheduleLesson = queryScheduleLesson.Where(e => e.IdSession == param.IdSession);

            var listScheduleLesson = await queryScheduleLesson
                       .Select(e => new
                       {
                           e.IdLesson,
                           e.Id,
                           e.IdSession,
                           sessionName = e.Session.Name,
                           sessionID = e.Session.SessionID,
                           sessionStartTime = e.Session.StartTime,
                           sessionEndTime = e.Session.EndTime,
                           e.AcademicYear.IdSchool,
                           e.IdLevel,
                           e.ScheduleDate,
                           e.Lesson.Semester
                       })
                       .ToListAsync(CancellationToken);

            var listIdScheduleLesson = listScheduleLesson.Select(e => e.Id).ToList();
            var listIdLessonByScheduleLesson = listScheduleLesson.Select(e => e.IdLesson).ToList();

            var scheduleLesson = listScheduleLesson.FirstOrDefault();

            if (scheduleLesson == null)
                throw new BadRequestException("Schedule is not found");

            if (PositionConstant.SubjectTeacher == param.CurrentPosition)
            {
                var accessAttendance = await _dbContext.Entity<MsLessonTeacher>()
                          .Include(e => e.Lesson)
                          .Where(e => e.IdUser == param.IdUser
                                  && e.Lesson.IdAcademicYear == param.IdAcademicYear
                                  && e.Lesson.ClassIdGenerated == param.ClassId
                                  && e.IsAttendance
                                  && e.IdLesson == scheduleLesson.IdLesson || listIdLessonRealization.Contains(e.IdLesson))
                          .Select(e => e.IdLesson)
                          .ToListAsync(CancellationToken);

                if (!accessAttendance.Any())
                    throw new BadRequestException("You dont have access attendance");
            }

            var listAttendance = await _dbContext.Entity<MsAttendanceMappingAttendance>()
                                .Include(e => e.Attendance)
                               .Where(e => e.Attendance.IdAcademicYear == param.IdAcademicYear
                                       && (e.Attendance.Code == "LT" || e.Attendance.Code == "PR")
                                       && e.MappingAttendance.IdLevel == scheduleLesson.IdLevel
                                       )
                               .ToListAsync(CancellationToken);

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                .Include(e => e.Student)
                                .Where(e => e.IdAcademicYear == param.IdAcademicYear
                                        && e.ActiveStatus
                                        && e.StartDate.Date <= param.Date.Date
                                        && (e.EndDate == null || e.EndDate >= param.Date.Date)
                                        )
                                .Select(e => new
                                {
                                    e.IdStudent,
                                    e.StartDate,
                                    EndDate = e.EndDate == null
                                               ? param.Date.Date
                                               : Convert.ToDateTime(e.EndDate),
                                    e.Student.IdBinusian
                                })
                                .ToListAsync(CancellationToken);

            var idStudent = listStudentStatus.Select(e => e.IdStudent).ToList();

            List<GetHomeroomStudent> studentEnrollment = new List<GetHomeroomStudent>();

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                    .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                    .ToListAsync(CancellationToken);

            var queryHomeroomStudentEnrollment = _dbContext.Entity<MsHomeroomStudentEnrollment>()
                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade)
                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                            .Include(e => e.HomeroomStudent).ThenInclude(e => e.Student)
                            .Include(e => e.Lesson)
                            .Where(e => idStudent.Contains(e.HomeroomStudent.IdStudent)
                                        && e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear
                                        );

            //if (!string.IsNullOrEmpty(param.IdHomeroom))
            //{
            //    queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.HomeroomStudent.IdHomeroom == param.IdHomeroom);
            //}
            //else
            //{
            //    queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.IdLesson == scheduleLesson.IdLesson);

            //}

            if (string.IsNullOrEmpty(param.IdHomeroom))
            {
                queryHomeroomStudentEnrollment = queryHomeroomStudentEnrollment.Where(e => e.IdLesson == scheduleLesson.IdLesson);
            }

            var listHomeroomStudentEnrollment = await queryHomeroomStudentEnrollment
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
                                                .Where(x => x.StartDate.Date <= param.Date.Date && x.LessonOld.IdAcademicYear == param.IdAcademicYear)
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
            //moving
            List<GetHomeroom> listStudentEnrollmentMoving = new List<GetHomeroom>();
            if (mappingAttendance.AbsentTerms == AbsentTerm.Day)
            {
                var queryScheduleLessonByMoving = _dbContext.Entity<MsScheduleLesson>()
                                                 .Include(e => e.Lesson)
                                                 .Include(e => e.Subject)
                                                 .Include(e => e.Session)
                                                 .Include(e => e.AcademicYear)
                                                 .Where(e => e.IdAcademicYear == param.IdAcademicYear
                                                              && e.ScheduleDate.Date == param.Date.Date
                                                              && listIdLesson.Contains(e.IdLesson)
                                                         );

                if (!string.IsNullOrEmpty(param.ClassId))
                    queryScheduleLessonByMoving = queryScheduleLessonByMoving.Where(e => e.ClassID == param.ClassId);

                if (!string.IsNullOrEmpty(param.IdSession))
                    queryScheduleLessonByMoving = queryScheduleLessonByMoving.Where(e => e.IdSession == param.IdSession);

                var listScheduleLessonMoving = await queryScheduleLessonByMoving
                                               .Select(e => e.IdLesson)
                                               .ToListAsync(CancellationToken);

                //moving
                foreach (var item in listScheduleLessonMoving)
                {
                    var getStudentEnrollmentMoving = GetMovingStudent(listStudentEnrollmentUnion, Convert.ToDateTime(scheduleLesson.ScheduleDate), scheduleLesson.Semester.ToString(), item);
                    listStudentEnrollmentMoving.AddRange(getStudentEnrollmentMoving);
                }

                //if (!string.IsNullOrEmpty(param.IdHomeroom))
                //{
                //    listStudentEnrollmentMoving = listStudentEnrollmentMoving.Where(e => e.Homeroom.Id == param.IdHomeroom).ToList();
                //}

                var StudentEnrollmentMoving = listStudentEnrollmentMoving
                                               .Where(e => idStudent.Contains(e.IdStudent))
                                               .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                               .ToList();

                studentEnrollment = StudentEnrollmentMoving
                                    .Where(e => !e.IsDelete)
                                    .GroupBy(e => new
                                    {
                                        IdHomeroomStudent = e.IdHomeroomStudent,
                                        IdStudent = e.IdStudent,
                                        FirstName = e.FirstName,
                                        MiddleName = e.MiddleName,
                                        LastName = e.LastName,
                                        BinusianID = e.BinusianID,
                                    })
                                    .Select(e => new GetHomeroomStudent
                                    {
                                        IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                        IdStudent = e.Key.IdStudent,
                                        FirstName = e.Key.FirstName,
                                        MiddleName = e.Key.MiddleName,
                                        LastName = e.Key.LastName,
                                        BinusianID = e.Key.BinusianID,
                                    })
                                    .ToList();
            }
            else
            {
                var getStudentEnrollmentMoving = GetMovingStudent(listStudentEnrollmentUnion, Convert.ToDateTime(scheduleLesson.ScheduleDate), scheduleLesson.Semester.ToString(), scheduleLesson.IdLesson);
                listStudentEnrollmentMoving.AddRange(getStudentEnrollmentMoving);

                var StudentEnrollmentMoving = listStudentEnrollmentMoving
                                               .Where(e => idStudent.Contains(e.IdStudent))
                                               .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                               .ToList();

                if (PositionConstant.ClassAdvisor == param.CurrentPosition)
                {
                    StudentEnrollmentMoving = StudentEnrollmentMoving.Where(x => x.Homeroom.Id == param.IdHomeroom).ToList();
                }

                studentEnrollment = StudentEnrollmentMoving
                                    .Where(e => e.IdLesson == scheduleLesson.IdLesson && !e.IsDelete)
                                    .GroupBy(e => new
                                    {
                                        IdHomeroomStudent = e.IdHomeroomStudent,
                                        IdStudent = e.IdStudent,
                                        FirstName = e.FirstName,
                                        MiddleName = e.MiddleName,
                                        LastName = e.LastName,
                                        BinusianID = e.BinusianID,
                                    })
                                    .Select(e => new GetHomeroomStudent
                                    {
                                        IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                        IdStudent = e.Key.IdStudent,
                                        FirstName = e.Key.FirstName,
                                        MiddleName = e.Key.MiddleName,
                                        LastName = e.Key.LastName,
                                        BinusianID = e.Key.BinusianID,
                                    })
                                    .ToList();

            }



            //var listWorkhabit = await _dbContext.Entity<MsMappingAttendanceWorkhabit>()
            //                        .Include(e=>e.Workhabit)
            //                       .Where(e => e.Workhabit.IdAcademicYear == param.IdAcademicYear && e.MappingAttendance.IdLevel==homeroom.level.Id)
            //                       .Select(e => new
            //                       {
            //                           e.Id,
            //                           e.IdWorkhabit,
            //                           workhobitCode = e.Workhabit.Code,
            //                           workhobitDescription = e.Workhabit.Description
            //                       })
            //                       .ToListAsync(CancellationToken);

            var queryAttendanceEntry = _dbContext.Entity<TrAttendanceEntryV2>()
                                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                               .Include(e => e.ScheduleLesson)
                               .Where(e => e.ScheduleLesson.IdAcademicYear == param.IdAcademicYear);

            if (mappingAttendance.AbsentTerms == AbsentTerm.Session)
            {
                queryAttendanceEntry = queryAttendanceEntry.Where(e => e.IdScheduleLesson == scheduleLesson.Id);
            }
            else
            {
                queryAttendanceEntry = queryAttendanceEntry.Where(e => listIdScheduleLesson.Contains(e.IdScheduleLesson));
            }

            var listAttendanceEntry = await queryAttendanceEntry
                           .Select(e => new
                           {
                               e.IdAttendanceEntry,
                               e.IdHomeroomStudent,
                               e.IdAttendanceMappingAttendance,
                               e.IsFromAttendanceAdministration,
                               e.LateTime,
                               e.Notes,
                               e.FileEvidence,
                               e.Status,
                               e.PositionIn,
                               attendanceCode = e.AttendanceMappingAttendance.Attendance.Code,
                               attendanceDescription = e.AttendanceMappingAttendance.Attendance.Description,
                               attendanceId = e.AttendanceMappingAttendance.Attendance.Id,
                               e.ScheduleLesson.IdLesson,
                               e.DateIn
                           })
                           .OrderBy(e => e.IdLesson)
                           .ToListAsync(CancellationToken);

            var listIdAttendanceEntry = listAttendanceEntry.Select(e => e.IdAttendanceEntry).ToList();

            var listAttendanceWorkhabit = await _dbContext.Entity<TrAttendanceEntryWorkhabitV2>()
                                .Include(e => e.MappingAttendanceWorkhabit).ThenInclude(e => e.Workhabit)
                              .Where(e => listIdAttendanceEntry.Contains(e.IdAttendanceEntry))
                              .Select(e => new
                              {
                                  e.IdAttendanceEntry,
                                  e.Id,
                                  workhobitCode = e.MappingAttendanceWorkhabit.Workhabit.Code,
                                  workhobitDescription = e.MappingAttendanceWorkhabit.Workhabit.Description,
                                  IdMappingAttendanceWorkhabit = e.MappingAttendanceWorkhabit.Id
                              })
                              .ToListAsync(CancellationToken);

            #region Tapping
            List<AttendanceLog> getAttendanceTeppingLog = new List<AttendanceLog>();
            var startHourtapping = new TimeSpan(0, 4, 0, 0);
            var EndHourtapping = new TimeSpan(0, 18, 0, 0);
            var studentEnrollmentIdStudent = studentEnrollment.Select(e => e.IdStudent).ToList();

            var firstSession = listScheduleLesson.OrderBy(x=> x.sessionStartTime).Select(x=> x.sessionID).FirstOrDefault();

            if (scheduleLesson.sessionID == firstSession || mappingAttendance.AbsentTerms == AbsentTerm.Day)
            {
                var listIdBinusian = listStudentStatus.Where(e => studentEnrollmentIdStudent.Contains(e.IdStudent)).Select(e => e.IdBinusian).ToList();

                var AttendanceLogMachineRequest = new GetAttendanceLogRequest
                {
                    IdSchool = scheduleLesson.IdSchool,
                    Year = param.Date.Year,
                    Month = param.Date.Month,
                    Day = param.Date.Day,
                    StartHour = startHourtapping.Hours,
                    EndHour = EndHourtapping.Hours,
                    StartMinutes = startHourtapping.Minutes,
                    EndMinutes = EndHourtapping.Minutes,
                    ListStudent = listIdBinusian
                };

                try
                {
                    getAttendanceTeppingLog = await GetAttendanceLogMachine.AttendanceLogMachine(_apiAttendanceLog, _apiAuth, AttendanceLogMachineRequest);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
            }

            #endregion

            var listIdStudent = studentEnrollment.Select(e => e.IdStudent).Distinct().ToList();

            var listHTrMoveStudentHomeroom = await _dbContext.Entity<HTrMoveStudentHomeroom>()
                                             .Include(e => e.HomeroomStudent)
                                             .Include(e => e.HomeroomNew).ThenInclude(e => e.Grade)
                                             .Include(e => e.HomeroomNew).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                              .Where(x => x.StartDate.Date <= param.Date.Date
                                                        && x.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                        && listIdStudent.Contains(x.HomeroomStudent.IdStudent))
                                             .Select(e => new GetStudentHomeroom
                                             {
                                                 Homeroom = new ItemValueVm
                                                 {
                                                     Id = e.HomeroomNew.Id,
                                                     Description = e.HomeroomNew.Grade.Code + e.HomeroomNew.GradePathwayClassroom.Classroom.Code
                                                 },
                                                 IdStudent = e.HomeroomStudent.IdStudent,
                                                 EffectiveDate = e.StartDate,
                                                 IsFromMaster = false,
                                                 IdGrade = e.HomeroomNew.Grade.Id,
                                                 Semester = e.HomeroomNew.Semester,
                                                 DateIn = e.DateIn,
                                                 IsShowHistory = e.IsShowHistory
                                             })
                                             .ToListAsync(CancellationToken);

            var listMsHomeroomStudent = await _dbContext.Entity<MsHomeroomStudent>()
                                               .Include(e => e.Homeroom).ThenInclude(e => e.Grade)
                                               .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                               .Where(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear
                                                        && listIdStudent.Contains(x.IdStudent))
                                               .Select(e => new GetStudentHomeroom
                                               {
                                                   Homeroom = new ItemValueVm
                                                   {
                                                       Id = e.Homeroom.Id,
                                                       Description = e.Homeroom.Grade.Code + e.Homeroom.GradePathwayClassroom.Classroom.Code
                                                   },
                                                   IdStudent = e.IdStudent,
                                                   IsFromMaster = true,
                                                   IdGrade = e.Homeroom.Grade.Id,
                                                   Semester = e.Homeroom.Semester,
                                                   DateIn = e.DateIn,
                                                   IsShowHistory = false,
                                               })
                                               .ToListAsync(CancellationToken);

            listMsHomeroomStudent.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.IdGrade && f.Semester==e.Semester).Select(f => f.AttendanceStartDate).Min());
            listHomeroomStudentEnrollment.ForEach(e => e.Datein = e.EffectiveDate);

            var listHomeroomStudentUnion = listMsHomeroomStudent.Union(listHTrMoveStudentHomeroom)
                                                .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.DateIn)
                                                .ToList();

            var idGrade = listHomeroomStudentUnion.Select(x => x.IdGrade).FirstOrDefault();         

            var currentSemester = listPeriod
                                    .Where(x => x.IdGrade == idGrade 
                                                && x.StartDate<=param.Date
                                                && x.EndDate>=param.Date)
                                    .Select(x=> x.Semester)
                                    .FirstOrDefault();

            if (currentSemester == 0)
                currentSemester = 1;

            listHomeroomStudentUnion = listHomeroomStudentUnion.Where(x => x.Semester == currentSemester).ToList();

            List<AttendanceEntryStudent> entry = new List<AttendanceEntryStudent>();
            for (var i = 0; i < studentEnrollment.Count(); i++)
            {
                var data = studentEnrollment[i];

                if (!string.IsNullOrEmpty(param.IdHomeroom))
                {
                    var listHomeroomStudentByIdStudent = listHomeroomStudentUnion.Where(e => e.IdStudent == data.IdStudent).ToList();
                    var homeroomStudentByIdStudent = listHomeroomStudentUnion.Where(e => e.IdStudent == data.IdStudent).LastOrDefault();

                    if (homeroomStudentByIdStudent == null)
                        continue;

                    if (homeroomStudentByIdStudent.Homeroom.Id != param.IdHomeroom)
                        continue;
                }

                var attendanceEntryIdLesson = listAttendanceEntry
                            .Where(e => e.IdHomeroomStudent == data.IdHomeroomStudent
                                && listIdLesson.Contains(e.IdLesson))
                            .Distinct().ToList();

                var attendanceEntryByStudent = listAttendanceEntry.Where(e => e.IdHomeroomStudent == data.IdHomeroomStudent).OrderBy(e => e.DateIn).LastOrDefault();

                if (!attendanceEntryIdLesson.Any())
                    attendanceEntryByStudent = null;


                var getAttendanceTeppingLogByStudent = getAttendanceTeppingLog.Where(e => e.BinusianID == data.BinusianID).FirstOrDefault();

                var attendancePresent = listAttendance.Where(e => e.Attendance.Code == "PR").ToList();
                var attendanceLate = listAttendance.Where(e => e.Attendance.Code == "LT").ToList();

                var attendance = new AttendanceEntryItem
                {
                    Code = attendanceEntryByStudent == null
                                                ? attendancePresent.Select(e => e.Attendance.Code).FirstOrDefault()
                                                : attendanceEntryByStudent.attendanceCode,
                    Description = attendanceEntryByStudent == null
                                                ? attendancePresent.Select(e => e.Attendance.Description).FirstOrDefault()
                                                : attendanceEntryByStudent.attendanceDescription,
                    Id = attendanceEntryByStudent == null
                                                ? attendancePresent.Select(e => e.IdAttendance).FirstOrDefault()
                                                : attendanceEntryByStudent.attendanceId,
                    IdAttendanceMapAttendance = attendanceEntryByStudent == null
                                                ? attendancePresent.Select(e => e.Id).FirstOrDefault()
                                                : attendanceEntryByStudent.IdAttendanceMappingAttendance,
                    IsFromAttendanceAdministration = attendanceEntryByStudent == null
                                                    ? false
                                                    : attendanceEntryByStudent.IsFromAttendanceAdministration,
                };

                List<AttendanceEntryItemWorkhabit> listWorkhabitAttendace = new List<AttendanceEntryItemWorkhabit>();

                if (attendanceEntryByStudent != null)
                {
                    listWorkhabitAttendace = listAttendanceWorkhabit
                                                .Where(e => e.IdAttendanceEntry == attendanceEntryByStudent.IdAttendanceEntry)
                                                .Select(e => new AttendanceEntryItemWorkhabit
                                                {
                                                    Id = e.IdMappingAttendanceWorkhabit,
                                                    Code = e.workhobitCode,
                                                    Description = e.workhobitDescription,
                                                    IsTick = listAttendanceWorkhabit.Where(e => e.IdAttendanceEntry == attendanceEntryByStudent.IdAttendanceEntry && e.Id == attendanceEntryByStudent.IdAttendanceMappingAttendance).Any()
                                                }).ToList();
                }

                TimeSpan? Late = null;
                var isFromTapping = false;
                if ((scheduleLesson.sessionID == firstSession || mappingAttendance.AbsentTerms == AbsentTerm.Day) && getAttendanceTeppingLogByStudent != null)
                {
                    Late = getAttendanceTeppingLogByStudent != null
                                ? new TimeSpan(0, getAttendanceTeppingLogByStudent.DetectedDate.Hour, getAttendanceTeppingLogByStudent.DetectedDate.Minute, 0, 0)
                                : attendanceEntryByStudent == null
                                    ? null
                                    : attendanceEntryByStudent.LateTime;

                    isFromTapping = getAttendanceTeppingLogByStudent != null ? true : false;

                    attendance = new AttendanceEntryItem
                    {
                        Code = attendanceEntryByStudent == null
                                ? attendancePresent.Select(e => e.Attendance.Code).FirstOrDefault()
                                : attendanceEntryByStudent.attendanceCode,
                        Description = attendanceEntryByStudent == null
                                ? attendancePresent.Select(e => e.Attendance.Description).FirstOrDefault()
                                : attendanceEntryByStudent.attendanceDescription,
                        Id = attendanceEntryByStudent == null
                                ? attendancePresent.Select(e => e.IdAttendance).FirstOrDefault()
                                : attendanceEntryByStudent.attendanceId,
                        IdAttendanceMapAttendance = attendanceEntryByStudent == null
                                ? attendancePresent.Select(e => e.Id).FirstOrDefault()
                                : attendanceEntryByStudent.IdAttendanceMappingAttendance,
                        IsFromAttendanceAdministration = attendanceEntryByStudent == null
                                ? false
                                : attendanceEntryByStudent.IsFromAttendanceAdministration,
                    };
                }

                entry.Add(new AttendanceEntryStudent
                {
                    Id = data.IdStudent,
                    Name = (data.FirstName == null ? "" : data.FirstName) + (data.MiddleName == null ? "" : " " + data.MiddleName) + (data.LastName == null ? "" : " " + data.LastName),
                    IdScheduleLesson = scheduleLesson.Id,
                    IdHomeroomStudent = data.IdHomeroomStudent,
                    IdSession = scheduleLesson.IdSession,
                    Attendance = attendance,
                    Workhabits = mappingAttendance.IsUseWorkhabit
                                    ? listWorkhabitAttendace
                                    : null,
                    IsFromTapping = isFromTapping,
                    Late = Late,
                    File = attendanceEntryByStudent == null ? null : attendanceEntryByStudent.FileEvidence,
                    Note = attendanceEntryByStudent == null ? null : attendanceEntryByStudent.Notes,
                    Status = attendanceEntryByStudent == null ? AttendanceEntryStatus.Unsubmitted : attendanceEntryByStudent.Status,
                    PositionIn = attendanceEntryByStudent == null ? null : attendanceEntryByStudent.PositionIn,
                });
            }

            if (param.Status.HasValue)
            {
                entry = entry.Where(x => x.Status == param.Status).ToList();
            }

            AttendanceEntrySummary summary = new AttendanceEntrySummary
            {
                TotalStudent = entry.Count(),
                Pending = entry.Where(e => e.Status == AttendanceEntryStatus.Pending).Count(),
                Submitted = entry.Where(e => e.Status == AttendanceEntryStatus.Submitted).Count(),
            };

            GetAttendanceEntryV2Result result = default;
            result = new GetAttendanceEntryV2Result
            {
                Id = homeroom.Homeroom.Id,
                Code = homeroom.Homeroom.Description,
                Level = homeroom.Level,
                IdScheduleLesson = scheduleLesson.Id,
                Grade = homeroom.Grade,
                Semester = homeroom.Semester,
                UsingCheckboxAttendance = mappingAttendance.UsingCheckboxAttendance,
                NeedValidation = mappingAttendance.IsNeedValidation,
                Teacher = Teacher,
                Date = param.Date.Date,
                Session = scheduleLesson.sessionName,
                Summary = summary,
                Entries = entry,
                RenderAttendance = mappingAttendance.RenderAttendance,
            };

            return Request.CreateApiResult2(result as object);
        }

        public static List<GetHomeroom> GetMovingStudent(List<GetHomeroom> listStudentEnrollmentUnion, DateTime scheduleDate, string semester, string idLesson)
        {
            var listIdHomeroomStudentEnrollment = listStudentEnrollmentUnion.Where(e => e.IdLesson == idLesson).Select(e => e.IdHomeroomStudentEnrollment)
                                                    .Distinct().ToList();

            listStudentEnrollmentUnion = listStudentEnrollmentUnion.Where(e => listIdHomeroomStudentEnrollment.Contains(e.IdHomeroomStudentEnrollment)).ToList();

            var listStudentEnrollmentByDate = listStudentEnrollmentUnion
                                                .Where(e => e.EffectiveDate.Date <= scheduleDate.Date && e.Semester.ToString() == semester)
                                                .ToList();

            listIdHomeroomStudentEnrollment = listStudentEnrollmentByDate.Select(e => e.IdHomeroomStudentEnrollment).Distinct().ToList();

            var listStudentEnrollmentNew = new List<GetHomeroom>();
            foreach (var idHomeroomStudentEnrollment in listIdHomeroomStudentEnrollment)
            {
                var studentEnrollment = listStudentEnrollmentByDate
                                        .Where(e => e.IdHomeroomStudentEnrollment == idHomeroomStudentEnrollment)
                                        .LastOrDefault();

                var checkFirstDeleteSubject = listStudentEnrollmentByDate.Where(x => x.IdHomeroomStudentEnrollment == studentEnrollment.IdHomeroomStudentEnrollment).ToList();
                if (checkFirstDeleteSubject.Count > 0)
                {
                    if (checkFirstDeleteSubject.Count() == 2 && checkFirstDeleteSubject.Any(x=> x.IsDelete == true))
                        continue;
                }

                if (studentEnrollment.IdLesson == idLesson && !studentEnrollment.IsDelete)
                {
                    listStudentEnrollmentNew.Add(studentEnrollment);
                }

            }

            return listStudentEnrollmentNew;
        }
    }
}
