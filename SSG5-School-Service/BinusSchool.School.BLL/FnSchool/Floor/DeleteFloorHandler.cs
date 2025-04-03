using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Floor;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Floor.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.School.FnSchool.Floor
{
    public class DeleteFloorHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteFloorHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteFloorRequest, DeleteFloorValidator>();

            var data = await _dbContext.Entity<MsFloor>()
                .Include(x => x.VenueMappings)
                .Include(x => x.Lockers)
                .Where(x => param.IdFloor.Contains(x.Id))
                .Where(x => !x.VenueMappings.Any())
                .Where(x => !x.Lockers.Any())
                .ToListAsync(CancellationToken);

            if (data.Count == 0)
            {
                throw new Exception("Data has been used.");
            }

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
                data.ForEach(x => x.IsActive = false);
                _dbContext.Entity<MsFloor>().UpdateRange(data);
                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
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
