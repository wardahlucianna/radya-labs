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
using BinusSchool.Persistence.SchedulingDb.Entities.School;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularUnattendance
{
    public class AddExtracurricularUnattendanceHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public AddExtracurricularUnattendanceHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<AddExtracurricularUnattendanceRequest, AddExtracurricularUnattendanceValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var IdSchool = await _dbContext.Entity<MsAcademicYear>()
                   .Where(x => x.Id == body.IdAcademicYear)
                   .Select(x => x.IdSchool)
                   .FirstOrDefaultAsync(CancellationToken);

                #region Create Temp List
                var extracurricularList = await _dbContext.Entity<MsExtracurricular>()
                    .Where(x => x.ExtracurricularGroup.IdSchool == IdSchool)
                    .ToListAsync(CancellationToken);

                var exculGenerateAttList = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                    .Where(x => x.Date >= body.UnattendanceStartDate && x.Date <= body.UnattendanceEndDate)
                    .ToListAsync(CancellationToken);
                #endregion

                #region Validate MsExtracurricular
                var falseExtracurricularList = body.UnattendanceExtracurricularList
                    .Where(ex => extracurricularList.All(ex2 => ex2.Id != ex.IdExtracurricular))
                    .Select(x => x.IdExtracurricular).ToList();

                if (falseExtracurricularList.Count() > 0)
                {
                    throw new BadRequestException($"Extracurricular with Id : {string.Join(",", falseExtracurricularList)} not exists");
                }
                #endregion

                #region Insert New Master Extracurricular No Att Date
                var param = new MsExtracurricularNoAttDate
                {
                    Id = Guid.NewGuid().ToString(),
                    StartDate = body.UnattendanceStartDate,
                    EndDate = body.UnattendanceEndDate,
                    Description = body.Description
                };
                _dbContext.Entity<MsExtracurricularNoAttDate>().Add(param);
                #endregion

                #region Mapping Extracurricular to No Att Date
                foreach (var excul in body.UnattendanceExtracurricularList)
                {
                    var paramExcul = new MsExtracurricularNoAttDateMapping
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdExtracurricular = excul.IdExtracurricular,
                        IdExtracurricularNoAttDate = param.Id,
                    };
                    _dbContext.Entity<MsExtracurricularNoAttDateMapping>().Add(paramExcul);

                    #region Soft Delete Generate Att
                    var deleteGenerateAttList = exculGenerateAttList
                        .Where(x => x.IdExtracurricular == excul.IdExtracurricular)
                        .ToList();

                    foreach (var deleteGenerateAtt in deleteGenerateAttList)
                    {
                        deleteGenerateAtt.IsActive = false;
                        deleteGenerateAtt.DateUp = DateTime.UtcNow;
                        deleteGenerateAtt.UserUp = AuthInfo.UserId;
                        _dbContext.Entity<TrExtracurricularGeneratedAtt>().Update(deleteGenerateAtt);
                    }
                    #endregion
                }
                #endregion

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
