using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.BankAccountInformation
{
    public class BankAccountInformationEndPoint
    {
        private const string _route = "student/bank-account-information";
        private const string _tag = "Bank";

        private readonly BankAccountInformationHandler _bankAccountInformationHandler;
        public BankAccountInformationEndPoint(BankAccountInformationHandler bankAccountInformationHandler)
        {
            _bankAccountInformationHandler = bankAccountInformationHandler;
        }

        [FunctionName(nameof(BankAccountInformationEndPoint.GetBankAccountInformations))]
        [OpenApiOperation(tags: _tag, Summary = "Get Bank Account Information List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetBankAccountInformationRequest.IdBank), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetBankAccountInformationRequest.IdStudent), In = ParameterLocation.Query)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBankAccountInformationResult))]
        public Task<IActionResult> GetBankAccountInformations(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _bankAccountInformationHandler.Execute(req, cancellationToken);
        }

        //[FunctionName(nameof(BankAccountInformationEndPoint.GetBankAccountInformationDetail))]
        //[OpenApiOperation(tags: _tag, Summary = "Get Bank Account Information Detail")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiParameter("id", Required = true)]
        //[OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetBankAccountInformationDetailResult))]
        //public Task<IActionResult> GetBankAccountInformationDetail(
        //   [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/{id}")] HttpRequest req,
        //   string id,
        //   CancellationToken cancellationToken)
        //{
        //    return _bankAccountInformationHandler.Execute(req, cancellationToken, true, "id".WithValue(id));
        //}

        //[FunctionName(nameof(BankAccountInformationEndPoint.AddBankAccountInformation))]
        //[OpenApiOperation(tags: _tag, Summary = "Add Bank Account Information")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(AddBankAccountInformationRequest))]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        //public Task<IActionResult> AddBankAccountInformation(
        //    [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
        //    CancellationToken cancellationToken)
        //{
        //    return _bankAccountInformationHandler.Execute(req, cancellationToken);
        //}

        [FunctionName(nameof(BankAccountInformationEndPoint.UpdateBankAccountInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Bank Account Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateBankAccountInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateBankAccountInformation(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _bankAccountInformationHandler.Execute(req, cancellationToken);
        }

        //[FunctionName(nameof(BankAccountInformationEndPoint.DeleteBankAccountInformation))]
        //[OpenApiOperation(tags: _tag, Summary = "Delete Bank Account Information")]
        //[OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        //[OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        //public Task<IActionResult> DeleteBankAccountInformation(
        //    [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        //    CancellationToken cancellationToken)
        //{
        //    return _bankAccountInformationHandler.Execute(req, cancellationToken);
        //}
    }
}
