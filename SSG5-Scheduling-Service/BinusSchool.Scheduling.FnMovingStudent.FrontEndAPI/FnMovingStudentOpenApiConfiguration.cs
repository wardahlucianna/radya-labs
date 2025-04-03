using BinusSchool.Common.Functions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BinusSchool.Scheduling.FnMovingSubject
{
  public class FnMovingStudentOpenApiConfiguration : DefaultOpenApiConfigurationOptions
  {
    public FnMovingStudentOpenApiConfiguration()
    {
#if RELEASE
            ForceHttps = true;
#endif
    }

    public override OpenApiInfo Info { get; set; } = new OpenApiInfo
    {
      Version = "1.0",
      Title = typeof(FnMovingStudentOpenApiConfiguration).Namespace
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
  }
}
