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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ElectivesObjective;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.ElectivesObjective.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnExtracurricular.ElectivesObjective
{
    public class ElectivesObjectiveHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ElectivesObjectiveHandler(ISchedulingDbContext DbContext)
        {
            _dbContext = DbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsExtracurricularObjective>()
                            .Where(x => ids.Any(y => y == x.Id))
                            .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            _dbContext.Entity<MsExtracurricularObjective>().RemoveRange(datas);

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
            var param = Request.ValidateParams<GetElectivesObjectiveRequest>(nameof(GetElectivesObjectiveRequest.IdExtracurricular));

            var extracurricularData = await _dbContext.Entity<MsExtracurricular>().FindAsync(param.IdExtracurricular);
            if (extracurricularData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["extracurricular"], "Id", param.IdExtracurricular));

            IReadOnlyList<IItemValueVm> items;
            items = await _dbContext.Entity<MsExtracurricularObjective>()
                                  .Where(a => a.IdExtracurricular == param.IdExtracurricular)
                                  .Select(a => new GetElectivesObjectiveResult()
                                  {
                                      OrderNumber = a.OrderNumber,
                                      IdElectivesObjective = a.Id,
                                      Description = a.Description
                                  })
                                  .OrderBy(a => a.OrderNumber)
                                  .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(items);
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddElectivesObjectiveRequest, AddElectivesObjectiveValidator>();

            var extracurricularData = await _dbContext.Entity<MsExtracurricular>().FindAsync(body.IdExtracurricular);
            if (extracurricularData is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["extracurricular"], "Id", body.IdExtracurricular));


            var ExtracurricularObjectives = await _dbContext.Entity<MsExtracurricularObjective>().Where(a => body.ElectivesObjectives.Select(b => b.IdElectivesObjective).Contains(a.Id))
                                                                                            .ToListAsync(CancellationToken);

            foreach (var itmComponent in ExtracurricularObjectives)
            {
                var itemChanges = body.ElectivesObjectives.Where(a => a.IdElectivesObjective == itmComponent.Id).FirstOrDefault();

                if (itemChanges != null)
                {
                    if (itmComponent.Description.Trim() != itemChanges.Description.Trim() || itmComponent.OrderNumber != itemChanges.OrderNumber)
                    {
                        itmComponent.Description = itemChanges.Description;
                        itmComponent.OrderNumber = itemChanges.OrderNumber;
                        _dbContext.Entity<MsExtracurricularObjective>().Update(itmComponent);
                    }
                }
            }

            var existsObjectives = await _dbContext.Entity<MsExtracurricularObjective>()                        
                            .Where(a => a.IdExtracurricular == body.IdExtracurricular)
                            .ToListAsync(CancellationToken);

            var ObjectivesDeleted = existsObjectives.Where(a => !body.ElectivesObjectives.Select(b => b.IdElectivesObjective).Contains(a.Id)).ToList();
                 
            _dbContext.Entity<MsExtracurricularObjective>().RemoveRange(ObjectivesDeleted);

            var ObjectivesInserted = body.ElectivesObjectives.Where(a => !existsObjectives.Select(b => b.Id).Contains(a.IdElectivesObjective))
                                                                .Select(c => new MsExtracurricularObjective()
                                                                {
                                                                    Id = Guid.NewGuid().ToString(),
                                                                    IdExtracurricular = body.IdExtracurricular,
                                                                    Description = c.Description,
                                                                    OrderNumber = c.OrderNumber
                                                                }).ToList();

            _dbContext.Entity<MsExtracurricularObjective>().AddRange(ObjectivesInserted);

            await _dbContext.SaveChangesAsync(CancellationToken);
            return Request.CreateApiResult2();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
