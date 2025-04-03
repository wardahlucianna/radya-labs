using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;
using BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentInfoUpdate : IFnStudent
    {
        [Get("/student/student-info-update")]
        Task<ApiErrorResult<IEnumerable<GetStudentInfoUpdateResult>>> GetStudentInfoUpdates(GetStudentInfoUpdateRequest query);

        [Get("/student/student-info-update")]
        Task<ApiErrorResult<IEnumerable<GetStudentInfoUpdateApprovalResult>>> GetStudentInfoUpdateByClass(GetMappingClassRequest query);
        
        [Get("/student/get-detail-summary-By-class")]
        Task<ApiErrorResult<IEnumerable<GetDetailApprovalSummaryResult>>> GetDetailSummaryByClass(GetDetailApprovalSummaryRequest query);

        [Put("/student/student-info-update-save")]
        Task<ApiErrorResult> UpdateStudentInfoUpdate([Body] GetStudentInfoUpdateRequest[] body);

        [Put("/student/parent-info-update-save")]
        Task<ApiErrorResult> UpdateParentInfoUpdate([Body] GetStudentInfoUpdateRequest[] body);

        [Put("/student/prevschool-info-update-save")]
        Task<ApiErrorResult> UpdateStudentPrevSchoolInfo([Body] UpdateStudentInfoUpdate[] body);

        [Put("/student/siblinggroup-info-update-save")]
        Task<ApiErrorResult> UpdateSiblingGroupInfoUpdate([Body] GetStudentInfoUpdateRequest[] body);

        [Put("/student/bank-account-info-update-save")]
        Task<ApiErrorResult> UpdateBankAccountInformationInfoUpdate([Body] GetStudentInfoUpdateRequest[] body);
        
    }
}
