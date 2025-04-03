using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class GetDatePeriodWithGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetDatePeriodWithGradeHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<SelectTermRequest>(nameof(SelectTermRequest.IdGrade));
            var query = _dbContext.Entity<MsGrade>()
                .Include(p => p.Periods)
                .Where(x => param.IdGrade.Any(y => y == x.Id));

            if (!query.FirstOrDefault().Periods.Any())
                throw new NotFoundException("Period date not found in Grade");

            var data = await query
                        .Select(x => new GetPeriodResult
                        {
                            Id = x.Id,
                            Code = x.Code,
                            Description = x.Description,
                            Acadyear = x.Level.AcademicYear.Description,
                            StartDate = x.Periods.Min(y => y.StartDate),
                            EndDate = x.Periods.Max(y => y.EndDate),
                            AttendanceStartDate = x.Periods.Min(y => y.AttendanceStartDate),
                            AttendanceEndDate = x.Periods.Max(y => y.AttendanceEndDate)
                        }).FirstOrDefaultAsync();

            return Request.CreateApiResult2(data as object);
        }
    }
}
