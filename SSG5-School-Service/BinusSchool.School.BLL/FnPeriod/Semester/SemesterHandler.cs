using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Semester;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Semester
{
    public class SemesterHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public SemesterHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetSemesterRequest>(nameof(GetSemesterRequest.IdGrade));

            var query = await _dbContext.Entity<MsPeriod>()
                .Where(x => x.IdGrade == param.IdGrade)
                .Select(x => x.Semester)
                .ToListAsync(CancellationToken);
            
            return Request.CreateApiResult2(query.Distinct().OrderBy(x => x) as object);
        }
    }
}