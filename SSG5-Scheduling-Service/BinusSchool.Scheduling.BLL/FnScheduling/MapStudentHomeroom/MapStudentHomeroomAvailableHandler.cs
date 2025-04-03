using System;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.MapStudentHomeroom;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.MapStudentHomeroom
{
    public class MapStudentHomeroomAvailableHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMapStudentPathway _mapStudentPathwayService;

        public MapStudentHomeroomAvailableHandler(ISchedulingDbContext dbContext, IMapStudentPathway mapStudentPathwayService)
        {
            _dbContext = dbContext;
            _mapStudentPathwayService = mapStudentPathwayService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetMapStudentHomeroomAvailableRequest>(
                nameof(GetMapStudentHomeroomAvailableRequest.IdGrade),
                nameof(GetMapStudentHomeroomAvailableRequest.IdHomeroom));

            var homeroom = await _dbContext.Entity<MsHomeroom>()
                                           .Where(x => x.Id == param.IdHomeroom)
                                           .FirstOrDefaultAsync(CancellationToken);
            if (homeroom == null)
                throw new NotFoundException("Homeroom is not found");

            var stdAlreadyHaveHomerooms = await _dbContext.Entity<MsHomeroomStudent>()
                .Where(x => x.Homeroom.IdGrade == param.IdGrade
                            && x.Semester == homeroom.Semester)
                .Select(x => x.IdStudent)
                .ToListAsync(CancellationToken);

            if (param.ExceptIds?.Any() ?? false)
                stdAlreadyHaveHomerooms.AddRange(param.ExceptIds);

            var students = await _mapStudentPathwayService.GetMapStudentPathways(new GetMapStudentPathwayRequest
            {
                GetAll = param.GetAll,
                Ids = param.Ids,
                OrderBy = param.OrderBy,
                OrderType = param.OrderType,
                Page = param.Page,
                Return = param.Return,
                Search = param.Search,
                SearchBy = param.SearchBy,
                Size = param.Size,
                IdGrade = param.IdGrade,
                IncludeLastHomeroom = true,
                ExceptIds = stdAlreadyHaveHomerooms.Distinct()
            });

            return Request.CreateApiResult2(students.Payload as object, students.Properties);
        }
    }
}
