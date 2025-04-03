using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingCategory;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnGuidanceCounseling.CounselingCategory.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingCategory
{
    public class CounselingCategoryHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        public CounselingCategoryHandler(IStudentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);
            var datas = await _dbContext.Entity<MsCounselingCategory>()
                .Include(x => x.CounselingServicesEntry)
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
                if (data.CounselingServicesEntry.Count != 0)
                {
                    undeleted.AlreadyUse ??= new Dictionary<string, string>();
                    undeleted.AlreadyUse.Add(data.Id, string.Format(Localizer["ExAlreadyUse"], data.CounselingCategoryName ?? data.CounselingCategoryName ?? data.Id));
                }
                else
                {
                    data.IsActive = false;
                    _dbContext.Entity<MsCounselingCategory>().Update(data);
                }
            }

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2(errors: undeleted.AsErrors());
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var data = await _dbContext.Entity<MsCounselingCategory>()
                .Select(x => new GetDetailCounselingCategoryResult
                {
                    Id = x.Id,
                    CounselingCategoryName = x.CounselingCategoryName
                })
                .FirstOrDefaultAsync(x => x.Id == id);

            return Request.CreateApiResult2(data as object);
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetCounselingCategoryRequest>();
            string[] _columns = { "CounselingCategoryName" };

            var predicate = PredicateBuilder.Create<MsCounselingCategory>(x => true);

            if (!string.IsNullOrWhiteSpace(param.IdSchool))
                predicate = predicate.And(x => x.IdSchool == param.IdSchool);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.CounselingCategoryName, param.SearchPattern()));

            var query = _dbContext.Entity<MsCounselingCategory>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;

            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new GetCounselingCategoryResult()
                    {
                        Id = x.Id,
                        CounselingCategoryName = x.CounselingCategoryName
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetCounselingCategoryResult()
                    {
                        Id = x.Id,
                        CounselingCategoryName = x.CounselingCategoryName
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(_columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            var body = await Request.ValidateBody<AddCounselingCategoryRequest, AddCounselingCategoryValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var isExist = await _dbContext.Entity<MsCounselingCategory>()
                .Where(x => x.CounselingCategoryName.ToLower() == body.CounselingCategoryName.ToLower() && x.IdSchool == body.IdSchool)
                .FirstOrDefaultAsync();

            if (isExist != null)
                throw new BadRequestException($"{body.CounselingCategoryName} already exists");

            var param = new MsCounselingCategory
            {
                Id = Guid.NewGuid().ToString(),
                IdSchool = body.IdSchool,
                CounselingCategoryName = body.CounselingCategoryName,
                UserIn = AuthInfo.UserId
            };

            _dbContext.Entity<MsCounselingCategory>().Add(param);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateCounselingCategoryRequest, UpdateCounselingCategoryValidator>();
            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            var data = await _dbContext.Entity<MsCounselingCategory>().FindAsync(body.Id);
            if (data is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Building"], "Id", body.Id));

            var isExist = await _dbContext.Entity<MsCounselingCategory>()
                .Where(x => x.Id != body.Id && x.CounselingCategoryName.ToLower() == body.CounselingCategoryName.ToLower())
                .FirstOrDefaultAsync();
            if (isExist != null)
                throw new BadRequestException($"{body.CounselingCategoryName} already exists");

            data.CounselingCategoryName = body.CounselingCategoryName;
            data.UserUp = AuthInfo.UserId;

            _dbContext.Entity<MsCounselingCategory>().Update(data);

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

            return Request.CreateApiResult2();
        }
    }
}
