using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Teaching.FnAssignment.TeacherPositionInfo;

namespace BinusSchool.Data.Api.Teaching.FnAssignment
{
    public interface ITeacherPositionInfo : IFnAssignment
    {
        [Post("/teaching/TeacherPositionInfo/GetTeacherPositionByUserID")]
        Task<ApiErrorResult<GetTeacherPositionByUserIDResult>> GetTeacherPositionByUserID([Body] GetTeacherPositionByUserIDRequest body);

        [Post("/teaching/TeacherPositionInfo/GetTeacherPositionByUserID/all")]
        Task<ApiErrorResult<IEnumerable<GetTeacherPositionByUserIDResult>>> GetTeacherPositionsByUserID([Body] GetTeacherPositionByUserIDRequest body);

        [Get("/teaching/TeacherPositionInfo/GetDetailTeacherPosition")]
        Task<ApiErrorResult<GetTeacherPositionByUserIDResult>> GetDetailTeacherPosition(GetTeacherPositionByUserIDRequest query);

    }
}
