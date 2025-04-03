using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveySummary;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveyTemplate;
using BinusSchool.Data.Model.User.FnUser.User;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.School.FnLongRun.Services.SurveySummary;
using BinusSchool.School.FnLongRun.SurveySummary;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Azure;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Asn1.X509.Qualified;

namespace BinusSchool.School.FnLongRun.Services.SurveySummary
{
    public class SurveySummaryStudentParent
    {
        public static async Task<byte[]> GenerateExcel(RespondentRequest param)
        {
            var workbook = new XSSFWorkbook();
            var boldFont = workbook.CreateFont();
            boldFont.IsBold = true;
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.BorderBottom = BorderStyle.Thin;
            headerStyle.BorderTop = BorderStyle.Thin;
            headerStyle.BorderRight = BorderStyle.Thin;
            headerStyle.BorderLeft = BorderStyle.Thin;

            var contentStyle = workbook.CreateCellStyle();
            contentStyle.CloneStyleFrom(headerStyle);

            headerStyle.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.Yellow.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            headerStyle.SetFont(boldFont);
            headerStyle.VerticalAlignment = VerticalAlignment.Center;

            var paramParticipant = new GetSheetStudentParticipantRequest
            {
                IdPublishSurvey = param.IdPublishSurvey,
                IdAcademicYear = param.IdAcademicYear,
                Date = param.Date,
                HeaderStyle = headerStyle,
                ContentStyle = contentStyle,
                Workbook = workbook,
                ServiceSurveySummary = param.ServiceSurveySummary
            };

            await GetSheetParticipantAsync(paramParticipant);

            var paramAnswer = new GetSheetStudentParticipantRequest
            {
                IdPublishSurvey = param.IdPublishSurvey,
                Date = param.Date,
                HeaderStyle = headerStyle,
                ContentStyle = contentStyle,
                Workbook = workbook,
                ServiceSurveySummary = param.ServiceSurveySummary,
                ServiceSurveySummaryNoSql = param.ServiceSurveySummaryNoSql,
                SurveyTemplateNoSql = param.SurveyTemplateNoSql,
                DbContext = param.DbContext
            };

            await GetSheetAnswerAsync(paramAnswer);

            using (var ms = new MemoryStream())
            {
                workbook.Write(ms);
                return ms.ToArray();
            }
        }

        public static async Task<string> GetSheetParticipantAsync(GetSheetStudentParticipantRequest request)
        {
            var getRespondentService = await request.ServiceSurveySummary.GetSurveySummaryRespondent(new GetSurveySummaryRespondentRequest
            {
                Id = request.IdPublishSurvey,
                IdAcademicYear = request.IdAcademicYear,
                Return = Common.Model.Enums.CollectionType.Lov,
                GetAll = true
            });
            if (!getRespondentService.IsSuccess)
                throw new BadRequestException($"Error : {getRespondentService.Errors}");

            var listRespondentAll = getRespondentService.Payload;
            var listRespondent = listRespondentAll.OrderBy(x => x.Grade).ThenBy(x => x.Homeroom);

            var participantSheet = request.Workbook.CreateSheet("Participant");
            // i = rowNumber, j = cellNumber
            int i = 0, j = 0;
            var rowHeader = participantSheet.CreateRow(i);
            CellRangeAddress cra = new CellRangeAddress(i, i, j, j + 3);
            var cell = rowHeader.CreateCell(j);
            cell.CellStyle = request.HeaderStyle;
            cell.SetCellValue("Get Data Time");
            participantSheet.AddMergedRegion(cra);

            j = 4;
            cra = new CellRangeAddress(i, i, j, j + 2);
            cell = rowHeader.CreateCell(j);
            cell.SetCellValue(request.Date.Date.ToString("dd/MM/yyyy HH:mm:ss"));
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
                cell.CellStyle = request.HeaderStyle;
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
                    cell.CellStyle = request.ContentStyle;
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
                cell.CellStyle = request.HeaderStyle;
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
                    cell.CellStyle = request.ContentStyle;
                }
            }

