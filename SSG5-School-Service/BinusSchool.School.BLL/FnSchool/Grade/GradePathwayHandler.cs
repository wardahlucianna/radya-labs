using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSchool.GradePathways;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradePathwayHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradePathwayHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradePathwayRequest>(nameof(GetGradePathwayRequest.IdSchool));
            var columns = new[] { "code", "acadyear" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[1], "Grade.Level.AcademicYear.code" }
            };

            var query = _dbContext.Entity<MsGradePathway>()
                .Include(x => x.Grade.Level.AcademicYear)
                .Where(x => param.IdSchoolAcadyear != null ? param.IdSchoolAcadyear.Contains(x.Grade.Level.IdAcademicYear) : param.IdSchool.Contains(x.Grade.Level.AcademicYear.IdSchool)
                         && (EF.Functions.Like(x.Grade.Code, param.SearchPattern())
                         || EF.Functions.Like(x.Grade.Description, param.SearchPattern())))
                // .SearchByDynamic(param)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Grade.Id, x.Grade.Code))
                    .Distinct()
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetGradePathwayResult
                    {
                        Id = x.Id,
                        Code = x.Grade.Code,
                        Description = x.Grade.Description,
                    })
                    .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
