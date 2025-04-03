using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class UpdateExtracurricularStudentScoreHandler : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public UpdateExtracurricularStudentScoreHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateExtracurricularStudentScoreRequest, UpdateExtracurricularStudentScoreValidator>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var isExtracurricularExist = await _dbContext.Entity<MsExtracurricular>()
                       .Where(x => x.Id == body.IdExtracurricular)
                       .FirstOrDefaultAsync(CancellationToken);              

                if (isExtracurricularExist is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Extracurricular"], "Id", body.IdExtracurricular));

                //handle double data
                var HandleInsertDouble = body.StudentScores.Where(a => string.IsNullOrEmpty(a.IdExtracurricularScoreEntry) && a.IdExtracurricularScoreLegend != null).ToList();
                var updateIdExtracurricularScoreEntry = await _dbContext.Entity<TrExtracurricularScoreEntry>()
                    .Where(x => HandleInsertDouble.Select(a => body.IdExtracurricular+"/"+a.IdExtracurricularScoreComponent+"/"+a.IdStudent).Contains(x.IdExtracurricular+"/"+x.IdExtracurricularScoreComponent+"/"+x.IdStudent))
                    .OrderByDescending(a => a.DateIn)
                    .ToListAsync(CancellationToken);

                if(updateIdExtracurricularScoreEntry.Count > 0)
                {
                    foreach(var updateData in updateIdExtracurricularScoreEntry)
                    {
                        foreach(var studData in body.StudentScores)
                        {
                            if((body.IdExtracurricular+ "/"+ studData.IdExtracurricularScoreComponent + "/" + studData.IdStudent) == (updateData.IdExtracurricular + "/" + updateData.IdExtracurricularScoreComponent + "/" + updateData.IdStudent))
                            {
                                studData.IdExtracurricularScoreEntry = updateData.Id;
                                break;
                            }
                        }                       
                    }
                }

                var ScoreUpdate = body.StudentScores.Where(a => a.IdExtracurricularScoreEntry != null && a.IdExtracurricularScoreLegend != null).ToList();

                var StudentScoreUpdated = await _dbContext.Entity<TrExtracurricularScoreEntry>()
                     .Where(x => ScoreUpdate.Select(a => a.IdExtracurricularScoreEntry).Contains(x.Id) && !ScoreUpdate.Select(a => a.IdExtracurricularScoreEntry + "/"+a.IdExtracurricularScoreLegend).Contains(x.Id+"/"+x.IdExtracurricularScoreLegend))
                     .ToListAsync(CancellationToken);

                var ScoreDelete = body.StudentScores.Where(a => a.IdExtracurricularScoreEntry != null &  a.IdExtracurricularScoreLegend == null).ToList();

                var StudentScoreDeleted = await _dbContext.Entity<TrExtracurricularScoreEntry>()
                                            .Where(a => ScoreDelete.Select(b => b.IdExtracurricularScoreEntry).Contains(a.Id)).ToListAsync();

                var StudentScoreInserted = body.StudentScores.Where(a => a.IdExtracurricularScoreEntry == null && a.IdExtracurricularScoreLegend != null)
                                                            .Select(c => new TrExtracurricularScoreEntry() { 
                                                                Id = Guid.NewGuid().ToString(),
                                                                IdStudent = c.IdStudent,
                                                                IdExtracurricular = body.IdExtracurricular,
                                                                IdExtracurricularScoreComponent = c.IdExtracurricularScoreComponent,
                                                                IdExtracurricularScoreLegend = c.IdExtracurricularScoreLegend                                                                
                                                            }).ToList();
                            
                foreach (var UpdateStudentScore in StudentScoreUpdated)
                {
                    UpdateStudentScore.IdExtracurricularScoreLegend = body.StudentScores.Where(a => a.IdStudent == UpdateStudentScore.IdStudent && a.IdExtracurricularScoreComponent == UpdateStudentScore.IdExtracurricularScoreComponent).First().IdExtracurricularScoreLegend;
                    UpdateStudentScore.UserUp = AuthInfo.UserId;
                    _dbContext.Entity<TrExtracurricularScoreEntry>().Update(UpdateStudentScore);
                }

                if(StudentScoreDeleted.Count() > 0)
                {
                    _dbContext.Entity<TrExtracurricularScoreEntry>().RemoveRange(StudentScoreDeleted);
                }

                if(StudentScoreInserted.Count() > 0)
                {
                    _dbContext.Entity<TrExtracurricularScoreEntry>().AddRange(StudentScoreInserted);
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
