using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.School.FnSchool;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Data.Model.School.FnSchoolLongrun.SurveySummary;
using BinusSchool.School.FnLongRun.Services.SurveySummary;
using BinusSchool.Data.Api.School.FnSurveyNoSql;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BinusSchool.School.FnLongRun.SurveySummary
{
    public class SurveySummaryHandler
    {
#if DEBUG
        private const string _blobPath = "survey-summary-debug/add/{name}.json";
#else
        private const string _blobPath = "survey-summary/add/{name}.json";
#endif

        private readonly ISchoolDbContext _dbContext;
        private readonly ISurveySummary _serviceSurveySummary;
        private readonly ISurveySummaryNoSql _serviceSurveySummaryNoSql;
        private readonly ISurveyTemplateNoSql _serviceSurveyTemplateNoSql;
        private readonly IStorageManager _storageManager;
        private readonly IMachineDateTime _dateTieme;
        private readonly ILogger<SurveySummaryHandler> _logger;
        public SurveySummaryHandler(ISchoolDbContext dbContext, ISurveySummary serviceSurveySummary, IStorageManager storageManager, ISurveySummaryNoSql serviceSurveySummaryNoSql, IMachineDateTime dateTieme, ISurveyTemplateNoSql serviceSurveyTemplateNoSql, ILogger<SurveySummaryHandler> logger)
        {
            _dbContext = dbContext;
            _serviceSurveySummary = serviceSurveySummary;
            _storageManager = storageManager;
            _serviceSurveySummaryNoSql = serviceSurveySummaryNoSql;
            _dateTieme = dateTieme;
            _serviceSurveyTemplateNoSql = serviceSurveyTemplateNoSql;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger)); // Pastikan logger tidak null
        }

        [FunctionName(nameof(SurveySummary))]
        public async Task SurveySummary([BlobTrigger(_blobPath)] Stream blobStream,
            IDictionary<string, string> metadata,
            string name,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation($"[Survey Summary Handler]-proses Number 1");
            var body = default(SurveySummaryDownloadRequest);
            _logger.LogInformation($"[Survey Summary Handler]-proses Number 2");
            try
            {
                using var blobStreamReader = new StreamReader(blobStream);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 3");
                using var jsonReader = new JsonTextReader(blobStreamReader);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 4");
                while (await jsonReader.ReadAsync(cancellationToken))
                {
                    _logger.LogInformation($"[Survey Summary Handler]-proses Number 5");
                    if (jsonReader.TokenType == JsonToken.StartObject)
                    {
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 6");
                        body = new JsonSerializer().Deserialize<SurveySummaryDownloadRequest>(jsonReader);
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 7");
                        break;
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 8");
                    }
                }

                _logger.LogInformation($"[Survey Summary Handler]-proses Number 9");
                GetSurveySummaryLogRequest paramGetSurveySummaryLog = new GetSurveySummaryLogRequest
                {
                    IdUser = body.IdUser
                };

                _logger.LogInformation($"[Survey Summary Handler]-proses Number 10");

                #region user have process
                var getSurveySummaryLog = await _serviceSurveySummary.GetSurveySummaryLog(new GetSurveySummaryLogRequest
                {
                    IdUser = body.IdUser
                });
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 11");
                var UserRequest = getSurveySummaryLog.IsSuccess ? getSurveySummaryLog.Payload : null;
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 12");

                if (UserRequest.IsStillProses)
                    throw new Exception("Already Process");
                #endregion
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 13");

                #region create log (start)
                await _serviceSurveySummary.AddAndUpdateSurveySummaryLog(new AddAndUpdateSurveySummaryLogRequest
                {
                    IdUser = body.IdUser,
                    IsDone = false,
                    IsError = false,
                    IsProses = true,
                });
                #endregion
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 14");

                var idAcademicYear = await _dbContext.Entity<TrPublishSurvey>()
                    .Where(x => x.Id == body.IdPublishSurvey)
                    .Select(x => x.IdAcademicYear)
                    .FirstOrDefaultAsync(cancellationToken);

                _logger.LogInformation($"[Survey Summary Handler]-proses Number 15");

                byte[] excel = default;
                if (body.SurveyTemplateType== SurveyTemplateType.Survey) 
                {
                    _logger.LogInformation($"[Survey Summary Handler]-proses Number 16");
                    if (body.Role.Contains("STAFF") || body.Role.Contains("TEACHER"))
                    {
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 17");
                        excel = await SurveySummaryGeneral.SurveyGeneralExcel(new RespondentRequest
                        {
                            CancellationToken = cancellationToken,
                            DbContext = _dbContext,
                            ServiceSurveySummary = _serviceSurveySummary,
                            ServiceSurveySummaryNoSql = _serviceSurveySummaryNoSql,
                            IdPublishSurvey = body.IdPublishSurvey,
                            Date = _dateTieme.ServerTime,
                            SurveyTemplateNoSql = _serviceSurveyTemplateNoSql,
                            IdAcademicYear = idAcademicYear
                        }, _logger);
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 18");
                    }
                    else
                    {
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 19");
                        excel = await SurveySummaryStudentParent.GenerateExcel(new RespondentRequest
                        {
                            CancellationToken = cancellationToken,
                            DbContext = _dbContext,
                            ServiceSurveySummary = _serviceSurveySummary,
                            ServiceSurveySummaryNoSql = _serviceSurveySummaryNoSql,
                            IdPublishSurvey = body.IdPublishSurvey,
                            Date = _dateTieme.ServerTime,
                            SurveyTemplateNoSql = _serviceSurveyTemplateNoSql,
                            IdAcademicYear = idAcademicYear
                        });
                        _logger.LogInformation($"[Survey Summary Handler]-proses Number 20");
                    }
                }
                else
                {
                    _logger.LogInformation($"[Survey Summary Handler]-proses Number 21");
                    excel = await SurveySummaryStudentLearning.GenerateExcel(new RespondentRequest
                    {
                        CancellationToken = cancellationToken,
                        DbContext = _dbContext,
                        ServiceSurveySummary = _serviceSurveySummary,
                        ServiceSurveySummaryNoSql = _serviceSurveySummaryNoSql,
                        IdPublishSurvey = body.IdPublishSurvey,
                        Date = _dateTieme.ServerTime,
                        SurveyTemplateNoSql = _serviceSurveyTemplateNoSql,
                        IdAcademicYear = idAcademicYear
                    });
                    _logger.LogInformation($"[Survey Summary Handler]-proses Number 22");
                }
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 23");
                #region save storage pdf
                var bytes = excel;

                // save to storage
                var blobName = $"SurveySummary-{Guid.NewGuid().ToString()}.xlsx";
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 24");
                var blobContainer = await _storageManager.GetOrCreateBlobContainer("survey-summary", ct: cancellationToken);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 25");
                var blobClient = blobContainer.GetBlobClient(blobName);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 26");

                var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, cancellationToken);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 27"); ;
                var rawBlobResult = blobResult.GetRawResponse();
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 28");

                if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                    throw new Exception(rawBlobResult.ReasonPhrase);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 29");
                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClient);
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 30");
                #endregion

                #region update log (finish)
                await _serviceSurveySummary.AddAndUpdateSurveySummaryLog(new AddAndUpdateSurveySummaryLogRequest
                {
                    IdUser = body.IdUser,
                    IsDone = true,
                    IsError = false,
                    IsProses = false,
                });
                #endregion
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 31");
                #region Email sucsess
                await _serviceSurveySummary.SendEmailSurveySummary(new SendEmailSurveySummaryRequest
                {
                    IdScenario = "ESS1",
                    IdUser = body.IdUser,
                    Link = sasUri.AbsoluteUri,
                    IdSchool = body.IdSchool
                });
                #endregion
                _logger.LogInformation($"[Survey Summary Handler]-proses Number 32");
            }
            catch (Exception ex)
            {
                #region update log (faild)
                await _serviceSurveySummary.AddAndUpdateSurveySummaryLog(new AddAndUpdateSurveySummaryLogRequest
                {
                    IdUser = body.IdUser,
                    IsDone = false,
                    IsError = true,
                    IsProses = false,
                    Message = ex.Message
                });
                #endregion

                #region Email faild
                await _serviceSurveySummary.SendEmailSurveySummary(new SendEmailSurveySummaryRequest
                {
                    IdScenario = "ESS2",
                    IdUser = body.IdUser,
                    IdSchool = body.IdSchool
                });
                #endregion
            }
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.AddMonths(1);

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }
    }

    public class RespondentRequest : DefaultRequest
    {
        public string IdPublishSurvey { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime Date {  get; set; }
    }
}
