using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using Refit;

namespace BinusSchool.Data.Api.Student.FnStudent
{
    public interface IStudentDemographicsReport : IFnStudent
    {
        #region Total Student
        [Post("/student-demographics-report/get-student-demography-report-total")]
        Task<ApiErrorResult<GetSDRTotalStudentReportsResult>> GetStudentDemographicsReportTotalStudent([Body] GetSDRTotalStudentReportsRequest body);

        [Post("/student-demographics-report/get-student-demography-report-total-detail")]
        Task<ApiErrorResult<IEnumerable<GetSDRTotalStudentReportDetailsResult>>> GetStudentDemographicsReportTotalDetailsStudent([Body] GetSDRTotalStudentReportDetailsRequest body);
        #endregion

        #region Religion
        [Post("/student-demographics-report/get-student-demography-report-religion")]
        Task<ApiErrorResult<IEnumerable<GetSDRReligionReportsResult>>> GetStudentDemographicsReportReligion([Body] GetSDRReligionReportsRequest body);

        [Post("/student-demographics-report/get-student-demography-report-religion-detail")]
        Task<ApiErrorResult<IEnumerable<GetSDRReligionReportDetailsResult>>> GetStudentDemographicsReportReligionDetails([Body] GetSDRReligionReportDetailsRequest body);
        #endregion

        #region Nationality
        [Post("/student-demographics-report/get-student-nationality-demography")]
        Task<ApiErrorResult<IEnumerable<GetStudentNationalityDemographyResult>>> GetStudentNationalityDemography([Body] GetStudentNationalityDemographyRequest body);

        [Post("/student-demographics-report/get-student-nationality-demography-detail")]
        Task<ApiErrorResult<GetStudentNationalityDemographyDetailResult>> GetStudentNationalityDemographyDetail([Body] GetStudentNationalityDemographyDetailRequest body);
        #endregion

        #region Gender
        [Post("/student-demographics-report/get-student-gender-demography")]
        Task<ApiErrorResult<IEnumerable<GetStudentGenderDemographyResult>>> GetStudentGenderDemography([Body] GetStudentGenderDemographyRequest body);

        [Post("/student-demographics-report/get-student-gender-demography-detail")]
        Task<ApiErrorResult<IEnumerable<GetStudentGenderDemographyDetailResult>>> GetStudentGenderDemographyDetail([Body] GetStudentGenderDemographyDetailRequest body);
        #endregion

        #region Total Family
        [Post("/student-demographics-report/get-student-total-family-demographics")]
        Task<ApiErrorResult<IEnumerable<GetStudentTotalFamilyDemographicsResult>>> GetStudentTotalFamilyDemographics([Body] GetStudentTotalFamilyDemographicsRequest body);

        [Post("/student-demographics-report/get-student-total-family-demographics-detail")]
        Task<ApiErrorResult<IEnumerable<GetStudentTotalFamilyDemographicsDetailResult>>> GetStudentTotalFamilyDemographicsDetail([Body] GetStudentTotalFamilyDemographicsDetailRequest body);
        #endregion

        [Get("/student-demographics-report/get-student-demography-report-type")]
        Task<ApiErrorResult<IEnumerable<GetStudentDemographyReportTypeResult>>> GetStudentDemographyReportType(GetStudentDemographyReportTypeRequest query);

        #region Excel
        [Post("/student-demographics-report/master-student-demographics-generate-excel")]
        Task<HttpResponseMessage> MasterStudentDemographicsGenerateExcel([Body] MasterStudentDemographicsGenerateExcelRequest body);
        #endregion
    }
}
