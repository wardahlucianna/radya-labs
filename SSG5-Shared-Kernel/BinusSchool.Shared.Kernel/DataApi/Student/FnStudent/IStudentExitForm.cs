using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitForm;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitHistory;
using BinusSchool.Data.Model.Student.FnStudent.StudentExitReason;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentExitForm : IFnStudent
    {
        [Get("/student/student-exit-form")]
        Task<ApiErrorResult<IEnumerable<GetStudentExitFormResult>>> GetStudentExitForms(GetStudentExitFormRequest query);

        [Get("/student/student-exit-form/detail/{id}")]
        Task<ApiErrorResult<GetStudentExitFormDetailResult>> GetStudentExitFormDetail(string id);

        [Post("/student/student-exit-form")]
        Task<ApiErrorResult> AddStudentExitForm([Body] AddStudentExitFormRequest body);

        [Put("/student/student-exit-form")]
        Task<ApiErrorResult> UpdateStudentExitForm([Body] UpdateStudentExitFormRequest body);

        [Delete("/student/student-exit-form")]
        Task<ApiErrorResult> DeleteStudentExitForm([Body] IEnumerable<string> ids);

        [Get("/student/student-exit-form/list-reason")]
        Task<ApiErrorResult<IEnumerable<GetStudentExitReasonResult>>> GetListStudentExitReasons(GetStudentExitReasonRequest query);

        [Get("/student/student-exit-form/list-history")]
        Task<ApiErrorResult<IEnumerable<GetStudentExitHistoryResult>>> GetListStudentExitHistories(GetStudentExitHistoryRequest query);

        [Get("/student/student-exit-form/parent-by-child")]
        Task<ApiErrorResult<GetParentByChildResult>> GetParentByChild(GetParentByChildRequest query);
        [Get("/student/student-exit-form/access")]
        Task<ApiErrorResult<GetAccessStudentExitFormResult>> GetAccessStudentExitForm(GetAccessStudentExitFormRequest query);
    }
}
