using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.School.FnSchool.AcademicYear;
using BinusSchool.Data.Model.School.FnSchool.Level;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using EasyCaching.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;
using CalendarEventTypeVm = BinusSchool.Data.Model.Scheduling.FnSchedule.CalendarEvent.CalendarEventTypeVm;

namespace BinusSchool.Util.FnConverter.CalendarEventToPdf
{
    public class ConvertCalendarEventToPdfHandler : FunctionsHttpSingleHandler
    {
        private static readonly IEnumerable<DayOfWeek> _days = Enum.GetValues(typeof(DayOfWeek)).Cast<DayOfWeek>();

        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly IStorageManager _storageManager;
        private readonly ISchool _schoolService;
        private readonly IAcademicYear _ayService;
        private readonly ILevel _levelService;
        private readonly IEventSchool _schoolEvent;

        public ConvertCalendarEventToPdfHandler(IConverter converter, IEasyCachingProvider inMemoryCahce, IStorageManager storageManager,
            ISchool schoolService, IAcademicYear ayService, ILevel levelService, IEventSchool schoolEvent)
        {
            _converter = converter;
            _inMemoryCache = inMemoryCahce;
            _storageManager = storageManager;
            _schoolService = schoolService;
            _ayService = ayService;
            _levelService = levelService;
            _schoolEvent = schoolEvent;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = Request.ValidateParams<ConvertCalendarEventToPdfRequest>(nameof(ConvertCalendarEventToPdfRequest.IdSchool),
                nameof(ConvertCalendarEventToPdfRequest.IdAcadyear), nameof(ConvertCalendarEventToPdfRequest.IdLevel));

            var schoolResult = default(GetSchoolDetailResult);
            var schoolResultKey = $"{nameof(schoolResult)}_{request.IdSchool}";
            if (!_inMemoryCache.TryGetCacheValue(schoolResultKey, out schoolResult))
            {
                var result = await _schoolService.GetSchoolDetail(request.IdSchool);
                schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(schoolResultKey, schoolResult, TimeSpan.FromMinutes(10));
            }

            var ayResult = default(GetAcadyearByRangeResult);
            var ayResultKey = $"{nameof(ayResult)}_{request.IdAcadyear}";
            if (!_inMemoryCache.TryGetCacheValue(ayResultKey, out ayResult))
            {
                var result = await _ayService.GetAcademicyearRange(request.IdAcadyear);
                ayResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(ayResultKey, ayResult, TimeSpan.FromMinutes(10));
            }

            var levelResult = default(GetLevelDetailResult);
            var levelResultKey = $"{nameof(levelResult)}_{request.IdLevel}";
            if (!_inMemoryCache.TryGetCacheValue(levelResultKey, out levelResult))
            {
                var result = await _levelService.GetLevelDetail(request.IdLevel);
                levelResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(levelResultKey, levelResult, TimeSpan.FromMinutes(10));
            }

            var eventResult = default(IReadOnlyList<GetSchoolEventAcademicResult>);
            var eventResultKey = $"{nameof(eventResult)}_{request.IdAcadyear}_{request.IdLevel}_{ayResult.StartDate:yyyyMMdd}_{ayResult.EndDate:yyyyMMdd}";
            if (!_inMemoryCache.TryGetCacheValue(eventResultKey, out eventResult))
            {
                var result = await _schoolEvent.GetSchoolEventOfAcademicCalendar(
                    new GetSchoolEventAcademicRequest
                    {
                        IdAcadyear = request.IdAcadyear,
                        IdLevel = request.IdLevel,
                        StartDate = ayResult.StartDate,
                        EndDate = ayResult.EndDate
                    });
                eventResult = result.IsSuccess ? result.Payload.ToList() : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(eventResultKey, eventResult, TimeSpan.FromMinutes(1));
            }

            var eventTypes = eventResult
                .GroupBy(x => x.EventType.Id)
                .Select(x => new CalendarEventTypeVm
                {
                    Id = x.Key, 
                    Code = x.FirstOrDefault(y => y.EventType.Code != null)?.EventType?.Code, 
                    Description = x.FirstOrDefault(y => y.EventType.Description != null)?.EventType?.Description,
                    Color = x.FirstOrDefault(y => y.EventType.Color != null)?.EventType?.Color
                })
                .ToArray();
            var allMonths = DateTimeUtil.ToEachMonth(ayResult.StartDate, ayResult.EndDate);

            var blobName = $"CalendarAcademic_{schoolResult.Name}_{ayResult.Code}_{levelResult.Code}.pdf";
            var blobContainer = await _storageManager.GetOrCreateBlobContainer("academic-calendar", ct: CancellationToken);
            var blobClient = blobContainer.GetBlobClient(blobName);

            var calendarAlreadyExist = await blobClient.ExistsAsync(CancellationToken);
            var calendarLastUpdate = default(DateTime);
            var fileResult = default(ConvertCalendarEventToPdfResult);

            if (calendarAlreadyExist.Value)
            {
                var calendarProps = await blobClient.GetPropertiesAsync(cancellationToken: CancellationToken);
                var calendarMetadata = calendarProps.Value.Metadata;
                
                if (calendarMetadata.TryGetValue("lastUpdate", out var lastUpdate))
                    DateTime.TryParse(lastUpdate, out calendarLastUpdate);
            }

            // return already generated if exist & no update on event
            if (calendarAlreadyExist.Value && calendarLastUpdate >= (eventResult.Count != 0 ? eventResult.Max(x => x.LastUpdate) : DateTime.MaxValue))
            {
                fileResult = new ConvertCalendarEventToPdfResult
                {
                    FileName = blobName,
                    ContentType = "application/pdf",
                    Location = GenerateSasUri(blobClient)
                };
                return new JsonResult(fileResult, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
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


            // TODO: need to rework generate html
            var eventTypesHtml = BuildEventTypes(eventTypes);
            var eventHtml = BuildEvents(allMonths, eventResult);
            var htmlGenerated = BuildCalendarEvent(eventTypesHtml, eventHtml, ayResult.StartDate, ayResult.EndDate, levelResult.Description, logo);

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
            var bytes = _converter.Convert(doc);
            
            // save to storage
            var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, CancellationToken);
            var rawBlobResult = blobResult.GetRawResponse();

            if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                throw new Exception(rawBlobResult.ReasonPhrase);
            
            await blobClient.SetMetadataAsync(new Dictionary<string, string>
            {
                { "lastUpdate", eventResult.Count != 0 ? eventResult.Max(x => x.LastUpdate).ToString("O") : DateTime.MinValue.ToString("O") }
            }, cancellationToken: CancellationToken);

            // generate SAS uri with expire time in 10 minutes
            var sasUri = GenerateSasUri(blobClient);
            fileResult = new ConvertCalendarEventToPdfResult
            {
                FileName = blobName,
                ContentType = "application/pdf",
                Location = sasUri
            };
            
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

        private string BuildEventTypes(CalendarEventTypeVm[] eventTypes)
        {
            // eventTypes = Enumerable.Range(1, 7)
            //     .Select((x, i) => new CodeWithIdVm((i + 1).ToString()))
            //     .ToArray();
            var eventTypeMatrix = new CalendarEventTypeVm[(int)Math.Ceiling(eventTypes.Length / 4d), 4];
            int eventTypeMatrixRow = 0, eventTypeMatrixCol = 0;
            for (var i = 0; i < eventTypes.Length; i++)
            {
                eventTypeMatrix[eventTypeMatrixRow, eventTypeMatrixCol] = eventTypes[i];
                eventTypeMatrixCol++;
                
                if (i != 0 && (i + 1) % 4 == 0)
                {
                    eventTypeMatrixRow++;
                    eventTypeMatrixCol = 0;
                }
            }

            const string trStart = "<tr class='center'>";
            const string trEnd = "</tr>";

            var builder = new StringBuilder();
            for (var i = 0; i < eventTypeMatrix.GetLength(0); i++)
            {
                builder.Append(trStart);
                for (var j = 0; j < eventTypeMatrix.GetLength(1); j++)
                {
                    if (eventTypeMatrix[i, j] != null)
                    {
                        builder.AppendFormat(null, 
                            "<th style='background-color: {0};width: 25%;'>{1}</th>",
                            eventTypeMatrix[i, j].Color,
                            eventTypeMatrix[i, j].Code);
                    }
                    else
                        break;

                    // add additional <th> as separator each event type
                    if (j < 3 && eventTypeMatrix[i, j] != null && eventTypeMatrix[i, j + 1] != null)
                        builder.Append("<th></th>");
                }
                builder.Append(trEnd);
            }

            return builder.ToString();
        }

        private string BuildEvents(IEnumerable<DateTime> months, IEnumerable<GetSchoolEventAcademicResult> events)
        {
            const string monthStart = "<td class='calendar-column'><table class='bg-black center'><thead class='bg-maroon'><tr><th colspan='7'>{0}</th></tr></thead><tbody><tr class='bg-maroon'><td>S </td><td>M </td><td>T </td><td>W </td><td>T </td><td>F </td><td>S </td></tr>";
            const string monthEnd = "</tbody></table></td>";

            var builder = new StringBuilder();
            builder.Append("<div class='calendar'>");

            var eachEvents = events
                .SelectMany(x => x.Dates.Select(y => (x.Id, x.Name, y.Start, y.End, x.EventType, x.IntendedFor)))
                .ToArray();
            
            #region Dummy Events

            // var rand = new Random();
            // eachEvents = Enumerable.Range(1, 100)
            //     .Select(x => (
            //         Id: x.ToString(), 
            //         Name: $"Event {x}",
            //         Start: new DateTime(2020, rand.Next(1, 12), rand.Next(1, 2), rand.Next(24), rand.Next(60), 0),
            //         End: new DateTime(2021, rand.Next(1, 12), rand.Next(1, 2), rand.Next(24), rand.Next(60), 0),
            //         EventType: new CalendarEventTypeVm
            //         {
            //             Id = Guid.Empty.ToString(),
            //             Code = "Yes",
            //             Color = "#abcdef"
            //         },
            //         IntendedFor: "ALL")
            //     );

            #endregion

            // create table for each month
            var monthCount = 0;
            foreach (var month in months)
            {
                monthCount++;
                var daysInMonth = DateTime.DaysInMonth(month.Year, month.Month);
                var firstDayOfMonth = month.DayOfWeek;
                var weeksInMonth = (int)Math.Ceiling(((int)firstDayOfMonth + daysInMonth) / 7d);
                var dayOfMonth = month;
                var shouldRenderDate = false;

                // select event per month
                var eventsInMonth = eachEvents
                    .Where(x => DateTimeUtil.IsIntersect(x.Start, x.End, dayOfMonth, dayOfMonth.AddDays(daysInMonth - 1)))
                    .ToArray();

                // create new table each 2 month
                if (monthCount % 2 == 1)
                    builder.Append("<table><tr>");

                // render table month here
                builder.AppendFormat(null, monthStart, month.ToString("MMMM yyyy"));

                for (var i = 0; i < weeksInMonth; i++)
                {
                    builder.Append("<tr class='bg-white'>");
                    for (var j = 0; j < _days.Count(); j++)
                    {
                        // check first day of month with cureent day of week
                        // if match, than start render calendar
                        if (dayOfMonth.Day == 1 && _days.ElementAt(j) == firstDayOfMonth)
                            shouldRenderDate = true;

                        var colDay = "<td></td>";

                        // render date & schedule value here
                        if (shouldRenderDate)
                        {
                            var eventsInDay = eventsInMonth
                                .Where(x => DateTimeUtil.IsIntersect(x.Start, x.End, dayOfMonth, dayOfMonth.AddHours(24)))
                                .OrderBy(x => x.Start.Hour)
                                .ToArray();

                            colDay = eventsInDay.Length != 0
                                ? string.Format(null, "<td style='background-color: {0}'>{1}</td>", eventsInDay[0].EventType.Color, dayOfMonth.Day)
                                : string.Format("<td>{0}</td>", dayOfMonth.Day);
                        }

                        builder.Append(colDay);

                        // make sure render calendar not overflow with total days in month
                        if (shouldRenderDate && dayOfMonth.Day < daysInMonth)
                            dayOfMonth = dayOfMonth.AddDays(1);
                        else
                            shouldRenderDate = false;
                    }
                    builder.Append("</tr>");
                }

                // month legend
                if (eventsInMonth.Length != 0)
                {
                    builder.Append("<tr class='bg-white'><td colspan='7'>");
                    foreach (var (Id, Name, Start, End, EventType, IntendedFor) in eventsInMonth)
                    {
                        builder.AppendFormat(null,
                            "<div class='footer-calendar'><div class='date' style='color: {0}'>{1}</div><div class='role'>{2}</div><div class='event' style='color: {0}'>{3}</div></div>",
                            EventType.Color,
                            Start.Day == End.Day ? Start.Day.ToString() : $"{Start.Day} - {End.Day}",
                            IntendedFor,
                            Name);
                    }
                    builder.Append("</td></tr>");
                }

                builder.Append(monthEnd);

                // create new table each 2 month
                if (monthCount % 2 == 0)
                    builder.Append("</tr></table>");
            }

            builder.Append("</div>");

            return builder.ToString();
        }

        private string BuildCalendarEvent(string eventType, string @event, DateTime startDate, DateTime endDate, string level, string logoBase64)
        {
            const string sectionHead = "<head> <meta charset='utf-8'> <meta http-equiv='X-UA-Compatible' content='IE=edge'> <title>Academic Calendar</title> <meta name='viewport' content='width=device-width, initial-scale=1'> <style> body { font-family: Arial; } #wrapper { width: 900px; margin: 0px auto; padding-top: 10px; text-align: left; } h1 { margin-top: 40px; margin-bottom: -10px; text-align: right; color: #333; font-size: 30px; } h2 { margin-bottom: 10px; color: #671130; text-align: right; font-size: 22px; margin-top: 28px; } h3 { font-size: 18px; font-weight: 400; } p { font-size: 14px; } table { margin: 0px auto; width: 100%; margin-top: 10px; } th { margin-right: 1px; padding: 8px; font-size: 16px; } td { margin-right: 1px; padding: 8px; line-height: 20px; font-size: 14px; } .logo img { width: 300px; } .calendar { width: 100%; } .calendar table { margin-bottom: 25px; } .calendar .calendar-column { width: 50%; vertical-align: top; } .full { width: 100%; } .full td { padding: 0px; } .center { text-align: center; } .left { text-align: left; } .left-margin { margin-left: 4%; } .bg-maroon { background-color: #671130; color: #fff; } .bg-orange { background-color: #f18700; } .bg-white { background-color: #fff; } .bg-black { background-color: #333; } .maroon { color: #671130; } .orange { color: #f18700; } .white { color: #fff; } .footer-calendar { width: 100%; height: auto; overflow: hidden; margin-bottom: 8px; text-align: left; font-weight: 600; } .footer-calendar .date { width: 15%; float: left; text-align: center; } .footer-calendar .role { width: 23%; margin-right: 2%; float: left; } .footer-calendar .event { width: 60%; float: left; } </style></head>";
            const string sectionFooter = "<table class='full'> <tr> <td> <p>Note :<br /> <b>Please be informed that some scheduled dates may change throughout the years. Any changes to the calendar will be notified</b> </p> </td> </tr> </table>";
            const string section1 = "<body><div id='wrapper'><table><tr>";
            const string section2 = "</tr></table><table>";
            const string section3 = "</table>";

            var builder = new StringBuilder();
            builder.Append("<!DOCTYPE html><html>");
            builder.Append(sectionHead);
            builder.Append(section1);

            // school logo
            builder.AppendFormat(null,
                "<td colspan='2' rowspan='6' class='logo'><img src='data:image/jpeg;base64,{0}'></td>",
                logoBase64);

            // calendar date range
            builder.AppendFormat(null,
                "<td colspan='4' rowspan='2'><h1>ACADEMIC CALENDAR</h1><h2>{0}</h2><h2 style='margin-top: 14px;'>{1} to {2}</h2></td>",
                level, startDate.ToString("MMMM yyyy"), endDate.ToString("MMMM yyyy"));

            builder.Append(section2);

            // event types
            builder.Append(eventType);

            builder.Append(section3);

            // last update
            builder.AppendFormat(null,
                "<h3>Last Updated : <b>{0}</b></h3>",
                DateTimeUtil.ServerTime.ToString("dd MMMM yyyy"));

            // event months
            builder.Append(@event);
            
            builder.Append(sectionFooter);
            builder.Append("</body></html>");

            return builder.ToString();
        }

        #endregion
    }
}
