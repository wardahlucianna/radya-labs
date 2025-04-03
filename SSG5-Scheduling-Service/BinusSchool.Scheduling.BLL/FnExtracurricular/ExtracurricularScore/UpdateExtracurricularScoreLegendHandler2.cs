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
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class UpdateExtracurricularScoreLegendHandler2 : FunctionsHttpSingleHandler
    {
        private IDbContextTransaction _transaction;

        private readonly ISchedulingDbContext _dbContext;

        public UpdateExtracurricularScoreLegendHandler2(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<UpdateExtracurricularScoreLegendRequest2, UpdateExtracurricularScoreLegendValidator2>();

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var isExtracurricularExist = await _dbContext.Entity<MsSchool>()
                       .Where(x => x.Id == body.IdSchool)
                       .FirstOrDefaultAsync(CancellationToken);

                if (isExtracurricularExist is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["School"], "Id", body.IdSchool));


                if (body.ScoreLegendCategoryDeleted != null)
                {
                    var ScoreLegendCategoryDelete = await _dbContext.Entity<MsExtracurricularScoreLegendCategory>()
                                        .Where(a => body.ScoreLegendCategoryDeleted.Contains(a.Id))
                                        .ToListAsync();


                    var CheckScoreLegendCategoryDelete = await _dbContext.Entity<MsExtracurricularScoreLegend>()
                                       .Include(x => x.ExtracurricularScoreEntries)
                                       .Include(x => x.ExtracurricularScoreLegendCategory)
                                       .Where(x => ScoreLegendCategoryDelete.Select(a => a.Id).Contains(x.ExtracurricularScoreLegendCategory.Id)
                                       && x.ExtracurricularScoreEntries.Count() > 0)
                                       .ToListAsync(CancellationToken);


                    if ((CheckScoreLegendCategoryDelete?.Count() ?? 0) > 0)
                    {
                        throw new BadRequestException(string.Format(Localizer["ExAlreadyUse"], Localizer["ExtracurricularScoreLegend"], "Id", string.Join(",", CheckScoreLegendCategoryDelete.Select(a => a.ExtracurricularScoreLegendCategory.Id))));
                    }

                    if (ScoreLegendCategoryDelete.Count() > 0)
                    {
                        _dbContext.Entity<MsExtracurricularScoreLegendCategory>().RemoveRange(ScoreLegendCategoryDelete);
                    }
                }


                foreach (var itemCategory in body.ScoreLegendCategories)
                {
                    var isExtracurricularScoreLegendCategoryExist = await _dbContext.Entity<MsExtracurricularScoreLegendCategory>()
                    .Where(x => x.Id == itemCategory.IdExtracurricularScoreLegendCategory)
                    .FirstOrDefaultAsync(CancellationToken);

                    //update
                    if (isExtracurricularScoreLegendCategoryExist != null)
                    {                
                        var ScoreLegendUpdate = await _dbContext.Entity<MsExtracurricularScoreLegend>()
                                              .Where(x => itemCategory.ScoreLegends.Select(a => a.IdExtracurricularScoreLegend).Contains(x.Id)
                                                         && !itemCategory.ScoreLegends.Select(a => a.IdExtracurricularScoreLegend+ "/"+ a.Score.Trim() + "/" + a.Description.Trim()).Contains(x.Id+"/"+x.Score.Trim() + "/" + x.Description.Trim())
                                                         && x.IdSchool == body.IdSchool
                                                         && x.IdExtracurricularScoreLegendCategory == itemCategory.IdExtracurricularScoreLegendCategory)
                                              .ToListAsync(CancellationToken);

                        var ScoreLegendInsert = itemCategory.ScoreLegends.Where(a => a.IdExtracurricularScoreLegend is null)
                                                            .Select(c => new MsExtracurricularScoreLegend()
                                                            {
                                                                Id = Guid.NewGuid().ToString(),
                                                                Score = c.Score,
                                                                Description = c.Description,
                                                                IdSchool = body.IdSchool,
                                                                IdExtracurricularScoreLegendCategory = itemCategory.IdExtracurricularScoreLegendCategory
                                                            }).ToList();

                        var ScoreLegendDelete = await _dbContext.Entity<MsExtracurricularScoreLegend>()
                                      .Where(x => !itemCategory.ScoreLegends.Select(a => a.IdExtracurricularScoreLegend).Contains(x.Id)
                                      && x.IdSchool == body.IdSchool
                                      && x.IdExtracurricularScoreLegendCategory == itemCategory.IdExtracurricularScoreLegendCategory)
                                      .ToListAsync(CancellationToken);

                        var CheckScoreLegendDelete = await _dbContext.Entity<MsExtracurricularScoreLegend>()
                                           .Include(x => x.ExtracurricularScoreEntries)
                                           .Where(x => ScoreLegendDelete.Select(a => a.Id).Contains(x.Id)
                                           && x.ExtracurricularScoreEntries.Count() > 0)
                                           .ToListAsync(CancellationToken);


                        if ((CheckScoreLegendDelete?.Count() ?? 0) > 0)
                        {
                            throw new BadRequestException(string.Format(Localizer["ExAlreadyUse"], Localizer["ExtracurricularScoreLegend"], "Id", string.Join(",", CheckScoreLegendDelete.Select(a => a.Id))));
                        }


                        if (ScoreLegendDelete.Count() > 0)
                        {
                            _dbContext.Entity<MsExtracurricularScoreLegend>().RemoveRange(ScoreLegendDelete);
                        }


                        isExtracurricularScoreLegendCategoryExist.Description = itemCategory.Description;
                        isExtracurricularScoreLegendCategoryExist.IdSchool = body.IdSchool;
                        _dbContext.Entity<MsExtracurricularScoreLegendCategory>().Update(isExtracurricularScoreLegendCategoryExist);

                        foreach (var UpdateLegendScore in ScoreLegendUpdate)
                        {
                            var LegendScore = itemCategory.ScoreLegends.Where(a => a.IdExtracurricularScoreLegend == UpdateLegendScore.Id).First();
                            UpdateLegendScore.Score = LegendScore.Score;
                            UpdateLegendScore.Description = LegendScore.Description;
                            UpdateLegendScore.UserUp = AuthInfo.UserId;
                            _dbContext.Entity<MsExtracurricularScoreLegend>().Update(UpdateLegendScore);
                        }

                        if (ScoreLegendInsert.Count() > 0)
                        {
                            _dbContext.Entity<MsExtracurricularScoreLegend>().AddRange(ScoreLegendInsert);
                        }

                    }
                    else
                    {
                        //insert

                        var newGuid = Guid.NewGuid().ToString();
                        MsExtracurricularScoreLegendCategory InsertCategory = new MsExtracurricularScoreLegendCategory();
                        InsertCategory.IdSchool = body.IdSchool;
                        InsertCategory.Description = itemCategory.Description;
                        InsertCategory.Id = newGuid;
                        _dbContext.Entity<MsExtracurricularScoreLegendCategory>().Add(InsertCategory);

                        var insertLegend = itemCategory.ScoreLegends.Select(a => new MsExtracurricularScoreLegend
                        {
                            Id = Guid.NewGuid().ToString(),
                            Score = a.Score,
                            Description = a.Description,
                            IdSchool = body.IdSchool,
                            IdExtracurricularScoreLegendCategory = newGuid
                        }).ToList();

                        if (insertLegend.Count() > 0)
                        {
                            _dbContext.Entity<MsExtracurricularScoreLegend>().AddRange(insertLegend);
                        }

                    }

                    await _dbContext.SaveChangesAsync(CancellationToken);
                }

              
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
