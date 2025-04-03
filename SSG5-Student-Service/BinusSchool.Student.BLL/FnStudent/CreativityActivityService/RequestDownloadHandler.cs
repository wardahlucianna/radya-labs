using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Student.FnLongRun;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.CreativityActivityService.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class RequestDownloadHandler : FunctionsHttpSingleHandler
    {
#if DEBUG
        private const string _containerName = "cas-download-request-debug";
#else
        private const string _containerName = "cas-download-request";
#endif

        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _machineDateTime;
        private readonly ILogger<RequestDownloadHandler> _logger;

        public RequestDownloadHandler(IStudentDbContext dbContext,
            IMachineDateTime machineDateTime,
            ILogger<RequestDownloadHandler> logger)
        {
            _dbContext = dbContext;
            _machineDateTime = machineDateTime;
            _logger = logger;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            FillConfiguration();

            var body = await Request
                .ValidateBody<CasExperienceDownloadRequest, CasExperienceDownloadRequestValidator>();

            _logger.LogInformation("Cas request download requested by {IdUser}", body.IdUser);

            var requestDownloadExperience = await _dbContext.Entity<TrRequestDownloadExperience>()
                .Where(e => e.IdUserRequest == body.IdUser)
                .OrderByDescending(e => e.DateIn)
                .FirstOrDefaultAsync();

            if (requestDownloadExperience != null)
            {
                if (requestDownloadExperience.State == RequestDownloadState.Request ||
                    requestDownloadExperience.State == RequestDownloadState.OnGoing)
                {
                    if (requestDownloadExperience.State == RequestDownloadState.OnGoing)
                    {
                        //if request download exp state takes longer than 2 hours, then marked as error
                        var dt = requestDownloadExperience.DateIn!.Value.AddHours(2);
                        if (dt > _machineDateTime.ServerTime)
                            throw new BadRequestException("Your last request still in progress");
                    }
                    else
                        throw new BadRequestException("Your last request still in progress");
                }
            }

            var id = Guid.NewGuid().ToString();
            _dbContext.Entity<TrRequestDownloadExperience>().Add(new TrRequestDownloadExperience()
            {
                Id = id,
                IdUserRequest = body.IdUser,
                State = RequestDownloadState.Request
            });
            await _dbContext.SaveChangesAsync();

            //create file container in blob storage 
            CreateContainerIfNotExists(_containerName);
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(_containerName);

            var filename = $"{Guid.NewGuid()}.json";
            var blob = container.GetBlockBlobReference(filename);
            blob.Properties.ContentType = "application/json";

            var s = JsonConvert.SerializeObject(new ExperienceDownloadRequest
                {
                    Id = id, IdUser = body.IdUser, IdStudent = body.IdStudent, IdAcademicYears = body.IdAcademicYears,
                    IdSchool = body.IdSchool, Role = body.Role, IncludeComment = body.IncludeComment,
            },
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented, ContractResolver = new CamelCasePropertyNamesContractResolver()
                });

            using (var ms = new MemoryStream())
            {
                await using (var sw = new StreamWriter(ms))
                {
                    await sw.WriteAsync(s);
                    await sw.FlushAsync();
                    ms.Position = 0;
                    //upload file
                    await blob.UploadFromStreamAsync(ms);
                }
            }

            return Request.CreateApiResult2();
        }

        public CloudStorageAccount GetCloudStorageAccount()
        {
            var x = Configuration["ConnectionStrings:Student:AccountStorage"];
            var storageAccount = CloudStorageAccount.Parse(x);
            return storageAccount;
        }

        public void CreateContainerIfNotExists(string containerName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var containers = new[] { containerName };

            foreach (var item in containers)
            {
                var blobContainer = blobClient.GetContainerReference(item);
                blobContainer.CreateIfNotExistsAsync();
            }
        }
    }
}
