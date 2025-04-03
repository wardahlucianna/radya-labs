using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Session;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Session.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class CopySessionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public CopySessionHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<CopySessionRequest, CopySessionValidator>();
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var sessionFrom = await _dbContext.Entity<MsSessionSet>()
                .Include(p => p.Sessions)
                .Where(p => p.Id == body.IdSessionSetFrom).FirstOrDefaultAsync();

            if (sessionFrom == null)
                throw new NotFoundException("Copy from Sessions Set Not found");

            if (sessionFrom.Sessions == null)
                throw new NotFoundException("Copy from Sessions Not found");

            var sessionTo = await _dbContext.Entity<MsSessionSet>()
                .Include(p => p.Sessions)
                .Where(p => p.Id == body.IdSessionSetTo).FirstOrDefaultAsync();

            if (sessionTo == null)
                throw new NotFoundException("Copy To session set Not found");

            foreach (var itemSession in sessionFrom.Sessions)
            {
                var dataSession = new MsSession();
                dataSession.Id = Guid.NewGuid().ToString();
                dataSession.IdSessionSet = body.IdSessionSetTo;
                dataSession.IdDay = itemSession.IdDay;
                dataSession.IdGradePathway = itemSession.IdGradePathway;
                dataSession.SessionID = itemSession.SessionID;
                dataSession.Name = itemSession.Name;
                dataSession.Alias = itemSession.Alias;
                dataSession.DurationInMinutes = itemSession.DurationInMinutes;
                dataSession.StartTime = itemSession.StartTime;
                dataSession.EndTime = itemSession.EndTime;
                dataSession.UserIn = AuthInfo.UserId;
                _dbContext.Entity<MsSession>().Add(dataSession);
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await _transaction.CommitAsync();

            return Request.CreateApiResult2();
        }

        protected override Task<IActionResult> OnException(Exception ex)
        {
            _transaction?.Rollback();
            return base.OnException(ex);
        }

        protected override void OnFinally()
        {
            _transaction?.Dispose();
        }
    }
}
