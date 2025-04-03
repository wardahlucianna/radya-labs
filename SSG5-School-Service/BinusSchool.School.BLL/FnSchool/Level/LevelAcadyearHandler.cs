using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Level
{
    public class LevelAcadyearHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public LevelAcadyearHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetLevelRequest>(nameof(GetLevelRequest.IdSchool));
            var columns = new[] { "acadyear", "description", "code" };

            var predicate = PredicateBuilder.Create<MsLevel>(x => param.IdSchool.Any(y => y == x.AcademicYear.IdSchool));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Code, param.SearchPattern()));

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Id == param.IdLevel);

            var query = _dbContext.Entity<MsLevel>()
                .Where(predicate)
                .OrderByDynamic(param);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetLevelResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Acadyear = x.AcademicYear.Description
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
