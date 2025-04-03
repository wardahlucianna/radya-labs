using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnSchedule.Homeroom.Validator;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class GetHomeroomByLevelGradeHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public GetHomeroomByLevelGradeHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<GetHomeroomByLevelGradeRequest, GetHomeroomByLevelGradeValidator>();

            var query = await _dbContext.Entity<MsHomeroom>()
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .Where(x => x.Semester == param.Semester)
                .Where(x => x.Grade.IdLevel == (param.IdLevel != null ? param.IdLevel : x.Grade.IdLevel))
                .Where(x => x.IdGrade == (param.IdGrade != null ? param.IdGrade : x.IdGrade))
                .OrderBy(x => x.Grade.Level.OrderNumber)
                    .ThenBy(x => x.Grade.OrderNumber)
                .Select(x => new ItemValueVm
                {
                    Id = x.Id,
                    Description = x.Grade.Code + "" + x.GradePathwayClassroom.Classroom.Code
                })
                .OrderBy(x => x.Description)
                .ToListAsync(CancellationToken);

            if (query is null)
                throw new BadRequestException("Homeroom not found");

            return Request.CreateApiResult2(query as object);
        }
    }
}
