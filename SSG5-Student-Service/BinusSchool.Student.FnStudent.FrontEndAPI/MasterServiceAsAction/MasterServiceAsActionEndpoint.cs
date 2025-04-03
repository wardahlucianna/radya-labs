using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Azure.WebJobs;
using Microsoft.OpenApi.Models;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction;
using BinusSchool.Student.FnStudent.MasterServiceAsAction.Status;
using BinusSchool.Common.Model;
using BinusSchool.Student.FnStudent.MasterServiceAsAction.Location;
using BinusSchool.Student.FnStudent.MasterServiceAsAction.Type;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Type;
using BinusSchool.Data.Model.Student.FnStudent.MasterServiceAsAction.Status;
using BinusSchool.Student.BLL.FnStudent.MasterServiceAsAction.Sdg;

namespace BinusSchool.Student.FnStudent.MasterServiceAsAction
{
    public class MasterServiceAsActionEndpoint
    {
        private const string _route = "student/master-service-as-action";
        private const string _tag = "Master Service As Action";

        private readonly GetListServiceAsActionStatusHandler _getListExperienceStatusHandler;

        private readonly GetListServiceAsActionLocationHandler _getListExperienceLocationHandler;

        private readonly GetListServiceAsActionTypeHandler _getListExperienceTypeHandler;

        private readonly GetListServiceAsActionSdgHandler _getListServiceAsActionSdgHandler;

        public MasterServiceAsActionEndpoint
        (
            GetListServiceAsActionStatusHandler getListExperienceStatusHandler,

            GetListServiceAsActionLocationHandler getListExperienceLocationHandler,

            GetListServiceAsActionTypeHandler getListExperienceTypeHandler,

            GetListServiceAsActionSdgHandler getListServiceAsActionSdgHandler
        )
        {
            _getListExperienceStatusHandler = getListExperienceStatusHandler;

            _getListExperienceLocationHandler = getListExperienceLocationHandler;

            _getListExperienceTypeHandler = getListExperienceTypeHandler;

            _getListServiceAsActionSdgHandler = getListServiceAsActionSdgHandler;
        }
        #region Status
        [FunctionName(nameof(MasterServiceAsActionEndpoint.GetListExperienceStatusTemporary))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Service As Action Status Temporary")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListServiceAsActionStatusRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<GetListServiceAsActionStatusResult>))]
        public Task<IActionResult> GetListExperienceStatusTemporary(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-service-as-action-status-temporary")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _getListExperienceStatusHandler.Execute(req, cancellationToken);
        }

        #endregion

        #region Location

        [FunctionName(nameof(MasterServiceAsActionEndpoint.GetListExperienceLocation))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Service As Action Location")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListServiceAsActionLocationRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetListExperienceLocation(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-service-as-action-location")] HttpRequest req,
                                  CancellationToken cancellationToken)
        {
            return _getListExperienceLocationHandler.Execute(req, cancellationToken);
        }

        #endregion

        #region Type

        [FunctionName(nameof(MasterServiceAsActionEndpoint.GetListExperienceType))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Service As Action Type")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetListExperienceTypeRequest.IdAcademicYear), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetListExperienceType(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-service-as-action-type")] HttpRequest req,
                                  CancellationToken cancellationToken)
        {
            return _getListExperienceTypeHandler.Execute(req, cancellationToken);
        }

        #endregion

        #region Sdg

        [FunctionName(nameof(MasterServiceAsActionEndpoint.GetListServiceAsActionSdg))]
        [OpenApiOperation(tags: _tag, Summary = "Get List Service As Action SDG")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(IEnumerable<ItemValueVm>))]
        public Task<IActionResult> GetListServiceAsActionSdg(
                       [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-list-service-as-action-sdg")] HttpRequest req,
                                  CancellationToken cancellationToken)
        {
            return _getListServiceAsActionSdgHandler.Execute(req, cancellationToken);
        }
        #endregion
    }
}
