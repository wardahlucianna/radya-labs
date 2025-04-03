using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.CheckUsage
{
    public class CheckUsagePathwayDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public CheckUsagePathwayDetailHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));

            var query = await _dbContext.Entity<MsHomeroomPathway>()
                .Where(x => param.Ids.Contains(x.IdGradePathwayDetail))
                .Select(x => x.IdGradePathwayDetail)
                .Distinct()
                .ToListAsync(CancellationToken);

            var result = query
                .ToDictionary(x => x, _ => true)
                .Concat(param.Ids.ToDictionary(x => x, _ => false))
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.First().Value);

            return Request.CreateApiResult2(result as object);
        }
    }
}