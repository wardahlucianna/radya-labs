using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Session.Validator;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class AddSessionFromASCHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public AddSessionFromASCHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<List<AddSessionFromAscRequest>, AddSessionFromAscValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            foreach (var item in body)
            {
                var param = new MsSession
                {
                    Id = item.Id,
                    IdSessionSet = item.IdSessionSet,
                    IdDay = item.IdDay,
                    IdGradePathway = item.IdGradePathway,
                    SessionID = int.Parse(item.SessionId),
                    Name = item.Name,
                    Alias = item.Alias,
                    DurationInMinutes = item.DurationInMinutes,
                    StartTime = TimeSpan.Parse(item.StartTime),
                    EndTime = TimeSpan.Parse(item.EndTime),
                    UserIn = AuthInfo.UserId
                };

                _dbContext.Entity<MsSession>().Add(param);
            }
            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
