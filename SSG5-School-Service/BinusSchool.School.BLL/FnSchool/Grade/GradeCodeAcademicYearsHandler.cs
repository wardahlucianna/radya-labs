using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradeCodeAcademicYearsHandler  : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradeCodeAcademicYearsHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GradeCodeAcademicYearsRequest>(
                            nameof(GradeCodeAcademicYearsRequest.IdSchool));

            var columns = new[] { "code" };
            var aliasColumns = new Dictionary<string, string>
                {
                    { columns[0], "code" }
                };

            var predicate = PredicateBuilder.Create<MsLevel>(x
                => EF.Functions.Like(x.Code, param.SearchPattern())
                );

            var getGradeByLevelCode = await _dbContext.Entity<MsGrade>()
                                        .Include(x => x.Level)
                                            .ThenInclude(x => x.AcademicYear)
                                        .Where(x => x.Level.AcademicYear.IdSchool == param.IdSchool &&
                                                    (string.IsNullOrEmpty(param.CodeAcademicYearStart) ? true : x.Level.AcademicYear.Code.CompareTo(param.CodeAcademicYearStart) >= 0) &&
                                                    (string.IsNullOrEmpty(param.CodeAcademicYearEnd) ? true : x.Level.AcademicYear.Code.CompareTo(param.CodeAcademicYearEnd) <= 0)
                                                )
                                        .OrderBy(x => x.Code.Length)
                                        .ThenByDescending(x => x.DateIn)
                                        .GroupBy(x => new
                                        {
                                            LevelCode = x.Level.Code,
                                            GradeCode = x.Code,
                                            GradeDescription = x.Description
                                        })
                                        .Select(x => new
                                        {
                                            LevelCode = x.Key.LevelCode,
                                            Grade = new CodeVm
                                            {
                                                Code = x.Key.GradeCode,
                                                Description = x.Key.GradeDescription
                                            }
                                        })
                                        .Distinct()
                                        .ToListAsync(CancellationToken);

            var query = _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .Where(predicate)
                .Where(x => x.AcademicYear.IdSchool == param.IdSchool &&
                            (string.IsNullOrEmpty(param.CodeAcademicYearStart) ? true : x.AcademicYear.Code.CompareTo(param.CodeAcademicYearStart) >= 0) &&
                            (string.IsNullOrEmpty(param.CodeAcademicYearEnd) ? true : x.AcademicYear.Code.CompareTo(param.CodeAcademicYearEnd) <= 0)
                            )
                .Select(x => new
                {
                    LevelCode = x.Code,
                    LevelDescription = x.Description
                })
                .Distinct();

            query = param.OrderBy switch
            {
                "code" => param.OrderType == OrderType.Asc
                    ? query.OrderBy(x => x.LevelCode)
                    : query.OrderByDescending(x => x.LevelCode),
                _ => query
            };

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new CodeWithIdVm
                    {
                        Code = x.LevelCode,
                        Description = x.LevelDescription
                    })
                    .Distinct()
                    .ToListAsync();
            else
            {
                items = await query
                    .SetPagination(param)
                    .Select(x => new GradeCodeAcademicYearsResult
                    {
                        Code = x.LevelCode,
                        Description = x.LevelDescription,
                        //Grades = getGradeByLevelCode
                        //            .Where(y => y.LevelCode == x.LevelCode)
                        //            .Select(y => new CodeWithIdVm
                        //            {
                        //                Code = y.Grade.Code,
                        //                Description = y.Grade.Description
                        //            })
                        //            .ToList()
                    })
                    .ToListAsync();

                foreach (GradeCodeAcademicYearsResult item in items)
                {
                    item.Grades = getGradeByLevelCode
                                    .Where(y => y.LevelCode == item.Code)
                                    .GroupBy(x => x.Grade.Code, (key, g) => g.First())
                                    .Select(y => new CodeWithIdVm
                                    {
                                        Code = y.Grade.Code,
                                        Description = y.Grade.Description
                                    })
                                    .OrderBy(x => x.Code.Length)
                                    .ThenBy(x => x.Code)
                                    .ToList();
                }
            }

            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.LevelCode).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
