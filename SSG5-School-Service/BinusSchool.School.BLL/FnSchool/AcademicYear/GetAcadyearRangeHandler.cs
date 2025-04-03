using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Comparers;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.AcademicYear
{
    public class GetAcadyearRangeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetAcadyearRangeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            if (!KeyValues.TryGetValue("id", out var id))
                throw new ArgumentNullException(nameof(id));

            var query = await _dbContext.Entity<MsPeriod>()
                .Include(x => x.Grade).ThenInclude(x => x.Level).ThenInclude(x => x.AcademicYear)
                .Where(x => x.Grade.Level.IdAcademicYear == (string)id)
                .ToListAsync(CancellationToken);

            if (query.Count == 0)
                throw new BadRequestException(string.Format(Localizer["ExNotFound"], id));

            var result = query
                .GroupBy(x => x.Grade.Level.IdAcademicYear)
                .Select(x => new GetAcadyearByRangeResult
                {
                    Id = x.Key,
                    Code = x.First().Grade.Level.AcademicYear.Code,
                    Description = x.First().Grade.Level.AcademicYear.Description,
                    StartDate = x.Min(y => y.StartDate),
                    EndDate = x.Max(y => y.EndDate)
                });

            return Request.CreateApiResult2(result.First() as object);
        }
    }
}