using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnPeriod.SessionSet;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.SessionSet
{
    public class GetSessionSetWithLevelGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetSessionSetWithLevelGradeHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetByLevelGradeRequest>(nameof(GetByLevelGradeRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsSessionSet>(x => param.IdSchool == x.IdSchool);

            if (!string.IsNullOrWhiteSpace(param.IdLevel))
                predicate = predicate.And(x=> x.Sessions.Any(p=> p.GradePathway.Grade.IdLevel==param.IdLevel));

            if (!string.IsNullOrWhiteSpace(param.IdGrade))
                predicate = predicate.And(x => x.Sessions.Any(p => p.GradePathway.IdGrade==param.IdGrade));

            var query = await _dbContext.Entity<MsSessionSet>()
                .Include(p => p.Sessions).ThenInclude(p => p.GradePathway.Grade)
                .Where(predicate)
                .Select(p => new ItemValueVm
                {
                    Id=p.Id,
                    Description=p.Description,
                }).ToListAsync();


            return Request.CreateApiResult2(query as object);
        }
    }
}
