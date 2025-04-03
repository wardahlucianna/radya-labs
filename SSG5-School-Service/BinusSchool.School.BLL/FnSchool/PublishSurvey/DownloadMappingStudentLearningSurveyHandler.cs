using System;
using System.Collections.Generic;
using System.Text;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Common.Extensions;
using System.Threading.Tasks;
using System.Threading;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceSummaryTerm;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.Azure.Documents.SystemFunctions;
using BinusSchool.Common.Exceptions;
using Newtonsoft.Json;
using BinusSchool.Persistence.SchoolDb.Abstractions;
using BinusSchool.Persistence.SchoolDb.Entities;
using BinusSchool.Persistence.SchoolDb.Entities.Scheduling;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Model.School.FnSchool.PublishSurvey;
using BinusSchool.Common.Constants;

namespace BinusSchool.School.FnSchool.PublishSurvey
{
    public class DownloadMappingStudentLearningSurveyHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchoolDbContext _dbContext;
        private readonly IPublishSurvey _servicePublishSurvey;
        public DownloadMappingStudentLearningSurveyHandler(ISchoolDbContext dbContext, IPublishSurvey servicePublishSurvey)
        {
            _dbContext = dbContext;
            _servicePublishSurvey = servicePublishSurvey;
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            FillConfiguration();

            var param = Request.ValidateParams<DownloadMappingStudentLearningSurveyRequest>();

            #region Get data
            var getSurveyTemplatePublish = await _dbContext.Entity<TrPublishSurvey>()
                                           .Include(e => e.AcademicYear)
                                           .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyDepartments)
                                           .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Grade)
                                           .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyGrades).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                                           .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyPositions)
                                           .Include(e => e.PublishSurveyRespondents).ThenInclude(e => e.PublishSurveyUsers)
                                           .Where(e => e.Id == param.IdPublishSurvey)
                                           .FirstOrDefaultAsync(CancellationToken);

            if(getSurveyTemplatePublish == null)
                throw new BadRequestException("publish survey is exsis");

            var listHomeroom = await _dbContext.Entity<MsHomeroom>()
                                    .Include(e => e.Grade)
                                    .Include(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                  .Where(e => e.IdAcademicYear == getSurveyTemplatePublish.IdAcademicYear && e.Semester== getSurveyTemplatePublish.Semester)
                                  .Select(e => new
                                  {
                                      IdHomeroom = e.Id,
                                      Homeroom = e.Grade.Code + e.GradePathwayClassroom.Classroom.Code,
                                      IdGrade = e.IdGrade,
                                      Grade = e.Grade.Code,
                                      IdLevel = e.Grade.IdLevel,
                                      Level = e.Grade.Level.Code,
                                      Semester = e.Semester,
                                      AcademicYear = e.Grade.Level.AcademicYear.Code,
                                  })
                                  .ToListAsync(CancellationToken);

            List<PublishSurveyRespondentHomeroom> listRespondent =  new List<PublishSurveyRespondentHomeroom> ();

            foreach(var item in getSurveyTemplatePublish.PublishSurveyRespondents)
            {
                if (item.Role == PublishSurveyRole.Student)
                {
                    if (item.Option == PublishSurveyOption.Grade)
                    {
                        var _listRespondent = getSurveyTemplatePublish.PublishSurveyRespondents
                            .SelectMany(e => e.PublishSurveyGrades
                            .GroupBy(e => new
                            {
                                IdGrade = e.IdGrade,
                                IdHomeroom = e.IdHomeroom,
                                IdLevel = e.IdLevel,
                                Semester = e.Semester,
                                AcademicYear = e.Grade.Level.AcademicYear.Code,
                                Level = e.Grade.Level.Code,
                                Grade = e.Grade.Code,
                                Homeroom = listHomeroom.Where(f => f.IdHomeroom == e.IdHomeroom).Select(e => e.Homeroom).FirstOrDefault(),
                            }))
                            .Select(e => new PublishSurveyRespondentHomeroom
                            {
                                IdGrade = e.Key.IdGrade,
                                IdHomeroom = e.Key.IdHomeroom,
                                IdLevel = e.Key.IdLevel,
                                Semester = e.Key.Semester,
                                Homeroom = e.Key.Homeroom,
                                AcademicYear = e.Key.AcademicYear,
                                Level = e.Key.Level,
                                Grade = e.Key.Grade,
                            })
                            .OrderBy(e => e.Homeroom).ToList();

                        listRespondent.AddRange(_listRespondent);
                    }
                    else if(item.Option == PublishSurveyOption.Level || item.Option == PublishSurveyOption.All)
                    {
                        var _listRespondent = listHomeroom
                            .GroupBy(e => new 
                            {
                                IdGrade = e.IdGrade,
                                IdHomeroom = e.IdHomeroom,
                                IdLevel = e.IdLevel,
                                Semester = e.Semester,
                                AcademicYear = e.AcademicYear,
                                Level = e.Level,
                                Grade = e.Grade,
                                Homeroom = listHomeroom.Where(f => f.IdHomeroom == e.IdHomeroom).Select(e => e.Homeroom).FirstOrDefault(),
                            })
                            .Select(e => new PublishSurveyRespondentHomeroom
                            {
                                IdGrade = e.Key.IdGrade,
                                IdHomeroom = e.Key.IdHomeroom,
                                IdLevel = e.Key.IdLevel,
                                Semester = e.Key.Semester,
                                Homeroom = e.Key.Homeroom,
                                AcademicYear = e.Key.AcademicYear,
                                Level = e.Key.Level,
                                Grade = e.Key.Grade,
                            })
                            .OrderBy(e => e.Homeroom).ToList();

                        var listIdLevel = item.PublishSurveyGrades.Where(e=>e.IdLevel!=null).Select(e=>e.IdLevel).Distinct().ToList();

                        if(listIdLevel.Any())
                            _listRespondent=_listRespondent.Where(e => listIdLevel.Contains(e.IdLevel)).ToList();

                        listRespondent.AddRange(_listRespondent);
                    }
                }
            }

            
            #endregion

            var excelRecap = await GenerateExcel(listRespondent, getSurveyTemplatePublish, _servicePublishSurvey);

            return new FileContentResult(excelRecap, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Mapping-Student-Learning-Survey{DateTime.Now.Ticks}.xlsx"
            };
            return null;
        }
        private async Task<byte[]> GenerateExcel(List<PublishSurveyRespondentHomeroom> listRespondent, TrPublishSurvey dataPublishSurvey, IPublishSurvey _servicePublishSurvey)
        {
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.BorderBottom = BorderStyle.Thin;
            boldStyle.BorderLeft = BorderStyle.Thin;
            boldStyle.BorderRight = BorderStyle.Thin;
            boldStyle.BorderTop = BorderStyle.Thin;
            boldStyle.WrapText = true;
            boldStyle.SetFont(fontBold);

            var borderCellStyle = workbook.CreateCellStyle();
            borderCellStyle.BorderBottom = BorderStyle.Thin;
            borderCellStyle.BorderLeft = BorderStyle.Thin;
            borderCellStyle.BorderRight = BorderStyle.Thin;
            borderCellStyle.BorderTop = BorderStyle.Thin;
            boldStyle.WrapText = true;

            foreach (var itemRespondent in listRespondent)
            {
                var paramAddMappingStudent = new GetResetMappingStudentLearningSurveyRequest
                {
                    IdAcademicYear = dataPublishSurvey.IdAcademicYear,
                    Semester = dataPublishSurvey.Semester,
                    IdPublishSurvey = dataPublishSurvey.Id,
                    IdLevel = itemRespondent.IdLevel,
                    IdGrade = itemRespondent.IdGrade,
                    IdHomeroom = itemRespondent.IdHomeroom,
                    GetAll = true,
                    Return = CollectionType.Lov
                };

                var serviceGetMappingStudentLearningSurvey = await _servicePublishSurvey.GetMappingStudentLearningSurvey(paramAddMappingStudent);
                var GetMappingStudentLearningSurvey = serviceGetMappingStudentLearningSurvey.IsSuccess ? serviceGetMappingStudentLearningSurvey.Payload : null;

                if (GetMappingStudentLearningSurvey == null)
                    continue;
                
                var sheet = workbook.CreateSheet(itemRespondent.Homeroom);

                #region
                var rowIndex = 0;
                int startColumn = 0;
                var rowHeader = sheet.CreateRow(rowIndex);
                var cellNo = rowHeader.CreateCell(rowIndex);
                cellNo.SetCellValue("Academic Year");

                startColumn = 1;
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemRespondent.AcademicYear);
                rowIndex++;

                startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue("Semester");

                startColumn = 1;
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemRespondent.Semester.ToString());
                rowIndex++;

                startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue("Level");

                startColumn = 1;
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemRespondent.Level);
                rowIndex++;

                startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue("Grade");

                startColumn = 1;
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemRespondent.Grade);
                rowIndex++;

                startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue("Homeroom");

                startColumn = 1;
                cellNo = rowHeader.CreateCell(startColumn);
                cellNo.SetCellValue(itemRespondent.Homeroom);
                rowIndex += 2;
                #endregion

                #region header
                List<string> listHeader = new List<string>();
                List<string> listHeaderKey = new List<string>();
                listHeader.Add("Student Name");
                listHeaderKey.Add("StudentName");
                listHeader.Add("Religion");
                listHeaderKey.Add("Religion");

                foreach (var itemHeader in GetMappingStudentLearningSurvey.Header)
                {
                    listHeaderKey.Add($"{itemHeader.ClassId}-{itemHeader.IdUser}");
                    listHeader.Add($"{itemHeader.ClassId} \r\n {itemHeader.Subject} \r\n {itemHeader.IdUser} \r\n {itemHeader.TeacherName}");
                }

                startColumn = 0;
                rowHeader = sheet.CreateRow(rowIndex);
                foreach (var itemHeader in listHeader)
                {
                    cellNo = rowHeader.CreateCell(startColumn);
                    cellNo.SetCellValue(itemHeader);
                    cellNo.CellStyle = boldStyle;
                    startColumn++;
                }
                rowIndex++;
                #endregion

                #region body
                foreach (var itemBody in GetMappingStudentLearningSurvey.MappingStudentLearningSurvey)
                {
                    rowHeader = sheet.CreateRow(rowIndex);
                    startColumn = 0;
                    foreach (var itemHeaderKey in listHeaderKey)
                    {
                        var getValue = itemBody.Where(e => e.Key == itemHeaderKey).Select(e => e.Value).FirstOrDefault();
                        var value = getValue==null?"": getValue.ToString();

                        if (itemHeaderKey != "StudentName" && itemHeaderKey != "Religion")
                        {
                            MappingStudentLearningValueResult json = JsonConvert.DeserializeObject<MappingStudentLearningValueResult>(value);
                            value = json.IsChecked == true ? "Yes" :"-";
                        }

                        cellNo = rowHeader.CreateCell(startColumn);
                        cellNo.SetCellValue(value.ToString());
                        cellNo.CellStyle = borderCellStyle;
                        startColumn++;
                    }
                    rowIndex++;
                }
                #endregion

            }
            using var ms = new MemoryStream();
            ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
    }

}
