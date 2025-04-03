using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Persistence.Entities;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.Metadata
{
    public class MetadataHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public MetadataHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var result = new GetMetadataResult();
            var param = Request.GetParams<GetMetadataRequest>();

            result.Acadyears = param.Acadyears?.Any() ?? false
                ? await GetItemValues<MsAcademicYear>(param.Acadyears)
                : Enumerable.Empty<ItemValueVm>();

            result.Levels = param.Levels?.Any() ?? false
                ? await GetItemValues<MsLevel>(param.Levels)
                : Enumerable.Empty<ItemValueVm>();

            result.Grades = param.Grades?.Any() ?? false
                ? await GetItemValues<MsGrade>(param.Grades)
                : Enumerable.Empty<ItemValueVm>();

            result.Terms = param.Terms?.Any() ?? false
                ? await GetItemValues<MsPeriod>(param.Terms)
                : Enumerable.Empty<ItemValueVm>();

            result.Subjects = param.Subjects?.Any() ?? false
                ? await GetItemValues<MsSubject>(param.Subjects)
                : Enumerable.Empty<ItemValueVm>();

            result.Classrooms = param.Classrooms?.Any() ?? false
                ? await GetItemValues<MsClassroom>(param.Classrooms)
                : Enumerable.Empty<ItemValueVm>();

            result.Departments = param.Departments?.Any() ?? false
                ? await GetItemValues<MsDepartment>(param.Departments)
                : Enumerable.Empty<ItemValueVm>();

            result.Streamings = param.Streamings?.Any() ?? false
                ? await GetItemValues<MsPathway>(param.Streamings)
                : Enumerable.Empty<ItemValueVm>();

            result.Buldings = param.Buildings?.Any() ?? false
                ? await GetItemValues<MsBuilding>(param.Buildings)
                : Enumerable.Empty<ItemValueVm>();

            result.Venues = param.Venues?.Any() ?? false
               ? await GetItemValues<MsVenue>(param.Venues)
               : Enumerable.Empty<ItemValueVm>();

            result.Divisions = param.Divisions?.Any() ?? false
                ? await GetItemValues<MsDivision>(param.Divisions)
                : Enumerable.Empty<ItemValueVm>();

            return Request.CreateApiResult2(result as object);
        }

        private Task<List<ItemValueVm>> GetItemValues<T>(IEnumerable<string> ids, bool checkDescriptionFirst = true)
            where T : CodeEntity, ISchoolEntity
        {
            return _dbContext.Entity<T>()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new ItemValueVm(x.Id, checkDescriptionFirst
                    ? x.Description ?? x.Code
                    : x.Code ?? x.Description))
                .AsNoTracking()
                .ToListAsync(CancellationToken);
        }
    }
}
