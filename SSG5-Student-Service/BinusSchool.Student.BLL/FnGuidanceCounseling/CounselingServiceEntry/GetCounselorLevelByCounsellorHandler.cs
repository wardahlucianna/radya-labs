using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.RolePosition;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.CounselingServiceEntry;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Schedulling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.Teaching;
using BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnGuidanceCounseling.CounselingServiceEntry
{
    public class GetCounselorLevelByCounsellorHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IRolePosition _serviceRolePosition;

        public GetCounselorLevelByCounsellorHandler(IStudentDbContext dbContext, IRolePosition serviceRolePosition)
        {
            _dbContext = dbContext;
            _serviceRolePosition = serviceRolePosition;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetCounselorLevelByCounsellorRequest>();
            List<CodeWithIdVm> items = new List<CodeWithIdVm>();

            #region position allow akses
            List<string> listPosition = new List<string>()
            {
                PositionConstant.VicePrincipal,
                PositionConstant.Principal,
                PositionConstant.AffectiveCoordinator,
                PositionConstant.NonTeachingStaff, //HOGC
            };
            #endregion

            #region Position
            var listIdTeacherPosition = await _dbContext.Entity<MsTeacherPosition>()
                        .Where(e => listPosition.Contains(e.Position.Code))
                        .Select(e => e.Id)
                        .ToListAsync(CancellationToken);

            if (listIdTeacherPosition.Any())
            {
                var Request = new GetSubjectByUserRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    ListIdTeacherPositions = listIdTeacherPosition,
                    IdUser = param.IdUser,
                };

                var apiSubjectByUser = await _serviceRolePosition.GetSubjectByUser(Request);
                var getSubjectByUser = apiSubjectByUser.IsSuccess ? apiSubjectByUser.Payload : null;
                if (getSubjectByUser != null)
                {
                    var listHomeroomStudentByPosition = getSubjectByUser
                            .Select(e => new CodeWithIdVm
                            {
                                Id = e.Level.Id,
                                Code = e.Level.Code,
                                Description = e.Level.Description
                            })
                            .Distinct().ToList();
                    items.AddRange(listHomeroomStudentByPosition);
                }
            }
            #endregion

            #region Counsellor
            var listGradeByCounsellor = await _dbContext.Entity<MsCounselorGrade>()
                                    .Include(e => e.Grade).ThenInclude(e=>e.MsLevel)
                                    .Where(e => e.Counselor.IdUser == param.IdUser
                                                && e.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear)
                                    .Select(e => new CodeWithIdVm
                                    {
                                        Id = e.Grade.MsLevel.Id,
                                        Code = e.Grade.MsLevel.Code,
                                        Description = e.Grade.MsLevel.Description
                                    })
                                    .ToListAsync(CancellationToken);

            items.AddRange(listGradeByCounsellor);
            #endregion

            items = items
                .GroupBy(e => new
                {
                    e.Id,
                    e.Code,
                    e.Description,
                })
                .Select(e => new CodeWithIdVm
                {
                    Id = e.Key.Id,
                    Code = e.Key.Code,
                    Description = e.Key.Description
                })
                .ToList();

            return Request.CreateApiResult2(items as object);
        }
    }
}
