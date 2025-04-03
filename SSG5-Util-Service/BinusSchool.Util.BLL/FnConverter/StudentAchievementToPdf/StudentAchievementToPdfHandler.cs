using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Student.FnStudent.Achievement;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.User.FnAuth.UserPassword;
using BinusSchool.Data.Model.Util.FnConverter.CalendarEventToPdf;
using BinusSchool.Data.Model.Util.FnConverter.StudentAchivementToPdf;
using EasyCaching.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.StudentAchievementToPdf
{
    public class StudentAchievementToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly IStorageManager _storageManager;
        private readonly IAchievement _achievementService;
        private readonly ISchool _schoolService;
        private readonly IStudent _studentService;

        public StudentAchievementToPdfHandler(IConverter converter,
            IEasyCachingProvider inMemoryCache,
            IStorageManager storageManager,
            IAchievement achievementService,
            ISchool schoolService,
            IStudent studentService)
        {
            _converter = converter;
            _inMemoryCache = inMemoryCache;
            _storageManager = storageManager;
            _achievementService = achievementService;
            _schoolService = schoolService;
            _studentService = studentService;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new System.NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<StudentAchievementToPdfRequest>(nameof(StudentAchievementToPdfRequest.IdAcademicYear),
                nameof(StudentAchievementToPdfRequest.IdUser),
                nameof(StudentAchievementToPdfRequest.IdSchool));

            var studentDetailResult = new GetStudentDetailResult();
            var studentDetailResultKey = $"{nameof(studentDetailResult)}_{param.IdUser}";
            if (!_inMemoryCache.TryGetCacheValue(studentDetailResultKey, out studentDetailResult))
            {
                var result = await _studentService.GetStudentDetail(param.IdUser);
                studentDetailResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);
                await _inMemoryCache.SetAsync(studentDetailResultKey, studentDetailResult, TimeSpan.FromMinutes(10));
            }

            var achievementResult = new List<GetAchievementResult>();
            var achievementResultKey = $"{nameof(achievementResult)}_{param.IdUser}";
            if (!_inMemoryCache.TryGetCacheValue(achievementResultKey, out achievementResult))
            {
                var result = await _achievementService.GetAchievement(new GetAchievementRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = param.Semester,
                    Status = "Approved",
                    IdUser = param.IdUser,
                    Role = param.Role,
                    Type = param.Type,
                    Search = param.Search,
                    OrderBy = param.OrderBy,
                    OrderType = param.OrderType,
                    Return = Common.Model.Enums.CollectionType.Lov
                });
                achievementResult = result.IsSuccess ? result.Payload.ToList() : throw new System.Exception(result.Message);

                await _inMemoryCache.SetAsync(achievementResultKey, achievementResult, TimeSpan.FromMinutes(10));
            }

            var schoolResult = default(GetSchoolDetailResult);
            var schoolResultKey = $"{nameof(schoolResult)}_{param.IdSchool}";
            if (!_inMemoryCache.TryGetCacheValue(schoolResultKey, out schoolResult))
            {
                var result = await _schoolService.GetSchoolDetail(param.IdSchool);
                schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(schoolResultKey, schoolResult, TimeSpan.FromMinutes(10));
            }

            var blobName = $"StudentAchievement_{param.IdUser}.pdf";
            var blobContainer = await _storageManager.GetOrCreateBlobContainer("student-achievement", ct: CancellationToken);
            var blobClient = blobContainer.GetBlobClient(blobName);

            var achievementAlreadyExist = await blobClient.ExistsAsync(CancellationToken);
            var achievementLastUpdate = default(DateTime);
            var fileResult = default(StudentAchivementToPdfResult);

            if (achievementAlreadyExist.Value)
            {
                var achievementProps = await blobClient.GetPropertiesAsync(cancellationToken: CancellationToken);
                var achievementMetadata = achievementProps.Value.Metadata;

                if (achievementMetadata.TryGetValue("dateOfCompletion", out var lastUpdate))
                    DateTime.TryParse(lastUpdate, out achievementLastUpdate);
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

            var achievementCategories = achievementResult.Select(x => x.AchievementCategory).Distinct();

            var scoreSummaries = new List<ScoreSummariesResult>();
            foreach (var achievementCategory in achievementCategories)
            {
                scoreSummaries.Add(new ScoreSummariesResult
                {
                    AchievementCategory = achievementCategory,
                    Score = achievementResult.Where(x => x.AchievementCategory == achievementCategory).Sum(x => x.Point)
                });
            }

            scoreSummaries = scoreSummaries.OrderByDescending(x => x.Score).ToList();

            // TODO: need to rework generate html
            var htmlGenerated = BuildStudentAchievement(achievementResult, logo, param.Semester, scoreSummaries, studentDetailResult.NameInfo, param.IdUser, param.IdAcademicYear);

            var doc = new HtmlToPdfDocument()
            {
                GlobalSettings = {
                    ColorMode = ColorMode.Grayscale,
                    Orientation = Orientation.Landscape,
                    PaperSize = PaperKind.A4,
                    Outline = false,
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
                { "dateOfCompletion", achievementResult.Count != 0 ? achievementResult.Max(x => x.DateOfCompletion).ToString("O") : DateTime.MinValue.ToString("O") }
            }, cancellationToken: CancellationToken);

            // generate SAS uri with expire time in 10 minutes
            var sasUri = GenerateSasUri(blobClient);
            fileResult = new StudentAchivementToPdfResult
            {
                FileName = blobName,
                ContentType = "application/pdf",
                Location = sasUri
            };

            return new JsonResult(fileResult, SerializerSetting.GetJsonSerializer(Request.IsShowAll()));
        }

        private string BuildStudentAchievement(List<GetAchievementResult> achievementResult, string logo, int? semester, List<ScoreSummariesResult> scoreSummaries, Common.Model.Information.NameInfoVm studentNameInfo, string idUser, string idAcademicYear)
        {
            const string sectionHead = "<!DOCTYPE html>\r\n<html lang=\"en\" xmlns=\"http://www.w3.org/1999/html\">\r\n<head>\r\n    <meta charset=\"UTF-8\">\r\n    <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\r\n    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n    <title>SUMMARY DATA MERIT & ACHIEVEMENT</title>\r\n</head>";
            const string sectionStyle = "<style>\r\n    body {\r\n      margin: 0;\r\n      padding: 0;\r\n      background-color: #fafafa;\r\n    }\r\n\r\n    * {\r\n      box-sizing: border-box;\r\n      -moz-box-sizing: border-box;\r\n      border-collapse: collapse;\r\n    }\r\n\r\n    .page {\r\n      width: 29.7cm;\r\n      min-height: 21cm;\r\n      padding-bottom: 2cm;\r\n      padding-right: 1cm;\r\n      padding-left: 1cm;\r\n      padding-top: 3cm;\r\n      margin: 1cm auto;\r\n      border: 1px #d3d3d3 solid;\r\n      border-radius: 5px;\r\n      background: white;\r\n      box-shadow: 0 0 5px rgba(0, 0, 0, 0.1);\r\n    }\r\n\r\n    @page {\r\n      size: A4;\r\n      margin: 0;\r\n    }\r\n\r\n    @media print {\r\n      .page {\r\n        margin: 0;\r\n        border: initial;\r\n        border-radius: initial;\r\n        width: initial;\r\n        min-height: initial;\r\n        box-shadow: initial;\r\n        background: initial;\r\n        page-break-after: always;\r\n      }\r\n    }\r\n\r\n    .judul {\r\n      font: 15pt \"Times New Roman\";\r\n      font-weight: bold;\r\n      letter-spacing: 3px;\r\n    }\r\n\r\n    .isi {\r\n      margin-top: 30px;\r\n    }\r\n\r\n    p {\r\n      font-size: 11pt \"Times New Roman\";\r\n    }\r\n\r\n    .m-0 {\r\n      margin: 0px;\r\n    }\r\n\r\n    .row {\r\n      display: flex;\r\n      padding-bottom: 30px;\r\n    }\r\n\r\n    .bordering-table {\r\n      table-layout: fixed;\r\n      width: 100%;   \r\n    }\r\n\r\n    .bordering-table th {\r\n      border: 1px solid;\r\n      word-wrap: break-word;\r\n    }\r\n\r\n    .bordering-table td {\r\n      border: 1px solid;\r\n      word-wrap: break-word;\r\n    }\r\n  </style>";
            const string regexPattern = @"\d+";

            string student = $"{NameUtil.GenerateFullNameWithId(idUser, studentNameInfo.FirstName, studentNameInfo.LastName)}";
            string acadYear = string.Empty;
            if (Regex.IsMatch(idAcademicYear, regexPattern))
            {
                Regex rg = new Regex(regexPattern);
                int year = Convert.ToInt32(rg.Match(idAcademicYear).Value);
                acadYear = $"{year}-{year + 1}";
            }
            else
            {
                throw new Exception("Id Academic Year not valid");
            }

            var builder = new StringBuilder();
            builder.Append(sectionHead);
            builder.Append(sectionStyle);
            builder.AppendFormat(null, "<body>\r\n<input type=\"hidden\" value=\"IsFromDownloadPdfAntiXSSPrevent\" />\r\n    <div class=\"book\">\r\n        <div class=\"page\">\r\n            <div class=\"judul\">\r\n                <img src=\"data:image/png;base64, {0}\" class=\"logo-binus\" style=\"width: 100px;\">\r\n                <span style=\"padding-left: 200px; position: absolute; padding-top: 30px;\">SUMMARY DATA MERIT & ACHIEVEMENT</span>\r\n            </div>",
                logo);

            builder.Append("<div class=\"isi\">\r\n                <div class=\"row\">");
            builder.AppendFormat(null,
                "<table>\r\n" +
                    "<tbody>\r\n<tr>\r\n<td>\r\nStudent Name\r\n</td>\r\n<td style=\"padding-left: 25px;\">\r\n:\r\n</td>\r\n<td>\r\n{0}\r\n</td>\r\n</tr>" +
                    "\r\n<tr>\r\n<td>\r\nAcademic Year\r\n</td>\r\n<td style=\"padding-left: 25px;\">\r\n:\r\n</td>\r\n<td>\r\n{1}\r\n</td>\r\n</tr>\r\n" +
                    "<tr>\r\n<td>\r\nSemester\r\n</td>\r\n<td style=\"padding-left: 25px;\">\r\n:\r\n</td>\r\n<td>\r\n{2}\r\n</td>\r\n</tr>\r\n</tbody>\r\n</table>\r\n</div>",
                student, acadYear, semester == null ? "1 & 2" : semester.ToString());
            builder.Append("<div class=\"row\">\r\n<table class=\"bordering-table\">\r\n<thead>\r\n<tr>\r\n<th style=\"background-color: #e1dddd;\">Achievement Category</th>\r\n<th style=\"background-color: #e1dddd;\">Score</th>\r\n</tr>\r\n</thead>\r\n<tbody>");

            foreach (var scoreSummary in scoreSummaries)
            {
                builder.AppendFormat(null,
                    "<tr>\r\n<td>\r\n{0}\r\n</td>\r\n<td>\r\n{1}\r\n</td>\r\n</tr>",
                    scoreSummary.AchievementCategory, scoreSummary.Score.ToString());
            }

            builder.Append("</tbody>\r\n</table>\r\n</div>");
            builder.Append("</div>\r\n</div>\r\n</div>");

            builder.Append("<div class=\"book\">\r\n        <div class=\"page\">\r\n            <div class=\"isi\">\r\n                <div class=\"row\">\r\n                    <table class=\"bordering-table\">\r\n                        <thead>\r\n                            <tr>\r\n                                <th style=\"background-color: #e1dddd;\">Academic Year</th>\r\n                                <th style=\"background-color: #e1dddd;\">Semester</th>\r\n                                <th style=\"background-color: #e1dddd;\">Achievement Name</th>\r\n                                <th style=\"background-color: #e1dddd;\">Achievement Category</th>\r\n                                <th style=\"background-color: #e1dddd;\">Verifying Teacher</th>\r\n                                <th style=\"background-color: #e1dddd;\">Focus Area</th>\r\n                                <th style=\"background-color: #e1dddd;\">Date of Completion</th>\r\n                                <th style=\"background-color: #e1dddd;\">Point</th>\r\n                                <th style=\"background-color: #e1dddd;\">Created By</th>\r\n                                <th style=\"background-color: #e1dddd;\">Merit/Achievement</th>\r\n                                <th style=\"background-color: #e1dddd;\">Approval Notes</th>\r\n                                <th style=\"background-color: #e1dddd;\">Status</th>\r\n                            </tr>\r\n                        </thead>\r\n<tbody>");
            foreach (var achievement in achievementResult)
            {
                builder.AppendFormat(null,
                    "<tr>\r\n" +
                    "<td>{0}</td>\r\n" +
                    "<td>{1}</td>\r\n" +
                    "<td>{2}</td>\r\n" +
                    "<td>{3}</td>\r\n" +
                    "<td>{4}</td>\r\n" +
                    "<td>{5}</td>\r\n" +
                    "<td>{6}</td>\r\n" +
                    "<td>{7}</td>\r\n" +
                    "<td>{8}</td>\r\n" +
                    "<td>{9}</td>\r\n" +
                    "<td>{10}</td>\r\n" +
                    "<td>{11}</td>\r\n" +
                    "</tr>",
                    achievement.AcademicYear,
                    achievement.Semester,
                    achievement.AchievementName,
                    achievement.AchievementCategory,
                    achievement.VerifyingTeacher,
                    achievement.FocusArea,
                    achievement.DateOfCompletion.ToString("dd MMMM yyyy"),
                    achievement.Point,
                    achievement.CreateBy,
                    achievement.Type,
                    achievement.ApprovalNote,
                    achievement.Status);
            }
            builder.Append("</tbody>\r\n</table>\r\n</div>\r\n</div>\r\n</div>\r\n</div>");
            builder.Append("</body>\r\n</html>");

            return builder.ToString();
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.Add(TimeSpan.FromMinutes(5));

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }
    }
}
