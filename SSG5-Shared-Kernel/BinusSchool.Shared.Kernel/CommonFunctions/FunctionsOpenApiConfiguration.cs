//using System;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
//using Microsoft.OpenApi.Models;

//namespace BinusSchool.Common.Functions
//{
//    public class FunctionsOpenApiConfiguration<TFunctions> : DefaultOpenApiConfigurationOptions where TFunctions : new()
//    {
//        public FunctionsOpenApiConfiguration()
//        {
//#if RELEASE
//            ForceHttps = true;
//#endif
//        }

//        public override OpenApiInfo Info { get; set; } = new OpenApiInfo
//        {
//            Version = "1.0",
//            Title = typeof(TFunctions).Namespace
//        };

//        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
//    }
//}