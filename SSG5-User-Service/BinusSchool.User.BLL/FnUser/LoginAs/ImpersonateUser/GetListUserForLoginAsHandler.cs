using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ImpersonateUser;
using BinusSchool.Data.Model.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.Scheduling;
using BinusSchool.User.FnUser.LoginAs.ManageAccessRoleLoginAs;
using Microsoft.EntityFrameworkCore;

namespace BinusSchool.User.FnUser.LoginAs.ImpersonateUser
{
    public class GetListUserForLoginAsHandler : FunctionsHttpSingleHandler
    {
        private readonly IUserDbContext _dbContext;
        private readonly GetAllRoleDataForManageAccessRoleLoginAsHandler _getAllRoleDataForManageAccessRoleLoginAsHandler;

        public GetListUserForLoginAsHandler(
            IUserDbContext dbContext,
            GetAllRoleDataForManageAccessRoleLoginAsHandler getAllRoleDataForManageAccessRoleLoginAsHandler
            )
        {
            _dbContext = dbContext;
            _getAllRoleDataForManageAccessRoleLoginAsHandler = getAllRoleDataForManageAccessRoleLoginAsHandler;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = Request.ValidateParams<GetListUserForLoginAsRequest>(
                nameof(GetListUserForLoginAsRequest.IdUser),
                nameof(GetListUserForLoginAsRequest.IdSchool),
                nameof(GetListUserForLoginAsRequest.IdAcademicYear),
                nameof(GetListUserForLoginAsRequest.Semester),
                nameof(GetListUserForLoginAsRequest.IdRoleGroup),
                nameof(GetListUserForLoginAsRequest.RoleGroupCode)
                );

            var result = new List<GetListUserForLoginAsResult>();

            var getAllRoleDataForManageAccessRoleLoginAs = await _getAllRoleDataForManageAccessRoleLoginAsHandler.GetAllRoleDataForManageAccessRoleLoginAs(new GetAllRoleDataForManageAccessRoleLoginAsRequest
            {
                IdSchool = param.IdSchool
            });

            var getAllRoleUser = _dbContext.Entity<MsUserRole>()
                    .Where(x => x.IdUser == param.IdUser)
                    .Select(x => x.IdRole)
                    .Distinct()
                    .ToList();

            var getAllAuthorizedRole = getAllRoleDataForManageAccessRoleLoginAs
                    .Where(x => x.IdAuthorizedRole != null)
                    .Where(x => getAllRoleUser.Any(y => y == x.IdRole))
                    .Distinct()
                    .ToList();

            if(getAllAuthorizedRole.Any(x => x.AuthorizedRoleGroupCode == param.RoleGroupCode))
            {
                if (param.RoleGroupCode == RoleConstant.Parent || param.RoleGroupCode == RoleConstant.Student)
                {
                    result = GetDataStudentForLoginAs(param, getAllAuthorizedRole);
                }
                else
                {
                    result = GetDataStaffForLoginAs(param, getAllAuthorizedRole);
                }
            }

            return Request.CreateApiResult2(result as object);
        }

        private List<GetListUserForLoginAsResult> GetDataStudentForLoginAs(GetListUserForLoginAsRequest param, List<GetAllRoleDataForManageAccessRoleLoginAsResult> getAllAuthorizedRole)
        {
            var predicate = PredicateBuilder.Create<MsHomeroomStudent>(x => x.Homeroom.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.Homeroom.IdAcademicYear == param.IdAcademicYear);
            if (!string.IsNullOrEmpty(param.Semester.ToString()))
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Homeroom.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Homeroom.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.IdHomeroom == param.IdHomeroom);
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x => EF.Functions.Like((string.IsNullOrWhiteSpace(x.Student.FirstName) ? "" : x.Student.FirstName.Trim() + " ") +
                                       (string.IsNullOrWhiteSpace(x.Student.MiddleName) ? "" : x.Student.MiddleName.Trim() + " ") +
                                       (string.IsNullOrWhiteSpace(x.Student.LastName) ? "" : x.Student.LastName.Trim()), param.SearchPattern()
                                       ));

            var canUserLoginAsParent = getAllAuthorizedRole
                    .Any(x => x.AuthorizedRoleGroupCode == RoleConstant.Parent);

            var canUserLoginAsStudent = getAllAuthorizedRole
                    .Any(x => x.AuthorizedRoleGroupCode == RoleConstant.Student);

            var data = _dbContext.Entity<MsHomeroomStudent>()
                    .Where(predicate)
                    .Select(x => new 
                    {
                        IdStudent = x.IdStudent,
                        IdHomeroom = x.IdHomeroom,
                        HomeroomDesc = x.Homeroom.Grade.Description + x.Homeroom.GradePathwayClassroom.MsClassroom.Code,
                        Name = NameUtil.GenerateFullName(x.Student.FirstName, x.Student.LastName),
                    }).Distinct().ToList();

            #region Data Student
            var getUserRoleData = GetUserRoleData(param);

            var getStudentData = getUserRoleData
                    .Where(x => x.RoleGroupCode == RoleConstant.Student)
                    .Distinct().ToList();

