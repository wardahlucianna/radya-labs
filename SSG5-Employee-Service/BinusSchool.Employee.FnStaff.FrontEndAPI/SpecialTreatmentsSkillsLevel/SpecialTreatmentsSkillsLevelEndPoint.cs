﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Employee.FnStaff.SpecialTreatmentsSkillsLevel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using BinusSchool.Data.Model;

namespace BinusSchool.Employee.FnStaff.SpecialTreatmentsSkillsLevel
{
    public class SpecialTreatmentsSkillsLevelEndPoint
    {
        private const string _route = "staff/special-treatments-skills-level";
        private const string _tag = "LtTable";
        private readonly SpecialTreatmentsSkillsLevelHandler _handler;
        public SpecialTreatmentsSkillsLevelEndPoint(SpecialTreatmentsSkillsLevelHandler specialTreatmentsSkillsLevelHandler)
        {
            _handler = specialTreatmentsSkillsLevelHandler;
        }

        [FunctionName(nameof(SpecialTreatmentsSkillsLevelEndPoint.GetSpecialTreatmentsSkillsLevels))]
        [OpenApiOperation(tags: _tag, Summary = "Get Special Treatments Skills Level List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(nameof(CollectionRequest.Search), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(CollectionRequest.Return), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(CollectionRequest.Page), In = ParameterLocation.Query, Type = typeof(int))]
        //[OpenApiParameter(nameof(CollectionRequest.Size), In = ParameterLocation.Query, Type = typeof(int))]
        [OpenApiParameter(nameof(CollectionRequest.OrderBy), In = ParameterLocation.Query)]
        [OpenApiParameter(nameof(CollectionRequest.OrderType), In = ParameterLocation.Query)]
        //[OpenApiParameter(nameof(CollectionRequest.GetAll), In = ParameterLocation.Query, Type = typeof(bool))]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetSpecialTreatmentsSkillsLevels(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}