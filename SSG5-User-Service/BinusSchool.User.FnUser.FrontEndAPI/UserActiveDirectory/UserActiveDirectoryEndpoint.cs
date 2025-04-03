using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.UserActiveDirectory;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.UserActiveDirectory
{
    public class UserActiveDirectoryEndpoint
    {
        private const string _route = "user-ad";
        private const string _tag = "User Active Directory";
        
        [FunctionName(nameof(GetUserActiveDirectory))]
        [OpenApiOperation(tags: _tag, Description = @"
            Pagination on this endpoint is different, there is no number of page information.
            System only accept nextPageToken to get next page values. The nextPageToken can be obtained from field properties.nextPageToken.
            If the nextPageToken value is null, then there is no next page to obtain the values.")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        // [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        // [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserActiveDirectoryRequest.NextPageToken), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserActiveDirectoryResult))]
        public Task<IActionResult> GetUserActiveDirectory(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetUserActiveDirectoryHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
