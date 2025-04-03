using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class ExportExcelCurrentAvailableEquipmentStockHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IVenueReservation _venueReservation;
        private readonly IMachineDateTime _dateTime;
        public ExportExcelCurrentAvailableEquipmentStockHandler(ISchedulingDbContext dbContext,
            IVenueReservation venueReservation,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _venueReservation = venueReservation;
            _dateTime = dateTime;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var body = await Request.GetBody<ExportExcelCurrentAvailableEquipmentStockRequest>();

            var getAvailableStock = await _venueReservation.GetCurrentAvailableEquipmentStock(body);
            var availableStock = getAvailableStock.Payload.ToList();

            var title = "Current Available Equipment Stock";

            if (availableStock != null)
            {
                var generateExcelByte = GenerateExcel(availableStock, body.BookingStartDate.Date, body.BookingEndDate.Date, body.StartTime, body.EndTime);
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
        public void SetDynamicColumnWidthExcel(int columnComponentCount, ref ISheet excelSheet)
        {
            for (int i = 0; i < columnComponentCount; i++)
            {
                excelSheet.SetColumnWidth(i, 30 * 256);
            }
        }

        public byte[] GenerateExcel(List<GetCurrentAvailableEquipmentStockResult> availableStock, DateTime bookingStartDate, DateTime bookingEndDate, TimeSpan startTime, TimeSpan endTime)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Current Available Equipment Stock");

                int columnComponentCount = 3;

                //Create style for header
                ICellStyle headerStyle = workbook.CreateCellStyle();

                //Set border style 
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.VerticalAlignment = VerticalAlignment.Center;
                headerStyle.Alignment = HorizontalAlignment.Left;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                headerStyle.SetFont(font);

                ICellStyle columnHeaderStyle = workbook.CreateCellStyle();
                columnHeaderStyle.CloneStyleFrom(headerStyle);
                columnHeaderStyle.FillPattern = FillPattern.SolidForeground;
                columnHeaderStyle.FillForegroundColor = IndexedColors.LightCornflowerBlue.Index;

                //Create style for header
                ICellStyle dataStyle = workbook.CreateCellStyle();

                //Set border style 
                dataStyle.BorderBottom = BorderStyle.Thin;
                dataStyle.BorderLeft = BorderStyle.Thin;
                dataStyle.BorderRight = BorderStyle.Thin;
                dataStyle.BorderTop = BorderStyle.Thin;
                dataStyle.VerticalAlignment = VerticalAlignment.Center;
                dataStyle.Alignment = HorizontalAlignment.Left;
                dataStyle.WrapText = true;

                //Set font style
                IFont Datafont = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                Datafont.FontName = "Arial";
                Datafont.FontHeightInPoints = 12;
                dataStyle.SetFont(Datafont);

                //Title 
                IRow row = excelSheet.CreateRow(0);

                row = excelSheet.CreateRow(row.RowNum);
                row.CreateCell(0).SetCellValue("Summary of Current Available Equipment Stock");
                row.Cells[0].CellStyle = headerStyle;
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, columnComponentCount));
                

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue($"Booking Period: From {bookingStartDate.ToString("dd MMMM yyyy")} to {bookingEndDate.ToString("dd MMMM yyyy")}, each day from {startTime.ToString(@"hh\:mm")} to {endTime.ToString(@"hh\:mm")}");
                row.Cells[0].CellStyle = headerStyle;
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, columnComponentCount));

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue($"This information is valid as of {_dateTime.ServerTime.ToString("dd MMMM yyyy hh:mm tt")}");
                row.Cells[0].CellStyle = headerStyle;
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, columnComponentCount));

                row = excelSheet.CreateRow(row.RowNum + 1);

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Equipment Type");
                row.Cells[0].CellStyle = columnHeaderStyle;
                row.CreateCell(1).SetCellValue("Equipment Name");
                row.Cells[1].CellStyle = columnHeaderStyle;
                row.CreateCell(2).SetCellValue("Max Borrowing Quantity");
                row.Cells[2].CellStyle = columnHeaderStyle;
                row.CreateCell(3).SetCellValue("Current Available Stock");
                row.Cells[3].CellStyle = columnHeaderStyle;

                foreach (var item in availableStock)
                {
                    row = excelSheet.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue(item.EquipmentType.Name);
                    row.Cells[0].CellStyle = dataStyle;
                    row.CreateCell(1).SetCellValue(item.EquipmentName);
                    row.Cells[1].CellStyle = dataStyle;
                    row.CreateCell(2).SetCellValue(item.MaxQtyBorrowing == null ? "-" : item.MaxQtyBorrowing.ToString());
                    row.Cells[2].CellStyle = dataStyle;
                    row.CreateCell(3).SetCellValue(item.CurrentAvailableStock.ToString());
                    row.Cells[3].CellStyle = dataStyle;
                }

                SetDynamicColumnWidthExcel(columnComponentCount, ref excelSheet);

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
