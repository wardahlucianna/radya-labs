using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.GradePathwayDetails;
using BinusSchool.Data.Model.School.FnSchool.GradePathways;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradePathwayDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradePathwayDetailHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradePathwayDetailRequest>(nameof(GetGradePathwayDetailRequest.IdGrade));
            var columns = new[] { "code", "acadyear" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[1], "Grade.Level.AcademicYear.code" }
            };

            var predicate = PredicateBuilder.Create<MsGradePathwayDetail>(x => param.IdGrade.Contains(x.GradePathway.IdGrade));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Pathway.Code, $"%{param.Search}%"));

            var query = _dbContext.Entity<MsGradePathwayDetail>()
                .Include(x => x.GradePathway).ThenInclude(x => x.Grade.Level.AcademicYear)
                .Include(x => x.Pathway)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(
                        x.IdGradePathway,
                        string.Join(",", x.Pathway.Code)
                        )
                    )
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetGradePathwayResult
                    {
                        Id = x.IdGradePathway,
                        Code = x.Pathway.Code,
                        Description = x.Pathway.Description,
                    })
                    .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
