using System;

namespace BinusSchool.Common.Utils
{
    public static class EnvironmentUtil
    {
        public static bool IsDevelopment()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT")))
                return Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT").Equals("Development", StringComparison.InvariantCultureIgnoreCase);

            return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals("Development", StringComparison.InvariantCultureIgnoreCase);
        }
    }
}