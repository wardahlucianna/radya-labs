using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.Attendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetHomeroomAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _datetimeNow;

        public GetHomeroomAttendanceHandler(IAttendanceDbContext dbContext, IMachineDateTime datetimeNow)
        {
            _dbContext = dbContext;
            _datetimeNow = datetimeNow;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceRequest>();

            var listHomeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(e => e.IdBinusian == param.IdUser
                                        && e.Homeroom.IdAcademicYear == param.IdAcademicYear
                                        && e.Homeroom.Semester == param.Semester
                                        )
                                .GroupBy(e => new
                                {
                                    e.IdHomeroom,
                                    gradeCode = e.Homeroom.Grade.Code,
                                    classroomCode = e.Homeroom.GradePathwayClassroom.Classroom.Code,
                                    e.Homeroom.Grade.IdLevel,
                                    level = e.Homeroom.Grade.Level.Description,
                                    levelCode = e.Homeroom.Grade.Level.Code
                                })
                                .Select(e => new
                                {
                                    e.Key.IdHomeroom,
                                    homeroom = $"{e.Key.gradeCode}{e.Key.classroomCode}",
                                    e.Key.IdLevel,
                                    level = e.Key.level,
                                    levelCode = e.Key.levelCode,
                                })
                                .ToListAsync(CancellationToken);

            if (!string.IsNullOrEmpty(param.IdHomeroom))
                listHomeroom = listHomeroom.Where(e => e.IdHomeroom == param.IdHomeroom).ToList();

            var listIdHomeroom = listHomeroom.Select(e => e.IdHomeroom).ToList();
            var listIdlevel = listHomeroom.Select(e => e.IdLevel).ToList();

            var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                               .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.ActiveStatus)
                               .Select(e => new
                               {
                                   e.IdStudent,
                                   e.StartDate,
                                   EndDate = e.EndDate == null
                                               ? param.Date.Date
                                               : Convert.ToDateTime(e.EndDate)
                               })
                               .ToListAsync(CancellationToken);

            var listIdStudent = listStudentStatus
                                .Where(e => e.StartDate.Date <= param.Date.Date && e.EndDate.Date >= param.Date.Date)
                                .Select(e => e.IdStudent).ToList();

            //var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
            //                    .Include(e => e.HomeroomStudent)
            //                    .Where(e => (listIdHomeroom.Contains(e.HomeroomStudent.IdHomeroom))
            //                        && listIdStudent.Contains(e.HomeroomStudent.IdStudent) && e.HomeroomStudent.Homeroom.Semester == param.Semester)
            //                    .GroupBy(e => new
            //                    {
            //                        e.IdLesson,
            //                        e.IdHomeroomStudent,
            //                        e.HomeroomStudent.IdHomeroom
            //                    })
            //                    .Select(e => new
            //                    {
            //                        e.Key.IdLesson,
            //                        e.Key.IdHomeroomStudent,
            //                        e.Key.IdHomeroom
            //                    })
            //                    .ToListAsync(CancellationToken);

            var listPeriod = await _dbContext.Entity<MsPeriod>()
                      .Where(e => e.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                      .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                               .Include(e => e.HomeroomStudent).ThenInclude(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                               .Where(e => listIdHomeroom.Contains(e.HomeroomStudent.IdHomeroom) && e.HomeroomStudent.Homeroom.IdAcademicYear == param.IdAcademicYear)
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
                                   IsFromMaster = true,
                                   IsDelete = false,
                                   IsShowHistory = false,
                               })
                               .ToListAsync(CancellationToken);

            listHomeroomStudentEnrollment.ForEach(e =>
            {
                e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
                e.Datein = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min();
            });

            var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                           .Include(e => e.SubjectNew)
                           .Include(e => e.LessonNew)
                           .Where(x => x.StartDate.Date <= _datetimeNow.ServerTime.Date && x.LessonOld.IdAcademicYear == param.IdAcademicYear && listIdHomeroom.Contains(x.HomeroomStudent.IdHomeroom))
                           .Select(e => new GetHomeroom
                           {
                               IdLesson = e.IdLessonNew,
                               IdStudent = e.HomeroomStudent.IdStudent,
                               IdHomeroomStudent = e.IdHomeroomStudent,
                               Homeroom = new ItemValueVm
                               {
                                   Id = e.HomeroomStudent.Homeroom.Id,
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
                               IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                               IsFromMaster = false,
                               EffectiveDate = e.StartDate,
                               IsDelete = e.IsDelete,
                               Datein = e.DateIn.Value,
                               IsShowHistory = e.IsShowHistory,
                           })
                           .ToListAsync(CancellationToken);

            var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                    .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                    .ToList();

            var listIdLessonByMoving = listStudentEnrollmentUnion.Select(e => e.IdLesson).Distinct().ToList();

            var queryScheduleLessonByMoving = _dbContext.Entity<MsScheduleLesson>()
                                                .Include(e => e.Lesson)
                                                .Include(e => e.Subject)
                                                .Include(e => e.Session)
                                                .Include(e => e.AcademicYear)
                                                .Where(e => e.IdAcademicYear == param.IdAcademicYear
                                                             && e.ScheduleDate.Date == param.Date.Date
                                                             && listIdLessonByMoving.Contains(e.IdLesson)
                                                             && e.Lesson.Semester==param.Semester
                                                        );

            var listScheduleLesoon = await queryScheduleLessonByMoving
                                           .ToListAsync(CancellationToken);


            var listIdLessonByScheduleLessonMoving = await queryScheduleLessonByMoving
                                           .Select(e => e.IdLesson)
                                           .ToListAsync(CancellationToken);

            //moving
            List<GetHomeroom> listStudentEnrollmentMoving = new List<GetHomeroom>();
            foreach (var item in listIdLessonByScheduleLessonMoving)
            {
                var getStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, Convert.ToDateTime(param.Date.Date), param.Semester.ToString(), item);
                listStudentEnrollmentMoving.AddRange(getStudentEnrollmentMoving);
            }

            var listIdScheduleLesson = listScheduleLesoon.Select(e => e.Id).ToList();

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                .Include(e => e.HomeroomStudent)
                                .Include(e => e.ScheduleLesson)
                                .Include(e => e.AttendanceMappingAttendance).ThenInclude(e => e.Attendance)
                                .Where(e => listIdScheduleLesson.Contains(e.IdScheduleLesson)
                                        && (e.Status == AttendanceEntryStatus.Submitted || e.Status == AttendanceEntryStatus.Pending)
                                        && e.ScheduleLesson.ScheduleDate.Date == param.Date.Date
                                        && listIdStudent.Contains(e.HomeroomStudent.IdStudent)
                                        && listIdHomeroom.Contains(e.HomeroomStudent.IdHomeroom)
                                        )
                                .Select(e => new
                                {
                                    e.IdScheduleLesson,
                                    e.DateIn,
                                    e.UserIn,
                                    e.UserUp,
                                    e.IdHomeroomStudent,
                                    e.AttendanceMappingAttendance.Attendance.AbsenceCategory,
                                    e.HomeroomStudent.IdHomeroom
                                })
                                .ToListAsync(CancellationToken);

            var listMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                .Where(e => listIdlevel.Contains(e.IdLevel))
                                .Select(e => new
                                {
                                    e.IdLevel,
                                    e.AbsentTerms
                                })
                                .ToListAsync(CancellationToken);

            List<HomeroomAttendanceV2Result> result = new List<HomeroomAttendanceV2Result>();
            for (var i = 0; i < listHomeroom.Count(); i++)
            {
                var data = listHomeroom[i];
                var listStudent = listStudentEnrollmentMoving.Where(e => e.Homeroom.Id == data.IdHomeroom && listIdStudent.Contains(e.IdStudent)).Select(e => e.IdHomeroomStudent).Distinct().ToList();

                var term = listMappingAttendance.Where(e => e.IdLevel == data.IdLevel).SingleOrDefault();
                if (term == null)
                    throw new BadRequestException($"Absent term with id level = {data.IdLevel} is not found");

                var listIdScheduleLesoon = listScheduleLesoon.Select(e => e.Id).Distinct().ToList();

                var listAttendanceEntryByHomeroom = listAttendanceEntry
                                .Where(e => e.IdHomeroom == data.IdHomeroom && listStudent.Contains(e.IdHomeroomStudent))
                                .Select(e => e.IdHomeroomStudent).Distinct().ToList();

                var unsubmited = listStudent.Count() - listAttendanceEntryByHomeroom.Count();

                var lastBy = string.Empty;
                var lastAt = new DateTime?();
                var lastByIdUser = string.Empty;
                if (unsubmited != listStudent.Count())
                {
                    var lastByIdUserData = listAttendanceEntry
                                    .Where(e => listIdScheduleLesoon.Contains(e.IdScheduleLesson)
                                    && e.UserIn == param.IdUser
                                    )
                                    .OrderByDescending(e => e.DateIn)
                                    .Select(e => new
                                    {
                                        idUser = e.UserUp ?? e.UserIn
                                    }).FirstOrDefault();

                    if (lastByIdUserData != null)
                        lastByIdUser = lastByIdUserData.idUser;

                    lastAt = listAttendanceEntry
                                .Where(e => listIdScheduleLesoon.Contains(e.IdScheduleLesson)
                                && e.UserIn == param.IdUser
                                )
                                .OrderByDescending(e => e.DateIn)
                                .Select(e => e.DateIn)
                                .Max();

                    if (string.IsNullOrEmpty(lastByIdUser))
                    {
                        lastByIdUserData = listAttendanceEntry
                                        .Where(e => listIdScheduleLesoon.Contains(e.IdScheduleLesson))
                                        .OrderByDescending(e => e.DateIn)
                                        .Select(e => new
                                        {
                                            idUser = e.UserUp ?? e.UserIn
                                        }).FirstOrDefault();

                        if (lastByIdUserData != null)
                            lastByIdUser = lastByIdUserData.idUser;

                        lastAt = listAttendanceEntry
                                   .Where(e => listIdScheduleLesoon.Contains(e.IdScheduleLesson))
                                   .OrderByDescending(e => e.DateIn)
                                   .Select(e => e.DateIn)
                                   .Max();
                    }

                    lastBy = await _dbContext.Entity<MsUser>()
                                        .Where(e => e.Id == lastByIdUser)
                                        .Select(e => e.DisplayName)
                                        .FirstOrDefaultAsync(CancellationToken);
                }

                #region UnexcudesAbsen
                var listUserEntry = new List<string>();

                var listHomeRoomTeacher = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Where(e => e.IdHomeroom == data.IdHomeroom)
                                .Select(e => e.IdBinusian)
                                .Distinct()
                                .ToListAsync(CancellationToken);

                if (listHomeRoomTeacher.Any(idBinusian => idBinusian == param.IdUser))
                {
                    listUserEntry.AddRange(listHomeRoomTeacher);
                }
                else
                {
                    listUserEntry.Add(param.IdUser);
                }

                var listAttendanceEntryUnexcudesAbsen = listAttendanceEntry
                                                        .Where(e =>
                                                            listIdScheduleLesoon.Contains(e.IdScheduleLesson) &&
                                                            e.AbsenceCategory == AbsenceCategory.Unexcused &&
                                                            listUserEntry.Contains(e.UserIn)
                                                            )
                                                        .Select(e => e.IdHomeroomStudent)
                                                        .Distinct()
                                                        .ToList();

                var unexcusedAbsence = listAttendanceEntryUnexcudesAbsen.Count();
                #endregion

                if (listScheduleLesoon.Any())
                {
                    result.Add(new HomeroomAttendanceV2Result
                    {
                        Level = new CodeWithIdVm
                        {
                            Id = data.IdLevel,
                            Code = data.levelCode,
                            Description = data.level
                        },
                        Homeroom = new ItemValueVm
                        {
                            Id = data.IdHomeroom,
                            Description = data.homeroom
                        },
                        TotalStudent = listStudent.Count(),
                        Unsubmitted = unsubmited,
                        UnexcusedAbsence = unexcusedAbsence,
                        LastSavedBy = lastBy,
                        LastSavedAt = lastAt,
                    });
                }
            }

            return Request.CreateApiResult2(result as object);
        }
    }
}
