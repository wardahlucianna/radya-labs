using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.Timetable;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TimetablePreferences
{
    public class GetTimeTablePreferencesStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public GetTimeTablePreferencesStatusHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            // var param = Request.ValidateParams<IdCollection>(nameof(IdCollection.Ids));
            var ids = (await GetIdsFromBody()).Distinct();

            var results = await _dbContext.Entity<TrTimeTablePrefHeader>()
                // .SearchByIds(param)
                .Where(x => ids.Contains(x.Id))
                .Select(x => new TimetableResult
                {
                    Id = x.Id,
                    Status = x.Status,
                    IsMerge = x.IsMerge,
                    CanDelete = x.CanDelete
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(results as object);
        }
    }
}
