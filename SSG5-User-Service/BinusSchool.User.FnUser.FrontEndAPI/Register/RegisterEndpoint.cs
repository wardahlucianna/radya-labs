using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Data.Model.User.FnUser.Register;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.Register
{
    public class RegisterEndpoint
    {
        private const string _route = "register";
        private const string _tag = "Register";

        [FunctionName(nameof(RegisterEndpoint.RegisterPushToken))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(RegisterPushTokenRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> RegisterPushToken(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/push-token")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<RegisterPushTokenHandler>();
            return handler.Execute(req, cancellationToken);
        }

        //[FunctionName(nameof(RegisterEndpoint.GetFirebaseToken))]
        //[OpenApiOperation(tags: _tag, Summary = "Get Firebase Token")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(GetFirebaseTokenRequest))]
        //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<string>))]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        //public Task<IActionResult> GetFirebaseToken(
        //    [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/get-firebase-token")] HttpRequest req,
        //    CancellationToken cancellationToken)
        //{
        //    var handler = req.HttpContext.RequestServices.GetService<GetFirebaseTokenHandler>();
        //    return handler.Execute(req, cancellationToken);
        //}

        [FunctionName(nameof(RegisterEndpoint.GetFirebaseToken))]
        [OpenApiOperation(tags: _tag, Summary = "Get Firebase Token")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(GetFirebaseTokenRequest))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(List<GetFirebaseTokenResult>))]
        public Task<IActionResult> GetFirebaseToken(
       [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-firebase-token")] HttpRequest req,
       CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetFirebaseTokenHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
