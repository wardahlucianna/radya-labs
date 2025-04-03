using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Data.Model.School.FnSchool.Division;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Utils;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;
using System.Collections.Generic;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities.Teaching;

namespace BinusSchool.School.FnSchool.Division
{
    public class DivisionCombinationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public DivisionCombinationHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetDivisionCombinationRequest>(nameof(GetDivisionCombinationRequest.IdTimetablePreferenceHeader));

            var predicate = PredicateBuilder.Create<MsSubjectCombination>(x => x.Id == param.IdTimetablePreferenceHeader);
            //if (!string.IsNullOrEmpty(param.IdParent))
            //    predicate = predicate.Or(x => x.Id == param.IdParent);
            
            if (param.IdChild != null && param.IdChild.Any())
                predicate = predicate.Or(x => param.IdChild.Any(p => p == x.Id));

            var query = _dbContext.Entity<MsSubjectCombination>()
                    .Where(predicate)
                    .SelectMany(x => x.GradePathwayClassroom.ClassroomDivisions)
                    .Distinct();

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Division.Description ?? x.Division.Code))
                    .Distinct()
                    .ToListAsync(CancellationToken);
            else
            {
                items = await query
                    .OrderBy(p=> p.Division.Description)
                    .SetPagination(param)
                    .Select(x => new CodeWithIdVm
                    {
                        Id = x.Division.Id,
                        Code = x.Division.Code,
                        Description = x.Division.Description
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);
            }
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty());
        }
    }
}
