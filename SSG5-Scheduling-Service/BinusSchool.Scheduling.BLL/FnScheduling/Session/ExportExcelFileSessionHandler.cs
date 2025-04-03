using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnSchedule.Session
{
    public class ExportExcelFileSessionHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;

        public ExportExcelFileSessionHandler(ISchedulingDbContext schoolDbContext)
        {
            _dbContext = schoolDbContext;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();

            //try
            //{
            //    await req.HttpContext.DetermineLocalization();

            //    _userInfo = new AuthenticationInfo(req);
            //    if (!_userInfo.IsValid)
            //        throw new UnauthorizeException(_localizer[_userInfo.Message]);

            //    var param = req.ValidateParams<GetListSessionRequest>(nameof(GetListSessionRequest.IdSchool), nameof(GetListSessionRequest.IdSessionSet));
            //    var columns = new[] { "grade", "streamingPathway", "schoolDay", "name", "alias", "startTime", "endTime", "durationInMinutes" };
            //    var aliasColumns = new Dictionary<string, string>
            //{
            //    { columns[0], "gradePathway.grade.description" },
            //    { columns[1], "gradePathway.gradePathwayDetails.pathway.description" },
            //    { columns[2], "day.description" },
            //};

            //    var predicate = PredicateBuilder.Create<SchoolSession>(x
            //        => param.IdSchool.Contains(x.SessionSet.IdSchool)
            //        && x.IdSessionSet == param.IdSessionSet
            //        && (EF.Functions.Like(x.SchoolAcadYearLevelGradePathway.SchoolAcadyearLevelGrade.SchoolGrade.Description, param.SearchPattern())
            //        || EF.Functions.Like(x.DaysName.Description, param.SearchPattern())
            //        || EF.Functions.Like(x.Name, param.SearchPattern())
            //        || EF.Functions.Like(x.Alias, param.SearchPattern())
            //        || EF.Functions.Like(Convert.ToString(x.DurationInMinutes), param.SearchPattern())
            //        || x.SchoolAcadYearLevelGradePathway.SchoolAcadYearLevelGradePathwayDetails.Any(y => y.SchoolPathway.Description.Contains(param.Search))));

            //    // don't parse to TimeSpan when Search can parse to int
            //    if (!int.TryParse(param.Search, out _) && TimeSpan.TryParse(param.Search, out var time))
            //    {
            //        // if (param.SearchPattern(columns[5]) != "%")
            //        //     predicate = predicate.Or(x => x.StartTime == time);
            //        // if (param.SearchPattern(columns[6]) != "%")
            //        //     predicate = predicate.Or(x => x.EndTime == time);
            //        if (param.SearchPattern() != "%")
            //            predicate = predicate.Or(x => x.StartTime == time || x.EndTime == time);
            //    }

            //    if (!string.IsNullOrWhiteSpace(param.IdGrade))
            //        predicate = predicate.And(x => x.SchoolAcadYearLevelGradePathway.SchoolAcadyearLevelGrade.IdSchoolGrade == param.IdGrade);
            //    if (!string.IsNullOrWhiteSpace(param.IdStreaming))
            //        predicate = predicate.And(x => x.SchoolAcadYearLevelGradePathway.SchoolAcadYearLevelGradePathwayDetails.Any(y => y.IdSchoolPathway == param.IdStreaming));
            //    if (!string.IsNullOrWhiteSpace(param.IdDays))
            //        predicate = predicate.And(x => x.IdDaysName == param.IdDays);

            //    var query = _dbContext.Entity<SchoolSession>()
            //        .Include(p => p.SchoolAcadYearLevelGradePathway).ThenInclude(p => p.SchoolAcadYearLevelGradePathwayDetails).ThenInclude(p => p.SchoolPathway)
            //        .Include(p => p.SchoolAcadYearLevelGradePathway).ThenInclude(p => p.SchoolAcadyearLevelGrade).ThenInclude(p => p.SchoolGrade)
            //        .Include(p => p.DaysName)
            //        .Where(predicate)
            //        .OrderByDynamic(param, aliasColumns);

            //    var get = await query.SetPagination(param).ToListAsync(cancellationToken);
            //    var getExcel = GenerateExcel(get);


            //    return new FileContentResult(getExcel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            //    {
            //        FileDownloadName = $"ExportSession_{DateTime.Now.Ticks}.xlsx"
            //    };

            //}
            //catch (Exception ex)
            //{
            //    log.LogError(ex, ex.Message);
            //    return req.CreateApiErrorResponse(ex);
            //}
        }

        #region Helpers
        //belum ada kebutuhan 
        public byte[] GenerateExcel(List<MsSession> data)
        {
            var result = new byte[0];
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Session Data");

                //Create style
                ICellStyle style = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;
                // style.BottomBorderColor = HSSFColor.Yellow.Index;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Arial";
                font.FontHeight = 13;
                font.IsItalic = true;
                style.SetFont(font);

                //header 
                IRow row = excelSheet.CreateRow(0);

                var header = row.CreateCell(2);
                header.SetCellValue("No");
                header.CellStyle = style;

                var header2 = row.CreateCell(3);
                header2.SetCellValue("Grade");
                header2.CellStyle = style;

                var header3 = row.CreateCell(4);
                header3.SetCellValue("Streaming");
                header3.CellStyle = style;

                var header4 = row.CreateCell(5);
                header4.SetCellValue("School Day");
                header4.CellStyle = style;

                var header5 = row.CreateCell(6);
                header5.SetCellValue("Session Name");
                header5.CellStyle = style;

                var header6 = row.CreateCell(7);
                header6.SetCellValue("Session Alias");
                header6.CellStyle = style;

                var header7 = row.CreateCell(8);
                header7.SetCellValue("Start Time");
                header7.CellStyle = style;

                var header8 = row.CreateCell(9);
                header8.SetCellValue("End Time");
                header8.CellStyle = style;

                var header9 = row.CreateCell(10);
                header9.SetCellValue("Duration in Minute");
                header9.CellStyle = style;

                for (int i = 0; i < data.Count(); i++)
                {
                    //body
                    row = excelSheet.CreateRow(row.RowNum + 1);

                    row.CreateCell(2).SetCellValue(i);
                    row.Cells[0].CellStyle = style;

                    row.CreateCell(3).SetCellValue(data[i].GradePathway.Grade.Description);
                    row.Cells[1].CellStyle = style;

                    row.CreateCell(4).SetCellValue(string.Join("-", data[i].GradePathway.GradePathwayDetails.Select(p => p.Pathway.Description)));
                    row.Cells[2].CellStyle = style;

                    row.CreateCell(5).SetCellValue(data[i].Day.Description);
                    row.Cells[3].CellStyle = style;

                    row.CreateCell(6).SetCellValue(data[i].Name);
                    row.Cells[4].CellStyle = style;

                    row.CreateCell(7).SetCellValue(data[i].Alias);
                    row.Cells[5].CellStyle = style;

                    //row.CreateCell(8).SetCellValue(data[i].StartTime.ToString(@"hh\\:mm"));
                    row.CreateCell(8).SetCellValue(Math.Round(data[i].StartTime.TotalHours) + ":" + data[i].StartTime.Minutes.ToString("00"));
                    row.Cells[6].CellStyle = style;

                    //row.CreateCell(9).SetCellValue(data[i].EndTime.ToString(@"hh\\:mm"));
                    row.CreateCell(9).SetCellValue(Math.Round(data[i].EndTime.TotalHours) + ":" + data[i].EndTime.Minutes.ToString("00"));
                    row.Cells[7].CellStyle = style;

                    row.CreateCell(10).SetCellValue(data[i].DurationInMinutes.ToString());
                    row.Cells[8].CellStyle = style;
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }
        #endregion
    }
}
