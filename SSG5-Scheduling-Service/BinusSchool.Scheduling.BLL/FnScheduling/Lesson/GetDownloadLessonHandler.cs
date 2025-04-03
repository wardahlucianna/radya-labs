using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Scheduling.FnSchedule.Lesson;
using BinusSchool.Data.Model.Scheduling.FnSchedule.ScheduleRealizationV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.StudentCertification;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.EntityFrameworkCore;
using Microsoft.OData;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;


namespace BinusSchool.Scheduling.FnSchedule.Lesson
{
    public class GetDownloadLessonHandler : FunctionsHttpSingleHandler
    {
        private static readonly Lazy<string[]> _requiredParams = new Lazy<string[]>(new[]
        {
            nameof(GetLessonRequest.IdAcadyear)
        });

        private readonly ISchedulingDbContext _dbContext;

        public GetDownloadLessonHandler(ISchedulingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = Request.ValidateParams<GetLessonRequest>(_requiredParams.Value);

            var predicate = PredicateBuilder.Create<MsLesson>(x => x.IdAcademicYear == param.IdAcadyear);
            if (param.Semester > 0)
                predicate = predicate.And(x => x.Semester == param.Semester);
            if (!string.IsNullOrEmpty(param.IdLevel))
                predicate = predicate.And(x => x.Grade.IdLevel == param.IdLevel);
            if (!string.IsNullOrEmpty(param.IdGrade))
                predicate = predicate.And(x => x.IdGrade == param.IdGrade);
            if (!string.IsNullOrEmpty(param.IdSubject))
                predicate = predicate.And(x => x.IdSubject == param.IdSubject);
            if (!string.IsNullOrEmpty(param.IdHomeroom))
                predicate = predicate.And(x => x.LessonPathways.Any(y => y.HomeroomPathway.IdHomeroom == param.IdHomeroom));
            if (param.ExceptIds != null && param.ExceptIds.Any())
                predicate = predicate.And(x => !param.ExceptIds.Contains(x.Id));
            if (!string.IsNullOrWhiteSpace(param.Search))
                predicate = predicate.And(x
                    => EF.Functions.Like(x.Grade.Description, param.SearchPattern())
                    || EF.Functions.Like(x.ClassIdGenerated, param.SearchPattern())
                    || EF.Functions.Like(x.Subject.Description, param.SearchPattern())
                    || EF.Functions.Like(x.TotalPerWeek.ToString(), param.SearchPattern())
                    || x.LessonTeachers.Any(y => EF.Functions.Like(y.Staff.FirstName, param.SearchPattern()))
                    || x.LessonPathways.Any(y => EF.Functions.Like(y.HomeroomPathway.Homeroom.Grade.Code, param.SearchPattern()))
                    || x.LessonPathways.Any(y => EF.Functions.Like(y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code, param.SearchPattern())));

            var academicYear = await _dbContext.Entity<MsAcademicYear>().FirstOrDefaultAsync(x => x.Id == param.IdAcadyear, CancellationToken);
            var semester = !string.IsNullOrEmpty(param.Semester.ToString()) ? param.Semester.ToString() : "";

            var query = await _dbContext.Entity<MsLesson>()
                .SearchByIds(param)
                .Where(predicate)
                .Select(x => new GetLessonResult
                {
                    Id = x.Id,
                    Grade = x.Grade.Description,
                    Semester = x.Semester,
                    ClassId = x.ClassIdGenerated,
                    Subject = x.Subject.Description,
                    Teachers = x.LessonTeachers.Select(y => new NameValueVm(y.IdUser, !string.IsNullOrEmpty(y.Staff.FirstName) ? y.Staff.FirstName : y.Staff.LastName)),
                    TotalPerWeek = x.TotalPerWeek,
                    Homeroom = string.Join(", ", x.LessonPathways
                            .OrderBy(y => y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code)
                            .Select(y => string.Format("{0} {1}{2}",
                                y.HomeroomPathway.Homeroom.Grade.Code,
                                y.HomeroomPathway.Homeroom.GradePathwayClassroom.Classroom.Code,
                                y.HomeroomPathway.GradePathwayDetail.Pathway.Code.Equals("No Pathway", StringComparison.OrdinalIgnoreCase)
                                    ? string.Empty
                                    : " " + y.HomeroomPathway.GradePathwayDetail.Pathway.Code)))
                })
                .OrderBy(x => x.Subject)
                .ToListAsync(CancellationToken);

            var excelSubstitution = GenerateExcel(query, academicYear.Description, semester);

            return new FileContentResult(excelSubstitution, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"Acaademic_Year_Data_Setting_Lesson_{DateTime.Now.Ticks}.xlsx"
            };
        }

