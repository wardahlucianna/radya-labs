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
    public class GetExcelCounselingServicesEntryByStudentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        public GetExcelCounselingServicesEntryByStudentHandler(IStudentDbContext EntryMeritDemetitDbContext)
        {
            _dbContext = EntryMeritDemetitDbContext;
        }


        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.GetBody<GetExcelCounselingServicesEntryByStudentRequest>();

            var GetStudent = _dbContext.Entity<MsStudent>()
                                .Where(e=>e.Id == param.IdUserStudent).SingleOrDefault();

            var title = "StudentCounselingHistory_"+ NameUtil.GenerateFullName(GetStudent.FirstName, GetStudent.MiddleName, GetStudent.LastName) + "_"+DateTime.Now.ToString("ddMMyyyy");
            var generateExcelByte = GenerateExcel(title, param);
            return new FileContentResult(generateExcelByte, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{title}.xlsx"
            };
        }

        public byte[] GenerateExcel(string sheetTitle, GetExcelCounselingServicesEntryByStudentRequest param)
        {
            var predicate = PredicateBuilder.Create<TrCounselingServicesEntry>(x => x.IdAcademicYear == param.IdAcademicYear && x.IdStudent == param.IdUserStudent);

            if (!string.IsNullOrEmpty(param.Search))
                predicate = predicate.And(x => x.CounselingCategory.CounselingCategoryName.Contains(param.Search) || x.Counselor.User.DisplayName.Contains(param.Search));
            if (!string.IsNullOrEmpty(param.IdConselingCategory))
                predicate = predicate.And(x => x.IdCounselingCategory == param.IdConselingCategory);

            var GetCounselingServiceEntry = _dbContext.Entity<TrCounselingServicesEntry>()
                                            .Include(e => e.CounselingCategory)
                                            .Include(e => e.Counselor)
                                            .Include(e => e.Student)
                                            .Include(e => e.AcademicYear)
                                            .Where(predicate).ToList();

            var CounselingServiceEntry = GetCounselingServiceEntry.FirstOrDefault();

            if (CounselingServiceEntry == null)
                throw new BadRequestException("Counselling services entry with id counselling category: " + param.IdConselingCategory + " is not found.");

            var GetHomeroomStudent = _dbContext.Entity<MsHomeroomStudent>()
                                .Include(e => e.Homeroom).ThenInclude(e=>e.MsGradePathwayClassroom).ThenInclude(e=>e.GradePathway).ThenInclude(e=>e.Grade).ThenInclude(e=>e.MsLevel)
                                .Include(e => e.Homeroom).ThenInclude(e=>e.MsGradePathwayClassroom).ThenInclude(e=>e.Classroom)
                                .Where(e=>e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear==param.IdAcademicYear
                                        && e.IdStudent==param.IdUserStudent && e.IdHomeroom==param.IdHomeroom)
                                .SingleOrDefault();

            if (GetHomeroomStudent == null)
                throw new BadRequestException("Homeroom student with id student: " + param.IdUserStudent + " is not found.");

            var GethomeroomTeacher = _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Staff)
                                .Where(e => e.Homeroom.MsGradePathwayClassroom.GradePathway.Grade.MsLevel.IdAcademicYear == param.IdAcademicYear
                                        && e.IdHomeroom == param.IdHomeroom && e.IsAttendance==true)
                                .SingleOrDefault();

            if (GetHomeroomStudent == null)
                throw new BadRequestException("Homeroom teacher with id homeroom: " + GetHomeroomStudent.IdHomeroom + " is not found.");

            var GetCounselingServicesEntryConcern = _dbContext.Entity<TrCounselingServicesEntryConcern>()
                                            .Include(e => e.ConcernCategory)
                                            .Where(e => GetCounselingServiceEntry.Select(e => e.Id).Contains(e.IdCounselingServicesEntry)).ToList();

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
                cell.SetCellValue("Counseling With");
                cell.CellStyle = boldStyle;

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue("Date");
                cell.CellStyle = boldStyle;

                cell = rowHeader.CreateCell(2);
                cell.SetCellValue("Time");
                cell.CellStyle = boldStyle;

                cell = rowHeader.CreateCell(3);
                cell.SetCellValue("Counseling Category");
                cell.CellStyle = boldStyle;

                cell = rowHeader.CreateCell(4);
                cell.SetCellValue("Refered By");
                cell.CellStyle = boldStyle;
                cell.CellStyle.WrapText = true;

                cell = rowHeader.CreateCell(5);
                cell.SetCellValue("Concern Category");
                cell.CellStyle = boldStyle;
                cell.CellStyle.WrapText = true;

                cell = rowHeader.CreateCell(6);
                cell.SetCellValue("Brief Report");
                cell.CellStyle = boldStyle;
                cell.CellStyle.WrapText = true;

                cell = rowHeader.CreateCell(7);
                cell.SetCellValue("Follow Up");
                cell.CellStyle = boldStyle;
                cell.CellStyle.WrapText = true;

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
                //cell.SetCellValue(CounselingServiceEntry.Counselor.User.DisplayName);

                //Row5
                rowHeader = excelSheet.CreateRow(4);
                cell = rowHeader.CreateCell(0);
                cell.SetCellValue("Student Name");

                cell = rowHeader.CreateCell(1);
                cell.SetCellValue(NameUtil.GenerateFullName(CounselingServiceEntry.Student.FirstName, CounselingServiceEntry.Student.MiddleName, CounselingServiceEntry.Student.LastName));

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
                foreach (var itemData in GetCounselingServiceEntry)
                {
                    var ConcernCategory = "";
                    var GetCounselingServicesEntryConcernById = GetCounselingServicesEntryConcern.Where(e => e.IdCounselingServicesEntry == itemData.Id).ToList();
                    foreach (var itemDataChild in GetCounselingServicesEntryConcernById)
                    {
                        ConcernCategory += (GetCounselingServicesEntryConcernById.IndexOf(itemDataChild) + 1) == GetCounselingServicesEntryConcernById.Count()
                            ? itemDataChild.ConcernCategory.ConcernCategoryName
                            : itemDataChild.ConcernCategory.ConcernCategoryName + ", ";
                    }

                    rowHeader = excelSheet.CreateRow(rowIndex);
                    cell = rowHeader.CreateCell(0);
                    cell.SetCellValue(itemData.CounselingWith.ToString());
                    cell = rowHeader.CreateCell(1);
                    cell.SetCellValue(itemData.DateTime.ToString("dd/MM/yyyy"));
                    cell = rowHeader.CreateCell(2);
                    cell.SetCellValue(itemData.DateTime.ToString("hh:mm"));
                    cell = rowHeader.CreateCell(3);
                    cell.SetCellValue(itemData.CounselingCategory.CounselingCategoryName);
                    cell = rowHeader.CreateCell(4);
                    cell.SetCellValue(itemData.ReferredBy);
                    cell = rowHeader.CreateCell(5);
                    cell.SetCellValue(ConcernCategory);
                    cell = rowHeader.CreateCell(6);
                    cell.SetCellValue(itemData.BriefReport);
                    cell = rowHeader.CreateCell(7);
                    cell.SetCellValue(itemData.FollowUp);
                    cell = rowHeader.CreateCell(8);
                    rowIndex++;
                    startColumn++;

                }

                IRow headerRow = excelSheet.GetRow(rowIndex-1);

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
