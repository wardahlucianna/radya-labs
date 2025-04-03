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
    public class ExtracurricularScoreComponentHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ExtracurricularScoreComponentHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }
    
        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsExtracurricularScoreComponent>()                
                            .Where(x => ids.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));


            var CannotbeDeleted = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                            .Include(a => a.ExtracurricularScoreEntries)
                            .Where(x => ids.Any(y => y == x.Id)
                            && x.ExtracurricularScoreEntries.Count() > 0)
                            .ToListAsync(CancellationToken);

            if(CannotbeDeleted.Count() > 0)
            {
                undeleted.AlreadyUse = ids.ToDictionary(x => x, x => string.Format(Localizer["ExAlreadyUse"], x));
                await Transaction.RollbackAsync(CancellationToken);
                return Request.CreateApiResult2(errors: undeleted.AsErrors());
            }

            _dbContext.Entity<MsExtracurricularScoreComponent>().RemoveRange(datas);

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
            var param = Request.ValidateParams<GetExtracurricularScoreComponentRequest>(nameof(GetExtracurricularScoreComponentRequest.IdAcademicYear));

            var acadYear = await _dbContext.Entity<MsAcademicYear>().FindAsync(param.IdAcademicYear);
            if (acadYear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", param.IdAcademicYear));

            IReadOnlyList<IItemValueVm> items;
            items = await _dbContext.Entity<MsExtracurricularScoreComponent>() 
                                  .Where(a => a.IdAcademicYear == param.IdAcademicYear)
                                  .Select(a => new GetExtracurricularScoreComponentResult()
                                  {
                                      OrderNumber = a.OrderNumber,
                                      IdExtracurricularScoreComponent = a.Id,
                                      Description = a.Description
                                      
                                  })
                                  .OrderBy(a => a.OrderNumber)
                                  .ToListAsync(CancellationToken);         

            return Request.CreateApiResult2(items);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
           var body = await Request.ValidateBody<AddExtracurricularScoreComponentRequest, AddExtracurricularScoreComponentValidator>();

            var acadYear = await _dbContext.Entity<MsAcademicYear>().FindAsync(body.IdAcademicYear);
            if (acadYear is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["AcademicYear"], "Id", body.IdAcademicYear));


            var ScoreComponents = await _dbContext.Entity<MsExtracurricularScoreComponent>().Where(a => body.ScoreComponents.Select(b => b.IdExtracurricularScoreComponent).Contains(a.Id))                                                                                           
                                                                                            .ToListAsync(CancellationToken);

            foreach(var itmComponent in ScoreComponents)
            {
                var itemChanges = body.ScoreComponents.Where(a => a.IdExtracurricularScoreComponent == itmComponent.Id).FirstOrDefault();

                if(itemChanges != null)
                {
                    if(itmComponent.Description.Trim() != itemChanges.Description || itmComponent.OrderNumber != itemChanges.OrderNumber)
                    {
                        itmComponent.Description = itemChanges.Description;
                        itmComponent.OrderNumber = itemChanges.OrderNumber;
                        _dbContext.Entity<MsExtracurricularScoreComponent>().Update(itmComponent);
                    }
                }
            }

            var existsScoreComponents = await _dbContext.Entity<MsExtracurricularScoreComponent>()
                              .Include(a => a.ExtracurricularScoreEntries)
                              .Where(a => a.IdAcademicYear == body.IdAcademicYear)
                              .ToListAsync(CancellationToken);

            var ScoreComponentsDeleted = existsScoreComponents.Where(a => !body.ScoreComponents.Select(b => b.IdExtracurricularScoreComponent).Contains(a.Id)).ToList();

            var checkScoreComponentDelete = ScoreComponentsDeleted.Where(a => a.ExtracurricularScoreEntries.Count() > 0).Count();

            if(checkScoreComponentDelete > 0)
            {
                throw new BadRequestException(string.Format(Localizer["ExAlreadyUse"], Localizer["ExtracurricularScoreComponent"], "Id", string.Join(",", ScoreComponentsDeleted.Select(a => a.Id))));
            }

            _dbContext.Entity<MsExtracurricularScoreComponent>().RemoveRange(ScoreComponentsDeleted);

            var ScoreComponentsInserted = body.ScoreComponents.Where(a => !existsScoreComponents.Select(b => b.Id).Contains(a.IdExtracurricularScoreComponent))
                                                                .Select(c => new MsExtracurricularScoreComponent()
                                                                {
                                                                    Id = Guid.NewGuid().ToString(),
                                                                    IdAcademicYear = body.IdAcademicYear,
                                                                    Description = c.Description,
                                                                    OrderNumber = c.OrderNumber
                                                                }).ToList();

            _dbContext.Entity<MsExtracurricularScoreComponent>().AddRange(ScoreComponentsInserted);


            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
