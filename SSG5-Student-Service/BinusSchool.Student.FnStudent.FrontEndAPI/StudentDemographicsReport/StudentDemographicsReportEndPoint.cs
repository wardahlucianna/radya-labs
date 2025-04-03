using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class StudentDemographicsReportEndPoint
    {
        private const string _route = "student-demographics-report";
        private const string _tag = "Student Demographics Report";

        #region Nationality
        private readonly GetStudentNationalityDemographyHandler _getStudentNationalityDemographyHandler;
        private readonly GetStudentNationalityDemographyDetailHandler _getStudentNationalityDemographyDetailHandler;
        #endregion
        #region Gender
        private readonly GetStudentGenderDemographyHandler _getStudentGenderDemographyHandler;
        private readonly GetStudentGenderDemographyDetailHandler _getStudentGenderDemographyDetailHandler;
        #endregion
        #region Total Student
        private readonly GetStudentDemographicsReportTotalStudentHandler _getStudentDemographicsReportTotalStudentHandler;
        private readonly GetStudentDemographicsReportTotalStudentDetailsHandler _getStudentDemographicsReportTotalStudentDetailsHandler;
        #endregion
        #region Religion
        private readonly GetStudentDemographicsReportReligionHandler _getStudentDemographicsReportReligionHandler;
        private readonly GetStudentDemographicsReportReligionDetailsHandler _getStudentDemographicsReportReligionDetailsHandler;
        #endregion
        #region Total Family
        private readonly GetStudentTotalFamilyDemographicsHandler _getStudentTotalFamilyDemographicsHandler;
        private readonly GetStudentTotalFamilyDemographicsDetailHandler _getStudentTotalFamilyDemographicsDetailHandler;
        #endregion
        private readonly GetStudentDemographyReportTypeHandler _getStudentDemographyReportTypeHandler;

        //Excel
        private readonly MasterStudentDemographicsGenerateExcelHandler _masterStudentDemographicsGenerateExcelHandler;
        private readonly StudentNationalityDemographicsExcelHandler _studentNationalityDemographicsExcelHandler;

        public StudentDemographicsReportEndPoint(
            #region Nationality
            GetStudentNationalityDemographyHandler getStudentNationalityDemographyHandler,
            GetStudentNationalityDemographyDetailHandler getStudentNationalityDemographyDetailHandler,
            #endregion
            #region Gender
            GetStudentGenderDemographyHandler getStudentGenderDemographyHandler,
            GetStudentGenderDemographyDetailHandler getStudentGenderDemographyDetailHandler,
            #endregion
            #region Total Student
            GetStudentDemographicsReportTotalStudentHandler getStudentDemographicsReportTotalStudentHandler,
            GetStudentDemographicsReportTotalStudentDetailsHandler getStudentDemographicsReportTotalStudentDetailsHandler,
            #endregion
            #region Religion
            GetStudentDemographicsReportReligionHandler getStudentDemographicsReportReligionHandler,
            GetStudentDemographicsReportReligionDetailsHandler getStudentDemographicsReportReligionDetailsHandler,
        #endregion
            #region Total Family
            GetStudentTotalFamilyDemographicsHandler getStudentTotalFamilyDemographicsHandler,
            GetStudentTotalFamilyDemographicsDetailHandler getStudentTotalFamilyDemographicsDetailHandler,
            #endregion
            GetStudentDemographyReportTypeHandler getStudentDemographyReportTypeHandler,

            //Excel
            MasterStudentDemographicsGenerateExcelHandler masterStudentDemographicsGenerateExcelHandler,
            StudentNationalityDemographicsExcelHandler studentNationalityDemographicsExcelHandler
            )
        {
            #region Nationality
            _getStudentNationalityDemographyHandler = getStudentNationalityDemographyHandler;
            _getStudentNationalityDemographyDetailHandler = getStudentNationalityDemographyDetailHandler;
            #endregion
            #region Gender
            _getStudentGenderDemographyHandler = getStudentGenderDemographyHandler;
            _getStudentGenderDemographyDetailHandler = getStudentGenderDemographyDetailHandler;
            #endregion
            #region Total Student
            _getStudentDemographicsReportTotalStudentHandler = getStudentDemographicsReportTotalStudentHandler;
            _getStudentDemographicsReportTotalStudentDetailsHandler = getStudentDemographicsReportTotalStudentDetailsHandler;
            #endregion
            #region Religion
            _getStudentDemographicsReportReligionHandler = getStudentDemographicsReportReligionHandler;
            _getStudentDemographicsReportReligionDetailsHandler = getStudentDemographicsReportReligionDetailsHandler;
            #endregion
            #region Total Family
            _getStudentTotalFamilyDemographicsHandler = getStudentTotalFamilyDemographicsHandler;
            _getStudentTotalFamilyDemographicsDetailHandler = getStudentTotalFamilyDemographicsDetailHandler;
            #endregion
            _getStudentDemographyReportTypeHandler = getStudentDemographyReportTypeHandler;

            //Excel
            _masterStudentDemographicsGenerateExcelHandler = masterStudentDemographicsGenerateExcelHandler;
            _studentNationalityDemographicsExcelHandler = studentNationalityDemographicsExcelHandler;
        }

        #region Nationality
        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentNationalityDemography))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Nationality Demography")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentNationalityDemographyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentNationalityDemographyResult>))]
        public Task<IActionResult> GetStudentNationalityDemography(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-nationality-demography")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentNationalityDemographyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentNationalityDemographyDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Nationality Demography Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentNationalityDemographyDetailRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentNationalityDemographyDetailResult))]
        public Task<IActionResult> GetStudentNationalityDemographyDetail(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-nationality-demography-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentNationalityDemographyDetailHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Gender
        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentGenderDemography))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Gender Demography")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentGenderDemographyRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentGenderDemographyResult>))]
        public Task<IActionResult> GetStudentGenderDemography(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-gender-demography")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentGenderDemographyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentGenderDemographyDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Gender Demography Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentGenderDemographyDetailRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentGenderDemographyDetailResult>))]
        public Task<IActionResult> GetStudentGenderDemographyDetail(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-gender-demography-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentGenderDemographyDetailHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Total Student
        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentDemographicsReportTotalStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Demographics Report Total Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetSDRTotalStudentReportsRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSDRTotalStudentReportsResult))]
        public Task<IActionResult> GetStudentDemographicsReportTotalStudent(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-demography-report-total")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentDemographicsReportTotalStudentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentDemographicsReportTotalDetailsStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Demographics Report Total Details Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetSDRTotalStudentReportDetailsRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSDRTotalStudentReportDetailsResult))]
        public Task<IActionResult> GetStudentDemographicsReportTotalDetailsStudent(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-demography-report-total-detail")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getStudentDemographicsReportTotalStudentDetailsHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Religion
        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentDemographicsReportReligion))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Demographics Report Religion")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetSDRReligionReportsRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSDRReligionReportsResult>))]
        public Task<IActionResult> GetStudentDemographicsReportReligion(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-demography-report-religion")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentDemographicsReportReligionHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentDemographicsReportReligionDetails))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Demographics Report Religion Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetSDRReligionReportDetailsRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetSDRReligionReportDetailsResult>))]
        public Task<IActionResult> GetStudentDemographicsReportReligionDetails(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-demography-report-religion-detail")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            return _getStudentDemographicsReportReligionDetailsHandler.Execute(req, cancellationToken);
        }
        #endregion

        #region Total Family
        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentTotalFamilyDemographics))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Total Family Demographics")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentTotalFamilyDemographicsRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentGenderDemographyDetailResult>))]
        public Task<IActionResult> GetStudentTotalFamilyDemographics(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-total-family-demographics")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentTotalFamilyDemographicsHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentTotalFamilyDemographicsDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Total Family Demographics Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentTotalFamilyDemographicsDetailRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentTotalFamilyDemographicsDetailResult>))]
        public Task<IActionResult> GetStudentTotalFamilyDemographicsDetail(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-student-total-family-demographics-detail")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentTotalFamilyDemographicsDetailHandler.Execute(req, cancellationToken);
        }

        #endregion

        [FunctionName(nameof(StudentDemographicsReportEndPoint.GetStudentDemographyReportType))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Demography Report Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.Return), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentDemographyReportTypeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentDemographyReportTypeResult>))]
        public Task<IActionResult> GetStudentDemographyReportType(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-student-demography-report-type")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _getStudentDemographyReportTypeHandler.Execute(req, cancellationToken);
        }

        #region Excel
        [FunctionName(nameof(StudentDemographicsReportEndPoint.MasterStudentDemographicsGenerateExcel))]
        [OpenApiOperation(tags: _tag, Summary = "Master Student Demographics Generate Excel")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(MasterStudentDemographicsGenerateExcelRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> MasterStudentDemographicsGenerateExcel(
          [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/master-student-demographics-generate-excel")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _masterStudentDemographicsGenerateExcelHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
