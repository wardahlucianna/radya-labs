using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.DailyAttendanceRecap.Validator;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Apis.Binusian.BinusSchool;
using BinusSchool.Data.Model.Attendance.FnAttendance.DailyAttendanceRecap;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.DailyAttendanceRecap
{
    public class GenerateExcelDailyAttendanceRecapHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IAuth _apiAuth;
        private readonly IAttendanceLog _apiAttendanceLog;
        private readonly IMachineDateTime _datetimeNow;

        private readonly GetDailyAttendanceRecapHandler _getDailyAttendanceRecapHandler;

        public GenerateExcelDailyAttendanceRecapHandler(IAttendanceDbContext dbContext, IAuth apiAuth, IAttendanceLog apiAttendanceLog, IMachineDateTime datetimeNow, GetDailyAttendanceRecapHandler getDailyAttendanceRecapHandler)
        {
            _dbContext = dbContext;
            _apiAuth = apiAuth;
            _apiAttendanceLog = apiAttendanceLog;
            _datetimeNow = datetimeNow;
            _getDailyAttendanceRecapHandler = getDailyAttendanceRecapHandler;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var body = await Request.ValidateBody<GenerateExcelDailyAttendanceRecapRequest, GenerateExcelDailyAttendanceRecapValidator>();

            var dailyAttendanceRecap = await _getDailyAttendanceRecapHandler.GetDailyAttendanceRecap(new GetDailyAttendanceRecapRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester,
                IdBinusian = body.IdBinusian,
                IdHomeroom = body.IdHomeroom,
                IdLevel = body.IdLevel,
                IdGrade = body.IdGrade
            });

            var excelResult = GenerateExcel(dailyAttendanceRecap);

            var retval = new GenerateExcelDailyAttendanceRecapResult();
            retval.ExcelOutput = excelResult;
            retval.FileName = "unsubmitted-attendance-recap";

            return new FileContentResult(retval.ExcelOutput, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{retval.FileName}_{DateTime.Now.Ticks}.xlsx"
            };
        }

        private byte[] GenerateExcel(List<GetDailyAttendanceRecapResult> param)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            { 
                #region Initialize excel
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                ISheet excelSheet = workbook.CreateSheet("Unsubmitted Attendance Recap");

                //Create Style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                ICellStyle styleValue = workbook.CreateCellStyle();
                ICellStyle styleValueDate = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;

                styleValue.BorderBottom = BorderStyle.Thin;
                styleValue.BorderLeft = BorderStyle.Thin;
                styleValue.BorderRight = BorderStyle.Thin;
                styleValue.BorderTop = BorderStyle.Thin;

                styleValueDate.BorderBottom = BorderStyle.Thin;
                styleValueDate.BorderLeft = BorderStyle.Thin;
                styleValue.BorderRight = BorderStyle.Thin;
                styleValueDate.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                font.FontName = "Calibri";
                font.FontHeightInPoints = 11;

                style.SetFont(font);
                styleHeaderTable.SetFont(font);
                styleValue.SetFont(font);
                styleValueDate.SetFont(font);


                // Set alignment style
                style.VerticalAlignment = VerticalAlignment.Top;
                style.WrapText = true;

                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.Alignment = HorizontalAlignment.Left;

                styleValue.VerticalAlignment = VerticalAlignment.Center;
                styleValue.Alignment = HorizontalAlignment.Left;

                styleValueDate.VerticalAlignment = VerticalAlignment.Center;
                styleValueDate.Alignment = HorizontalAlignment.Right;
                #endregion

                #region Table header
                IRow tableHeaderRow = excelSheet.CreateRow(0);

                var tableHeaderRowFirstCol = tableHeaderRow.CreateCell(0);
                tableHeaderRowFirstCol.SetCellValue("Class");
                tableHeaderRowFirstCol.CellStyle = styleHeaderTable;
       
                var tableHeaderRowSecondCol = tableHeaderRow.CreateCell(1);
                tableHeaderRowSecondCol.SetCellValue("Homeroom Teacher");
                tableHeaderRowSecondCol.CellStyle = styleHeaderTable;

                var tableHeaderRowThirdCol = tableHeaderRow.CreateCell(2);
                tableHeaderRowThirdCol.SetCellValue("Schedule Date");
                tableHeaderRowThirdCol.CellStyle = styleHeaderTable;
                #endregion

                #region Table data
                int rowNumber = 1;
                foreach (var row in param)
                {
                    if (row.TotalUnsubmitted > 0)
                    {
                        var classroom = row.Class;
                        var homeroomTeacher = row.HomeroomTeacher;

                        foreach (var unsubmittedDate in row.UnsubmittedDate)
                        {
                            IRow tableCellRow = excelSheet.CreateRow(rowNumber);

                            var tableCellFirstCol = tableCellRow.CreateCell(0);
                            tableCellFirstCol.SetCellValue(classroom);
                            tableCellFirstCol.CellStyle = styleValue;

                            var tableCellSecondCol = tableCellRow.CreateCell(1);
                            tableCellSecondCol.SetCellValue(homeroomTeacher);
                            tableCellSecondCol.CellStyle = styleValue;

                            var tableCellThirdCol = tableCellRow.CreateCell(2);
                            tableCellThirdCol.SetCellValue(DateTime.ParseExact(unsubmittedDate, "dd/MM/yyyy", null).ToString("dddd, MMMM dd, yyyy"));
                            tableCellThirdCol.CellStyle = styleValueDate;

                            rowNumber++;
                        }
                    }
                }

                for(int col = 0; col < 3; col++)
                {
                    excelSheet.AutoSizeColumn(col);
                }
                #endregion

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();

                return result;
            }
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }


    }
}
