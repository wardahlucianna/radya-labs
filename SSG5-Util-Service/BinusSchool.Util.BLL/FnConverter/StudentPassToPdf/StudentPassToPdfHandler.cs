using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Util.FnConverter.StudentPassToPdf;
using BinusSchool.Util.FnConverter.StudentPassToPdf.validator;
using EasyCaching.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.StudentPassToPdf
{
    public class StudentPassToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly IStorageManager _storageManager;
        private readonly ISchool _schoolService;
        private readonly IAttendanceAdministrationV2 _attendanceAdministrationV2Api;
        public StudentPassToPdfHandler(IConverter converter, IEasyCachingProvider inMemoryCahce, IStorageManager storageManager,
            ISchool schoolService, IAttendanceAdministrationV2 attendanceAdministrationV2Api)
        {
            _converter = converter;
            _inMemoryCache = inMemoryCahce;
            _storageManager = storageManager;
            _schoolService = schoolService;
            _attendanceAdministrationV2Api = attendanceAdministrationV2Api;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var body = await Request.ValidateBody<StudentPassToPdfRequest, StudentPassToPdfValidator>();

            var schoolResult = default(GetSchoolDetailResult);
            var schoolResultKey = $"{nameof(schoolResult)}_{body.IdSchool}";
            if (!_inMemoryCache.TryGetCacheValue(schoolResultKey, out schoolResult))
            {
                var result = await _schoolService.GetSchoolDetail(body.IdSchool);
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

                    // generate SAS uri with expire time in 10 minutes
                    var sasUri = GenerateSasUri(blobClientLogo);

                    using var client = new HttpClient();
                    logo = await client.GetImageAsBase64(sasUri.AbsoluteUri);

                    await _inMemoryCache.SetAsync(logoKey, logo, TimeSpan.FromMinutes(10));
                }
            }

            GetAttendanceAdministrationDetailV2Result dataResult = new GetAttendanceAdministrationDetailV2Result();
            var ExperienceKey = $"{nameof(dataResult)}_{body.IdStudent}";
            if (!_inMemoryCache.TryGetCacheValue(ExperienceKey, out dataResult))
            {
                var result = await _attendanceAdministrationV2Api.GetAttendancAdministrationDetailV2(body.Id);
                dataResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(ExperienceKey, dataResult, TimeSpan.FromMinutes(10));
            }

            StudentPassToPdfResult fileResult = new StudentPassToPdfResult();
            if(dataResult != null)
            {
                var htmlStudent = BuildStudentPass(dataResult, body, logo);
                var htmlGenerated = BuildFullStudentPass(dataResult, htmlStudent);

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
                var blobName = $"Student-Pass-{Guid.NewGuid().ToString()}.pdf";
                var blobContainer = await _storageManager.GetOrCreateBlobContainer("student-pass", ct: CancellationToken);
                var blobClient = blobContainer.GetBlobClient(blobName);

                var blobResult = await blobClient.UploadAsync(new BinaryData(bytes), overwrite: true, CancellationToken);
                var rawBlobResult = blobResult.GetRawResponse();

                if (!(rawBlobResult.Status == StatusCodes.Status200OK || rawBlobResult.Status == StatusCodes.Status201Created))
                    throw new Exception(rawBlobResult.ReasonPhrase);

                // generate SAS uri with expire time in 10 minutes
                var sasUri = GenerateSasUri(blobClient);

                fileResult = new StudentPassToPdfResult
                {
                    RealFileName = $"Student-Pass-{dataResult.Student.Code}.pdf",
                    FileName = blobName,
                    ContentType = "application/pdf",
                    Location = sasUri
                };
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

        private string BuildStudentPass(GetAttendanceAdministrationDetailV2Result data, StudentPassToPdfRequest request, string logoBase64)
        {
            var builder = new StringBuilder();
            builder.Append("<table border='0'>");

            //school logo
            builder.AppendFormat(null,
                @"<tr>
                    <td class='logo'><img src='data:image/jpeg;base64,{0}'></td>",
                logoBase64);

            builder.AppendFormat(null,
                @"<td style='width: 100%;text-decoration: underline'><h1 style='margin-left: 150px;'>STUDENT PASS</h1></td>
                </tr>");

            builder.Append("</table>");
            builder.Append("</br></br>");

            builder.Append("<table border='0'>");
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Student Name</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          data.Student.Code + " - " + data.Homeroom.Description);
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Date</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          data.StartDate.ToString("dd MMMM yyyy") +" - "+ data.EndDate.ToString("dd MMMM yyyy"));
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Time</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          data.StartDate.ToString("HH:mm") + " - " + data.EndDate.ToString("HH:mm"));
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 30%;text-align: left;'>Detail Status</td>
                                <td style='width: 2%;text-align: left;'>:</td>
                                <td style='width: 70%;text-align: left;'>{0}</td>
                            </tr>",
                          data.Reason);
            //builder.AppendFormat(null,
            //              @"<tr>
            //                    <td style='width: 30%;text-align: left;'>Reason</td>
            //                    <td style='width: 2%;text-align: left;'>:</td>
            //                    <td style='width: 70%;text-align: left;'>{0}</td>
            //                </tr>",
            //              data.Attendance.Description);
            builder.Append("</table>");

            builder.Append("</br></br></br></br>");
            builder.Append("<table border='0'>");
            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: center;'>Printed by,</td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: center;'>Approved/Received by,</td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>");

            builder.AppendFormat(null,
                         @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>");

            builder.AppendFormat(null,
                        @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>");

            builder.AppendFormat(null,
                        @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>");

            builder.AppendFormat(null,
                        @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>");

            builder.AppendFormat(null,
                        @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: left;'></td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>");

            builder.AppendFormat(null,
                          @"<tr>
                                <td style='width: 10%;text-align: left;'></td>
                                <td style='width: 30%;text-align: center;border-bottom: 1px solid black'>{0}</td>
                                <td style='width: 20%;text-align: left;'></td>
                                <td style='width: 30%;text-align: center;border-bottom: 1px solid black'></td>
                                <td style='width: 10%;text-align: left;'></td>
                            </tr>",
                          request.NameUser);
            builder.Append("</table>");
            return builder.ToString();
        }

        private string BuildFullStudentPass(GetAttendanceAdministrationDetailV2Result data, string htmlData)
        {
            const string sectionHead = "<head> <meta charset='utf-8'> <meta http-equiv='X-UA-Compatible' content='IE=edge'> <title>Student Pass</title> <meta name='viewport' content='width=device-width, initial-scale=1'> <style> body { font-family: Arial; } #wrapper { width: 900px; margin: 0px auto; padding-top: 10px; text-align: left; } h1 { margin-top: 40px; margin-bottom: -10px; color: #333; font-size: 30px; } h2 { margin-bottom: 10px; color: #671130; text-align: right; font-size: 22px; margin-top: 28px; } h3 { font-size: 18px; font-weight: 400; } p { font-size: 14px; } table { margin: 0px auto; width: 100%; margin-top: 10px; } th { margin-right: 1px; padding: 8px; font-size: 16px; } td { margin-right: 1px; padding: 8px; line-height: 20px; font-size: 14px; } .logo img { width: 150px; display: block;margin: 0 auto;} .calendar { width: 100%; } .calendar table { margin-bottom: 25px; } .calendar .calendar-column { width: 50%; vertical-align: top; } .full { width: 100%; } .full td { padding: 0px; } .center { text-align: center; } .left { text-align: left; } .left-margin { margin-left: 4%; } .bg-maroon { background-color: #671130; color: #fff; } .bg-orange { background-color: #f18700; } .bg-white { background-color: #fff; } .bg-black { background-color: #333; } .maroon { color: #671130; } .orange { color: #f18700; } .white { color: #fff; } .footer-calendar { width: 100%; height: auto; overflow: hidden; margin-bottom: 8px; text-align: left; font-weight: 600; } .footer-calendar .date { width: 15%; float: left; text-align: center; } .footer-calendar .role { width: 23%; margin-right: 2%; float: left; } .footer-calendar .event { width: 60%; float: left; } </style></head>";
            const string section1 = "<body><div id='wrapper'>";
            const string section2 = "</div></body>";

            var builder = new StringBuilder();
            builder.Append("<!DOCTYPE html><html>");
            builder.Append(sectionHead);
            builder.Append(section1);
            builder.Append(htmlData);
            builder.Append(section2);

            return builder.ToString();
        }

        #endregion
    }
}
