using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentStatus;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentStatus : IFnStudent
    {
        [Get("/student/student-status/lt-student-status")]
        Task<ApiErrorResult<IEnumerable<GetLtStudentStatusResult>>> GetLtStudentStatus(GetLtStudentStatusRequest param);

        [Get("/student/student-status/student-status-view-configuration")]
        Task<ApiErrorResult<GetStudentStatusViewConfigurationResult>> GetStudentStatusViewConfiguration(GetStudentStatusViewConfigurationRequest param);

        [Get("/student/student-status/get-student-status-list-by-ay")]
        Task<ApiErrorResult<IEnumerable<GetStudentStatusListByAYResult>>> GetStudentStatusListByAY(GetStudentStatusListByAYRequest param);

        [Post("/student/student-status/update")]
        Task<ApiErrorResult> UpdateStudentStatus([Body]UpdateStudentStatusRequest param);

        [Post("/student/student-status/create-record")]
        Task<ApiErrorResult> CreateStudentStatusRecord([Body] CreateStudentStatusRecordRequest param);

        [Get("/student/student-status/get-history")]
        Task<ApiErrorResult<IEnumerable<GetStudentStatusHistoryResult>>> GetStudentStatusHistory(GetStudentStatusHistoryRequest param);

        [Get("/student/student-status/get-unmapped-student-status-by-ay")]
        Task<ApiErrorResult<IEnumerable<GetUnmappedStudentStatusByAYResult>>> GetUnmappedStudentStatusByAY(GetUnmappedStudentStatusByAYRequest param);

        [Post("/student/student-status/generate-mapping-active-ay")]
        Task<ApiErrorResult> GenerateStudentStatusMappingActiveAY([Body] GenerateStudentStatusMappingActiveAYRequest param);
    }
}
