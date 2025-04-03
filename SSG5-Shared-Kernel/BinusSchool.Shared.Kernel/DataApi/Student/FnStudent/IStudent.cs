using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudent : IFnStudent
    {
        [Get("/student/student-by-grade")]
        Task<ApiErrorResult<IEnumerable<GetStudentByGradeResult>>> GetStudentsByGrade(GetStudentByGradeRequest request);
        [Get("/student/student-map-by-grade")]
        Task<ApiErrorResult<IEnumerable<GetStudentMapByGradeResult>>> GetStudentMapByGrade(GetStudentMapByGradeRequest request);
        [Get("/student/student-copy-by-grade")]
        Task<ApiErrorResult<IEnumerable<GetStudentCopyByGradeResult>>> GetStudentCopyByGrade(GetStudentCopyByGradeRequest request);
        [Post("/student/student-by-binusianid")]
        Task<ApiErrorResult<IEnumerable<GetStudentUploadAscResult>>> GetStudentForUploadXML([Body] GetStudentUploadAscRequest body);

        [Get("/student/student-by-siblinggroup")]
        Task<ApiErrorResult<IEnumerable<GetStudentBySiblingGroupResult>>> GetStudentBySiblingGroup(GetStudentRequest query);

        [Get("/student/student_information")]
        Task<ApiErrorResult<IEnumerable<GetStudentResult>>> GetStudents(GetStudentRequest query);

        [Get("/student/student_information/{id}")]
        Task<ApiErrorResult<GetStudentDetailResult>> GetStudentDetail(string id);

        [Post("/student/student_information")]
        Task<ApiErrorResult> AddStudent([Body] AddStudentRequest body);

        [Put("/student/student_information")]
        Task<ApiErrorResult> UpdateStudent([Body] UpdateStudentRequest body);

        [Delete("/student/student_information")]
        Task<ApiErrorResult> DeleteStudent([Body] IEnumerable<string> ids);

        [Put("/student/student/Update-Student-Personal-Information")]
        Task<ApiErrorResult> UpdateStudentPersonalInformation([Body] UpdateStudentPersonalInformationRequest body);

        [Put("/student/student/Update-International-Student-Formalities")]
        Task<ApiErrorResult> UpdateInternationalStudentFormalities([Body] UpdateInternationalStudentFormalitiesRequest body);

        [Put("/student/student/Update-Student-Address-Information")]
        Task<ApiErrorResult> UpdateStudentAddressInformation([Body] UpdateStudentAddressInformationRequest body);

        [Put("/student/student/Update-Student-Contact-Information")]
        Task<ApiErrorResult> UpdateStudentContactInformation([Body] UpdateStudentContactInformationRequest body);

        [Put("/student/student/Update-Student-Other-Information")]
        Task<ApiErrorResult> UpdateStudentOtherInformation([Body] UpdateStudentOtherInformationRequest body);

        [Get("/student/studentlist-by-idparent")]
        Task<ApiErrorResult<IEnumerable<GetStudentByIdParentResult>>> GetStudentByIdParent(GetStudentByIdParentRequest query);

        [Get("/student/student/unmap-to-user")]
        Task<ApiErrorResult<IEnumerable<GetUnmapStudentResult>>> GetUnmapStudents(CollectionSchoolRequest query);

        [Get("/student/student/homeroom-student")]
        Task<ApiErrorResult<IEnumerable<GetStudentHomeroomResult>>> GetStudentHomeroom(GetStudentHomeroomRequest query);

        [Post("/student/student-information-changes-history")]
        Task<ApiErrorResult<IEnumerable<GetStudentInformationChangesHistoryResult>>> GetStudentInformationChangesHistory([Body] GetStudentInformationChangesHistoryRequest query);

        [Get("/student/student-all-with-status-homeroom")]
        Task<ApiErrorResult<IEnumerable<GetAllStudentWithStatusAndHomeroomResult>>> GetAllStudentWithStatusAndHomeroom(GetAllStudentWithStatusAndHomeroomRequest request);

        [Get("/student/student-all-enrollment")]
        Task<ApiErrorResult<IEnumerable<GetAllStudentEnrollmentResult>>> GetAllStudentEnrollment(GetAllStudentEnrollmentRequest request);

        [Get("/student/student-all-enrollment-no-id-homeroom")]
        Task<ApiErrorResult<IEnumerable<GetAllStudentEnrollmentResult>>> GetAllStudentEnrollmentNoIdHomeroom(GetAllStudentEnrollmentRequest request);

        [Post("/student/student-information-for-bnsreport")]
        Task<ApiErrorResult<GetStudentInformationForBNSReportResult>> GetStudentInformationForBNSReport([Body] GetStudentInformationForBNSReportRequest body);

        [Get("/student/student_information-encrypted")]
        Task<ApiErrorResult<GetStudentDetailResult>> GetStudentDetailEncrypted(GetStudentDetailRequest query);

        [Get("/student/student-by-siblinggroup-encrypted")]
        Task<ApiErrorResult<IEnumerable<GetStudentBySiblingGroupResult>>> GetStudentBySiblingGroupEncrypted(GetStudentRequest query);

        [Get("/student/student_information-student-status")]
        Task<ApiErrorResult<GetStudentInformationWithStudentStatusResult>> GetStudentInformationWithStudentStatus(GetStudentInformationWithStudentStatusRequest query);
        [Get("/student/student-multiple-grade")]
        Task<ApiErrorResult<IEnumerable<GetStudentMultipleGradeResult>>> GetStudentMultipleGrade(GetStudentMultipleGradeRequest query);

        [Post("/student​/student-for-salesforce")]
        Task<ApiErrorResult<IEnumerable<GetStudentForSalesForceRequest>>> GetStudentForSalesForce([Body] GetStudentForSalesForceRequest body);
    }
}
