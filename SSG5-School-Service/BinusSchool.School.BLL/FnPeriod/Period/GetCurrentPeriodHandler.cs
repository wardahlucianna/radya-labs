using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.Period;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnPeriod.Period
{
    public class GetCurrentPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public GetCurrentPeriodHandler(
                ISchoolDbContext dbContext, 
                IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCurrentPeriodRequest>(nameof(GetCurrentPeriodRequest.IdGrade));

            var queryPeriod = await _dbContext.Entity<MsPeriod>()
                    .Include(x => x.Grade)
                    .Where(x => x.IdGrade == param.IdGrade &&
                            _dateTime.ServerTime.Date >= x.StartDate.Date &&
                            _dateTime.ServerTime.Date <= x.EndDate.Date)
                    .OrderBy(x => x.OrderNumber)
                        .ThenBy(x => x.Semester)
                        .ThenBy(x => x.Code)
                    .FirstOrDefaultAsync(CancellationToken);

            var data = queryPeriod == null ? null : new GetCurrentPeriodResult
            {
                IdPeriod = queryPeriod.Id,
                Code = queryPeriod.Code.Substring(queryPeriod.Code.Length - 1).ToString(),
                PeriodCode = queryPeriod.Code,
                PeriodDescription = queryPeriod.Description,
                Semester = queryPeriod.Semester
            };

            return Request.CreateApiResult2(data as object);
        }
    }
}
