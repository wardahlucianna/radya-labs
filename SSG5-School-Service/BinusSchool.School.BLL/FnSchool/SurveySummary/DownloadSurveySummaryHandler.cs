using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.School.FnSchool.SurveySummary
{
    public class DownloadSurveySummaryHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly ISurveySummary _serviceSummary;
        private readonly IMachineDateTime _machineDateTime;

        public DownloadSurveySummaryHandler(ISchoolDbContext dbContext, ISurveySummary serviceSummary, IMachineDateTime machineDateTime)
        {
            _dbContext = dbContext;
            _serviceSummary = serviceSummary;
            _machineDateTime = machineDateTime;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<DownloadSurveySummaryRequest>();
            
            // Init log
            try
            {
                // Get SurveySummaryRespondent for Participant sheet
                GetSurveySummaryRespondentRequest _paramSummaryRespondent = new GetSurveySummaryRespondentRequest
                {
                    Id = param.Id,
                    Return = Common.Model.Enums.CollectionType.Lov,
                    GetAll = true
                };

                var listRespondentResult = await _serviceSummary.GetSurveySummaryRespondent(_paramSummaryRespondent);
                var listRespondentAll = listRespondentResult.Payload;
                // Get data by role
                var listRespondent = listRespondentAll.OrderBy(x => x.Grade).ThenBy(x => x.Homeroom).ToList();

                var excelFile = SurveySummaryGenerateExcel(listRespondent);

                return new FileContentResult(excelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                {
                    FileDownloadName = $"Survey_summary_{DateTime.Now.Ticks}.xlsx"
                };
            }
            catch (Exception e)
            {
                // Create failed log, sent to email
                throw;
            }            
        }

        private byte[] SurveySummaryGenerateExcel(List<GetSurveySummaryRespondentResult> listRespondent)
        {
            var workbook = new XSSFWorkbook();
            var boldFont = workbook.CreateFont();
            boldFont.IsBold = true;
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            headerStyle.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;

            var contentStyle = workbook.CreateCellStyle();
            contentStyle.CloneStyleFrom(headerStyle);

            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            headerStyle.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
            headerStyle.SetFont(boldFont);
            headerStyle.VerticalAlignment = VerticalAlignment.Center;

            var participantSheet = workbook.CreateSheet("Participant");
            // i = rowNumber, j = cellNumber
            int i = 0, j = 0;
            var rowHeader = participantSheet.CreateRow(i);
            CellRangeAddress cra = new CellRangeAddress(i, i, j, j + 3);
            var cell = rowHeader.CreateCell(j);
            cell.CellStyle = headerStyle;
            cell.SetCellValue("Get Data Time");
            participantSheet.AddMergedRegion(cra);

            j = 4;
            cra = new CellRangeAddress(i, i, j, j + 2);
            cell = rowHeader.CreateCell(j);
            cell.SetCellValue(_machineDateTime.ServerTime.Date.ToString("dd/MM/yyyy HH:mm:ss"));
            participantSheet.AddMergedRegion(cra);

            // Items
            i = 2;
            j = 0;
            var contentHeader = participantSheet.CreateRow(i);
            var contentHeaderList = new List<string>() { "Level", "Grade", "Homeroom", "Total Student", "Total Respondent", "Total Not Answered", "%" };

            for (int z = 0; z < contentHeaderList.Count; z++)
            {
                cell = contentHeader.CreateCell(j + z);
                cell.SetCellValue(contentHeaderList[z]);
                cell.CellStyle = headerStyle;
            }

            foreach (var respondent in listRespondent)
            {
                j = 0;
                var contentItem = participantSheet.CreateRow(++i);

                for (int z = j; z < 7; z++)
                {
                    cell = contentItem.CreateCell(z);
                    switch (z)
                    {
                        case 0:
                            cell.SetCellValue(respondent.Level);
                            break;
                        case 1:
                            cell.SetCellValue(respondent.Grade);
                            break;
                        case 2:
                            cell.SetCellValue(respondent.Homeroom);
                            break;
                        case 3:
                            cell.SetCellValue(respondent.Total);
                            break;
                        case 4:
                            cell.SetCellValue(respondent.TotalRespondent);
                            break;
                        case 5:
                            cell.SetCellValue(respondent.TotalNotAnswer);
                            break;
                        case 6:
                            cell.SetCellValue(respondent.Percent);
                            break;
                        default:
                            break;
                    }
                    cell.CellStyle = contentStyle;
                }
            }

            i = 2; 
            j = 8;

            contentHeaderList.Clear();
            contentHeaderList.AddRange(new List<string>() { "Level", "Grade", "Total Student", "Total Respondent", "Total Not Answered", "%" });

            for (int z = 0; z < contentHeaderList.Count; z++)
            {
                cell = contentHeader.CreateCell(j + z);
                cell.SetCellValue(contentHeaderList[z]);
                cell.CellStyle = headerStyle;
            }

            var respondentGroups = listRespondent.GroupBy(x => x.Grade)
                .Select(x => new GetSurveySummaryRespondentResult
                {
                    Level = x.FirstOrDefault().Level,
                    Grade = x.FirstOrDefault().Grade,
                    Total = x.Sum(y => y.Total),
                    TotalRespondent = x.Sum(y => y.TotalRespondent),
                    TotalNotAnswer = x.Sum(y => y.TotalNotAnswer),
                    Percent = Math.Round(x.Sum(y => y.TotalRespondent) / x.Sum(y => y.Total) * 100, 2)
                });

            foreach (var respondentGrouped in respondentGroups)
            {
                j = 8;
                var contentGroupItem = participantSheet.GetRow(++i);

                for (int z = j; z < j + 6; z++)
                {
                    cell = contentGroupItem.CreateCell(z);
                    switch (z)
                    {
                        case 8:
                            cell.SetCellValue(respondentGrouped.Level);
                            break;
                        case 9:
                            cell.SetCellValue(respondentGrouped.Grade);
                            break;
                        case 10:
                            cell.SetCellValue(respondentGrouped.Total);
                            break;
                        case 11:
                            cell.SetCellValue(respondentGrouped.TotalRespondent);
                            break;
                        case 12:
                            cell.SetCellValue(respondentGrouped.TotalNotAnswer);
                            break;
                        case 13:
                            cell.SetCellValue(respondentGrouped.Percent);
                            break;
                        default:
                            break;
                    }
                    cell.CellStyle = contentStyle;
                }
            }



            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                return ms.ToArray();
            }
        }
    }
}