            return default;
        }

        public static async Task<string> GetSheetAnswerAsync(GetSheetStudentParticipantRequest request)
        {
            var idSurveyTemplateChild = await request.DbContext.Entity<TrPublishSurvey>()
                .Where(x => x.Id == request.IdPublishSurvey)
                .Select(x => x.IdSurveyTemplateChild).FirstOrDefaultAsync();
            if (idSurveyTemplateChild == null)
                throw new NotFoundException("ID Survey Template Child not found");

            var getDetailSurveyService = await request.SurveyTemplateNoSql.DetailSurveyTemplateNoSql(idSurveyTemplateChild);
            if (!getDetailSurveyService.IsSuccess)
                throw new BadRequestException($"Error : {getDetailSurveyService.Errors}");
            var getSurveyTemplate = getDetailSurveyService.Payload;

            var listSurvey = await SurveySummaryGeneral.GetSurveyAnswerRespondent(new SurveySummaryGeneralRequest
            {
                IdPublishSurvey = request.IdPublishSurvey,
                ServiceSurveySummary = request.ServiceSurveySummary,
                ServiceSurveySummaryNoSql = request.ServiceSurveySummaryNoSql
            });

            var sections = getSurveyTemplate.Sections.ToList();
            int questionNumber = 0;
            int sectionIndex = 0;

            foreach (var section in sections)
            {
                var questionsTemplate = section.Questions.ToList();
                int questionIndex = 0;
                foreach (var question in questionsTemplate)
                {
                    var sheet = request.Workbook.CreateSheet($"Question {++questionNumber}");
                    CreateHeaderQuestion(sheet, question.Type.ToLower(), request.HeaderStyle, question.TypeValues, question.LikertValues, question.QuestionText);
                    switch (question.Type.ToLower())
                    {
                        case "text":
                        case "upload file":
                        case "date":
                            CreateTextContentAnswer(sheet, question.Type.ToLower(), request.ContentStyle, question.QuestionText, question.TypeValues,
                                listSurvey.OrderBy(x => x.Homeroom).ToList(), sectionIndex, questionIndex);
                            break;
                        case "radio button":
                        case "checkboxes":
                        case "dropdown":
                            CreateRadioButtonContentAnswer(sheet, question.Type.ToLower(), request.ContentStyle, question.QuestionText, question.TypeValues,
                                listSurvey.OrderBy(x => x.Homeroom).ToList(), sectionIndex, questionIndex, questionNumber);
                            break;
                        case "likert":
                            CreateLikertContentAnswer(sheet, question.Type.ToLower(), request.ContentStyle, request.HeaderStyle, question.QuestionText, question.LikertValues,
                                listSurvey.OrderBy(x => x.Homeroom).ToList(), sectionIndex, questionIndex, questionNumber);
                            break;
                        default:
                            break;
                    }
                    questionIndex++;
                }

                sectionIndex++;
            }

            return default;
        }

        private static void CreateLikertContentAnswer(ISheet sheet, string type, ICellStyle contentStyle,
            ICellStyle headerStyle, string questionText,
            List<LikertValue> likertValues,
            List<GetSurveySummaryNoSqlResult> listSurvey, int sectionIndex, int questionIndex, int questionNumber)
        {
            int i = 3;
            int j = 0;

            var homerooms = listSurvey.OrderBy(x => x.Homeroom)
                    .GroupBy(x => new
                    {
                        x.Level,
                        x.Grade,
                        x.Homeroom
                    })
                    .ToDictionary(g => new LevelGradeByHomeroomModel
                    {
                        Level = g.Key.Level,
                        Grade = g.Key.Grade,
                        Homeroom = g.Key.Homeroom
                    }, g => g.SelectMany(x => x.Sections).ToList());

            if (homerooms == null)
                return;

            Dictionary<string, List<StatementAverageModel>> summaryByGrade = new Dictionary<string, List<StatementAverageModel>>();
            Dictionary<string, List<StatementAverageModel>> summaryByLevel = new Dictionary<string, List<StatementAverageModel>>();
            var statements = likertValues.Select(x => x.RowStatements).FirstOrDefault();

            foreach (var homeroom in homerooms)
            {
                foreach (var statement in statements)
                {
                    j = 0;
                    var answers = homeroom.Value.SelectMany(x => x.Questions)
                            .Where(x => x.Number == questionNumber).SelectMany(x => x.RespondentAnswers)
                            .Where(x => x.Number == statement.Number).ToList();
                    string level = homeroom.Key.Level;
                    string grade = homeroom.Key.Grade;

                    var contentRow = sheet.CreateRow(i++);
                    var contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(level);
                    contentCell.CellStyle = contentStyle;

                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(grade);
                    contentCell.CellStyle = contentStyle;

                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(homeroom.Key.Homeroom);
                    contentCell.CellStyle = contentStyle;

                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(statement.Value);
                    contentCell.CellStyle = contentStyle;

                    List<int> allValue = new List<int>();
                    var options = likertValues.Select(x => x.ColumnOptions).FirstOrDefault();
                    foreach (var option in options)
                    {
                        int answerCount = 0;
                        answerCount = answers.Count(x => x.LikertValueAnswer.Number == option.Number);

                        contentCell = contentRow.CreateCell(j++);
                        contentCell.SetCellValue(answerCount);
                        contentCell.CellStyle = contentStyle;
                        allValue.Add(answerCount);
                    }

                    contentCell = contentRow.CreateCell(j++);
                    double avg = Math.Round(Convert.ToDouble(allValue.Sum()) / Convert.ToDouble(options.Count()), 2);
                    contentCell.SetCellValue(avg);
                    contentCell.CellStyle = contentStyle;

                    #region Summary by grade
                    if (!summaryByGrade.TryGetValue(grade, out var statementAvgGrade))
                    {
                        summaryByGrade.Add(grade, new List<StatementAverageModel>
                        {
                            new StatementAverageModel()
                            {
                                Number = statement.Number,
                                Statement = statement.Value,
                                Avg = avg
                            }
                        });
                    }
                    else
                    {
                        statementAvgGrade.Add(new StatementAverageModel
                        {
                            Number = statement.Number,
                            Statement = statement.Value,
                            Avg = avg
                        });
                    }
                    #endregion

                    #region Summary by level
                    if (!summaryByLevel.TryGetValue(level, out var statementAvgLevel))
                    {
                        summaryByLevel.Add(level, new List<StatementAverageModel>
                        {
                            new StatementAverageModel()
                            {
                                Number = statement.Number,
                                Statement = statement.Value,
                                Avg = avg
                            }
                        });
                    }
                    else
                    {
                        statementAvgLevel.Add(new StatementAverageModel
                        {
                            Number = statement.Number,
                            Statement = statement.Value,
                            Avg = avg
                        });
                    }
                    #endregion
                }
            }

            #region Add Summary By Grade and Level data
            i = 3;
            j = 6 + likertValues.FirstOrDefault().ColumnOptions.Count;
            foreach (var itemGrade in summaryByGrade)
            {
                var contentRow = sheet.GetRow(i++);
                if (contentRow == null)
                    contentRow = sheet.CreateRow(i);

                var contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(itemGrade.Key);
                contentCell.CellStyle = contentStyle;

                int countHomeroomByGrade = homerooms.Where(x => x.Key.Grade == itemGrade.Key).Select(x => x.Key.Homeroom).Count();
                double avgPerGrade = 0;
                foreach (var statement in statements)
                {
                    var avgStatementPerGrade = Math.Round(itemGrade.Value.Where(x => x.Number == statement.Number).Sum(x => x.Avg) / 
                        countHomeroomByGrade, 2);
                    avgPerGrade += avgStatementPerGrade;

                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(avgStatementPerGrade);
                    contentCell.CellStyle = contentStyle;
                }

                contentCell = contentRow.CreateCell(j++);
                double statementAvgPerGrade = Math.Round(avgPerGrade / itemGrade.Value.Count, 2);
                contentCell.SetCellValue(statementAvgPerGrade);
                contentCell.CellStyle = contentStyle;

                j = 6 + likertValues.FirstOrDefault().ColumnOptions.Count;
            }

            i++; // skip 1 row
            j = 6 + likertValues.FirstOrDefault().ColumnOptions.Count;
            Dictionary<string, List<StatementAverageModel>> avgStatementByLevel = new Dictionary<string, List<StatementAverageModel>>(); 
            foreach (var itemLevel in summaryByLevel)
            {
                int tempIteration = i;
                var contentRow = sheet.GetRow(tempIteration++);
                if (contentRow == null)
                    contentRow = sheet.CreateRow(i);

                i++;
                var contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(itemLevel.Key);
                contentCell.CellStyle = headerStyle;

                int countGradeByLevel = homerooms.Where(x => x.Key.Level == itemLevel.Key).Select(x => x.Key.Grade).Count();
                double avgPerLevel = 0;
                foreach (var statement in statements)
                {
                    var avgStatementPerLevel = Math.Round(itemLevel.Value.Where(x => x.Number == statement.Number).Sum(x => x.Avg) /
                        countGradeByLevel, 2);
                    avgPerLevel += avgStatementPerLevel;

                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(avgStatementPerLevel);
                    contentCell.CellStyle = contentStyle;

                    List<StatementAverageModel> statementAvgByLevel = new List<StatementAverageModel>();
                    if (!avgStatementByLevel.TryGetValue(itemLevel.Key, out statementAvgByLevel))
                    {
                        avgStatementByLevel.Add(itemLevel.Key, new List<StatementAverageModel>()
                        {
                            new StatementAverageModel
                            {
                                Number = statement.Number,
                                Statement = statement.Value,
                                Avg = avgStatementPerLevel
                            }
                        });
                    }
                    else
                    {
                        statementAvgByLevel.Add(new StatementAverageModel
                        {
                            Number = statement.Number,
                            Statement = statement.Value,
                            Avg = avgStatementPerLevel
                        });
                        avgStatementByLevel[itemLevel.Key] = statementAvgByLevel;
                    }
                }

                contentCell = contentRow.CreateCell(j++);
                double statementAvgPerLevel = Math.Round(avgPerLevel / itemLevel.Value.Count, 2);
                contentCell.SetCellValue(statementAvgPerLevel);
                contentCell.CellStyle = contentStyle;

                j = 6 + likertValues.FirstOrDefault().ColumnOptions.Count;
            }
            #endregion

            #region Add merged level data if exist
            i++;
            var mergedLevels = new Dictionary<int, List<string>>
            {
                {
                    1, new List<string>
                    {
                        "Middle School", "High School"
                    }
                    },
                {
                    2,
                    new List<string>
                    {
                        "Early Childhood Years", "Elementary"
                    }
                },
                {
                    3, new List<string>
                    {
                        "MS", "HS"
                    }
                    },
                {
                    4,
                    new List<string>
                    {
                        "ECY", "EL"
                    }
                }
            };
            foreach (var mergedLevel in mergedLevels)
            {
                if (avgStatementByLevel.Any(x => x.Key == mergedLevel.Value[0]) && avgStatementByLevel.Any(x => x.Key == mergedLevel.Value[1]))
                {
                    j = 6 + likertValues.FirstOrDefault().ColumnOptions.Count;
                    int tempIteration = i;
                    var contentRow = sheet.GetRow(tempIteration++);
                    if (contentRow == null)
                        contentRow = sheet.CreateRow(i);

                    i++;
                    var contentCell = contentRow.CreateCell(j++);
                    if (mergedLevel.Key == 1 || mergedLevel.Key == 3)
                    {
                        contentCell.SetCellValue("MSHS");
                    }
                    else
                    {
                        contentCell.SetCellValue("ECYEL");
                    }
                    contentCell.CellStyle = headerStyle;

                    double totalAvg = 0;
                    foreach (var statement in statements)
                    {
                        double avgPerStatement = 0;
                        foreach (var level in mergedLevel.Value)
                        {
                            var selectedValue = avgStatementByLevel.Where(x => x.Key == level).Select(x => x.Value).FirstOrDefault();
                            var selectedAvg = selectedValue.Where(x => x.Number == statement.Number).Select(x => x.Avg).FirstOrDefault();
                            avgPerStatement += selectedAvg;
                        }
                        totalAvg += avgPerStatement;
                        contentCell = contentRow.CreateCell(j++);
                        contentCell.SetCellValue(avgPerStatement);
                        contentCell.CellStyle = contentStyle;
                    }

                    double totalAvgPerLevel = Math.Round(Convert.ToDouble(totalAvg) / statements.Count, 2);
                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(totalAvgPerLevel);
                    contentCell.CellStyle = contentStyle;

                    j = 6 + likertValues.FirstOrDefault().ColumnOptions.Count;
                }
            }
            #endregion
        }

        private static void CreateRadioButtonContentAnswer(ISheet sheet, string type, ICellStyle contentStyle, 
            string questionText,
             List<TypeValue> typeValues,
            List<GetSurveySummaryNoSqlResult> listSurvey, int sectionIndex, int questionIndex, int questionNumber)
        {
            int i = 3;
            int j = 0;

            var homerooms = listSurvey.OrderBy(x => x.Homeroom)
                    .GroupBy(x => new
                    {
                        x.Level,
                        x.Grade,
                        x.Homeroom
                    })
                    .ToDictionary(g => new LevelGradeByHomeroomModel
                    {
                        Level = g.Key.Level,
                        Grade = g.Key.Grade,
                        Homeroom = g.Key.Homeroom
                    }, g => g.SelectMany(x => x.Sections).ToList());

            if (homerooms == null)
                return;
            Dictionary<string, List<LevelGradeByHomeroomModel>> levelGradeByHomerooms = new Dictionary<string, List<LevelGradeByHomeroomModel>>();
            Dictionary<string, List<HomeroomAverageModel>> summaryByLevel = new Dictionary<string, List<HomeroomAverageModel>>();

            foreach (var homeroom in homerooms)
            {
                j = 0;

                string level = homeroom.Key.Level;
                string grade = homeroom.Key.Grade;
                var contentRow = sheet.CreateRow(i++);
                var contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(level);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(grade);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(homeroom.Key.Homeroom);
                contentCell.CellStyle = contentStyle;

                List<int> allValue = new List<int>();
                foreach (var typeValue in typeValues)
                {
                    var answerCount = 0;
                    //var answers = homeroom.Value.SelectMany(x => x.Questions)
                    //        .Where(x => x.QuestionText == questionText).SelectMany(x => x.RespondentAnswers);
                    var answers = homeroom.Value.SelectMany(x => x.Questions).Where(x => x.Number == questionNumber).SelectMany(x => x.RespondentAnswers);
                    answerCount = answers.Count(x => x.Number == typeValue.Number);

                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(answerCount);
                    contentCell.CellStyle = contentStyle;
                    allValue.Add(answerCount);
                }

                contentCell = contentRow.CreateCell(j++);
                double avg = Math.Round(Convert.ToDouble(allValue.Sum()) / Convert.ToDouble(allValue.Count()), 2);
                contentCell.SetCellValue(avg);
                contentCell.CellStyle = contentStyle;

                if (!summaryByLevel.TryGetValue(level, out var homeroomAvgModel))
                {
                    homeroomAvgModel = new List<HomeroomAverageModel>
                    {
                        new HomeroomAverageModel
                        {
                            Homeroom = homeroom.Key.Homeroom,
                            Avg = avg
                        }
                    };
                    summaryByLevel.Add(level, homeroomAvgModel);
                }
                else
                {
                    homeroomAvgModel.Add(new HomeroomAverageModel
                    {
                        Homeroom = homeroom.Key.Homeroom,
                        Avg = avg
                    });
                }
            }

            #region Add summary by level data
            i = 3;
            j = 5 + typeValues.Count;
            foreach (var item in summaryByLevel)
            {
                int countHomeroom = listSurvey.Where(x => x.Level == item.Key).Select(x => x.Homeroom).Distinct().Count();
                var contentRow = sheet.GetRow(i++);
                if (contentRow == null)
                {
                    contentRow = sheet.CreateRow(i);
                }
                var contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(item.Key);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                var avgByLevel = Math.Round(item.Value.Sum(x => x.Avg) / countHomeroom, 2);
                contentCell.SetCellValue(avgByLevel);
                contentCell.CellStyle = contentStyle;

                j = 5 + typeValues.Count;
            }
            #endregion
        }

        private static void CreateTextContentAnswer(ISheet sheet, string type, ICellStyle contentStyle,
            string questionText,
            List<TypeValue> typeValues,
            List<GetSurveySummaryNoSqlResult> listSurvey, int sectionIndex, int questionIndex)
        {
            int i = 3;
            int j = 0;

            foreach (var survey in listSurvey)
            {
                //var sections = survey.Sections.Where(x => x.Questions.Any(y => y.QuestionText == questionText))
                //    .Select(x => x.Questions).FirstOrDefault();
                //if (sections == null || !sections.Any())
                //    continue;
                //var answer = sections.Where(x => x.QuestionText == questionText).Select(x => x.RespondentAnswers).FirstOrDefault();
                //if (answer == null || !answer.Any())
                //    continue;
                var answer = survey.Sections[sectionIndex].Questions[questionIndex].RespondentAnswers.FirstOrDefault();
                if (answer == null)
                    continue;
                j = 0;
                var contentRow = sheet.CreateRow(i++);
                var contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(survey.Level);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(survey.Grade);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(survey.Homeroom);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.CellStyle = contentStyle;
                if (type == "text" || type == "date")
                {
                    contentCell.SetCellValue(answer.Value);
                }
                else
                {
                    contentCell.SetCellValue(answer.FileValueAnswer.Url);
                }
            }
        }

        private static void CreateHeaderQuestion(ISheet sheet, string type, ICellStyle headerStyle,
            List<TypeValue> typeValues,
            List<LikertValue> likertValues, string questionsText)
        {
            int i = 0;
            int j = 0;

            var headerRow = sheet.CreateRow(i);
            var headerCell = headerRow.CreateCell(j);
            headerCell.SetCellValue(questionsText);

            i = 2;
            headerRow = sheet.CreateRow(i);
            switch (type)
            {
                case "text":
                case "upload file":
                case "date":
                    for (int z = 0; z < 4; z++)
                    {
                        headerCell = headerRow.CreateCell(z);
                        headerCell.CellStyle = headerStyle;
                        switch (z)
                        {
                            case 0:
                                headerCell.SetCellValue("Level");
                                break;
                            case 1:
                                headerCell.SetCellValue("Grade");
                                break;
                            case 2:
                                headerCell.SetCellValue("Homeroom");
                                break;
                            case 3:
                                if (type == "text")
                                {
                                    headerCell.SetCellValue("Answer");
                                }
                                else if (type == "date")
                                {
                                    headerCell.SetCellValue("Date");
                                }
                                else
                                {
                                    headerCell.SetCellValue("Link File");
                                }
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case "radio button":
                case "checkboxes":
                case "dropdown":
                    headerCell = headerRow.CreateCell(j);
                    headerCell.SetCellValue("Level");
                    headerCell.CellStyle = headerStyle;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Grade");
                    headerCell.CellStyle = headerStyle;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Homeroom");
                    headerCell.CellStyle = headerStyle;

                    foreach (var typevalue in typeValues)
                    {
                        headerCell = headerRow.CreateCell(++j);
                        if (type == "radiobutton")
                        {
                            headerCell.SetCellValue($"{typevalue.Value} (option)");
                        }
                        else
                        {
                            headerCell.SetCellValue($"{typevalue.Value}");
                        }
                        headerCell.CellStyle = headerStyle;
                    }

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Average");
                    headerCell.CellStyle = headerStyle;

                    // skip 1 cell
                    j++;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Level");
                    headerCell.CellStyle = headerStyle;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Average");
                    headerCell.CellStyle = headerStyle;
                    break;
                case "likert":
                    headerCell = headerRow.CreateCell(j);
                    headerCell.SetCellValue("Level");
                    headerCell.CellStyle = headerStyle;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Grade");
                    headerCell.CellStyle = headerStyle;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Homeroom");
                    headerCell.CellStyle = headerStyle;

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Statement");
                    headerCell.CellStyle = headerStyle;

                    foreach (var columnOption in likertValues.First().ColumnOptions)
                    {
                        headerCell = headerRow.CreateCell(++j);
                        headerCell.SetCellValue(columnOption.Value);
                        headerCell.CellStyle = headerStyle;
                    }

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Average");
                    headerCell.CellStyle = headerStyle;
                    // Skip 1 cell
                    j++;
                    // Total Per-Statement
                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Grade");
                    headerCell.CellStyle = headerStyle;

                    foreach (var statement in likertValues.FirstOrDefault().RowStatements)
                    {
                        headerCell = headerRow.CreateCell(++j);
                        headerCell.SetCellValue(statement.Value);
                        headerCell.CellStyle = headerStyle;
                    }

                    headerCell = headerRow.CreateCell(++j);
                    headerCell.SetCellValue("Total Score");
                    headerCell.CellStyle = headerStyle;
                    // total per level in another function because its dynamic row
                    break;
                default:
                    break;
            }
        }
    }

    public class GetSheetStudentParticipantRequest : DefaultRequest
    {
        public string IdPublishSurvey { get; set; }
        public string IdAcademicYear { get; set; }
        public DateTime Date { get; set; }
        public ICellStyle HeaderStyle { get; set; }
        public ICellStyle ContentStyle { get; set; }
        public XSSFWorkbook Workbook { get; set; }
    }

    public class HomeroomAverageModel
    {
        public string Homeroom { get; set; }
        public double Avg { get; set; }
    }

    public class GradeAverageModel
    {
        public GradeAverageModel()
        {
            StatementAvg = new List<StatementAverageModel>();
        }
        public string Grade { get; set; }
        public List<StatementAverageModel> StatementAvg { get; set; }
    }

    public class StatementAverageModel
    {
        public int Number { get; set; }
        public string Statement { get; set; }
        public double Avg { get; set; }
    }

    public class LevelGradeByHomeroomModel
    {
        public string Level { get; set; }
        public string Grade { get; set; }
        public string Homeroom { get; set; }
    }
}
