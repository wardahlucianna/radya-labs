using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherAssignment;
using Refit;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherAssignment : IFnAssignment
    {
        [Get("/assignment/teacher")]
        Task<ApiErrorResult<IEnumerable<TeacherAssignmentGetListResult>>> GetTeacherAssignments(TeacherAssignmentGetListRequest query);

        [Get("/assignment/teacher/detail")]
        Task<ApiErrorResult<TeacherAssignmentGetDetailResult>> GetTeacherAssignmentDetail(TeacherAssignmentDetailRequest query);

        [Post("/assignment/teacher")]
        Task<ApiErrorResult> AddTeacherAssignment([Body] AddTeacherAssignmentRequest body);

        [Delete("/assignment/teacher/delete-non-academic")]
        Task<ApiErrorResult> DeleteNonAcademic([Body] IEnumerable<string> ids);

        [Delete("/assignment/teacher/delete-academic")]
        Task<ApiErrorResult> DeleteAcademic([Body] IEnumerable<string> ids);

        #region copy
        [Get("/assignment/teacher/copy")]
        Task<ApiErrorResult<GetTeacherAssignmentCopyResult>> GetTeacherAssignmentCopy(GetTeacherAssignmentCopyRequest query);

        [Post("/assignment/teacher/copy")]
        Task<ApiErrorResult> TeacherAssignmentCopy(TeacherAssignmentCopyRequest query);
        #endregion
    }
}
