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
    public class SummaryPathwayGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public SummaryPathwayGradeHandler(ISchoolDbContext schoolDbContext)
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


            var query = _dbContext.Entity<MsGradePathway>()
                .Include(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                .Include(x => x.Grade)
                .Where(x => x.IdGrade == param.IdGrade)
                .Where(x =>
                (!string.IsNullOrEmpty(param.Search) ? x.GradePathwayDetails.Any(c => c.Pathway.Code.Contains(param.Search)) : true) ||
                (!string.IsNullOrEmpty(param.Search) ? x.GradePathwayDetails.Any(c => c.Pathway.Description.Contains(param.Search)) : true)
                )
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, string.Join(",", x.GradePathwayDetails.Select(x => x.Pathway.Description))))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.Id,
                        Code = string.Join(",", x.GradePathwayDetails.Select(x => x.Pathway.Code)),
                        Description = string.Join(",", x.GradePathwayDetails.Select(x => x.Pathway.Description))
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
