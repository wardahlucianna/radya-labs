using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.School.FnSchool.Metadata;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnSchool.Metadata
{
    public class MetadataEndpoint
    {
        private const string _route = "school/metadata";
        private const string _tag = "School Metadata";

        private readonly MetadataHandler _handler;

        public MetadataEndpoint(MetadataHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(MetadataEndpoint.GetMetadata))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMetadataRequest.Acadyears), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Levels), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Grades), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Terms), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Subjects), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Classrooms), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Departments), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Streamings), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Buildings), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Venues), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.Divisions), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(GetMetadataRequest.SubjectCombinations), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMetadataResult))]
        public Task<IActionResult> GetMetadata(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}
