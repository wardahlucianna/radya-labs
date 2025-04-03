using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.Scheduling.FnSchedule.Homeroom
{
    public class HomeroomByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IApiService<IClassroomMap> _classroomMapService;

        public HomeroomByStudentHandler(ISchedulingDbContext dbContext, IApiService<IClassroomMap> classroomMapService)
        {
            _dbContext = dbContext;
            _classroomMapService = classroomMapService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetHomeroomByStudentRequest>(
                nameof(GetHomeroomByStudentRequest.Ids), nameof(GetHomeroomByStudentRequest.IdGrades));

            var hrStudents = await _dbContext.Entity<MsHomeroomStudent>()
                .Include(x => x.Homeroom)
                .Where(x => param.Ids.Contains(x.IdStudent) && param.IdGrades.Contains(x.Homeroom.IdGrade))
                .ToListAsync(CancellationToken);

            FillConfiguration();
            _classroomMapService.SetConfigurationFrom(ApiConfiguration);
            var crMapResult = await _classroomMapService.Execute.GetClassroomMappedsByGrade(new GetListGradePathwayClassRoomRequest
            {
                Ids = param.IdGrades
            });

            var homerooms = hrStudents
                .GroupBy(x => x.Homeroom)
                .Select(x => new GetHomeroomByStudentResult
                {
                    Id = x.Key.Id,
                    Grade = new CodeWithIdVm
                    {
                        Id = x.Key.IdGrade,
                        Code = crMapResult.Payload?.FirstOrDefault()?.Grade?.Code,
                        Description = crMapResult.Payload?.FirstOrDefault()?.Grade?.Description
                    },
                    Classroom = new CodeWithIdVm
                    {
                        Id = x.Key.IdGradePathwayClassRoom,
                        Code = crMapResult.Payload?.FirstOrDefault()?.Code,
                        Description = crMapResult.Payload?.FirstOrDefault()?.Description
                    },
                    IdStudents = x.Select(y => y.IdStudent)
                });

            return Request.CreateApiResult2(homerooms as object);
        }
    }
}