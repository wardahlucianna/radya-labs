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
using BinusSchool.Data.Model.Document.FnDocument.DocumentRequest.MasterDocumentRequest;
using BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Document.FnDocument.DocumentRequest.MasterDocumentRequest
{
    public class ExportExcelGetDocumentRequestListHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly GetDocumentRequestListWithDetailHandler _getDocumentRequestListWithDetailHandler;
        private readonly IMachineDateTime _dateTime;

        public ExportExcelGetDocumentRequestListHandler(
            IDocumentDbContext dbContext,
            GetDocumentRequestListWithDetailHandler getDocumentRequestListWithDetailHandler,
            IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _getDocumentRequestListWithDetailHandler = getDocumentRequestListWithDetailHandler;
            _dateTime = dateTime;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelGetDocumentRequestListRequest, ExportExcelGetDocumentRequestListValidator>();

            // param desc
            var getSchool = await _dbContext.Entity<MsSchool>()
                                .Where(x => x.Id == param.IdSchool)
                                .FirstOrDefaultAsync(CancellationToken);

            var getDocumentRequestTypeParam = await _dbContext.Entity<MsDocumentReqType>()
                                                .Where(x => x.Id == param.IdDocumentReqType)
                                                .Select(x => x.Name)
                                                .FirstOrDefaultAsync(CancellationToken);

            var paramDesc = new ExportExcelGetDocumentRequestListResult_ParamDesc
            {
                SchoolName = getSchool.Description,
                RequestYear = param.RequestYear == 1 ? "Within Last One Year" : param.RequestYear.ToString(),
                DocumentTypeName = string.IsNullOrEmpty(getDocumentRequestTypeParam) ? "All" : getDocumentRequestTypeParam,
                ApprovalStatus = param.ApprovalStatus == null ? "All" : param.ApprovalStatus.Value.GetDescription(),
                PaymentStatus = param.PaymentStatus == null ? "All" : param.PaymentStatus.Value.GetDescription(),
                DocumentReqStatusWorkflow = param.IdDocumentReqStatusWorkflow == null ? "All" : param.IdDocumentReqStatusWorkflow.Value.GetDescription(),
                SearchKeyword = string.IsNullOrEmpty(param.SearchQuery) ? "-" : param.SearchQuery
            };

            var title = "MasterDocumentRequest";

            var documentRequestListApi = await _getDocumentRequestListWithDetailHandler.GetDocumentRequestListWithDetail(new GetDocumentRequestListWithDetailRequest
            {
                IdSchool = param.IdSchool,
                RequestYear = param.RequestYear,
                IdDocumentReqType = param.IdDocumentReqType,
                ApprovalStatus = param.ApprovalStatus,
                PaymentStatus = param.PaymentStatus,
                IdDocumentReqStatusWorkflow = param.IdDocumentReqStatusWorkflow,
                SearchQuery = param.SearchQuery
            });

            if (documentRequestListApi != null && documentRequestListApi.Count() > 0)
            {
                var result = documentRequestListApi;

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, result, title);
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

        public byte[] GenerateExcel(ExportExcelGetDocumentRequestListResult_ParamDesc paramDesc, List<GetDocumentRequestListWithDetailResult> dataList, string sheetTitle)
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
                excelSheet.SetColumnWidth(22, 30 * 256);
                excelSheet.SetColumnWidth(23, 30 * 256);
                excelSheet.SetColumnWidth(24, 30 * 256);

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
                cellTitleRow.SetCellValue("Master Document Request");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[7] {
                    "School",
                    "Request Year",
                    "Document Type Name",
                    "Approval Status",
                    "Payment Status",
                    "Request Status",
                    "Search Keyword",
                };

                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "School")
                        cellValueParamRow.SetCellValue(paramDesc.SchoolName);
                    if (field == "Request Year")
                        cellValueParamRow.SetCellValue(paramDesc.RequestYear);
                    if (field == "Document Type Name")
                        cellValueParamRow.SetCellValue(paramDesc.DocumentTypeName);
                    if (field == "Approval Status")
                        cellValueParamRow.SetCellValue(paramDesc.ApprovalStatus);
                    if (field == "Payment Status")
                        cellValueParamRow.SetCellValue(paramDesc.PaymentStatus);
                    if (field == "Request Status")
                        cellValueParamRow.SetCellValue(paramDesc.DocumentReqStatusWorkflow);
                    if (field == "Search Keyword")
                        cellValueParamRow.SetCellValue(paramDesc.SearchKeyword);

                    rowIndex++;
                }

                rowIndex += 2;

                // summary content
                var headerList = new string[24] {
                    "Request Number",
                    "Request Date",
                    "Requested By",
                    "Created By",
                    "Student ID",
                    "Student Name",
                    "Student Status When Request Was Created",
                    "Homeroom When Request Was Created",
                    "Estimated Finish Date",
                    "Total Amount (Rp)",
                    "Request Status",
                    "Document Name",
                    "Document Receiver",
                    "Is Academic Document",
                    "Homeroom / Academic Year",
                    "Term",
                    "Hardcopy",
                    "Softcopy",
                    "Number of Pages",
                    "Number of Copies",
                    "Price (Rp)",
                    "PIC",
                    "Additional Fields Question",
                    "Additional Fields Answer",
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
                        IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                        int firstSubItemRow = rowIndex;
                        int rowSubItem = firstSubItemRow;
                        int lastSubItemRow = firstSubItemRow;

                        // Merge Row 
                        int[] mergedCellSubItem = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

                        foreach (var subItem in itemData.DocumentList)
                        {
                            int firstSubSubItemRow = rowSubItem;
                            int rowSubSubItem = firstSubSubItemRow;
                            int lastSubSubItemRow = firstSubSubItemRow;

                            // Data merge Sub Sub Row 
                            int[] mergedCellSubSubItem = { 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22 };
                            var tempCountSubSubItem = 0;

                            totalRow2 = excelSheet.CreateRow(firstSubSubItemRow);

                            if(subItem.AdditionalFieldList == null || !subItem.AdditionalFieldList.Any())
                            {
                                // Additional Fields Name
                                var cellSubSub1 = totalRow2.CreateCell(23);
                                cellSubSub1.SetCellValue("-");
                                cellSubSub1.CellStyle = styleTable;


                                // Additional Fields Answer
                                var cellSubSub2 = totalRow2.CreateCell(24);
                                cellSubSub2.SetCellValue("-");
                                cellSubSub2.CellStyle = styleTable;
                            }
                            else
                            {
                                foreach (var subSubItem in subItem.AdditionalFieldList)
                                {
                                    totalRow2 = excelSheet.CreateRow(rowSubSubItem);

                                    // Additional Fields Question
                                    var cellSubSub1 = totalRow2.CreateCell(23);
                                    cellSubSub1.SetCellValue(subSubItem.QuestionDescription);
                                    cellSubSub1.CellStyle = styleTable;


                                    // Additional Fields Answer
                                    var cellSubSub2 = totalRow2.CreateCell(24);
                                    cellSubSub2.SetCellValue(subSubItem.AnswerTextList.Count == 1 ? subSubItem.AnswerTextList.FirstOrDefault().ToString() : string.Join("\n", subSubItem.AnswerTextList.Select(x => "- " + x)));
                                    cellSubSub2.CellStyle = styleTable;

                                    // create border in empty cell
                                    for (int i = 0; i < mergedCellSubSubItem.Length; i++)
                                    {
                                        var cellEmpty = totalRow2.CreateCell(mergedCellSubSubItem[i]);
                                        cellEmpty.CellStyle = styleTable;
                                    }

                                    if (tempCountSubSubItem != 0)
                                    {
                                        rowSubItem++;
                                    }
                                    rowSubSubItem++;
                                    tempCountSubSubItem++;
                                }
                            }
                            
                            lastSubSubItemRow = rowSubItem;

                            totalRow2 = excelSheet.GetRow(firstSubSubItemRow);

                            // Document Name
                            var cellSub1 = totalRow2.CreateCell(12);
                            cellSub1.SetCellValue(subItem.DocumentName);
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 12, 12));
                            cellSub1.CellStyle = styleTable;

                            // Document Receiver
                            var cellSub2 = totalRow2.CreateCell(13);
                            cellSub2.SetCellValue(subItem.DocumentIsReady?.ReceivedDateByStaff == null ? "-" : (subItem.DocumentIsReady.BinusianReceiver.Name.Trim() + " (" + subItem.DocumentIsReady.BinusianReceiver.Id + ")"));
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 13, 13));
                            cellSub2.CellStyle = styleTable;

                            // Is Academic Document
                            var cellSub3 = totalRow2.CreateCell(14);
                            cellSub3.SetCellValue(subItem.IsAcademicDocument ? "Yes" : "No");
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 14, 14));
                            cellSub3.CellStyle = styleTable;

                            // Academic Year / Homeroom
                            var cellSub4 = totalRow2.CreateCell(15);
                            cellSub4.SetCellValue(subItem.AcademicYearDocument == null ? "-" : subItem.HomeroomName + " (" + subItem.AcademicYearDocument.Description + ")");
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 15, 15));
                            cellSub4.CellStyle = styleTable;

                            // Term
                            var cellSub6 = totalRow2.CreateCell(16);
                            cellSub6.SetCellValue(subItem.PeriodDocument == null ? "-" : subItem.PeriodDocument.Description);
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 16, 16));
                            cellSub6.CellStyle = styleTable;

                            // Hardcopy
                            var cellSub7 = totalRow2.CreateCell(17);
                            cellSub7.SetCellValue(subItem.NeedHardCopy ? "Yes" : "No");
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 17, 17));
                            cellSub7.CellStyle = styleTable;

                            // Softcopy
                            var cellSub8 = totalRow2.CreateCell(18);
                            cellSub8.SetCellValue(subItem.NeedSoftCopy ? "Yes" : "No");
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 18, 18));
                            cellSub8.CellStyle = styleTable;

                            // Number of Pages
                            var cellSub9 = totalRow2.CreateCell(19);
                            cellSub9.SetCellValue(subItem.NoOfPages == null ? "-" : subItem.NoOfPages.Value.ToString());
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 19, 19));
                            cellSub9.CellStyle = styleTable;

                            // Number of Copies
                            var cellSub10 = totalRow2.CreateCell(20);
                            cellSub10.SetCellValue(subItem.NoOfCopy.ToString());
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 20, 20));
                            cellSub10.CellStyle = styleTable;

                            // Price
                            var cellSub11 = totalRow2.CreateCell(21);
                            cellSub11.SetCellValue(string.Format("{0:N}", subItem.PriceInvoice));
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 21, 21));
                            cellSub11.CellStyle = styleTable;

                            // PIC
                            var cellSub12 = totalRow2.CreateCell(22);
                            cellSub12.SetCellValue(subItem.PICList.Count == 1 ? subItem.PICList.Select(x => x.Name + " (" + x.Id + ")").FirstOrDefault().ToString() : string.Join("\n", subItem.PICList.Select(x => "- " + x.Name + " (" + x.Id + ")")));
                            if (firstSubSubItemRow != lastSubSubItemRow)
                                excelSheet.AddMergedRegion(new CellRangeAddress(firstSubSubItemRow, lastSubSubItemRow, 22, 22));
                            cellSub12.CellStyle = styleTable;


                            // create border in empty cell
                            for(int j = firstSubSubItemRow; j <= lastSubSubItemRow; j++)
                            {
                                IRow tempRow = excelSheet.GetRow(j);
                                for (int i = 0; i < mergedCellSubItem.Length; i++)
                                {
                                    var cellEmpty = tempRow.CreateCell(mergedCellSubItem[i]);
                                    cellEmpty.CellStyle = styleTable;
                                }
                            }

                            rowSubSubItem++;
                            rowSubItem++;
                        }

                        lastSubItemRow = rowSubItem - 1;

                        totalRow2 = excelSheet.GetRow(firstSubItemRow);

                        // Request Nummber
                        var cell1 = totalRow2.CreateCell(1);
                        cell1.SetCellValue(itemData.RequestNumber);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 1, 1));
                        cell1.CellStyle = styleTable;

                        // Request Date
                        var cell2 = totalRow2.CreateCell(2);
                        cell2.SetCellValue(itemData.RequestDate.ToString("dd MMM yyyy, hh:mm tt"));
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 2, 2));
                        cell2.CellStyle = styleTable;

                        // Requested By
                        var cell3 = totalRow2.CreateCell(3);
                        cell3.SetCellValue(itemData.RequestedBy.Name);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 3, 3));
                        cell3.CellStyle = styleTable;

                        // Created By
                        var cell4 = totalRow2.CreateCell(4);
                        cell4.SetCellValue(itemData.CreatedBy);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 4, 4));
                        cell4.CellStyle = styleTable;

                        // Student ID
                        var cell5 = totalRow2.CreateCell(5);
                        cell5.SetCellValue(itemData.Student.Id);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 5, 5));
                        cell5.CellStyle = styleTable;

                        // Student Name
                        var cell6 = totalRow2.CreateCell(6);
                        cell6.SetCellValue(itemData.Student.Name);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 6, 6));
                        cell6.CellStyle = styleTable;

                        // Student Status
                        var cell7 = totalRow2.CreateCell(7);
                        cell7.SetCellValue(itemData.StudentStatusWhenRequestWasCreated == null ? "-" : itemData.StudentStatusWhenRequestWasCreated.Description + (itemData.StudentStatusWhenRequestWasCreated.StartDate == null ? "" : " (" + itemData.StudentStatusWhenRequestWasCreated.StartDate.Value.ToString("dd MMMM yyyy") + ")"));
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 7, 7));
                        cell7.CellStyle = styleTable;

                        // Homeroom
                        var cell8 = totalRow2.CreateCell(8);
                        cell8.SetCellValue(itemData.HomeroomWhenRequestWasMade == null ? "-" : itemData.HomeroomWhenRequestWasMade.Description);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 8, 8));
                        cell8.CellStyle = styleTable;

                        // Estimated Finish Date
                        var cell9 = totalRow2.CreateCell(9);
                        cell9.SetCellValue(itemData.EstimationFinishDate == null ? "-" : itemData.EstimationFinishDate.Value.ToString("dd MMM yyyy"));
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 9, 9));
                        cell9.CellStyle = styleTable;

                        // Total Amount
                        var cell10 = totalRow2.CreateCell(10);
                        cell10.SetCellValue(string.Format("{0:N}", itemData.TotalAmountReal));
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 10, 10));
                        cell10.CellStyle = styleTable;

                        // Request Status
                        var cell11 = totalRow2.CreateCell(11);
                        cell11.SetCellValue(itemData.LatestDocumentReqStatusWorkflow.Description);
                        if (firstSubItemRow != lastSubItemRow)
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 11, 11));
                        cell11.CellStyle = styleTable;

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
