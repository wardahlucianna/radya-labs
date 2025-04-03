using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.AcademicYear
{
    public class GetAcadyearByRangeHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _requiredParams =
        {
            nameof(GetAcadyearByRangeRequest.IdSchool),
            nameof(DateTimeRange.Start), 
            nameof(DateTimeRange.End)
        };
        
        private readonly ISchoolDbContext _dbContext;

        public GetAcadyearByRangeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAcadyearByRangeRequest>(_requiredParams);
                
            var query = await _dbContext.Entity<MsPeriod>()
                .Where(x => x.Grade.Level.AcademicYear.IdSchool == param.IdSchool)
                .Where(x 
                    => x.StartDate == param.Start || x.EndDate == param.End 
                    || (x.StartDate < param.Start
                        ? (x.EndDate > param.Start && x.EndDate < param.End) || x.EndDate > param.End
                        : (param.End > x.StartDate && param.End < x.EndDate) || param.End > x.EndDate))
                .Select(x => new
                {
                    x.Grade.Level.IdAcademicYear,
                    x.Grade.Level.AcademicYear.Code,
                    x.Grade.Level.AcademicYear.Description,
                    x.StartDate,
                    x.EndDate
                })
                .Distinct()
                .ToListAsync(CancellationToken);

            var results = query
                .GroupBy(x => x.IdAcademicYear)
                .Select(x => new GetAcadyearByRangeResult
                {
                    Id = x.Key,
                    Code = x.First().Code,
                    Description = x.First().Description,
                    StartDate = x.Min(y => y.StartDate),
                    EndDate = x.Max(y => y.EndDate)
                })
                .Where(x => DateTimeUtil.IsIntersect(x.StartDate, x.EndDate, param.Start, param.End))
                .OrderBy(x => x.Code);

            return Request.CreateApiResult2(results as object);
        }
    }
}
