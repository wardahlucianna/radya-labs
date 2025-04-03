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
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.Grade.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradeCodeHandler : FunctionsHttpCrudHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradeCodeHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetGradeCodeRequest>(nameof(GetGradeCodeRequest.IdSchool));
            var columns = new[] { "acadyear", "level", "description", "code" };
            var aliasColumns = new Dictionary<string, string>
            {
                { columns[0], "level.academicYear.code" },
                { columns[1], "level.code" }
            };

            var predicate = PredicateBuilder.Create<MsGrade>(x => param.IdSchool.Any(y => y == x.Level.AcademicYear.School.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Description, param.SearchPattern())
                    || EF.Functions.Like(x.Level.Description, $"%{param.Search}%"));

            if (!string.IsNullOrEmpty(param.CodeLevel))
                predicate = predicate.And(x => x.Level.Code == param.CodeLevel);

            var query = _dbContext.Entity<MsGrade>()
                .SearchByIds(param)
                .Where(predicate)
                .Where(x => (string.IsNullOrEmpty(param.CodeAcademicYear) ? true : x.Level.AcademicYear.Code.CompareTo(param.CodeAcademicYear) == 0)
                            );

            query = param.OrderBy switch
            {
                "code" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Code.Length).ThenBy(x => x.Code)
                    : query.OrderByDescending(x => x.Code.Length).ThenByDescending(x => x.Code),
                "description" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.Description.Length).ThenBy(x => x.Description)
                    : query.OrderByDescending(x => x.Description.Length).ThenByDescending(x => x.Description),
                _ => query.OrderByDynamic(param, aliasColumns)
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm
                    {
                        Code = x.Code,
                        Description = x.Description
                    })
                    .Distinct()
                    .ToListAsync();
            else
                items = await query
                    .GroupBy(x => new { GradeCode = x.Code, GradeDescription = x.Description, LevelCode = x.Level.Code, LevelDescription = x.Level.Description, SchoolId = x.Level.AcademicYear.IdSchool, SchoolCode = x.Level.AcademicYear.School.Name, SchoolDescription = x.Level.AcademicYear.School.Description })
                    .SetPagination(param)
                    .Select(x => new GetGradeCodeResult
                    {
                        Code = x.Key.GradeCode,
                        Description = x.Key.GradeDescription,
                        Level = new CodeWithIdVm
                        {
                            Code = x.Key.LevelCode,
                            Description = x.Key.LevelDescription
                        },
                        School = new CodeWithIdVm
                        {
                            Id = x.Key.SchoolId,
                            Code = x.Key.SchoolCode,
                            Description = x.Key.LevelDescription
                        }
                    })
                    .ToListAsync();

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> PutHandler()
        {
            throw new NotImplementedException();
        }
    }
}
