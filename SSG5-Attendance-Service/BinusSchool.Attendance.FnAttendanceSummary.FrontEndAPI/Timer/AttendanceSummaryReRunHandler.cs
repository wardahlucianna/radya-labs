using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Constants;
using BinusSchool.Data.Model.Student.FnStudent.MeritDemeritTeacher;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Linq;
using BinusSchool.Common.Abstractions;
using NPOI.OpenXmlFormats.Spreadsheet;
using static ClosedXML.Excel.XLPredefinedFormat;
using Microsoft.WindowsAzure.Storage;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Diagnostics;
using BinusSchool.Data.Api.School.FnSchool;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;
using BinusSchool.Persistence.AttendanceDb.Models;
using Newtonsoft.Json;
using NPOI.XWPF.UserModel;
using Microsoft.Extensions.DependencyInjection;
using BinusSchool.Attendance.FnAttendanceSummary.Services;


namespace BinusSchool.Attendance.FnAttendanceSummary.Timer
{
    public class AttendanceSummaryReRunHandler
    {

#if DEBUG
        private const string _blobPath = "attendance-summary-debug/{name}.json";
        private const string _containerPath = "attendance-summary-debug";
        private const string _backupContainerPath = "attendance-summary-source-debug";
#else
        private const string _blobPath = "attendance-summary/{name}.json";
        private const string _containerPath = "attendance-summary";
        private const string _backupContainerPath = "attendance-summary-source";
#endif
        private const string _ncrontabExperssion = "0 */10 * * * *"; // once every 10 minute  

        private readonly IServiceProvider _serviceProvider;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<AttendanceSummaryReRunHandler> _logger;
        private readonly IMachineDateTime _dateTime;
        private readonly IConfiguration _configuration;

        public AttendanceSummaryReRunHandler(
            IServiceProvider serviceProvider,
            IAttendanceDbContext dbContext,
            IMachineDateTime dateTime,
            IConfiguration configuration,
            ILogger<AttendanceSummaryReRunHandler> logger)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _logger = logger;
            _dateTime = dateTime;
            _configuration = configuration;
        }

        public CloudStorageAccount GetCloudStorageAccount()
        {
            var s = _configuration["ConnectionStrings:Attendance:AccountStorage"];

#if DEBUG
            s = "UseDevelopmentStorage=true";
#endif

            var storageAccount = CloudStorageAccount.Parse(s);
            return storageAccount;
        }

        public void CreateContainerIfNotExists(string containerName)
        {
            var storageAccount = GetCloudStorageAccount();
            var blobClient = storageAccount.CreateCloudBlobClient();
            var blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExistsAsync();
        }

        [FunctionName(nameof(AttendanceSummaryReRun))]
        public async Task AttendanceSummaryReRun([TimerTrigger(_ncrontabExperssion
#if DEBUG
                , RunOnStartup = true
#endif
            )]
            TimerInfo myTimer,
    CancellationToken cancellationToken)
        {
            _logger.LogInformation("Re run read data json Attendance Summary", _dateTime.ServerTime);

            var tasks = new Task[0];                 

            if (_dateTime.ServerTime.Hour > 1 && _dateTime.ServerTime.Hour < 6)
            {

                var listBody = new List<SummaryDto>();
                var storageAccount = GetCloudStorageAccount();
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(_containerPath);

                BlobContinuationToken blobContinuationToken = null;

                var resultSegment = await container.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.None, 20, blobContinuationToken, null, null);
                if (resultSegment != null)
                {
                    if (resultSegment.Results.Any())
                    {
                        foreach (var item in resultSegment.Results)
                        {
                            var blobName = item.Uri.Segments.Last().Replace("%20", " ");
                            var blob = container.GetBlobReference(blobName);

                            if (await blob.ExistsAsync())
                            {
                                var response = await blob.OpenReadAsync();
                                try
                                {
                                    using var blobStreamReader = new StreamReader(response);
                                    using var jsonReader = new JsonTextReader(blobStreamReader);

                                    while (await jsonReader.ReadAsync(cancellationToken))
                                    {
                                        if (jsonReader.TokenType != JsonToken.StartObject) continue;

                                        var reader = new JsonSerializer().Deserialize<SummaryDto>(jsonReader);
                                        reader.JsonName = blobName;
                                        listBody.Add(reader);

                                        break;
                                    }
                                }
                                catch (Exception e)
                                {
                                    _logger.LogError(e, "Error occurs");
                                    throw;
                                }
                            }
                        }
                    }
                }

                if (listBody.Count > 0)
                {
                    _logger.LogInformation($"Process {listBody.Count} data json");

                    var sw = Stopwatch.StartNew();

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                        var dbContext = scope.ServiceProvider.GetRequiredService<IAttendanceDbContext>();
                        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                        var service = new ReRunAttendanceSummary(dbContext,
                            configuration,
                            loggerFactory.CreateLogger<ReRunAttendanceSummary>());

                        await service.RunAsync(listBody, cancellationToken);
                    }

                    sw.Stop();

                    _logger.LogInformation("Process data json Attendance Summary done for {TotalSeconds}s", Math.Round(sw.Elapsed.TotalSeconds));
                }
                else
                {
                    _logger.LogInformation($"No data json");
                }
            }
            await Task.WhenAll(tasks);
        }
    }
}
