using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.ImmersionPeriod;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnActivities.Immersion.ImmersionPeriod
{
    public class DeleteImmersionPeriodHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public DeleteImmersionPeriodHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteImmersionPeriodRequest, DeleteImmersionPeriodValidator>();

            // check immersion period whether it is used or not
            var usedImmersionRule = _dbContext.Entity<MsImmersion>()
                                        .Include(i => i.ImmersionPeriod)
                                        .Where(x => x.IdImmersionPeriod == param.IdImmersionPeriod)
                                        .ToList();

            if(usedImmersionRule.Count() > 0)
            {
                throw new BadRequestException("Failed! This immersion period has been related to one or more master immersion(s).");
            }

            var deleteImmersionRule = _dbContext.Entity<MsImmersionPeriod>()
                                        .Where(x => x.Id == param.IdImmersionPeriod)
                                        .FirstOrDefault();

            _dbContext.Entity<MsImmersionPeriod>().Remove(deleteImmersionRule);
            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
