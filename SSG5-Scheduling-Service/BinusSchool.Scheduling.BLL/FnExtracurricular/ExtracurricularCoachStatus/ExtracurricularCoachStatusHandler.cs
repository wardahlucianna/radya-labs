using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularCoachStatus;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularCoachStatus
{
    public class ExtracurricularCoachStatusHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ExtracurricularCoachStatusHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {

            IReadOnlyList<IItemValueVm> items;
            items = await _dbContext.Entity<LtExtracurricularCoachStatus>()                                  
                                  .Select(a => new CodeWithIdVm()
                                  {
                                      Id = a.Id,
                                      Description = a.Description
                                  })
                                  .OrderBy(a => a.Id)
                                  .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items);
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
