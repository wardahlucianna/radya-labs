using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Student.FnStudent.Student;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.Student
{
    public class StudentEndPoint
    {
        private const string _route = "student/student";
        private const string _tag = "Student";

        private readonly StudentHandler _studentHandler;
        private readonly GetStudentBySiblingGroupHandler _siblingGroupHandler;
        private readonly UpdateInternationalStudentFormalitiesHandler _internationalStudentFormalitiesHandler;
        private readonly UpdateStudentAddressInformationHandler _studentAddressInformationHandler;
        private readonly UpdateStudentContactInformationHandler _studentContactInformationHandler;
        private readonly UpdateStudentOtherInformationHandler _studentOtherInformationHandler;
        private readonly UpdateStudentPersonalInformationHandler _studentPersonalInformationHandler;
        private readonly GetStudentByIdParentHandler _getstudentbyidparenthandler;
        private readonly GetStudentInformationChangesHistoryHandler _getStudentInformationChangesHistoryHandler;
        private readonly GetAllStudentWithStatusAndHomeroomHandler _getAllStudentWithStatusAndHomeroomHandler;
        private readonly GetAllStudentEnrollmentHandler _getAllStudentEnrollementHandler;
        private readonly GetStudentDetailEncryptedHandler _getStudentDetailEncryptedHandler;
        private readonly GetStudentBySiblingGroupEncryptedHandler _getStudentBySiblingGroupEncryptedHandler;

        private readonly GetStudentInformationForBNSReportHandler _getStudentInformationForBNSReportHandler;
        private readonly GetStudentInformationWithStudentStatusHandler _getStudentInformationWithStudentStatusHandler;
        private readonly GetStudentMultipleGradeHandler _getStudentMultipleGradeHandler;

        private readonly GetStudentForSalesForceHandler _getStudentForSalesForceHandler;

        public StudentEndPoint(StudentHandler studentHandler,
        GetStudentBySiblingGroupHandler siblingGroupHandler,
        UpdateInternationalStudentFormalitiesHandler internationalStudentFormalitiesHandler,
        UpdateStudentAddressInformationHandler studentAddressInformationHandler,
        UpdateStudentContactInformationHandler studentContactInformationHandler,
        UpdateStudentOtherInformationHandler studentOtherInformationHandler,
        UpdateStudentPersonalInformationHandler studentPersonalInformationHandler,
        GetStudentByIdParentHandler getstudentbyidparenthandler,
        GetStudentInformationChangesHistoryHandler getStudentInformationChangesHistoryHandler,
        GetAllStudentWithStatusAndHomeroomHandler getAllStudentWithStatusAndHomeroomHandler,
        GetAllStudentEnrollmentHandler getAllStudentEnrollementHandler,
        GetStudentDetailEncryptedHandler getStudentDetailEncryptedHandler,
        GetStudentBySiblingGroupEncryptedHandler getStudentBySiblingGroupEncryptedHandler,
        GetStudentInformationForBNSReportHandler getStudentInformationForBNSReportHandler,
        GetStudentInformationWithStudentStatusHandler getStudentInformationWithStudentStatusHandler,
        GetStudentMultipleGradeHandler getStudentMultipleGradeHandler,
        GetStudentForSalesForceHandler getStudentForSalesForceHandler
        )
        {
            _studentHandler = studentHandler;
            _siblingGroupHandler = siblingGroupHandler;
            _internationalStudentFormalitiesHandler = internationalStudentFormalitiesHandler;
            _studentAddressInformationHandler = studentAddressInformationHandler;
            _studentContactInformationHandler = studentContactInformationHandler;
            _studentOtherInformationHandler = studentOtherInformationHandler;
            _studentPersonalInformationHandler = studentPersonalInformationHandler;
            _getstudentbyidparenthandler = getstudentbyidparenthandler;
            _getStudentInformationChangesHistoryHandler = getStudentInformationChangesHistoryHandler;
            _getAllStudentWithStatusAndHomeroomHandler = getAllStudentWithStatusAndHomeroomHandler;
            _getAllStudentEnrollementHandler = getAllStudentEnrollementHandler;
            _getStudentDetailEncryptedHandler = getStudentDetailEncryptedHandler;
            _getStudentBySiblingGroupEncryptedHandler = getStudentBySiblingGroupEncryptedHandler;
            _getStudentInformationForBNSReportHandler = getStudentInformationForBNSReportHandler;
            _getStudentInformationWithStudentStatusHandler = getStudentInformationWithStudentStatusHandler;
            _getStudentMultipleGradeHandler = getStudentMultipleGradeHandler;
            _getStudentForSalesForceHandler = getStudentForSalesForceHandler;
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentsByGrade))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Description = "Filter IdStudent")]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentByGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentByGradeRequest.IncludePathway), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentByGradeRequest.ExceptIds), In = ParameterLocation.Query, Type = typeof(string[]), Description = "Filter except IdStudent")]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(CodeWithIdVm))]
        public Task<IActionResult> GetStudentsByGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-by-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<StudentByGradeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentMapByGrade))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Description = "Filter Student Map by Grade")]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentMapByGradeRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentMapByGradeRequest.AcademicYear), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMapByGradeRequest.Gender), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMapByGradeRequest.IdGrade), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMapByGradeRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentMapByGradeResult))]
        public Task<IActionResult> GetStudentMapByGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-map-by-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentMapByGradeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentCopyByGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get students that hadn't mapped by grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]), Description = "Filter Student Map by Grade")]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStudentCopyByGradeRequest.IdAcademicYearSource), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentCopyByGradeRequest.IdAcademicYearTarget), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentCopyByGradeRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentCopyByGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentCopyByGradeResult))]
        public Task<IActionResult> GetStudentCopyByGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-copy-by-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentCopyByGradeHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentForAsc))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student List For Upload XMl ")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentUploadAscRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentUploadAscResult))]
        public Task<IActionResult> GetStudentForAsc(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-by-binusianid")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentForUploadASCHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentResult))]
        public Task<IActionResult> GetStudents(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_information")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentDetailResult))]
        public Task<IActionResult> GetStudentDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_information/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _studentHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(StudentEndPoint.AddStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStudent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "_information")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _studentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.UpdateStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudent(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "_information")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _studentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.DeleteStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteStudent(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "_information")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _studentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentBySiblingGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student By Sibling Group")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentRequest.ForApproval), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentRequest))]
        public Task<IActionResult> GetStudentBySiblingGroup(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "student/student-by-siblinggroup")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _siblingGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.UpdateStudentPersonalInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Personal Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentPersonalInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentPersonalInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Student-Personal-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _studentPersonalInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.UpdateInternationalStudentFormalities))]
        [OpenApiOperation(tags: _tag, Summary = "Update International Student Formalities")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateInternationalStudentFormalitiesRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateInternationalStudentFormalities(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-International-Student-Formalities")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _internationalStudentFormalitiesHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.UpdateStudentAddressInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Address Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentAddressInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentAddressInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Student-Address-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _studentAddressInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.UpdateStudentContactInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Contact Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentContactInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentContactInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Student-Contact-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _studentContactInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.UpdateStudentOtherInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Student Other Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStudentOtherInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStudentOtherInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Student-Other-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _studentOtherInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentByIdParent))]
        [OpenApiOperation(tags: _tag, Summary = "Get StudentList By IDParent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentByIdParentRequest.IdParent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentByIdParentRequest.IdStudentStatusExcludeList), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetStudentByIdParentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentByIdParentResult>))]
        public Task<IActionResult> GetStudentByIdParent(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = "student/studentlist-by-idparent")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            return _getstudentbyidparenthandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetUnmapStudentToUser))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student that Unmapped to User")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUnmapStudentHandler))]
        public Task<IActionResult> GetUnmapStudentToUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/unmap-to-user")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUnmapStudentHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.IdLevel), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.IdHomeroom), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.Semester), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentHomeroomRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentHomeroomResult[]))]
        public Task<IActionResult> GetStudentHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/homeroom-student")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetStudentHomeroomHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentInformationChangesHistory))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Information Changes History")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentInformationChangesHistoryRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetStudentInformationChangesHistoryResult>))]
        public Task<IActionResult> GetStudentInformationChangesHistory(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-information-changes-history")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentInformationChangesHistoryHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetAllStudentWithStatusAndHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Student With Status and Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentWithStatusAndHomeroomRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetAllStudentWithStatusAndHomeroomResult>))]
        public Task<IActionResult> GetAllStudentWithStatusAndHomeroom(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-all-with-status-homeroom")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getAllStudentWithStatusAndHomeroomHandler.Execute(req, cancellationToken);
        }


        [FunctionName(nameof(StudentEndPoint.GetAllStudentEnrollment))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Student Enrolment with Grade n Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetAllStudentEnrollmentResult>))]
        public Task<IActionResult> GetAllStudentEnrollment(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-all-enrollment")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getAllStudentEnrollementHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetAllStudentEnrollmentNoIdHomeroom))]
        [OpenApiOperation(tags: _tag, Summary = "Get All Student Enrolment with Grade n Homeroom")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.IdLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetAllStudentEnrollmentRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetAllStudentEnrollmentResult>))]
        public Task<IActionResult> GetAllStudentEnrollmentNoIdHomeroom(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-all-enrollment-no-id-homeroom")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAllStudentEnrollmentNoIdHomeroomHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentDetailEncrypted))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Detail Encrypted")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentDetailRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentDetailResult))]
        public Task<IActionResult> GetStudentDetailEncrypted(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_information-encrypted")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getStudentDetailEncryptedHandler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentBySiblingGroupEncrypted))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student By Sibling Group Encrypted")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentRequest.ForApproval), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentRequest))]
        public Task<IActionResult> GetStudentBySiblingGroupEncrypted(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "student/student-by-siblinggroup-encrypted")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getStudentBySiblingGroupEncryptedHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentInformationWithStudentStatus))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Information With Student Status")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentInformationWithStudentStatusRequest.IdStudentEncrypt), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetStudentInformationWithStudentStatusRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentInformationWithStudentStatusResult))]
        public Task<IActionResult> GetStudentInformationWithStudentStatus(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_information-student-status")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getStudentInformationWithStudentStatusHandler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(StudentEndPoint.GetStudentInformationForBNSReport))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Information For BNSReport")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentInformationForBNSReportRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentInformationForBNSReportResult))]
        public Task<IActionResult> GetStudentInformationForBNSReport(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-information-for-bnsreport")] HttpRequest req,
        CancellationToken cancellationToken)
        {
            return _getStudentInformationForBNSReportHandler.Execute(req, cancellationToken, true);
        }

        [FunctionName(nameof(GetStudentMultipleGrade))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student Multiple Grade")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.IdAcadYear), In = ParameterLocation.Query, Type = typeof(string), Required = true)]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.IdGrades), In = ParameterLocation.Query, Type = typeof(string[]), Required = true, Description = "Filter IdGrades")]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetStudentMultipleGradeRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentHomeroomResult[]))]
        public Task<IActionResult> GetStudentMultipleGrade(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-multiple-grade")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentMultipleGradeHandler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(GetStudentForSalesForce))]
        [OpenApiOperation(tags: _tag, Summary = "Get Student For SalesForce")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetStudentForSalesForceRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStudentForSalesForceResult))]
        public Task<IActionResult> GetStudentForSalesForce(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-for-salesforce")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getStudentForSalesForceHandler.Execute(req, cancellationToken);
        }
    }
}
