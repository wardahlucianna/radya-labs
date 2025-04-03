using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.School.FnSurveyNoSql;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveySummary;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnLongRun.SurveySummary;
using FluentEmail.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using ICell = NPOI.SS.UserModel.ICell;

namespace BinusSchool.School.FnLongRun.Services.SurveySummary
{
    public class SurveySummaryGeneral
    {
        public static async Task<List<GetSurveySummaryNoSqlResult>> GetSurveyAnswerRespondent(SurveySummaryGeneralRequest param)
        {
            var getserviceDetailSurveySummaryRespondent = await param.ServiceSurveySummary.DetailSurveySummaryRespondent(new DetailSurveySummaryRespondentRequest
            {
                IdPublishSurvey = param.IdPublishSurvey
            });

            var getSurveySummaryRespondent = getserviceDetailSurveySummaryRespondent.Payload;
            var listIdSurveyChild = getSurveySummaryRespondent.Where(e => e.IdSurveyChild != null).Select(e => e.IdSurveyChild).Distinct().ToList();
            var count = getSurveySummaryRespondent.Count();

            var startIndex = 0;
            List<GetSurveySummaryNoSqlResult> listSurveyAnswer = new List<GetSurveySummaryNoSqlResult>();

            do
            {
                //var length = 500;
                var length = 10;

                var serviceSurveySummaryNoSql = await param.ServiceSurveySummaryNoSql.GetSurveySummaryNoSql(new GetSurveySummaryNoSqlRequest
                {
                    IdPublishSurvey = param.IdPublishSurvey,
                    StartIndex = startIndex,
                    Lenght = length
                });

                var listSurveySummaryNoSql = serviceSurveySummaryNoSql.Payload.Where(e=> listIdSurveyChild.Contains(e.IdSurvey)).ToList();
                listSurveyAnswer.AddRange(listSurveySummaryNoSql);
                startIndex = startIndex+length;
            }
            while(startIndex < count);

            return listSurveyAnswer;
        }

