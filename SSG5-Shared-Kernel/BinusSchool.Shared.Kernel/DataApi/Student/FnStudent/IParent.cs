using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IParent : IFnStudent
    {
        [Get("/student/parent_information")]
        Task<ApiErrorResult<IEnumerable<GetParentResult>>> GetParents(GetParentRequest query);

        [Get("/student/parent_information/{id}")]
        Task<ApiErrorResult<GetParentDetailResult>> GetParentDetail(string id);

        [Get("/student/family-information")]
        Task<ApiErrorResult<IEnumerable<GetParentDetailResult>>> GetFamilyByStudent(GetStudentRequest query);

        [Post("/student/parent_information")]
        Task<ApiErrorResult> AddParent([Body] AddParentRequest body);

        [Put("/student/parent_information")]
        Task<ApiErrorResult> UpdateParent([Body] UpdateParentRequest body);

        [Delete("/student/parent_information")]
        Task<ApiErrorResult> DeleteParent([Body] IEnumerable<string> ids);

        [Put("/student/parent_information/Update-Parent-Personal-Information")]
        Task<ApiErrorResult> UpdateParentPersonalInformation([Body] UpdateParentPersonalInformationRequest body);

        [Put("/student/parent_information/Update-Parent-Address-Information")]
        Task<ApiErrorResult> UpdateParentAddressInformation([Body] UpdateParentAddressInformationRequest body);

        [Put("/student/parent_information/Update-Parent-Contact-Information")]
        Task<ApiErrorResult> UpdateParentContactInformation([Body] UpdateParentContactInformationRequest body);

        [Put("/student/parent_information/Update-Parent-Occupation-Information")]
        Task<ApiErrorResult> UpdateParentOccupationInformation([Body] UpdateParentOccupationInformationRequest body);

        [Get("/student/parent_information_childrens")]
        Task<ApiErrorResult<IEnumerable<GetChildResult>>> GetChildrens(GetChildRequest query);

        [Get("/student/parent_information_for_generate")]
        Task<ApiErrorResult<GetParentGenerateAccountResult>> GetParentGenerateAccount(GetParentGenerateAccountRequest query);

        [Get("/student/parent_information_encrypted")]
        Task<ApiErrorResult<GetParentDetailResult>> GetParentDetailEncrypted(GetParentDetailRequest query);
    }
}
