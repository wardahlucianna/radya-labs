using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreComponent
{
    public class GetExtracurricularScoreCalculationTypeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetExtracurricularScoreCalculationTypeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetExtracurricularScoreCalculationTypeRequest>
                (nameof(GetExtracurricularScoreCalculationTypeRequest.IdAcademicYear));

            var getIdSchool = await _dbContext.Entity<MsAcademicYear>()
                .Where(a => a.Id == param.IdAcademicYear)
                .FirstOrDefaultAsync(CancellationToken);

            var getElectiveScoreCalculationType = _dbContext.Entity<MsExtracurricularScoreCalculationType>()
                .Where(a => a.IdSchool == getIdSchool.IdSchool)
                .ToList();

            var result = getElectiveScoreCalculationType.Select(a => new GetExtracurricualrScoreCalculationTypeResult
            {
                IdExtracurricularScoreCalculationType = a.Id,
                CalculationType = a.CalculationType,
                IdSchool = a.IdSchool
            })
                .OrderBy(a => a.CalculationType)
                .ToList();

            return Request.CreateApiResult2(result as object);
        }
    }
}
