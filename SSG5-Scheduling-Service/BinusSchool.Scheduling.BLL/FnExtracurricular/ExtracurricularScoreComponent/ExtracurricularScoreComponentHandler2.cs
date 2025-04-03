using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScoreComponent;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreComponent.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScoreComponent
{
    public class ExtracurricularScoreComponentHandler2 : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ExtracurricularScoreComponentHandler2(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var datas = await _dbContext.Entity<MsExtracurricularScoreCompCategory>()
                            .Include(x => x.ExtracurricularScoreComponents)
                            .Where(x => ids.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));



            var CannotbeDeleted = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                            .Include(a => a.ExtracurricularScoreEntries)
                            .Include(a => a.ExtracurricularScoreCompCategory)
                            .Where(x => ids.Any(y => y == x.IdExtracurricularScoreCompCategory)
                            && x.ExtracurricularScoreEntries.Count() > 0)
                            .ToListAsync(CancellationToken);

            if (CannotbeDeleted.Count() > 0)
            {
                undeleted.AlreadyUse = ids.ToDictionary(x => x, x => string.Format(Localizer["ExAlreadyUse"], x));
                await Transaction.RollbackAsync(CancellationToken);
                return Request.CreateApiResult2(errors: undeleted.AsErrors());
            }

            _dbContext.Entity<MsExtracurricularScoreCompCategory>().RemoveRange(datas);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetExtracurricularScoreComponentRequest2>(nameof(GetExtracurricularScoreComponentRequest2.IdAcademicYear));

            var acadYear = await _dbContext.Entity<MsAcademicYear>().FindAsync(param.IdAcademicYear);
            if (acadYear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", param.IdAcademicYear));

            //MsExtracurricularScoreComponent
            IReadOnlyList<IItemValueVm> items;
            items = await _dbContext.Entity<MsExtracurricularScoreCompCategory>()
                                  .Where(a => a.IdAcademicYear == param.IdAcademicYear)
                                  .Select(a => new GetExtracurricularScoreComponentResult2()
                                  {                                      
                                      Id = a.Id,                                  
                                      Description = a.Description,
                                      CalculationType = new ItemValueVm
                                      {
                                          Id = a.IdExtracurricularScoreCalculationType,
                                          Description = a.ExtracurricularScoreCalculationType.CalculationType
                                      },
                                      ScoreComponentList = a.ExtracurricularScoreComponents.Select(b => new ScoreComponentVm()
                                      {
                                          IdExtracurricularScoreComponent  = b.Id,
                                          Description = b.Description,
                                          OrderNumber = b.OrderNumber
                                      }).OrderBy(a => a.OrderNumber).ToList()

                                  })                           
                                  .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddExtracurricularScoreComponentRequest2, AddExtracurricularScoreComponentValidator2>();

            var acadYear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
            if (acadYear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.IdAcademicYear));

            if(body.ScoreCompCategoryDeleted != null)
            {
                var ids = body.ScoreCompCategoryDeleted;
                var datas = await _dbContext.Entity<MsExtracurricularScoreCompCategory>()
                          .Include(x => x.ExtracurricularScoreComponents)
                          .Where(x => ids.Any(y => y == x.Id))
                          .ToListAsync(CancellationToken);                           

                var CannotbeDeleted = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                                .Include(a => a.ExtracurricularScoreEntries)
                                .Include(a => a.ExtracurricularScoreCompCategory)
                                .Where(x => ids.Any(y => y == x.IdExtracurricularScoreCompCategory)
                                && x.ExtracurricularScoreEntries.Count() > 0)
                                .ToListAsync(CancellationToken);

                if (CannotbeDeleted.Count() > 0)
                {
                   throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ExtracurricularScoreCompCategory"], "Id", string.Join(",", CannotbeDeleted.Select(a => a.ExtracurricularScoreCompCategory.Id))));

                }

                _dbContext.Entity<MsExtracurricularScoreCompCategory>().RemoveRange(datas);
            }

            foreach (var itemCategory in body.ScoreComponentCategories)
            {
                var ScoreComponentCategory = await _dbContext.Entity<MsExtracurricularScoreCompCategory>().Where(a => a.Id == itemCategory.IdExtracurricularScoreCompCategory).FirstOrDefaultAsync();

                if (ScoreComponentCategory != null)
                {
                    //update               

                    var ScoreComponents = await _dbContext.Entity<MsExtracurricularScoreComponent>().Where(a => itemCategory.ScoreComponents.Select(b => b.IdExtracurricularScoreComponent).Contains(a.Id)
                                                                                                                && a.IdExtracurricularScoreCompCategory == itemCategory.IdExtracurricularScoreCompCategory)
                                                                                             .ToListAsync(CancellationToken);

                    foreach (var itmComponent in ScoreComponents)
                    {
                        var itemChanges = itemCategory.ScoreComponents.Where(a => a.IdExtracurricularScoreComponent == itmComponent.Id).FirstOrDefault();

                        if (itemChanges != null)
                        {
                            if (itmComponent.Description.Trim() != itemChanges.Description || itmComponent.OrderNumber != itemChanges.OrderNumber)
                            {
                                itmComponent.Description = itemChanges.Description;
                                itmComponent.OrderNumber = itemChanges.OrderNumber;
                                _dbContext.Entity<MsExtracurricularScoreComponent>().Update(itmComponent);
                            }
                        }
                    }

                    var existsScoreComponents = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                                      .Include(a => a.ExtracurricularScoreEntries)
                                      .Where(a => a.IdAcademicYear == body.IdAcademicYear
                                      && a.IdExtracurricularScoreCompCategory == itemCategory.IdExtracurricularScoreCompCategory)
                                      .ToListAsync(CancellationToken);

                    var ScoreComponentsDeleted = existsScoreComponents.Where(a => !itemCategory.ScoreComponents.Select(b => b.IdExtracurricularScoreComponent).Contains(a.Id)).ToList();

                    var checkScoreComponentDelete = ScoreComponentsDeleted.Where(a => a.ExtracurricularScoreEntries.Count() > 0).Count();

                    if (checkScoreComponentDelete > 0)
                    {
                        throw new BadRequestException(string.Format(Localizer["ExAlreadyUse"], Localizer["ExtracurricularScoreComponent"], "Id", string.Join(",", ScoreComponentsDeleted.Select(a => a.Id))));
                    }

                    ScoreComponentCategory.Description = itemCategory.Description;
                    ScoreComponentCategory.IdAcademicYear = body.IdAcademicYear;
                    ScoreComponentCategory.IdExtracurricularScoreCalculationType = itemCategory.IdExtracurricularScoreCalculationType;
                    _dbContext.Entity<MsExtracurricularScoreCompCategory>().Update(ScoreComponentCategory);

                    _dbContext.Entity<MsExtracurricularScoreComponent>().RemoveRange(ScoreComponentsDeleted);

                    var ScoreComponentsInserted = itemCategory.ScoreComponents.Where(a => !existsScoreComponents.Select(b => b.Id).Contains(a.IdExtracurricularScoreComponent))
                                                                        .Select(c => new MsExtracurricularScoreComponent()
                                                                        {
                                                                            Id = Guid.NewGuid().ToString(),
                                                                            IdAcademicYear = body.IdAcademicYear,
                                                                            Description = c.Description,
                                                                            OrderNumber = c.OrderNumber,
                                                                            IdExtracurricularScoreCompCategory = itemCategory.IdExtracurricularScoreCompCategory
                                                                        }).ToList();

                    _dbContext.Entity<MsExtracurricularScoreComponent>().AddRange(ScoreComponentsInserted);

                }
                else
                {
                    //insert
                    var insertCategory = new MsExtracurricularScoreCompCategory();
                    var GuidCategory = Guid.NewGuid().ToString();

                    insertCategory.Id = GuidCategory;
                    insertCategory.Description = itemCategory.Description;
                    insertCategory.IdAcademicYear = body.IdAcademicYear;
                    insertCategory.IdExtracurricularScoreCalculationType = itemCategory.IdExtracurricularScoreCalculationType;
                    _dbContext.Entity<MsExtracurricularScoreCompCategory>().AddRange(insertCategory);

                    var insertCompScore = itemCategory.ScoreComponents.Select(a => new MsExtracurricularScoreComponent
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdAcademicYear = body.IdAcademicYear,
                        Description = a.Description,
                        OrderNumber = a.OrderNumber,
                        IdExtracurricularScoreCompCategory = GuidCategory
                    }).ToList();

                    if (insertCompScore.Count > 0)
                    {
                        _dbContext.Entity<MsExtracurricularScoreComponent>().AddRange(insertCompScore);
                    }

                }
            }

         

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
