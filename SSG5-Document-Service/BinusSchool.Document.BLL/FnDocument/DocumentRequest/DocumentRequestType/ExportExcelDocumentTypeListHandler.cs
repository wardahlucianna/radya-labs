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
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.DocumentRequestType;
using BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Document.FnDocument.DocumentRequest.DocumentRequestType
{
    public class ExportExcelDocumentTypeListHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IDocumentRequestType _documentRequestTypeApi;
        private readonly IMachineDateTime _dateTime;

        public ExportExcelDocumentTypeListHandler(
            IDocumentDbContext dbContext,
            IDocumentRequestType documentRequestTypeApi,
            IMachineDateTime dateTime
            )
        {
            _dbContext = dbContext;
            _documentRequestTypeApi = documentRequestTypeApi;
            _dateTime = dateTime;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelDocumentTypeListRequest, ExportExcelDocumentTypeListValidator>();

            // param desc
            var getAY = await _dbContext.Entity<MsAcademicYear>()
                            .Where(x => x.Code == param.AcademicYearCode)
                            .FirstOrDefaultAsync(CancellationToken);

            var paramDesc = await _dbContext.Entity<MsGrade>()
                                .Include(x => x.Level)
                                    .ThenInclude(x => x.AcademicYear)
                                    .ThenInclude(x => x.School)
                                .Where(x => x.Level.AcademicYear.IdSchool == param.IdSchool &&
                                            (string.IsNullOrEmpty(param.LevelCode) ? true : x.Level.Code == param.LevelCode) &&
                                            (string.IsNullOrEmpty(param.GradeCode) ? true : x.Code == param.GradeCode)
                                            )
                                .Select(x => new ExportExcelDocumentTypeListResult_ParamDesc
                                {
                                    SchoolDesc = x.Level.AcademicYear.School.Description,
                                    AcademicYear = string.IsNullOrEmpty(param.AcademicYearCode) || getAY == null ? "All" : getAY.Description,
                                    LevelDesc = string.IsNullOrEmpty(param.LevelCode) ? "All" : x.Level.Description,
                                    GradeDesc = string.IsNullOrEmpty(param.GradeCode) ? "All" : x.Description,
                                    ActiveStatus = param.ActiveStatus == null ? "All" : param.ActiveStatus == true ? "Active" : "Inactive",
                                    VisibleToParent = param.VisibleToParent == null ? "All" : param.VisibleToParent == true ? "Yes" : "No",
                                    PaidDocument = param.PaidDocument == null ? "All" : param.PaidDocument == true ? "Yes" : "No",
                                })
                                .FirstOrDefaultAsync(CancellationToken);

            var title = "DocumentRequestTypeList";

            // result
            var resultList = new List<GetDocumentRequestTypeListResult>();

            var masterDocumentRequestType = await _documentRequestTypeApi.GetDocumentRequestTypeList(new GetDocumentRequestTypeListRequest
            {
                GetAll = true,
                IdSchool = param.IdSchool,
                AcademicYearCode = param.AcademicYearCode,
                LevelCode = param.LevelCode,
                GradeCode = param.GradeCode,
                ActiveStatus = param.ActiveStatus,
                VisibleToParent = param.VisibleToParent,
                PaidDocument = param.PaidDocument,
                OrderBy = "DocumentName",
                OrderType = Common.Model.Enums.OrderType.Asc
            });

            if (masterDocumentRequestType.Payload?.Count() > 0)
            {
                var masterDocumentRequestTypeList = masterDocumentRequestType.Payload.ToList();

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, masterDocumentRequestTypeList, title);
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

        public byte[] GenerateExcel(ExportExcelDocumentTypeListResult_ParamDesc paramDesc, List<GetDocumentRequestTypeListResult> dataList, string sheetTitle)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);
                excelSheet.SetColumnWidth(1, 30 * 256);
                excelSheet.SetColumnWidth(2, 30 * 256);
                excelSheet.SetColumnWidth(3, 30 * 256);
                excelSheet.SetColumnWidth(4, 30 * 256);
                excelSheet.SetColumnWidth(5, 30 * 256);
                excelSheet.SetColumnWidth(6, 30 * 256);
                excelSheet.SetColumnWidth(7, 30 * 256);
                excelSheet.SetColumnWidth(8, 30 * 256);
                excelSheet.SetColumnWidth(9, 30 * 256);
                excelSheet.SetColumnWidth(10, 30 * 256);
                excelSheet.SetColumnWidth(11, 30 * 256);
                excelSheet.SetColumnWidth(12, 30 * 256);
                excelSheet.SetColumnWidth(13, 30 * 256);
                excelSheet.SetColumnWidth(14, 30 * 256);
                excelSheet.SetColumnWidth(15, 30 * 256);
                excelSheet.SetColumnWidth(16, 30 * 256);
                excelSheet.SetColumnWidth(17, 30 * 256);
                excelSheet.SetColumnWidth(18, 30 * 256);
                excelSheet.SetColumnWidth(19, 30 * 256);
                excelSheet.SetColumnWidth(20, 30 * 256);
                excelSheet.SetColumnWidth(21, 30 * 256);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();
                style.WrapText = true;
                style.VerticalAlignment = VerticalAlignment.Center;

                ICellStyle styleTable = workbook.CreateCellStyle();
                styleTable.BorderBottom = BorderStyle.Thin;
                styleTable.BorderLeft = BorderStyle.Thin;
                styleTable.BorderRight = BorderStyle.Thin;
                styleTable.BorderTop = BorderStyle.Thin;
                styleTable.VerticalAlignment = VerticalAlignment.Center;
                styleTable.WrapText = true;

                ICellStyle styleHeader = workbook.CreateCellStyle();
                styleHeader.WrapText = true;
                styleHeader.VerticalAlignment = VerticalAlignment.Center;

                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;
                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.WrapText = true;


                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                //font.IsItalic = true;
                font.FontName = "Arial";
                font.FontHeightInPoints = 12;
                style.SetFont(font);

                styleTable.SetFont(font);

                IFont fontHeader = workbook.CreateFont();
                fontHeader.FontName = "Arial";
                fontHeader.FontHeightInPoints = 12;
                fontHeader.IsBold = true;
                styleHeader.SetFont(fontHeader);

                styleHeaderTable.SetFont(fontHeader);

                int rowIndex = 0;
                int startColumn = 1;

                // Excel generate date
                IRow totalRowExcelGenerateDate = excelSheet.CreateRow(rowIndex);
                var cellExcelGenerateDate = totalRowExcelGenerateDate.CreateCell(1);
                cellExcelGenerateDate.SetCellValue("Excel Generate Date:\n" + _dateTime.ServerTime.ToString("dd MMM yyyy, HH:mm:ss") + " WIB");
                cellExcelGenerateDate.CellStyle = style;
                rowIndex++;

                //Title 
                rowIndex++;
                IRow titleRow = excelSheet.CreateRow(rowIndex);
                var cellTitleRow = titleRow.CreateCell(1);
                cellTitleRow.SetCellValue("Master Document Request Type");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[6] { 
                    "Academic Year", 
                    "Level",
                    "Grade",
                    "Active Status",
                    "Visible to Parent",
                    "Paid Document",
                };

                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYear);
                    if (field == "Level")
                        cellValueParamRow.SetCellValue(paramDesc.LevelDesc);
                    if (field == "Grade")
                        cellValueParamRow.SetCellValue(paramDesc.GradeDesc);
                    if (field == "Active Status")
                        cellValueParamRow.SetCellValue(paramDesc.ActiveStatus);
                    if (field == "Visible to Parent")
                        cellValueParamRow.SetCellValue(paramDesc.VisibleToParent);
                    if (field == "Paid Document")
                        cellValueParamRow.SetCellValue(paramDesc.PaidDocument);

                    rowIndex++;
                }

                rowIndex += 2;

                // summary content
                var headerList = new string[21] { 
                    "Document Name", 
                    "Document Description", 
                    "Active Status",
                    "Price per Copy (Rp)", 
                    "Invoice Payment Expired Hours", 
                    "Default Process Days", 
                    "HardCopy Available",
                    "SoftCopy Available", 
                    "Available From Academic Year", 
                    "Available Until Academic Year", 
                    "Is Academic Document", 
                    "Have Term Options", 
                    "Visible to Parent", 
                    "Need Approval", 
                    "Using No of Pages",
                    "Default No of Pages",
                    "Fillable No of Copy",
                    "Maximum No of Copies Allowed",
                    "Grade Mapping",
                    "PIC Individual",
                    "PIC Group"
                };

                int startColumnHeader = startColumn;
                int startColumnContent = startColumn;

                IRow sumRow2 = excelSheet.CreateRow(rowIndex);
                foreach (string header in headerList)
                {
                    var cellLevelHeader = sumRow2.CreateCell(startColumnHeader);
                    cellLevelHeader.SetCellValue(header);
                    cellLevelHeader.CellStyle = styleHeaderTable;

                    startColumnHeader++;
                }

                rowIndex++;

                if (dataList != null)
                {
                    foreach (var itemData in dataList)
                    {
                        int firstSubItemRow = rowIndex;
                        int rowSubItem = firstSubItemRow;
                        int lastSubItemRow = rowSubItem - 1;

                        IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                        // Merge Row 
                        var mergedCellSource = -1;
                        //foreach (var subItem in itemData.GradeList)
                        //{
                        //    totalRow2 = excelSheet.CreateRow(rowSubItem);

                        //    var cellGradeList = totalRow2.CreateCell(mergedCellSource);
                        //    cellGradeList.SetCellValue(subItem.Description);
                        //    cellGradeList.CellStyle = styleTable;

                        //    // create border in empty cell
                        //    var cellEmpty = totalRow2.CreateCell(1);
                        //    cellEmpty.CellStyle = styleTable;

                        //    for (int i = 2; i <= 15; i++)
                        //    {
                        //        if (i != mergedCellSource)
                        //        {
                        //            cellEmpty = totalRow2.CreateCell(i);
                        //            cellEmpty.CellStyle = styleTable;
                        //        }
                        //    }

                        //    rowSubItem++;
                        //}

                        // Grade Mapping List
                        string gradeMappingText = "'";
                        gradeMappingText = !itemData.IsUsingGradeMapping ? "" : string.Join("\n", itemData.GradeMappingList.Select(x => "- " + x.GradeDescription));

                        // PIC Individual List
                        string picIndividualText = "'";
                        picIndividualText = itemData.PICIndividualList.Count == 1 ? itemData.PICIndividualList.Select(x => x.Binusian.Name + " (" + x.Binusian.Id + ")").FirstOrDefault().ToString() : string.Join("\n", itemData.PICIndividualList.Select(x => "- " + x.Binusian.Name + " (" + x.Binusian.Id + ")"));

                        // PIC Group List
                        string picGroupText = "'";
                        picGroupText = itemData.PICGroupList.Count == 1 ? itemData.PICGroupList.Select(x => x.RoleGroup.Description).FirstOrDefault().ToString() : string.Join("\n", itemData.PICGroupList.Select(x => "- " + x.RoleGroup.Description));

                        lastSubItemRow++;

                        totalRow2 = excelSheet.GetRow(firstSubItemRow);

                        // Single Data
                        var contentDataList = new string[21]
                        {
                            itemData.DocumentName,
                            itemData.DocumentDescription,
                            itemData.ActiveStatus ? "Active" : "Inactive",
                            string.Format("{0:N}", itemData.Price),
                            //string.Format(CultureInfo.CreateSpecificCulture("id-id"), "{0:C}", itemData.Price),
                            itemData.InvoiceDueHoursPayment == null ? "-" : itemData.InvoiceDueHoursPayment.ToString() + " hour(s)",
                            itemData.DefaultNoOfProcessDay.ToString() + " day(s)",
                            itemData.HardCopyAvailable ? "Yes" : "No",
                            itemData.SoftCopyAvailable ? "Yes" : "No",
                            string.IsNullOrEmpty(itemData.AcademicYearStart?.Description) ? "Unspecified" : itemData.AcademicYearStart.Description,
                            string.IsNullOrEmpty(itemData.AcademicYearEnd?.Description) ? "Unspecified" : itemData.AcademicYearEnd.Description,
                            itemData.IsAcademicDocument ? "Yes" : "No",
                            itemData.DocumentHasTerm ? "Yes" : "No",
                            itemData.VisibleToParent ? "Yes" : "No",
                            itemData.ParentNeedApproval ? "Yes" : "No",
                            itemData.IsUsingNoOfPages ? "Yes" : "No",
                            itemData.DefaultNoOfPages == null ? "-" : itemData.DefaultNoOfPages.ToString(),
                            itemData.IsUsingNoOfCopy ? "Yes" : "No",
                            itemData.MaxNoOfCopy == null ? "-" : itemData.MaxNoOfCopy.ToString(),
                            itemData.IsUsingGradeMapping ? gradeMappingText : "-",
                            itemData.PICIndividualList.Any() ? picIndividualText : "-",
                            itemData.PICGroupList.Any() ? picGroupText : "-"
                        };

                        var cellData = totalRow2.CreateCell(1);
                        cellData.SetCellValue(contentDataList[0]);
                        if (firstSubItemRow != lastSubItemRow)
                        {
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 1, 1));
                        }
                        cellData.CellStyle = styleTable;

                        for (int i = 2; i <= contentDataList.Count(); i++)
                        {
                            if (i != mergedCellSource)
                            {
                                cellData = totalRow2.CreateCell(i);
                                cellData.SetCellValue(contentDataList[i - 1]);
                                if (firstSubItemRow != lastSubItemRow)
                                {
                                    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, i, i));
                                }
                                cellData.CellStyle = styleTable;
                            }
                        }

                        rowIndex = lastSubItemRow;
                        rowIndex++;
                    }
                }
                else
                {
                    IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                    for (int i = 1; i <= headerList.Count(); i++)
                    {
                        var cellBlank = totalRow2.CreateCell(i);
                        cellBlank.SetCellValue("");
                        cellBlank.CellStyle = styleTable;
                    }

                    rowIndex++;
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
