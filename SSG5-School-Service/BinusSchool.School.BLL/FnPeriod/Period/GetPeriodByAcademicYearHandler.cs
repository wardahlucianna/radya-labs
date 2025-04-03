using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class GetPeriodByAcademicYearHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetPeriodByAcademicYearHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPeriodByAcademicYearRequest>
                (nameof(GetPeriodByAcademicYearRequest.IdSchool),
                 nameof(GetPeriodByAcademicYearRequest.IdAcademicYear));

            var getPeriod = _dbContext.Entity<MsPeriod>()
                .Include(a => a.Grade.Level.AcademicYear)
                .Where(a => a.Grade.Level.AcademicYear.IdSchool == param.IdSchool
                    && a.Grade.Level.AcademicYear.Id == param.IdAcademicYear)
                .Distinct();

            var items = await getPeriod
                .Select(a => new GetPeriodByAcademicYearResult
                {
                    Id = a.Code,
                    Description = a.Description
                })
                .Distinct()
                .OrderBy(a => a.Id)
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items as object);
        }
    }
}
