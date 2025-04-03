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
    public class GetClassroomMapByGrade : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetClassroomMapByGrade(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListGradePathwayClassRoomRequest>(nameof(GetListGradePathwayClassRoomRequest.Ids));

            var predicate = PredicateBuilder.Create<MsGradePathwayClassroom>(x => param.Ids.Contains(x.GradePathway.IdGrade));
         
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Classroom.Code, $"%{param.Search}%")
                    || EF.Functions.Like(x.Classroom.Description, $"%{param.Search}%"));

            var query = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Include(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails)
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
                    Pathway = new ClassroomMapPathway
                    {
                        Id = x.GradePathway.Id,
                        PathwayDetails = x.GradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Pathway.Code,
                            Description = y.Pathway.Description
                        })
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
