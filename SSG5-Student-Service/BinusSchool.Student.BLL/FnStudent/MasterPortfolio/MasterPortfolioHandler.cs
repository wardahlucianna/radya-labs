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
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.MasterPortfolio.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnStudent.MasterPortfolio
{
    public class MasterPortfolioHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;

        public MasterPortfolioHandler(IStudentDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }
        protected async override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsLearnerProfile>()
                .Where(x => ids.Any(y => y == x.Id))
                .ToListAsync(CancellationToken);

            var undeleted = new UndeletedResult2();

            // find not found ids
            ids = ids.Except(ids.Intersect(datas.Select(x => x.Id)));
            undeleted.NotFound = ids.ToDictionary(x => x, x => string.Format(Localizer["ExNotFound"], x));

            // find already used ids
            foreach (var data in datas)
            {
                // don't set inactive when row have to-many relation
                // if (data.LearningGoalStudent.Any(x => x.IsActive))
                // {
                //     undeleted.AlreadyUse ??= new Dictionary<string, string>();
                //     undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.Description ?? data.Code ?? data.Id));
                // }
                // else
                // {
                    data.IsActive = false;
                    _dbContext.Entity<MsLearnerProfile>().Update(data);
                // }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected async override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsLearnerProfile>()
                .Select(x => new GetMasterPortfolioDetailResult
                {
                    Id = x.Id,
                    Name = x.Name,
                    Type = x.Type
                })
                .FirstOrDefaultAsync(x => x.Id == id, CancellationToken);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetMasterPortfolioRequest>(nameof(GetMasterPortfolioRequest.Type));

            var columns = new[] { "Name", "TypeName"};
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "Name" },
                { columns[1], "TypeName"}
            };

            var predicate = PredicateBuilder.Create<MsLearnerProfile>(x => true);
                predicate = predicate.And(x => x.Type == param.Type);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Name, param.SearchPattern()));

            var query = _dbContext.Entity<MsLearnerProfile>()
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> itemsLov;
            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
            {
                itemsLov = await query
                    .Select(x => new ItemValueVm(x.Id, x.Name))
                    .ToListAsync(CancellationToken);
                return Request.CreateApiResult2(itemsLov);
            }
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new ItemValueVm(x.Id, x.Name))
                    .ToListAsync(CancellationToken);
                    
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
            }
        }

        protected async override Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddMasterPortfolioRequest, AddMasterPortfolioValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isCodeExist = await _dbContext.Entity<MsLearnerProfile>()
                .Where(x => x.Type == body.Type && x.Name.ToLower() == body.Name.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isCodeExist != null)
                throw new BadRequestException($"{body.Name} already exists");

            var param = new MsLearnerProfile
            {
                Id = Guid.NewGuid().ToString(),
                Name = body.Name,
                Type = body.Type
            };

            _dbContext.Entity<MsLearnerProfile>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected async override Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateMasterPortfolioRequest, UpdateMasterPortfolioValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsLearnerProfile>()
                .Where(x => x.Id == body.Id)
                .FirstOrDefaultAsync(CancellationToken);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id"], "Id", body.Id));

            var isCodeExist = await _dbContext.Entity<MsLearnerProfile>()
                .Where(x => x.Id != body.Id && x.Type == body.Type && x.Name.ToLower() == body.Name.ToLower())
                .FirstOrDefaultAsync(CancellationToken);
            if (isCodeExist != null)
                throw new BadRequestException($"{body.Name} already exists");

            data.Type = body.Type;
            data.Name = body.Name;

            _dbContext.Entity<MsLearnerProfile>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
