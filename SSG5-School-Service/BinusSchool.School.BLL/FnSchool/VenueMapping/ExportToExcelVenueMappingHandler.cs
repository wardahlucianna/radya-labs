using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Data.Model.School.FnSchool.VenueMapping;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnSchool.VenueMapping.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.School.FnSchool.VenueMapping
{
    public class ExportToExcelVenueMappingHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        public ExportToExcelVenueMappingHandler(ISchoolDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportToExcelVenueMappingRequest, ExportToExcelVenueMappingValidator>();

            var getVenueMapping = await _dbContext.Entity<MsVenueMapping>()
                .Include(x => x.Venue).ThenInclude(x => x.Building)
                .Include(x => x.AcademicYear).ThenInclude(x => x.School)
                .Include(x => x.Floor)
                .Include(x => x.VenueType)
                .Include(x => x.ReservationOwner)
                .Include(x => x.VenueMappingApprovals).ThenInclude(x => x.Staff)
                .Where(x => x.IdAcademicYear == param.IdAcademicYear)
                .Where(x => x.Venue.IdBuilding == (param.IdBuilding ?? x.Venue.IdBuilding))
                .ToListAsync(CancellationToken);

            var exportData = new ExportToExcelVenueMappingData
            {
                School = getVenueMapping.FirstOrDefault().AcademicYear.School.Name,
                AcademicYear = getVenueMapping.FirstOrDefault().AcademicYear.Code,
                Building = param.IdBuilding == null ?
                    "All" :
                    getVenueMapping.Select(x => x.Venue.Building.Description).FirstOrDefault(),
                MappingList = getVenueMapping.Select(x => new ExportToExcelVenueMappingData_MappingList
                {
                    VenueName = x.Venue.Description,
                    Building = x.Venue.Building.Description,
                    Floor = x.Floor.FloorName,
                    VenueType = x.VenueType.VenueTypeName,
                    Owner = x.ReservationOwner.OwnerName,
                    Description = x.Description == null ? "-" : x.Description,
                    Active = x.IsVenueActive ? "Yes" : "No",
                    NeedApproval = x.IsNeedApproval ? "Yes" : "No",
                    ApproverName = !x.IsNeedApproval ? "-" :
                        String.Join(", ", x.VenueMappingApprovals.Select(x => NameUtil.GenerateFullName(x.Staff.FirstName, x.Staff.LastName)).ToList())
                }).ToList()
            };

            var title = "List Mapping Venue";

            if (exportData != null)
            {
                var generateExcelByte = GenerateExcel(exportData);
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

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }


        public void SetDynamicColumnWidthExcel(int columnComponentCount, ref ISheet excelSheet)
        {
            for (int i = 0; i < columnComponentCount; i++)
            {
                excelSheet.SetColumnWidth(i, 30 * 256);
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

        public byte[] GenerateExcel(ExportToExcelVenueMappingData data)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("List Mapping Venue");

                int columnComponentCount = 9;

                //Create style for header
                ICellStyle headerStyle = workbook.CreateCellStyle();

                //Set border style 
                headerStyle.BorderBottom = BorderStyle.Thin;
                headerStyle.BorderLeft = BorderStyle.Thin;
                headerStyle.BorderRight = BorderStyle.Thin;
                headerStyle.BorderTop = BorderStyle.Thin;
                headerStyle.VerticalAlignment = VerticalAlignment.Center;
                headerStyle.Alignment = HorizontalAlignment.Center;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsBold = true;
                headerStyle.SetFont(font);

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

                //header 
                //IRow row = excelSheet.CreateRow(0);

                //Title 

                IRow row = excelSheet.CreateRow(0);
                row.CreateCell(0).SetCellValue("School: ");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(data.School);
                row.Cells[1].CellStyle = dataStyle;

                row = excelSheet.CreateRow(row.RowNum + 1);

                row.CreateCell(0).SetCellValue("Academic Year: ");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(data.AcademicYear);
                row.Cells[1].CellStyle = dataStyle;

                row = excelSheet.CreateRow(row.RowNum + 1);

                row.CreateCell(0).SetCellValue("Building: ");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(data.Building);
                row.Cells[1].CellStyle = dataStyle;

                row = excelSheet.CreateRow(row.RowNum + 1);

                //Body

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Venue Name");
                row.Cells[0].CellStyle = headerStyle;
                row.CreateCell(1).SetCellValue("Building");
                row.Cells[1].CellStyle = headerStyle;
                row.CreateCell(2).SetCellValue("Floor");
                row.Cells[2].CellStyle = headerStyle;
                row.CreateCell(3).SetCellValue("Venue Type");
                row.Cells[3].CellStyle = headerStyle;
                row.CreateCell(4).SetCellValue("Owner");
                row.Cells[4].CellStyle = headerStyle;
                row.CreateCell(5).SetCellValue("Description");
                row.Cells[5].CellStyle = headerStyle;
                row.CreateCell(6).SetCellValue("Active");
                row.Cells[6].CellStyle = headerStyle;
                row.CreateCell(7).SetCellValue("Need Approval");
                row.Cells[7].CellStyle = headerStyle;
                row.CreateCell(8).SetCellValue("Approver Name");
                row.Cells[8].CellStyle = headerStyle;

                foreach (var mapping in data.MappingList)
                {
                    row = excelSheet.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue(mapping.VenueName);
                    row.Cells[0].CellStyle = dataStyle;
                    row.CreateCell(1).SetCellValue(mapping.Building);
                    row.Cells[1].CellStyle = dataStyle;
                    row.CreateCell(2).SetCellValue(mapping.Floor);
                    row.Cells[2].CellStyle = dataStyle;
                    row.CreateCell(3).SetCellValue(mapping.VenueType);
                    row.Cells[3].CellStyle = dataStyle;
                    row.CreateCell(4).SetCellValue(mapping.Owner);
                    row.Cells[4].CellStyle = dataStyle;
                    row.CreateCell(5).SetCellValue(mapping.Description);
                    row.Cells[5].CellStyle = dataStyle;
                    row.CreateCell(6).SetCellValue(mapping.Active);
                    row.Cells[6].CellStyle = dataStyle;
                    row.CreateCell(7).SetCellValue(mapping.NeedApproval);
                    row.Cells[7].CellStyle = dataStyle;
                    row.CreateCell(8).SetCellValue(mapping.ApproverName);
                    row.Cells[8].CellStyle = dataStyle;
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


        public class ExportToExcelVenueMappingData
        {
            public string School { get; set; }
            public string AcademicYear { get; set; }
            public string Building { get; set; }
            public List<ExportToExcelVenueMappingData_MappingList> MappingList { get; set; }
        }

        public class ExportToExcelVenueMappingData_MappingList
        {
            public string VenueName { get; set; }
            public string Building { get; set; }
            public string Floor { get; set; }
            public string VenueType { get; set; }
            public string Owner { get; set; }
            public string Description { get; set; }
            public string Active { get; set; }
            public string NeedApproval { get; set; }
            public string ApproverName { get; set; }
        }
    }
}
