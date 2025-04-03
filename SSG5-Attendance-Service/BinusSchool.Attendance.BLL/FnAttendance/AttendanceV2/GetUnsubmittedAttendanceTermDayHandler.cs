using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2
{
    public class GetUnsubmittedAttendanceTermDayHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;

        public GetUnsubmittedAttendanceTermDayHandler(IAttendanceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetUnresolvedAttendanceV2Request>();

            var listIdLesson = await _dbContext.Entity<MsLessonTeacher>()
                            .Include(e => e.Lesson)
                            .Where(e => e.IdUser == param.IdUser
                                    && e.Lesson.IdAcademicYear == param.IdAcademicYear)
                            .Select(e => e.IdLesson)
                            .ToListAsync(CancellationToken);

            var listStaudetStatus = await _dbContext.Entity<TrStudentStatus>()
                                .Where(e => e.IdAcademicYear == param.IdAcademicYear && e.ActiveStatus)
                                .ToListAsync(CancellationToken);

            var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                                .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                .Where(e => listIdLesson.Contains(e.IdLesson))
                                .GroupBy(e => new GetHomeroom
                                {
                                    IdLesson = e.IdLesson,
                                    IdHomeroomStudent = e.IdHomeroomStudent,
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
                                    }
                                })
                                .Select(e => new GetHomeroom
                                {
                                    IdLesson = e.Key.IdLesson,
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
                                    }
                                })
                                .ToListAsync(CancellationToken);

            var listIdlevel = listHomeroomStudentEnrollment.Select(e => e.Level.Id).Distinct().ToList();

            var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                               .Include(e => e.ScheduleLesson)
                               .Where(e => e.ScheduleLesson.IdAcademicYear == param.IdAcademicYear)
                               .ToListAsync(CancellationToken);

            var listScheduleLesoon = await _dbContext.Entity<MsScheduleLesson>()
                                .Include(e => e.Subject)
                                .Include(e => e.Session)
                                .Where(e => listIdLesson.Contains(e.IdLesson)
                                        && e.IdAcademicYear == param.IdAcademicYear
                                        )
                                .ToListAsync(CancellationToken);

            List<UnresolvedAttendanceGroupV2Result> attendance = new List<UnresolvedAttendanceGroupV2Result>();
            for (var i = 0; i < listScheduleLesoon.Count(); i++)
            {
                var data = listScheduleLesoon[i];

                var listStatusStudentByDate = listStaudetStatus.Where(e => e.StartDate.Date <= data.ScheduleDate.Date).ToList();
                var listStudentEnrolmentBySchedule = listHomeroomStudentEnrollment.Where(e => e.IdLesson == data.IdLesson).Select(e => e.IdHomeroomStudent).ToList();
                var listAttendanceEntryBySchedule = listAttendanceEntry.Where(e => e.IdScheduleLesson == data.Id).Select(e => e.IdHomeroomStudent).ToList();
                var HomeroomBySchedule = listHomeroomStudentEnrollment.Where(e => e.IdLesson == data.IdLesson).Select(e => e.Homeroom.Id).FirstOrDefault();
                var homeroom = listHomeroomStudentEnrollment.Where(e => e.IdLesson == data.IdLesson).FirstOrDefault();

                if (listAttendanceEntryBySchedule.Any())
                {
                    var studentExcludeEnrollment = listStudentEnrolmentBySchedule
                                                   .Where(e => !listAttendanceEntryBySchedule
                                                   .Contains(e))
                                                   .ToList();

                    if (!studentExcludeEnrollment.Any())
                        attendance.Add(new UnresolvedAttendanceGroupV2Result
                        {
                            Date = data.ScheduleDate.Date,
                            ClassID = data.ClassID,
                            Homeroom = new ItemValueVm
                            {
                                Id = homeroom.Homeroom.Id,
                                Description = homeroom.Grade.Code + homeroom.ClassroomCode
                            },
                            Session = new ItemValueVm
                            {
                                Id = data.Session.Id,
                                Description = data.Session.Name,
                            },
                            TotalStudent = listStudentEnrolmentBySchedule.Count(),
                        });
                }
                else
                {
                    attendance.Add(new UnresolvedAttendanceGroupV2Result
                    {
                        Date = data.ScheduleDate.Date,
                        ClassID = data.ClassID,
                        Homeroom = new ItemValueVm
                        {
                            Id = homeroom.Homeroom.Id,
                            Description = homeroom.Grade.Code + homeroom.ClassroomCode
                        },
                        Session = new ItemValueVm
                        {
                            Id = data.Session.Id,
                            Description = data.Session.Name,
                        },
                        TotalStudent = listStudentEnrolmentBySchedule.Count(),
                    });
                }
            }

            var result = new GetUnresolvedAttendanceV2Result
            {
                IsShowingPopup = attendance.Any(),
                Attendances = attendance.GroupBy(e => new
                                {
                                    e.Date,
                                    e.Homeroom,
                                    e.TotalStudent,
                                })
                                .Select(e => new UnresolvedAttendanceGroupV2Result
                                {
                                    Date = e.Key.Date,
                                    Homeroom = e.Key.Homeroom,
                                    TotalStudent = e.Key.TotalStudent,
                                }).ToList(),
            };

            return Request.CreateApiResult2(result as object);
        }
    }
}
