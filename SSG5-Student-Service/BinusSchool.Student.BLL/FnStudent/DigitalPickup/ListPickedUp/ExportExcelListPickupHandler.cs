using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.DigitalPickup.ListPickedUp;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.DigitalPickup.ListPickedUp
{
    public class ExportExcelListPickupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;

        public ExportExcelListPickupHandler(IStudentDbContext dbContext, IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<ExportExcelListPickupRequest>(
                nameof(ExportExcelListPickupRequest.IdAcademicYear), nameof(ExportExcelListPickupRequest.Semester),
                nameof(ExportExcelListPickupRequest.StartDate), nameof(ExportExcelListPickupRequest.EndDate));

            var pickupData = await _dbContext.Entity<TrDigitalPickup>()
                .Where(x => x.Date.Date >= param.StartDate.Date && x.Date.Date <= param.EndDate.Date &&
                            x.IdAcademicYear == param.IdAcademicYear &&
                            x.Semester == param.Semester)
                .Join(
                    _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.Grade)
                        .ThenInclude(x => x.MsLevel)
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.MsGradePathwayClassroom)
                        .ThenInclude(x => x.Classroom)
                    .Include(x => x.Student)
                    .Where(x => x.Homeroom.Grade.IdLevel == (param.IdLevel == null ? x.Homeroom.Grade.IdLevel : param.IdLevel))
                    .Where(x => x.Homeroom.IdGrade == (param.IdGrade == null ? x.Homeroom.IdGrade : param.IdGrade))
                    .Where(x => x.IdHomeroom == (param.IdHomeroom == null ? x.IdHomeroom : param.IdHomeroom)),
                    pickup => new { pickup.IdStudent, pickup.IdAcademicYear, pickup.Semester },
                    student => new { student.IdStudent, student.Homeroom.Grade.MsLevel.IdAcademicYear, student.Semester },
                    (pickup, student) => new ExportExcelListPickup_PickupData
                    {
                        Student = new NameValueVm
                        {
                            Id = pickup.IdStudent,
                            Name = NameUtil.GenerateFullName(student.Student.FirstName, student.Student.MiddleName, student.Student.LastName)
                        },
                        Date = pickup.Date,
                        Homeroom = student.Homeroom.Grade.Code + student.Homeroom.MsGradePathwayClassroom.Classroom.Code,
                        QrScanTime = pickup.QrScanTime,
                        PickupTime = pickup.PickupTime
                    })
                .OrderBy(x => x.Date).ThenBy(x => x.Homeroom).ThenBy(x => x.QrScanTime).ThenBy(x => x.PickupTime).ThenBy(x => x.Student.Id)
                .ToListAsync(CancellationToken);

            var headerData = await _dbContext.Entity<MsAcademicYear>()
                .Where(x => x.Id == param.IdAcademicYear)
                .Select(x => new
                {
                    AcademicYearDescription = x.Description,
                    SchoolDescription = x.MsSchool.Description,
                }).FirstOrDefaultAsync(CancellationToken);

            var resultData = new ExportExcelListPickupResult
            {
                SchoolName = headerData.SchoolDescription,
                AcademicYear = headerData.AcademicYearDescription,
                Semester = param.Semester.ToString(),
                PickupData = pickupData
            };

            if (param.IdLevel != null)
            {
                var LevelData = await _dbContext.Entity<MsLevel>()
                    .Where(x => param.IdLevel.Contains(x.Id))
                    .Select(x => x.Code)
                    .OrderBy(x => x)
                    .ToListAsync(CancellationToken);

                resultData.Level = string.Join("; ", LevelData);
            }

            if (param.IdGrade != null)
            {
                var GradeData = await _dbContext.Entity<MsGrade>()
                    .Where(x => param.IdGrade.Contains(x.Id))
                    .Select(x => x.Description)
                    .OrderBy(x => x)
                    .ToListAsync(CancellationToken);

                resultData.Grade = string.Join("; ", GradeData);
            }

            if (param.IdHomeroom != null)
            {
                var ClassData = await _dbContext.Entity<MsHomeroom>()
                    .Include(x => x.Grade)
                    .Include(x => x.MsGradePathwayClassroom).ThenInclude(x => x.Classroom)
                    .Where(x => param.IdHomeroom.Contains(x.Id))
                    .Select(x => x.Grade.Code + x.MsGradePathwayClassroom.Classroom.Code)
                    .OrderBy(x => x)
                    .ToListAsync(CancellationToken);

                resultData.Class = string.Join("; ", ClassData);
            }

            var title = "List Pickup";

            if (resultData != null)
            {
                var generateExcelByte = GenerateExcel(resultData);
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
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook = new XSSFWorkbook();
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
                font.FontName = "Arial";
                font.FontHeightInPoints = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(2);

                workbook.Write(ms);

                return ms.ToArray();
            }
        }

        public void SetDynamicColumnWidthExcel(int columnComponentCount, ref ISheet excelSheet)
        {
            for (int i = 0; i < columnComponentCount; i++)
            {
                excelSheet.SetColumnWidth(i, 30 * 256);
            }
        }

        public byte[] GenerateExcel(ExportExcelListPickupResult Data)
        {
            var result = new byte[0];
            var pickupData = Data.PickupData;

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet ListPickup = workbook.CreateSheet("List Pickup");

                int columnComponentCount = 6;

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

                //Create style for data
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

                //Create style for label
                ICellStyle labelStyle = workbook.CreateCellStyle();

                //Set border style 
                labelStyle.BorderBottom = BorderStyle.Thin;
                labelStyle.BorderLeft = BorderStyle.Thin;
                labelStyle.BorderRight = BorderStyle.Thin;
                labelStyle.BorderTop = BorderStyle.Thin;
                labelStyle.VerticalAlignment = VerticalAlignment.Center;
                labelStyle.Alignment = HorizontalAlignment.Left;

                //Set font style
                labelStyle.SetFont(font);

                //Header

                IRow row = ListPickup.CreateRow(0);
                var cellTitleRow = row.CreateCell(0);
                cellTitleRow.SetCellValue("List Pickup");
                cellTitleRow.CellStyle = headerStyle;
                ListPickup.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, 1));

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("School Name:");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(Data.SchoolName);
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Academic Year :");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(Data.AcademicYear);
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Semester :");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(Data.Semester);
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Level :");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(Data.Level);
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Grade :");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(Data.Grade);
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Class :");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(Data.Class);
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Generated Date :");
                row.Cells[0].CellStyle = labelStyle;
                row.CreateCell(1).SetCellValue(_dateTime.ServerTime.ToString("dd-MM-yyyy HH:mm"));
                row.Cells[1].CellStyle = dataStyle;

                row = ListPickup.CreateRow(row.RowNum + 1);

                //Body
                row = ListPickup.CreateRow(row.RowNum + 2);
                row.CreateCell(0).SetCellValue("Date");
                row.Cells[0].CellStyle = headerStyle;
                row.CreateCell(1).SetCellValue("Homeroom");
                row.Cells[1].CellStyle = headerStyle;
                row.CreateCell(2).SetCellValue("Student ID");
                row.Cells[2].CellStyle = headerStyle;
                row.CreateCell(3).SetCellValue("Student Name");
                row.Cells[3].CellStyle = headerStyle;
                row.CreateCell(4).SetCellValue("QR Scanned Time");
                row.Cells[4].CellStyle = headerStyle;
                row.CreateCell(5).SetCellValue("Picked Up Time");
                row.Cells[5].CellStyle = headerStyle;

                foreach (var item in pickupData)
                {

                    row = ListPickup.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue(item.Date.ToString("dd-MM-yyyy"));
                    row.Cells[0].CellStyle = dataStyle;
                    row.CreateCell(1).SetCellValue(item.Homeroom);
                    row.Cells[1].CellStyle = dataStyle;
                    row.CreateCell(2).SetCellValue(item.Student.Id);
                    row.Cells[2].CellStyle = dataStyle;
                    row.CreateCell(3).SetCellValue(item.Student.Name);
                    row.Cells[3].CellStyle = dataStyle;
                    row.CreateCell(4).SetCellValue(item.QrScanTime.ToString("dd-MM-yyyy HH:mm"));
                    row.Cells[4].CellStyle = dataStyle;
                    row.CreateCell(5).SetCellValue(item.PickupTime?.ToString("dd-MM-yyyy HH:mm") ?? "-");
                    row.Cells[5].CellStyle = dataStyle;
                }

                SetDynamicColumnWidthExcel(columnComponentCount, ref ListPickup);

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
