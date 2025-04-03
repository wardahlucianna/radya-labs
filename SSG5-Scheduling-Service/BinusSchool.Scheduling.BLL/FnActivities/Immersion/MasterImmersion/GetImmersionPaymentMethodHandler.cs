using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class GetImmersionPaymentMethodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetImmersionPaymentMethodHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetImmersionPaymentMethodRequest>();

            param.SearchBy = new List<string>() { "Description" };

            var resultList = await _dbContext.Entity<LtImmersionPaymentMethod>()
                            .Where(x => string.IsNullOrEmpty(param.IdImmersionPaymentMethod) ? true : x.Id == param.IdImmersionPaymentMethod)
                            .SearchByDynamic(param)
                            .Select(x => new GetImmersionPaymentMethodResult
                            {
                                Id = x.Id,
                                Description = x.Description
                            })
                            .OrderBy(x => x.Description)
                            .OrderByDynamic(param)
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(resultList as object);
        }
    }
}
