using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Persistence.Abstractions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.School.FnSchool.PublishSurvey.Validator;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;

namespace BinusSchool.School.FnSchool.SurveySummary
{

    public class AddAndUpdateSurveySummaryLogHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public AddAndUpdateSurveySummaryLogHandler(ISchoolDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<AddAndUpdateSurveySummaryLogRequest, SurveySummaryLogValidator>();

            var SurveySummaryLog = await _dbContext.Entity<TrSurveySummaryLog>()
                        .Where(e => e.UserIn == body.IdUser && e.IsProcess)
                        .FirstOrDefaultAsync(CancellationToken);

            if (SurveySummaryLog == null)
            {
                TrSurveySummaryLog newSurveySummaryLog = new TrSurveySummaryLog
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = _dateTime.ServerTime,
                    UserIn = body.IdUser,
                    IsProcess = true,
                };

                if (body.IsDone)
                {
                    newSurveySummaryLog.IsDone = body.IsDone;
                    newSurveySummaryLog.IsProcess = false;
                }

                if (body.IsError)
                {
                    newSurveySummaryLog.IsError = body.IsError;
                    newSurveySummaryLog.ErrorMessage = body.Message;
                    newSurveySummaryLog.IsProcess = false;
                }

                _dbContext.Entity<TrSurveySummaryLog>().Add(newSurveySummaryLog);
            }
            else
            {
                SurveySummaryLog.EndDate = _dateTime.ServerTime;
                SurveySummaryLog.IsProcess = true;

                if (body.IsDone)
                {
                    SurveySummaryLog.IsDone = body.IsDone;
                    SurveySummaryLog.IsProcess = false;
                }

                if (body.IsError)
                {
                    SurveySummaryLog.IsError = body.IsError;
                    SurveySummaryLog.ErrorMessage = body.Message;
                    SurveySummaryLog.IsProcess = false;
                }

                _dbContext.Entity<TrSurveySummaryLog>().Update(SurveySummaryLog);
            }


            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }
    }
}
