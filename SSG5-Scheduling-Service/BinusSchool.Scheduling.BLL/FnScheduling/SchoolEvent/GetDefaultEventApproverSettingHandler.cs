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
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolsEvent;

namespace BinusSchool.Scheduling.FnSchedule.SchoolEvent
{
    public class GetDefaultEventApproverSettingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetDefaultEventApproverSettingHandler(ISchedulingDbContext schedulingDbContext)
        {
            _dbContext = schedulingDbContext;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDefaultEventApproverSettingRequest>(nameof(GetDefaultEventApproverSettingRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsEventApproverSetting>(x => true);
            if (!string.IsNullOrEmpty(param.IdSchool))
                predicate = predicate.And(x=>x.IdSchool == param.IdSchool);
            var query = _dbContext.Entity<MsEventApproverSetting>()
                .Include(x => x.Approver1)
                .Include(x => x.Approver2)
                .Where(predicate);

            if (param.IdSchool != null)
                query = query.Where(x => x.IdSchool == param.IdSchool);
            
            var dataApproverSetting = await query
                    .FirstOrDefaultAsync(CancellationToken);

            if (dataApproverSetting == null)
                 return Request.CreateApiResult2(new GetDefaultEventApproverSettingResult() as object);

            var eventApproverSetting = new GetDefaultEventApproverSettingResult
                {
                    Id = dataApproverSetting.Id,
                    IsSetDefaultApprover1 = dataApproverSetting.IdApprover1 != null ? true : false,
                    IsSetDefaultApprover2 = dataApproverSetting.IdApprover2 != null ? true : false,
                    UserApprover1 =  dataApproverSetting == null ? null : new CodeWithIdVm
                    {
                        Id = dataApproverSetting?.IdApprover1,
                        Description = dataApproverSetting?.Approver1?.DisplayName,
                    },
                    UserApprover2 =  dataApproverSetting.IdApprover2 == null ? null : new CodeWithIdVm
                    {
                        Id = dataApproverSetting?.IdApprover2,
                        Description = dataApproverSetting?.Approver2?.DisplayName,
                    }
                };

            return Request.CreateApiResult2(eventApproverSetting as object);
        }
    }
}
