﻿using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Student.FnStudent.ParentSalaryGroup
{
    public class ParentSalaryGroupEndPoint
    {
        private const string _route = "student/parent-salary-group";
        private const string _tag = "Literal table";

         private readonly ParentSalaryGroupHandler _parentSalaryGroupHandler;
        public ParentSalaryGroupEndPoint(ParentSalaryGroupHandler parentSalaryGroupHandler)
        {
            _parentSalaryGroupHandler = parentSalaryGroupHandler;
        }

        [FunctionName(nameof(ParentSalaryGroupEndPoint.GetParentSalaryGroup))]
        [OpenApiOperation(tags: _tag, Summary = "Get Parent Salary Group List")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(HttpStatusCode.OK, "application/json", typeof(ItemValueVm))]
        public Task<IActionResult> GetParentSalaryGroup(
           [HttpTrigger(AuthorizationLevel.Function, "get", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _parentSalaryGroupHandler.Execute(req, cancellationToken);
        }



    }
}
