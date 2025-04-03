using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectiveCoach;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectiveCoach
{
    public class UpdateElectiveExternalCoachHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public UpdateElectiveExternalCoachHandler(
            ISchedulingDbContext dbContext,
             IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            
            var body = await Request.ValidateBody<UpdateElectiveExternalCoachRequest, UpdateElectiveExternalCoachValidator>();


            var taxStatusData = await _dbContext.Entity<LtExtracurricularExtCoachTaxStatus>().FindAsync(body.IdExtracurricularExtCoachTaxStatus);
            if (taxStatusData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["extracurricularExtCoachTaxStatus"], "Id", body.IdExtracurricularExtCoachTaxStatus));

            var ExtCoach = _dbContext.Entity<MsExtracurricularExternalCoach>().Where(a => a.Id == body.IdUser).FirstOrDefault();

            if (ExtCoach != null)
            {                
                ExtCoach.Name = body.UserName;
                ExtCoach.IdExtracurricularExtCoachTaxStatus = body.IdExtracurricularExtCoachTaxStatus;
                ExtCoach.NPWP = body.NPWP;
                ExtCoach.AccountBank = body.AccountBank;
                ExtCoach.AccountBankBranch = body.AccountBankBranch;
                ExtCoach.AccountNumber = body.AcountNumber;
                ExtCoach.AccountName = body.AccountName;
                _dbContext.Entity<MsExtracurricularExternalCoach>().Update(ExtCoach);

            }
            else
            {
                var idSchool = await _dbContext.Entity<MsUserRole>()
                                    .Include(x => x.Role)
                                    .Where(a => a.IdUser == body.IdUser)
                                    .Select(a => a.Role.IdSchool).FirstOrDefaultAsync();
                                            
                var currPreID = _dateTime.ServerTime.ToString("yyMM");
                var GetLatestID = _dbContext.Entity<MsExtracurricularExternalCoach>()
                                .Where(a =>                                  
                                     a.IdExternalCoach.StartsWith(currPreID)
                                )                                
                                .OrderByDescending(a => a.IdExternalCoach)
                                .Select(a => a.IdExternalCoach)
                                .ToList();

                var newExternalCoach = "";
                if (GetLatestID.Count > 0)
                {
                    newExternalCoach = (Convert.ToInt64(GetLatestID.FirstOrDefault().ToString())+ 1).ToString();
                }
                else
                {
                    newExternalCoach = currPreID + "000001";
                }


                var insertExtCoach = new MsExtracurricularExternalCoach();
                insertExtCoach.IdSchool = idSchool;
                insertExtCoach.Id = body.IdUser;
                insertExtCoach.Name = body.UserName;
                insertExtCoach.IdExternalCoach = newExternalCoach;
                insertExtCoach.IdExtracurricularExtCoachTaxStatus = body.IdExtracurricularExtCoachTaxStatus;
                insertExtCoach.NPWP = body.NPWP;
                insertExtCoach.AccountBank = body.AccountBank;
                insertExtCoach.AccountBankBranch = body.AccountBankBranch;
                insertExtCoach.AccountNumber = body.AcountNumber;
                insertExtCoach.AccountName = body.AccountName;
                _dbContext.Entity<MsExtracurricularExternalCoach>().Add(insertExtCoach);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
