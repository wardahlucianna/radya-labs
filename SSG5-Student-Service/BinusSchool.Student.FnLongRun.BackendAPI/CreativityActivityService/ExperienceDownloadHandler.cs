using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Api.Util.FnConverter;
using BinusSchool.Data.Model.Student.FnLongRun;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Util.FnConverter.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnLongRun.CreativityActivityService.Validator;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BinusSchool.Student.FnLongRun.CreativityActivityService
{
    public class CasExperienceDownloadHandler : FunctionsHttpHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly ICreativityActivityServiceToPdf _creativityActivityServiceToPdf;
        private readonly ICreativityActivityService _creativityActivityService;

        private readonly ILogger<CasExperienceDownloadHandler> _logger;

        private readonly IStringLocalizer _stringLocalizer;
        private readonly IMachineDateTime _machineDateTime;
        private readonly string _utilStorageConnection;
        private readonly string _studentStorageConnection;

#if DEBUG
        private const string _blobPath = "cas-download-request-debug/{name}.json";
#else
        private const string _blobPath = "cas-download-request/{name}.json";
#endif

        public CasExperienceDownloadHandler(IStudentDbContext dbContext,
            ICreativityActivityServiceToPdf creativityActivityServiceToPdf,
            ICreativityActivityService creativityActivityService,
            IStringLocalizer stringLocalizer,
            IMachineDateTime machineDateTime,
            IConfiguration configuration,
            ILogger<CasExperienceDownloadHandler> logger)
        {
            _dbContext = dbContext;
            _creativityActivityServiceToPdf = creativityActivityServiceToPdf;
            _creativityActivityService = creativityActivityService;
            _logger = logger;
            _stringLocalizer = stringLocalizer;
            _machineDateTime = machineDateTime;
#if DEBUG
            //_utilStorageConnection = "UseDevelopmentStorage=true";
            _utilStorageConnection = configuration["ConnectionStrings:Util:AccountStorage"];
            _studentStorageConnection = "UseDevelopmentStorage=true";
#else
            _utilStorageConnection = configuration["ConnectionStrings:Util:AccountStorage"];
            _studentStorageConnection = configuration["ConnectionStrings:Student:AccountStorage"];
#endif
        }

        [FunctionName(nameof(CasExperienceDownload))]
        public async Task CasExperienceDownload([BlobTrigger(_blobPath)] Stream blobStream, string name,
            CancellationToken cancellationToken)
        {
            var requestId = Guid.NewGuid();

            _logger.LogInformation(
                "Blob trigger function Processed blob \n Name:{Name} \n Size: {myBlob.Length} Bytes, with request id : {RequestId}",
                name,
                blobStream.Length, requestId);

            var body = default(ExperienceDownloadRequest);
            using var blobStreamReader = new StreamReader(blobStream);
            using var jsonReader = new JsonTextReader(blobStreamReader);

            while (await jsonReader.ReadAsync(cancellationToken))
            {
                if (jsonReader.TokenType != JsonToken.StartObject) continue;
                body = new JsonSerializer().Deserialize<ExperienceDownloadRequest>(jsonReader);
                break;
            }

            if (body is null)
                return;

            // validate body
            (await new ExperienceDownloadRequestValidator().ValidateAsync(body, cancellationToken)).EnsureValid(
                localizer: _stringLocalizer);

            var requestExp = await _dbContext.Entity<TrRequestDownloadExperience>().Where(e => e.Id == body!.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (requestExp is null || requestExp.State != RequestDownloadState.Request)
            {
                _logger.LogWarning("Data transactions request download exp is null when ID {Name}", body!.Id);
                return;
            }

            requestExp!.StartDate = _machineDateTime.ServerTime;
            requestExp!.State = RequestDownloadState.OnGoing;
            _dbContext.Entity<TrRequestDownloadExperience>().Update(requestExp!);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var timeline = await _creativityActivityServiceToPdf.TimelineToPdf(new TimelineToPdfRequest
            {
                IdSchool = body.IdSchool,
                IdUser = body.IdUser,
                IdStudent = body.IdStudent,
                Role = body.Role,
                IdAcademicYear = body.IdAcademicYears
            });

            var listExp = await _creativityActivityServiceToPdf.ExperienceToPdf(new ExperienceToPdfRequest
            {
                IdSchool = body.IdSchool,
                IdUser = body.IdUser,
                IdStudent = body.IdStudent,
                Role = body.Role,
                IsComment = body.IncludeComment,
                IdAcademicYear = body.IdAcademicYears
            });

            listExp = listExp.ToList();

            if (!listExp.Any())
            {
                _logger.LogError("Data exp is empty");
                return;
            }

            var zipFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid()}.zip";

            var files = new List<TempModel>
                { new TempModel { FileName = timeline.FileName, RealFileName = timeline.RealFileName } };
            files.AddRange(listExp.Select(item => new TempModel
                { FileName = item.FileName, RealFileName = item.RealFileName }));
            string blobUrl;

            try
            {
                var zipBlobClient = new BlockBlobClient(_studentStorageConnection, "output", zipFileName);

                await using var zipFileStream = await zipBlobClient.OpenWriteAsync(true,
                    options: new BlockBlobOpenWriteOptions
                    {
                        HttpHeaders = new BlobHttpHeaders
                        {
                            ContentType = "application/zip"
                        }
                    }, cancellationToken: cancellationToken);

                await using var zipFileOutputStream = new ZipOutputStream(zipFileStream)
                {
                    IsStreamOwner = false
                };

                const int level = 0;
                _logger.LogInformation("Using Level {Level} compression", level);
                zipFileOutputStream.SetLevel(0);

                foreach (var file in files)
                {
                    var blockBlobClient =
                        new BlockBlobClient(_utilStorageConnection, "creativity-activity-service", file.FileName);

                    var properties =
                        await blockBlobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

                    var zipEntry = new ZipEntry(file.RealFileName)
                    {
                        Size = properties.Value.ContentLength
                    };
                    zipFileOutputStream.PutNextEntry(zipEntry);
                    await blockBlobClient.DownloadToAsync(zipFileOutputStream, cancellationToken);
                    zipFileOutputStream.CloseEntry();

                    //delete the file
                    await blockBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                }

                blobUrl = zipBlobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                var blockBlobClient = new BlockBlobClient(_studentStorageConnection, "output", zipFileName);
                await blockBlobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);

                requestExp!.EndDate = _machineDateTime.ServerTime;
                requestExp!.State = RequestDownloadState.Failed;
                requestExp!.ErrorMessage = ex.Message;

                _dbContext.Entity<TrRequestDownloadExperience>().Update(requestExp!);
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw;
            }

            requestExp!.EndDate = _machineDateTime.ServerTime;
            requestExp!.State = RequestDownloadState.Done;
            _dbContext.Entity<TrRequestDownloadExperience>().Update(requestExp!);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Zip already created with full url : {BlobUrl}", blobUrl);

            #region Notification
            var result = await _creativityActivityService.EmailDownload(new EmailDownloadResult
            {
                IdUser = body.IdUser,
                Link = blobUrl,
                IdSchool = body.IdSchool
            });
            #endregion
        }

        private class TempModel
        {
            public string FileName { get; set; }
            public string RealFileName { get; set; }
        }
    }
}
