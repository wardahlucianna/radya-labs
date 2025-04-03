using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.ClassRoom;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.School.FnSchool.ClassRoomMapping
{
    public class GetListClassroomMapHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public GetListClassroomMapHandler(ISchoolDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }


        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetClassroomMapRequest>(nameof(GetClassroomMapRequest.IdGrade));
            var columns = new[] { "code", "description" };
            var aliasColumns = new Dictionary<string, string>
                {
                    { columns[0], "classroom.code" },
                    { columns[1], "classroom.description" }
                };

            var predicate = PredicateBuilder.Create<MsGradePathwayClassroom>(x => x.GradePathway.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdPathway))
                predicate = predicate.And(x => x.GradePathway.GradePathwayDetails.Any(y => y.Id == param.IdPathway));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Classroom.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Classroom.Description, param.SearchPattern()));

            var query = _dbContext.Entity<MsGradePathwayClassroom>()
                .Include(x => x.GradePathway).ThenInclude(x => x.GradePathwayDetails)
                .Include(x => x.Classroom)
                .SearchByIds(param)
                .Where(predicate)
                .OrderByDynamic(param, aliasColumns);

            IReadOnlyList<IItemValueVm> items = default;
            if (param.Return == CollectionType.Lov)
                items = await query
                    .Select(x => new ItemValueVm(x.Id, x.Classroom.Description ?? x.Classroom.Code))
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetClassroomMapResult
                    {
                        Id = x.Id,
                        Code = x.Classroom.Code,
                        Description = x.Classroom.Description,
                        Pathways = x.GradePathway.GradePathwayDetails.Select(y => new CodeWithIdVm
                        {
                            Id = y.Id,
                            Code = y.Pathway.Code,
                            Description = y.Pathway.Description
                        })
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.Id).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items as object, param.CreatePaginationProperty(count).AddColumnProperty(columns));
        }
    }
}
