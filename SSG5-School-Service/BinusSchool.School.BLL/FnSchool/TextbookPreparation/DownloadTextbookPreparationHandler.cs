using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.School.FnSchool.TextbookPreparation;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.School.FnSchool.TextbookPreparation
{
    public class DownloadTextbookPreparationHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        public DownloadTextbookPreparationHandler(ISchoolDbContext DbContext, IMachineDateTime DateTime)
        {
            _dbContext = DbContext;
            _dateTime = DateTime;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetTextbookPreparationRequest>();

            var GetAcademicYear = await _dbContext.Entity<MsAcademicYear>()
                .Where(e => e.Id == param.IdAcademicYear)
               .FirstOrDefaultAsync(CancellationToken);

            if (GetAcademicYear == null)
                throw new BadRequestException($"Academic year with Id: {param.IdAcademicYear} is not found");

            var predicate = PredicateBuilder.Create<TrTextbook>(x => x.IdBinusianCreated==param.IdUser);

            if (!string.IsNullOrEmpty(param.IdAcademicYear))
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);

            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Subject.Grade.IdLevel == param.IdLevel);

            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.Subject.IdGrade == param.IdGrade);

            if (!string.IsNullOrEmpty(param.Status))
            {
                if (param.Status == TextbookPreparationStatus.Hold.GetDescription())
                {
                    predicate = predicate.And(x => x.Status == TextbookPreparationStatus.Hold);
                }
                if (param.Status == TextbookPreparationStatus.OnReview1.GetDescription())
                {
                    predicate = predicate.And(x => x.Status == TextbookPreparationStatus.OnReview1);
                }
                if (param.Status == TextbookPreparationStatus.OnReview2.GetDescription())
                {
                    predicate = predicate.And(x => x.Status == TextbookPreparationStatus.OnReview2);
                }
                if (param.Status == TextbookPreparationStatus.OnReview3.GetDescription())
                {
                    predicate = predicate.And(x => x.Status == TextbookPreparationStatus.OnReview3);
                }
                if (param.Status == TextbookPreparationStatus.Declined.GetDescription())
                {
                    predicate = predicate.And(x => x.Status == TextbookPreparationStatus.Declined);
                }
                if (param.Status == TextbookPreparationStatus.Approved.GetDescription())
                {
                    predicate = predicate.And(x => x.Status == TextbookPreparationStatus.Approved);
                }
            }
            //serach
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Subject.Description.Contains(param.Search) || x.Title.Contains(param.Search));

            var GetTextbook = await _dbContext.Entity<TrTextbook>()
                .Include(e => e.AcademicYear)
                .Include(e => e.Subject)
                .Where(predicate)
               .Select(x => new DownloadTextbookPreparationResult
               {
                   IdAcademicYear = x.AcademicYear.Description,
                   Level = x.Subject.Grade.Level.Code,
                   Grade = x.Subject.Grade.Code,
                   SubjectGroup = x.TextbookSubjectGroup.SubjectGroupName,
                   Subject = x.Subject.Description,
                   Streaming = x.Pathway.Description,
                   ISBN = x.ISBN,
                   Title = x.Title,
                   Author = x.Author,
                   Publisher = x.Publisher,
                   Weight = x.Weight,
                   IsMandatory = x.IsMandatory ? "Yes" : "No",
                   MinQty = x.MinQty,
                   MaxQty = x.MaxQty,
                   Continuity = x.IsCountinuity ? "Yes" : "No",
                   AvailableStatus = x.IsAvailableStatus ? "Yes" : "No",
                   Supplier = x.Supplier,
                   Location = x.Location,
                   LastModif = x.LastModif,
                   Vendor = x.Vendor,
                   OriginalPrice = x.OriginalPrice,
                   PriceAfterDiscount = x.PriceAfterDiscount,
                   Relagion = x.IsAvailableStatus ? "Yes" : "No",
                   NoSku = x.NoSKU,
                   BookType = x.BookType.GetDescription(),
                   Note = x.Note,
                   Status = x.Status.GetDescription(),
                   UrlImg = x.TextbookAttachments.Select(e => e.Url).FirstOrDefault(),
               }).ToListAsync(CancellationToken);

            var Title = $"TextbookPreparationApproval-{param.IdUser}-{_dateTime.ServerTime.ToString()}";
            var SheetTitle = $"TextbookPreparationApproval";
            var generateExcelByte = GenerateExcel(SheetTitle, GetTextbook);
            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = Title
            };
        }

        public byte[] GenerateExcel(string SheetTitle, List<DownloadTextbookPreparationResult> dataList)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(SheetTitle);

                var DoubleCellStyle = workbook.CreateCellStyle();
                DoubleCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0.##");
                var IntCellStyle = workbook.CreateCellStyle();
                IntCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("#,##0");

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
                var fontBold = workbook.CreateFont();
                fontBold.IsBold = true;
                var boldStyle = workbook.CreateCellStyle();
                boldStyle.SetFont(fontBold);

                var AcademicYear = dataList.Select(e => e.IdAcademicYear).FirstOrDefault();

                IRow rowHeader0 = excelSheet.CreateRow(0);
                var cellDateGenerate = rowHeader0.CreateCell(0);
                cellDateGenerate.SetCellValue("Date Generate");
                var cellDateGenerateValue = rowHeader0.CreateCell(1);
                cellDateGenerateValue.SetCellValue(_dateTime.ServerTime.ToString("dd MMM yyyy"));

                IRow rowHeader1 = excelSheet.CreateRow(1);
                var cellAcademicYear = rowHeader1.CreateCell(0);
                cellAcademicYear.SetCellValue("Academic Year");
                var cellAcademicYearValue = rowHeader1.CreateCell(1);
                cellAcademicYearValue.SetCellValue(AcademicYear);

                List<string> GetHeader = new List<string>()
                {
                    "Level",
                    "Grade",
                    "Subject Group",
                    "Subject",
                    "Streaming",
                    "ISBN",
                    "Title",
                    "Author",
                    "Publisher",
                    "Weight",
                    "Is Mandatory",
                    "Min Qty",
                    "Max Qty",
                    "Continuity",
                    "Available Status",
                    "Supplier",
                    "Location",
                    "Last Modified",
                    "Vendor",
                    "Original Price",
                    "Price After Discount",
                    "Religion",
                    "No SKU",
                    "Book Type",
                    "Note",
                    "Status",
                    "URL Image",
                };

                IRow rowHeader = excelSheet.CreateRow(3);
                foreach (var item in GetHeader)
                {
                    var i = GetHeader.IndexOf(item);
                    var cell = rowHeader.CreateCell(i);
                    cell.SetCellValue(item);
                    cell.CellStyle = boldStyle;
                }

                int rowIndex = 4;
                int startColumn = 0;
                foreach (var itemData in dataList)
                {
                    rowHeader = excelSheet.CreateRow(rowIndex);
                    foreach (var item in GetHeader)
                    {
                        var i = GetHeader.IndexOf(item);
                        var value = "";
                        if (i == 0)
                            value = itemData.Level;
                        if (i == 1)
                            value = itemData.Grade;
                        if (i == 2)
                            value = itemData.SubjectGroup;
                        if (i == 3)
                            value = itemData.Subject;
                        if (i == 4)
                            value = itemData.Streaming;
                        if (i == 5)
                            value = itemData.ISBN;
                        if (i == 6)
                            value = itemData.Title;
                        if (i == 7)
                            value = itemData.Author;
                        if (i == 8)
                            value = itemData.Publisher;
                        if (i == 9)
                            value = itemData.Weight.ToString();
                        if (i == 10)
                            value = itemData.IsMandatory;
                        if (i == 11)
                            value = itemData.MinQty.ToString();
                        if (i == 12)
                            value = itemData.MaxQty.ToString();
                        if (i == 13)
                            value = itemData.Continuity;
                        if (i == 14)
                            value = itemData.AvailableStatus;
                        if (i == 15)
                            value = itemData.Supplier;
                        if (i == 16)
                            value = itemData.Location;
                        if (i == 17)
                            value = itemData.LastModif;
                        if (i == 18)
                            value = itemData.Vendor;
                        if (i == 19)
                            value = itemData.OriginalPrice.ToString();
                        if (i == 20)
                            value = itemData.PriceAfterDiscount.ToString();
                        if (i == 21)
                            value = itemData.Relagion;
                        if (i == 22)
                            value = itemData.NoSku;
                        if (i == 23)
                            value = itemData.BookType;
                        if (i == 24)
                            value = itemData.Note;
                        if (i == 25)
                            value = itemData.Status;
                        if (i == 26)
                            value = itemData.UrlImg;

                        if (i == 11)
                        {
                            var cell = rowHeader.CreateCell(i);
                            cell.SetCellValue(int.Parse(value));
                            cell.CellStyle = IntCellStyle;
                        }
                        else if (i == 12)
                        {
                            var cell = rowHeader.CreateCell(i);
                            cell.SetCellValue(int.Parse(value));
                            cell.CellStyle = IntCellStyle;
                        }
                        else if (i == 19)
                        {
                            var cell = rowHeader.CreateCell(i);
                            cell.SetCellValue(int.Parse(value));
                            cell.CellStyle = IntCellStyle;
                        }
                        else if (i == 20)
                        {
                            var cell = rowHeader.CreateCell(i);
                            cell.SetCellValue(int.Parse(value));
                            cell.CellStyle = IntCellStyle;
                        }
                        else if (i == 9)
                        {
                            var cell = rowHeader.CreateCell(i);
                            cell.SetCellValue(double.Parse(value));
                            cell.CellStyle = DoubleCellStyle;
                        }
                        else
                        {
                            var cell = rowHeader.CreateCell(i);
                            cell.SetCellValue(value);
                        }

                    }
                    rowIndex++;
                    startColumn++;
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
