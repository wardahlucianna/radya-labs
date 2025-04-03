using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;


namespace BinusSchool.Attendance.FnAttendance.File
{
    public class FileEndpoint
    {
        private const string _route = "attendance/file";
        private const string _tag = "Attendance File";

        [FunctionName(nameof(FileEndpoint.DeleteFile))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(FileRequest))]
        [OpenApiResponseWithoutBody(System.Net.HttpStatusCode.OK)]
        public Task<IActionResult> DeleteFile(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route + "/delete")] HttpRequest req,
        ExecutionContext context,
        System.Threading.CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<DeleteFileHandler>();
            return handler.Execute(req, cancellationToken, keyValues: nameof(ExecutionContext).WithValue(context));
        }
    }
}
