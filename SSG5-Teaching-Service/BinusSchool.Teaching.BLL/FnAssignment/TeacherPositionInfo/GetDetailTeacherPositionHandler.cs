using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Employee.FnStaff;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities;
using BinusSchool.Persistence.TeachingDb.Entities.School;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using BinusSchool.Persistence.TeachingDb.Entities.Employee;
using Microsoft.EntityFrameworkCore;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;

namespace BinusSchool.Teaching.FnAssignment.TeacherPositionInfo
{
    public class GetDetailTeacherPositionHandler : FunctionsHttpSingleHandler
    {
        private readonly ITeachingDbContext _dbContext;
        public GetDetailTeacherPositionHandler(
            ITeachingDbContext dbContext,
            IApiService<IMetadata> metaData,
            IApiService<IUser> userApi)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {

            var param = Request.ValidateParams<GetDetailTeacherPositionByUserIDRequest>(nameof(GetDetailTeacherPositionByUserIDRequest.UserId));

            var userRole = await _dbContext.Entity<MsUser>()
                            .Include(x => x.UserRoles).ThenInclude(y => y.Role).ThenInclude(z => z.RoleGroup)
                            .Include(x => x.UserSchools).ThenInclude(a => a.School)
                            .Select(x => new GetDetailTeacherPositionByUserIDResult
                            {
                                Id = x.Id,
                                UserName = x.Username,
                                DisplayName = x.DisplayName,
                                Roles = x.UserRoles.Select(y => new DetailUserRole
                                {
                                    Id = y.Role.Id,
                                    Name = y.Role.Code,
                                    RoleGroup = new NameValueVm
                                    {
                                        Id = y.Role.RoleGroup.Id,
                                        Name = y.Role.RoleGroup.Code,
                                    }
                                }).ToList(),
                                School = x.UserSchools.Select(y => new NameValueVm
                                {
                                    Id = y.School.Id,
                                    Name = y.School.Name,
                                }).FirstOrDefault(),
                                NonTeachingAssignmentAcademic = _dbContext.Entity<TrNonTeachingLoad>()
                                    .Include(x => x.MsNonTeachingLoad)
                                      .ThenInclude(x => x.TeacherPosition)
                                    .Where(x => x.IdUser == param.UserId)
                                    .Where(x => x.MsNonTeachingLoad.Category == AcademicType.Academic)
                                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
                                    .Select(x => new DetailNonteacingLoadAcademic()
                                    {
                                        Data = x.Data,
                                        PositionName = x.MsNonTeachingLoad.TeacherPosition.Position.Description,
                                        PositionShortName = x.MsNonTeachingLoad.TeacherPosition.Position.Code
                                    }).ToList(),
                                NonTeachingAssignmentNonAcademic = _dbContext.Entity<TrNonTeachingLoad>()
                                    .Include(x => x.MsNonTeachingLoad)
                                      .ThenInclude(x => x.TeacherPosition)
                                    .Where(x => x.IdUser == param.UserId)
                                    .Where(x => x.MsNonTeachingLoad.Category == AcademicType.NonAcademic)
                                    .Where(x => x.MsNonTeachingLoad.IdAcademicYear == param.IdSchoolAcademicYear)
                                    .Select(x => new DetailNonteacingLoadNonAcademic()
                                    {
                                        Data = x.Data,
                                        PositionName = x.MsNonTeachingLoad.TeacherPosition.Position.Description,
                                        PositionShortName = x.MsNonTeachingLoad.TeacherPosition.Position.Code
                                    }).ToList()
                            })
                            .SingleOrDefaultAsync(x => x.Id == param.UserId);

            return Request.CreateApiResult2(userRole as object);
        }

    }
}