        public static async Task<byte[]> SurveyGeneralExcel(RespondentRequest param, ILogger _logger)
        {
            _logger.LogInformation("[Survey Summary General]-proses Number 1");

            var workbook = new XSSFWorkbook();
            _logger.LogInformation("[Survey Summary General]-proses Number 2");
            #region style
            var fontBold = workbook.CreateFont();
            _logger.LogInformation("[Survey Summary General]-proses Number 3");
            fontBold.IsBold = true;
            _logger.LogInformation("[Survey Summary General]-proses Number 4");
            #region bold with background
            var boldStyleWithBackground = workbook.CreateCellStyle();
            _logger.LogInformation("[Survey Summary General]-proses Number 5");
            boldStyleWithBackground.Alignment = HorizontalAlignment.Center;
            _logger.LogInformation("[Survey Summary General]-proses Number 6");
            boldStyleWithBackground.VerticalAlignment = VerticalAlignment.Center;
            _logger.LogInformation("[Survey Summary General]-proses Number 7");
            boldStyleWithBackground.BorderBottom = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 8");
            boldStyleWithBackground.BorderLeft = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 9");
            boldStyleWithBackground.BorderRight = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 10");
            boldStyleWithBackground.BorderTop = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 11");
            boldStyleWithBackground.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            _logger.LogInformation("[Survey Summary General]-proses Number 12");
            boldStyleWithBackground.FillPattern = NPOI.SS.UserModel.FillPattern.SolidForeground;
            _logger.LogInformation("[Survey Summary General]-proses Number 13");
            boldStyleWithBackground.SetFont(fontBold);
            _logger.LogInformation("[Survey Summary General]-proses Number 14");
            boldStyleWithBackground.WrapText = true;
            _logger.LogInformation("[Survey Summary General]-proses Number 15");
            #endregion

            #region bold only
            var boldStyle = workbook.CreateCellStyle();
            _logger.LogInformation("[Survey Summary General]-proses Number 16");
            boldStyle.Alignment = HorizontalAlignment.Center;
            _logger.LogInformation("[Survey Summary General]-proses Number 17");
            boldStyle.VerticalAlignment = VerticalAlignment.Center;
            _logger.LogInformation("[Survey Summary General]-proses Number 18");
            boldStyle.BorderBottom = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 19");
            boldStyle.BorderLeft = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 20");
            boldStyle.BorderRight = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 21");
            boldStyle.BorderTop = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 22");
            boldStyle.SetFont(fontBold);
            _logger.LogInformation("[Survey Summary General]-proses Number 23");
            boldStyle.WrapText = true;
            _logger.LogInformation("[Survey Summary General]-proses Number 24");
            #endregion

            #region normal
            var fontNormalHeader = workbook.CreateFont();
            _logger.LogInformation("[Survey Summary General]-proses Number 25");
            var normalStyleHeader = workbook.CreateCellStyle();
            _logger.LogInformation("[Survey Summary General]-proses Number 26");
            normalStyleHeader.Alignment = HorizontalAlignment.Center;
            _logger.LogInformation("[Survey Summary General]-proses Number 27");
            normalStyleHeader.VerticalAlignment = VerticalAlignment.Center;
            _logger.LogInformation("[Survey Summary General]-proses Number 28");
            normalStyleHeader.BorderBottom = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 29");
            normalStyleHeader.BorderLeft = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 30");
            normalStyleHeader.BorderRight = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 31");
            normalStyleHeader.BorderTop = BorderStyle.Thin;
            _logger.LogInformation("[Survey Summary General]-proses Number 32");
            normalStyleHeader.SetFont(fontNormalHeader);
            _logger.LogInformation("[Survey Summary General]-proses Number 33");
            boldStyle.WrapText = true;
            _logger.LogInformation("[Survey Summary General]-proses Number 34");
            #endregion
            #endregion
            _logger.LogInformation("[Survey Summary General]-proses Number 35");

            var paramParticipant = new GetSheetParticipantRequest
            {
                NormalStyle = normalStyleHeader,
                BoldStyleWithBackground = boldStyleWithBackground,
                BoldStyle = boldStyle,
                Workbook = workbook,
                IdPublishSurvey = param.IdPublishSurvey,
                CancellationToken = param.CancellationToken,
                DbContext = param.DbContext,
                ServiceSurveySummary = param.ServiceSurveySummary,
                ServiceSurveySummaryNoSql = param.ServiceSurveySummaryNoSql,
                SurveyTemplateNoSql = param.SurveyTemplateNoSql,
                Date = param.Date,
                IdAcademicYear = param.IdAcademicYear
            };
            _logger.LogInformation("[Survey Summary General]-proses Number 36");
            await GetSheetParticipant(paramParticipant, _logger);
            _logger.LogInformation("[Survey Summary General]-proses Number 37");
            await GetSheetSurvey(paramParticipant, _logger);
            _logger.LogInformation("[Survey Summary General]-proses Number 38");

            using var ms = new MemoryStream();
            _logger.LogInformation("[Survey Summary General]-proses Number 39");
            ms.Position = 0;
            _logger.LogInformation("[Survey Summary General]-proses Number 40");
            workbook.Write(ms);
            _logger.LogInformation("[Survey Summary General]-proses Number 41");

            return ms.ToArray();
        }

