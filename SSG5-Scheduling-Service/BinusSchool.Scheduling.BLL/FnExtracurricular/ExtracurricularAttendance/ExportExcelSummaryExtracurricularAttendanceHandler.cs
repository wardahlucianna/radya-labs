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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularAttendance;
using BinusSchool.Data.Model.Scoring.FnScoring.ScoreSummaryByLevel;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;


namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularAttendance
{
    public class ExportExcelSummaryExtracurricularAttendanceHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IExtracurricularAttendance _extracurricularAttendanceAPI;

        public ExportExcelSummaryExtracurricularAttendanceHandler(ISchedulingDbContext dbContext, IExtracurricularAttendance extracurricularAttendanceAPI)
        {
            _dbContext = dbContext;
            _extracurricularAttendanceAPI = extracurricularAttendanceAPI;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<ExportExcelSummaryExtracurricularAttendanceRequest>();

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

            var GetStudentAttendanceList = new List<GetStudentAttendanceResult>();

            foreach (var excul in param.IdExtracurricular)
            {
                var GetStudentAttendance = await _extracurricularAttendanceAPI.GetStudentAttendance(new GetStudentAttendanceRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = param.Semester,
                    IdExtracurricular = excul,
                    Month = param.Month
                });

                GetStudentAttendanceList.Add(GetStudentAttendance.Payload);
            }

            var title = "ElectivesAttendanceSummary";
            if (GetStudentAttendanceList != null)
            {
                var generateExcelByte = GenerateExcel(paramDesc, param, GetStudentAttendanceList, title);
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

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var TitleValidated = regex.Replace(sheetTitle, " ");
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(TitleValidated);

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

        public byte[] GenerateExcel(GetParameterDescriptionResult paramDesc, ExportExcelSummaryExtracurricularAttendanceRequest paramRequest, List<GetStudentAttendanceResult> data, string sheetTitle)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                foreach(var excul in data)
                {

                    var pattern = "[/\\\\:*?<>|\"]";
                    var regex = new Regex(pattern);
                    var ExculNameValidated = regex.Replace(excul.Extracurricular.Name, " ");
                    ISheet existingSheet = workbook.GetSheet(ExculNameValidated);

                    if (existingSheet != null)
                    {
                        int sheetNumber = 2;
                        while (workbook.GetSheet(ExculNameValidated + " (" + sheetNumber + ")") != null)
                        {
                            sheetNumber++;
                        }

                        ExculNameValidated = ExculNameValidated + " (" + sheetNumber + ")";
                    }

                    ISheet excelSheet = workbook.CreateSheet(ExculNameValidated);
                    //ISheet excelSheet = workbook.CreateSheet(ExculNameValidated);
                    excelSheet.SetColumnWidth(0, 5 * 256);
                    excelSheet.SetColumnWidth(1, 20 * 256);
                    excelSheet.SetColumnWidth(2, 40 * 256);
                    excelSheet.SetColumnWidth(3, 15 * 256);
                    excelSheet.SetColumnWidth(4, 30 * 256);
                    excelSheet.SetColumnWidth(5, 30 * 256);
                    excelSheet.SetColumnWidth(6, 30 * 256);
                    excelSheet.SetColumnWidth(7, 30 * 256);
                    excelSheet.SetColumnWidth(8, 30 * 256);
                    excelSheet.SetColumnWidth(9, 30 * 256);
                    excelSheet.SetColumnWidth(10, 30 * 256);
                    excelSheet.SetColumnWidth(11, 30 * 256);
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

                    IFont fontHeaderTable = workbook.CreateFont();
                    // font.Color = HSSFColor.Red.Index;
                    fontHeaderTable.FontName = "Arial";
                    fontHeaderTable.FontHeightInPoints = 12;
                    fontHeaderTable.IsBold = true;

                    style.SetFont(font);
                    styleHeader.SetFont(font);
                    styleHeaderTable.SetFont(fontHeaderTable);
                    style.VerticalAlignment = VerticalAlignment.Top;
                    styleHeaderTable.VerticalAlignment = VerticalAlignment.Top;
                    styleHeaderTable.Alignment = HorizontalAlignment.Center;
                    styleHeaderTable.WrapText = true;
                    style.WrapText = true;

                    //Title 
                    IRow titleRow = excelSheet.CreateRow(1);
                    var cellTitleRow = titleRow.CreateCell(1);
                    cellTitleRow.SetCellValue("Electives Attendance Summary");
                    cellTitleRow.CellStyle = styleHeader;

                    //Parameter
                    int paramRowIndex = 3;
                    string[] fieldDataList = new string[7] { "School", "Academic Year", "Semester", "Electives", "Month", "Supervisor", "Coach" };
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
                        if (field == "Electives")
                            cellValueParamRow.SetCellValue(excul.Extracurricular.Name);
                        if (field == "Month")
                            cellValueParamRow.SetCellValue(paramRequest.Month == 0 ? "All" : ((Month)paramRequest.Month).ToString());
                        if (field == "Supervisor")
                            cellValueParamRow.SetCellValue(string.Join(", ", excul.Supervisor.OrderBy(a => a.Name).Select(a => a.Name)));
                        if (field == "Coach")
                            cellValueParamRow.SetCellValue(excul.Coach != null ? string.Join(", ", excul.Coach.OrderBy(a => a.Name).Select(a => a.Name)) : "-");
                            //cellValueParamRow.SetCellValue((excul.Coach != null ? excul.Coach.Name : "-"));

                        cellValueParamRow.CellStyle = styleHeader;

                        paramRowIndex++;
                    }

                    //Summary Header
                    var headerList = new string[3] { "Student ID", "Student Name", "Homeroom" };
                    var headerSessionList = excul.HeaderList.Select(x => x.ExtracurricularGeneratedAtt.Name + Environment.NewLine + x.ExtracurricularGeneratedDate.ToString(@"dd MMM yyyy")).OfType<string>();

                    var summaryScoreHeaderList = new List<string>();

                    summaryScoreHeaderList.AddRange(headerList);
                    summaryScoreHeaderList.AddRange(headerSessionList);

                    int indexSummaryHeader = 0;
                    IRow rowSummaryHeader = excelSheet.CreateRow(11);
                    foreach (string summaryScoreHeader in summaryScoreHeaderList)
                    {
                        indexSummaryHeader++;
                        ICell cellSummaryHeader = rowSummaryHeader.CreateCell(indexSummaryHeader);
                        cellSummaryHeader.SetCellValue(summaryScoreHeader);
                        cellSummaryHeader.CellStyle = styleHeaderTable;
                    }

                    //Summary Value
                    int indexSummaryValue = 0;
                    foreach (var item in excul.BodyList)
                    {
                        IRow rowSummaryValue = excelSheet.CreateRow(12 + indexSummaryValue);
                        if (indexSummaryValue < excul.BodyList.Count())
                        {
                            ICell cellLevel = rowSummaryValue.CreateCell(1);
                            cellLevel.SetCellValue(excul.BodyList[indexSummaryValue].Student.Id);
                            cellLevel.CellStyle = style;

                            ICell cellCounter = rowSummaryValue.CreateCell(2);
                            cellCounter.SetCellValue(excul.BodyList[indexSummaryValue].Student.Name);
                            cellCounter.CellStyle = style;

                            ICell cellSubmitted = rowSummaryValue.CreateCell(3);
                            cellSubmitted.SetCellValue(excul.BodyList[indexSummaryValue].Homeroom.Name);
                            cellSubmitted.CellStyle = style;

                            int indexCriteria = 1;
                            foreach (var sessionAttendance in item.SessionAttendanceList)
                            {
                                ICell cellCriteria = rowSummaryValue.CreateCell(3 + indexCriteria);
                                cellCriteria.SetCellValue(sessionAttendance.ExtracurricularStatusAtt.Name == null ? " " : sessionAttendance.ExtracurricularStatusAtt.Name + ((sessionAttendance.Reason == "" || sessionAttendance.Reason == null) ? "" : "(" + sessionAttendance.Reason + ")"));
                                cellCriteria.CellStyle = style;
                                indexCriteria++;
                            }

                        }
                        indexSummaryValue++;
                    }
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
