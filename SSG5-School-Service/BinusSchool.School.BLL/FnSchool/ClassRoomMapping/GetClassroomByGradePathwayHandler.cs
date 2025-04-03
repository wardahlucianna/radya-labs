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
    public class GetClassroomByGradePathwayHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;

        public GetClassroomByGradePathwayHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassroomByGradePathwayRequest>(nameof(GetListGradePathwayClassRoomRequest.Ids));

            var predicate = PredicateBuilder.Create<MsGradePathwayClassroom>(x => param.Ids.Contains(x.IdGradePathway));

            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Classroom.Code, $"%{param.Search}%")
                    || EF.Functions.Like(x.Classroom.Description, $"%{param.Search}%"));

            var query = await _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.Classroom)
                .Where(x => param.Ids.Contains(x.IdGradePathway))
                .Where(predicate)
                .Select(x => new GetClassroomMapByGradeResult
                {
                    Id = x.Id,
                    Code = x.Classroom.Code,
                    Description = x.Classroom.Description,
                })
                .ToListAsync(CancellationToken);

            return Request.CreateApiResult2(query as object);
        }
    }
}