        private byte[] GenerateExcel(List<GetLessonResult> data, string academicYear, string semester)
        {
            var workbook = new XSSFWorkbook();

            var fontBold = workbook.CreateFont();
            fontBold.IsBold = true;

            var cellStyle = workbook.CreateCellStyle();
            cellStyle.BorderRight = BorderStyle.Thin;
            cellStyle.BorderLeft = BorderStyle.Thin;
            cellStyle.BorderBottom = BorderStyle.Thin;
            cellStyle.BorderTop = BorderStyle.Thin;

            var boldStyle = workbook.CreateCellStyle();
            boldStyle.SetFont(fontBold);

            var cellStyleAndBold = workbook.CreateCellStyle();
            cellStyleAndBold.BorderRight = BorderStyle.Thin;
            cellStyleAndBold.BorderLeft = BorderStyle.Thin;
            cellStyleAndBold.BorderBottom = BorderStyle.Thin;
            cellStyleAndBold.BorderTop = BorderStyle.Thin;
            cellStyleAndBold.SetFont(fontBold);
            cellStyleAndBold.Alignment = HorizontalAlignment.Center;

            var sheet = workbook.CreateSheet();
            
            var rowAyHeader = sheet.CreateRow(0);
            var cellAy = rowAyHeader.CreateCell(0);
            var cellAyValue = rowAyHeader.CreateCell(1);
            cellAy.SetCellValue("Academic Year");
            cellAy.CellStyle = boldStyle;
            cellAyValue.SetCellValue(academicYear);

            var rowSemesterHeader = sheet.CreateRow(1);
            var cellSemester= rowSemesterHeader.CreateCell(0);
            var cellSemesterValue = rowSemesterHeader.CreateCell(1);
            cellSemester.SetCellValue("Semester");
            cellSemester.CellStyle = boldStyle;
            cellSemesterValue.SetCellValue(semester);

            var rowHeader = sheet.CreateRow(3);
            var cellGrade = rowHeader.CreateCell(0);
            cellGrade.SetCellValue("Grade");
            cellGrade.CellStyle = cellStyleAndBold;

            var cellClassID = rowHeader.CreateCell(1);
            cellClassID.SetCellValue("Class ID");
            cellClassID.CellStyle = cellStyleAndBold;

            var cellSubject = rowHeader.CreateCell(2);
            cellSubject.SetCellValue("Subject");
            cellSubject.CellStyle = cellStyleAndBold;

            var cellTeacherName = rowHeader.CreateCell(3);
            cellTeacherName.SetCellValue("Teacher");
            cellTeacherName.CellStyle = cellStyleAndBold;

            var cellTotalWeek = rowHeader.CreateCell(4);
            cellTotalWeek.SetCellValue("Total/Week");
            cellTotalWeek.CellStyle = cellStyleAndBold;

            var cellHomeroomName = rowHeader.CreateCell(5);
            cellHomeroomName.SetCellValue("Homeroom Name");
            cellHomeroomName.CellStyle = cellStyleAndBold;

            int rowIndex = 4;
            int startColumn = 0;

            foreach (var item in data.Batch(200))
            {
                foreach (var itemData in item)
                {
                    rowHeader = sheet.CreateRow(rowIndex);
                    sheet.AutoSizeColumn(0);
                    cellGrade = rowHeader.CreateCell(0);
                    cellGrade.SetCellValue(itemData.Grade);
                    cellGrade.CellStyle = cellStyle;

                    cellClassID = rowHeader.CreateCell(1);
                    sheet.AutoSizeColumn(1);
                    cellClassID.SetCellValue(itemData.ClassId);
                    cellClassID.CellStyle = cellStyle;

                    cellSubject = rowHeader.CreateCell(2);
                    sheet.AutoSizeColumn(2);
                    cellSubject.SetCellValue(itemData.Subject);
                    cellSubject.CellStyle = cellStyle;

                    cellTeacherName = rowHeader.CreateCell(3);
                    sheet.AutoSizeColumn(3);
                    cellTeacherName.SetCellValue(string.Join(",", itemData.Teachers.Select(x => x.Name).ToList()));
                    cellTeacherName.CellStyle = cellStyle;

                    cellTotalWeek = rowHeader.CreateCell(4);
                    sheet.AutoSizeColumn(4);
                    cellTotalWeek.SetCellValue(itemData.TotalPerWeek);
                    cellTotalWeek.CellStyle = cellStyle;

                    cellHomeroomName = rowHeader.CreateCell(5);
                    sheet.AutoSizeColumn(5);
                    cellHomeroomName.SetCellValue(itemData.Homeroom);
                    cellHomeroomName.CellStyle = cellStyle;

                    rowIndex++;
                    startColumn++;
                }
            }

            //var batchData = data
            //     .Select((x, i) => new { Index = i, Value = x })
            //     .GroupBy(x => x.Index / 200)
            //     .Select(x => x.Select(v => v.Value).ToList())
            //     .ToList();

            //foreach (var batch in batchData)
            //{
            //    foreach (var itemData in batch)
            //    {
            //        rowHeader = sheet.CreateRow(rowIndex);
            //        sheet.AutoSizeColumn(0);
            //        cellGrade = rowHeader.CreateCell(0);
            //        cellGrade.SetCellValue(itemData.Grade);
            //        cellGrade.CellStyle = cellStyle;

            //        cellClassID = rowHeader.CreateCell(1);
            //        sheet.AutoSizeColumn(1);
            //        cellClassID.SetCellValue(itemData.ClassId);
            //        cellClassID.CellStyle = cellStyle;

            //        cellSubject = rowHeader.CreateCell(2);
            //        sheet.AutoSizeColumn(2);
            //        cellSubject.SetCellValue(itemData.Subject);
            //        cellSubject.CellStyle = cellStyle;

            //        cellTeacherName = rowHeader.CreateCell(3);
            //        sheet.AutoSizeColumn(3);
            //        cellTeacherName.SetCellValue(string.Join(",", itemData.Teachers.Select(x => x.Name).ToList()));
            //        cellTeacherName.CellStyle = cellStyle;

            //        cellTotalWeek = rowHeader.CreateCell(4);
            //        sheet.AutoSizeColumn(4);
            //        cellTotalWeek.SetCellValue(itemData.TotalPerWeek);
            //        cellTotalWeek.CellStyle = cellStyle;

            //        cellHomeroomName = rowHeader.CreateCell(5);
            //        sheet.AutoSizeColumn(5);
            //        cellHomeroomName.SetCellValue(itemData.Homeroom);
            //        cellHomeroomName.CellStyle = cellStyle;

            //        rowIndex++;
            //        startColumn++;
            //    }
            //}

            sheet.AutoSizeColumn(25);

            using var ms = new MemoryStream();
            //ms.Position = 0;
            workbook.Write(ms);

            return ms.ToArray();
        }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> Batch<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            if (enumerable == null) throw new ArgumentNullException(nameof(enumerable));
            if (batchSize <= 0) throw new ArgumentOutOfRangeException(nameof(batchSize));
            return enumerable.BatchCore(batchSize);
        }

        private static IEnumerable<IEnumerable<T>> BatchCore<T>(this IEnumerable<T> enumerable, int batchSize)
        {
            var c = 0;
            var batch = new List<T>();
            foreach (var item in enumerable)
            {
                batch.Add(item);
                if (++c % batchSize == 0)
                {
                    yield return batch;
                    batch = new List<T>();
                }
            }
            if (batch.Count != 0)
            {
                yield return batch;
            }
        }
    }
}
