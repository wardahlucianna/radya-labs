using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetExtCoachTaxStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetExtCoachTaxStatusHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var ReturnResult = await _dbContext.Entity<LtExtracurricularExtCoachTaxStatus>()
                                  .Select(a => new GetExtCoachTaxStatusResult()
                                  {
                                      IdTaxStatus = a.Id,
                                      TaxStatusName = a.Description + " - (" + a.Percentage + "%)"      
                                  })   
                                  .OrderBy(a => a.TaxStatusName)
                                  .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
