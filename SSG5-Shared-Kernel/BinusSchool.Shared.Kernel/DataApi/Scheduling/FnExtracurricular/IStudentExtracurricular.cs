using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using Refit;

namespace BinusSchool.Data.Api.Scheduling.FnExtracurricular
{
    public interface IStudentExtracurricular : IFnExtracurricular
    {
        [Post("/student-extracurricular/get-student-extracurricular")]
        Task<ApiErrorResult<IEnumerable<GetStudentExtracurricularResult>>> GetStudentExtracurricular([Body] GetStudentExtracurricularRequest body);

        [Get("/student-extracurricular/get-detail-student-extracurricular")]
        Task<ApiErrorResult<GetDetailStudentExtracurricularResult>> GetDetailStudentExtracurricular(GetDetailStudentExtracurricularRequest body);

        [Put("/student-extracurricular/update-student-extracurricular-priority")]
        Task<ApiErrorResult> UpdateStudentExtracurricularPriority([Body] UpdateStudentExtracurricularPriorityRequest body);

        [Post("/student-extracurricular/student-extracurricular-excel")]
        Task<HttpResponseMessage> ExportExcelStudentExtracurricular([Body] ExportExcelStudentExtracurricularRequest body);
    }
}
