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
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.LessonPlan;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.Scheduling;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan
{
    public class LessonPlanGradeByPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        private readonly IRolePosition _rolePositionService;

        public LessonPlanGradeByPositionHandler(ITeachingDbContext dbContext, IRolePosition RolePositionService)
        {
            _dbContext = dbContext;
            _rolePositionService = RolePositionService;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<LessonPlanGradeByPositionRequest>();
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

            var idGrades = GetSubjectByUser
                            .Where(e => e.Level.Id == param.IdLevel)
                            .GroupBy(e => new
                            {
                                IdGrade = e.Grade.Id,
                                Grade = e.Grade.Description,
                                GradeOrderNumber = e.Grade.OrderNumber,
                            })
                            .OrderBy(e => e.Key.GradeOrderNumber)
                            .Select(e => new ItemValueVm
                            {
                                Id = e.Key.IdGrade,
                                Description = e.Key.Grade
                            }).ToList();

            return Request.CreateApiResult2(idGrades as object);
        }
    }
}
