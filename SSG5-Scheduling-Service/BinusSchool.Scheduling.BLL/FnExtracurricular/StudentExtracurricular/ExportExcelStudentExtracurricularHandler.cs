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
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.StudentExtracurricular;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular.Validator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Scheduling.FnExtracurricular.StudentExtracurricular
{
    public class ExportExcelStudentExtracurricularHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IStudentExtracurricular _studentExtracurricularApi;

        public ExportExcelStudentExtracurricularHandler(ISchedulingDbContext dbContext, IStudentExtracurricular studentExtracurricularApi)
        {
            _dbContext = dbContext;
            _studentExtracurricularApi = studentExtracurricularApi;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<ExportExcelStudentExtracurricularRequest, ExportExcelStudentExtracurricularValidator>();

            string currAY = "";
            string CurrLevel = "";
            string CurrGrade = "";
            string CurrHomeroom = "";
            var AY = _dbContext.Entity<MsAcademicYear>().Where(a => a.Id == param.IdAcademicYear).FirstOrDefault();
            if(AY != null)
            {
                currAY = AY.Description;
            }

            if (param.IdLevel != null)
            {
                var Level = _dbContext.Entity<MsLevel>().Where(a => a.Id == param.IdLevel).FirstOrDefault();
                if (Level != null)
                {
                    CurrLevel = Level.Description;
                }
            }
            else
            {
                CurrLevel = "ALL";
            }

            if (param.IdGrade != null)
            {
                var Grade = _dbContext.Entity<MsGrade>().Where(a => a.Id == param.IdGrade).FirstOrDefault();
                if (Grade != null)
                {
                    CurrGrade = Grade.Description;
                }
            }
            else
            {
                CurrGrade = "ALL";
            }

            if (param.IdHomeroom != null)
            {
                var Homeroom = _dbContext.Entity<MsGrade>().Where(a => a.Id == param.IdGrade).FirstOrDefault();
                if (Homeroom != null)
                {
                    CurrHomeroom = Homeroom.Description;
                }
            }
            else
            {
                CurrHomeroom = "ALL";
            }

            var paramDesc = new ExportExcelStudentExtracurricularResult_ParamDesc() { AcademicYear = currAY, Semester = param.Semester, Level = CurrLevel, Grade = CurrGrade, Homeroom = CurrHomeroom};

            //// param desc
            //var paramDesc = _dbContext.Entity<MsHomeroom>()
            //                    .Include(hs => hs.Grade)
            //                    .Include(h => h.GradePathwayClassroom)
            //                    .ThenInclude(gpc => gpc.Classroom)
            //                    .Include(hs => hs.Grade)
            //                    .ThenInclude(g => g.Level)
            //                    .ThenInclude(l => l.AcademicYear)
            //                    .Where(x => x.Grade.Id == param.IdGrade &&
            //                                x.Grade.Level.Id == param.IdLevel &&
            //                                x.Grade.Level.AcademicYear.Id == param.IdAcademicYear &&
            //                                param.IdHomeroom == "all" ? true == true : x.Id == param.IdHomeroom)
            //                    .Select(x => new ExportExcelStudentExtracurricularResult_ParamDesc
            //                    {
            //                        AcademicYear = x.Grade.Level.AcademicYear.Description,
            //                        Grade = x.Grade.Description,
            //                        Semester = param.Semester,
            //                        Homeroom = param.IdHomeroom == "all" ? "ALL" : string.Format("{0}{1}", x.Grade.Code, x.GradePathwayClassroom.Classroom.Code)
            //                    })
            //                    .FirstOrDefault();

            var title = "StudentParticipant";

            // result
            var resultList = new List<ExportExcelStudentExtracurricularResult>();

            var allStudentExtracurricularPayload = await _studentExtracurricularApi.GetStudentExtracurricular(new GetStudentExtracurricularRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                IdLevel = param.IdLevel,
                IdGrade = param.IdGrade,
                Semester = param.Semester,
                IdHomeroom = param.IdHomeroom,
                GetAll = true
            });

            if (allStudentExtracurricularPayload.Payload?.Count() > 0)
            {
                var allStudentExtracurricularList = allStudentExtracurricularPayload.Payload.ToList();

                foreach (var student in allStudentExtracurricularList)
                {
                    var studentParticipantDetailPayload = await _studentExtracurricularApi.GetDetailStudentExtracurricular(new GetDetailStudentExtracurricularRequest
                    {
                        IdAcademicYear = param.IdAcademicYear,
                        IdLevel = student.IdLevel,
                        IdGrade = student.IdGrade,
                        Semester = param.Semester,
                        IdStudent = student.Student.Id
                    });

                    var studentExtracurricularDetailList = new List<GetDetailStudentExtracurricularResult_Extracurricular>();
                    if (studentParticipantDetailPayload.Payload != null)
                    {
                        var extracurricularList = studentParticipantDetailPayload.Payload.StudentExtracurricularList;

                        studentExtracurricularDetailList = extracurricularList;
                    }
                    else
                    {
                        studentExtracurricularDetailList = null;
                    }

                    var result = new ExportExcelStudentExtracurricularResult
                    {
                        Homeroom = new NameValueVm
                        {
                            Id = student.Homeroom.Id,
                            Name = student.Homeroom.Name
                        },
                        Student = new NameValueVm
                        {
                            Id = student.Student.Id,
                            Name = student.Student.Name
                        },
                        ExtracurricularList = studentExtracurricularDetailList,
                        //PrimaryExtracurricular = new NameValueVm
                        //{
                        //    Id = student.PrimaryExtracurricular?.Id,
                        //    Name = student.PrimaryExtracurricular?.Name
                        //}
                    };

                    resultList.Add(result);
                }

                // generate excel
                var generateExcelByte = GenerateExcel(paramDesc, resultList, title);
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

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }

        public byte[] GenerateBlankExcel(string sheetTitle)
        {
            var result = new byte[0];
            //string[] fieldDataList = fieldData.Split(",");

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

        public byte[] GenerateExcel(ExportExcelStudentExtracurricularResult_ParamDesc paramDesc, List<ExportExcelStudentExtracurricularResult> dataList, string sheetTitle)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet(sheetTitle);
                excelSheet.SetColumnWidth(1, 30 * 256);
                excelSheet.SetColumnWidth(2, 30 * 256);
                excelSheet.SetColumnWidth(3, 30 * 256);
                excelSheet.SetColumnWidth(4, 30 * 256);
                excelSheet.SetColumnWidth(5, 30 * 256);
                excelSheet.SetColumnWidth(6, 30 * 256);

                //Create style
                ICellStyle style = workbook.CreateCellStyle();
                style.WrapText = true;
                style.VerticalAlignment = VerticalAlignment.Center;

                ICellStyle styleTable = workbook.CreateCellStyle();
                styleTable.BorderBottom = BorderStyle.Thin;
                styleTable.BorderLeft = BorderStyle.Thin;
                styleTable.BorderRight = BorderStyle.Thin;
                styleTable.BorderTop = BorderStyle.Thin;
                styleTable.VerticalAlignment = VerticalAlignment.Center;
                styleTable.WrapText = true;

                ICellStyle styleHeader = workbook.CreateCellStyle();
                styleHeader.WrapText = true;
                styleHeader.VerticalAlignment = VerticalAlignment.Center;

                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;
                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.WrapText = true;


                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                //font.IsItalic = true;
                font.FontName = "Arial";
                font.FontHeightInPoints = 12;
                style.SetFont(font);

                styleTable.SetFont(font);

                IFont fontHeader = workbook.CreateFont();
                fontHeader.FontName = "Arial";
                fontHeader.FontHeightInPoints = 12;
                fontHeader.IsBold = true;
                styleHeader.SetFont(fontHeader);

                styleHeaderTable.SetFont(fontHeader);

                int rowIndex = 0;
                int startColumn = 1;

                //Title 
                rowIndex++;
                IRow titleRow = excelSheet.CreateRow(rowIndex);
                var cellTitleRow = titleRow.CreateCell(1);
                cellTitleRow.SetCellValue("Student Electives Summary");
                cellTitleRow.CellStyle = styleHeader;

                //Parameter
                rowIndex += 2;
                string[] fieldDataList = new string[4] { "Academic Year", "Grade", "Semester", "Homeroom" };
                foreach (string field in fieldDataList)
                {
                    IRow paramRow = excelSheet.CreateRow(rowIndex);
                    var cellParamRow = paramRow.CreateCell(1);
                    cellParamRow.SetCellValue(field);
                    cellParamRow.CellStyle = styleHeader;
                    var cellValueParamRow = paramRow.CreateCell(2);
                    cellValueParamRow.CellStyle = style;

                    if (field == "Academic Year")
                        cellValueParamRow.SetCellValue(paramDesc.AcademicYear);
                    if (field == "Grade")
                        cellValueParamRow.SetCellValue(paramDesc.Grade);
                    if (field == "Semester")
                        cellValueParamRow.SetCellValue(paramDesc.Semester);
                    if (field == "Homeroom")
                        cellValueParamRow.SetCellValue(paramDesc.Homeroom);
                    rowIndex++;
                }

                rowIndex += 2;

                // summary content
                var headerList = new string[5] { "Homeroom", "Student ID", "Student Name",  "Electives Joined", "Primary Electives" };

                int startColumnHeader = startColumn;
                int startColumnContent = startColumn;

                IRow sumRow2 = excelSheet.CreateRow(rowIndex);
                foreach (string header in headerList)
                {
                    var cellLevelHeader = sumRow2.CreateCell(startColumnHeader);
                    cellLevelHeader.SetCellValue(header);
                    cellLevelHeader.CellStyle = styleHeaderTable;

                    startColumnHeader++;
                }

                rowIndex++;

                if (dataList != null)
                {
                    foreach (var itemData in dataList)
                    {
                        int firstSubItemRow = rowIndex;
                        int lastSubItemRow;
                        int rowSubItem = firstSubItemRow;

                        IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                        // List Extracurricular Joined
                        if(itemData.ExtracurricularList == null)
                        {
                            totalRow2 = excelSheet.CreateRow(rowSubItem);

                            var cellListExtracurricular = totalRow2.CreateCell(4);
                            cellListExtracurricular.SetCellValue("-");
                            cellListExtracurricular.CellStyle = styleTable;

                            // create border in empty cell
                            var cellEmpty = totalRow2.CreateCell(1);
                            cellEmpty.CellStyle = styleTable;

                            cellEmpty = totalRow2.CreateCell(2);
                            cellEmpty.CellStyle = styleTable;

                            cellEmpty = totalRow2.CreateCell(3);
                            cellEmpty.CellStyle = styleTable;

                            cellEmpty = totalRow2.CreateCell(5);
                            cellEmpty.CellStyle = styleTable;

                            rowSubItem++;
                        }
                        else
                        {
                            foreach (var subItem in itemData.ExtracurricularList)
                            {
                                totalRow2 = excelSheet.CreateRow(rowSubItem);

                                var cellListExtracurricular = totalRow2.CreateCell(4);
                                cellListExtracurricular.SetCellValue(subItem.Extracurricular.Name);
                                cellListExtracurricular.CellStyle = styleTable;

                                var cellPrimaryExtracurricular = totalRow2.CreateCell(5);
                                cellPrimaryExtracurricular.SetCellValue(subItem.IsPrimary == false ? "" : "v");
                                cellPrimaryExtracurricular.CellStyle = styleTable;

                                // create border in empty cell
                                var cellEmpty = totalRow2.CreateCell(1);
                                cellEmpty.CellStyle = styleTable;

                                cellEmpty = totalRow2.CreateCell(2);
                                cellEmpty.CellStyle = styleTable;

                                cellEmpty = totalRow2.CreateCell(3);
                                cellEmpty.CellStyle = styleTable;

                                rowSubItem++;
                            }
                        }

                        lastSubItemRow = rowSubItem - 1;

                        totalRow2 = excelSheet.GetRow(firstSubItemRow);

                        var cellHomeroom = totalRow2.CreateCell(1);
                        cellHomeroom.SetCellValue(itemData.Homeroom.Name);
                        if(firstSubItemRow != lastSubItemRow)
                        {
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 1, 1));
                        }
                        cellHomeroom.CellStyle = styleTable;

                        var cellIdStudent = totalRow2.CreateCell(2);
                        cellIdStudent.SetCellValue(itemData.Student.Id);
                        if (firstSubItemRow != lastSubItemRow)
                        {
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 2, 2));
                        }
                        cellIdStudent.CellStyle = styleTable;

                        var cellStudentName = totalRow2.CreateCell(3);
                        cellStudentName.SetCellValue(itemData.Student.Name);
                        if (firstSubItemRow != lastSubItemRow)
                        {
                            excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 3, 3));
                        }
                        cellStudentName.CellStyle = styleTable;
                        
                        //var cellPrimaryExtracurricular = totalRow2.CreateCell(5);
                        //cellPrimaryExtracurricular.SetCellValue(itemData.PrimaryExtracurricular.Name == null ? "-" : itemData.PrimaryExtracurricular.Name);
                        //if (firstSubItemRow != lastSubItemRow)
                        //{
                        //    excelSheet.AddMergedRegion(new CellRangeAddress(firstSubItemRow, lastSubItemRow, 5, 5));
                        //}
                        //cellPrimaryExtracurricular.CellStyle = styleTable;

                        rowIndex = lastSubItemRow;
                        rowIndex++;
                    }
                }
                else
                {
                    IRow totalRow2 = excelSheet.CreateRow(rowIndex);

                    for (int i = 1; i <= 5; i++)
                    {
                        var cellBlank = totalRow2.CreateCell(i);
                        cellBlank.SetCellValue("");
                        cellBlank.CellStyle = styleTable;
                    }

                    rowIndex++;
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