        private static async Task<string> GetSheetParticipant(GetSheetParticipantRequest data, ILogger _logger)
        {
            _logger.LogInformation("Survey Summary General]-proses Number 42");
            var sheet = data.Workbook.CreateSheet("Participant");

            _logger.LogInformation("Survey Summary General]-proses Number 42A");
            #region get Data
            var getRespondentService = await data.ServiceSurveySummary.GetSurveySummaryRespondent(new GetSurveySummaryRespondentRequest
            {
                Id = data.IdPublishSurvey,
                IdAcademicYear = data.IdAcademicYear
            });
            _logger.LogInformation("Survey Summary General]-proses Number 43");
            var listRespondent = getRespondentService.IsSuccess ? getRespondentService.Payload : null;
            #endregion
            _logger.LogInformation("Survey Summary General]-proses Number 44");
            #region Row 1
            var rowIndex = 0;
            _logger.LogInformation("Survey Summary General]-proses Number 45");
            var rowHeader = sheet.CreateRow(rowIndex);
            _logger.LogInformation("Survey Summary General]-proses Number 46");

            int startColumn = 0;
            _logger.LogInformation("Survey Summary General]-proses Number 47");
            var cellNo = rowHeader.CreateCell(startColumn);
            _logger.LogInformation("Survey Summary General]-proses Number 48");
            var merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 1);
            _logger.LogInformation("Survey Summary General]-proses Number 49");
            cellNo.SetCellValue("Get Date Time");
            _logger.LogInformation("Survey Summary General]-proses Number 50");
            cellNo.CellStyle = data.BoldStyleWithBackground;
            _logger.LogInformation("Survey Summary General]-proses Number 51");
            sheet.AddMergedRegion(merge);
            _logger.LogInformation("Survey Summary General]-proses Number 52");
            startColumn++;
            _logger.LogInformation("Survey Summary General]-proses Number 53");

            for (var i = 0; i < 1; i++)
            {
                _logger.LogInformation("Survey Summary General]-proses Number 54");
                cellNo = rowHeader.GetCell(startColumn);
                _logger.LogInformation("Survey Summary General]-proses Number 55");
                if (cellNo == null)
                    cellNo = rowHeader.CreateCell(startColumn);
                _logger.LogInformation("Survey Summary General]-proses Number 56");
                cellNo.CellStyle = data.BoldStyleWithBackground;
                _logger.LogInformation("Survey Summary General]-proses Number 57");
                startColumn++;
                _logger.LogInformation("Survey Summary General]-proses Number 58");
            }

            cellNo = rowHeader.CreateCell(startColumn);
            _logger.LogInformation("Survey Summary General]-proses Number 59");
            merge = new NPOI.SS.Util.CellRangeAddress(rowIndex, rowIndex, startColumn, startColumn + 2);
            _logger.LogInformation("Survey Summary General]-proses Number 60");
            cellNo.SetCellValue(data.Date.ToString("dd/MM/yyyy HH:mm:ss"));
            _logger.LogInformation("Survey Summary General]-proses Number 61");
            cellNo.CellStyle = data.NormalStyle;
            _logger.LogInformation("Survey Summary General]-proses Number 62");
            sheet.AddMergedRegion(merge);
            _logger.LogInformation("Survey Summary General]-proses Number 63");
            startColumn++;

            for (var i = 0; i < 2; i++)
            {
                _logger.LogInformation("[Survey Summary General]-proses Number 64");
                cellNo = rowHeader.GetCell(startColumn);
                _logger.LogInformation("[Survey Summary General]-proses Number 65");
                if (cellNo == null)
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 66");
                    cellNo = rowHeader.CreateCell(startColumn);
                }
                _logger.LogInformation("[Survey Summary General]-proses Number 67");
                cellNo.CellStyle = data.BoldStyleWithBackground;
                _logger.LogInformation("[Survey Summary General]-proses Number 68");
                startColumn++;
                _logger.LogInformation("[Survey Summary General]-proses Number 69");
            }

            #endregion

            #region Header Table
            rowIndex = 2;
            _logger.LogInformation("[Survey Summary General]-proses Number 70");
            rowHeader = sheet.CreateRow(rowIndex);
            _logger.LogInformation("[Survey Summary General]-proses Number 71");

            List<string> header = new List<string>();
            _logger.LogInformation("[Survey Summary General]-proses Number 72");
            header.Add("Role");
            _logger.LogInformation("[Survey Summary General]-proses Number 73");
            header.Add("Total");
            _logger.LogInformation("[Survey Summary General]-proses Number 74");
            header.Add("Total Respondent");
            _logger.LogInformation("[Survey Summary General]-proses Number 75");
            header.Add("Total Not Answered");
            _logger.LogInformation("[Survey Summary General]-proses Number 76");
            header.Add("%");
            _logger.LogInformation("[Survey Summary General]-proses Number 77");

