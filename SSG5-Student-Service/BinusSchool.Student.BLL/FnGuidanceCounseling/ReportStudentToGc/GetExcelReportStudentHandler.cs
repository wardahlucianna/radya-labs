using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.School;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc
{
    public class GetExcelReportStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetExcelReportStudentHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }


        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<GetExcelReportStudentRequest>();

            var GetStudent = _dbContext.Entity<MsStudent>()
                               .Where(e => e.Id == param.IdUserStudent).SingleOrDefault();

            var title = "StudentReportHistory_" + NameUtil.GenerateFullName(GetStudent.FirstName, GetStudent.MiddleName, GetStudent.LastName) + "_" + DateTime.Now.ToString("ddMMyyyy");
            var generateExcelByte = GenerateExcel(title, param);
            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{title}.xlsx"
            };
        }

        public byte[] GenerateExcel(string sheetTitle, GetExcelReportStudentRequest param)
        {
            var predicate = PredicateBuilder.Create<TrGcReportStudent>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdUserStudent);

            //filter
            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.Note.Contains(param.Search) || x.UserReport.DisplayName.Contains(param.Search));


            var GetGcReportStudent = _dbContext.Entity<TrGcReportStudent>()
                        .Include(e => e.Student)
                        .Include(e => e.UserReport)
                        .Include(e => e.AcademicYear)
                        .Where(predicate)
                        .ToList();

            var ReportStudent = GetGcReportStudent.FirstOrDefault();

            if (ReportStudent == null)
                throw new BadRequestException("Report Student with id Student: " + param.IdUserStudent + " is not found.");

            var GetHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.GradePathway).ThenInclude(e => e.Grade).ThenInclude(e => e.MsLevel)
                                .Include(e => e.Homeroom).ThenInclude(e => e.MsGradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(e => e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                        && e.IdStudent == param.IdUserStudent && e.IdHomeroom == param.IdHomeroom)
                                .SingleOrDefault();

            if (GetHomeroomStudent == null)
                throw new BadRequestException("Homeroom student with id student: " + param.IdUserStudent + " is not found.");

            var GetCounselor = _dbContext.Entity<MsCounselorGrade>()
                                .Include(e => e.Counselor).ThenInclude(e=>e.User)
                                .Where(e => e.Counselor.IdAcademicYear == param.IdAcademicYear && e.IdGrade== GetHomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.IdGrade).SingleOrDefault();

            var GethomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Staff)
                                .Where(e => e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                        && e.IdHomeroom == param.IdHomeroom && e.IsAttendance == true)
                                .SingleOrDefault();

            if (GetHomeroomStudent == null)
                throw new BadRequestException("Homeroom teacher with id homeroom: " + GetHomeroomStudent.IdHomeroom + " is not found.");

            var result = new byte[0];
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);

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
                var fontBold = workbook.CreateFont();
                fontBold.IsBold = true;
                var boldStyle = workbook.CreateCellStyle();
                boldStyle.SetFont(fontBold);

                IRow rowHeader = excelSheet.CreateRow(8);
                var cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Report By");
                cell.CellStyle = boldStyle;

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue("Date");
                cell.CellStyle = boldStyle;

                cell = rowHeader.CreateCell(2);
                cell.SetCellValue("Note");
                cell.CellStyle = boldStyle;

                //Row1
                rowHeader = excelSheet.CreateRow(0);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Student Couselling Log");
                cell.CellStyle = boldStyle;

                //Row2
                rowHeader = excelSheet.CreateRow(1);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Generate Date");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(DateTime.Now.ToString("dd/MM/yyyy"));

                //Row4
                rowHeader = excelSheet.CreateRow(3);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Counsellor");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(GetCounselor.Counselor.User.DisplayName);

                //Row5
                rowHeader = excelSheet.CreateRow(4);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Student Name");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(NameUtil.GenerateFullName(ReportStudent.Student.FirstName, ReportStudent.Student.MiddleName, ReportStudent.Student.LastName));

                //Row6
                rowHeader = excelSheet.CreateRow(5);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Garde");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(GetHomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code);

                //row7
                rowHeader = excelSheet.CreateRow(5);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Homeroom");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(GetHomeroomStudent.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.Code + GetHomeroomStudent.Homeroom.MsGradePathwayClassroom.Classroom.Code);

                //row7
                rowHeader = excelSheet.CreateRow(6);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Class Advisor");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(NameUtil.GenerateFullName(GethomeroomTeacher.Staff.FirstName, "", GethomeroomTeacher.Staff.LastName));


                int rowIndex = 9;
                int startColumn = 0;
                foreach (var itemData in GetGcReportStudent)
                {
                    rowHeader = excelSheet.CreateRow(rowIndex);
                    cell = rowHeader.CreateCell(0);
                    cell.SetCellValue(itemData.UserReport.DisplayName);
                    cell = rowHeader.CreateCell(1);
                    cell.SetCellValue(itemData.Date.ToString("dd/MM/yyyy"));
                    cell = rowHeader.CreateCell(2);
                    cell.SetCellValue(itemData.Note);
                    rowIndex++;
                    startColumn++;

                }

                IRow headerRow = excelSheet.GetRow(rowIndex - 1);

                for (int i = 0, l = headerRow.LastCellNum; i < l; i++)
                {
                    excelSheet.AutoSizeColumn(i);
                }

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
