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
using BinusSchool.Persistence.SchedulingDb.Entities;
using FluentEmail.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.VenueReservation.VenueAndEquipmentReservationSummary
{
    public class ExportExcelEquipmentReservationSummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _context;

        public ExportExcelEquipmentReservationSummaryHandler(ISchedulingDbContext context)
        {
            _context = context;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var request = await Request.GetBody<ExportExcelEquipmentReservationSummaryRequest>();

            var response = new List<ExportExcelEquipmentReservationSummaryResponse>();

            var getEquipmentReservation = await _context.Entity<TrEquipmentReservation>()
                .Include(a => a.Equipment.EquipmentType.ReservationOwner)
                .Include(a => a.MappingEquipmentReservation.User)
                .Include(a => a.MappingEquipmentReservation.Venue)
                .Where(a => request.BookingStartDate <= a.MappingEquipmentReservation.ScheduleEndDate.Date
                    && request.BookingEndDate >= a.MappingEquipmentReservation.ScheduleStartDate.Date
                    && (string.IsNullOrEmpty(request.IdEquipmentType) ? true : a.Equipment.IdEquipmentType == request.IdEquipmentType)
                    && (string.IsNullOrEmpty(request.IdEquipment) ? true : a.IdEquipment == request.IdEquipment))
                .ToListAsync(CancellationToken);

            var equipmentGroup = getEquipmentReservation
                .GroupBy(a => new
                {
                    ScheduleDate = a.MappingEquipmentReservation.ScheduleStartDate.Date,
                    Start = a.MappingEquipmentReservation.ScheduleStartDate.TimeOfDay,
                    End = a.MappingEquipmentReservation.ScheduleEndDate.TimeOfDay,
                    IdRequester = a.MappingEquipmentReservation?.IdUser,
                    RequesterName = a.MappingEquipmentReservation?.User?.DisplayName.Trim(),
                    IdVenue = a.MappingEquipmentReservation?.IdVenue,
                    VenueName = a.MappingEquipmentReservation?.Venue?.Description ?? a.MappingEquipmentReservation?.VenueNameinEquipment,
                    Event = a.MappingEquipmentReservation?.EventDescription,
                    Notes = a.MappingEquipmentReservation?.Notes
                })
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => new ExportExcelEquipmentReservationSummaryResponse_Equipment
                    {
                        Name = e.Equipment?.EquipmentName,
                        Type = e.Equipment?.EquipmentType?.EquipmentTypeName,
                        Owner = e.Equipment?.EquipmentType?.ReservationOwner?.OwnerName,
                        BorrowingQty = e.EquipmentBorrowingQty,
                    }).ToList()
                );

            var insertEquipmentReservation = equipmentGroup
                .Select(a => new ExportExcelEquipmentReservationSummaryResponse
                {
                    ScheduleDate = a.Key.ScheduleDate,
                    Time = new ExportExcelEquipmentReservationSummaryResponse_Time
                    {
                        Start = a.Key.Start,
                        End = a.Key.End,
                    },
                    Requester = new ItemValueVm
                    {
                        Id = a.Key.IdRequester,
                        Description = a.Key.RequesterName,
                    },
                    Venue = new ItemValueVm
                    {
                        Id = a.Key.IdVenue,
                        Description = a.Key.VenueName,
                    },
                    Event = a.Key.Event,
                    Notes = a.Key.Notes,
                    Equipments = a.Value
                });

            response.AddRange(insertEquipmentReservation);

            var title = "EquipmentReservationSummary";

            if (response != null)
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

        public byte[] GenerateExcel(string sheetTitle, List<ExportExcelEquipmentReservationSummaryResponse> summaryData)
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
                var headerList = new string[] { "Schedule Date", "Time Booked", "Reserved By", "Venue", "Event", "Notes" };
                var headerEquipment = "Equipment";
                var headerEquipmentDetail = new string[] { "Name", "Type", "Owner", "Borrowing Qty" };

                int rowCell = 0;
                int indexHeader = 0;
                IRow rowHeader = excelSheet.CreateRow(rowCell);
                IRow rowHeaderEquipmentDetail = excelSheet.CreateRow(rowCell + 1);

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

                // Creating the "Equipment" header and merging the cells
                ICell cellHeaderEquipment = rowHeader.CreateCell(indexHeader);
                cellHeaderEquipment.SetCellValue(headerEquipment);
                excelSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 3));
                CellRangeAddress mergeHeader = new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 3);
                RegionUtil.SetBorderTop((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                RegionUtil.SetBorderRight((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                RegionUtil.SetBorderBottom((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                RegionUtil.SetBorderLeft((short)BorderStyle.Thin.GetHashCode(), mergeHeader, excelSheet);
                cellHeaderEquipment.CellStyle = styleTableHeader;

                // Creating the sub-headers under "Equipment"
                for (int i = indexHeader; i <= indexHeader + 3; i++)
                {
                    ICell cell = rowHeaderEquipmentDetail.CreateCell(i);
                    cell.SetCellValue(headerEquipmentDetail[i - indexHeader]);
                    cell.CellStyle = styleTableHeader;
                }

                rowCell = rowCell + 2;
                int indexBody = 0;
                int tempBody = 0;

                // body
                foreach (var body in summaryData)
                {
                    int equipmentCount = body.Equipments.Count;
                    IRow rowBody = excelSheet.CreateRow(rowCell + indexBody);

                    // Creating the main row
                    ICell scheduleDate = rowBody.CreateCell(0);
                    scheduleDate.SetCellValue(body.ScheduleDate.ToString("dd MMMM yyyy"));
                    scheduleDate.CellStyle = style;

                    ICell time = rowBody.CreateCell(1);
                    time.SetCellValue($"{body.Time.Start:hh\\:mm} - {body.Time.End:hh\\:mm}");
                    time.CellStyle = style;

                    ICell requester = rowBody.CreateCell(2);
                    requester.SetCellValue(body.Requester?.Description ?? "-");
                    requester.CellStyle = style;

                    ICell venue = rowBody.CreateCell(3);
                    venue.SetCellValue(body.Venue?.Description ?? "-");
                    venue.CellStyle = style;

                    ICell eventDesc = rowBody.CreateCell(4);
                    eventDesc.SetCellValue(body.Event ?? "-");
                    eventDesc.CellStyle = style;

                    ICell notes = rowBody.CreateCell(5);
                    notes.SetCellValue(body.Notes ?? "-");
                    notes.CellStyle = style;

                    // Merging cells if there are multiple equipment entries
                    if (equipmentCount > 1)
                    {
                        for (int i = 0; i <= 5; i++)
                        {
                            excelSheet.AddMergedRegion(new CellRangeAddress(rowCell + indexBody, rowCell + indexBody + equipmentCount - 1, i, i));
                            CellRangeAddress mergeBody = new CellRangeAddress(rowCell + indexBody, rowCell + indexBody + equipmentCount - 1, i, i);
                            RegionUtil.SetBorderTop((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                            RegionUtil.SetBorderRight((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                            RegionUtil.SetBorderBottom((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                            RegionUtil.SetBorderLeft((short)BorderStyle.Thin.GetHashCode(), mergeBody, excelSheet);
                        }
                    }

                    // Creating rows for each equipment entry
                    foreach (var equipment in body.Equipments)
                    {
                        IRow equipmentRow;
                        if (indexBody == tempBody)
                        {
                            equipmentRow = rowBody;
                            indexBody++;
                        }
                        else
                        {
                            equipmentRow = excelSheet.CreateRow(rowCell + indexBody);
                            indexBody++;
                        }

                        ICell equipmentName = equipmentRow.CreateCell(6);
                        equipmentName.SetCellValue(equipment.Name);
                        equipmentName.CellStyle = style;

                        ICell equipmentType = equipmentRow.CreateCell(7);
                        equipmentType.SetCellValue(equipment.Type);
                        equipmentType.CellStyle = style;

                        ICell equipmentOwner = equipmentRow.CreateCell(8);
                        equipmentOwner.SetCellValue(equipment.Owner);
                        equipmentOwner.CellStyle = style;

                        ICell equipmentQty = equipmentRow.CreateCell(9);
                        equipmentQty.SetCellValue(equipment.BorrowingQty);
                        equipmentQty.CellStyle = style;

                        if (equipment == body.Equipments.Last())
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
