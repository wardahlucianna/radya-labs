using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Scoring.FnScoring;
using BinusSchool.Data.Model.School.FnSchool.School;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumEntry;
using BinusSchool.Data.Model.Scoring.FnScoring.LearningContinuum.LearningContinuumSummary;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.Util.FnConverter.LearningContinuumToPdf;
using BinusSchool.Util.FnConverter.LearningContinuumToPdf.Validator;
using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;
using NPOI.OpenXmlFormats;
using NPOI.XWPF.UserModel;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace BinusSchool.Util.FnConverter.LearningContinuumToPdf
{
    public class ConvertLearningContinuumToPdfHandler : FunctionsHttpSingleHandler
    {
        private readonly IConverter _converter;
        private readonly IEasyCachingProvider _inMemoryCache;
        private readonly ISchool _schoolService;
        private readonly IStorageManager _storageManager;
        private readonly ILearningContinuum _learningContinuum;

        public ConvertLearningContinuumToPdfHandler
        (
            IConverter converter, 
            IEasyCachingProvider inMemoryCache, 
            ISchool schoolService, 
            IStorageManager storageManager,
            ILearningContinuum learningContinuum
        )
        {
            _converter = converter;
            _inMemoryCache = inMemoryCache;
            _schoolService = schoolService;
            _storageManager = storageManager;
            _learningContinuum = learningContinuum;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ConvertLearningContinuumToPdfRequest, ConvertLearningContinuumToPdfValidator>();

            var schoolResult = default(GetSchoolDetailResult);
            var schoolResultKey = $"{nameof(schoolResult)}_{param.IdSchool}";
            if (!_inMemoryCache.TryGetCacheValue(schoolResultKey, out schoolResult))
            {
                var result = await _schoolService.GetSchoolDetail(param.IdSchool);
                schoolResult = result.IsSuccess ? result.Payload : throw new Exception(result.Message);

                await _inMemoryCache.SetAsync(schoolResultKey, schoolResult, TimeSpan.FromMinutes(10));
            }

            var htmlOutput = "";

            var getLOCItems = await _learningContinuum.GetLOCItem(new GetLOCItemRequest
            {
                IdAcademicYear = param.AcademicYear.Id,
                IdSubjectContinuum = param.SubjectContinuum.Id,
                IdGrade = param.Grade.Id,
            });
            var getLOCItemsData = getLOCItems.Payload;

            var detailStudentData = new ExportLearningContinuumToPdfResult_Data();
            if(param.Entry != null)
            {
                var getDetail = await _learningContinuum.GetLearningContinuumEntryDetailByStudent(new GetLearningContinuumEntryDetailByStudentRequest
                {
                    IdAcademicYear = param.AcademicYear.Id,
                    IdSubjectContinuum = param.SubjectContinuum.Id,
                    IdGrade = param.Grade.Id,
                    IdStudent = param.Student.Id,
                    IdSchool = param.IdSchool,
                    IdClass = param.Entry.IdClass,
                    IdHomeroom = param.Entry.IdHomeroom,
                    IdUser = param.Entry.IdUser,  
                    Semester = param.Semester,
                });

                var getDetailData = getDetail.Payload;

                detailStudentData.NextStudent = getDetailData.NextStudent;
                detailStudentData.PrevStudent = getDetailData.PrevStudent;
                detailStudentData.LastSavedDate = getDetailData.LastSavedDate;
                detailStudentData.LastSavedBy = getDetailData.LastSavedBy;
                detailStudentData.IdLearningContinuumList = getDetailData.IdLearningContinuumList;
            }
            else
            {
                var getDetail = await _learningContinuum.GetLearningContinuumSummaryDetailByStudent(new GetLearningContinuumSummaryDetailByStudentRequest
                {
                    IdSubjectContinuum = param.SubjectContinuum.Id,
                    IdGrade = param.Grade.Id,
                    IdStudent = param.Student.Id,
                    FilterSemester = param.Summary.FilterSemester,
                    IdFilterAcademicYear = param.Summary.IdFilterAcademicYear,
                    IdFilterGrade = param.Summary.IdFilterGrade,
                    IdFilterHomeroom = param.Summary.IdFilterHomeroom,
                    IdFilterLevel = param.Summary.IdFilterLevel,
                    IdFilterSubjectContinuum = param.Summary.IdFilterSubjectContinuum
                });

                var getDetailData = getDetail.Payload;

                detailStudentData.NextStudent = new ItemValueVm()
                {
                    Id = getDetailData.NextStudent.Id,
                    Description = getDetailData.NextStudent.Description
                };
                detailStudentData.PrevStudent = new ItemValueVm()
                {
                    Id = getDetailData.PrevStudent.Id,
                    Description = getDetailData.PrevStudent.Description
                };
                detailStudentData.LastSavedDate = getDetailData.LastSavedDate;
                detailStudentData.LastSavedBy = getDetailData.LastSavedBy;
                detailStudentData.IdLearningContinuumList = getDetailData.IdLearningContinuumList;
            }

            htmlOutput = await convertDataToHtml(getLOCItemsData, detailStudentData, param);

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
                        HtmlContent = htmlOutput,
                        WebSettings = { DefaultEncoding = "utf-8" }
                    }
                }
            };
            var bytes = _converter.Convert(doc);

            return new FileContentResult(bytes, "application/pdf")
            {
                FileDownloadName = $"Learning_Continuum_{param.Student.Id}_{DateTime.Now.Ticks}.pdf"
            };
        }

        protected async Task<string> convertDataToHtml(GetLOCItemResult LOCItems, ExportLearningContinuumToPdfResult_Data studentDetail, ConvertLearningContinuumToPdfRequest param)
        {

            string result = "<!DOCTYPE html><html lang='en'><head><meta charset='UTF-8'><meta name='viewport' content='widtd=device-widtd, initial-scale=1.0'>" +
                "<title>Learning Continuum</title><style>body {font: 16px Arial;letter-spacing: 1px;}table {width: 100%;border-collapse: collapse;}td {padding: 8px;padding-left: 0;text-align: left;}.filter {margin-bottom: 20px;}" +
                ".filter tbody tr, .filter tbody td{border: none;}.period {margin-top: 20px;}.lastSaved {margin-top: 10px;}.loc {margin-top: 20px;page-break-inside: avoid;}" +
                ".head {background-color: #e1e1ef;color: #028ED5;font-weight: bold;padding: 8px;}.locPhase-container {margin-left: 20px;} table.loc tr td {\r\n  padding: 10px;\r\n}\r\n\r\ntable.locTypeSection tr td {\r\n  padding: 0;\r\n}</style>" +
                "</head><body><table class='filter'><tbody><tr><td style='width: 300px;'>Academic Year</td><td>: {{IdAcademicYear}}</td></tr><tr><td>Student ID</td><td>: {{IdStudent}}</td></tr><tr><td>Student Name</td><td>: {{StudentName}}</td></tr>" +
                "<tr><td>Grade</td><td>: {{Grade}}</td></tr><tr><td>Subject Continuum</td><td>: {{SubjectName}}</td></tr>  <tr><td>Learning Continuum End Period</td><td>: {{PeriodEnd}}</td></tr><tr><td>Last Updated</td><td>: {{DateUp}} by {{UserUp}}</td></tr><tr>\r\n  <td>Date Generated</td>\r\n  <td>: {{DateTimeGenerated}} </td>\r\n</tr></tbody></table>" +
                "{{ListLOC}}</body></html>";

            var dataCheckedByStudent = studentDetail.IdLearningContinuumList;

            var locList = LOCItems.LOCList;

            var typelistData = "";

            foreach (var type in locList)
            {
                var categories = type.LearningContinuumCategories;

                var categoriesListTemplate = "";

                int totalItems = 0;
                var checkedItems = 0;

                foreach (var category in categories)
                {
                    var phases = category.Phases;

                    var phasesListTemplate = "";

                    foreach (var phase in phases)
                    {

                        var Items = phase.LOCItems;

                        var locItemsData = "";
                        
                        totalItems += Items.Count;

                        foreach (var item in Items)
                        {

                            var locTemplate = "<tr><td style='vertical-align: top;'>{{IsChecked}}</td><td><span class='locItem'>{{LOCDescription}}</span></td></tr>";
                            bool isChecked = dataCheckedByStudent.Contains(item.IdLearningContinuum);

                            if (isChecked)
                            {
                                checkedItems++;
                            }

                            locTemplate = locTemplate.Replace("{{IsChecked}}", isChecked ?
                                "<svg width=\"18px\" height=\"18px\" viewBox=\"0 0 24 24\">\r\n    <g>\r\n        <g fill=\"#212121\">\r\n            <path d=\"M18.25,3 C19.7687831,3 21,4.23121694 21,5.75 L21,18.25 C21,19.7687831 19.7687831,21 18.25,21 L5.75,21 C4.23121694,21 3,19.7687831 3,18.25 L3,5.75 C3,4.23121694 4.23121694,3 5.75,3 L18.25,3 Z M18.25,4.5 L5.75,4.5 C5.05964406,4.5 4.5,5.05964406 4.5,5.75 L4.5,18.25 C4.5,18.9403559 5.05964406,19.5 5.75,19.5 L18.25,19.5 C18.9403559,19.5 19.5,18.9403559 19.5,18.25 L19.5,5.75 C19.5,5.05964406 18.9403559,4.5 18.25,4.5 Z M10,14.4393398 L16.4696699,7.96966991 C16.7625631,7.6767767 17.2374369,7.6767767 17.5303301,7.96966991 C17.7965966,8.23593648 17.8208027,8.65260016 17.6029482,8.94621165 L17.5303301,9.03033009 L10.5303301,16.0303301 C10.2640635,16.2965966 9.84739984,16.3208027 9.55378835,16.1029482 L9.46966991,16.0303301 L6.46966991,13.0303301 C6.1767767,12.7374369 6.1767767,12.2625631 6.46966991,11.9696699 C6.73593648,11.7034034 7.15260016,11.6791973 7.44621165,11.8970518 L7.53033009,11.9696699 L10,14.4393398 L16.4696699,7.96966991 L10,14.4393398 Z\"></path>\r\n        </g>\r\n    </g>\r\n</svg>"
                                :
                                "<svg width=\"18px\" height=\"18px\" viewBox=\"0 0 24 24\">\r\n    <g>\r\n        <g fill=\"#212121\">\r\n            <path d=\"M5.75,3 L18.25,3 C19.7687831,3 21,4.23121694 21,5.75 L21,18.25 C21,19.7687831 19.7687831,21 18.25,21 L5.75,21 C4.23121694,21 3,19.7687831 3,18.25 L3,5.75 C3,4.23121694 4.23121694,3 5.75,3 Z M5.75,4.5 C5.05964406,4.5 4.5,5.05964406 4.5,5.75 L4.5,18.25 C4.5,18.9403559 5.05964406,19.5 5.75,19.5 L18.25,19.5 C18.9403559,19.5 19.5,18.9403559 19.5,18.25 L19.5,5.75 C19.5,5.05964406 18.9403559,4.5 18.25,4.5 L5.75,4.5 Z\"></path>\r\n        </g>\r\n    </g>\r\n</svg>");
                            locTemplate = locTemplate.Replace("{{LOCDescription}}", item.Description);

                            locItemsData += locTemplate;
                        }

                        var phasesTemplate = "<div class='locPhase-container'> <div class='locPhase'>Phase {{Phase}} - Learning Outcomes</div><table class='filter'><tbody>{{LocItemsData}}</tbody></table></div>";

                        phasesTemplate = phasesTemplate.Replace("{{Phase}}", phase.Phase.ToString());
                        phasesTemplate = phasesTemplate.Replace("{{LocItemsData}}", locItemsData);

                        phasesListTemplate += phasesTemplate;
                    }

                    var categoryTemplate = "<tr> <td class='locCategory'>{{CategoryName}}</td></tr><tr><td>{{Phases}}</td></tr>";

                    categoryTemplate = categoryTemplate.Replace("{{CategoryName}}", category.Description);
                    categoryTemplate = categoryTemplate.Replace("{{Phases}}", phasesListTemplate);

                    categoriesListTemplate += categoryTemplate;
                }

                var typeTemplate = "<table class='loc'><tbody><tr><td class='head locType'><table class=\"locTypeSection\">\r\n    <tbody>\r\n      <tr>\r\n        <td>Learning Continuum for {{TypeDescription}}</td>\r\n        <td style=\"float: inline-end;\">{{totalChecked}} out of {{totalItems}}</td>\r\n      </tr>\r\n    </tbody>\r\n  </table></td></tr>{{Categories}}</tbody></table>";

                typeTemplate = typeTemplate.Replace("{{TypeDescription}}", type.LearningContinuumType.Description);
                typeTemplate = typeTemplate.Replace("{{Categories}}", categoriesListTemplate);
                typeTemplate = typeTemplate.Replace("{{totalItems}}", totalItems.ToString());
                typeTemplate = typeTemplate.Replace("{{totalChecked}}", checkedItems.ToString());

                typelistData += typeTemplate;
            }

            result = result.Replace("{{IdAcademicYear}}", param.AcademicYear.Description);
            result = result.Replace("{{IdStudent}}", param.Student.Id);
            result = result.Replace("{{StudentName}}", param.Student.Description);
            result = result.Replace("{{Grade}}", param.Grade.Description);
            result = result.Replace("{{SubjectName}}", param.SubjectContinuum.Description);
            result = result.Replace("{{PeriodEnd}}", LOCItems.PeriodEndDate != null ? ((DateTime) LOCItems.PeriodEndDate).ToString("dd MMM yyyy HH:mm") : "");
            result = result.Replace("{{DateUp}}",  studentDetail.LastSavedDate != null ? ((DateTime) studentDetail.LastSavedDate).ToString("dd MMM yyyy HH:mm") : "NO DATA SAVED");
            result = result.Replace("{{UserUp}}", studentDetail.LastSavedBy);
            result = result.Replace("{{DateTimeGenerated}}", DateTime.Now.AddHours(7).ToString("dd MMM yyyy HH:mm"));

            result = result.Replace("{{ListLOC}}", typelistData);

            return result;
        }
    }
}
