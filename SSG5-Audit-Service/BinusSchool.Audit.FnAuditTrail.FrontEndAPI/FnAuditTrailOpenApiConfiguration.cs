using System.Collections.Generic;
using BinusSchool.Common.Functions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Audit.FnAuditTrail
{
    public class FnAuditTrailOpenApiConfiguration : DefaultOpenApiConfigurationOptions
    {
        public FnAuditTrailOpenApiConfiguration()
        {
#if RELEASE
            ForceHttps = true;
#endif
        }

        public override OpenApiInfo Info { get; set; } = new OpenApiInfo
        {
            Version = "1.0",
            Title = typeof(FnAuditTrailOpenApiConfiguration).Namespace
        };

        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
    }
}