using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Model.Student.FnStudent.MoveStudentPathway;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.MoveStudentPahway
{
    public class MoveStudentPathwayEndpoint
    {
        private const string _route = "student/move-pathway";
        private const string _tag = "Move Student Pathway";

        private readonly MoveStudentPathwayHandler _handler;

        public MoveStudentPathwayEndpoint(MoveStudentPathwayHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(MoveStudentPathwayEndpoint.AddMoveStudent))]
        [OpenApiOperation(tags: _tag, Summary = "Move Student Pathway")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMoveStudentPathwayRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMoveStudent(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}
