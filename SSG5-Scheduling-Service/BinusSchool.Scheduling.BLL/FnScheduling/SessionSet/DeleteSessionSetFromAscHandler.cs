using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnPeriod.SessionSet;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnSchedule.SessionSet
{
    public class DeleteSessionSetFromAscHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteSessionSetFromAscHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<DeleteSessionSetFromAscRequest>(nameof(DeleteSessionSetFromAscRequest.SessionSetId));
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            try
            {
                var query = await _dbContext.Entity<MsSessionSet>()
                           .Include(p => p.Sessions)
                           .Where(p=> p.Id==param.SessionSetId)
                           .FirstOrDefaultAsync();

                if (query!=null)
                {
                    query.IsActive = false;
                    _dbContext.Entity<MsSessionSet>().Update(query);

                    foreach (var item in query.Sessions)
                    {
                        item.IsActive = false;
                        _dbContext.Entity<MsSession>().Update(item);
                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                    await _transaction.CommitAsync(CancellationToken);
                }
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw ex;
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2();
        }
    }
}
