using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using Refit;
using BinusSchool.Data.Model.Employee.FnStaff.StaffCertificationInformation;

namespace BinusSchool.Data.Api.Employee.FnStaff
{
    public interface IStaffCertificationInformation : IFnStaff
    {
        [Get("/employee/staff-certification-information")]
        Task<ApiErrorResult<IEnumerable<GetStaffCertificationInformationResult>>> GetStaffCertificationInformations(GetStaffCertificationInformationRequest query);

        //[Get("/employee/staff-certification-information/{id}")]
        //Task<ApiErrorResult<IEnumerable<GetStaffCertificationInformationResult>>> GetStudentPrevSchoolInfoDetail(string id);

        [Post("/employee/staff-certification-information")]
        Task<ApiErrorResult> AddStaffCertificationInformation([Body] AddStaffCertificationInformationRequest body);

        [Put("/employee/staff-certification-information")]
        Task<ApiErrorResult> UpdateStaffCertificationInformation([Body] UpdateStaffCertificationInformationRequest body);

        [Delete("/employee/staff-certification-information")]
        Task<ApiErrorResult> DeleteStaffCertificationInformation([Body] DeleteStaffCertificationInformationRequest ids);
    }
}
