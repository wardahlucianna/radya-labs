using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;
using DocumentFormat.OpenXml.Drawing;
using FluentEmail.Core;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class GetEmergencyAttendancePrivilegeHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public GetEmergencyAttendancePrivilegeHandler(IAttendanceDbContext dbContext,
         IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetEmergencyAttendancePrivilegeRequest>(nameof(GetEmergencyAttendancePrivilegeRequest.IdAcademicYear),nameof(GetEmergencyAttendancePrivilegeRequest.IdUser));

    
            var periodActived = await _dbContext.Entity<MsPeriod>()
                                .Include(x => x.Grade)
                                    .ThenInclude(y => y.Level)
                                    .ThenInclude(y => y.AcademicYear)
                                .Where(a => a.Grade.Level.IdAcademicYear == param.IdAcademicYear
                                && a.StartDate < _dateTime.ServerTime && _dateTime.ServerTime < a.EndDate)
                                .FirstOrDefaultAsync();

            DateTime getDateTime = _dateTime.ServerTime;

            //DateTime getDateTime = DateTime.ParseExact("2023-07-12 00:00:00", "yyyy-MM-dd HH:mm:ss",
            //                           System.Globalization.CultureInfo.InvariantCulture);

            if (periodActived == null)
            {     
                periodActived = new MsPeriod() { Semester = 2 };
                //throw new BadRequestException("Period active not found");
            }

            GetEmergencyAttendancePrivilegeResult returnresult = new GetEmergencyAttendancePrivilegeResult();
            List<ItemValueVm> AllGrade = new List<ItemValueVm>();

            var getHomeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(x => x.TeacherPosition)
                                .Include(x => x.Homeroom)
                                    .ThenInclude(y => y.Grade)
                                    .ThenInclude(y => y.Level)
                                .Include(x => x.Homeroom)
                                    .ThenInclude(y => y.GradePathwayClassroom)
                                    .ThenInclude(y => y.Classroom)
                                .Where(a => a.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear
                                && a.Homeroom.Semester == periodActived.Semester
                                && a.IdBinusian == param.IdUser)
                                .Select(a => new 
                                {
                                    Id = a.IdHomeroom,
                                    Description = a.Homeroom.Grade.Code + "" + a.Homeroom.GradePathwayClassroom.Classroom.Code,
                                    IdGrade = a.Homeroom.Grade.Id,
                                    GradeName = a.Homeroom.Grade.Description
                                })
                                .ToListAsync();
            if (getHomeroom.Count > 0)
            {
                returnresult.Homeroom = getHomeroom.Select(a => new ItemValueVm
                {
                    Id = a.Id,
                    Description = a.Description
                }).FirstOrDefault();

                var getgrade = getHomeroom.Select(a => new ItemValueVm
                {
                    Id = a.IdGrade,
                    Description = a.GradeName
                }).ToList();
                if(getgrade.Count > 0)
                {
                    AllGrade.AddRange(getgrade);
                }

            }

            List<CodeWithIdVm> SessionList = new List<CodeWithIdVm>();
            var getScheduleLesson = await _dbContext.Entity<MsScheduleLesson>()
                                        .Include(x => x.Lesson)
                                            .ThenInclude(y=> y.LessonTeachers)
                                        .Include(x => x.Grade)
                                        .Where(a => a.ScheduleDate.Date == getDateTime.Date
                                        && a.Lesson.LessonTeachers.Any(b => b.IdUser == param.IdUser)
                                        && a.IdAcademicYear == param.IdAcademicYear
                                        && a.IsGenerated == true)
                                        .Select(a => new
                                        {
                                            IdScheduleLesson = a.Id,
                                            SessionID = a.SessionID,
                                            StartTime = a.StartTime,
                                            EndTime = a.EndTime,
                                            SubjectName = a.SubjectName,
                                            IdGrade = a.IdGrade,
                                            GradeCode = a.Grade.Code,
                                            GradeName = a.Grade.Description
                                        })
                                        .ToListAsync(CancellationToken);

            if(getScheduleLesson.Count > 0)
            {
                SessionList.AddRange(getScheduleLesson.Select(a => new CodeWithIdVm
                {
                    Id = a.IdScheduleLesson,
                    Code = "("+ a.StartTime.ToString(@"hh\:mm") + " - " + a.EndTime.ToString(@"hh\:mm") +")",
                    Description = a.GradeCode + " " + a.SubjectName,
                }).ToList());


                var getgrade = getScheduleLesson.Select(a => new ItemValueVm
                {
                    Id = a.IdGrade,
                    Description = a.GradeName
                }).ToList();
                if (getgrade.Count > 0)
                {
                    AllGrade.AddRange(getgrade);
                }
            }

            var getScheduleRealization = await _dbContext.Entity<TrScheduleRealization2>()
                                        .Where(a => a.ScheduleDate.Date == getDateTime.Date
                                        && a.IdBinusianSubtitute == param.IdUser
                                        && a.IsCancel == false
                                        && a.IdAcademicYear == param.IdAcademicYear)
                                        .Select(a => new
                                        {
                                            a.ScheduleDate,
                                            a.SessionID,
                                        
                                        })
                                        .ToListAsync(CancellationToken);

            if(getScheduleRealization.Count() > 0)
            {
                var getScheduleLessonbySubtitute = await _dbContext.Entity<MsScheduleLesson>()
                                        .Include(x => x.Grade)
                                        .Where(a => a.ScheduleDate.Date == getDateTime.Date
                                        && getScheduleRealization.Select(b => b.SessionID).Contains(a.SessionID))
                                        .Select(a => new
                                        {
                                            IdScheduleLesson = a.Id,
                                            SessionID = a.SessionID,
                                            StartTime = a.StartTime,
                                            EndTime = a.EndTime,
                                            SubjectName = a.SubjectName,
                                            IdGrade = a.IdGrade,
                                            GradeCode = a.Grade.Code,
                                            GradeName = a.Grade.Description
                                        })
                                        .ToListAsync(CancellationToken);

                if (getScheduleLessonbySubtitute.Count > 0)
                {
                    SessionList.AddRange(getScheduleLessonbySubtitute.Select(a => new CodeWithIdVm
                    {
                        Id = a.IdScheduleLesson,
                        Code = "(" + a.StartTime.ToString(@"hh\:mm") + " - " + a.EndTime.ToString(@"hh\:mm") + ")",
                        Description = a.GradeCode + " " + a.SubjectName,
                    }).ToList());

                    var getgrade = getScheduleLessonbySubtitute.Select(a => new ItemValueVm
                    {
                        Id = a.IdGrade,
                        Description = a.GradeName
                    }).ToList();
                    if (getgrade.Count > 0)
                    {
                        AllGrade.AddRange(getgrade);
                    }
                }
            }

            if(SessionList.Count > 0)
            {
                returnresult.Sessions = SessionList.OrderBy(a => a.Code).Distinct().ToList();
            }
            if(AllGrade.Count > 0)
            {
                returnresult.Grades = AllGrade.GroupBy(a => new { a.Id, a.Description}).Select(a => new ItemValueVm { Id = a.Key.Id, Description = a.Key.Description }).ToList();
            }

            return Request.CreateApiResult2(returnresult as object);
        }
    }
}
