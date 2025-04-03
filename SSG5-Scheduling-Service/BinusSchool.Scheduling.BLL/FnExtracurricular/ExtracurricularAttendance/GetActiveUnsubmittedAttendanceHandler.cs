using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetActiveUnsubmittedAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetActiveUnsubmittedAttendanceHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetActiveUnsubmittedAttendanceRequest, GetActiveUnsubmittedAttendanceValidator>();

            var School = _dbContext.Entity<MsSchool>()
                               .Where(a => a.Id == param.IdSchool)
                               .FirstOrDefault();
            #region Get Active AY
            var getActiveAcademicYear = await _dbContext.Entity<MsPeriod>()
                                        .Include(x => x.Grade)
                                        .ThenInclude(y => y.Level)
                                        .ThenInclude(z => z.AcademicYear)
                                        .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && _dateTime.ServerTime.Date <= x.EndDate.Date)
                                        .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                                        .OrderByDescending(x => x.StartDate)
                                        .Select(x => new
                                        {
                                            AcademicYear = new ItemValueVm
                                            {
                                                Id = x.Grade.Level.AcademicYear.Id,
                                                Description = x.Grade.Level.AcademicYear.Description
                                            },
                                            AcademicYearCode = x.Grade.Level.AcademicYear.Code,
                                            Semester = x.Semester,
                                        })
                                        .FirstOrDefaultAsync();
            #endregion

            var idAcademicYear = getActiveAcademicYear.AcademicYear.Id;
            //var semester = getActiveAcademicYear.Semester;
            bool IsACOP = false;

            var CheckUserACOP = await _dbContext.Entity<MsUserRole>()
                            .Include(x => x.Role)
                            .Where(a => a.IdUser == param.IdUser
                            && a.Role.Code.ToLower().Contains("acop"))
                            .FirstOrDefaultAsync();

            if (CheckUserACOP != null)
            {
                IsACOP = true;
            }

            var getextracurricularIdList = _dbContext.Entity<MsExtracurricularSpvCoach>()
                            .Include(esc => esc.Extracurricular)
                            .Where(x => x.Extracurricular.ShowAttendanceRC == true && x.Extracurricular.Status == true);

            if (getextracurricularIdList.Where(x => x.IdBinusian == param.IdUser || IsACOP).ToList().Count() > 0)
            {
                getextracurricularIdList = getextracurricularIdList.Where(x => x.IdBinusian == param.IdUser || IsACOP);
            }

            var extracurricularIdList = getextracurricularIdList
                             .Join(_dbContext.Entity<TrExtracurricularGradeMapping>()
                                         .Include(x => x.Grade)
                                         .ThenInclude(x => x.Level)
                                         .ThenInclude(x => x.AcademicYear)
                                         .Where(x => x.Grade.Level.AcademicYear.Id == idAcademicYear),
                                     extracurricular => extracurricular.IdExtracurricular,
                                     grade => grade.IdExtracurricular,
                                     (extracurricular, grade) => extracurricular.IdExtracurricular)
                             .Select(x => x)
                             .Distinct()
                             .ToList();

            var attendanceSessionList = _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                                           .Include(ega => ega.ExtracurricularSession)
                                           .ThenInclude(es => es.Day)
                                           .Include(ega => ega.Extracurricular).ThenInclude(x => x.ExtracurricularGradeMappings)
                                           .Where(x => extracurricularIdList.Contains(x.IdExtracurricular) &&
                                                       x.Date <= _dateTime.ServerTime.Date)
                                           .Join(_dbContext.Entity<MsExtracurricularSpvCoach>()
                                                       .Include(esc => esc.Staff)
                                                       .Include(y => y.ExtracurricularCoachStatus)
                                                       .Where(x => extracurricularIdList.Any(y => y == x.IdExtracurricular) &&
                                                                   (x.IsSpv == true || x.ExtracurricularCoachStatus.Code == "SPV"))
                                                       ,
                                                   session => session.IdExtracurricular,
                                                   supervisor => supervisor.IdExtracurricular,
                                                   (session, supervisor) => new
                                                   {
                                                       idExtracurricularGeneratedAtt = session.Id,
                                                       extracurricular = new NameValueVm
                                                       {
                                                           Id = session.Extracurricular.Id,
                                                           Name = session.Extracurricular.Name
                                                       },
                                                       supervisor = new NameValueVm
                                                       {
                                                           Id = supervisor.Staff.IdBinusian,
                                                           Name = NameUtil.GenerateFullName(supervisor.Staff.FirstName, supervisor.Staff.LastName)
                                                       },
                                                       day = session.ExtracurricularSession.Day.Description,
                                                       date = session.Date,
                                                       semester = session.Extracurricular.Semester,
                                                       grade = string.Join(";", session.Extracurricular.ExtracurricularGradeMappings.Select(a => a.Grade.Description)),
                                                   })
                                                   .OrderBy(x => x.extracurricular.Name)
                                           .Select(x => x)
                                           .ToList();

            attendanceSessionList = attendanceSessionList
                .GroupBy(x => x)
                .Select(x => x.FirstOrDefault())
                .ToList();


            var studentAttendanceList = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                .Include(x => x.ExtracurricularGeneratedAtt)
                    .ThenInclude(x => x.Extracurricular)
                .Include(x => x.ExtracurricularStatusAtt)
                .Where(x => attendanceSessionList.Select(y => y.idExtracurricularGeneratedAtt).Contains(x.IdExtracurricularGeneratedAtt))
                .ToListAsync(CancellationToken);

            var groupSessionList = attendanceSessionList
               .GroupBy(x => new
               {
                   x.extracurricular.Name,
                   x.extracurricular.Id,
                   x.semester,
                   x.day,
                   x.date,
                   x.idExtracurricularGeneratedAtt,
                   x.grade
               })
               .Select(x => new
               {
                   extracurricular = new NameValueVm
                   {
                       Id = x.Key.Id,
                       Name = x.Key.Name
                   },
                   x.Key.semester,
                   x.Key.day,
                   x.Key.date,
                   supervisorCoach = string.Join("; ", x.Select(x => x.supervisor.Name)),
                   x.Key.idExtracurricularGeneratedAtt,
                   x.Key.grade
               })
               .Distinct().ToList();

            var getExtracurricularParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                .Include(x => x.Extracurricular)
                .Include(x => x.Student)
                .Where(x => groupSessionList.Select(x => x.extracurricular.Id).Contains(x.IdExtracurricular) && x.Status == true)
                .ToListAsync(CancellationToken);

            var unsubmittedList = new List<GetUnsubmittedAttendanceResult_UnsubmittedList>();

            foreach (var sessionData in groupSessionList)
            {
                var filterExtracurricularParticipant = getExtracurricularParticipant
                    .Where(x => x.IdExtracurricular == sessionData.extracurricular.Id)
                    .Where(x => x.JoinDate <= sessionData.date)
                    .ToList();

                var unsubmittedParticipantCount = filterExtracurricularParticipant
                            .GroupJoin(studentAttendanceList
                            .Where(x => x.IdExtracurricularGeneratedAtt == sessionData.idExtracurricularGeneratedAtt),
                                p => p.IdStudent,
                                a => a.IdStudent,
                                (p, a) => new { p, a })
                            .SelectMany(
                                x => x.a.DefaultIfEmpty(),
                                (participant, attendace) => new { participant, attendace })
                            .Where(x => x.attendace == null)
                            .Select(x => x.participant?.p.IdStudent)
                            .Distinct()
                            .Count();

                if (unsubmittedParticipantCount != 0)
                {

                    var unsubmittedAttendance = new GetUnsubmittedAttendanceResult_UnsubmittedList
                    {
                        Supervisor = sessionData.supervisorCoach,
                        Extracurricular = sessionData.extracurricular,
                        ExtracurricularDate = sessionData.date,
                        ExtracurricularDay = sessionData.day,
                        Grade = sessionData.grade,
                        TotalUnsubmittedStudent = filterExtracurricularParticipant.Count(),
                        Semester = sessionData.semester
                    };

                    unsubmittedList.Add(unsubmittedAttendance);
                }
            };

            var resultUnsubmiited = unsubmittedList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                resultUnsubmiited = resultUnsubmiited.Where(a => a.Grade.Contains(param.Search)
                                                            || a.Extracurricular.Name.Contains(param.Search));
            }


            if (!string.IsNullOrEmpty(param.OrderBy))
            {
                resultUnsubmiited = param.OrderBy switch
                {
                    "Supervisor" => param.OrderType == OrderType.Asc
                        ? resultUnsubmiited.OrderBy(x => x.Supervisor)
                        : resultUnsubmiited.OrderByDescending(x => x.Supervisor),
                    "Extracurricular" => param.OrderType == OrderType.Asc
                        ? resultUnsubmiited.OrderBy(x => x.Extracurricular.Name)
                        : resultUnsubmiited.OrderByDescending(x => x.Extracurricular.Name),
                    "ExtracurricularDay" => param.OrderType == OrderType.Asc
                        ? resultUnsubmiited.OrderBy(x => x.ExtracurricularDay)
                        : resultUnsubmiited.OrderByDescending(x => x.ExtracurricularDay),
                    "TotalUnsubmittedStudent" => param.OrderType == OrderType.Asc
                        ? resultUnsubmiited.OrderBy(x => x.TotalUnsubmittedStudent)
                        : resultUnsubmiited.OrderByDescending(x => x.TotalUnsubmittedStudent),
                    _ => resultUnsubmiited.OrderBy(x => x.Grade)
                };
            }

            var unsubmittedItems = resultUnsubmiited.OrderByDynamic(param).SetPagination(param).ToList();
            var unsubmittedCount = resultUnsubmiited.Count();

            var totalSupervisor = resultUnsubmiited.Select(x => x.Supervisor).ToList().SelectMany(x => x.Split("; ")).Distinct().Count();


            GetActiveUnsubmittedAttendanceResult result = new GetActiveUnsubmittedAttendanceResult
            {
                TotalSupervisor = totalSupervisor,
                UnsubmittedAttendanceList = unsubmittedItems
            };

            return Request.CreateApiResult2(result as object, param.CreatePaginationProperty(unsubmittedCount));
        }
    }
}
