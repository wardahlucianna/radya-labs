using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class GetSessionBySessionSetAscTimeTableHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetSessionBySessionSetAscTimeTableHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSessionBySessionSetRequest>(nameof(GetSessionBySessionSetRequest.IdSessionSet));
            var get = await _dbContext.Entity<MsSession>()
                         .Include(p => p.GradePathway)
                         .ThenInclude(p => p.GradePathwayDetails)
                         .ThenInclude(p => p.Pathway)
                         .Include(p => p.GradePathway)
                         .ThenInclude(p => p.Grade)
                         .Include(p => p.Day)
                         .Where(p => p.IdSessionSet == param.IdSessionSet)
                         .Select(p => new GetSessionAscTimetableResult
                         {
                             Id=p.Id,
                             Name=p.Name,
                             Alias=p.Alias,
                             DaysCode=p.Day.Code,
                             DaysName=p.Day.Description,
                             SessionId = p.SessionID,
                             StartTime = p.StartTime,
                             EndTime = p.EndTime,
                             DurationInMinutes = p.DurationInMinutes,
                             Pathway = string.Join("-", p.GradePathway.GradePathwayDetails.Select(x => x.Pathway.Description)),
                             Grade = p.GradePathway.Grade.Description,
                         }).ToListAsync();

            return Request.CreateApiResult2(get as object);
        }
    }
}
