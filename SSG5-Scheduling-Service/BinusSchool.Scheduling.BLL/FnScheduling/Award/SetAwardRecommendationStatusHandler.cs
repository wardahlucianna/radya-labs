using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Award;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Award.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Award
{
    public class SetAwardRecommendationStatusHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public SetAwardRecommendationStatusHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<SetAwardRecommendationStatusRequest, SetAwardRecommendationStatusValidator>();

            var data = await _dbContext.Entity<MsAward>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["EventType"], "Id", body.Id));

            var maxRecommendation = await _dbContext.Entity<MsAward>()
                .Where(x => x.Id != body.Id && x.IdSchool == body.IdSchool && x.IsSetRecommendation == true)
            .ToListAsync(CancellationToken);
            if (maxRecommendation.Count >= 5)
                throw new BadRequestException($"Maximum recommendation is 5");

            data.IsSetRecommendation = body.IsSetRecommendation;

            _dbContext.Entity<MsAward>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
