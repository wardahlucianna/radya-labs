using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffInfoUpdate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.School.FnSchool.ClassRoomMapping;

namespace BinusSchool.Employee.FnStaff.StaffInfoUpdate
{
    public class StaffInfoUpdateEndPoint
    {
        private const string _route = "employee/";
        private const string _tag = "Staff Info Update";

        private readonly StaffInfoUpdateHandler _staffInfoUpdateHandler;
        private readonly UpdateStaffCertificationHandler _updateStaffCertificationHandler;
        private readonly UpdateStaffEducationHandler _updateStaffEducationHandler;
        private readonly UpdateStaffHandler _updateStaffHandler;

        public StaffInfoUpdateEndPoint(StaffInfoUpdateHandler staffInfoUpdateHandler,
        UpdateStaffCertificationHandler updateStaffCertificationHandler,
        UpdateStaffEducationHandler updateStaffEducationHandler,
        UpdateStaffHandler updateStaffHandler
        )
        {
            _staffInfoUpdateHandler = staffInfoUpdateHandler;
            _updateStaffCertificationHandler = updateStaffCertificationHandler;
            _updateStaffEducationHandler = updateStaffEducationHandler;
            _updateStaffHandler = updateStaffHandler;
        }

        /*[FunctionName(nameof(StaffInfoUpdateEndPoint.GetStaffInfoUpdates))]
        [OpenApiOperation(tags: _tag, Summary = "Get Staff Info Updates List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetStaffInfoUpdateRequest.IdBinusian), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetStaffInfoUpdateResult))]
        public Task<IActionResult> GetStaffInfoUpdates(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "staff-info-update")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _staffInfoUpdateHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StaffInfoUpdateEndPoint.UpdateStaffCertification))]
        [OpenApiOperation(tags: _tag, Summary = "Update Staff Certification")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStaffInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStaffCertification(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "update-staff-certification")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateStaffCertificationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StaffInfoUpdateEndPoint.UpdateStaffEducation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Staff Education")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStaffInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStaffEducation(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "update-staff-education")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateStaffEducationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(StaffInfoUpdateEndPoint.UpdateStaffJobInfo))]
        [OpenApiOperation(tags: _tag, Summary = "Update Staff Job Info")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<UpdateStaffInfoUpdate>))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStaffJobInfo(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "update-staff-job-info")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _updateStaffHandler.Execute(req, cancellationToken);
        }
        */
    }
}
