using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffCertificationInformation;
using BinusSchool.Employee.FnStaff.StaffCertificationInformation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Employee.FnStaff.StaffCertificationInformation
{
    public class StaffCertificationInformationEndPoint
    {
        private const string _route = "employee/staff-certification-information";
        private const string _tag = "Staff Certification Information";

        private readonly StaffCertificationInformationHandler _handler;
        private readonly DeleteStaffCertificationInformationHandler _deleteStaffCertificationInformationHandler;
        public StaffCertificationInformationEndPoint(StaffCertificationInformationHandler staffCertificationInformationHandler,
            DeleteStaffCertificationInformationHandler deleteStaffCertificationInformationHandler
        )
        {
            _handler = staffCertificationInformationHandler;
            _deleteStaffCertificationInformationHandler = deleteStaffCertificationInformationHandler;
        }

        [FunctionName(nameof(StaffCertificationInformationEndPoint.GetStaffCertificationInformations))]
        [OpenApiOperation(tags: _tag, Summary = "Get Staff Certification Information List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetStaffCertificationInformationRequest.IdBinusian), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStaffCertificationInformationResult))]
        public Task<IActionResult> GetStaffCertificationInformations(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        /*[FunctionName(nameof(StaffCertificationInformationEndPoint.GetStaffCertificationInformationDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Staff Certification Information Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        //[OpenApiParameter(nameof(CollectionSchoolRequest.Ids), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStaffCertificationInformationResult))]
        public Task<IActionResult> GetStaffCertificationInformationDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }*/

        [FunctionName(nameof(StaffCertificationInformationEndPoint.AddStaffCertificationInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Add Staff Certification Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddStaffCertificationInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddStaffCertificationInformation(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StaffCertificationInformationEndPoint.UpdateStaffCertificationInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Staff Certification Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStaffCertificationInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStaffCertificationInformation(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StaffCertificationInformationEndPoint.DeleteStaffCertificationInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Staff Certification Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(DeleteStaffCertificationInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteStaffCertificationInformation(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deleteStaffCertificationInformationHandler.Execute(req, cancellationToken);
        }
    }
}
