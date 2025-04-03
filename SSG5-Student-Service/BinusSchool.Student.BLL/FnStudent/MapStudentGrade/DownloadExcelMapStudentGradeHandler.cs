using System;
using System.IO;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MapStudentGrade
{
    public class DownloadExcelMapStudentGradeHandler : FunctionsHttpSingleHandler
    {
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet();

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;

            var headerCellStyle = workbook.CreateCellStyle();
            headerCellStyle.CloneStyleFrom(borderCellStyle);
            headerCellStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            headerCellStyle.FillPattern = FillPattern.SolidForeground;

            var firstRow = sheet.CreateRow(0);
            var firstCell = firstRow.CreateCell(0);
            firstCell.SetCellValue("Student ID");
            firstCell.CellStyle = headerCellStyle;

            var studentIds = new string[] { "1070000931", "1070000957", "1070000974", "1070001021", "1070001034", "1070001047" };
            int i = 1;

            foreach (var studentId in studentIds)
            {
                var contentRow = sheet.CreateRow(i++);
                var contentCell = contentRow.CreateCell(0);
                contentCell.SetCellValue(studentId);
                contentCell.CellStyle = borderCellStyle;
            }

            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                var result = ms.ToArray();

                return new FileContentResult(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Template Excel Upload Mapping Student Grade.xlsx"
                };
            }
        }
    }
}
