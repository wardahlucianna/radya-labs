using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Homeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentHomeroomDetail;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IStudentHomeroomDetail : IFnSchedule
    {
        [Get("/schedule/student-homeroom-detail")]
        Task<ApiErrorResult<GetStudentHomeroomDetailResult>> GetHomeroomByStudentId(GetStudentHomeroomDetailRequest query);

        [Get("/schedule/get-grade-and-class-student")]
        Task<ApiErrorResult<IEnumerable<GetGradeAndClassByStudentResult>>> GetGradeAndClassByStudents(GetHomeroomByStudentRequest query);

        [Post("/schedule/get-homeroom-student-by-level-grade")]
        Task<ApiErrorResult<List<GetHomeroomStudentByLevelGradeResult>>> GetHomeroomStudentByLevelGrade([Body] GetHomeroomStudentByLevelGradeRequest body);
    }
}
