using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPosition;
using Refit;


namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherPosition : IFnAssignment
    {
        [Get("/assignment/teacher/position")]
        Task<ApiErrorResult<IEnumerable<GetTeacherPositionResult>>> GetTeacherPositions(GetTeacherPositionRequest query);

        [Get("/assignment/teacher/position/{id}")]
        Task<ApiErrorResult<GetTeacherPositionDetailResult>> GetTeacherPositionDetail(string id);

        [Get("/assignment/teacher/position/get-ca")]
        Task<ApiErrorResult<string>> GetPositionCAforAsc([Query]GetCAForAscTimetableRequest data);

        [Post("/assignment/teacher/position")]
        Task<ApiErrorResult> AddTeacherPosition([Body] AddTeacherPositionRequest body);

        [Put("/assignment/teacher/position")]
        Task<ApiErrorResult> UpdateTeacherPosition([Body] UpdateTeacherPositionRequest body);

        [Delete("/assignment/teacher/position")]
        Task<ApiErrorResult> DeleteTeacherPosition([Body] IEnumerable<string> ids);
        [Get("/assignment/teacher/position-has-not-non-teaching-load")]
        Task<ApiErrorResult<IEnumerable<GetTeacherPositionHasNotNonTeachingLoadResult>>> GetTeacherPositionsHasNotNonTeachingLoad(GetTeacherPositionHasNotNonTeachingLoadRequest query);
        [Get("/assignment/teacher/position/role")]
        Task<ApiErrorResult<IEnumerable<ItemValueVm>>> GetTeacherPositionByRole(GetTeacherPositionByRoleRequest query);
    }
}
