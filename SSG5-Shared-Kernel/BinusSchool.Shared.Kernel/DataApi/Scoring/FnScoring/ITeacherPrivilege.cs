using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scoring.FnScoring.TeacherPrivilege;
using Refit;

namespace BinusSchool.Data.Api.Scoring.FnScoring
{
    public interface ITeacherPrivilege : IFnScoring
    {
        [Get("/scoring/teacher-privilage-by-iduser")]
        Task<ApiErrorResult<GetTeacherPrivilageByIdUserResult>> GetTeacherPrivilageByIdUser(GetTeacherPrivilageByIdUserRequest query);

        [Get("/scoring/GetTeacherByPrivilageCALH")]
        Task<ApiErrorResult<GetTeacherByPrivilageResult>> GetTeacherByPrivilage(GetTeacherByPrivilageRequest query);

        [Get("/scoring/teacher-privilege")]
        Task<ApiErrorResult<GetTeacherPrivilegeResult>> GetTeacherPrivilege(GetTeacherPrivilegeRequest query);

        [Get("/scoring/teacher-privilege-forTeacherComment")]
        Task<ApiErrorResult<GetTeacherPrivilageForTeacherCommentResult>> GetTeacherPrivilageForTeacherComment(GetTeacherPrivilegeRequest query);

        [Get("/scoring/teacher-privilege-avaiability-position")]
        Task<ApiErrorResult<IEnumerable<CodeWithIdVm>>> GetAvailabilityPositionByIdUser(GetAvailabilityPositionByIdUserRequest query);

        [Post("/scoring/GetAllDataTeacherPrivilege")]
        Task<ApiErrorResult<List<GetAllDataTeacherPrivilegeResult>>> GetAllDataTeacherPrivilege([Body] GetAllDataTeacherPrivilegeRequest query);

        [Post("/scoring/GetUserActionNextByTeacherPosition")]
        Task<ApiErrorResult<List<GetUserActionNextByTeacherPositionResult>>> GetUserActionNextByTeacherPosition([Body] GetUserActionNextByTeacherPositionRequest query);
    }
}
