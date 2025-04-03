using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.PreviousSchoolNew;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model;

namespace BinusSchool.Student.FnStudent.PreviousSchoolNew
{
    public class PreviousSchoolNewEndPoint
    {
        private const string _route = "student/previous-school-new";
        private const string _tag = "Previous School";

        private readonly PreviousSchoolNewHandler _previousSchoolNewHandler;
        public PreviousSchoolNewEndPoint(PreviousSchoolNewHandler previousSchoolNewHandler)
        {
            _previousSchoolNewHandler = previousSchoolNewHandler;
        }

        [FunctionName(nameof(PreviousSchoolNewEndPoint.GetPreviousSchoolNew))]
        [OpenApiOperation(tags: _tag, Summary = "Get Previous School New List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetPreviousSchoolNewRequest.IdPreviousSchoolNew), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetPreviousSchoolNewResult))]
        public Task<IActionResult> GetPreviousSchoolNew(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _previousSchoolNewHandler.Execute(req, cancellationToken);
        }
    }
}
