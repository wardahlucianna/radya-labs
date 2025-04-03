using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.LockerReservation.LockerReservation;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.LockerReservation.LockerReservation.Validator;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.LockerReservation.LockerReservation
{
    public class ExportExcelSummaryLockerReservationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly GetStudentLockerDataHandler _getStudentLockerDataHandler;

        public ExportExcelSummaryLockerReservationHandler(
            IStudentDbContext dbContext,
            GetStudentLockerDataHandler getStudentLockerDataHandler
            )
        {
            _dbContext = dbContext;
            _getStudentLockerDataHandler = getStudentLockerDataHandler;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelSummaryLockerReservationRequest, ExportExcelSummaryLockerReservationValidator>();

            var getStudentMasterDataForHeaderReport = await _getStudentLockerDataHandler.GetStudentLockerData(new GetStudentLockerDataRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester
            });

            var data = getStudentMasterDataForHeaderReport.OrderBy(x => x.FloorName).ThenBy(x => x.BuildingName).ThenBy(x => x.LockerName);

            var title = "LockerReservation";

            if (getStudentMasterDataForHeaderReport.Count() > 0)
            {
                var generateExcelByte = GenerateExcel(param, data.ToList(), title);
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
            //throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string sheetTitle)
        {
            var result = new byte[0];

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

        public byte[] GenerateExcel(ExportExcelSummaryLockerReservationRequest param, List<GetStudentLockerDataResult> data, string sheetTitle)
        {
            
            var result = new byte[0];

            var AcademicYearDesc = _dbContext.Entity<MsAcademicYear>()
                   .Where(x => x.Id == param.IdAcademicYear)
                   .Select(x => x.Description)
                   .FirstOrDefault();

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                XSSFFormulaEvaluator evaluator = new XSSFFormulaEvaluator(workbook);
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);
                excelSheet.SetColumnWidth(0, 10 * 256);
                excelSheet.SetColumnWidth(1, 20 * 256);
                excelSheet.SetColumnWidth(2, 20 * 256);
                excelSheet.SetColumnWidth(3, 20 * 256);
                excelSheet.SetColumnWidth(4, 10 * 256);

                //Create style
                ICellStyle styleHeader = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                font.FontName = "Arial";
                font.FontHeightInPoints = 8;

                IFont fontHeader = workbook.CreateFont();
                fontHeader.FontName = "Arial";
                fontHeader.FontHeightInPoints = 10;
                fontHeader.IsBold = true;

                //font.IsItalic = true;
                styleHeader.SetFont(fontHeader);
                styleHeaderTable.SetFont(fontHeader);
                styleHeaderTable.Alignment = HorizontalAlignment.Center;
                style.SetFont(font);

                styleHeaderTable.FillPattern = FillPattern.SolidForeground;
                styleHeaderTable.FillForegroundColor = IndexedColors.Grey50Percent.Index;

                //Parameter
                int paramRowIndex = 0;
                string[] fieldDataList = new string[4] { "Print Date", "Print Time", "Academic Year", "Semester" };
                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(paramRowIndex);

                    ICell cellParamRow = paramRow.CreateCell(0);
                    //cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;

                    if (paramRowIndex == 0 && paramRowIndex == 2)
                    {
                        CellRangeAddress region = new CellRangeAddress(0, 0, paramRowIndex, 4);
                        excelSheet.AddMergedRegion(region);
                    }

                    ICell cellValueParamRow = paramRow.CreateCell(0);
                    if (field == "Print Date")
                        cellValueParamRow.SetCellValue("Print Date : " + DateTime.Now.ToString("dddd, dd MMMM yyyy"));
                    if (field == "Print Time")
                        cellValueParamRow.SetCellValue("Print Time : " + DateTime.Now.ToString("hh:mm tt"));
                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue("Academic Year : " + AcademicYearDesc);
                    if (field == "Semester")
                        cellValueParamRow.SetCellValue("Semester : " + param.Semester);

                    cellValueParamRow.CellStyle = styleHeader;

                    paramRowIndex++;
                }

                //Summary Header
                var summaryScoreHeaderList = new string[5] { "Floor", "Tower", "Locker Number", "Student", "Grade" };
                int indexSummaryHeader = 0;
                IRow rowSummaryHeader = excelSheet.CreateRow(5);
                foreach (string summaryScoreHeader in summaryScoreHeaderList)
                {
                    ICell cellSummaryHeader = rowSummaryHeader.CreateCell(indexSummaryHeader);
                    cellSummaryHeader.SetCellValue(summaryScoreHeader);
                    cellSummaryHeader.CellStyle = styleHeaderTable;
                    indexSummaryHeader++;
                }

                //Summary Value
                int indexSummaryValue = 0;
                foreach (var item in data)
                {
                    IRow rowSummaryValue = excelSheet.CreateRow(6 + indexSummaryValue);
                    if (indexSummaryValue < data.Count())
                    {
                        ICell cellLevel = rowSummaryValue.CreateCell(0);
                        cellLevel.SetCellValue(data[indexSummaryValue].FloorName);
                        cellLevel.CellStyle = style;

                        ICell cellCounter = rowSummaryValue.CreateCell(1);
                        cellCounter.SetCellValue(data[indexSummaryValue].BuildingName);
                        cellCounter.CellStyle = style;

                        ICell cellSubmitted = rowSummaryValue.CreateCell(2);
                        cellSubmitted.SetCellValue(data[indexSummaryValue].LockerName);
                        cellSubmitted.CellStyle = style;

                        ICell cellPending = rowSummaryValue.CreateCell(3);
                        cellPending.SetCellValue(data[indexSummaryValue].StudentName);
                        cellPending.CellStyle = style;

                        ICell cellUnsubmitted = rowSummaryValue.CreateCell(4);
                        cellUnsubmitted.SetCellValue(data[indexSummaryValue].GradeName);
                        cellUnsubmitted.CellStyle = style;
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
