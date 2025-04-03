using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class DeleteMasterExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public DeleteMasterExtracurricularHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<DeleteMasterExtracurricularRequest, DeleteMasterExtracurricularValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var isExtracurricularExist = await _dbContext.Entity<MsExtracurricular>()
                    .Where(x => x.Id == body.IdExtracurricular)
                    .SingleOrDefaultAsync(CancellationToken);
                if (isExtracurricularExist == null)
                    throw new BadRequestException($"Extracurricular not exists");

                var isExtracurricularSpvCoachExist = await _dbContext.Entity<MsExtracurricularSpvCoach>()
                    .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                    .ToListAsync(CancellationToken);
                if (isExtracurricularSpvCoachExist != null)
                {
                    foreach(var SpvCoach in isExtracurricularSpvCoachExist)
                    {
                        SpvCoach.IsActive = false;
                    }
                    _dbContext.Entity<MsExtracurricularSpvCoach>().UpdateRange(isExtracurricularSpvCoachExist);
                }

                var isExtracurricularExtCoachMappingExist = await _dbContext.Entity<MsExtracurricularExtCoachMapping>()
                   .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                   .ToListAsync(CancellationToken);
                if (isExtracurricularExtCoachMappingExist != null)
                {
                    foreach (var ExtCoach in isExtracurricularExtCoachMappingExist)
                    {
                        ExtCoach.IsActive = false;
                    }
                    _dbContext.Entity<MsExtracurricularExtCoachMapping>().UpdateRange(isExtracurricularExtCoachMappingExist);
                }

                var isExtracurricularScoreEntryExist = await _dbContext.Entity<TrExtracurricularScoreEntry>()
                    .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                    .ToListAsync(CancellationToken);
                if (isExtracurricularScoreEntryExist != null)
                {
                    foreach (var ScoreEntry in isExtracurricularScoreEntryExist)
                    {
                        ScoreEntry.IsActive = false;
                    }
                    _dbContext.Entity<TrExtracurricularScoreEntry>().UpdateRange(isExtracurricularScoreEntryExist);
                }

                var isExtracurricularNoAttDateMappingExist = await _dbContext.Entity<MsExtracurricularNoAttDateMapping>()
                    .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                    .ToListAsync(CancellationToken);
                if (isExtracurricularNoAttDateMappingExist != null)
                {
                    foreach (var NoAttDate in isExtracurricularNoAttDateMappingExist)
                    {
                        //var isMsExtracurricularNoAttDateExist = _dbContext.Entity<MsExtracurricularNoAttDate>()
                        //    .Where(x => x.Id == NoAttDate.IdExtracurricularNoAttDate)
                        //    .SingleOrDefault();
                        //if (isMsExtracurricularNoAttDateExist != null)
                        //    isMsExtracurricularNoAttDateExist.IsActive = false;
                        //    _dbContext.Entity<MsExtracurricularNoAttDate>().Update(isMsExtracurricularNoAttDateExist);

                        NoAttDate.IsActive = false;
                    }
                    _dbContext.Entity<MsExtracurricularNoAttDateMapping>().UpdateRange(isExtracurricularNoAttDateMappingExist);
                }

                var isExtracurricularSessionMapping = await _dbContext.Entity<TrExtracurricularSessionMapping>()
                    .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                    .ToListAsync(CancellationToken);
                if (isExtracurricularSessionMapping != null)
                {
                    foreach (var SessionMapping in isExtracurricularSessionMapping)
                    {
                        SessionMapping.IsActive = false;
                    }
                    _dbContext.Entity<TrExtracurricularSessionMapping>().UpdateRange(isExtracurricularSessionMapping);
                }

                var isExtracurricularGeneratedAtt = await _dbContext.Entity<TrExtracurricularGeneratedAtt>()
                   .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                   .ToListAsync(CancellationToken);
                if (isExtracurricularGeneratedAtt != null)
                {
                    foreach (var GeneratedAtt in isExtracurricularGeneratedAtt)
                    {
                        //var isMsExtracurricularSessionExist = _dbContext.Entity<MsExtracurricularSession>()
                        //    .Where(x => x.Id == GeneratedAtt.IdExtracurricularSession)
                        //    .SingleOrDefault();
                        //if (isMsExtracurricularSessionExist != null)
                        //    isMsExtracurricularSessionExist.IsActive = false;
                        //_dbContext.Entity<MsExtracurricularSession>().Update(isMsExtracurricularSessionExist);

                        GeneratedAtt.IsActive = false;
                    }
                    _dbContext.Entity<TrExtracurricularGeneratedAtt>().UpdateRange(isExtracurricularGeneratedAtt);
                }

                var isExtracurricularGradeMapping = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                   .Where(x => x.IdExtracurricular == body.IdExtracurricular)
                   .ToListAsync(CancellationToken);
                if (isExtracurricularGradeMapping != null)
                {
                    foreach (var GradeMapping in isExtracurricularGradeMapping)
                    {
                        GradeMapping.IsActive = false;
                    }
                    _dbContext.Entity<TrExtracurricularGradeMapping>().UpdateRange(isExtracurricularGradeMapping);
                }

                isExtracurricularExist.IsActive = false;
                _dbContext.Entity<MsExtracurricular>().Update(isExtracurricularExist);

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
