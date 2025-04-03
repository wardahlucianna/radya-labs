using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Common.Functions.Storage
{
    public class FunctionsStorageManager : StorageManager
    {
        public FunctionsStorageManager(IConfiguration configuration, ICurrentFunctions currentFunctions, ILogger<FunctionsStorageManager> logger) :
            base(configuration.GetConnectionString($"{currentFunctions.Domain}:AccountStorage"), logger)
        {
        }
    }
}
