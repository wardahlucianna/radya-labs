using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Pathway
{
    public class PathwayGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public PathwayGradeHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetPathwayGradeRequest>(nameof(GetPathwayGradeRequest.IdGrade));
            var columns = new[] { "code" };
            var aliasColumns = new Dictionary<string, string>
                {
                    { columns[0], "pathway.code" }
                };

            var predicate = PredicateBuilder.Create<MsGradePathwayDetail>(x
                => EF.Functions.Like(x.Pathway.Code, param.SearchPattern())
                || EF.Functions.Like(x.Pathway.Description, param.SearchPattern()));

            var query = _dbContext.Entity<MsGradePathwayDetail>()
                .Include(x => x.GradePathway).ThenInclude(x => x.Grade)
                .Include(x => x.Pathway)
                .Where(predicate)
                .Where(x=>x.GradePathway.Grade.Id == param.IdGrade)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Pathway.Id, x.Pathway.Description ?? x.Pathway.Code))
                    .Distinct()
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.Pathway.Id,
                        Code = x.Pathway.Code,
                        Description = x.Pathway.Description
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
