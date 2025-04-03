using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Student.FnStudent.SiblingGroup;
using BinusSchool.Student.FnStudent.SiblingGroup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.SiblingGroup
{
    public class SiblingGroupEndPoint
    {
        private const string _route = "student/sibling_group";
        private const string _tag = "Sibling Group";

        private readonly SiblingGroupHandler _siblingGroupHandler;
        private readonly DeleteSiblingGroupHandler _deleteSiblingGroupHandler;
        private readonly AddSiblingGroupHandler _addSiblingGroupHandler;
        public SiblingGroupEndPoint(SiblingGroupHandler siblingGroupHandler,
            DeleteSiblingGroupHandler deleteSiblingGroupHandler,
            AddSiblingGroupHandler AddSiblingGroupHandler
            )
        {
            _siblingGroupHandler = siblingGroupHandler;
            _deleteSiblingGroupHandler = deleteSiblingGroupHandler;
            _addSiblingGroupHandler = AddSiblingGroupHandler;
        }

        [FunctionName(nameof(SiblingGroupEndPoint.GetSiblingGroups))]
        [OpenApiOperation(tags: _tag, Summary = "Get Sibling Group List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionSchoolRequest.IdSchool), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSiblingGroupResult))]
        public Task<IActionResult> GetSiblingGroups(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _siblingGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SiblingGroupEndPoint.GetSiblingGroupDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get SiblingGroup Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        //[OpenApiParameter(nameof(GetSiblingGroupRequest.IdStudent), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetSiblingGroupDetailResult))]
        public Task<IActionResult> GetSiblingGroupDetail(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
           string id,
           CancellationToken cancellationToken)
        {
            return _siblingGroupHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        }

        [FunctionName(nameof(SiblingGroupEndPoint.AddSiblingGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Add Student In SiblingGroup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSiblingGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddSiblingGroup(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _addSiblingGroupHandler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(SiblingGroupEndPoint.DeleteSiblingGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Student In SiblingGroup")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddSiblingGroupRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> DeleteSiblingGroup(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _deleteSiblingGroupHandler.Execute(req, cancellationToken);
        }
    }
}
