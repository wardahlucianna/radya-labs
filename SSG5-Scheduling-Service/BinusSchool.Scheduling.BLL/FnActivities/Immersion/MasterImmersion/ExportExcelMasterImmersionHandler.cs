using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Scheduling.FnActivities;
using BinusSchool.Data.Model.Scheduling.FnActivities.Immersion.MasterImmersion;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnActivities.Immersion.MasterImmersion
{
    public class ExportExcelMasterImmersionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IImmersion _immersionApi;

        public ExportExcelMasterImmersionHandler(ISchedulingDbContext dbContext, IImmersion immersionApi)
        {
            _dbContext = dbContext;
            _immersionApi = immersionApi;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelMasterImmersionRequest, ExportExcelMasterImmersionValidator>();

            // param desc
            var schoolData = _dbContext.Entity<MsAcademicYear>()
                                .Include(x => x.School)
                                .Where(x => x.Id == param.IdAcademicYear)
                                .Select(x => new
                                {
                                    SchoolDesc = x.School.Description,
                                    AcademicYearDesc = x.Description
                                })
                                .FirstOrDefault();


            ExportExcelMasterImmersionResult_ParamDesc paramDesc = new ExportExcelMasterImmersionResult_ParamDesc();
            paramDesc.School = schoolData.SchoolDesc;
            paramDesc.AcademicYear = schoolData.AcademicYearDesc;
            paramDesc.Semester = param.Semester.ToString();

            var title = "MasterImmersionSummary";

            // result
            var resultList = new List<GetMasterImmersionResult>();

            var masterImmersionPayload = await _immersionApi.GetMasterImmersion(new GetMasterImmersionRequest
            {
                GetAll = true,
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                IdImmersionPeriod = string.IsNullOrEmpty(param.IdImmersionPeriod) ? null : param.IdImmersionPeriod
            });

            if (masterImmersionPayload.Payload?.Count() > 0)
            {
                var masterImmersionPayloadList = masterImmersionPayload.Payload.ToList();

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, masterImmersionPayloadList, title);
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

        public byte[] GenerateExcel(ExportExcelMasterImmersionResult_ParamDesc paramDesc, List<GetMasterImmersionResult> dataList, string sheetTitle)
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
                excelSheet.SetColumnWidth(4, 20 * 256);
                excelSheet.SetColumnWidth(5, 20 * 256);
                excelSheet.SetColumnWidth(6, 30 * 256);
                excelSheet.SetColumnWidth(7, 30 * 256);
                excelSheet.SetColumnWidth(8, 30 * 256);
                excelSheet.SetColumnWidth(9, 20 * 256);
                excelSheet.SetColumnWidth(10, 20 * 256);
                excelSheet.SetColumnWidth(11, 20 * 256);
                excelSheet.SetColumnWidth(12, 30 * 256);
                excelSheet.SetColumnWidth(13, 30 * 256);
                excelSheet.SetColumnWidth(14, 30 * 256);

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

                //Title 
                rowIndex++;
                IRow titleRow = excelSheet.CreateRow(rowIndex);
                var cellTitleRow = titleRow.CreateCell(1);
                cellTitleRow.SetCellValue("Master Immersion Summary");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[3] { "School", "Academic Year", "Semester" };

                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "School")
                        cellValueParamRow.SetCellValue(paramDesc.School);
                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYear);
                    if (field == "Semester")
                        cellValueParamRow.SetCellValue(paramDesc.Semester);

                    rowIndex++;
                }

                rowIndex += 2;

                // summary content
                var headerList = new string[14] { "Destination", "Description", "Grade", "Start Date", "End Date", "PIC Name", "PIC Email", "PIC Phone", "Min. Participant", "Max. Participant", "Currency", "Payment Method", "Registration Fee", "Total Cost" };

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
                        int lastSubItemRow;
                        int rowSubItem = firstSubItemRow;

                        IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                        // Merge Row 
                        var mergedCellSource = 3;
                        foreach (var subItem in itemData.GradeList)
                        {
                            totalRow2 = excelSheet.CreateRow(rowSubItem);

                            var cellGradeList = totalRow2.CreateCell(mergedCellSource);
                            cellGradeList.SetCellValue(subItem.Description);
                            cellGradeList.CellStyle = styleTable;

                            // create border in empty cell
                            var cellEmpty = totalRow2.CreateCell(1);
                            cellEmpty.CellStyle = styleTable;

                            for (int i = 2; i <= 15; i++)
                            {
                                if (i != mergedCellSource)
                                {
                                    cellEmpty = totalRow2.CreateCell(i);
                                    cellEmpty.CellStyle = styleTable;
                                }
                            }

                            rowSubItem++;
                        }

                        lastSubItemRow = rowSubItem - 1;

                        totalRow2 = excelSheet.GetRow(firstSubItemRow);

                        // Single Data
                        var contentDataList = new string[14]
                        {
                            itemData.Destination,
                            itemData.Description,
                            "GradeList",
                            itemData.StartDate.ToString("dd-MM-yyyy"),
                            itemData.EndDate.ToString("dd-MM-yyyy"),
                            itemData.PIC.Name,
                            itemData.PIC.Email,
                            itemData.PIC.PhoneNumber,
                            itemData.MinParticipant.ToString(),
                            itemData.MaxParticipant.ToString(),
                            itemData.Currency.Name,
                            itemData.ImmersionPaymentMethod.Description,
                            string.Format("{0} {1:N}", (itemData.CurrencySymbol.Contains("?") ? "" : itemData.CurrencySymbol), itemData.RegistrationFee),
                            string.Format("{0} {1:N}", (itemData.CurrencySymbol.Contains("?") ? "" : itemData.CurrencySymbol), itemData.TotalCost)
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
                            if(i != mergedCellSource)
                            {
                                cellData = totalRow2.CreateCell(i);
                                cellData.SetCellValue(contentDataList[i-1]);
                                if (firstSubItemRow != lastSubItemRow)
                                {
                                    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, i, i));
                                }
                                cellData.CellStyle = styleTable;
                            }
                        }

                        //var cell1 = totalRow2.CreateCell(1);
                        //cell1.SetCellValue(itemData.AcademicYear.Description);
                        //if (firstSubItemRow != lastSubItemRow)
                        //{
                        //    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 1, 1));
                        //}
                        //cell1.CellStyle = styleTable;

                        //var cell2 = totalRow2.CreateCell(2);
                        //cell2.SetCellValue(itemData.Description);
                        //if (firstSubItemRow != lastSubItemRow)
                        //{
                        //    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 2, 2));
                        //}
                        //cell2.CellStyle = styleTable;

                        //var cell4 = totalRow2.CreateCell(4);
                        //cell4.SetCellValue(itemData.Destination);
                        //if (firstSubItemRow != lastSubItemRow)
                        //{
                        //    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 4, 4));
                        //}
                        //cell4.CellStyle = styleTable;

                        //var cell5 = totalRow2.CreateCell(5);
                        //cell5.SetCellValue(itemData.StartDate.ToString());
                        //if (firstSubItemRow != lastSubItemRow)
                        //{
                        //    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 5, 5));
                        //}
                        //cell5.CellStyle = styleTable;

                        //var cell6 = totalRow2.CreateCell(6);
                        //cell6.SetCellValue(itemData.EndDate.ToString());
                        //if (firstSubItemRow != lastSubItemRow)
                        //{
                        //    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 6, 6));
                        //}
                        //cell6.CellStyle = styleTable;

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
