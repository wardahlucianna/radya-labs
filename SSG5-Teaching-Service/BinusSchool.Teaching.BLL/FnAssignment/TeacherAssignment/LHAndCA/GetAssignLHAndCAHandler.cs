using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment.LHAndCA;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Teaching.FnAssignment.TeacherAssignment.LHAndCA
{
    public class GetAssignLHAndCAHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetAssignLHAndCARequest.IdSchool)
        });
        private static readonly Lazy<string[]> _columns = new Lazy<string[]>(new[]
        {
            "acadyear", "level", "description", "code"
        });
        private static readonly Lazy<IDictionary<string, string>> _aliasColumns = new Lazy<IDictionary<string, string>>(new Dictionary<string, string>
        {
            { _columns.Value[0], "level.academicYear.code" },
            { _columns.Value[1], "level.code" }
        });

        private readonly ITeachingDbContext _dbContext;

        public GetAssignLHAndCAHandler(ITeachingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetAssignLHAndCARequest>(_requiredParams.Value);
            var predicate = PredicateBuilder.Create<MsGrade>(x => param.IdSchool.Contains(x.Level.AcademicYear.IdSchool));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Level.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Level.AcademicYear.Description, param.SearchPattern()));

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Id == param.IdGrade);
            else if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.IdLevel == param.IdLevel);
            else if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.Level.IdAcademicYear == param.IdAcadyear);

            var query = _dbContext.Entity<MsGrade>()
                .SearchByIds(param)
                .Where(predicate);

            query = param.OrderBy switch
            {
                "code" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Code.Length).ThenBy(x => x.Code)
                    : query.OrderByDescending(x => x.Code.Length).ThenByDescending(x => x.Code),
                "description" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                    : query.OrderByDescending(x => x.Description.Length).ThenByDescending(x => x.Description),
                _ => query.OrderByDynamic(param, _aliasColumns.Value)
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm(x.Id, x.Code, x.Description))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetAssignLHAndCAResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        Acadyear = new CodeWithIdVm
                        {
                            Id = x.Level.IdAcademicYear,
                            Code = x.Level.AcademicYear.Code,
                            Description = x.Level.AcademicYear.Description
                        },
                        Level = x.Level.Description
                    })
                    .ToListAsync(CancellationToken);

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(_columns.Value));
        }
    }
}