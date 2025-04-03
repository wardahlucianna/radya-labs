using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.NonTeachingLoad;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.TeachingDb.Entities;
using System.Linq;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Exceptions;

namespace BinusSchool.Teaching.FnAssignment.NonTeachingLoad
{
    public class GetCopyNonTeachingLoadHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetNonTeachLoadRequest.IdSchool)
        });
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[]
        {
            "acadyear", "category", "position", "load"
        });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "IdAcademicYear" },
            { _columns.Value[1], "Category" },
            { _columns.Value[2], "TeacherPosition.Description" },
            { _columns.Value[3], "Load" },
        });
        private readonly ITeachingDbContext _dbContext;

        public GetCopyNonTeachingLoadHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCopyNonTeachingLoadRequest>(_requiredParams.Value);

            if (param.IdAcadYearSource.ToLower() == param.IdAcadYearTarget.ToLower())
                throw new BadRequestException("Academic year source and target cannot be equal");

            var predicate = PredicateBuilder.Create<MsNonTeachingLoad>(x => param.IdSchool.Contains(x.AcademicYear.IdSchool));
            predicate = predicate.And(x => x.IdAcademicYear == param.IdAcadYearSource
            && !_dbContext.Entity<MsNonTeachingLoad>().Any(y => y.IdAcademicYear == param.IdAcadYearTarget
                && y.IdTeacherPosition == x.IdTeacherPosition
                && y.Category == x.Category));

            if (param.Category.HasValue)
                predicate = predicate.And(x => x.Category == param.Category.Value);

            if (!string.IsNullOrWhiteSpace(param.Search))
            {
                if (param.Search.Equals(nameof(AcademicType.Academic), StringComparison.InvariantCultureIgnoreCase))
                    predicate = predicate.And(x => x.Category == AcademicType.Academic);
                else if (param.Search.Equals(nameof(AcademicType.NonAcademic), StringComparison.InvariantCultureIgnoreCase))
                    predicate = predicate.And(x => x.Category == AcademicType.NonAcademic);
                else
                    predicate = predicate.And(x
                        => EF.Functions.Like(x.AcademicYear.Description, param.SearchPattern())
                        || EF.Functions.Like(x.TeacherPosition.Description, param.SearchPattern())
                        || EF.Functions.Like(Convert.ToString(x.Load), param.SearchPattern()));
            }

            var query = _dbContext.Entity<MsNonTeachingLoad>()
                //.SearchByIds(param) // bisa diganti dengan ini >> .Where(e => param.Ids.Contains(e.Id))
                .Where(predicate)
                .OrderByDynamic(param, _aliasColumns.Value);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = await query
                   .Select(x => new ItemValueVm(x.Id, x.TeacherPosition.Description))
                   .ToListAsync(CancellationToken);
            }
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetCopyNonTeachingLoadResult
                    {
                        Id = x.Id,
                        Description = x.TeacherPosition.Description,
                        Acadyear = x.AcademicYear.Description,
                        Category = x.Category,
                        Position = x.TeacherPosition.Description,
                        Load = x.Load
                    })
                    .ToListAsync(CancellationToken);
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}
