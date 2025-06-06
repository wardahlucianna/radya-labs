﻿using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Employee.FnStaff
{

    public class FnStaffOpenApiConfiguration : DefaultOpenApiConfigurationOptions
    {
        public FnStaffOpenApiConfiguration()
        {
        #if RELEASE
            ForceHttps = true;
        #endif
        }

        public override OpenApiInfo Info { get; set; } = new OpenApiInfo
        {
            Version = "1.0",
            Title = typeof(FnStaffOpenApiConfiguration).Namespace
        };

        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
    }
}
