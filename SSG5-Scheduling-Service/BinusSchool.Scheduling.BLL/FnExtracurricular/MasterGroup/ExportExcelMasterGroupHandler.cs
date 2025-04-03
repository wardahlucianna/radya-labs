using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.Scheduling.FnExtracurricular;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterGroup;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterGroup
{
    public class ExportExcelMasterGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMasterGroup _extracurricularMasterGroupApi;

        public ExportExcelMasterGroupHandler(ISchedulingDbContext dbContext, IMasterGroup masterGroupExtracurricularApi)
        {
            _dbContext = dbContext;
            _extracurricularMasterGroupApi = masterGroupExtracurricularApi;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<ExportExcelMasterGroupRequest>();

            // param desc
            string statusString = "";
            if(param.Status == null) statusString = "All";
            else if(param.Status == true) statusString = "Active";
            else if (param.Status == false) statusString = "Inactive";

            GetMasterGroupParameterDescriptionResult paramDesc = new GetMasterGroupParameterDescriptionResult();
            paramDesc.Status = statusString;

            var masterGroupList = await _extracurricularMasterGroupApi.GetMasterGroup(new GetMasterGroupRequest
            {
                IdSchool = param.IdSchool,
                Status = param.Status,
                OrderBy = "group.name",
                OrderType = 0
            });

            var title = "ElectivesMasterGroup";
            if (masterGroupList.Payload?.Count() > 0)
            {
                var generateExcelByte = GenerateExcel(paramDesc, masterGroupList.Payload.ToList(), title);
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

        public byte[] GenerateExcel(GetMasterGroupParameterDescriptionResult paramDesc, List<GetMasterGroupResult> data, string sheetTitle)
        {
            var result = new byte[0];

            var School = data.Select(x => x.School.Description).FirstOrDefault();

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

                //Create style
                ICellStyle style = workbook.CreateCellStyle();
                style.WrapText = true;

                ICellStyle styleTable = workbook.CreateCellStyle();
                styleTable.BorderBottom = BorderStyle.Thin;
                styleTable.BorderLeft = BorderStyle.Thin;
                styleTable.BorderRight = BorderStyle.Thin;
                styleTable.BorderTop = BorderStyle.Thin;
                styleTable.WrapText = true;

                ICellStyle styleHeader = workbook.CreateCellStyle();
                styleHeader.WrapText = true;

                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;
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
                cellTitleRow.SetCellValue("Electives Master Group");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[2] { "School", "Status" };
                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "School")
                        cellValueParamRow.SetCellValue(School??"");

                    if (field == "Status")
                        cellValueParamRow.SetCellValue(paramDesc.Status);
                    rowIndex++;
                }

                rowIndex += 2;

                //Summary
                var headerList = new string[3] { "Electives Group", "Description", "Status" };

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
                
                foreach (var itemData in data)
                {
                    IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                    var cellGroup = totalRow2.CreateCell(1);
                    cellGroup.SetCellValue(itemData.Group.Name);
                    cellGroup.CellStyle = styleTable;

                    var cellDescription = totalRow2.CreateCell(2);
                    cellDescription.SetCellValue(itemData.Description);
                    cellDescription.CellStyle = styleTable;

                    //var cellSchool = totalRow2.CreateCell(3);
                    //cellSchool.SetCellValue(itemData.School.Description);
                    //cellSchool.CellStyle = styleTable;

                    var cellStatus = totalRow2.CreateCell(3);
                    cellStatus.SetCellValue(itemData.Status == true ? "Active" : "Inactive");
                    cellStatus.CellStyle = styleTable;

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
