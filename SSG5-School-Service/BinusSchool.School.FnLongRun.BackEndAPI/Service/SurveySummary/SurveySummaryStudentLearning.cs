using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Data.Model.School.FnSchool.SurveySummary;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveySummary;
using BinusSchool.Data.Model.School.FnSurveyNoSql.SurveyTemplate;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Persistence.SchoolDb.Entities.User;
using BinusSchool.School.FnLongRun.SurveySummary;
using Microsoft.EntityFrameworkCore;
using NPOI.HPSF;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using Org.BouncyCastle.Utilities.IO.Pem;

namespace BinusSchool.School.FnLongRun.Services.SurveySummary
{
    public class SurveySummaryStudentLearning
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
                Date = param.Date,
                HeaderStyle = headerStyle,
                ContentStyle = contentStyle,
                Workbook = workbook,
                ServiceSurveySummary = param.ServiceSurveySummary,
                IdAcademicYear = param.IdAcademicYear
            };

            await GetSheetParticipantAsync(paramParticipant);

            var paramAnswer = new GetSheetStudentParticipantRequest
            {
                IdPublishSurvey = param.IdPublishSurvey,
                Date = param.Date,
                HeaderStyle = headerStyle,
                ContentStyle = contentStyle,
                Workbook = workbook,
                ServiceSurveySummaryNoSql = param.ServiceSurveySummaryNoSql,
                DbContext = param.DbContext,
                SurveyTemplateNoSql = param.SurveyTemplateNoSql,
                ServiceSurveySummary = param.ServiceSurveySummary,
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

            var listSurveys = await SurveySummaryGeneral.GetSurveyAnswerRespondent(new SurveySummaryGeneralRequest
            {
                IdPublishSurvey = request.IdPublishSurvey,
                ServiceSurveySummary = request.ServiceSurveySummary,
                ServiceSurveySummaryNoSql = request.ServiceSurveySummaryNoSql
            });

            var Sections = getSurveyTemplate.Sections.ToList();
            var sheet = request.Workbook.CreateSheet("Student Learning Survey");
            CreateHeaderQuestion(sheet, request.HeaderStyle, Sections);

            var dataValue = await CreateValueContentAnswerAsync(request, listSurveys);

            CreateContentAnswer(sheet, request.ContentStyle, dataValue);
            return default;
        }

        private static void CreateContentAnswer(ISheet sheet, ICellStyle contentStyle,
            List<GetValueLearningStudentResult> listRespondents)
        {
            int i = 1;
            int j = 0;

            var groupDataByTeacher = (
                from c in listRespondents
                group c by new
                {
                    c.IdUser,
                    c.IdBinusian,
                    c.TeacherName,
                    c.SubjectName,
                    c.SubjectId,
                    c.Grade,
                    c.ClassId,
                    c.Homeroom
                } into gcs
                select new
                {
                    IdBinusian = gcs.Key.IdBinusian,
                    TeacherName = gcs.Key.TeacherName,
                    SubjectName = gcs.Key.SubjectName,
                    SubjectId = gcs.Key.SubjectId,
                    Grade = gcs.Key.Grade,
                    ClassId = gcs.Key.ClassId,
                    Homeroom = gcs.Key.Homeroom,
                    Value = gcs.ToList(),
                }).ToList();

            foreach (var respondent in groupDataByTeacher)
            {
                j = 0;
                var contentRow = sheet.CreateRow(i++);
                var contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.IdBinusian);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.TeacherName);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.SubjectName);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.SubjectId);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.Grade);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.ClassId);
                contentCell.CellStyle = contentStyle;

                contentCell = contentRow.CreateCell(j++);
                contentCell.SetCellValue(respondent.Homeroom);
                contentCell.CellStyle = contentStyle;

                foreach (var item in respondent.Value.OrderBy(x => x.Section))
                {
                    contentCell = contentRow.CreateCell(j++);
                    contentCell.SetCellValue(item.Value);
                    contentCell.CellStyle = contentStyle;
                }
            }
        }

        private static async Task<List<GetValueLearningStudentResult>> CreateValueContentAnswerAsync(GetSheetStudentParticipantRequest request, List<GetSurveySummaryNoSqlResult> listSurvey)
        {
            var getserviceDetailSurveySummaryRespondent = await request.ServiceSurveySummary.DetailSurveySummaryRespondent(new DetailSurveySummaryRespondentRequest
            {
                IdPublishSurvey = request.IdPublishSurvey
            });

            var getSurveySummaryRespondent = getserviceDetailSurveySummaryRespondent.Payload;
            var listUsers = new List<string> { "2270003751", "2370005253" };

            var getDataUser = getSurveySummaryRespondent.Where(x => listUsers.Any(t => t == x.IdUser)).ToList();

            var valueLearningStudents = new List<GetValueLearningStudentResult>();

            foreach (var survey in listSurvey)
            {
                foreach (var section in survey.Sections)
                {
                    var questions = section.Questions.ToList();
                    if (!questions.Any())
                        continue;

                    foreach (var dataquestion in questions)
                    {
                        var respondentAnswers = dataquestion.RespondentAnswers.ToList();
                        if (!respondentAnswers.Any())
                            continue;

                        var learningStudentAnswers = respondentAnswers.Select(x => x.LikertLearningStudentAnswers).FirstOrDefault();
                        if (!learningStudentAnswers.Any())
                            continue;

                        var dataLearningStudents = learningStudentAnswers.Select(x => new GetValueLearningStudentResult
                        {
                            IdUser = survey.IdUser,
                            Semester = survey.Semester.ToString(),
                            Section = section.NameSection,
                            QuestionNumber = dataquestion.Number.ToString(),
                            IdBinusian = x.IdUserTeacher,
                            Value = x.Value
                        }).ToList();

                        valueLearningStudents.AddRange(dataLearningStudents);
                    }
                }
            }

            var idTeacher = valueLearningStudents.Select(x => x.IdBinusian).ToList();

            var GetAllTeacher = await request.DbContext.Entity<MsUser>()
                .Where(x => idTeacher.Any(t => t == x.Username))
                .Select(x => new { x.Username, x.DisplayName }).ToListAsync();

            var completedValueLearningStudentResult = valueLearningStudents.Select(x => new GetValueLearningStudentResult
            {
                IdUser = x.IdUser,
                Semester = x.Semester,
                Section = x.Section,
                QuestionNumber = x.QuestionNumber,
                IdBinusian = x.IdBinusian,
                TeacherName = GetAllTeacher.Where(t => t.Username == x.IdBinusian).Select(t => t.DisplayName).FirstOrDefault(),
                SubjectName = "SubjectName",
                SubjectId = "SubjectId",
                Grade = getSurveySummaryRespondent.Where(y => y.IdUser == x.IdUser).Select(t => t.Grade.Description).FirstOrDefault(),
                IdGrade = getSurveySummaryRespondent.Where(y => y.IdUser == x.IdUser).Select(t => t.Grade.Id).FirstOrDefault(),
                ClassId = "ClassId",
                Homeroom = getSurveySummaryRespondent.Where(y => y.IdUser == x.IdUser).Select(t => t.Homeroom.Description).FirstOrDefault(),
                Value = x.Value
            }).ToList();

            var getGroupTeacher = (from c in completedValueLearningStudentResult
                                   group c by new
                                   {
                                       c.IdBinusian,
                                       c.IdGrade,
                                       c.Semester,
                                   } into gcs
                                   select new
                                   {
                                       IdBinusian = gcs.Key.IdBinusian,
                                       IdGrade = gcs.Key.IdGrade,
                                       Semester = gcs.Key.Semester
                                   }).ToList();

            foreach (var data in getGroupTeacher)
            {
                var GetLessonTeacher = request.DbContext.Entity<MsLessonTeacher>()
                .Include(x => x.Lesson).ThenInclude(x => x.Subject)
                .ThenInclude(x => x.Grade)
                .Where(x =>
                x.IdUser == data.IdBinusian &&
                x.Lesson.Subject.IdGrade == data.IdGrade &&
                x.Lesson.Semester == int.Parse(data.Semester))
                .Select(x => new
                {
                    SubjectName = x.Lesson.Subject.Description,
                    SubjectId = x.Lesson.Subject.SubjectID,
                    ClassId = x.Lesson.ClassIdGenerated
                }).ToList();

                var dataInComplate = completedValueLearningStudentResult.Where(x => x.IdBinusian == data.IdBinusian).ToList();
                foreach (var item in dataInComplate)
                {
                    item.SubjectName = string.Join(" , ", GetLessonTeacher.Select(x => x.SubjectName));
                    item.SubjectId = string.Join(" , ", GetLessonTeacher.Select(x => x.SubjectId));
                    item.ClassId = string.Join(" , ", GetLessonTeacher.Select(x => x.ClassId));
                }
            }

            return completedValueLearningStudentResult;
        }

        private static void CreateHeaderQuestion(ISheet sheet, ICellStyle headerStyle, List<Data.Model.School.FnSurveyNoSql.SurveyTemplate.Section> sectionValues)
        {
            ICell cellNo = default;
            var startColumn = 0;
            var rowIndex = 0;
            var rowHeader = sheet.CreateRow(rowIndex);

            List<string> header = new List<string>() { "Binusian ID", "Teacher Name", "Subject Name", "Subject ID", "Grade", "Class ID", "Homeroom" };
            
            foreach (var sectionValue in sectionValues)
            {
                foreach (var Question in sectionValue.Questions)
                {
                    header.Add($"Section {sectionValue.NameSection} (Q{Question.Number}) ");
                }
            }

            foreach (var itemHeader in header)
            {
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemHeader);
                cellNo.CellStyle = headerStyle;
                startColumn++;
            }
            rowIndex++;
        }
    }

    public class GetValueLearningStudentResult : DefaultRequest
    {
        public string IdUser { get; set; }
        public string Semester { get; set; }
        public string Section { get; set; }
        public string QuestionNumber { get; set; }
        public string IdBinusian { get; set; }
        public string TeacherName { get; set; }
        public string SubjectName { get; set; }
        public string SubjectId { get; set; }
        public string Grade { get; set; }
        public string ClassId { get; set; }
        public string Homeroom { get; set; }
        public string Value {  get; set; }
        public string IdGrade { get; set; }
    }
}
