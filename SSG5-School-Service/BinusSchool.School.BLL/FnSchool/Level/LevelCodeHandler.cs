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
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Level.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Level
{
    public class LevelCodeHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public LevelCodeHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetLevelCodeRequest>(nameof(GetLevelCodeRequest.IdSchool));
            var columns = new[] { "acadyear", "description", "code" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "academicYear.code" }
            };

            var predicate = PredicateBuilder.Create<MsLevel>(x => param.IdSchool.Any(y => y == x.AcademicYear.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern()));

            if (!string.IsNullOrEmpty(param.CodeLevel))
                predicate = predicate.And(x => x.Code == param.CodeLevel);

            var query = _dbContext.Entity<MsLevel>()
                .Where(predicate)
                .Where(x => (string.IsNullOrEmpty(param.CodeAcademicYear) ? true : x.AcademicYear.Code.CompareTo(param.CodeAcademicYear) == 0)
                            )
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm
                    {
                        Code = x.Code,
                        Description = x.Description
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .GroupBy(x => new { x.Code, x.Description })
                    .SetPagination(param)
                    .Select(x => new GetLevelCodeResult
                    {
                        Code = x.Key.Code,
                        Description = x.Key.Description
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override async Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
