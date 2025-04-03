using System.Collections.Generic;
using System.Linq;
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
    public class GradeAcademicYearsHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradeAcademicYearsHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeAcadyearRequest>(nameof(GetGradeAcadyearRequest.IdAcadyear));
            var columns = new[] { "code" };
            var aliasColumns = new Dictionary<string, string>
                {
                    { columns[0], "code" }
                };

            var predicate = PredicateBuilder.Create<MsLevel>(x
                => x.AcademicYear.Id == param.IdAcadyear
                 && EF.Functions.Like(x.Code, param.SearchPattern())
                );
            //exclude have period
            //if (param.ExcludeHavePeriod.HasValue && param.ExcludeHavePeriod.Value)
            //    predicate = predicate.And(x => !x.SchoolAcadyearLevelGrades.Any(x => x.SchoolAcadyearTerms.Any()));
            //// exclude have subject
            //if (param.ExcludeHaveSubject.HasValue && param.ExcludeHaveSubject.Value)
            //    predicate = predicate.And(x => !x.SchoolAcadyearLevelGrades.Any(x => x.SchoolAcadyearLevelGradeSubjects.Any()));
            //// exclude have pathway
            //if (param.ExcludeHavePathway.HasValue && param.ExcludeHavePathway.Value)
            //    predicate = predicate.And(x => !x.SchoolAcadyearLevelGrades.Any(x => x.SchoolAcadYearLevelGradePathways.Any()));

            var query = _dbContext.Entity<MsLevel>()
                .Include(x => x.AcademicYear)
                .Include(x => x.Grades)
                // include when ExcludeHavePeriod true
                //.If(param.ExcludeHavePeriod.HasValue && param.ExcludeHavePeriod.Value, x =>
                //    x.Include(x => x.SchoolAcadyearLevelGrades).ThenInclude(x => x.SchoolAcadyearTerms))
                //// include when ExcludeHaveSubject true
                //.If(param.ExcludeHaveSubject.HasValue && param.ExcludeHaveSubject.HasValue, x =>
                //    x.Include(x => x.SchoolAcadyearLevelGrades).ThenInclude(x => x.SchoolAcadyearLevelGradeSubjects))
                //// include when ExcludeHavePathway true
                //.If(param.ExcludeHavePathway.HasValue && param.ExcludeHavePathway.HasValue, x =>
                //    x.Include(x => x.SchoolAcadyearLevelGrades).ThenInclude(x => x.SchoolAcadYearLevelGradePathways))
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Description ?? x.Code))
                    .ToListAsync();
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetGradeAcadyearResult
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Description = x.Description,
                        OrderNumber = x.OrderNumber,
                        Grades = x.Grades.OrderBy(x => x.OrderNumber).Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Code,
                            Description = y.Description
                        })
                    })
                    .OrderBy(x => x.OrderNumber)
                    .ToListAsync();
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
