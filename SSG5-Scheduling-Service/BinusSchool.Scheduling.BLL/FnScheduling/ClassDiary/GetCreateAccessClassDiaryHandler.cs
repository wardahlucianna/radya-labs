using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ClassDiary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Utilities;
using BinusSchool.Common.Constants;
using BinusSchool.Persistence.SchedulingDb.Entities.Teaching;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;

namespace BinusSchool.Scheduling.FnSchedule.ClassDiary
{
    public class GetCreateAccessClassDiaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;
        public GetCreateAccessClassDiaryHandler(ISchedulingDbContext dbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = dbContext;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCreateAccessClassDiaryRequest>();


            List<string> listCodePosition = new List<string>()
                                            {
                                                PositionConstant.SubjectTeacher,
                                                PositionConstant.ClassAdvisor,
                                            };

            var listIdTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                                            .Where(e => listCodePosition.Contains(e.Position.Code))
                                            .Select(e => e.Id)
                                            .ToListAsync(CancellationToken);

            var paramPositionByUser = new GetSubjectByUserRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdUser = param.IdUser,
                ListIdTeacherPositions = listIdTeacherPosition,
                IsClassDiary = true
            };

            var getApiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(paramPositionByUser);
            var getSubjectByUser = getApiSubjectByUser.IsSuccess ? getApiSubjectByUser.Payload : null;
            var getIdLessonByUser = new List<string>();
            if (getSubjectByUser != null)
            {
                getIdLessonByUser = getSubjectByUser.Select(e => e.Lesson.Id).Distinct().ToList(); 
            }

            var items = new GetCreateAccessClassDiaryResult
            {
                CanAccess = getIdLessonByUser.Any(),
            };

            return Request.CreateApiResult2(items as object);
        }
    }
}
