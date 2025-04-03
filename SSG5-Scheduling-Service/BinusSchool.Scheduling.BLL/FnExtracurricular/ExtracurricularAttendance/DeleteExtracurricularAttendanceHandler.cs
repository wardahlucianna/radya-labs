using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Generic;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class DeleteExtracurricularAttendanceHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteExtracurricularAttendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<DeleteSessionExtracurricularAttendanceRequest, DeleteSessionExtracurricularAttendanceValidator>();

            #region Validate Extracurricular Generate Att
            var exculGenerateAtt = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                .Where(x => x.Id == body.IdExtracurricularGeneratedAtt)
                .SingleOrDefaultAsync(CancellationToken);

            if (exculGenerateAtt == null)
            {
                throw new BadRequestException($"Extracurricular Generate Att with Id : {body.IdExtracurricularGeneratedAtt} not exists");
            }
            #endregion

            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var exculEntry = await _dbContext.Entity<TrExtracurricularAttendanceEntry>()
                    .Where(x => x.IdExtracurricularGeneratedAtt == exculGenerateAtt.Id)
                    .ToListAsync(CancellationToken);

                var exculSession = await _dbContext.Entity<MsExtracurricularSession>()
                    .Where(x => x.Id == exculGenerateAtt.IdExtracurricularSession)
                    .FirstOrDefaultAsync(CancellationToken);

                if (exculSession != null) 
                {
                    var exculSessionMapping = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                            .Where(x => x.IdExtracurricularSession == exculSession.Id)
                            .FirstOrDefaultAsync(CancellationToken);

                    exculSessionMapping.IsActive = false;
                    _dbContext.Entity<TrExtracurricularSessionMapping>().Update(exculSessionMapping);

                    exculSession.IsActive = false;
                    _dbContext.Entity<MsExtracurricularSession>().Update(exculSession);
                }    

                exculEntry.ForEach(x => x.IsActive = false);
                _dbContext.Entity<TrExtracurricularAttendanceEntry>().UpdateRange(exculEntry);

                exculGenerateAtt.IsActive = false;
                _dbContext.Entity<TrExtracurricularGeneratedAtt>().Update(exculGenerateAtt);

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
