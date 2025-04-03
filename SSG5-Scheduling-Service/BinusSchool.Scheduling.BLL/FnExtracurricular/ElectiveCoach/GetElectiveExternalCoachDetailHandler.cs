using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class GetElectiveExternalCoachDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetElectiveExternalCoachDetailHandler(
            ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetElectiveExternalCoachDetailRequest>(nameof(GetElectiveExternalCoachDetailRequest.IdExtracurricularExternalCoach));

            var electiveExternalCoach = await _dbContext.Entity<MsExtracurricularExternalCoach>().FindAsync(param.IdExtracurricularExternalCoach);
            if (electiveExternalCoach is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularExternalCoach"], "Id", param.IdExtracurricularExternalCoach));


            var ReturnResult = await _dbContext.Entity<MsExtracurricularExternalCoach>()
                                            .Include(x => x.ExtracurricularExtCoachTaxStatus)
                                            .Where(a => a.Id == param.IdExtracurricularExternalCoach)
                                            .Select(a => new GetElectiveExternalCoachDetailResult
                                            { 
                                                    IdUser = a.Id,
                                                    UserName = a.Name,
                                                    IdExternalCoach  = a.IdExternalCoach,
                                                    TaxStatus = new ItemValueVm() { Id = a.IdExtracurricularExtCoachTaxStatus, Description = (a.ExtracurricularExtCoachTaxStatus != null ? a.ExtracurricularExtCoachTaxStatus.Description : "-") },
                                                    NPWP = a.NPWP,
                                                    AccountBank = a.AccountBank,
                                                    AccountBankBranch = a.AccountBankBranch,
                                                    AcountNumber = a.AccountNumber,
                                                    AccountName = a.AccountName
                                            })
                                            .FirstOrDefaultAsync(CancellationToken);


            return Request.CreateApiResult2(ReturnResult as object);
        }
    }
}