            startColumn = 0;
            _logger.LogInformation("[Survey Summary General]-proses Number 78");
            foreach (var itemHeader in header)
            {
                _logger.LogInformation("[Survey Summary General]-proses Number 79");
                cellNo = rowHeader.CreateCell(startColumn);
                _logger.LogInformation("[Survey Summary General]-proses Number 80");
                cellNo.SetCellValue(itemHeader);
                _logger.LogInformation("[Survey Summary General]-proses Number 81");
                cellNo.CellStyle = data.BoldStyle;
                _logger.LogInformation("[Survey Summary General]-proses Number 82");
                sheet.AutoSizeColumn(startColumn, true);
                _logger.LogInformation("[Survey Summary General]-proses Number 83");
                startColumn++;
                _logger.LogInformation("[Survey Summary General]-proses Number 84");
            }
            rowIndex++;
            _logger.LogInformation("[Survey Summary General]-proses Number 85");
            #endregion

            #region Body Table
            rowIndex = 3;
            _logger.LogInformation("[Survey Summary General]-proses Number 86");
            for (var i = 0; i < listRespondent.Count(); i++)
            {
                _logger.LogInformation("[Survey Summary General]-proses Number 87");
                rowHeader = sheet.CreateRow(rowIndex);
                _logger.LogInformation("[Survey Summary General]-proses Number 88");
                if (i < listRespondent.Count())
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 89");
                    var itemRespondent = listRespondent[i];
                    _logger.LogInformation("[Survey Summary General]-proses Number 90");
                    startColumn = 0;
                    _logger.LogInformation("[Survey Summary General]-proses Number 91");

                    cellNo = rowHeader.CreateCell(startColumn);
                    _logger.LogInformation("[Survey Summary General]-proses Number 92");
                    cellNo.SetCellValue(itemRespondent.Role);
                    _logger.LogInformation("[Survey Summary General]-proses Number 93");
                    cellNo.CellStyle = data.NormalStyle;
                    _logger.LogInformation("[Survey Summary General]-proses Number 94");
                    sheet.AutoSizeColumn(startColumn, true);
                    _logger.LogInformation("[Survey Summary General]-proses Number 95");
                    startColumn++;
                    _logger.LogInformation("[Survey Summary General]-proses Number 96");

                    cellNo = rowHeader.CreateCell(startColumn);
                    _logger.LogInformation("[Survey Summary General]-proses Number 97");
                    cellNo.SetCellValue(itemRespondent.Total);
                    _logger.LogInformation("[Survey Summary General]-proses Number 98");
                    cellNo.CellStyle = data.NormalStyle;
                    _logger.LogInformation("[Survey Summary General]-proses Number 99");
                    sheet.AutoSizeColumn(startColumn, true);
                    _logger.LogInformation("[Survey Summary General]-proses Number 100");
                    startColumn++;
                    _logger.LogInformation("[Survey Summary General]-proses Number 101");

                    cellNo = rowHeader.CreateCell(startColumn);
                    _logger.LogInformation("[Survey Summary General]-proses Number 102");
                    cellNo.SetCellValue(itemRespondent.TotalRespondent);
                    _logger.LogInformation("[Survey Summary General]-proses Number 103");
                    cellNo.CellStyle = data.NormalStyle;
                    _logger.LogInformation("[Survey Summary General]-proses Number 104");
                    sheet.AutoSizeColumn(startColumn, true);
                    _logger.LogInformation("[Survey Summary General]-proses Number 105");
                    startColumn++;
                    _logger.LogInformation("[Survey Summary General]-proses Number 106");

                    cellNo = rowHeader.CreateCell(startColumn);
                    _logger.LogInformation("[Survey Summary General]-proses Number 107");
                    cellNo.SetCellValue(itemRespondent.TotalNotAnswer);
                    _logger.LogInformation("[Survey Summary General]-proses Number 108");
                    cellNo.CellStyle = data.NormalStyle;
                    _logger.LogInformation("[Survey Summary General]-proses Number 109");
                    sheet.AutoSizeColumn(startColumn, true);
                    _logger.LogInformation("[Survey Summary General]-proses Number 110");
                    startColumn++;
                    _logger.LogInformation("[Survey Summary General]-proses Number 111");

                    cellNo = rowHeader.CreateCell(startColumn);
                    _logger.LogInformation("[Survey Summary General]-proses Number 123");
                    cellNo.SetCellValue(itemRespondent.Percent);
                    _logger.LogInformation("[Survey Summary General]-proses Number 124");
                    cellNo.CellStyle = data.NormalStyle;
                    _logger.LogInformation("[Survey Summary General]-proses Number 125");
                    sheet.AutoSizeColumn(startColumn, true);
                    _logger.LogInformation("[Survey Summary General]-proses Number 126");
                    startColumn++;
                    _logger.LogInformation("[Survey Summary General]-proses Number 127");
                }
                _logger.LogInformation("[Survey Summary General]-proses Number 128");
                rowIndex++;
                _logger.LogInformation("[Survey Summary General]-proses Number 129");
            }
            #endregion
            _logger.LogInformation("[Survey Summary General]-proses Number 130");
            return default;
        }

        private static async Task<string> GetSheetSurvey(GetSheetParticipantRequest data, ILogger _logger)
        {
            _logger.LogInformation("[Survey Summary General]-proses Number 131");
            var sheet = data.Workbook.CreateSheet("Survey");
            _logger.LogInformation("[Survey Summary General]-proses Number 132");
            #region get Data
            var IdSurveyTemplateChild = await data.DbContext.Entity<TrPublishSurvey>()
                                            .Where(e => e.Id == data.IdPublishSurvey)
                                            .Select(e => e.IdSurveyTemplateChild)
                                            .FirstOrDefaultAsync(data.CancellationToken);

            _logger.LogInformation("[Survey Summary General]-proses Number 133");

            var getDetailSurveyService = await data.SurveyTemplateNoSql.DetailSurveyTemplateNoSql(IdSurveyTemplateChild);
            _logger.LogInformation("[Survey Summary General]-proses Number 134");
            var getSurveyTemplate = getDetailSurveyService.IsSuccess? getDetailSurveyService.Payload: null;
            _logger.LogInformation("[Survey Summary General]-proses Number 135");
            var listSurvey = await GetSurveyAnswerRespondent(new SurveySummaryGeneralRequest
            {
                IdPublishSurvey = data.IdPublishSurvey,
                ServiceSurveySummary = data.ServiceSurveySummary,
                ServiceSurveySummaryNoSql = data.ServiceSurveySummaryNoSql
            });
            _logger.LogInformation("[Survey Summary General]-proses Number 136");
            #endregion

            #region Header Table
            List<string> header = new List<string>(){"Role"};
            _logger.LogInformation("[Survey Summary General]-proses Number 137");
            var question = getSurveyTemplate.Sections.SelectMany(e => e.Questions).ToList();
            _logger.LogInformation("[Survey Summary General]-proses Number 138");
            var rowIndex = 0;
            _logger.LogInformation("[Survey Summary General]-proses Number 139");
            var rowHeader = sheet.CreateRow(rowIndex);
            _logger.LogInformation("[Survey Summary General]-proses Number 140");

            foreach (var itemQuestion in question)
            {
                _logger.LogInformation("[Survey Summary General]-proses Number 141");
                if (itemQuestion.Type == "Likert")
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 142");
                    var listQuestionLikert = itemQuestion.LikertValues.SelectMany(e => e.RowStatements).ToList();
                    _logger.LogInformation("[Survey Summary General]-proses Number 143");
                    foreach (var itemLikert in listQuestionLikert)
                    {
                        _logger.LogInformation("[Survey Summary General]-proses Number 144");
                        header.Add($"{itemQuestion.QuestionText} ({itemLikert.Value})");
                        _logger.LogInformation("[Survey Summary General]-proses Number 145");
                    }
                }
                else
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 146");
                    header.Add(itemQuestion.QuestionText);
                }
                _logger.LogInformation("[Survey Summary General]-proses Number 147");
            }
            _logger.LogInformation("[Survey Summary General]-proses Number 148");

            var startColumn = 0;
            _logger.LogInformation("[Survey Summary General]-proses Number 159");
            ICell cellNo = default;
            _logger.LogInformation("[Survey Summary General]-proses Number 160");
            foreach (var itemHeader in header)
            {
                _logger.LogInformation("[Survey Summary General]-proses Number 161");
                cellNo = rowHeader.CreateCell(startColumn);
                _logger.LogInformation("[Survey Summary General]-proses Number 162");
                cellNo.SetCellValue(itemHeader);
                _logger.LogInformation("[Survey Summary General]-proses Number 163");
                cellNo.CellStyle = data.BoldStyle;
                _logger.LogInformation("[Survey Summary General]-proses Number 164");
                sheet.SetColumnWidth(startColumn, 10000);
                _logger.LogInformation("[Survey Summary General]-proses Number 165");
                startColumn++;
                _logger.LogInformation("[Survey Summary General]-proses Number 166");
            }
            _logger.LogInformation("[Survey Summary General]-proses Number 167");
            rowIndex++;
            _logger.LogInformation("[Survey Summary General]-proses Number 168");
            #endregion

            #region Body Table
            rowIndex = 1;
            _logger.LogInformation("[Survey Summary General]-proses Number 169");
            var countHeader = header.Count();
            _logger.LogInformation("[Survey Summary General]-proses Number 170");
            for (var i = 0; i < listSurvey.Count(); i++)
            {
                _logger.LogInformation("[Survey Summary General]-proses Number 171");
                var getRow = sheet.GetRow(rowIndex);
                _logger.LogInformation("[Survey Summary General]-proses Number 172");
                if (getRow == null)
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 173");
                    rowHeader = sheet.CreateRow(rowIndex);
                    _logger.LogInformation("[Survey Summary General]-proses Number 174");
                }
               
                if (i > countHeader)
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 175");
                    continue;
                    _logger.LogInformation("[Survey Summary General]-proses Number 176");
                }

                var itemSurvey = listSurvey[i];
                _logger.LogInformation("[Survey Summary General]-proses Number 177");
                var listQuestion = itemSurvey.Sections.SelectMany(e => e.Questions).ToList();
                _logger.LogInformation("[Survey Summary General]-proses Number 178");
                startColumn = 0;
                _logger.LogInformation("[Survey Summary General]-proses Number 179");

                cellNo = rowHeader.CreateCell(startColumn);
                _logger.LogInformation("[Survey Summary General]-proses Number 180");
                cellNo.SetCellValue(itemSurvey.Role);
                _logger.LogInformation("[Survey Summary General]-proses Number 181");
                cellNo.CellStyle = data.NormalStyle;
                _logger.LogInformation("[Survey Summary General]-proses Number 182");
                cellNo.CellStyle.WrapText = true;
                _logger.LogInformation("[Survey Summary General]-proses Number 183");
                startColumn++;
                _logger.LogInformation("[Survey Summary General]-proses Number 184");

                foreach (var itemQuestion in listQuestion)
                {
                    _logger.LogInformation("[Survey Summary General]-proses Number 184");
                    if (itemQuestion.Type != "UploadFile" && itemQuestion.Type != "Likert")
                    {
                        _logger.LogInformation("[Survey Summary General]-proses Number 185");
                        var listAnswer = itemQuestion.RespondentAnswers.Select(e => e.Value).ToList();
                        _logger.LogInformation("[Survey Summary General]-proses Number 186");
                        var answer = string.Join("; ", listAnswer);
                        _logger.LogInformation("[Survey Summary General]-proses Number 187");

                        cellNo = rowHeader.CreateCell(startColumn);
                        _logger.LogInformation("[Survey Summary General]-proses Number 188");
                        cellNo.SetCellValue(answer);
                        _logger.LogInformation("[Survey Summary General]-proses Number 189");
                        cellNo.CellStyle = data.NormalStyle;
                        _logger.LogInformation("[Survey Summary General]-proses Number 190");
                        cellNo.CellStyle.WrapText = true;
                        _logger.LogInformation("[Survey Summary General]-proses Number 191");
                        startColumn++;
                        _logger.LogInformation("[Survey Summary General]-proses Number 192");
                    }
                    else if (itemQuestion.Type == "UploadFile")
                    {
                        _logger.LogInformation("[Survey Summary General]-proses Number 193");
                        var listAnswer = itemQuestion.RespondentAnswers.Select(e => e.FileValueAnswer.Url).ToList();
                        _logger.LogInformation("[Survey Summary General]-proses Number 194");
                        var answer = string.Join("; ", listAnswer);
                        _logger.LogInformation("[Survey Summary General]-proses Number 195");

                        cellNo = rowHeader.CreateCell(startColumn);
                        _logger.LogInformation("[Survey Summary General]-proses Number 196");
                        cellNo.SetCellValue(answer);
                        _logger.LogInformation("[Survey Summary General]-proses Number 197");

                        cellNo.CellStyle = data.NormalStyle;
                        _logger.LogInformation("[Survey Summary General]-proses Number 198");
                        cellNo.CellStyle.WrapText = true;
                        _logger.LogInformation("[Survey Summary General]-proses Number 199");
                        startColumn++;
                        _logger.LogInformation("[Survey Summary General]-proses Number 200");
                    }
                    else
                    {
                        _logger.LogInformation("[Survey Summary General]-proses Number 201");
                        var listAnswer = itemQuestion.RespondentAnswers.Select(e => e.LikertValueAnswer).ToList();
                        _logger.LogInformation("[Survey Summary General]-proses Number 202");

                        foreach (var itemAnswer in listAnswer)
                        {
                            _logger.LogInformation("[Survey Summary General]-proses Number 203");
                            cellNo = rowHeader.CreateCell(startColumn);
                            _logger.LogInformation("[Survey Summary General]-proses Number 204");
                            cellNo.SetCellValue(itemAnswer.Value);
                            _logger.LogInformation("[Survey Summary General]-proses Number 205");
                            cellNo.CellStyle = data.NormalStyle;
                            _logger.LogInformation("[Survey Summary General]-proses Number 206");
                            cellNo.CellStyle.WrapText = true;
                            _logger.LogInformation("[Survey Summary General]-proses Number 207");
                            startColumn++;
                            _logger.LogInformation("[Survey Summary General]-proses Number 208");
                        }
                        _logger.LogInformation("[Survey Summary General]-proses Number 209");
                    }
                    _logger.LogInformation("[Survey Summary General]-proses Number 210");

                }
                _logger.LogInformation("[Survey Summary General]-proses Number 211");
                rowIndex++;
                _logger.LogInformation("[Survey Summary General]-proses Number 212");
            }
            #endregion
            _logger.LogInformation("[Survey Summary General]-proses Number 213");
            return default;
        }
    }

    public class DefaultRequest
    {
        public CancellationToken CancellationToken { get; set; }
        public ISurveySummary ServiceSurveySummary {  get; set; }
        public ISurveySummaryNoSql ServiceSurveySummaryNoSql {  get; set; }
        public ISurveyTemplateNoSql SurveyTemplateNoSql {  get; set; }
        public ISchoolDbContext DbContext { get; set; }
    }

    public class SurveySummaryGeneralRequest : DefaultRequest
    {
        public string IdPublishSurvey { get; set; }
    }

    public class GetSheetParticipantRequest : DefaultRequest
    {
        public string IdPublishSurvey { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime Date { get; set; }
        public ICellStyle NormalStyle { get; set; }
        public ICellStyle BoldStyleWithBackground { get; set; }
        public ICellStyle BoldStyle { get; set; }
        public XSSFWorkbook Workbook {  get; set; }
    }
}
