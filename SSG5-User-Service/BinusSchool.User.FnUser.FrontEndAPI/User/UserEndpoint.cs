using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnUser.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.User.FnUser.User
{
    public class UserEndpoint
    {
        private const string _route = "user";
        private const string _tag = "User";

        private readonly UserHandler _handler;

        public UserEndpoint(UserHandler handler)
        {
            _handler = handler;
        }

        [FunctionName(nameof(UserEndpoint.GetUsersByRoleAndSchool))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserBySchoolAndRoleRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserBySchoolAndRoleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserResult))]
        public Task<IActionResult> GetUsersByRoleAndSchool(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user-by-school-role")] HttpRequest req,
           CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UserBySchoolAndRoleHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserEndpoint.GetUsersByRoleAndSchoolWithutValidateStaff))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetUserBySchoolAndRoleRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserBySchoolAndRoleRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserResult))]
        public Task<IActionResult> GetUsersByRoleAndSchoolWithutValidateStaff(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "user-by-school-role-without-validate-staff")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<UserBySchoolAndRoleWithoutValidateStaffHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserEndpoint.GenerateUsername))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GenerateUsernameRequest.IdRole), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GenerateUsernameRequest.BinusianIds), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(string))]
        public Task<IActionResult> GenerateUsername(
          [HttpTrigger(AuthorizationLevel.Function, "get", Route = "generate-username")] HttpRequest req,
          CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GenerateUsernameHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserEndpoint.GetUsers))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Ids), In = ParameterLocation.Query, Type = typeof(string[]))]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetUserRequest.IdSchool), In = ParameterLocation.Query, Required = true)]
        [OpenApiParameter(nameof(GetUserRequest.IdRole), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ListUserResult))]
        public Task<IActionResult> GetUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UserEndpoint.GetUserDetail))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetUserDetailResult))]
        public Task<IActionResult> GetUserDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, false, "id".WithValue(id));
        }

        [FunctionName(nameof(UserEndpoint.AddUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUserRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUser(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            [Queue("notification-usr-user")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken, keyValues: nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(UserEndpoint.UpdateUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateUserRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateUser(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserEndpoint.SetStatusUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(SetStatusUserRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetStatusUser(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/set-status")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetStatusUserHandler>();
            return handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserEndpoint.DeleteUser))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteUser(
            [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }

        [FunctionName(nameof(UserEndpoint.ResetPassword))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("idUser", In = ParameterLocation.Path, Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> ResetPassword(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/reset-password/{idUser}")] HttpRequest req,
            [Queue("notification-usr-user")] ICollector<string> collector,
            string idUser,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ResetPasswordHandler>();
            return handler.Execute(req, cancellationToken, true, "idUser".WithValue(idUser), "collector".WithValue(collector));
        }

        [FunctionName(nameof(UserEndpoint.ChangePassword))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ChangePasswordRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ChangePassword(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/change-password")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ChangePasswordHandler>();
            return handler.Execute(req, cancellationToken, false);
        }


        [FunctionName(nameof(UserEndpoint.ChangeUserPassword))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ChangeUserPasswordRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ChangeUserPassword(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route + "/change-user-password")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ChangeUserPasswordHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UserEndpoint.ForgotPassword))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(ForgotPasswordRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> ForgotPassword(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/forgot-password")] HttpRequest req,
            [Queue("notification-usr-user")] ICollector<string> collector,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<ForgotPasswordHandler>();
            return handler.Execute(req, cancellationToken, false, nameof(collector).WithValue(collector));
        }

        [FunctionName(nameof(UserEndpoint.AddUserSupervisorForExperience))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddUserSupervisorForExperienceRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddUserSupervisorForExperience(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/add-user-supervisor-for-experience")] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<AddUserSupervisorForExperienceHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UserEndpoint.SetRedis))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> SetRedis(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/set-redis")]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<SetRedisHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        [FunctionName(nameof(UserEndpoint.GetRedis))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> GetRedis(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route + "/get-redis")]
            HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetRedisHandler>();
            return handler.Execute(req, cancellationToken, false);
        }
    }
}
