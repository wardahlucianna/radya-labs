using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitSetting;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentExitSetting : IFnStudent
    {
        [Get("/student/student-exit-setting")]
        Task<ApiErrorResult<IEnumerable<GetAllStudentExitSettingResult>>> GetAllStudentExitSetting(GetAllStudentExitSettingRequest query);

        [Put("/student/student-exit-setting")]
        Task<ApiErrorResult> UpdateStudentExitSetting([Body] UpdateStudentExitSettingRequest query);
    }
}
