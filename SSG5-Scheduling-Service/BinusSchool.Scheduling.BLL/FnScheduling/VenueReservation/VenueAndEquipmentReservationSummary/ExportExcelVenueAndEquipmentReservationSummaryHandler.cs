using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class ExportExcelVenueAndEquipmentReservationSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IVenueReservation _venueReservation;

        public ExportExcelVenueAndEquipmentReservationSummaryHandler(ISchedulingDbContext context, IVenueReservation venueReservation)
        {
            _context = context;
            _venueReservation = venueReservation;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = await Request.GetBody<ExportExcelVenueAndEquipmentReservationSummaryRequest>();

            var venueAndEquipmentReservationSummaryResponse = new List<GetVenueAndEquipmentReservationSummaryResponse>();

            var getVenueAndEquipmentReservationSummary = await _venueReservation.GetVenueAndEquipmentReservationSummary(new GetVenueAndEquipmentReservationSummaryRequest
            {
                BookingStartDate = request.BookingStartDate,
                BookingEndDate = request.BookingEndDate,
                IdBuilding = request.IdBuilding,
                IdVenue = request.IdVenue,
                ApprovalStatus = request.ApprovalStatus
            });

            venueAndEquipmentReservationSummaryResponse.AddRange(getVenueAndEquipmentReservationSummary.Payload);

            var sortedData = venueAndEquipmentReservationSummaryResponse
                .OrderByDescending(a => a.ScheduleDate)
                .ToList();

            var title = "VenueAndEquipmentReservationSummary";

            if (venueAndEquipmentReservationSummaryResponse != null)
            {
                var generateExcel = GenerateExcel(title, sortedData);
                return new FileContentResult(generateExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcel = GenerateBlankExcel(title);
                return new FileContentResult(generateExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
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

        public byte[] GenerateExcel(string sheetTitle, List<GetVenueAndEquipmentReservationSummaryResponse> summaryData)
        {
            var result = new byte[0];

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
                ICellStyle styleTableHeader = workbook.CreateCellStyle();
                ICellStyle styleHyperLink = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                styleTableHeader.BorderBottom = BorderStyle.Thin;
                styleTableHeader.BorderLeft = BorderStyle.Thin;
                styleTableHeader.BorderRight = BorderStyle.Thin;
                styleTableHeader.BorderTop = BorderStyle.Thin;

                styleHyperLink.BorderBottom = BorderStyle.Thin;
                styleHyperLink.BorderLeft = BorderStyle.Thin;
                styleHyperLink.BorderRight = BorderStyle.Thin;
                styleHyperLink.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                font.FontName = "Aptos Narrow";
                font.FontHeightInPoints = 11;
                style.SetFont(font);

                IFont fontTableHeader = workbook.CreateFont();
                fontTableHeader.FontName = "Aptos Narrow";
                fontTableHeader.FontHeightInPoints = 11;
                fontTableHeader.IsBold = true;
                styleTableHeader.SetFont(fontTableHeader);

                IFont fontHyperLink = workbook.CreateFont();
                fontHyperLink.FontName = "Aptos Narrow";
                fontHyperLink.FontHeightInPoints = 11;
                fontHyperLink.Color = IndexedColors.Blue.Index;
                styleHyperLink.SetFont(fontHyperLink);

                // set allignment
                style.VerticalAlignment = VerticalAlignment.Top;
                style.WrapText = true;

                styleTableHeader.Alignment = HorizontalAlignment.Center;

                // table header
                var headerList = new string[] {"Schedule Date", "Time Booked", "Building", "Venue", "Reserved By", "Event", "Equipment", "Venue Approver", "Venue Status", "Notes", "Prep & Clean Up", "Layout"};

                int rowCell = 0;
                int indexHeader = 0;
                IRow rowHeader = excelSheet.CreateRow(rowCell);

                foreach (var header in headerList)
                {
                    ICell cellHeader = rowHeader.CreateCell(indexHeader);
                    cellHeader.SetCellValue(header);
                    cellHeader.CellStyle = styleTableHeader;
                    indexHeader++;
                }

                rowCell++;

                int indexBody = 0;

                foreach (var body in summaryData)
                {
                    IRow rowBody = excelSheet.CreateRow(rowCell + indexBody);

                    ICell scheduleDate = rowBody.CreateCell(0);
                    scheduleDate.SetCellValue(body.ScheduleDate.ToString("dd MMMM yyyy"));
                    scheduleDate.CellStyle = style;

                    ICell time = rowBody.CreateCell(1);
                    time.SetCellValue($"{body.Time.Start:hh\\:mm} - {body.Time.End:hh\\:mm}");
                    time.CellStyle = style;

                    ICell building = rowBody.CreateCell(2);
                    building.SetCellValue(body.Building.Description);
                    building.CellStyle = style;

                    ICell venue = rowBody.CreateCell(3);
                    venue.SetCellValue(body.Venue.Description);
                    venue.CellStyle = style;

                    ICell requester = rowBody.CreateCell(4);
                    requester.SetCellValue(body.Requester.Description);
                    requester.CellStyle = style;

                    ICell eventDesc = rowBody.CreateCell(5);
                    eventDesc.SetCellValue(body.Event);
                    eventDesc.CellStyle = style;

                    ICell equipment = rowBody.CreateCell(6);
                    equipment.SetCellValue(string.Join(", " , body.Equipments?.OrderBy(a => a.EquipmentName).Select(a => a.EquipmentName + " (" + a.EquipmentBorrowingQty.ToString() + ")")) ?? "-");
                    equipment.CellStyle = style;

                    ICell venueApprover = rowBody.CreateCell(7);
                    venueApprover.SetCellValue(string.Join(", ", body.VenueApprovalUsers?.OrderBy(a => a.Description).Select(a => a.Description)) ?? "-");
                    venueApprover.CellStyle = style;

                    ICell venueStatus = rowBody.CreateCell(8);
                    venueStatus.SetCellValue(body.BookingStatus.BookingDesc);
                    venueStatus.CellStyle = style;

                    ICell notes = rowBody.CreateCell(9);
                    notes.SetCellValue(body.Note ?? "-");
                    notes.CellStyle = style;

                    ICell additionalTime = rowBody.CreateCell(10);
                    additionalTime.SetCellValue($"{body.PreparationTime ?? null} - {body.CleanUpTime ?? null}");
                    additionalTime.CellStyle = style;

                    ICell layout = rowBody.CreateCell(11);
                    string url = body.FileUpload?.Url;
                    layout.SetCellValue(body.FileUpload.Url == null ? "-" : "Download");

                    if (body.FileUpload.Url != null)
                    {
                        IHyperlink link = workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.Url);
                        link.Address = url;
                        layout.Hyperlink = link;
                    }
                        
                    layout.CellStyle = body.FileUpload.Url == null ? style : styleHyperLink;

                    indexBody++;
                }

                for (int columnIndex = 0; columnIndex <= 11; columnIndex++)
                {
                    excelSheet.AutoSizeColumn(columnIndex);
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
