using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Data.Model.Student.FnStudent.MasterPortfolio;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.MasterPortfolio
{
    public class MasterPortfolioEndpoint
    {
        private const string _route = "student/master-data-portfolio";
        private const string _tag = "Master Data portfolio";

        //list
        [FunctionName(nameof(MasterPortfolioEndpoint.GetListMasterDataPortfolio))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetMasterPortfolioRequest.Type), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterPortfolioResult))]
        public Task<IActionResult> GetListMasterDataPortfolio(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<GetListMasterPortfolioHandler>();
            return handler.Execute(req, cancellationToken, false);
        }

        //add
        [FunctionName(nameof(MasterPortfolioEndpoint.AddMasterDataPortfolio))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(AddMasterPortfolioRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> AddMasterDataPortfolio(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MasterPortfolioHandler>();
            return handler.Execute(req, cancellationToken);
        }

        //update
        [FunctionName(nameof(MasterPortfolioEndpoint.UpdateMasterDataPortfolio))]
        [OpenApiOperation(tags: _tag)]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateMasterPortfolioRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateMasterDataPortfolio(
            [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MasterPortfolioHandler>();
            return handler.Execute(req, cancellationToken);
        }

        //detail
        [FunctionName(nameof(MasterPortfolioEndpoint.GetMasterDataPortfolioDetail))]
        [OpenApiOperation(tags: _tag, Summary = "Get Master Data Portfolio Detail")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter("id", Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(GetMasterPortfolioDetailResult))]
        public Task<IActionResult> GetMasterDataPortfolioDetail(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route + "/detail/{id}")] HttpRequest req,
            string id,
            CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MasterPortfolioHandler>();
            return handler.Execute(req, cancellationToken, keyValues: "id".WithValue(id));
        }

        //delete
        [FunctionName(nameof(MasterPortfolioEndpoint.DeleteMasterDataPortfolio))]
        [OpenApiOperation(tags: _tag, Summary = "Delete Master Data Portfolio")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(IEnumerable<string>), Required = true)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "See inner property")]
        public Task<IActionResult> DeleteMasterDataPortfolio(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = _route)] HttpRequest req,
        CancellationToken cancellationToken)
        {
            var handler = req.HttpContext.RequestServices.GetService<MasterPortfolioHandler>();
            return handler.Execute(req, cancellationToken);
        }
    }
}
