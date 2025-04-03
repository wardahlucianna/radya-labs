using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.MeritDemerit;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.MeritDemerit
{
    public class GetMeritDemeritDisciplineMappingLevelHandler : FunctionsHttpSingleHandler
    {
        private static readonly string[] _columns = { "AcademicYear", "Level", "Approval1", "Approval2", "Approval3" };
        private readonly ISchoolDbContext _dbContext;

        public GetMeritDemeritDisciplineMappingLevelHandler(ISchoolDbContext MeritDemetitDbContext)
        {
            _dbContext = MeritDemetitDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMeritDemeritLevelInfractionRequest>();
            var predicate = PredicateBuilder.Create<MsLevelOfInteraction>(x => x.IdSchool == param.IdSchool);
            IReadOnlyList<IItemValueVm> items = default;
            var Parent = await _dbContext.Entity<MsLevelOfInteraction>()
                .Where(x => x.IdSchool == param.IdSchool)
                .Where(x => x.IdParentLevelOfInteraction == null)
                .Where(x => !_dbContext.Entity<MsLevelOfInteraction>().Any(y => y.IdParentLevelOfInteraction == x.Id))
                .Select(x => new
                {
                    Id = x.Id,
                    SortingParent = Convert.ToInt32(x.NameLevelOfInteraction),
                    SortingChild = "",
                    Name = x.NameLevelOfInteraction,
                }).ToListAsync(CancellationToken);
            var Child = await (from child in _dbContext.Entity<MsLevelOfInteraction>()
                               join parent in _dbContext.Entity<MsLevelOfInteraction>() on child.IdParentLevelOfInteraction equals parent.Id
                               where
                               child.IdParentLevelOfInteraction != null
                               && child.IdSchool == param.IdSchool
                               select new
                               {
                                   Id = child.Id,
                                   SortingParent = Convert.ToInt32(parent.NameLevelOfInteraction),
                                   SortingChild = child.NameLevelOfInteraction,
                                   Name = parent.NameLevelOfInteraction + " " + child.NameLevelOfInteraction,
                               }).ToListAsync(CancellationToken);
            var res = Parent.Union(Child).OrderBy(x => x.SortingParent).ThenBy(x => x.SortingChild).ToList();
            items = res.Select(e => new ItemValueVm
            {
                Id = e.Id,
                Description = e.Name,
            }).ToList();

            return Request.CreateApiResult2(items as object);
        }


    }
}
