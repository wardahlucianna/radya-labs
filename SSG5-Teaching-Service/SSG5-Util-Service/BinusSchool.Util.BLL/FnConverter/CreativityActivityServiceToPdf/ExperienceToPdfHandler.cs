using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using BinusSchool.Data.Model.Util.FnConverter.CreativityActivityService;
using EasyCaching.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using CalendarEventTypeVm = BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent.CalendarEventTypeVm;

namespace BinusSchool.Util.FnConverter.CreativityActivityServiceToPdf
{
    public class ExperienceToPdfHandler : FunctionsHttpSingleHandler
    {
        private static readonly IEnumerable<DayOfWeek> _days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly IStorageManager _storageManager;
        private readonly ISchool _schoolService;
        private readonly ICreativityActivityService _creativityActivityService;

        public ExperienceToPdfHandler(IConverter converter, IEasyCachingProvider inMemoryCahce, IStorageManager storageManager,
            ICreativityActivityService creativityActivityService, ISchool schoolService)
        {
            _converter = converter;
            _inMemoryCache = inMemoryCahce;
            _storageManager = storageManager;
            _creativityActivityService = creativityActivityService;
            _schoolService = schoolService;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = Request.ValidateParams<ExperienceToPdfRequest>();

            var schoolResult = default(GetSchoolDetailResult);
            var schoolResultKey = $"{nameof(schoolResult)}_{request.IdSchool}";
            if (!_inMemoryCache.TryGetCacheValue(schoolResultKey, out schoolResult))
            {
                var result = await _schoolService.GetSchoolDetail(request.IdSchool);
                schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(schoolResultKey, schoolResult, TimeSpan.FromMinutes(10));
            }

            var logo = default(string);
            if (!string.IsNullOrEmpty(schoolResult.LogoUrl))
            {
                var logoKey = $"{nameof(logo)}-school{schoolResult.Id}";
                if (!_inMemoryCache.TryGetCacheValue(logoKey, out logo))
                {
                    var blobNameLogo = schoolResult.LogoUrl;
                    var blobContainerLogo = await _storageManager.GetOrCreateBlobContainer("school-logo", ct: CancellationToken);
                    var blobClientLogo = blobContainerLogo.GetBlobClient(blobNameLogo);

                    var sasUriLogo = GenerateSasUri(blobClientLogo);

                    using var client = new HttpClient();
                    logo = await client.GetImageAsBase64(sasUriLogo.AbsoluteUri);

                    await _inMemoryCache.SetAsync(logoKey, logo, TimeSpan.FromMinutes(10));
                }
            }


            List<GetExperienceResult> ExperienceResult = new List<GetExperienceResult>();
            var ExperienceKey = $"{nameof(ExperienceResult)}_{request.IdUser}_{request.IdStudent}_{request.IdAcademicYear}";
            if (!_inMemoryCache.TryGetCacheValue(ExperienceKey, out ExperienceResult))
            {
                    var result = await _creativityActivityService.GetExperienceToPdf(new GetExperienceToPdfRequest
                    {
                        IdUser = request.IdUser,
                        IdStudent = request.IdStudent,
                        IdAcademicYear = request.IdAcademicYear,
                        Role = request.Role,
                        IsComment = request.IsComment,
                    });
                    ExperienceResult = result.IsSuccess ? result.Payload.ToList() : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(ExperienceKey, ExperienceResult, TimeSpan.FromMinutes(10));
            }


            List<ConvertExperienceToPdfResult> fileResult = new List<ConvertExperienceToPdfResult>();
            foreach (var ItemExperience in ExperienceResult)
            {
                var htmExperience = BuildExperience(ItemExperience);
                var htmEvidance = BuildEvidance(ItemExperience.Evidances);
                var htmlGenerated = BuildFullExperience(ItemExperience, htmExperience, htmEvidance, logo);

                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                        ColorMode = ColorMode.Color,
                        Orientation = Orientation.Portrait,
                        PaperSize = PaperKind.A4,
                        Outline = false
                    },
                    Objects = {
                        new ObjectSettings() {
                            PagesCount = true,
                            HtmlContent = htmlGenerated,
                            WebSettings = { DefaultEncoding = "utf-8" },
                            // HeaderSettings = { FontSize = 9, Right = "Page [page] of [toPage]", Line = true, Spacing = 2.812 }
                        }
                    }
                };

