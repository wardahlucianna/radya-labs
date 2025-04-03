using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Document.FnDocument;
using BinusSchool.Data.Model.Document.FnDocument.BLPGroup;
using BinusSchool.Document.FnDocument.BLPGroup.Validator;
using BinusSchool.Persistence.DocumentDb.Abstractions;
using BinusSchool.Persistence.DocumentDb.Entities.Scheduling;
using BinusSchool.Persistence.DocumentDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Document.FnDocument.BLPGroup
{
    public class ExportExcelBLPGroupStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IDocumentDbContext _dbContext;
        private readonly IBLPGroup _blpGroupApi;

        public ExportExcelBLPGroupStudentHandler(
            IDocumentDbContext dbContext,
            IBLPGroup blpGroupApi)
        {
            _dbContext = dbContext;
            _blpGroupApi = blpGroupApi;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelBLPGroupStudentRequest, ExportExcelBLPGroupStudentValidator>();
            var predicate = PredicateBuilder.True<MsHomeroom>();

            if (param.IdAcademicYear != null)
                predicate = predicate.And(x => x.IdAcademicYear == param.IdAcademicYear);
            if (param.Semester != 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (param.IdLevel != null)
                predicate = predicate.And(x => x.Grade.Level.Id == param.IdLevel);
            if (param.IdGrade != null)
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);

            // param desc
            var schoolData = _dbContext.Entity<MsHomeroom>()
                                .Include(x => x.Grade)
                                .ThenInclude(x => x.Level)
                                .ThenInclude(x => x.AcademicYear)
                                .ThenInclude(x => x.School)
                                .Where(predicate)
                                //.Where(x => x.Id == param.IdHomeroom)
                                .Select(x => new
                                {
                                    IdSchool = x.Grade.Level.AcademicYear.School.Id,
                                    SchoolDesc = x.Grade.Level.AcademicYear.School.Description,
                                    AcademicYearDesc = x.Grade.Level.AcademicYear.Description,
                                    LevelDesc = param.IdLevel == null ? "ALL" : x.Grade.Level.Description,
                                    GradeDesc = param.IdGrade == null ? "ALL" : x.Grade.Description,
                                    Semester = x.Semester.ToString()
                                })
                                .FirstOrDefault();

            var paramDesc = new ExportExcelBLPGroupStudentResult_ParamDesc
            {
                IdSchool = schoolData.IdSchool,
                SchoolDesc = schoolData.SchoolDesc,
                AcademicYearDesc = schoolData.AcademicYearDesc,
                LevelDesc = schoolData.LevelDesc,
                GradeDesc = schoolData.GradeDesc,
                Semester = schoolData.Semester,
                BLPFinalStatusDesc = param.BLPFinalStatus == null ? null : param.BLPFinalStatus.Value.GetDescription()
            };

            var title = "BLPGroupStudentSummary";

            // result
            var resultList = new List<GetBLPGroupStudentResult>();

            var BLPGroupStudentPayload = await _blpGroupApi.GetBLPGroupStudent(new GetBLPGroupStudentRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                Semester = param.Semester,
                BLPFinalStatus = param.BLPFinalStatus
            });

            if (BLPGroupStudentPayload.Payload?.Count() > 0)
            {
                var BLPGroupStudentPayloadList = BLPGroupStudentPayload.Payload.ToList();

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, BLPGroupStudentPayloadList, title);
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

        public byte[] GenerateExcel(ExportExcelBLPGroupStudentResult_ParamDesc paramDesc, List<GetBLPGroupStudentResult> dataList, string sheetTitle)
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
                excelSheet.SetColumnWidth(5, 30 * 256);
                excelSheet.SetColumnWidth(6, 20 * 256);
                excelSheet.SetColumnWidth(7, 30 * 256);
                excelSheet.SetColumnWidth(8, 30 * 256);

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
                cellTitleRow.SetCellValue("Student F2F Summary");
                cellTitleRow.CellStyle = styleHeader;

                rowIndex++;
                IRow titleRow2 = excelSheet.CreateRow(rowIndex);
                var cellTitleRow2 = titleRow2.CreateCell(1);
                cellTitleRow2.SetCellValue($"Generated Date: {DateTime.UtcNow.AddHours(7).ToString("dd-MM-yyyy HH:mm:ss")}");
                cellTitleRow2.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[5] { "Academic Year", "Level", "Grade", "Semester", "Final Status" };

                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYearDesc);
                    if (field == "Level")
                        cellValueParamRow.SetCellValue(paramDesc.LevelDesc);
                    if (field == "Grade")
                        cellValueParamRow.SetCellValue(paramDesc.GradeDesc);
                    if (field == "Semester")
                        cellValueParamRow.SetCellValue(paramDesc.Semester);
                    if (field == "Final Status")
                        cellValueParamRow.SetCellValue(paramDesc.BLPFinalStatusDesc == null ? "All" : paramDesc.BLPFinalStatusDesc);

                    rowIndex++;
                }

                rowIndex += 2;

                // summary content
                string[] headerList = null;

                if (paramDesc.IdSchool == "1")
                    headerList = new string[5] { "Class", "Student ID", "Name", "F2F Status", 
                        //"Last Modified", "Group", "Consent Form Hardcopy", 
                        "Final Status"};
                else if (paramDesc.IdSchool == "2")
                    headerList = new string[8] { "Class", "Student ID", "Name", "F2F Status",
                        "Last Modified", "Group", "Consent Form Hardcopy",
                        "Final Status"};
                else
                    headerList = new string[8] { "Class", "Student ID", "Name", "F2F Status",
                        "Last Modified", "Group", "Consent Form Hardcopy",
                        "Final Status"};

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

                        IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                        // Single Data
                        string[] contentDataList = null;

                        if (paramDesc.IdSchool == "1")
                            contentDataList = new string[4]
                            {
                                itemData.Homeroom.Description,
                                itemData.Student.Id,
                                itemData.Student.Name,
                                itemData.BLPStatus.Description,
                                //itemData.LastModifiedDate == null ? "-" : itemData.LastModifiedDate.Value.ToString("dd-MM-yyyy"),
                                //itemData.BLPGroup.Description,
                                //itemData.HardCopySubmissionDate == null ? "-" : itemData.HardCopySubmissionDate.Value.ToString("dd-MM-yyyy"),
                            };
                        else if (paramDesc.IdSchool == "2")
                            contentDataList = new string[7]
                            {
                                itemData.Homeroom.Description,
                                itemData.Student.Id,
                                itemData.Student.Name,
                                itemData.BLPStatus.Description,
                                itemData.LastModifiedDate == null ? "-" : itemData.LastModifiedDate.Value.ToString("dd-MM-yyyy"),
                                itemData.BLPGroup.Description,
                                itemData.HardCopySubmissionDate == null ? "-" : itemData.HardCopySubmissionDate.Value.ToString("dd-MM-yyyy"),
                            };
                        else
                            contentDataList = new string[7]
                            {
                                itemData.Homeroom.Description,
                                itemData.Student.Id,
                                itemData.Student.Name,
                                itemData.BLPStatus.Description,
                                itemData.LastModifiedDate == null ? "-" : itemData.LastModifiedDate.Value.ToString("dd-MM-yyyy"),
                                itemData.BLPGroup.Description,
                                itemData.HardCopySubmissionDate == null ? "-" : itemData.HardCopySubmissionDate.Value.ToString("dd-MM-yyyy"),
                            };

                        var cellData = totalRow2.CreateCell(1);
                        cellData.SetCellValue(contentDataList[0]);
                        cellData.CellStyle = styleTable;

                        int lastColumnCounter = 0;
                        for (int i = 2; i <= contentDataList.Count(); i++)
                        {
                            cellData = totalRow2.CreateCell(i);
                            cellData.SetCellValue(contentDataList[i - 1]);
                            cellData.CellStyle = styleTable;

                            lastColumnCounter = i;
                        }

                        // BLP Final Status Column
                        #region column style for BLP final status
                        ICellStyle styleTableBLPFinalStatusAllowed = workbook.CreateCellStyle();
                        styleTableBLPFinalStatusAllowed.BorderBottom = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowed.BorderLeft = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowed.BorderRight = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowed.BorderTop = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowed.VerticalAlignment = VerticalAlignment.Center;
                        styleTableBLPFinalStatusAllowed.WrapText = true;
                        styleTableBLPFinalStatusAllowed.FillForegroundColor = IndexedColors.Green.Index;
                        styleTableBLPFinalStatusAllowed.FillPattern = FillPattern.SolidForeground;
                        styleTableBLPFinalStatusAllowed.SetFont(font);


                        ICellStyle styleTableBLPFinalStatusNotAllowed = workbook.CreateCellStyle();
                        styleTableBLPFinalStatusNotAllowed.BorderBottom = BorderStyle.Thin;
                        styleTableBLPFinalStatusNotAllowed.BorderLeft = BorderStyle.Thin;
                        styleTableBLPFinalStatusNotAllowed.BorderRight = BorderStyle.Thin;
                        styleTableBLPFinalStatusNotAllowed.BorderTop = BorderStyle.Thin;
                        styleTableBLPFinalStatusNotAllowed.VerticalAlignment = VerticalAlignment.Center;
                        styleTableBLPFinalStatusNotAllowed.WrapText = true;
                        styleTableBLPFinalStatusNotAllowed.FillForegroundColor = IndexedColors.Red.Index;
                        styleTableBLPFinalStatusNotAllowed.FillPattern = FillPattern.SolidForeground;
                        styleTableBLPFinalStatusNotAllowed.SetFont(font);

                        ICellStyle styleTableBLPFinalStatusAllowedOnCondition = workbook.CreateCellStyle();
                        styleTableBLPFinalStatusAllowedOnCondition.BorderBottom = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowedOnCondition.BorderLeft = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowedOnCondition.BorderRight = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowedOnCondition.BorderTop = BorderStyle.Thin;
                        styleTableBLPFinalStatusAllowedOnCondition.VerticalAlignment = VerticalAlignment.Center;
                        styleTableBLPFinalStatusAllowedOnCondition.WrapText = true;
                        styleTableBLPFinalStatusAllowedOnCondition.FillForegroundColor = IndexedColors.Yellow.Index;
                        styleTableBLPFinalStatusAllowedOnCondition.FillPattern = FillPattern.SolidForeground;
                        styleTableBLPFinalStatusAllowedOnCondition.SetFont(font);
                        #endregion

                        cellData = totalRow2.CreateCell(lastColumnCounter+1);
                        cellData.SetCellValue(string.IsNullOrEmpty(itemData.FinalStatus?.Description) ? "-" : itemData.FinalStatus.FinalStatusEnum.GetDescription());
                        cellData.CellStyle = string.IsNullOrEmpty(itemData.FinalStatus?.Description) ? styleTable : 
                                        itemData.FinalStatus.FinalStatusEnum == BLPFinalStatus.NotAllowed ? styleTableBLPFinalStatusNotAllowed :
                                        itemData.FinalStatus.FinalStatusEnum == BLPFinalStatus.Allowed ? styleTableBLPFinalStatusAllowed :
                                        styleTableBLPFinalStatusAllowedOnCondition;

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
