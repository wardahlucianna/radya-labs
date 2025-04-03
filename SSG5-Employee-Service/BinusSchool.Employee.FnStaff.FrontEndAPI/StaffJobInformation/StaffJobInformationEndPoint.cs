using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model;
using BinusSchool.Data.Model.Employee.FnStaff.StaffJobInformation;
using BinusSchool.Employee.FnStaff.StaffJobInformation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Employee.FnStaff.StaffJobInformation
{
    public class StaffJobInformationEndPoint
    {
        private const string _route = "employee/staff-job-information";
        private const string _tag = "Staff Job Information";

        private readonly StaffJobInformationHandler _handler;
        public StaffJobInformationEndPoint(StaffJobInformationHandler staffJobInformationHandler
        )
        {
            _handler = staffJobInformationHandler;
        }

        [FunctionName(nameof(StaffJobInformationEndPoint.UpdateStaffJobInformation))]
        [OpenApiOperation(tags: _tag, Summary = "Update Staff Job Information")]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "x-functions-key", In = OpenApiSecurityLocationType.Header)]
        [OpenApiSecurity("authorization", SecuritySchemeType.ApiKey, Name = "authorization", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody("application/json", typeof(UpdateStaffJobInformationRequest))]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        public Task<IActionResult> UpdateStaffJobInformation(
           [HttpTrigger(AuthorizationLevel.Function, "put", Route = _route)] HttpRequest req,
           CancellationToken cancellationToken)
        {
            return _handler.Execute(req, cancellationToken);
        }
    }
}
