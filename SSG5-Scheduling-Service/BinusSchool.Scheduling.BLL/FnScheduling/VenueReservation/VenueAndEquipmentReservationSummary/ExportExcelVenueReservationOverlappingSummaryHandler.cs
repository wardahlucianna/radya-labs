using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnSchedule;
using BinusSchool.Data.Model.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class ExportExcelVenueReservationOverlappingSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;
        private readonly IVenueReservation _venueReservation;
        private readonly GetVenueReservationOverlappingSummaryHandler _overlapSummary;

        public ExportExcelVenueReservationOverlappingSummaryHandler(ISchedulingDbContext context, IVenueReservation venueReservation, GetVenueReservationOverlappingSummaryHandler overlapSummary)
        {
            _context = context;
            _venueReservation = venueReservation;
            _overlapSummary = overlapSummary;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = await Request.GetBody<GetVenueReservationOverlappingSummaryRequest>();

            if (request.BookingStartDate == null || request.BookingEndDate == null || (request.BookingStartDate == null && request.BookingEndDate == null))
                throw new BadRequestException($"BookingStartDate or BookingEndDate cannot be null.");

            var response = new List<GetVenueReservationOverlappingSummaryResponse>();

            var venueReservationOverlappingSummaries = await _venueReservation.GetVenueReservationOverlappingSummary(request);

            response.AddRange(venueReservationOverlappingSummaries.Payload);

            // for testing
            // var venueReservationOverlappingSummaries = await _overlapSummary.GetVenueReservationOverlappingSummary(request);
            // response.AddRange(venueReservationOverlappingSummaries);

            var title = "VenueReservationOverlappingSummary";

            if (response.Count > 0)
            {
                var generateExcel = GenerateExcel(title, response);

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

        public byte[] GenerateBlankExcel(string title)
        {
            var result = new byte[0];

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var TitleValidated = regex.Replace(title, " ");
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

        public byte[] GenerateExcel(string title, List<GetVenueReservationOverlappingSummaryResponse> response)
        {
            var result = new byte[0];

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var TitleValidated = regex.Replace(title, " ");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(TitleValidated);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleTableHeader = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                styleTableHeader.BorderBottom = BorderStyle.Thin;
                styleTableHeader.BorderLeft = BorderStyle.Thin;
                styleTableHeader.BorderRight = BorderStyle.Thin;
                styleTableHeader.BorderTop = BorderStyle.Thin;

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

                // set allignment
                style.VerticalAlignment = VerticalAlignment.Center;
                style.Alignment = HorizontalAlignment.Left;
                style.WrapText = true;

                styleTableHeader.VerticalAlignment = VerticalAlignment.Center;
                styleTableHeader.Alignment = HorizontalAlignment.Center;

                // header
                var headerList = new string[] { "Schedule Date", "Time Booked", "Building", "Venue", "Reserved By", "Event"};
                var headerOverlap = "Overlapping with";
                var headerOverlapDetail = new string[] { "Teacher", "Time", "Subject", "From" };

                int rowCell = 0;
                int indexHeader = 0;
                IRow rowHeader = excelSheet.CreateRow(rowCell);
                IRow rowHeaderOverlapDetail = excelSheet.CreateRow(rowCell + 1);

                // Creating main headers and merging cells for the main headers
                foreach (var header in headerList)
                {
                    ICell cellHeader = rowHeader.CreateCell(indexHeader);
                    cellHeader.SetCellValue(header);
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell + 1, indexHeader, indexHeader));
                    cellHeader.CellStyle = styleTableHeader;

                    CellRangeAddress mergedRegion = new CellRangeAddress(rowCell, rowCell + 1, indexHeader, indexHeader);
                    RegionUtil.SetBorderTop((short)BorderStyle.Thin.GetHashCode(), mergedRegion, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin.GetHashCode(), mergedRegion, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin.GetHashCode(), mergedRegion, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin.GetHashCode(), mergedRegion, excelSheet);

                    indexHeader++;
                }

                // Creating the "Overlap" header and merging the cells
                ICell cellHeaderOverlap = rowHeader.CreateCell(indexHeader);
                cellHeaderOverlap.SetCellValue(headerOverlap);
                excelSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 3));
                CellRangeAddress mergeHeader = new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 3);
                RegionUtil.SetBorderTop((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                RegionUtil.SetBorderRight((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                RegionUtil.SetBorderBottom((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                RegionUtil.SetBorderLeft((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                cellHeaderOverlap.CellStyle = styleTableHeader;

                // Creating the sub-headers under "Overlap"
                for (int i = indexHeader; i <= indexHeader + 3; i++)
                {
                    ICell cell = rowHeaderOverlapDetail.CreateCell(i);
                    cell.SetCellValue(headerOverlapDetail[i - indexHeader]);
                    cell.CellStyle = styleTableHeader;
                }

                rowCell = rowCell + 2;
                int indexBody = 0;
                int tempBody = 0;

                // body
                foreach (var body in response)
                {
                    int overlapCount = body.Overlap.Count;
                    IRow rowBody = excelSheet.CreateRow(rowCell + indexBody);

                    // Creating the main row
                    ICell scheduleDate = rowBody.CreateCell(0);
                    scheduleDate.SetCellValue(body.ScheduleDate.ToString("dd MMMM yyyy"));
                    scheduleDate.CellStyle = style;

                    ICell time = rowBody.CreateCell(1);
                    time.SetCellValue($"{body.Time.Start:hh\\:mm} - {body.Time.End:hh\\:mm}");
                    time.CellStyle = style;

                    ICell building = rowBody.CreateCell(2);
                    building.SetCellValue(body.Building?.Description ?? "-");
                    building.CellStyle = style;

                    ICell venue = rowBody.CreateCell(3);
                    venue.SetCellValue(body.Venue?.Description ?? "-");
                    venue.CellStyle = style;

                    ICell requester = rowBody.CreateCell(4);
                    requester.SetCellValue(body.Requester?.Description ?? "-");
                    requester.CellStyle = style;

                    ICell eventDesc = rowBody.CreateCell(5);
                    eventDesc.SetCellValue(body.Event ?? "-");
                    eventDesc.CellStyle = style;

                    // Merging cells if there are multiple overlap entries
                    if (overlapCount > 1)
                    {
                        for (int i = 0; i <= 5; i++)
                        {
                            excelSheet.AddMergedRegion(new CellRangeAddress(rowCell + indexBody, rowCell + indexBody + overlapCount - 1, i, i));
                            CellRangeAddress mergeBody = new CellRangeAddress(rowCell + indexBody, rowCell + indexBody + overlapCount - 1, i, i);
                            RegionUtil.SetBorderTop((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                            RegionUtil.SetBorderRight((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                            RegionUtil.SetBorderBottom((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                            RegionUtil.SetBorderLeft((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                        }
                    }

                    // Creating rows for each overlap entry
                    foreach (var overlap in body.Overlap)
                    {
                        IRow overlapRow;
                        if (indexBody == tempBody)
                        {
                            overlapRow = rowBody;
                            indexBody++;
                        }
                        else
                        {
                            overlapRow = excelSheet.CreateRow(rowCell + indexBody);
                            indexBody++;
                        }

                        ICell overlapTeacher = overlapRow.CreateCell(6);
                        overlapTeacher.SetCellValue(overlap.Teacher.Description);
                        overlapTeacher.CellStyle = style;

                        ICell overlapTime = overlapRow.CreateCell(7);
                        overlapTime.SetCellValue($"{overlap.Time.Start:hh\\:mm} - {overlap.Time.End:hh\\:mm}");
                        overlapTime.CellStyle = style;

                        ICell overlapSubject = overlapRow.CreateCell(8);
                        overlapSubject.SetCellValue(overlap.Subject.Description);
                        overlapSubject.CellStyle = style;

                        ICell overlapFrom = overlapRow.CreateCell(9);
                        overlapFrom.SetCellValue(overlap.OverlapFrom);
                        overlapFrom.CellStyle = style;

                        if (overlap == body.Overlap.Last())
                            tempBody = indexBody;
                    }
                }

                rowCell += indexBody;

                // Auto-sizing the columns
                for (int columnIndex = 0; columnIndex <= 9; columnIndex++)
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