            var getParentData = getUserRoleData
                    .Where(x => x.RoleGroupCode == RoleConstant.Parent)
                    .Distinct().ToList();

            var joinStudentParentData = getStudentData
                    .Join( getParentData, 
                           student => student.IdUser,
                           parent => parent.IdStudent,
                           (student, parent) => new
                           {
                               IdStudent = student.IdUser,
                               UsernameStudent = student.UserName,
                               StudentDisplayName = student.DisplayName,
                               UsernameParent = parent.UserName,
                               ParentDisplayName = parent.DisplayName
                           })
                    .Distinct().ToList();
            #endregion

            var getAllRoleDataForManageAccessRoleLoginAs = data
                    .GroupJoin(joinStudentParentData,
                        Student => (Student.IdStudent),
                        User => (User.IdStudent),
                        (Student, User) => new { Student, User }
                    ).SelectMany(x => x.User.DefaultIfEmpty(),
                    (HomeroomStudent, StudentParentData) => new 
                    {
                        BinusianID = HomeroomStudent.Student.IdStudent,
                        BinusianUsername = StudentParentData?.IdStudent ?? HomeroomStudent.Student.IdStudent,
                        BinusianDisplayName = StudentParentData?.StudentDisplayName ?? HomeroomStudent.Student.Name,
                        IdHomeroom = HomeroomStudent.Student.IdHomeroom,
                        HomeroomDesc = HomeroomStudent.Student.HomeroomDesc,
                        IsStudentOrParent = true,
                        CanloginAsStudent = canUserLoginAsStudent,
                        CanLoginAsParent = canUserLoginAsParent,
                        ParentUsername = StudentParentData?.UsernameParent,
                        ParentDisplayName = StudentParentData?.ParentDisplayName
                    }).Distinct().ToList();

            var retVal = getAllRoleDataForManageAccessRoleLoginAs
                    .Select(x => new GetListUserForLoginAsResult
                    {
                        BinusianID = x.BinusianID,
                        BinusianUsername = x.BinusianUsername,
                        BinusianDisplayName = x.BinusianDisplayName,
                        Homeroom = new ItemValueVm {
                            Id = x.IdHomeroom,
                            Description = x.HomeroomDesc
                        },
                        IsStudentOrParent = x.IsStudentOrParent,
                        CanLoginAsStudent = x.CanloginAsStudent,
                        CanLoginAsParent = x.CanLoginAsParent,
                        ParentUsername = x.ParentUsername,
                        ParentDisplayName = x.ParentDisplayName
                    }).Distinct().ToList();

            return retVal;
        }

        private List<GetListUserForLoginAsResult> GetDataStaffForLoginAs(GetListUserForLoginAsRequest param, List<GetAllRoleDataForManageAccessRoleLoginAsResult> getAllAuthorizedRole)
        {
            var getUserRoleData = GetUserRoleData(param);
            getAllAuthorizedRole = getAllAuthorizedRole.Where(x => x.AuthorizedRoleGroupCode == param.RoleGroupCode).ToList();

            var getDataStaff = getUserRoleData
                    .Where(x => getAllAuthorizedRole.Any(y => y.IdAuthorizedRole == x.IdRole))
                    .Select(x => new 
                    {
                        BinusianID = x.IdUser,
                        BinusianUsername = x.UserName,
                        BinusianDisplayName = x.DisplayName
                    }).Distinct().ToList();

            var retVal = getDataStaff.Select(x => new GetListUserForLoginAsResult
                    {
                        BinusianID = x.BinusianID,
                        BinusianUsername = x.BinusianUsername,
                        BinusianDisplayName = x.BinusianDisplayName
            }).ToList();

            return retVal;
        }

        private List<GetListUserForLoginAsResult_RoleUser> GetUserRoleData(GetListUserForLoginAsRequest param)
        {
            var predicate = PredicateBuilder.Create<MsUserRole>(x => x.Role.IdSchool == param.IdSchool /*&& x.Role.RoleGroup.Code == param.RoleGroupCode*/);

            if (!string.IsNullOrEmpty(param.Search))
                    predicate = predicate.And(x =>
                    EF.Functions.Like(x.Role.RoleGroup.Code, param.SearchPattern())
                    || EF.Functions.Like(x.Role.Code, param.SearchPattern())
                    || EF.Functions.Like(x.IdUser, param.SearchPattern())
                    || EF.Functions.Like(x.Username, param.SearchPattern())
                    || EF.Functions.Like(x.User.DisplayName, param.SearchPattern())
                    );

            var getUserRoleData = _dbContext.Entity<MsUserRole>()
                    .Where(predicate)
                    .Select(x => new GetListUserForLoginAsResult_RoleUser
                    {
                        IdRoleGroup = x.Role.RoleGroup.Id,
                        RoleGroupCode = x.Role.RoleGroup.Code,
                        IdRole = x.Role.Id,
                        RoleCode = x.Role.Code,
                        IdUser = x.IdUser,
                        IdStudent = x.Role.RoleGroup.Code == RoleConstant.Parent ? x.IdUser.Substring(1) : x.IdUser,
                        UserName = x.Username,
                        DisplayName = x.User.DisplayName
                    }).AsQueryable();

            return getUserRoleData.ToList();
        }
    }
}
