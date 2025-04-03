using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetStudentAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetStudentAttendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetStudentAttendanceRequest>(
                            nameof(GetStudentAttendanceRequest.IdAcademicYear),
                            nameof(GetStudentAttendanceRequest.Semester),
                            nameof(GetStudentAttendanceRequest.IdExtracurricular),
                            nameof(GetStudentAttendanceRequest.Month));

            var result = new GetStudentAttendanceResult();

            var extracurricular = await _dbContext.Entity<MsExtracurricular>()
                                        .Where(x => x.Id == param.IdExtracurricular &&
                                                    x.Semester == param.Semester)
                                        .FirstOrDefaultAsync(CancellationToken);

            if(extracurricular == null)
            {
                throw new BadRequestException("Error: Extracurricular is not exists");
            }
            else
            {
                // Check if the extracurricular is active for attendance in report card or no
                bool isShowAttendanceRC = extracurricular.ShowAttendanceRC;

                // Get spv and coach
                var spvCoachList = _dbContext.Entity<MsExtracurricularSpvCoach>()
                                    .Include(esc => esc.Extracurricular)
                                    .Include(esc => esc.Staff)
                                    .Where(x => x.Extracurricular.Id == param.IdExtracurricular &&
                                                x.Extracurricular.Semester == param.Semester)
                                    .Select(x => new
                                    {
                                        spvCoach = new NameValueVm
                                        {
                                            Id = x.Staff.IdBinusian,
                                            Name = NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)
                                        },
                                        isSpv = (x.ExtracurricularCoachStatus.Code == "SPV") //x.IsSpv
                                    });

                var supervisorList = spvCoachList.Where(x => x.isSpv == true).ToList();
                var supervisor = supervisorList.Select(x => x.spvCoach).OrderBy(x => x.Name).ToList();
                var coachList = spvCoachList.Where(x => x.isSpv == false).ToList();
                var coach = coachList.Select(x => x.spvCoach).OrderBy(x => x.Name).ToList();

                //var bodyCount = 0;
                //if (isShowAttendanceRC)
                if (true)
                {
                    // Get session
                    var sessionList = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                        .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                                        .Distinct()
                                        .ToList()
                                        .OrderBy(x => x.Date)
                                        .Select((x, index) => new { Data = x, index })
                                        .ToList();

                    // all month
                    var currentMonthSessionList = sessionList
                                                    .Where(x => param.Month == Month.All ? true == true : x.Data.Date.Month == (int)param.Month)
                                                    .OrderBy(x => x.Data.Date)
                                                    .Distinct()
                                                    .ToList();

                    var currentMonthSessionIdList = currentMonthSessionList.Select(x => x.Data.Id).ToList();

                    // Get max session count
                    var maxSession = sessionList.Count();

                    // Get current month session count
                    var totalSessionCurrentMonth = currentMonthSessionList.Count();

                    // get student participant
                    var participantList = _dbContext.Entity<MsExtracurricularParticipant>()
                                            .Include(ep => ep.Student)
                                            .Include(ep => ep.Extracurricular)
                                            .Where(x => x.IdExtracurricular == param.IdExtracurricular && x.Status == true
                                                && x.Extracurricular.Semester == param.Semester)
                                            .Join(_dbContext.Entity<MsHomeroomStudent>()
                                                .Include(hs => hs.Homeroom)
                                                .ThenInclude(h => h.GradePathwayClassroom)
                                                .ThenInclude(gpc => gpc.Classroom)
                                                .Include(hs => hs.Homeroom)
                                                .ThenInclude(h => h.Grade)
                                            .Where(x => x.Homeroom.Grade.Level.IdAcademicYear == param.IdAcademicYear),
                                                extracurricular => new { p1 = extracurricular.Student.Id, p2 = extracurricular.IdGrade, p3 = extracurricular.Extracurricular.Semester },
                                                homeroom => new { p1 = homeroom.IdStudent, p2 = homeroom.Homeroom.IdGrade, p3 = homeroom.Homeroom.Semester },
                                                (extracurricular, homeroom) => new
                                                {
                                                    student = new NameValueVm
                                                    {
                                                        Id = extracurricular.Student.Id,
                                                        Name = NameUtil.GenerateFullName(extracurricular.Student.FirstName, extracurricular.Student.LastName)
                                                    },
                                                    homeroom = new NameValueVm
                                                    {
                                                        Id = homeroom.Homeroom.Id,
                                                        Name = string.Format("{0}{1}", homeroom.Homeroom.Grade.Code, homeroom.Homeroom.GradePathwayClassroom.Classroom.Code)
                                                    }
                                                })
                                            .Select(x => x)
                                            .Distinct()
                                            .ToList();

                    participantList = participantList.OrderBy(x => x.student.Name).ThenBy(x => x.homeroom.Name).ToList();

                    var UserUpdateList = await _dbContext.Entity<MsUser>()
                                    .Where(a => currentMonthSessionList.Select(c => c.Data.UserUp).Contains(a.Id))
                                    .ToListAsync();

                    // Header
                    var headerList = new List<GetStudentAttendanceResult_Header>();
                    foreach (var session in currentMonthSessionList)
                    {
                        var UserUpdate = UserUpdateList.Where(a => a.Id == session.Data.UserUp).FirstOrDefault();
                        var header = new GetStudentAttendanceResult_Header
                        {
                            ExtracurricularGeneratedAtt = new NameValueVm
                            {
                                Id = session.Data.Id,
                                Name = "Session " + (session.index + 1)
                            },
                            ExtracurricularGeneratedDate = session.Data.Date,
                            LastUpdated = (session.Data.UserUp != null ? ((UserUpdate != null ? UserUpdate.DisplayName : session.Data.UserUp) + ", "+ ((DateTime)session.Data.DateUp).ToString("dd-MM-yyyy HH:mm") ) : "")
                        };

                        headerList.Add(header);
                    }

                    // Body
                    var bodyList = new List<GetStudentAttendanceResult_Body>();

                    var JoinDateStudentList = await _dbContext.Entity<MsExtracurricularParticipant>()
                                            .Where(a => participantList.Select(b => b.student.Id).Contains(a.IdStudent)
                                            && a.IdExtracurricular == param.IdExtracurricular)
                                            .ToListAsync();

                    foreach (var participant in participantList)
                    {
                        var JoinDateStudent = JoinDateStudentList.Where(a => a.IdStudent == participant.student.Id).FirstOrDefault();
                        
                        var sessionAttendanceList = currentMonthSessionList
                                                    .GroupJoin(
                                                            _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                                                                .Include(eae => eae.ExtracurricularGeneratedAtt)
                                                                .Include(eae => eae.ExtracurricularStatusAtt)
                                                                .Where(x => x.IdStudent == participant.student.Id &&
                                                                            currentMonthSessionIdList.Contains(x.ExtracurricularGeneratedAtt.Id) &&
                                                                            x.ExtracurricularGeneratedAtt.IdExtracurricular == param.IdExtracurricular)
                                                                ,
                                                            s => s.Data.Id,
                                                            a => a.IdExtracurricularGeneratedAtt,
                                                            (s, a) => new { s, a })
                                                    .SelectMany(
                                                            x => x.a.DefaultIfEmpty(),
                                                            (session, attendance) => new GetStudentAttendanceResult_SessionAttendance
                                                            {
                                                                IdExtracurricularGeneratedAtt = session.s.Data.Id,
                                                                ExtracurricularGeneratedAttDate = session.s.Data.Date,
                                                                ExtracurricularStatusAtt = new NameValueVm
                                                                {
                                                                    Id = attendance?.ExtracurricularStatusAtt.Id,
                                                                    Name = attendance?.ExtracurricularStatusAtt.Description
                                                                },
                                                                NeedReason = attendance?.ExtracurricularStatusAtt.NeedReason,
                                                                Reason = attendance?.Reason,
                                                                NoNeedAttendance = (JoinDateStudent == null ? false : session.s.Data.Date.Date <= JoinDateStudent.JoinDate.Date)
                                                            })
                                                    .ToList();

                        var body = new GetStudentAttendanceResult_Body
                        {
                            Student = new NameValueVm
                            {
                                Id = participant.student.Id,
                                Name = participant.student.Name
                            },
                            Homeroom = new NameValueVm
                            {
                                Id = participant.homeroom.Id,
                                Name = participant.homeroom.Name
                            },
                            SessionAttendanceList = sessionAttendanceList,
                            JoinElectiveDate = JoinDateStudent.JoinDate
                        };

                        bodyList.Add(body);
                    }

                    //var bodyItems = bodyList.AsQueryable().OrderByDynamic(param).SetPagination(param).ToList();
                    //bodyCount = bodyList.Count();

                    result = new GetStudentAttendanceResult
                    {
                        Extracurricular = new NameValueVm
                        {
                            Id = extracurricular.Id,
                            Name = extracurricular.Name
                        },
                        IsRegularSchedule = extracurricular.IsRegularSchedule,
                        // Supervisor = supervisor?.spvCoach,
                        Supervisor = supervisor,
                        Coach = coach.Count() == 0 ? null : coach,
                        TotalSessionCurrentMonth = totalSessionCurrentMonth,
                        MaxSession = maxSession,
                        ElectivesStartDate = extracurricular.ElectivesStartDate,
                        ElectivesEndDate = extracurricular.ElectivesEndDate,
                        AttendanceStartDate = extracurricular.AttendanceStartDate,
                        AttendanceEndDate = extracurricular.AttendanceEndDate,
                        HeaderList = headerList,
                        BodyList = bodyList
                    };
                }
                else
                {
                    result = new GetStudentAttendanceResult
                    {
                        Extracurricular = new NameValueVm
                        {
                            Id = extracurricular.Id,
                            Name = extracurricular.Name
                        },
                        IsRegularSchedule = extracurricular.IsRegularSchedule,
                        Supervisor = supervisor,
                        Coach = coach.Count() == 0 ? null : coach,
                        TotalSessionCurrentMonth = 0,
                        MaxSession = 0,
                        AttendanceStartDate = null,
                        AttendanceEndDate = null,
                        HeaderList = null,
                        BodyList = null
                    };
                }

                return Request.CreateApiResult2(result as object);
            }
        }
    }
}
