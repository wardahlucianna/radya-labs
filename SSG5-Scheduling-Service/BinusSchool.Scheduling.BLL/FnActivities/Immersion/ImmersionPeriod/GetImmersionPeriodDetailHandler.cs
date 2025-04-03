using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod
{
    public class GetImmersionPeriodDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetImmersionPeriodDetailHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetImmersionPeriodDetailRequest>(
                            nameof(GetImmersionPeriodDetailRequest.IdImmersionPeriod));

            var result = await _dbContext.Entity<MsImmersionPeriod>()
                                    .Include(ip => ip.AcademicYear)
                                    .Where(x => x.Id == param.IdImmersionPeriod)
                                    .Select(x => new GetImmersionPeriodDetailResult
                                    {
                                        IdImmersionPeriod = x.Id,
                                        AcademicYear = new CodeWithIdVm
                                        {
                                            Id = x.IdAcademicYear,
                                            Code = x.AcademicYear.Code,
                                            Description = x.AcademicYear.Description
                                        },
                                        Semester = new ItemValueVm
                                        {
                                            Id = x.Semester.ToString(),
                                            Description = x.Semester.ToString(),
                                        },
                                        Name = x.Name,
                                        RegistrationStartDate = x.RegistrationStartDate,
                                        RegistrationEndDate = x.RegistrationEndDate
                                    })
                                    .FirstOrDefaultAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}
