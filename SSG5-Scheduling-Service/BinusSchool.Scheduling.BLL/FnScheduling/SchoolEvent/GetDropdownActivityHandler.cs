using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDropdownActivityHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDropdownActivityHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.GetParams<GetDropdownActivityRequest>();

            var predicate = PredicateBuilder.Create<MsActivity>(x => true);
            if (!string.IsNullOrWhiteSpace(param.Search)){
                var Search = param.Search.ToLower();
                predicate = predicate.And(x => EF.Functions.Like(x.Description.ToLower(), $"%{Search}%"));
            }

            if (!string.IsNullOrWhiteSpace(param.IdSchool)){
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);
            }

            var activities = await _dbContext.Entity<MsActivity>()
                .Where(predicate)
                .Select(c => new GetDropdownActivityResult
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Description = c.Description,
                    })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(activities as object);
        }
    }
}