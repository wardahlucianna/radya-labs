using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class DeleteMasterImmersionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public DeleteMasterImmersionHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<DeleteMasterImmersionRequest, DeleteMasterImmersionValidator>();

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var datas = await _dbContext.Entity<MsImmersion>()
                           .Where(x => param.IdImmersions.Any(y => y == x.Id))
                           .ToListAsync(CancellationToken);

                var undeleted = new UndeletedResult2();

                // find not found ids
                param.IdImmersions = param.IdImmersions.Except(param.IdImmersions.Intersect(datas.Select(x => x.Id)));
                undeleted.NotFound = param.IdImmersions.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

                // Remove TrImmersionGradeMapping
                var gradeMappingList = await _dbContext.Entity<TrImmersionGradeMapping>()
                                            .Where(x => param.IdImmersions.Any(y => y == x.IdImmersion))
                                            .ToListAsync(CancellationToken);

                _dbContext.Entity<TrImmersionGradeMapping>().RemoveRange(gradeMappingList);

                // Remove MsImmersion
                _dbContext.Entity<MsImmersion>().RemoveRange(datas);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);
                return Request.CreateApiResult2(errors: undeleted.AsErrors());
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction.Dispose();
            }

            throw new BadRequestException(null);
        }
    }
}
