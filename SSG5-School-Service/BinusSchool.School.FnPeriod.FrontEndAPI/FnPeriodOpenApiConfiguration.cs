using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.School.FnPeriod
{
    public class FnPeriodOpenApiConfiguration : DefaultOpenApiConfigurationOptions
    {
        public FnPeriodOpenApiConfiguration()
        {
#if RELEASE
            ForceHttps = true;
#endif
        }

        public override OpenApiInfo Info { get; set; } = new OpenApiInfo
        {
            Version = "1.0",
            Title = typeof(FnPeriodOpenApiConfiguration).Namespace
        };

        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
    }
}
