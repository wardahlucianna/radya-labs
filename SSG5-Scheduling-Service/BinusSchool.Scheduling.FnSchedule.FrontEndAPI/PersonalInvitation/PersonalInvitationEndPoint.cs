using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.PersonalInvitation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnSchedule.PersonalInvitation
{
    public class PersonalInvitationEndPoint
    {
        private const string _route = "schedule/personal-invitation";
        private const string _tag = "Personal Invitation";

        private readonly PersonalInvitationHandler _personalInvitationHandler;
        private readonly PersonalInvitationApprovalHandler _personalInvitationApprovalHandler;
        private readonly CancelPersonalInviationHandler _cancelPersonalInviationHandler;
        public PersonalInvitationEndPoint(PersonalInvitationHandler PersonalInvitationHandler, PersonalInvitationApprovalHandler PersonalInvitationApprovalHandler, CancelPersonalInviationHandler CancelPersonalInviationHandler)
        {
            _personalInvitationHandler = PersonalInvitationHandler;
            _personalInvitationApprovalHandler = PersonalInvitationApprovalHandler;
            _cancelPersonalInviationHandler = CancelPersonalInviationHandler;
        }

        #region personal invitation
        [FunctionName(nameof(PersonalInvitationEndPoint.GetPersonalInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Get Personal Invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.DateInvitation), In = ParameterLocation.Query, Type = typeof(DateTime))]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.TypeInvitation), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPersonalInvitationRequest.Status), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPersonalInvitationRequest))]
        public Task<IActionResult> GetPersonalInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _personalInvitationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.AddPersonalInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Add Personal Invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddPersonalInvitationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddPersonalInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _personalInvitationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.AddPersonalInvitationV2))]
        [OpenApiOperation(tags: _tag, Summary = "Add Personal Invitation V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddPersonalInvitationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> AddPersonalInvitationV2(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route +"-v2")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddPersonalInvitationV2Handler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.DetailPersonalInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Detail Personal Invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(DetailPersonalInvitationResult))]
        public Task<IActionResult> DetailPersonalInvitation(
       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req, string id,
          CancellationToken cancellationToken)
        {
            return _personalInvitationHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.UpdatePersonalInvitation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Personal Invitation")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePersonalInvitationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdatePersonalInvitation(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _personalInvitationHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.GetAvailabilityTimeTeacher))]
        [OpenApiOperation(tags: _tag, Summary = "Get Availability Time Teacher")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAvailabilityTimeTeacherRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailabilityTimeTeacherRequest.DateInvitation), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<string>))]
        public Task<IActionResult> GetAvailabilityTimeTeacher(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-availability-time")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAvailabilityTimeTeacherHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.GetAvailabilityTimeTeacherV2))]
        [OpenApiOperation(tags: _tag, Summary = "Get Availability Time Teacher V2")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetAvailabilityTimeTeacherRequest.IdUserTeacher), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailabilityTimeTeacherRequest.DateInvitation), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetAvailabilityTimeTeacherRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<string>))]
        public Task<IActionResult> GetAvailabilityTimeTeacherV2(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "-availability-time-v2")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetAvailabilityTimeTeacherV2Handler>();
            return handler.Execute(req, cancellationToken);
        }
        #endregion

        #region personal invitation approval
        [FunctionName(nameof(PersonalInvitationEndPoint.GetPersonalInvitationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Get Personal Invitation Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPersonalInvitationApprovalRequest.IdUser), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPersonalInvitationApprovalRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPersonalInvitationApprovalRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetPersonalInvitationApprovalRequest.Role), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetPersonalInvitationApprovalRequest.DateInvitation), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPersonalInvitationApprovalResult))]
        public Task<IActionResult> GetPersonalInvitationApproval(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route+"-approval")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _personalInvitationApprovalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.UpdatePersonalInvitationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Update Personal Invitation Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdatePersonalInvitationApprovalRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> UpdatePersonalInvitationApproval(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "-approval")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _personalInvitationApprovalHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(PersonalInvitationEndPoint.CancelPersonalInvitationApproval))]
        [OpenApiOperation(tags: _tag, Summary = "Cancel Personal Invitation Approval")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(CancelPersonalInvitationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> CancelPersonalInvitationApproval(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "-cancel")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _cancelPersonalInviationHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
