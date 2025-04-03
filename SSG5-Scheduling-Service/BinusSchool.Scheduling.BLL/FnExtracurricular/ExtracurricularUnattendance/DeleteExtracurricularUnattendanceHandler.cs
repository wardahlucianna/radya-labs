using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularUnattendance;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class DeleteExtracurricularUnattendanceHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteExtracurricularUnattendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<DeleteExtracurricularUnattendanceRequest, DeleteExtracurricularUnattendanceValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                #region Validate Extracurricular No Att Date
                var isExtracurricularNoAttDateExist = await _dbContext.Entity<MsExtracurricularNoAttDate>()
                        .Where(x => x.Id == body.IdExtracurricularNoAttDate)
                        .SingleOrDefaultAsync(CancellationToken);
                if (isExtracurricularNoAttDateExist == null)
                    throw new BadRequestException($"ExtracurricularNoAttDate with Id : {body.IdExtracurricularNoAttDate} not exists");
                #endregion

                #region Create Temp List
                var exculNoAttDateMappingList = await _dbContext.Entity<MsExtracurricularNoAttDateMapping>()
                         .Where(x => x.IdExtracurricularNoAttDate == body.IdExtracurricularNoAttDate)
                         .ToListAsync(CancellationToken);

                var exculGenerateAttList = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                        .IgnoreQueryFilters()
                        .Where(x => x.Date.Date >= isExtracurricularNoAttDateExist.StartDate.Date && x.Date.Date <= isExtracurricularNoAttDateExist.EndDate.Date)
                        .Where(x => x.IsActive == false)
                        .ToListAsync(CancellationToken);
                #endregion

                #region Soft Delete Extracurricular No Att Date
                isExtracurricularNoAttDateExist.IsActive = false;
                isExtracurricularNoAttDateExist.DateUp = DateTime.UtcNow;
                isExtracurricularNoAttDateExist.UserUp = AuthInfo.UserId;
                _dbContext.Entity<MsExtracurricularNoAttDate>().Update(isExtracurricularNoAttDateExist);
                #endregion

                foreach (var exculNoAttDateMapping in exculNoAttDateMappingList)
                {
                    #region Soft Delete Extracurricular No Att Date Mapping
                    var isExculNoAttDateMapping = exculNoAttDateMappingList
                        .Where(x => x.Id == exculNoAttDateMapping.Id)
                        .SingleOrDefault();

                    if (isExculNoAttDateMapping != null)
                    {
                        isExculNoAttDateMapping.IsActive = false;
                        isExculNoAttDateMapping.DateUp = DateTime.UtcNow;
                        isExculNoAttDateMapping.UserUp = AuthInfo.UserId;

                        _dbContext.Entity<MsExtracurricularNoAttDateMapping>().Update(isExculNoAttDateMapping);
                    }
                    #endregion

                    #region Restore Generate Att
                    var deletedGenerateAttList = exculGenerateAttList
                        .Where(x => x.IdExtracurricular == exculNoAttDateMapping.IdExtracurricular)
                        .ToList();

                    foreach(var deletedGenerateAtt in deletedGenerateAttList) 
                    {
                        deletedGenerateAtt.IsActive = true;
                        deletedGenerateAtt.DateUp = DateTime.UtcNow;
                        deletedGenerateAtt.UserUp = AuthInfo.UserId;
                        _dbContext.Entity<TrExtracurricularGeneratedAtt>().Update(deletedGenerateAtt);
                    }
                    #endregion
                }

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
