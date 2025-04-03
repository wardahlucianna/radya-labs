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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.PrivilegeUserElective;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.PrivilegeUserElective;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularScoreEntryHandler2 : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly GetPrivilegeUserElectiveHandler _getPrivilegeUserElectiveHandler;

        public GetExtracurricularScoreEntryHandler2(
         ISchedulingDbContext dbContext,
         IMachineDateTime dateTime,
         GetPrivilegeUserElectiveHandler getPrivilegeUserElectiveHandler)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _getPrivilegeUserElectiveHandler = getPrivilegeUserElectiveHandler;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularRequest2>(nameof(GetExtracurricularRequest.IdAcademicYear), nameof(GetExtracurricularRequest.Semester), nameof(GetExtracurricularRequest.IdSchool));
            var userLogin = AuthInfo.UserId;

            var getPrivilegeUserElective = await _getPrivilegeUserElectiveHandler.GetAvailabilityPositionUserElective(new GetPrivilegeUserElectiveRequest
            {
                IdUser = userLogin,
                IdSchool = param.IdSchool,
                IdAcademicYear = param.IdAcademicYear
            });

            var listElectiveAccess = getPrivilegeUserElective.Select(x => x.IdExtracurricular).ToList();

           
            var TempScoreLegends = new List<ExtracurricularScoreLegendVm>();

            var ElectiveList = await _dbContext.Entity<MsExtracurricular>()
              .Include(x => x.ExtracurricularGradeMappings)
                    .ThenInclude(y => y.Grade)
                    .ThenInclude(y => y.Level)
              .Include(x => x.ExtracurricularScoreLegendMappings).ThenInclude(y => y.ExtracurricularScoreLegendCategory)
              .Include(x => x.ExtracurricularSpvCoach)
                    .ThenInclude(y => y.ExtracurricularCoachStatus)
              .Where(a => a.Semester == param.Semester
                         && a.ExtracurricularGradeMappings.Any(b => b.Grade.Level.IdAcademicYear == param.IdAcademicYear)
                         && a.Status == true)
              .Where(a => listElectiveAccess.Any(z => z == a.Id))
              .Select(a => new GetExtracurricularResult2
              {
                  IdExtracurricular = a.Id,
                  ExtracurricularName = a.Name + (a.ExtracurricularGradeMappings != null ? (a.ExtracurricularGradeMappings.Count() > 0 ? (" - " + string.Join("; ", a.ExtracurricularGradeMappings.Select(b => b.Grade.Description))) : "") : ""),
                  ScoreStartDate = a.ScoreStartDate == null ? null : a.ScoreStartDate,
                  ScoreEndDate = a.ScoreEndDate == null ? null : a.ScoreEndDate,
                  InPeriodEntry = (_dateTime.ServerTime >= a.ScoreStartDate && _dateTime.ServerTime <= a.ScoreEndDate),
                  Supervisor = string.Join(", ", a.ExtracurricularSpvCoach.Where(b => b.IsSpv == true || b.ExtracurricularCoachStatus.Code == "SPV").Select(c => String.Format("{0} {1}", c.Staff.FirstName, c.Staff.LastName))),
                  Coach = string.Join(", ", a.ExtracurricularSpvCoach.Where(b => b.IsSpv == false || b.ExtracurricularCoachStatus.Code != "SPV").Select(c => String.Format("{0} {1}", c.Staff.FirstName, c.Staff.LastName))),                
              })
              //.OrderBy(a => a.ExtracurricularName)
              .ToListAsync(CancellationToken);

            var ScoreLegend2 = await _dbContext.Entity<MsExtracurricularScoreLegendMapping>()
                                .Include(x => x.ExtracurricularScoreLegendCategory)
                                    .ThenInclude(y => y.ExtracurricularScoreLegends)                                   
                               .Where(a => ElectiveList.Select(c => c.IdExtracurricular).Contains(a.IdExtracurricular))
                               .Select(a => new ExtracurricularScoreLengendVm
                               {
                                   IdExtracurricular = a.IdExtracurricular,
                                   ScoreLegendList = a.ExtracurricularScoreLegendCategory.ExtracurricularScoreLegends.Select(b => new ExtracurricularScoreLegendVm()
                                   {
                                      IdExtracurricularScoreLegend = b.Id,
                                      Score = b.Score,
                                      Description = b.Description
                                   }).ToList()
                                   
                               })
                               .ToListAsync();




            foreach (var item in ElectiveList)
            {
                var tempScoreLegend = ScoreLegend2.Where(b => b.IdExtracurricular == item.IdExtracurricular).FirstOrDefault();
                if(tempScoreLegend != null)
                {
                    item.ScoreLegends = tempScoreLegend.ScoreLegendList.OrderByDescending(a => a.Score).ToList();
                }
            }


            return Request.CreateApiResult2(ElectiveList.OrderBy(a => a.ExtracurricularName) as object);
        }       

        public class ExtracurricularScoreLengendVm
        {
            public string IdExtracurricular { set; get; }
            public List<ExtracurricularScoreLegendVm> ScoreLegendList { set; get; }

        }      
    }

    
}
