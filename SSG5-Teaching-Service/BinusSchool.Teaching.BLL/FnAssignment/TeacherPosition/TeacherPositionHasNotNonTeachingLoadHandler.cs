using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeacherPosition
{
    public class TeacherPositionHasNotNonTeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;

        public TeacherPositionHasNotNonTeachingLoadHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetTeacherPositionHasNotNonTeachingLoadRequest>(
                nameof(CollectionSchoolRequest.IdSchool), 
                nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.IdAcadyear),
                nameof(GetTeacherPositionHasNotNonTeachingLoadRequest.Category));

            var predicate = PredicateBuilder.Create<MsTeacherPosition>(x => x.Category == param.Category);

            predicate = predicate.And(x => param.IdSchool.Any(y => y == x.IdSchool));

            if (!string.IsNullOrWhiteSpace(param.PositionCode))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern()));

            var query = _dbContext.Entity<MsTeacherPosition>()
                .Include(x => x.NonTeachingLoads)
                .Where(x => !x.NonTeachingLoads.Any(y => y.IdAcademicYear == param.IdAcadyear && y.IdTeacherPosition == x.Id))
                .Where(predicate)
                .AsQueryable();

            var items = await query
                .SetPagination(param)
                .Select(x => new GetTeacherPositionHasNotNonTeachingLoadResult
                {
                    Id = x.Id,
                    Code = x.Code,
                    Description = x.Description
                })
                .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
           ? items.Count
           : await query.Select(x => x.Id).CountAsync();
            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(null));
        }
    }
}
