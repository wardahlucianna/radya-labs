using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ClassRoomMapping
{
    public class GetClassroomMapByLevelHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetClassroomMapByLevelHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMappingClassByLevelRequest>(nameof(GetMappingClassByLevelRequest.IdSchool));

            var predicate = PredicateBuilder.Create<MsGradePathwayClassroom>(x => param.IdSchool.Any(y => y == x.GradePathway.Grade.Level.AcademicYear.IdSchool));

            if (!string.IsNullOrEmpty(param.IdAcadyear))
                predicate = predicate.And(x => x.GradePathway.Grade.Level.IdAcademicYear == param.IdAcadyear);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.GradePathway.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Classroom.Code, $"%{param.Search}%")
                    || EF.Functions.Like(x.Classroom.Description, $"%{param.Search}%"));

            var query = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.GradePathway)
                .Where(predicate)
                .Select(x => new GetClassroomMapByGradeResult
                {
                    Id = x.Id,
                    Code = x.Classroom.Code,
                    Description = x.Classroom.Description,
                    Formatted = $"{x.GradePathway.Grade.Code}{x.Classroom.Code}",
                    Grade = new CodeWithIdVm
                    {
                        Id = x.GradePathway.IdGrade,
                        Code = x.GradePathway.Grade.Code,
                        Description = x.GradePathway.Grade.Description
                    },
                    Class = new CodeWithIdVm
                    {
                        Id = x.Classroom.Id,
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description
                    }
                })
                .OrderBy(x => x.Grade.Code).ThenBy(x => x.Code)
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
