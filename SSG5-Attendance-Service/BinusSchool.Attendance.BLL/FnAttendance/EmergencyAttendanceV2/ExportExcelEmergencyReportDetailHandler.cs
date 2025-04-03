using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2.Validator;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.Attendance.FnAttendance;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Attendance.FnAttendance.EmergencyAttendanceV2
{
    public class ExportExcelEmergencyReportDetailHandler : FunctionsHttpSingleHandler
    {
        private readonly IAttendanceDbContext _dbContext;
        private readonly IEmergencyAttendanceV2 _emergencyAttendanceV2Api;
        private string _title = "EmergencyReportDetail";

        public ExportExcelEmergencyReportDetailHandler(IAttendanceDbContext dbContext,
            IEmergencyAttendanceV2 emergencyAttendanceV2Api)
        {
            _dbContext = dbContext;
            _emergencyAttendanceV2Api = emergencyAttendanceV2Api;
        }
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var body = await Request.GetBody<ExportExcelEmergencyReportDetailRequest>();

            EmergencyReportDetail_paramDescVm paramDesc = new EmergencyReportDetail_paramDescVm();

            if (!string.IsNullOrEmpty(body.idHomeroom))
            {
                paramDesc = await _dbContext.Entity<MsHomeroom>()
                                    .Where(a => a.Id == body.idHomeroom)
                                    .Select(a => new EmergencyReportDetail_paramDescVm()
                                    {
                                        HomeroomName = a.Grade.Code + "" + a.GradePathwayClassroom.Classroom.Code,
                                        GradeName = a.Grade.Description,
                                        LevelName = a.Grade.Level.Description
                                    })
                                    .FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(body.idGrade))
            {
                paramDesc = await _dbContext.Entity<MsGrade>()
                                  .Where(a => a.Id == body.idGrade)
                                  .Select(a => new EmergencyReportDetail_paramDescVm()
                                  {
                                      GradeName = a.Description,
                                      LevelName = a.Level.Description
                                  })
                                  .FirstOrDefaultAsync();
            }
            else if (!string.IsNullOrEmpty(body.idLevel))
            {
                paramDesc = await _dbContext.Entity<MsLevel>()
                               .Where(a => a.Id == body.idLevel)
                               .Select(a => new EmergencyReportDetail_paramDescVm()
                               {
                                   LevelName = a.Description
                               })
                               .FirstOrDefaultAsync();
            }

            if (!string.IsNullOrEmpty(body.IdEmergencyStatus))
            {
                var getEmergency = await _dbContext.Entity<LtEmergencyStatus>()
                               .Where(a => a.Id == body.IdEmergencyStatus)
                               .Select(a => new EmergencyReportDetail_paramDescVm()
                               {
                                   EmergencyStatusName = a.EmergencyStatusName
                               })
                               .FirstOrDefaultAsync();

                paramDesc.EmergencyStatusName = getEmergency?.EmergencyStatusName;
            }

            var getEmergencyReport = await _dbContext.Entity<TrEmergencyReport>()
                               .Where(a => a.Id == body.idEmergencyReport)
                               .Select(a => new EmergencyReportDetail_paramDescVm()
                               {
                                   AcademicYearName = a.AcademicYear.Description,
                                   StartedBy = a.StartedBy,
                                   StartedDate = a.StartedDate,
                                   ReportedBy = a.ReportedBy,
                                   ReportedDate = a.ReportedDate
                               })
                               .ToListAsync();

            var results2 = from report in getEmergencyReport
                           join userIn in _dbContext.Entity<MsUser>().Where(a => getEmergencyReport.Select(b => b.StartedBy).Contains(a.Id)).ToList()
                           on report.ReportedBy equals userIn.Id into userIn2
                           from userIn in userIn2.DefaultIfEmpty()
                           join userUp in _dbContext.Entity<MsUser>().Where(a => getEmergencyReport.Select(b => b.ReportedBy).Contains(a.Id)).ToList()
                           on report.StartedBy equals userUp.Id into userUp2
                           from userUp in userUp2.DefaultIfEmpty()
                           select new EmergencyReportDetail_paramDescVm
                           {
                               AcademicYearName = report.AcademicYearName,
                               StartedDate = report.StartedDate,
                               StartedBy = userIn != null ? userIn.DisplayName : report.StartedBy,
                               ReportedDate = report.ReportedDate,
                               ReportedBy = userUp != null ? userUp.DisplayName : report.ReportedBy
                           };

            var result = results2.FirstOrDefault();

            if (result != null)
            {
                paramDesc.AcademicYearName = result?.AcademicYearName;
                paramDesc.StartedDate = (result != null ? result.StartedDate : new DateTime());
                paramDesc.StartedBy = result?.StartedBy;
                paramDesc.ReportedDate = (result != null ? result.ReportedDate : new DateTime());
                paramDesc.ReportedBy = result?.ReportedBy;
            }

            var paramApi = new GetEmergencyReportDetailRequest();
            paramApi.idEmergencyReport = body.idEmergencyReport;
            paramApi.idLevel = body.idLevel;
            paramApi.idGrade = body.idGrade;
            paramApi.idHomeroom = body.idHomeroom;
            paramApi.IdEmergencyStatus = body.IdEmergencyStatus;
            paramApi.GetAll = true;

            var getEmergencyReportDetail = await _emergencyAttendanceV2Api.GetEmergencyReportDetail(paramApi);

            if (getEmergencyReportDetail.Payload?.Count() > 0)
            {
                var emergencyReportDetailList = getEmergencyReportDetail.Payload.ToList();

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, emergencyReportDetailList, _title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{_title}_{DateTime.Now.Ticks}.xlsx"
                };
            }
            else
            {
                var generateExcelByte = GenerateBlankExcel(_title);
                return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"{_title}_{DateTime.Now.Ticks}.xlsx"
                };
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

        public byte[] GenerateExcel(EmergencyReportDetail_paramDescVm paramDesc, List<GetEmergencyReportDetailResult> dataList, string sheetTitle)
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

                ICellStyle styleHeader2 = workbook.CreateCellStyle();
                styleHeader2.VerticalAlignment = VerticalAlignment.Center;
                styleHeader2.Alignment = HorizontalAlignment.Center;
                styleHeader2.WrapText = true;


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
                styleHeader2.SetFont(fontHeader);

                int rowIndex = 0;
                int startColumn = 1;

                //Title 
                rowIndex++;
                IRow titleRow = excelSheet.CreateRow(rowIndex);
                var cellTitleRow = titleRow.CreateCell(1);
                var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 5);
                cellTitleRow.SetCellValue("Emergency Attendance Report Detail");
                excelSheet.AddMergedRegion(merge);
                cellTitleRow.CellStyle = styleHeader2;               

                rowIndex++;
                IRow titleRow2 = excelSheet.CreateRow(rowIndex);
                var cellTitleRow2 = titleRow2.CreateCell(5);
                var mergeGenDate = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, 5, 6);
                cellTitleRow2.SetCellValue($"Generated Date: {DateTime.UtcNow.AddHours(7).ToString("dd MMM yyyy HH:mm")}");
                excelSheet.AddMergedRegion(mergeGenDate);

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[9] { "Academic Year", "Level", "Grade", "Homeroom", "Emergency Status", "StartBy", "Start Date", "ReportBy", "Report Date" };

                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYearName == null ? "All" : paramDesc.AcademicYearName);
                    if (field == "Level")
                        cellValueParamRow.SetCellValue(paramDesc.LevelName == null ? "All" : paramDesc.LevelName);
                    if (field == "Grade")
                        cellValueParamRow.SetCellValue(paramDesc.GradeName == null ? "All" : paramDesc.GradeName);
                    if (field == "Homeroom")
                        cellValueParamRow.SetCellValue(paramDesc.HomeroomName == null ? "All" : paramDesc.HomeroomName);
                    if (field == "Emergency Status")
                        cellValueParamRow.SetCellValue(paramDesc.EmergencyStatusName == null ? "All" : paramDesc.EmergencyStatusName);
                    if (field == "StartBy")                   
                        cellValueParamRow.SetCellValue(paramDesc.StartedBy == null ? "-" : paramDesc.StartedBy);                    
                    if (field == "Start Date")
                        cellValueParamRow.SetCellValue(paramDesc.StartedDate == null ? "-" : paramDesc.StartedDate.ToString("dd MMM yyyy HH:mm"));
                    if (field == "ReportBy")
                        cellValueParamRow.SetCellValue(paramDesc.ReportedBy == null ? "-" : paramDesc.ReportedBy);
                    if (field == "Report Date")
                        cellValueParamRow.SetCellValue(paramDesc.ReportedDate == null ? "-" : ((DateTime)paramDesc.ReportedDate).ToString("dd MMM yyyy HH:mm"));

                    if (field == "StartBy" || field == "ReportBy")
                    {
                        var merge2 = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn+1, startColumn+2);
                        excelSheet.AddMergedRegion(merge2);
                    }

                    rowIndex++;
                }

                rowIndex += 2;

                // summary content
                string[] headerList = null;

                //headerList = new string[7] { "Student ID", "Student Name", "Level Name", "Homeroom Name", "Emergency Status", "Description", "Marked By" };
                headerList = new string[6] { "Student ID", "Student Name", "Level Name", "Homeroom Name", "Emergency Status", "Marked By" };

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

                        var studEmergencyStatus = itemData.emergencyStatusList.Where(a => a.isSelected == true).FirstOrDefault();
                        // Single Data
                        string[] contentDataList = null;

                        contentDataList = new string[6]
                        {
                            itemData.student.Id,
                            itemData.student.Description,
                            itemData.levelName,
                            itemData.homeroomName,
                            studEmergencyStatus?.emergencyStatusName??"-",
                            //studEmergencyStatus?.description??"-",
                            studEmergencyStatus?.markBy??"-",
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

                        //// BLP Final Status Column
                        //#region column style for BLP final status
                        //ICellStyle styleTableBLPFinalStatusAllowed = workbook.CreateCellStyle();
                        //styleTableBLPFinalStatusAllowed.BorderBottom = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowed.BorderLeft = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowed.BorderRight = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowed.BorderTop = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowed.VerticalAlignment = VerticalAlignment.Center;
                        //styleTableBLPFinalStatusAllowed.WrapText = true;
                        //styleTableBLPFinalStatusAllowed.FillForegroundColor = IndexedColors.Green.Index;
                        //styleTableBLPFinalStatusAllowed.FillPattern = FillPattern.SolidForeground;
                        //styleTableBLPFinalStatusAllowed.SetFont(font);


                        //ICellStyle styleTableBLPFinalStatusNotAllowed = workbook.CreateCellStyle();
                        //styleTableBLPFinalStatusNotAllowed.BorderBottom = BorderStyle.Thin;
                        //styleTableBLPFinalStatusNotAllowed.BorderLeft = BorderStyle.Thin;
                        //styleTableBLPFinalStatusNotAllowed.BorderRight = BorderStyle.Thin;
                        //styleTableBLPFinalStatusNotAllowed.BorderTop = BorderStyle.Thin;
                        //styleTableBLPFinalStatusNotAllowed.VerticalAlignment = VerticalAlignment.Center;
                        //styleTableBLPFinalStatusNotAllowed.WrapText = true;
                        //styleTableBLPFinalStatusNotAllowed.FillForegroundColor = IndexedColors.Red.Index;
                        //styleTableBLPFinalStatusNotAllowed.FillPattern = FillPattern.SolidForeground;
                        //styleTableBLPFinalStatusNotAllowed.SetFont(font);

                        //ICellStyle styleTableBLPFinalStatusAllowedOnCondition = workbook.CreateCellStyle();
                        //styleTableBLPFinalStatusAllowedOnCondition.BorderBottom = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowedOnCondition.BorderLeft = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowedOnCondition.BorderRight = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowedOnCondition.BorderTop = BorderStyle.Thin;
                        //styleTableBLPFinalStatusAllowedOnCondition.VerticalAlignment = VerticalAlignment.Center;
                        //styleTableBLPFinalStatusAllowedOnCondition.WrapText = true;
                        //styleTableBLPFinalStatusAllowedOnCondition.FillForegroundColor = IndexedColors.Yellow.Index;
                        //styleTableBLPFinalStatusAllowedOnCondition.FillPattern = FillPattern.SolidForeground;
                        //styleTableBLPFinalStatusAllowedOnCondition.SetFont(font);
                        //#endregion

                        //cellData = totalRow2.CreateCell(lastColumnCounter + 1);
                        //cellData.SetCellValue(string.IsNullOrEmpty(itemData.FinalStatus?.Description) ? "-" : itemData.FinalStatus.FinalStatusEnum.GetDescription());
                        //cellData.CellStyle = string.IsNullOrEmpty(itemData.FinalStatus?.Description) ? styleTable :
                        //                itemData.FinalStatus.FinalStatusEnum == BLPFinalStatus.NotAllowed ? styleTableBLPFinalStatusNotAllowed :
                        //                itemData.FinalStatus.FinalStatusEnum == BLPFinalStatus.Allowed ? styleTableBLPFinalStatusAllowed :
                        //                styleTableBLPFinalStatusAllowedOnCondition;

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

        public class EmergencyReportDetail_paramDescVm
        {
            public string StartedBy { get; set; }
            public DateTime StartedDate { get; set; }
            public string ReportedBy { get; set; }
            public DateTime? ReportedDate { get; set; }
            public string AcademicYearName { get; set; }
            public string LevelName { get; set; }
            public string GradeName { get; set; }
            public string HomeroomName { get; set; }
            public string EmergencyStatusName { get; set; }

        }

    }
}
