using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterExtracurricular;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByLevel;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterExtracurricular
{
    public class ExportExcelMasterExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ExportExcelMasterExtracurricularHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<ExportExcelMasterExtracurricularRequest>();

            var paramDesc = await _dbContext.Entity<MsAcademicYear>()
                  .Include(x => x.School)
                  .Where(x => x.Id == param.IdAcademicYear)
                  .Where(x => x.School.Id == param.IdSchool)
                  .Select(x => new GetParameterDescriptionResult
                  {
                      School = x.School.Name,
                      AcademicYear = x.Description,
                      Semester = param.Semester,
                  })
                  .FirstOrDefaultAsync(CancellationToken);

            if (paramDesc == null)
            {
                throw new BadRequestException($"Data Not Found");
            }

            var masterExcul = await _dbContext.Entity<MsExtracurricular>()
                    .Include(x => x.ExtracurricularGroup)
                    .Include(x => x.ExtracurricularSpvCoach)
                        .ThenInclude(x => x.Staff)
                   .Include(x => x.ExtracurricularSpvCoach)
                        .ThenInclude(x => x.ExtracurricularCoachStatus)
                    .Include(x => x.ExtracurricularSessionMappings)
                        .ThenInclude(x => x.ExtracurricularSession)
                        .ThenInclude(x => x.Day)
                    .Include(x => x.ExtracurricularSessionMappings)
                        .ThenInclude(x => x.ExtracurricularSession)
                        .ThenInclude(x => x.Venue)
                    .Include(x => x.ExtracurricularGradeMappings)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.Level)
                        .ThenInclude(x => x.AcademicYear)
                    .Include(x => x.ExtracurricularExtCoachMappings)
                        .ThenInclude(y => y.ExtracurricularExternalCoach)
                    .Where(x => param.Status == null || x.Status == param.Status)
                    .Where(x => x.Semester == param.Semester)
                    .Where(x => x.ExtracurricularGradeMappings.Select(y => y.Grade.Level.AcademicYear.Id).Contains(param.IdAcademicYear))
                    .ToListAsync(CancellationToken);

            if (masterExcul.Count() == 0)
            {
                throw new BadRequestException($"Extracurricular Not Found");
            }

            var resultExcul = masterExcul.Select(x => new GetMasterExtracurricularforExportExcelResult
            {
                ExtracurricularName = x.Name,
                ExtracurricularGroup = x.ExtracurricularGroup.Name,
                Description = x.Description,
                Price = x.Price,
                Grade = x.ExtracurricularGradeMappings.Count != 0 // force order asc one-to-many relation
                            ? string.Join("; ", x.ExtracurricularGradeMappings
                                .OrderBy(y => y.Grade.Code)
                                .Select(y => y.Grade.Description))
                            : Localizer[""],
                ShowAttendance = x.ShowAttendanceRC == true ? "Yes" : "No",
                ShowScore = x.ShowScoreRC == true ? "Yes" : "No",
                Schedule = x.ExtracurricularSessionMappings.Count != 0 // force order asc one-to-many relation
                            ? (x.IsRegularSchedule == true ?
                                    string.Join(";", x.ExtracurricularSessionMappings
                                    .OrderBy(y => y.ExtracurricularSession.Day.Id).Select(y => y.ExtracurricularSession.Day.Description + " " + y.ExtracurricularSession.StartTime.ToString(@"hh\:mm") + "-" + y.ExtracurricularSession.EndTime.ToString(@"hh\:mm")))
                                : "-")
                            : Localizer["-"],
                Venue = x.ExtracurricularSessionMappings.Count != 0 // force order asc one-to-many relation
                            ? string.Join(";", x.ExtracurricularSessionMappings
                                .OrderBy(y => y.ExtracurricularSession.Venue.Code)
                                .Select(y => y.ExtracurricularSession.Venue.Code))
                            : Localizer[""],
                ElectivesDate = x.ElectivesStartDate.ToString(@"dd MMM yyyy") + " - " + x.ElectivesEndDate.ToString(@"dd MMM yyyy"),
                Category = x.Category,
                Participant = x.MinParticipant + " - " + x.MaxParticipant,
                ScoringDate = (x.ScoreStartDate == null ? "n/a" : x.ScoreStartDate?.ToString(@"dd MMM yyyy")) + " - " + (x.ScoreEndDate == null ? "n/a" : x.ScoreEndDate?.ToString(@"dd MMM yyyy")),
                Supervisor = x.ExtracurricularSpvCoach.Count != 0 // force order asc one-to-many relation
                            ? string.Join(";", x.ExtracurricularSpvCoach
                                .Where(y => (y.IsSpv == true || y.ExtracurricularCoachStatus.Code == "SPV"))
                                .OrderBy(y => y.Staff.FirstName)
                                .Select(y => ((string.IsNullOrEmpty(y.Staff.FirstName) ? "" : y.Staff.FirstName.Trim()) + (string.IsNullOrEmpty(y.Staff.LastName) ? "" : (" " + y.Staff.LastName))).Trim()))
                            : Localizer[""],
                Coach = x.ExtracurricularSpvCoach.Count != 0 // force order asc one-to-many relation
                            ? string.Join(";", x.ExtracurricularSpvCoach
                                .Where(y => (y.IsSpv == false || y.ExtracurricularCoachStatus.Code != "SPV"))
                                .OrderBy(y => y.Staff.FirstName)
                                .Select(y => ((string.IsNullOrEmpty(y.Staff.FirstName) ? "" : y.Staff.FirstName.Trim()) + (string.IsNullOrEmpty(y.Staff.LastName) ? "" : (" " + y.Staff.LastName))).Trim()))
                            : Localizer[""],
                ExtCoach = x.ExtracurricularExtCoachMappings.Count != 0 // force order asc one-to-many relation
                            ? string.Join(";", x.ExtracurricularExtCoachMappings                             
                                .OrderBy(y => y.ExtracurricularExternalCoach.Name)
                                .Select(y => (y.ExtracurricularExternalCoach.Name).Trim()))
                            : Localizer[""]
            }).OrderBy(x => x.ExtracurricularName).ToList();

            var title = "MasterElectives";
            if (resultExcul != null)
            {
                var generateExcelByte = GenerateExcel(paramDesc, resultExcul, title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcelByte = GenerateBlankExcel(title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string sheetTitle)
        {
            var result = new byte[0];
            //string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(2);

                #region Cara Baru biar bisa dynamic
                int fieldCount = 0;
                //foreach (string field in fieldDataList)
                //{
                //    var Judul = row.CreateCell(fieldCount);
                //    Judul.SetCellValue(field);
                //    row.Cells[fieldCount].CellStyle = style;
                //    fieldCount++;
                //}

                #endregion
                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcel(GetParameterDescriptionResult paramDesc, List<GetMasterExtracurricularforExportExcelResult> resultExcul, string sheetTitle)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);
                excelSheet.SetColumnWidth(0, 5 * 256);
                excelSheet.SetColumnWidth(1, 30 * 256);
                excelSheet.SetColumnWidth(2, 25 * 256);
                excelSheet.SetColumnWidth(3, 30 * 256);
                excelSheet.SetColumnWidth(4, 30 * 256);
                excelSheet.SetColumnWidth(5, 20 * 256);
                excelSheet.SetColumnWidth(6, 25 * 256);
                excelSheet.SetColumnWidth(7, 15 * 256);
                excelSheet.SetColumnWidth(8, 30 * 256);
                excelSheet.SetColumnWidth(9, 15 * 256);
                excelSheet.SetColumnWidth(10, 35 * 256);
                excelSheet.SetColumnWidth(11, 15 * 256);
                excelSheet.SetColumnWidth(12, 30 * 256);
                excelSheet.SetColumnWidth(13, 30 * 256);
                excelSheet.SetColumnWidth(14, 30 * 256);
                excelSheet.SetColumnWidth(15, 30 * 256);
                excelSheet.SetColumnWidth(16, 30 * 256);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleHeader = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                //Set border style 
                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 12;
                //font.IsItalic = true;
                style.SetFont(font);
                styleHeader.SetFont(font);
                style.VerticalAlignment = VerticalAlignment.Top;
                style.WrapText = true;

                IFont fontHeaderTable = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                fontHeaderTable.FontName = "Arial";
                fontHeaderTable.FontHeightInPoints = 12;
                fontHeaderTable.IsBold = true;

                styleHeaderTable.SetFont(fontHeaderTable);
                styleHeaderTable.VerticalAlignment = VerticalAlignment.Top;
                styleHeaderTable.Alignment = HorizontalAlignment.Center;
                styleHeaderTable.WrapText = true;

                //Title 
                IRow titleRow = excelSheet.CreateRow(1);
                var cellTitleRow = titleRow.CreateCell(1);
                cellTitleRow.SetCellValue("List of Electives");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                int paramRowIndex = 3;
                string[] fieldDataList = new string[3] { "School", "Academic Year", "Semester"};
                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(paramRowIndex);

                    ICell cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;

                    ICell cellValueParamRow = paramRow.CreateCell(2);
                    if (field == "School")
                        cellValueParamRow.SetCellValue(paramDesc.School);
                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYear);
                    if (field == "Semester")
                        cellValueParamRow.SetCellValue(paramDesc.Semester);
                    cellValueParamRow.CellStyle = styleHeader;

                    paramRowIndex++;
                }

                //Extracurricular Header
                var headerList = new string[16] { "Electives Name", "Electives Group", "Description", "Price", "Grade", "Show Attendance",
                    "Show Score", "Schedule", "Venue", "(Start - End) Electives Date", "Category",
                    "(Min - Max) Participant", "(Start - End) Scoring Date", "Supervisor", "Coach", "External Coach" };

                var summaryScoreHeaderList = new List<string>();

                summaryScoreHeaderList.AddRange(headerList);

                int indexSummaryHeader = 0;
                IRow rowSummaryHeader = excelSheet.CreateRow(7);
                foreach (string summaryScoreHeader in summaryScoreHeaderList)
                {
                    indexSummaryHeader++;
                    ICell cellSummaryHeader = rowSummaryHeader.CreateCell(indexSummaryHeader);
                    cellSummaryHeader.SetCellValue(summaryScoreHeader);
                    cellSummaryHeader.CellStyle = styleHeaderTable;
                }

                //Extracurricular Value
                int indexSummaryValue = 0;
                foreach (var item in resultExcul)
                {
                    IRow rowSummaryValue = excelSheet.CreateRow(8 + indexSummaryValue);
                    if (indexSummaryValue < resultExcul.Count())
                    {
                        ICell cellName = rowSummaryValue.CreateCell(1);
                        cellName.SetCellValue(resultExcul[indexSummaryValue].ExtracurricularName);
                        cellName.CellStyle = style;

                        ICell cellGroup = rowSummaryValue.CreateCell(2);
                        cellGroup.SetCellValue(resultExcul[indexSummaryValue].ExtracurricularGroup);
                        cellGroup.CellStyle = style;

                        ICell cellDescription = rowSummaryValue.CreateCell(3);
                        cellDescription.SetCellValue(resultExcul[indexSummaryValue].Description);
                        cellDescription.CellStyle = style;

                        ICell cellPrice = rowSummaryValue.CreateCell(4);
                        cellPrice.SetCellValue((double)resultExcul[indexSummaryValue].Price);
                        cellPrice.CellStyle = style;

                        ICell cellGrade = rowSummaryValue.CreateCell(5);
                        cellGrade.SetCellValue(resultExcul[indexSummaryValue].Grade);
                        cellGrade.CellStyle = style;

                        ICell cellShowAttendance = rowSummaryValue.CreateCell(6);
                        cellShowAttendance.SetCellValue(resultExcul[indexSummaryValue].ShowAttendance);
                        cellShowAttendance.CellStyle = style;

                        ICell cellShowScore = rowSummaryValue.CreateCell(7);
                        cellShowScore.SetCellValue(resultExcul[indexSummaryValue].ShowScore);
                        cellShowScore.CellStyle = style;

                        ICell cellSchedule = rowSummaryValue.CreateCell(8);
                        cellSchedule.SetCellValue(resultExcul[indexSummaryValue].Schedule.Replace(";", ";" + Environment.NewLine));
                        cellSchedule.CellStyle = style;

                        ICell cellVenue = rowSummaryValue.CreateCell(9);
                        cellVenue.SetCellValue(resultExcul[indexSummaryValue].Venue.Replace(";", ";" + Environment.NewLine));
                        cellVenue.CellStyle = style;

                        ICell cellAttendanceDate = rowSummaryValue.CreateCell(10);
                        cellAttendanceDate.SetCellValue(resultExcul[indexSummaryValue].ElectivesDate);
                        cellAttendanceDate.CellStyle = style;

                        ICell cellCategory = rowSummaryValue.CreateCell(11);
                        cellCategory.SetCellValue(resultExcul[indexSummaryValue].Category.ToString());
                        cellCategory.CellStyle = style;

                        ICell cellParticipant = rowSummaryValue.CreateCell(12);
                        cellParticipant.SetCellValue(resultExcul[indexSummaryValue].Participant);
                        cellParticipant.CellStyle = style;

                        ICell cellScoringDate = rowSummaryValue.CreateCell(13);
                        cellScoringDate.SetCellValue(resultExcul[indexSummaryValue].ScoringDate);
                        cellScoringDate.CellStyle = style;

                        ICell cellSupervisor = rowSummaryValue.CreateCell(14);
                        cellSupervisor.SetCellValue(resultExcul[indexSummaryValue].Supervisor.Replace(";", ";" + Environment.NewLine));
                        cellSupervisor.CellStyle = style;

                        ICell cellCoach = rowSummaryValue.CreateCell(15);
                        cellCoach.SetCellValue(resultExcul[indexSummaryValue].Coach.Replace(";", ";" + Environment.NewLine));
                        cellCoach.CellStyle = style;

                        ICell cellExtCoach = rowSummaryValue.CreateCell(16);
                        cellExtCoach.SetCellValue(resultExcul[indexSummaryValue].ExtCoach.Replace(";", ";" + Environment.NewLine));
                        cellExtCoach.CellStyle = style;
                    }
                    indexSummaryValue++;
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }
    }
}
