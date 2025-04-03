using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData
{
    public class StudentGenderDemographicsExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentDemographicsReport _studentDemographicsReport;

        public StudentGenderDemographicsExcelHandler(IStudentDbContext dbContext, IStudentDemographicsReport studentDemographicsReport)
        {
            _dbContext = dbContext;
            _studentDemographicsReport = studentDemographicsReport;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MasterStudentDemographicsGenerateExcelRequest, MasterStudentDemographicsGenerateExcelValidator>();

            var result = await GetStudentGenderDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester,
                Level = body.Level,
                Grade = body.Grade,
                Homeroom = body.Homeroom,
                IsDetail = body.IsDetail,
                ViewCategoryType = body.ViewCategoryType,
                Gender = body.Gender,
                IdReportDetailType = body.IdReportDetailType
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<MasterStudentDemographicsGenerateExcelResult> GetStudentGenderDemographicsReport(MasterStudentDemographicsGenerateExcelRequest body)
        {
            var paramDesc = await _dbContext.Entity<MsAcademicYear>()
                .Include(a => a.MsSchool)
                .Where(a => a.Id == body.IdAcademicYear)
                .Select(a => new StudentDemographicParameterDescriptionResult
                {
                    School = a.MsSchool.Name,
                    AcademicYear = a.Code
                })
                .FirstOrDefaultAsync(CancellationToken);

            paramDesc.Semester = body.Semester;
            paramDesc.ViewCategoryType = body.ViewCategoryType;

            if (body.IsDetail == false)
            {
                var getStudentGenderList = new List<GetStudentGenderDemographyResult>();

                var getStudentGender = await _studentDemographicsReport.GetStudentGenderDemography(new GetStudentGenderDemographyRequest
                {
                    IdAcademicYear = body.IdAcademicYear,
                    Grade = body.Grade,
                    Level = body.Level,
                    Semester = body.Semester,
                    ViewCategoryType = body.ViewCategoryType
                });

                getStudentGenderList.AddRange(getStudentGender.Payload);

                var getStudentGenderDetailList = new List<StudentDemographicsDetailGenerateExcelDataResult>();

                int s = 0;
                int n = 0;

                if (body.Semester == null)
                {
                    s = 1; n = 2;
                }
                else
                {
                    s = (int)body.Semester; n = (int)body.Semester;
                }

                for (int i = s; i <= n; i++)
                {
                    var getStudentGenderDetail = await _studentDemographicsReport.GetStudentGenderDemographyDetail(new GetStudentGenderDemographyDetailRequest
                    {
                        IdAcademicYear = body.IdAcademicYear,
                        Semester = i,
                        Grade = body.Grade,
                        Homeroom = body.Homeroom,
                        ViewCategoryType = body.ViewCategoryType,
                        Gender = body.Gender
                    });

                    var insertStudentGenderDetail = new StudentDemographicsDetailGenerateExcelDataResult
                    {
                        Semester = i,
                        GenderDetail = getStudentGenderDetail.Payload.ToList()
                    };

                    getStudentGenderDetailList.Add(insertStudentGenderDetail);
                }

                if (getStudentGenderList != null)
                {
                    var generateExcelByte = GenerateExcel(paramDesc, getStudentGenderList, getStudentGenderDetailList);

                    var result = new MasterStudentDemographicsGenerateExcelResult()
                    {
                        ExcelOutput = generateExcelByte
                    };

                    return result;
                }
                else
                {
                    var generateExcelByte = GenerateBlankExcel();

                    var result = new MasterStudentDemographicsGenerateExcelResult()
                    {
                        ExcelOutput = generateExcelByte
                    };

                    return result;
                }
            }
            else if (body.IsDetail == true)
            {
                var getStudentGenderDetailList = new List<GetStudentGenderDemographyDetailResult>();

                var getStudentGenderDetail = await _studentDemographicsReport.GetStudentGenderDemographyDetail(new GetStudentGenderDemographyDetailRequest
                {
                    IdAcademicYear = body.IdAcademicYear,
                    Semester = (int)body.Semester,
                    Grade = body.Grade,
                    Homeroom = body.Homeroom,
                    ViewCategoryType = body.ViewCategoryType,
                    Gender = body.Gender
                });

                getStudentGenderDetailList.AddRange(getStudentGenderDetail.Payload);

                if (getStudentGenderDetailList != null)
                {
                    var generateExcelByte = GenerateExcelDetail(paramDesc, getStudentGenderDetailList);

                    var result = new MasterStudentDemographicsGenerateExcelResult()
                    {
                        ExcelOutput = generateExcelByte
                    };

                    return result;
                }
                else
                {
                    var generateExcelByte = GenerateBlankExcel();

                    var result = new MasterStudentDemographicsGenerateExcelResult()
                    {
                        ExcelOutput = generateExcelByte
                    };

                    return result;
                }
            }

            return null;
        }

        public byte[] GenerateBlankExcel()
        {
            var result = new byte[0];

            var pattern = "[/\\\\:*?<>|\"]";
            var regex = new Regex(pattern);
            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet excelSheet = workbook.CreateSheet("Nationality");

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

        public byte[] GenerateExcel(StudentDemographicParameterDescriptionResult paramDesc, List<GetStudentGenderDemographyResult> data, List<StudentDemographicsDetailGenerateExcelDataResult> detail)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var genderSheetName = "Student Gender";
                ISheet excelSheet = workbook.CreateSheet(genderSheetName);

                //Create Style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleHeader = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                styleHeader.BorderBottom = BorderStyle.Thin;
                styleHeader.BorderLeft = BorderStyle.Thin;
                styleHeader.BorderRight = BorderStyle.Thin;
                styleHeader.BorderTop = BorderStyle.Thin;

                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Calibri";
                font.FontHeightInPoints = 11;
                //font.IsItalic = true;

                IFont fontHeaderTable = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                fontHeaderTable.FontName = "Calibri";
                fontHeaderTable.FontHeightInPoints = 11;
                fontHeaderTable.IsBold = true;

                style.SetFont(font);
                styleHeader.SetFont(fontHeaderTable);
                styleHeaderTable.SetFont(font);

                style.VerticalAlignment = VerticalAlignment.Center;
                style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;

                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.Alignment = HorizontalAlignment.Center;
                //styleHeaderTable.WrapText = true;

                styleHeader.Alignment = HorizontalAlignment.Center;

                int rowCell = 0;

                foreach (var gender in data)
                {
                    //Title 
                    IRow titleRow = excelSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Student Gender Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = excelSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {gender.Semester}");
                    rowCell++;

                    //Summary Header
                    var headerList = new string[] { "Year Level", "Male", "Female", "Total"};

                    int indexHeader = 0;
                    IRow rowHeader = excelSheet.CreateRow(rowCell);
                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    excelSheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    excelSheet.AddMergedRegion(new CellRangeAddress(rowCell - 1, rowCell - 1, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = secondTitleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = secondTitleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    rowCell++;

                    //Summary Value
                    int indexSummaryValue = 0;
                    foreach (var item in gender.DataList)
                    {
                        IRow rowValue = excelSheet.CreateRow(rowCell + indexSummaryValue);

                        ICell cellYearLevel = rowValue.CreateCell(0);
                        cellYearLevel.SetCellValue(item.CategoryType.Description);
                        cellYearLevel.CellStyle = style;

                        ICell cellMale = rowValue.CreateCell(1);
                        cellMale.SetCellValue(item.Male);
                        cellMale.CellStyle = style;

                        ICell cellFemale = rowValue.CreateCell(2);
                        cellFemale.SetCellValue(item.Female);
                        cellFemale.CellStyle = style;

                        ICell cellTotal = rowValue.CreateCell(3);
                        cellTotal.SetCellValue(item.Total);
                        cellTotal.CellStyle = style;

                        indexSummaryValue++;
                    }

                    IRow rowValueTotal = excelSheet.CreateRow(rowCell + indexSummaryValue);

                    ICell cellTotalStudent = rowValueTotal.CreateCell(0);
                    cellTotalStudent.SetCellValue("Total Students");
                    cellTotalStudent.CellStyle = style;

                    int indexTotalGenderValue = 1;

                    ICell cellTotalMale = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalMale.SetCellValue(gender.TotalStudent.Male);
                    cellTotalMale.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalFemale = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalFemale.SetCellValue(gender.TotalStudent.Female);
                    cellTotalFemale.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalValue = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalValue.SetCellValue(gender.TotalStudent.Total);
                    cellTotalValue.CellStyle = style;

                    for (int columnIndex = 0; columnIndex <= indexTotalGenderValue; columnIndex++)
                    {
                        excelSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }

                foreach (var itemDetail in detail)
                {
                    ISheet genderDetailSheetName = workbook.CreateSheet("Semester " + itemDetail.Semester); ;

                    //Summary Header
                    var headerList = new string[] { "No", "Student ID", "Student Name", "Year Level", "Homeroom Class", "Streaming/Pathway", "Gender" };

                    int indexHeader = 0;
                    IRow rowHeader = genderDetailSheetName.CreateRow(0);
                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    //Summary Value
                    int indexSummaryValue = 0;
                    int number = 1;

                    foreach (var item in itemDetail.GenderDetail)
                    {
                        IRow rowValue = genderDetailSheetName.CreateRow(1 + indexSummaryValue);

                        ICell cellNumber = rowValue.CreateCell(0);
                        cellNumber.SetCellValue(number);
                        cellNumber.CellStyle = style;

                        ICell cellIdStudent = rowValue.CreateCell(1);
                        cellIdStudent.SetCellValue(item.Student.Id);
                        cellIdStudent.CellStyle = style;

                        ICell cellStudentName = rowValue.CreateCell(2);
                        cellStudentName.SetCellValue(item.Student.Name);
                        cellStudentName.CellStyle = style;

                        ICell cellGrade = rowValue.CreateCell(3);
                        cellGrade.SetCellValue(item.Grade.Description);
                        cellGrade.CellStyle = style;

                        ICell cellHomeroom = rowValue.CreateCell(4);
                        cellHomeroom.SetCellValue(item.Homeroom.Description);
                        cellHomeroom.CellStyle = style;

                        ICell cellPathway = rowValue.CreateCell(5);
                        cellPathway.SetCellValue(item.Streaming.Description);
                        cellPathway.CellStyle = style;

                        ICell cellGender = rowValue.CreateCell(6);
                        cellGender.SetCellValue(item.Gender.ToString());
                        cellGender.CellStyle = style;

                        indexSummaryValue++;
                        number++;
                    }

                    for (int columnIndex = 0; columnIndex <= indexSummaryValue; columnIndex++)
                    {
                        genderDetailSheetName.AutoSizeColumn(columnIndex);
                    }
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcelDetail(StudentDemographicParameterDescriptionResult paramDesc, List<GetStudentGenderDemographyDetailResult> data)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var genderSheetName = regex.Replace(paramDesc.Semester.ToString(), " ");
                genderSheetName = "Semester " + genderSheetName;
                ISheet excelSheet = workbook.CreateSheet(genderSheetName);

                //Create Style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

                styleHeaderTable.BorderBottom = BorderStyle.Thin;
                styleHeaderTable.BorderLeft = BorderStyle.Thin;
                styleHeaderTable.BorderRight = BorderStyle.Thin;
                styleHeaderTable.BorderTop = BorderStyle.Thin;

                //Set font style
                IFont font = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                font.FontName = "Calibri";
                font.FontHeightInPoints = 11;
                //font.IsItalic = true;

                IFont fontHeaderTable = workbook.CreateFont();
                // font.Color = HSSFColor.Red.Index;
                fontHeaderTable.FontName = "Calibri";
                fontHeaderTable.FontHeightInPoints = 11;
                fontHeaderTable.IsBold = true;

                style.SetFont(font);
                styleHeaderTable.SetFont(font);

                style.VerticalAlignment = VerticalAlignment.Center;
                style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;

                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.Alignment = HorizontalAlignment.Center;
                //styleHeaderTable.WrapText = true;

                //Summary Header
                var headerList = new string[] { "No", "Student ID", "Student Name", "Year Level", "Homeroom Class", "Streaming/Pathway", "Gender" };

                int indexHeader = 0;
                IRow rowHeader = excelSheet.CreateRow(0);
                foreach (string header in headerList)
                {
                    ICell cellHeader = rowHeader.CreateCell(indexHeader);
                    cellHeader.SetCellValue(header);
                    cellHeader.CellStyle = styleHeaderTable;
                    indexHeader++;
                }

                //Summary Value
                int indexSummaryValue = 0;
                int number = 1;

                foreach (var item in data)
                {
                    IRow rowValue = excelSheet.CreateRow(1 + indexSummaryValue);

                    ICell cellNumber = rowValue.CreateCell(0);
                    cellNumber.SetCellValue(number);
                    cellNumber.CellStyle = style;

                    ICell cellIdStudent = rowValue.CreateCell(1);
                    cellIdStudent.SetCellValue(item.Student.Id);
                    cellIdStudent.CellStyle = style;

                    ICell cellStudentName = rowValue.CreateCell(2);
                    cellStudentName.SetCellValue(item.Student.Name);
                    cellStudentName.CellStyle = style;

                    ICell cellGrade = rowValue.CreateCell(3);
                    cellGrade.SetCellValue(item.Grade.Description);
                    cellGrade.CellStyle = style;

                    ICell cellHomeroom = rowValue.CreateCell(4);
                    cellHomeroom.SetCellValue(item.Homeroom.Description);
                    cellHomeroom.CellStyle = style;

                    ICell cellPathway = rowValue.CreateCell(5);
                    cellPathway.SetCellValue(item.Streaming.Description);
                    cellPathway.CellStyle = style;

                    ICell cellGender = rowValue.CreateCell(6);
                    cellGender.SetCellValue(item.Gender.ToString());
                    cellGender.CellStyle = style;

                    indexSummaryValue++;
                    number++;
                }

                for (int columnIndex = 0; columnIndex <= indexSummaryValue; columnIndex++)
                {
                    excelSheet.AutoSizeColumn(columnIndex);
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
