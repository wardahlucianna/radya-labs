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
    public class GetExtracurricularScoreLegendHandler2 : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetExtracurricularScoreLegendHandler2(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularScoreLegendRequest2>(nameof(GetExtracurricularScoreLegendRequest2.IdSchool));
          

            var ScoreLegends = await _dbContext.Entity<MsExtracurricularScoreLegendCategory>()
                                      .Include(x => x.ExtracurricularScoreLegends)
                                      .Where(a => a.IdSchool == param.IdSchool)
                                      .Select(a => new GetExtracurricularScoreLegendResult2
                                      {
                                          IdExtracurricularScoreLegendCategory = a.Id,                                          
                                          Description = a.Description,
                                          ScoreLegends = a.ExtracurricularScoreLegends.Select(b => new ExtracurricularScoreLegendVm
                                          {
                                                IdExtracurricularScoreLegend = b.Id,
                                                Score = b.Score,
                                                Description = b.Description
                                          }).OrderBy(b => b.Score).ToList()
                                      })
                                      .OrderBy(a => a.Description)
                                      .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(ScoreLegends as object);
        }
    }
}
