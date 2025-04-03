using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Extensions;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.Grade;
using BinusSchool.Data.Model.School.FnSchool.Pathway;
using BinusSchool.Data.Model.Student.FnStudent.MapStudentPathway;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.MapStudentPathway
{
    public class DownloadMapStudentPathwayTemplateHandler : FunctionsHttpSingleHandler
    {
        private readonly IGrade _gradeService;
        private readonly IPathway _pathwayService;

        public DownloadMapStudentPathwayTemplateHandler(IGrade gradeService, IPathway pathwayService)
        {
            _gradeService = gradeService;
            _pathwayService = pathwayService;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadMapStudentPathwayRequest>(
                nameof(DownloadMapStudentPathwayRequest.IdSchool),
                nameof(DownloadMapStudentPathwayRequest.IdAcadyear));

            // get grades by acadyear
            var gradesResult = await _gradeService.GetGradesByAcadyear(new GetGradeAcadyearRequest
            {
                IdAcadyear = param.IdAcadyear,
                GetAll = true
            });
            var grades = gradesResult.Payload ?? Enumerable.Empty<GetGradeAcadyearResult>();

            // get pathways by acadyear
            var pathwaysResult = await _pathwayService.GetPathways(new GetPathwayRequest
            {
                IdSchool = new[] { param.IdSchool },
                IdAcadyear = param.IdAcadyear,
                GetAll = true
            });
            var pathways = pathwaysResult.Payload ?? Enumerable.Empty<GetPathwayResult>();

            var template = GenerateExcel(grades, pathways);

            return new FileContentResult(template, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"MappingStudentPathway_{pathways.First()?.Acadyear}_{DateTime.Now.Ticks}.xlsx"
            };
        }
        
        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        private byte[] GenerateExcel(IEnumerable<GetGradeAcadyearResult> grades, IEnumerable<GetPathwayResult> pathways)
        {
            using var ms = new MemoryStream();
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            // fontBold.IsBold = true;

            var style = workbook.CreateCellStyle();
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.SetFont(fontBold);

            var headers = new[]
            {
                "Level",
                "Grade",
                "Student ID",
                "Student Name",
                "Streaming",
                "Next Streaming",
            };
            var sheet = workbook.CreateSheet("Mapping Student Pathway");
            var row = sheet.CreateRow(0);
            CreateHeaders(sheet, row, headers, 0, style);

            var sheet2 = workbook.CreateSheet($"Reference ({pathways.First()?.Acadyear})");
            var row2 = sheet2.CreateRow(0);
            CreateHeaders(sheet2, row2, new[] { "Level", "Grade" }, 0, style);
            CreateHeaders(sheet2, row2, new[] { "Pathway" }, 3, style);

            var gs = grades.SelectMany(g => g.Grades, (l, g) => new[] { l.Code, g.Code }).ToArray();
            FillColumns(sheet2, gs, 1, 0, style);

            var ps = pathways.Select(x => new[] { x.Code }).ToArray();
            FillColumns(sheet2, ps, 1, 3, style);

            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }

        private void CreateHeaders(ISheet sheet, IRow row, string[] headers, int startColumn = 0, ICellStyle style = null)
        {
            for (var column = startColumn; column < headers.Length + startColumn; column++)
            {
                var cell = row.CreateCell(column);
                cell.SetCellValue(headers[column - startColumn]);
                sheet.AutoSizeColumn(column);

                if (style != null)
                    cell.CellStyle = style;
            }
        }

        private void FillColumns(ISheet sheet, string[][] datas, int startRow, int startColumn, ICellStyle style = null)
        {
            for (int row = startRow; row < datas.Length + startRow; row++)
            {
                var sheetRow = sheet.GetRow(row) ?? sheet.CreateRow(row);
                for (int column = startColumn; column < datas[row - startRow].Length + startColumn; column++)
                {
                    var sheetCell = sheetRow.GetCell(column) ?? sheetRow.CreateCell(column);
                    var val = datas[row - startRow][column - startColumn];
                    sheetCell.SetCellValue(val);
                    sheet.AutoSizeColumn(column);

                    if (style != null)
                        sheetCell.CellStyle = style;
                }
            }
        }
    }
}
