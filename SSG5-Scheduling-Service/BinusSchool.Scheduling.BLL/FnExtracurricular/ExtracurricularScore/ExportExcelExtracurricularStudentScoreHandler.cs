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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.ExtracurricularScore;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;

namespace BinusSchool.Scheduling.FnExtracurricular.ExtracurricularScore
{
    public class ExportExcelExtracurricularStudentScoreHandler : FunctionsHttpSingleHandler
    { 
        private readonly ISchedulingDbContext _dbContext;
        private readonly IExtracurricularScore _extracurricularScoreApi;
        public ExportExcelExtracurricularStudentScoreHandler(
            ISchedulingDbContext dbContext, 
            IExtracurricularScore extracurricularScoreApi)
        {
            _dbContext = dbContext;
            _extracurricularScoreApi = extracurricularScoreApi;
        }

        protected override async Task<IActionResult> RawHandler()
        {          
            //var param = Request.ValidateParams<GetExtracurricularStudentScoreRequest>(nameof(GetExtracurricularStudentScoreRequest.IdAcademicYear), nameof(GetExtracurricularStudentScoreRequest.Semester), nameof(GetExtracurricularStudentScoreRequest.IdExtracurricular));

            var param = await Request.GetBody<GetExtracurricularStudentScoreRequest>();

            var AcademicYear = _dbContext.Entity<MsAcademicYear>()
                               .Where(a => a.Id == param.IdAcademicYear)
                               .FirstOrDefault();

            var StudentScoreList = await _extracurricularScoreApi.GetExtracurricularStudentScore(param);

            var ScoreLegends = await _extracurricularScoreApi.GetExtracurricularScoreLegend2(new GetExtracurricularScoreLegendRequest2() { IdSchool = AcademicYear.IdSchool});

            var title = "Electives Score";

            if (StudentScoreList.Payload != null)
            {
                var generateExcelByte = GenerateExcel(StudentScoreList.Payload, ScoreLegends.Payload, title, AcademicYear.Description, param.Semester);
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
        protected override async Task<ApiErrorResult<object>> Handler()
        {
           
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string sheetTitle)
        {
            var result = new byte[0];
            //string[] fieldDataList = fieldData.Split(",");

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var TitleValidated = regex.Replace(sheetTitle, " ");
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(TitleValidated);

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

        public void SetDynamicColumnWidthExcel(int columnComponentCount, ref ISheet excelSheet)
        {
            for (int i = 0; i < columnComponentCount; i++)
            {
                excelSheet.SetColumnWidth(i, 30 * 256);
            }
        }

        public byte[] GenerateExcel(GetExtracurricularStudentScoreResult data, IEnumerable<GetExtracurricularScoreLegendResult2> ScoreLegends, string sheetTitle, string AY, int Smt)
        {
            var result = new byte[0];
            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            var TitleValidated = regex.Replace(sheetTitle, " ");

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(TitleValidated);

                int columnComponentCount = 3;

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

                //Create style for header
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

                //header 
                //IRow row = excelSheet.CreateRow(0);

                //Title 
                IRow row = excelSheet.CreateRow(0);
                var cellTitleRow = row.CreateCell(0);
                cellTitleRow.SetCellValue("Electives Score Summary");
                cellTitleRow.CellStyle = headerStyle;
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, 3));

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Academic Year :");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(AY);
                row.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 3; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = dataStyle;
                }
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 3));

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Semester :");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(Smt);
                row.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 3; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = dataStyle;
                }
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 3));

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Electives :");
                row.Cells[0].CellStyle = dataStyle;
                row.CreateCell(1).SetCellValue(data.Body.FirstOrDefault()?.ExtracurricularName??"");
                row.Cells[1].CellStyle = dataStyle;
                for (int i = 2; i <= 3; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = dataStyle;
                }
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 3));

                row = excelSheet.CreateRow(row.RowNum + 1);

                //Body

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("StudentID");
                row.Cells[0].CellStyle = headerStyle;
                row.CreateCell(1).SetCellValue("Student Name");
                row.Cells[1].CellStyle = headerStyle;
                row.CreateCell(2).SetCellValue("Homeroom");
                row.Cells[2].CellStyle = headerStyle;

                int startIndex = 3;
                foreach (var rptStudentScore in data.Header)
                {
                    row.CreateCell(startIndex).SetCellValue(rptStudentScore.ScoreComponentName);
                    row.Cells[startIndex].CellStyle = headerStyle;
                    startIndex++;
                }               

                foreach (var rptStudentScore in data.Body)
                {

                    row = excelSheet.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue(rptStudentScore.IdStudent);
                    row.Cells[0].CellStyle = dataStyle;
                    row.CreateCell(1).SetCellValue(rptStudentScore.StudentName);
                    row.Cells[1].CellStyle = dataStyle;
                    row.CreateCell(2).SetCellValue(rptStudentScore.Homeroom);
                    row.Cells[2].CellStyle = dataStyle;

                    startIndex = 3;
                    foreach (var rptComponentScore in rptStudentScore.ComponentScores)
                    {
                        row.CreateCell(startIndex).SetCellValue((rptComponentScore.score == null ? "-" : rptComponentScore.score.Description));
                        row.Cells[startIndex].CellStyle = dataStyle;
                        startIndex++;
                    }
                }


                row = excelSheet.CreateRow(row.RowNum + 1);       
                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Score Legend");
                row.Cells[0].CellStyle = headerStyle;
                for (int i = 1; i <= 2; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = headerStyle;
                }
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 0, 2));

                row = excelSheet.CreateRow(row.RowNum + 1);
                row.CreateCell(0).SetCellValue("Score");
                row.Cells[0].CellStyle = headerStyle;          
                row.CreateCell(1).SetCellValue("Description");
                row.Cells[1].CellStyle = headerStyle;
                for (int i = 2; i <= 2; i++)
                {
                    row.CreateCell(i).SetCellValue("");
                    row.Cells[i].CellStyle = headerStyle;
                }
                excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 2));

                foreach (var rptLegend in ScoreLegends)
                {

                    row = excelSheet.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue(rptLegend.Description);
                    row.Cells[0].CellStyle = headerStyle;              
                    for (int i = 1; i <= 2; i++)
                    {
                        row.CreateCell(i).SetCellValue("");
                        row.Cells[i].CellStyle = headerStyle;
                    }
                    excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 2));

                    foreach (var rptitm in rptLegend.ScoreLegends)
                    {
                        row = excelSheet.CreateRow(row.RowNum + 1);
                        row.CreateCell(0).SetCellValue(rptitm.Score);
                        row.Cells[0].CellStyle = dataStyle;
                        row.CreateCell(1).SetCellValue(rptitm.Description);
                        row.Cells[1].CellStyle = dataStyle;
                        for (int i = 2; i <= 2; i++)
                        {
                            row.CreateCell(i).SetCellValue("");
                            row.Cells[i].CellStyle = dataStyle;
                        }
                        excelSheet.AddMergedRegion(new NPOI.SS.Util.CellRangeAddress(row.RowNum, row.RowNum, 1, 2));

                    }

                    row = excelSheet.CreateRow(row.RowNum + 1);
                    row.CreateCell(0).SetCellValue("");
                    row.Cells[0].CellStyle = dataStyle;

                }

                SetDynamicColumnWidthExcel(columnComponentCount + data.Header.Count, ref excelSheet);

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
