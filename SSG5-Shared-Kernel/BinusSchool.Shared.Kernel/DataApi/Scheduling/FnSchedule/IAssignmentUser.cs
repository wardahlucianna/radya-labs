using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.TeacherHomeroomAndSubjectTeacher;
using Refit;
namespace BinusSchool.Data.Api.Scheduling.FnSchedule
{
    public interface IAssignmentUser : IFnSchedule
    {
        [Get("/schedule/teacher-assignment")]
        Task<ApiErrorResult<IEnumerable<TeacherHomeroomAndSubjectTeacherResult>>> GetAssigmentByTeacher(TeacherHomeroomAndSubjectTeacherRequest query);
    }
}
