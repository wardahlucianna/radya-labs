﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Province;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.Province
{
    public class ProvinceEndPoint
    {
        private const string _route = "student/province";
        private const string _tag = "Literal table";

        private readonly ProvinceHandler _provinceHandler;
        public ProvinceEndPoint(ProvinceHandler provinceHandler)
        {
            _provinceHandler = provinceHandler;
        }

        [FunctionName(nameof(ProvinceEndPoint.GetProvinces))]
        [OpenApiOperation(tags: _tag, Summary = "Get Province List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiParameter(nameof(GetProvinceRequest.IdCountry), In = ParameterLocation.Query, Required = true)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetProvinces(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _provinceHandler.Execute(req, cancellationToken);
        }
    }
}