                #region save storage pdf
                var bytes = _converter.Convert(doc);

                // save to storage
                var blobName = $"Experience-{Guid.NewGuid().ToString()}.pdf";
                var blobContainer = await _storageManager.GetOrCreateBlobContainer("creativity-activity-service", ct: CancellationToken);
                var blobClient = blobContainer.GetBlobClient(blobName);

                var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, CancellationToken);
                var rawBlobResult = blobResult.GetRawResponse();

                if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                    throw new Exception(rawBlobResult.ReasonPhrase);

                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClient);

                fileResult.Add(new ConvertExperienceToPdfResult
                {
                    RealFileName = $"Experience-{ItemExperience.ExperienceName}-{ItemExperience.Student}.pdf",
                    FileName = blobName,
                    ContentType = "application/pdf",
                    Location = sasUri
                });
                #endregion

            }
            return new JsonResult(fileResult, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
        }

        #region Private Method

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.Add(TimeSpan.FromMinutes(5));

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }

        private string BuildExperience(GetExperienceResult GetExperience)
        {
            var builder = new StringBuilder();
            builder.Append("<table border='0'>");
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Student</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          GetExperience.Student);
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Activity Date</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          GetExperience.ActivityDate);
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Organization</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          GetExperience.Organizer == null ? "N/A" : GetExperience.Organizer);
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Description</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          GetExperience.Description);

            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Contribution to Me & My Community</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          GetExperience.Organizer);

            var LearningOutcome = "<ol style='padding-left: 20px;margin:0px;'>";
            foreach (var ItemLearningOutcomes in GetExperience.LearningOutcome)
            {
                LearningOutcome += $"<li>{ItemLearningOutcomes}</li>";
            }
            LearningOutcome += "</ol>";

            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;vertical-align: top;'>Learning Outcomes</td>
                                <td style='width: 2%;text-align: left;vertical-align: top;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          LearningOutcome);
            builder.Append("</table>");
            return builder.ToString();
        }

        private string BuildEvidance(List<GetEvidance> Evidance)
        {
            var builder = new StringBuilder();

            if (Evidance.Any())
            {
                foreach (var ItemEvidance in Evidance)
                {
                    var indexEvidance = Evidance.IndexOf(ItemEvidance);
                    if(indexEvidance == 0)
                        builder.Append("<h3 style='text-align: left'><b>evidences</b></h3>");

                    builder.Append("<table border='1'>");

                    var content = "";
                    if (ItemEvidance.Type == EvidencesType.Text.ToString())
                    {
                        content = ItemEvidance.Value;
                    }
                    else if (ItemEvidance.Type == EvidencesType.Link.ToString())
                    {
                        content = ItemEvidance.Link;
                    }
                    else if (ItemEvidance.Type == EvidencesType.Image.ToString())
                    {
                        foreach (var ItemAtt in ItemEvidance.Attachment)
                        {
                            content += $"<img src='{ItemAtt}' alt='alternatetext' style = 'max-width:150px;max-height:150px;width: auto;height: auto;padding-right: 20px;'>";
                        }
                    }
                    else if (ItemEvidance.Type == EvidencesType.File.ToString())
                    {
                        foreach (var ItemAtt in ItemEvidance.Attachment)
                        {
                            content += ItemAtt + "<br>";
                        }
                    }

                    builder.AppendFormat(null,
                        @"<tr>
                            <td style='width: 100%;text-align: left;background:#b8b0b0'>
                                <table width= '100%'>
                                    <tr>
                                        <td style='width: 50%;text-align: left;'><b>Post By {0}</b></td>
                                        <td style='width: 50%;text-align: right;'>{1}</td>
                                    </tr>
                                </table>
                            </td>
                        </tr>", ItemEvidance.Student, ItemEvidance.Date);

                    var LearningOutcomeHtml = "<ol style='margin-left: 30px;'>";
                    foreach (var ItemLearningOutcomes in ItemEvidance.LearningOutcome)
                    {
                        LearningOutcomeHtml += $"<li>{ItemLearningOutcomes}</li>";
                    }
                    LearningOutcomeHtml += "</ol>";

                    builder.AppendFormat(null,
                       @"<tr>
                            <td style='width: 100%;text-align: left;'>
                                <b>{0}</b><br>
                                {1} <br><br>
                                <b>Learning Outcomes</b>
                                " + LearningOutcomeHtml + @"
                            </td>
                        </tr>", ItemEvidance.Type, content);

                    if (ItemEvidance.IsComment)
                    {
                        foreach (var ItemCommnet in ItemEvidance.Commens)
                        {
                            var indexComment = ItemEvidance.Commens.IndexOf(ItemCommnet);
                            if(indexComment==0)
                                builder.Append(
                                   @"<tr>
                                        <th style='width: 100%;text-align: left;'>
                                            Comment
                                        </th>
                                    </tr>");

                            builder.Append(
                           @"<tr>
                                <td style='width: 100%;text-align: left;'>
                                    <b>" + ItemCommnet.NameComment + @" - " + ItemCommnet.IdUserComment + @"</b> | " + ItemCommnet.Date + @"<br>
                                    " + ItemCommnet.Comment + @"
                                </td>
                            </tr>");


                        }
                    }

                    builder.Append("</table><br>");
                }
            }

            return builder.ToString();
        }

        private string BuildFullExperience(GetExperienceResult GetExperience, string htmlExperience, string htmlEvidance, string logoBase64)
        {
            const string sectionHead = "<head> <meta charset='utf-8'> <meta http-equiv='X-UA-Compatible' content='IE=edge'> <title>Academic Calendar</title> <meta name='viewport' content='width=device-width, initial-scale=1'> <style> body { font-family: Arial; } #wrapper { width: 900px; margin: 0px auto; padding-top: 10px; text-align: left; } h1 { margin-top: 40px; margin-bottom: -10px; text-align: right; color: #333; font-size: 30px; } h2 { margin-bottom: 10px; color: #671130; text-align: right; font-size: 22px; margin-top: 28px; } h3 { font-size: 18px; font-weight: 400; } p { font-size: 14px; } table { margin: 0px auto; width: 100%; margin-top: 10px; } th { margin-right: 1px; padding: 8px; font-size: 16px; } td { margin-right: 1px; padding: 8px; line-height: 20px; font-size: 14px; } .logo img { width: 300px; } .calendar { width: 100%; } .calendar table { margin-bottom: 25px; } .calendar .calendar-column { width: 50%; vertical-align: top; } .full { width: 100%; } .full td { padding: 0px; } .center { text-align: center; } .left { text-align: left; } .left-margin { margin-left: 4%; } .bg-maroon { background-color: #671130; color: #fff; } .bg-orange { background-color: #f18700; } .bg-white { background-color: #fff; } .bg-black { background-color: #333; } .maroon { color: #671130; } .orange { color: #f18700; } .white { color: #fff; } .footer-calendar { width: 100%; height: auto; overflow: hidden; margin-bottom: 8px; text-align: left; font-weight: 600; } .footer-calendar .date { width: 15%; float: left; text-align: center; } .footer-calendar .role { width: 23%; margin-right: 2%; float: left; } .footer-calendar .event { width: 60%; float: left; } </style></head>";
            const string section1 = "<body><div id='wrapper'><table>";
            const string section2 = "</table>";
            const string section3 = "</div></body>";

            var builder = new StringBuilder();
            builder.Append("<!DOCTYPE html><html>");
            builder.Append(sectionHead);
            builder.Append(section1);

            //school logo
            builder.AppendFormat(null,
                "<td colspan='2' rowspan='6' class='logo'><img src='data:image/jpeg;base64,{0}'></td>",
                logoBase64);

            //calendar date range
            builder.AppendFormat(null,
                "<td colspan='4' rowspan='2'><h1>{0}</h1></td>",
                GetExperience.ExperienceName);

            builder.Append(section2);
            builder.Append("<br><br><br>");
            builder.Append(section3);
            builder.Append(htmlExperience);
            builder.Append("<br><br>");
            builder.Append(htmlEvidance);
            builder.Append(section3);

            return builder.ToString();
        }

        #endregion
    }
}
