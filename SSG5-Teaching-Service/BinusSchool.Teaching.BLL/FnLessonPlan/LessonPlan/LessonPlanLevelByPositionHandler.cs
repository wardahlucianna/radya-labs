using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Attendance.FnAttendance.LevelByPosition;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanLevelByPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IRolePosition _rolePositionService;

        public LessonPlanLevelByPositionHandler(ITeachingDbContext dbContext, IRolePosition RolePositionService)
        {
            _dbContext = dbContext;
            _rolePositionService = RolePositionService;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<LevelByPositionRequest>();

            var ListIdTeacherPositions = await _dbContext.Entity<MsTeacherPosition>()
                                            .Where(x => x.Position.Code == param.SelectedPosition)
                                            .Select(e => e.Id)
                                            .ToListAsync(CancellationToken);

            GetSubjectByUserRequest paramSubjectByGrade = new GetSubjectByUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                ListIdTeacherPositions = ListIdTeacherPositions
            };

            var apiGetSubjectByUser = await _rolePositionService.GetSubjectByUser(paramSubjectByGrade);
            var GetSubjectByUser = apiGetSubjectByUser.IsSuccess ? apiGetSubjectByUser.Payload : null;

            if (GetSubjectByUser == null)
                throw new BadRequestException("data level is not found");

            var idLevels = GetSubjectByUser
                            .GroupBy(e => new
                            {
                                IdLevel = e.Level.Id,
                                Level = e.Level.Description,
                                LevelOrderNumber = e.Level.OrderNumber,
                            })
                            .OrderBy(e => e.Key.LevelOrderNumber)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Key.IdLevel,
                                Description = e.Key.Level,
                            }).ToList();

            return Request.CreateApiResult2(idLevels as object);
        }
    }
}
