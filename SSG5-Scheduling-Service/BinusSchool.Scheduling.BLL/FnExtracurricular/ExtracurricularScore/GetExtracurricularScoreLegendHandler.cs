using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class GetExtracurricularScoreLegendHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetExtracurricularScoreLegendHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularScoreLegendRequest>(nameof(GetExtracurricularScoreLegendRequest.IdSchool));
            //GetExtracurricularScoreLegendResult

            var ScoreLegends = await _dbContext.Entity<MsExtracurricularScoreLegend>()
                                      .Where(a => a.IdSchool == param.IdSchool)
                                      .Select(a => new GetExtracurricularScoreLegendResult
                                      {
                                          IdExtracurricularScoreLegend = a.Id,
                                          Score = a.Score,
                                          Description = a.Description
                                      })
                                      .OrderBy(a => a.Score)
                                      .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(ScoreLegends as object);
        }
    }
}
