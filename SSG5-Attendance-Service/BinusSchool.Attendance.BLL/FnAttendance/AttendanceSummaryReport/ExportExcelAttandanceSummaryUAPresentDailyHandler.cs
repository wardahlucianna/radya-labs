using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryReport;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace BinusSchool.Attendance.FnAttendance.AttendanceSummaryReport
{
    public class ExportExcelAttandanceSummaryUAPresentDailyHandler : FunctionsHttpSingleHandler
    {
        private readonly IMachineDateTime _datetime;
        public ExportExcelAttandanceSummaryUAPresentDailyHandler(IMachineDateTime datetime)
        {
            _datetime = datetime;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var body = await Request.GetBody<ExportExcelAttandanceSummaryUAPresentDailyRequest>();

            var excelRecap = new byte[8];

            if (body.DataAttandance.UAPresentStudent != null)
            {
                excelRecap = GenerateExcel(body.AttendanceDate, body.DataAttandance);
            }
            else
            {
                excelRecap = GenerateBlankExcel();
            }

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Recap-All-Unexcused-Absence{DateTime.Now.Ticks}.xlsx"
            };
        }

        public byte[] GenerateExcel(DateTime AttendanceDate, GetAttendanceSummaryDailyReportResult data)
        {
            var workbook = new XSSFWorkbook();

            #region style
            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.Alignment = HorizontalAlignment.Center;
            boldStyle.VerticalAlignment = VerticalAlignment.Center;
            boldStyle.BorderBottom = BorderStyle.Thin;
            boldStyle.BorderLeft = BorderStyle.Thin;
            boldStyle.BorderRight = BorderStyle.Thin;
            boldStyle.BorderTop = BorderStyle.Thin;
            boldStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
            boldStyle.FillPattern = FillPattern.SolidForeground;
            boldStyle.WrapText = true;
            boldStyle.SetFont(fontBold);

            //===================================================
            var fontBoldHeader = workbook.CreateFont();
            fontBoldHeader.IsBold = true;
            fontBoldHeader.FontHeightInPoints = 20;

            var boldStyleHeader = workbook.CreateCellStyle();
            boldStyleHeader.Alignment = HorizontalAlignment.Center;
            boldStyleHeader.VerticalAlignment = VerticalAlignment.Center;
            boldStyleHeader.WrapText = true;
            boldStyleHeader.SetFont(fontBoldHeader);

            //====================
            var fontNormalHeader = workbook.CreateFont();
            var normalStyleHeader = workbook.CreateCellStyle();
            normalStyleHeader.Alignment = HorizontalAlignment.Center;
            normalStyleHeader.SetFont(fontNormalHeader);


            //====================
            var fontNormalBody = workbook.CreateFont();
            var normalStyleBody = workbook.CreateCellStyle();
            normalStyleBody.BorderBottom = BorderStyle.Thin;
            normalStyleBody.BorderLeft = BorderStyle.Thin;
            normalStyleBody.BorderRight = BorderStyle.Thin;
            normalStyleBody.BorderTop = BorderStyle.Thin;
            normalStyleBody.Alignment = HorizontalAlignment.Center;
            normalStyleBody.VerticalAlignment = VerticalAlignment.Center;
            normalStyleBody.WrapText = true;
            normalStyleBody.SetFont(fontNormalBody);

            //====================
            var boldStyleBody = workbook.CreateCellStyle();
            boldStyleBody.Alignment = HorizontalAlignment.Center;
            boldStyleBody.VerticalAlignment = VerticalAlignment.Center;
            boldStyleBody.WrapText = true;
            boldStyleBody.SetFont(fontBold);

            var CustomfontStyle = workbook.CreateFont();
            ICellStyle CustomStyle = workbook.CreateCellStyle();
            CustomStyle.BorderBottom = BorderStyle.Thin;
            CustomStyle.BorderLeft = BorderStyle.Thin;
            CustomStyle.BorderRight = BorderStyle.Thin;
            CustomStyle.BorderTop = BorderStyle.Thin;
            CustomStyle.Alignment = HorizontalAlignment.Center;
            CustomStyle.VerticalAlignment = VerticalAlignment.Center;
            CustomStyle.FillForegroundColor = HSSFColor.Black.Index;
            CustomStyle.FillPattern = FillPattern.SolidForeground;
            CustomfontStyle.Color = HSSFColor.White.Index;
            CustomStyle.WrapText = true;
            CustomStyle.SetFont(CustomfontStyle);
            #endregion

            byte[] imageBytes = new byte[8];

            string someUrl = "https://e-desk.binus.sch.id/assets/img/binus_logo_big.png";
            using (var webClient = new WebClient())
            {
                imageBytes = webClient.DownloadData(someUrl);
            }

            var paramSheet1 = new GetAttendanceSummaryDailyReportRequest_ExcelSheet
            {
                Workbook = workbook,
                BoldStyleHeader = boldStyleHeader,
                NormalStyleHeader = normalStyleHeader,
                BoldStyle = boldStyle,
                NormalStyleBody = normalStyleBody,
                BoldStyleBody = boldStyleBody,
                CancellationToken = CancellationToken,
                Logo = imageBytes,
                Date = AttendanceDate,
                AttendanceSummaryDailyReport = data
            };
            GetSheet1(paramSheet1);

            var paramSheet2 = new GetAttendanceSummaryDailyReportRequest_ExcelSheet
            {
                Workbook = workbook,
                BoldStyleHeader = boldStyleHeader,
                NormalStyleHeader = normalStyleHeader,
                BoldStyle = boldStyle,
                NormalStyleBody = normalStyleBody,
                BoldStyleBody = boldStyleBody,
                CancellationToken = CancellationToken,
                CustomStyle = CustomStyle,
                Logo = imageBytes,
                Date = AttendanceDate,
                AttendanceSummaryDailyReport = data
            };

            GetSheet2(paramSheet2);
            GetSheet3(paramSheet2);
            GetSheet4(paramSheet2);

            using var ms = new MemoryStream();
            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }

        private string GetSheet1(GetAttendanceSummaryDailyReportRequest_ExcelSheet data)
        {
            var sheet = data.Workbook.CreateSheet("UA Student");
            
            sheet.CreateFreezePane(0, 6, 0, 6);
            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row

                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1, 2.7);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);

            #region List Of UA Student
            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 1, startColumn, startColumn + 2);
            cellNo.SetCellValue("List of UA Student");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            #endregion

            #region List Teachers who have not filled Attendance
            startColumn = 6;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 3, startColumn, startColumn + 3);
            cellNo.SetCellValue("Teachers who have not filled Attendance");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            #endregion

            #region UA & Present Students who did not Tap In
            startColumn = 12;
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 3, startColumn, startColumn + 3);
            cellNo.SetCellValue("UA & Present Students who did not Tap In");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            #endregion
            rowIndex++;
            #endregion

            #region Row 2 & 3

            #region Date Generate
            startColumn = 2;
            rowIndex = 2;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 2);
            cellNo.SetCellValue($"Attendance Date {data.Date.ToString("dd MMMM yyyy")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.BoldStyleBody;
            #endregion

            #region Date Attendance
            startColumn = 2;
            rowIndex = 3;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 2);
            cellNo.SetCellValue($"as of {_datetime.ServerTime.ToString("dd MMMM yyyy HH:mm")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.NormalStyleHeader;
            #endregion
            #endregion

            #region Header Table
            rowIndex = 5;
            rowHeader = sheet.CreateRow(rowIndex);

            #region List Of UA Student
            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");

            startColumn = 0;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            
            rowIndex++;
            #endregion

            #region  List Teachers who have not filled Attendance
            header = new List<string>();
            header.Add("No");
            header.Add("Teacher ID");
            header.Add("Teacher Name");
            //header.Add("Class");
            header.Add("Session");

            startColumn = 6;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region  List Students who did not Tap In
            header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");

            startColumn = 12;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion
            #endregion

            #region Body Table
            #region List Of UA Student
            var ListUAStudent = data.AttendanceSummaryDailyReport.UAStudents.ListUAStudent.ToList();
            var ListTeacherAttendance = data.AttendanceSummaryDailyReport.UAStudents.ListTeacherAttendance.ToList();
            var ListNotTapIn = data.AttendanceSummaryDailyReport.UAStudents.ListNotTapIn.ToList();

            List<int> listCount = new List<int> { ListUAStudent.Count(), ListTeacherAttendance.Count(), ListNotTapIn.Count() };
            var count = listCount.Max();

            rowIndex = 6;
            for (var i = 0; i < count; i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < ListUAStudent.Count())
                {
                    var itemUa = ListUAStudent[i];

                    startColumn = 0;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUa.Student.Id);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUa.Student.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUa.Homeroom.Description);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                if (i < ListTeacherAttendance.Count())
                {
                    var itemUnsubmited = ListTeacherAttendance[i];

                    startColumn = 6;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.Teacher.Id);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.Teacher.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemUnsubmited.SessionTeacher);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    //cellNo = rowHeader.CreateCell(startColumn);
                    //cellNo.SetCellValue(itemUnsubmited.SessionTeacher);
                    //cellNo.CellStyle = data.NormalStyleBody;
                    //sheet.AutoSizeColumn(startColumn, true);
                    //startColumn++;
                }

                if (i < ListNotTapIn.Count())
                {
                    var itemTapping = ListNotTapIn[i];

                    startColumn = 12;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemTapping.Student.Id);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemTapping.Student.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemTapping.Homeroom.Description);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion
            #endregion

            sheet.SetColumnWidth(0, 9 * 256);
            sheet.SetColumnWidth(6, 6 * 256);
            sheet.SetColumnWidth(12, 6 * 256);

            return default;
        }

        private string GetSheet2(GetAttendanceSummaryDailyReportRequest_ExcelSheet data)
        {
            var checkSymbol = '\u2713';
            var xSymbol = '\u2717';
            var sheet = data.Workbook.CreateSheet("UA & Present Student");
            sheet.CreateFreezePane(4, 6);
            var ListSession = data.AttendanceSummaryDailyReport.UAPresentStudent.ListUAPresentCheck.SelectMany(e => e.SessionAttandance.Select(f => f.Id)).Distinct().ToList();

            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1, 2.7);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);
            int startColumn = 4;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 1, startColumn, startColumn + ListSession.Count() + 3);
            cellNo.SetCellValue("List of Students with UA & Present");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);

            rowIndex++;
            #endregion

            #region Date Generate
            startColumn = 4;
            rowIndex = 2;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + ListSession.Count() + 3);
            cellNo.SetCellValue($"Attendance Date {data.Date.ToString("dd MMMM yyyy")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.BoldStyleBody;
            #endregion

            #region Date Attendance
            startColumn = 4;
            rowIndex = 3;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + ListSession.Count() + 3);
            cellNo.SetCellValue($"as at {_datetime.ServerTime.ToString("dd MMMM yyyy HH:mm")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.NormalStyleHeader;
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            #region Hader Table Pertama
            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");
            header.Add("Tapping Time");
            header.Add("Session");
            header.Add("Total Present");
            header.Add("Total Absent");
            header.Add("Total Late");

            startColumn = 0;
            foreach (var itemHeader in header)
            {
                if (itemHeader != "Session")
                {
                    merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 1, startColumn, startColumn);
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemHeader);
                    cellNo.CellStyle = data.BoldStyle;
                    sheet.AutoSizeColumn(startColumn, true);
                    sheet.AddMergedRegion(merge);
                    startColumn++;
                }
                else
                {
                    merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + ListSession.Count() - 1);
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemHeader);
                    cellNo.CellStyle = data.BoldStyle;
                    sheet.AutoSizeColumn(startColumn, true);
                    sheet.AddMergedRegion(merge);
                    startColumn++;

                    //for border
                    for (var i = 0; i <= ListSession.Count() - 2; i++)
                    {
                        cellNo = rowHeader.GetCell(startColumn);
                        if (cellNo == null)
                            cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.CellStyle = data.BoldStyle;
                        sheet.AutoSizeColumn(startColumn, true);
                        startColumn++;
                    }
                }
            }
            rowIndex++;
            #endregion

            #region Hader Table Ke-2
            rowHeader = sheet.CreateRow(rowIndex);

            startColumn = 0;
            for (var i = 0; i <= ListSession.Count() + 8; i++)
            {
                if (i == 5)
                {
                    foreach (var itemHeader in ListSession)
                    {
                        cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.SetCellValue(itemHeader);
                        cellNo.CellStyle = data.BoldStyle;
                        sheet.AutoSizeColumn(startColumn, true);
                        startColumn++;
                        i++;
                    }
                }
                else
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.CellStyle = data.BoldStyle;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

            }
            rowIndex++;
            #endregion
            #endregion

            #region Body Table
            for (var i = 0; i < data.AttendanceSummaryDailyReport.UAPresentStudent.ListUAPresentCheck.Count(); i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < data.AttendanceSummaryDailyReport.UAPresentStudent.ListUAPresentCheck.Count())
                {
                    var itemPresentStudent = data.AttendanceSummaryDailyReport.UAPresentStudent.ListUAPresentCheck[i];
                    var listSessionStudent = itemPresentStudent.SessionAttandance.OrderBy(e => e.Id).ToList();
                    startColumn = 0;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.Student.Id);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.Student.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.Homeroom.Description);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresentStudent.TappingTime);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    for (var sessionName = 0; sessionName < ListSession.Count(); sessionName++) //foreach (var itemSession in listSessionStudent)
                    {
                        cellNo = rowHeader.CreateCell(startColumn);

                        var dataSession = listSessionStudent.Where(x => x.Id == sessionName.ToString()).Select(x => x.Code).FirstOrDefault();
                        
                        if (dataSession == "Present")
                        {
                            dataSession = checkSymbol.ToString();
                            cellNo.CellStyle = data.NormalStyleBody;
                        }
                        else if (dataSession == "UA" || dataSession == "Late")
                        {
                            cellNo.CellStyle = data.CustomStyle;
                        }
                        else if (dataSession == "X")
                        {
                            dataSession = xSymbol.ToString();
                            cellNo.CellStyle = data.NormalStyleBody;
                        }
                        else
                        {
                            cellNo.CellStyle = data.NormalStyleBody;
                        }

                        cellNo.SetCellValue(dataSession ?? xSymbol.ToString());
                        //cellNo.CellStyle = data.NormalStyleBody;
                        sheet.AutoSizeColumn(startColumn, true);
                        startColumn++;
                    }

                    var countPreset = listSessionStudent.Where(e => e.Code == "Present").Count();
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(countPreset.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    var countAbsent = listSessionStudent.Where(e => e.Code == "UA").Count();
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(countAbsent.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    var countLate = listSessionStudent.Where(e => e.Code == "Late").Count();
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(countLate.ToString());
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion

            sheet.SetColumnWidth(0, 9 * 256);
            return default;
        }

        private string GetSheet3(GetAttendanceSummaryDailyReportRequest_ExcelSheet data)
        {
            var sheet = data.Workbook.CreateSheet("UA & Present - Details");
            sheet.CreateFreezePane(0, 5, 0, 5);
            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1, 2.7);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);
            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 1, startColumn, startColumn + 6);
            cellNo.SetCellValue("List of Students with UA & Present - Details");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            rowIndex++;
            #endregion

            #region Date Generate
            startColumn = 2;
            rowIndex = 2;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 7);
            cellNo.SetCellValue($"Attendance Date {data.Date.ToString("dd MMMM yyyy")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.BoldStyleBody;
            #endregion

            #region Date Attendance
            startColumn = 2;
            rowIndex = 3;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 7);
            cellNo.SetCellValue($"as at {_datetime.ServerTime.ToString("dd MMMM yyyy HH:mm")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.NormalStyleHeader;
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            List<string> header = new List<string>();
            header.Add("No");
            header.Add("Student ID");
            header.Add("Student Name");
            header.Add("Class");
            header.Add("Class ID");
            header.Add("Session");
            header.Add("Status");
            header.Add("Teacher");

            startColumn = 0;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region Body Table
            #region List Of UA Student
            var listPresent = data.AttendanceSummaryDailyReport.UAPresentStudent.ListDetail.ToList();

            rowIndex = 5;
            for (var i = 0; i < listPresent.Count(); i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < listPresent.Count())
                {
                    var itemPresent = listPresent[i];

                    startColumn = 0;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(i + 1);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Student.Id);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Student.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Homeroom.Description);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.ClassID);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Session.Id);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Session.Code);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Teacher.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion
            #endregion
            sheet.SetColumnWidth(0, 9 * 256);
            return default;
        }

        private string GetSheet4(GetAttendanceSummaryDailyReportRequest_ExcelSheet data)
        {
            var sheet = data.Workbook.CreateSheet("Summary");
            sheet.CreateFreezePane(0, 5, 0, 5);
            #region Add Logo
            if (data.Logo != null)
            {
                byte[] dataImg = data.Logo;
                int pictureIndex = data.Workbook.AddPicture(dataImg, PictureType.PNG);
                ICreationHelper helper = data.Workbook.GetCreationHelper();
                IDrawing drawing = sheet.CreateDrawingPatriarch();
                IClientAnchor anchor = helper.CreateClientAnchor();
                anchor.Col1 = 0;//0 index based column
                anchor.Row1 = 0;//0 index based row
                IPicture picture = drawing.CreatePicture(anchor, pictureIndex);
                picture.Resize(1, 2.7);
            }
            #endregion

            #region Row 1
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);
            int startColumn = 2;
            var cellNo = rowHeader.CreateCell(startColumn);
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex + 1, startColumn, startColumn + 8);
            cellNo.SetCellValue("Summary");
            cellNo.CellStyle = data.BoldStyleHeader;
            sheet.AddMergedRegion(merge);
            rowIndex++;
            #endregion

            #region Date Generate
            startColumn = 2;
            rowIndex = 2;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 8);
            cellNo.SetCellValue($"Attendance Date {data.Date.ToString("dd MMMM yyyy")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.BoldStyleBody;
            #endregion

            #region Date Attendance
            startColumn = 2;
            rowIndex = 3;
            rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 8);
            cellNo.SetCellValue($"as at {_datetime.ServerTime.ToString("dd MMMM yyyy HH:mm")}");
            sheet.AddMergedRegion(merge);
            cellNo.CellStyle = data.NormalStyleHeader;
            #endregion

            #region Header Table
            rowIndex = 4;
            rowHeader = sheet.CreateRow(rowIndex);

            List<string> header = new List<string>();
            header.Add("Grade Level");
            header.Add("Class");
            header.Add("Student Name");
            header.Add("Class ID");
            header.Add("Session");

            startColumn = 0;
            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;
            #endregion

            #region Body Table
            #region Summary
            var listPresent = data.AttendanceSummaryDailyReport.Summary.ListDetail.ToList();

            rowIndex = 5;
            for (var i = 0; i < listPresent.Count(); i++)
            {
                rowHeader = sheet.CreateRow(rowIndex);

                if (i < listPresent.Count())
                {
                    var itemPresent = listPresent[i];

                    startColumn = 0;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.GradeLevel);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Homeroom.Description);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Student.Name);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.ClassID);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;

                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemPresent.Session);
                    cellNo.CellStyle = data.NormalStyleBody;
                    sheet.AutoSizeColumn(startColumn, true);
                    startColumn++;
                }

                rowIndex++;
            }
            #endregion

            #region Summary Data
            rowIndex = 4;
            startColumn = 6;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Unexcused Absences");
            cellNo.CellStyle = data.NormalStyleHeader;
            sheet.AutoSizeColumn(startColumn, true);
            rowIndex++;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Summary Data");
            cellNo.CellStyle = data.NormalStyleHeader;
            sheet.AutoSizeColumn(startColumn, true);
            rowIndex++;

            //rowHeader = sheet.GetRow(rowIndex);
            //if (rowHeader == null)
            //    rowHeader = sheet.CreateRow(rowIndex);
            //cellNo = rowHeader.CreateCell(startColumn);
            //cellNo.SetCellValue($"{data.Date.ToString("dddd, dd MMMM yyyy HH:mm")}");
            //cellNo.CellStyle = data.BoldStyleBody;
            //sheet.AutoSizeColumn(startColumn, true);
            //rowIndex += 2;

            #region summary grade
            List<string> headerSummaryGrade = new List<string>();
            headerSummaryGrade.Add("Grade Level");
            headerSummaryGrade.Add("Total Session");

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            foreach (var itemHeader in headerSummaryGrade)
            {
                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;

            var summaryGrade = listPresent
                        .GroupBy(e => new
                        {
                            Grade = e.GradeLevel
                        })
                        .ToList();

            var totalAll = (decimal) listPresent.Count();

            foreach (var item in summaryGrade)
            {
                var grade = item.Key.Grade;
                var total = item.Count();
                startColumn = 6;

                rowHeader = sheet.GetRow(rowIndex);
                if (rowHeader == null)
                    rowHeader = sheet.CreateRow(rowIndex);

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"Grade {grade}");
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(total);
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
                rowIndex++;
            }

            #region total summary grade
            startColumn = 6;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Total");
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue(totalAll.ToString());
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;
            rowIndex++;
            #endregion
            #endregion

            #region summary session
            List<string> headerSummarySession = new List<string>();
            headerSummarySession.Add("Session");
            headerSummarySession.Add("Total Session");
            headerSummarySession.Add("Present");

            startColumn = 6;
            rowIndex += 3;
            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);
            foreach (var itemHeader in headerSummarySession)
            {
                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = data.BoldStyle;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
            }
            rowIndex++;

            var summarySession = listPresent
                        .GroupBy(e => new
                        {
                            Session = e.Session
                        })
                        .OrderBy(e => e.Key.Session)
                        .ToList();

            foreach (var item in summarySession)
            {
                var session = item.Key.Session;
                decimal total = item.Count();
                var Percentage = (decimal)(total / totalAll) * 100;
                var avg = decimal.Round(Percentage, 2, MidpointRounding.AwayFromZero).ToString();

                startColumn = 6;
                rowHeader = sheet.GetRow(rowIndex);
                if (rowHeader == null)
                    rowHeader = sheet.CreateRow(rowIndex);

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"Session {session}");
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(total.ToString());
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;

                cellNo = rowHeader.GetCell(startColumn);
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue($"{avg} %");
                cellNo.CellStyle = data.NormalStyleBody;
                sheet.AutoSizeColumn(startColumn, true);
                startColumn++;
                rowIndex++;
            }

            #region total summary grade
            startColumn = 6;

            rowHeader = sheet.GetRow(rowIndex);
            if (rowHeader == null)
                rowHeader = sheet.CreateRow(rowIndex);

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue("Total");
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;

            cellNo = rowHeader.GetCell(startColumn);
            if (cellNo == null)
                cellNo = rowHeader.CreateCell(startColumn);
            cellNo.SetCellValue(totalAll.ToString());
            cellNo.CellStyle = data.BoldStyle;
            sheet.AutoSizeColumn(startColumn, true);
            startColumn++;
            rowIndex++;
            #endregion
            #endregion
            #endregion
            #endregion

            sheet.SetColumnWidth(0, 9 * 256);
            return default;
        }

        private Uri GenerateSasUri(BlobClient blobClient)
        {
            var wit = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            var dto = new DateTimeOffset(wit, TimeSpan.FromHours(DateTimeUtil.OffsetHour));

            // set expire time
            dto = dto.AddMonths(1);

            return blobClient.GenerateSasUri(Azure.Storage.Sas.BlobSasPermissions.Read, dto);
        }

        public byte[] GenerateBlankExcel()
        {
            var result = new byte[0];
            //string[] fieldDataList = fieldData.Split(",");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Data Not Found");

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
