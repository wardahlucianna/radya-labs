using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class GetSessionDayHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetSessionDayHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSessionDayRequest>(nameof(GetSessionDayRequest.IdSessionSet), nameof(GetSessionDayRequest.IdSessionSet));
            var predicate = PredicateBuilder.Create<MsSession>(x => x.IdSessionSet == param.IdSessionSet && x.GradePathway.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.SessionId) && int.TryParse(param.SessionId, out var sessionId))
                predicate = predicate.And(x => x.SessionID == sessionId);

            var query = await _dbContext.Entity<MsSession>()
                .Where(predicate)
                .OrderBy(x => x.IdDay)
                .Select(x => new GetSessionDayResult
                {
                    Id = x.IdDay,
                    Code = x.Day.Code,
                    Description = x.Day.Description,
                    Session = new NameValueVm(x.Id, x.Name)
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
