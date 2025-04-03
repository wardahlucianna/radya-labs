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
using Microsoft.Extensions.Localization;

namespace BinusSchool.School.FnSchool.Grade
{
    public class GradeWithPathwayHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GradeWithPathwayHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetGradeWithPathwayRequest>(new string[] { nameof(GetGradeWithPathwayRequest.IdAcadyear), nameof(GetGradeWithPathwayRequest.IdSessionSet) });
            var columns = new[] { "code" };

            var predicate = PredicateBuilder.Create<MsGradePathway>(x
                => x.Grade.Level.AcademicYear.Id == param.IdAcadyear
                && EF.Functions.Like(x.Grade.Code, param.SearchPattern())
                && x.Sessions.Any(x => x.SessionSet.Id == param.IdSessionSet)); 

            var query = _dbContext.Entity<MsGradePathway>()
                .Include(x => x.Grade.Level.AcademicYear)
                .Include(x => x.Sessions).ThenInclude(p=> p.SessionSet)
                .Include(x => x.GradePathwayDetails).ThenInclude(x => x.Pathway)
                .Where(predicate);

            var results = await query.ToListAsync();
            var grouped = results.GroupBy(x => x.Grade);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
            {
                items = grouped
                    .Select(x => new ItemValueVm
                    {
                        Id = x.Key.Id,
                        Description = string.Join(", ", x.SelectMany(y => y.GradePathwayDetails).Select(y => y.Pathway.Description ?? y.Pathway.Code))
                    })
                    .ToList();
            }
            else
            {
                items = grouped
                    .OrderBy(x => x.Key.Code)
                    .Select(x => new GetGradeWithPathwayResult
                    {
                        Id = x.Key.Id,
                        IdGrade = x.Key.Id,
                        Code = x.Key.Code,
                        Description = x.Key.Description,
                        Pathways = x.SelectMany(y => y.GradePathwayDetails).Select(y => new ItemValueVm
                        {
                            Id = y.Id,
                            Description = y.Pathway.Description ?? y.Pathway.Code
                        })
                    })
                    .Skip(param.CalculateOffset())
                    .Take(param.Size)
                    .ToList();
            }
            var count = grouped.Count();

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
