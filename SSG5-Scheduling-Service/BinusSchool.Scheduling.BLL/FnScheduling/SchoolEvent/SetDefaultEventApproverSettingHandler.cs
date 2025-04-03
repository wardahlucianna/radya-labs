using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Scheduling.FnSchedule.Award.Validator;
using BinusSchool.Persistence.SchedulingDb.Entities.User;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class SetDefaultEventApproverSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SetDefaultEventApproverSettingHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetDefaultEventApproverSettingRequest, SetDefaultEventApproverSettingValidator>();

            var dataEventApproverSetting = await _dbContext.Entity<MsEventApproverSetting>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (dataEventApproverSetting is null)
                throw new BadRequestException($"Event approver setting not found");

            if(body.IdApprover2 != null){
                var cekApprover1 = await _dbContext.Entity<MsUser>()
                    .Where(x => x.Id == body.IdApprover1)
                    .FirstOrDefaultAsync(CancellationToken);
                if (cekApprover1 is null)
                    throw new BadRequestException($"User in Approver 1 not found");
            }

            if(body.IdApprover2 != null){
                var cekApprover2 = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == body.IdApprover2)
                .FirstOrDefaultAsync(CancellationToken);
                if (cekApprover2 is null)
                    throw new BadRequestException($"User in Approver 2 not found");
            }

            var data = _dbContext.Entity<MsEventApproverSetting>()
                .FirstOrDefault(x => x.Id == body.Id);
            
            data.IdApprover1 = body.IdApprover1;
            data.IdApprover2 = body.IdApprover2;

            _dbContext.Entity<MsEventApproverSetting>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
