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
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
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
    public class TimelineToPdfHandler : FunctionsHttpSingleHandler
    {
        private static readonly IEnumerable<DayOfWeek> _days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly IStorageManager _storageManager;
        private readonly ISchool _schoolService;
        private readonly IEventSchool _schoolEvent;
        private readonly ICreativityActivityService _creativityActivityService;

        public TimelineToPdfHandler(IConverter converter, IEasyCachingProvider inMemoryCahce, IStorageManager storageManager,
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
            var request = Request.ValidateParams<TimelineToPdfRequest>();

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


            var TimelineResult = default(GetTimelineBySupervisorResult);
            var TimelineKey = $"{nameof(TimelineResult)}_{request.IdUser}_{request.IdStudent}_{request.IdAcademicYear}";
            if (!_inMemoryCache.TryGetCacheValue(TimelineKey, out TimelineResult))
            {
                    var result = await _creativityActivityService.GetTimelineToPdf(new GetTimelineToPdfRequest
                    {
                        IdUser = request.IdUser,
                        IdStudent = request.IdStudent,
                        IdAcademicYear = request.IdAcademicYear,
                        Role = request.Role,
                    });
                    TimelineResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(TimelineKey, TimelineResult, TimeSpan.FromMinutes(10));
            }

            var htmlHeaderRow1 = BuildHeaderRow1(TimelineResult);
            var htmlHeaderRow2 = BuildHeaderRow2(TimelineResult);
            var htmlBody = BuildBody(TimelineResult);
            var htmlGenerated = BuildTimeline(htmlHeaderRow1, htmlHeaderRow2, htmlBody, logo);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Landscape,
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
            var blobName = $"Timeline-{Guid.NewGuid().ToString()}.pdf";
            var blobContainer = await _storageManager.GetOrCreateBlobContainer("creativity-activity-service", ct: CancellationToken);
            var blobClient = blobContainer.GetBlobClient(blobName);

            var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, CancellationToken);
            var rawBlobResult = blobResult.GetRawResponse();

            if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                throw new Exception(rawBlobResult.ReasonPhrase);

            // generate SAS uri with expire time in 10 minutes
            var sasUri = GenerateSasUri(blobClient);
            var fileResult = default(ConvertExperienceToPdfResult);
            fileResult= new ConvertExperienceToPdfResult
            {
                RealFileName = $"Timeline-{TimelineResult.Student}.pdf",
                FileName = blobName,
                ContentType = "application/pdf",
                Location = sasUri
            };
            #endregion
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

        private string BuildHeaderRow1(GetTimelineBySupervisorResult GetTimeline)
        {
            var builder = new StringBuilder();
            var Header = "";
            builder.Append("<tr>");
            builder.Append("<th style='width: 20%;text-align: center;' rowspan='2' >Activity</th>");
            foreach (var ItemHeader in GetTimeline.Headers)
            {
                if (Header != ItemHeader.Year.ToString())
                {
                    Header = ItemHeader.Year.ToString();
                    var Colspan = GetTimeline.Headers.Where(e => e.Year == ItemHeader.Year).Count();
                    builder.AppendFormat(null,
                           "<th style='width: 5%;text-align: center;' colspan='{0}'>{1}</th>",
                           Colspan,
                           ItemHeader.Year.ToString());
                }
            }
            builder.Append("</tr>");
            return builder.ToString();
        }

        private string BuildHeaderRow2(GetTimelineBySupervisorResult GetTimeline)
        {
            var builder = new StringBuilder();
            builder.Append("<tr>");
            foreach (var ItemHeader in GetTimeline.Headers)
            {
                builder.AppendFormat(null,
                          "<th style='width: 5%;'>{0}</th>",
                          ItemHeader.Month);
            }
            builder.Append("</tr>");
            return builder.ToString();
        }

        private string BuildBody(GetTimelineBySupervisorResult GetTimeline)
        {
            var builder = new StringBuilder();
            foreach (var ItemBody in GetTimeline.Bodys)
            {
                builder.Append("<tr>");
                builder.AppendFormat(null,
                          "<td style='width: 20%;'>{0}</td>",
                          ItemBody.NameExperience);

                foreach (var ItemBodyDetail in ItemBody.Timeline)
                {
                    builder.AppendFormat(null,
                          "<td style='width: 5%;background:{0}'></td>",
                          ItemBodyDetail.IsChecked == true ? "red" : "");
                }
                builder.Append("</tr>");
            }
            return builder.ToString();
        }

        private string BuildTimeline(string htmlHeaderRow1, string htmlHeaderRow2, string htmlBody, string logoBase64)
        {
            const string sectionHead = "<head> <meta charset='utf-8'> <meta http-equiv='X-UA-Compatible' content='IE=edge'> <title>Academic Calendar</title> <meta name='viewport' content='width=device-width, initial-scale=1'> <style> body { font-family: Arial; } #wrapper { width: 900px; margin: 0px auto; padding-top: 10px; text-align: left; } h1 { margin-top: 40px; margin-bottom: -10px; text-align: right; color: #333; font-size: 30px; } h2 { margin-bottom: 10px; color: #671130; text-align: right; font-size: 22px; margin-top: 28px; } h3 { font-size: 18px; font-weight: 400; } p { font-size: 14px; } table { margin: 0px auto; width: 100%; margin-top: 10px; } th { margin-right: 1px; padding: 8px; font-size: 16px; } td { margin-right: 1px; padding: 8px; line-height: 20px; font-size: 14px; } .logo img { width: 300px; } .calendar { width: 100%; } .calendar table { margin-bottom: 25px; } .calendar .calendar-column { width: 50%; vertical-align: top; } .full { width: 100%; } .full td { padding: 0px; } .center { text-align: center; } .left { text-align: left; } .left-margin { margin-left: 4%; } .bg-maroon { background-color: #671130; color: #fff; } .bg-orange { background-color: #f18700; } .bg-white { background-color: #fff; } .bg-black { background-color: #333; } .maroon { color: #671130; } .orange { color: #f18700; } .white { color: #fff; } .footer-calendar { width: 100%; height: auto; overflow: hidden; margin-bottom: 8px; text-align: left; font-weight: 600; } .footer-calendar .date { width: 15%; float: left; text-align: center; } .footer-calendar .role { width: 23%; margin-right: 2%; float: left; } .footer-calendar .event { width: 60%; float: left; } </style></head>";
            const string section1 = "<body><div id='wrapper'><table>";
            const string section2 = "</table>";
            const string section3 = "<table border='1'>";
            const string section4 = "</table></div></body>";

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
                "<td colspan='4' rowspan='2'><h1>CREATIVITY, ACTIVITY & SERVICE</h1></td>",
                "1", "2", "3");

            builder.Append(section2);
            builder.Append("<br><br><br>");
            builder.Append(section3);
            builder.Append(htmlHeaderRow1);
            builder.Append(htmlHeaderRow2);
            builder.Append(htmlBody);
            builder.Append(section4);

            return builder.ToString();
        }

        #endregion
    }
}
