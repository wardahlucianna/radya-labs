using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class GetAttendanceStatusBySchoolHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        public GetAttendanceStatusBySchoolHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAttendanceStatusBySchoolRequest>(
                            nameof(GetAttendanceStatusBySchoolRequest.IdSchool));

            var result = await _dbContext.Entity<LtExtracurricularStatusAtt>()
                            .Where(x => x.IdSchool == param.IdSchool)
                            .Select(x => new GetAttendanceStatusBySchoolResult
                            {
                                Status = new CodeWithIdVm
                                {
                                    Id = x.Id,
                                    Code = x.Code,
                                    Description = x.Description
                                },
                                NeedReason = x.NeedReason
                            })
                            .Distinct()
                            .OrderBy(x => x.Status.Code)
                            .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(result as object);
        }
    }
}
