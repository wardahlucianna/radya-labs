using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.CASStudentAdvisor;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface ICASStudentAdvisor : IStudent
    {
        [Get("/casstudentadvisor/get-list-filter-advisor")]
        Task<ApiErrorResult<List<GetFilterCASStudentAdvisorResult>>> GetListFilterAdvisorByGradeAcademicYear(GetFilterCASStudentAdvisorRequest query);

        [Get("/casstudentadvisor/list-student-advisor")]
        Task<ApiErrorResult<List<GetListCASStudentAdvisorResult>>> GetCASStudentAdvisor(GetListCASStudentAdvisorRequest query);

        [Put("/casstudentadvisor/save-cas-student-mapping")]
        Task<ApiErrorResult<SaveCasStudentMappingAdvisorResult>> SaveCasStudentMapping([Body] SaveCasStudentMappingAdvisorRequest query);

        [Get("/casstudentadvisor/get-list-advisor")]
        Task<ApiErrorResult<List<GetListCASAdvisorResult>>> GetListCASAdvisor(GetListCASAdvisorRequest query);

        [Delete("/casstudentadvisor/delete-cas-advisor")]
        Task<ApiErrorResult> DeleteCASAdvisor([Body] DeleteCASAdvisorRequest query);

        [Get("/casstudentadvisor/get-list-teacher-for-cas")]
        Task<ApiErrorResult<List<GetListTeacherForCASResult>>> GetListTeacherForCAS(GetListTeacherForCASRequest query);

        [Post("/casstudentadvisor/add-cas-advisor")]
        Task<ApiErrorResult> AddCASAdvisor([Body] AddCASAdvisorRequest query);
    }
}
