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
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ConcernCategory;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.ConcernCategory.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.ConcernCategory
{
    public class ConcernCategoryHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public ConcernCategoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsConcernCategory>()
                .Include(x => x.CounselingServicesEntryConcern)
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
                if (data.CounselingServicesEntryConcern.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.ConcernCategoryName ?? data.ConcernCategoryName ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsConcernCategory>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsConcernCategory>()
                .Select(x => new GetDetailConcernCategoryResult
                {
                    Id = x.Id,
                    ConcernCategoryName = x.ConcernCategoryName
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetConcernCategoryRequest>();
            string[] _columns = { "ConcernCategoryName" };

            var predicate = PredicateBuilder.Create<MsConcernCategory>(x => true);

            if (!string.IsNullOrWhiteSpace(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.ConcernCategoryName, param.SearchPattern()));

            var query = _dbContext.Entity<MsConcernCategory>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items = default;

            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetConcernCategoryResult() { 
                        Id = x.Id,
                        ConcernCategoryName = x.ConcernCategoryName
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetConcernCategoryResult()
                    {
                        Id = x.Id,
                        ConcernCategoryName = x.ConcernCategoryName
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddConcernCategoryRequest, AddConcernCategoryValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isExist = await _dbContext.Entity<MsConcernCategory>()
                .Where(x => x.ConcernCategoryName.ToLower() == body.ConcernCategoryName.ToLower() && x.IdSchool == body.IdSchool)
                .FirstOrDefaultAsync();

            if (isExist != null)
                throw new BadRequestException($"{body.ConcernCategoryName} already exists");

            var param = new MsConcernCategory
            {
                Id = Guid.NewGuid().ToString(),
                ConcernCategoryName = body.ConcernCategoryName,
                IdSchool = body.IdSchool,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsConcernCategory>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateConcernCategoryRequest, UpdateConcernCategoryValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsConcernCategory>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Building"], "Id", body.Id));

            var isExist = await _dbContext.Entity<MsConcernCategory>()
                .Where(x => x.Id != body.Id && x.ConcernCategoryName.ToLower() == body.ConcernCategoryName.ToLower())
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.ConcernCategoryName} already exists");

            data.ConcernCategoryName = body.ConcernCategoryName;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsConcernCategory>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
