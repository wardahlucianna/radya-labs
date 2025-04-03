using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.ParentRole;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Student.FnStudent.Parent;
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
    public class ParentEndPoint
    {
        private const string _route = "student/parent_information";
        private const string _tag = "Parent";

        private readonly ParentHandler _parentHandler;
        private readonly GetFamilyByStudentHandler _getFamilyHandler;
        private readonly UpdateParentAddressInformationHandler _parentAddressInformationHandler;
        private readonly UpdateParentContactInformationHandler _parentContactInformationHandler;
        private readonly UpdateParentOccupationInformationHandler _parentOccupationInformationHandler;
        private readonly UpdateParentPersonalInformationHandler _parentPersonalInformationHandler;
        private readonly GetChildrensHandler _getChildrensHandler;
        private readonly GetParentGenerateAccountHandler _getparentgenerateaccounthandler;
        private readonly GetParentDetailEnryptedHandler _getParentDetailEncryptedHandler;
        public ParentEndPoint(ParentHandler parentHandler,
            GetFamilyByStudentHandler getFamilyHandler,
            UpdateParentAddressInformationHandler parentAddressInformationHandler,
            UpdateParentContactInformationHandler parentContactInformationHandler,
            UpdateParentOccupationInformationHandler parentOccupationInformationHandler,
            UpdateParentPersonalInformationHandler parentPersonalInformationHandler,
            GetChildrensHandler getChildrensHandler,
            GetParentGenerateAccountHandler getparentgenerateaccounthandler,
            GetParentDetailEnryptedHandler getParentDetailEncryptedHandler)
        {
            _parentHandler = parentHandler;
            _getFamilyHandler = getFamilyHandler;
            _parentAddressInformationHandler = parentAddressInformationHandler;
            _parentContactInformationHandler = parentContactInformationHandler;
            _parentOccupationInformationHandler = parentOccupationInformationHandler;
            _parentPersonalInformationHandler = parentPersonalInformationHandler;
            _getChildrensHandler = getChildrensHandler;
            _getparentgenerateaccounthandler = getparentgenerateaccounthandler;
            _getParentDetailEncryptedHandler = getParentDetailEncryptedHandler;
        }

        [FunctionName(nameof(ParentEndPoint.GetParents))]
        [OpenApiOperation(tags: _tag, Summary = "Get Parent List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetParentRequest.IdParentRole), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdReligion), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdLastEducationLevel), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdNationality), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdCountry), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdAddressCity), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdAddressStateProvince), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdAddressCountry), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdOccupationType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetParentRequest.IdParentSalaryGroup), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetParentResult))]
        public Task<IActionResult> GetParents(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _parentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.GetParentDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Parent Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        //[OpenApiParameter(nameof(CollectionSchoolRequest.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetParentDetailResult))]
        public Task<IActionResult> GetParentDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _parentHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(ParentEndPoint.AddParent))]
        [OpenApiOperation(tags: _tag, Summary = "Add Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddParent(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _parentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.UpdateParent))]
        [OpenApiOperation(tags: _tag, Summary = "Update Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateParentRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateParent(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _parentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.DeleteParent))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Parent")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteParent(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _parentHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.GetFamilyByStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Get Family Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiParameter("id", Required = true)]
        [OpenApiParameter(nameof(GetStudentRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        //[OpenApiParameter(nameof(GetStudentByGradeRequest.IdGrade), In = ParameterLocation.Query, Required = true)]
        //[OpenApiParameter("IdStudent", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetParentRoleResult))]
        public Task<IActionResult> GetFamilyByStudent(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = "student/family-information")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getFamilyHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.UpdateParentPersonalInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Parent Personal Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateParentPersonalInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateParentPersonalInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Parent-Personal-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _parentPersonalInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.UpdateParentOccupationInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Parent Occupation Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateParentOccupationInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateParentOccupationInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Parent-Occupation-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _parentOccupationInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.UpdateParentAddressInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Parent Address Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateParentAddressInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateParentAddressInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Parent-Address-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _parentAddressInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.UpdateParentContactInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Parent Contact Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateParentContactInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateParentContactInformation(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/Update-Parent-Contact-Information")] HttpRequest req, CancellationToken cancellationToken)
        {
            return _parentContactInformationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.GetChildrens))]
        [OpenApiOperation(tags: _tag, Summary = "Get Childrens")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetChildRequest.IdParent), In = ParameterLocation.Query,Required =true)]
        [OpenApiParameter(nameof(GetChildRequest.IdAcademicYear), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetChildResult>))]
        public Task<IActionResult> GetChildrens(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_childrens")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getChildrensHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.GetParentGenerateAccount))]
        [OpenApiOperation(tags: _tag, Summary = "Get parent info for generate Account")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetParentGenerateAccountRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetParentGenerateAccountResult))]
        public Task<IActionResult> GetParentGenerateAccount(
         [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_for_generate")] HttpRequest req,
         CancellationToken cancellationToken)
        {
            return _getparentgenerateaccounthandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(ParentEndPoint.GetParentDetailEncrypted))]
        [OpenApiOperation(tags: _tag, Summary = "Get Parent Detail Encrypted")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetParentDetailRequest.IdParent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetParentDetailResult))]
        public Task<IActionResult> GetParentDetailEncrypted(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "_encrypted")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _getParentDetailEncryptedHandler.Execute(req, cancellationToken, true);
        }
    }
}
