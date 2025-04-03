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
using BinusSchool.Common.Model.Enums;
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
    public class StudentNationalityDemographicsExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentDemographicsReport _studentDemographicsReport;

        public StudentNationalityDemographicsExcelHandler(IStudentDbContext dbContext, IStudentDemographicsReport studentDemographicsReport)
        {
            _dbContext = dbContext;
            _studentDemographicsReport = studentDemographicsReport;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBody<MasterStudentDemographicsGenerateExcelRequest, MasterStudentDemographicsGenerateExcelValidator>();

            var result = await GetStudentNationalityDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
            {
                IdAcademicYear = param.IdAcademicYear,
                Semester = param.Semester,
                Level = param.Level,
                Grade = param.Grade,
                Homeroom = param.Homeroom,
                IsDetail = param.IsDetail,
                ViewCategoryType = param.ViewCategoryType,
                Gender = param.Gender,
                IdReportDetailType = param.IdReportDetailType
            });

            return Request.CreateApiResult2(result as object);
        }

        public async Task<MasterStudentDemographicsGenerateExcelResult> GetStudentNationalityDemographicsReport(MasterStudentDemographicsGenerateExcelRequest param)
        {
            var paramDesc = await _dbContext.Entity<MsAcademicYear>()
                .Include(a => a.MsSchool)
                .Where(a => a.Id == param.IdAcademicYear)
                .Select(a => new StudentDemographicParameterDescriptionResult
                {
                    School = a.MsSchool.Name,
                    AcademicYear = a.Code
                })
                .FirstOrDefaultAsync(CancellationToken);

            paramDesc.Semester = param.Semester;
            paramDesc.ViewCategoryType = param.ViewCategoryType;

            if (param.IsDetail == false)
            {
                var getStudentNationalityList = new List<GetStudentNationalityDemographyResult>();

                var getStudentNationality = await _studentDemographicsReport.GetStudentNationalityDemography(new GetStudentNationalityDemographyRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    IdGrade = param.Grade,
                    IdLevel = param.Level,
                    Semester = param.Semester,
                    ViewCategoryType = param.ViewCategoryType
                });

                getStudentNationalityList.AddRange(getStudentNationality.Payload);

                var getStudentNationalityDetailList = new List<StudentDemographicsDetailGenerateExcelDataResult>();

                int s = 0;
                int n = 0;

                if (param.Semester == null)
                {
                    s = 1; n = 2;
                }
                else
                {
                    s = (int)param.Semester; n = (int)param.Semester;
                }

                for (int i = s; i <= n; i++)
                {
                    var getStudentNationalityDetail = await _studentDemographicsReport.GetStudentNationalityDemographyDetail(new GetStudentNationalityDemographyDetailRequest
                    {
                        IdAcademicYear = param.IdAcademicYear,
                        Semester = i,
                        Grade = param.Grade,
                        Homeroom = param.Homeroom,
                        ViewCategoryType = param.ViewCategoryType,
                        Gender = param.Gender
                    });

                    var insertStudentNationalityDetail = new StudentDemographicsDetailGenerateExcelDataResult
                    {
                        Semester = i,
                        NationalityDetail = getStudentNationalityDetail.Payload.ListData
                    };

                    getStudentNationalityDetailList.Add(insertStudentNationalityDetail);
                }

                if (getStudentNationalityList != null)
                {
                    var generateExcelByte = GenerateExcel(paramDesc, getStudentNationalityList, getStudentNationalityDetailList);

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
            else if (param.IsDetail == true)
            {
                var getStudentNationalityDetailList = new GetStudentNationalityDemographyDetailResult();

                var getStudentNationalityDetail = await _studentDemographicsReport.GetStudentNationalityDemographyDetail(new GetStudentNationalityDemographyDetailRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = (int)param.Semester,
                    Grade = param.Grade,
                    Homeroom = param.Homeroom,
                    IdCountry = param.IdReportDetailType,
                    ViewCategoryType = param.ViewCategoryType,
                    Gender = param.Gender
                });

                getStudentNationalityDetailList.Category = getStudentNationalityDetail.Payload.Category;
                getStudentNationalityDetailList.ListData = getStudentNationalityDetail.Payload.ListData;

                if (getStudentNationalityDetailList != null)
                {
                    var generateExcelByte = GenerateExcelDetail(paramDesc, getStudentNationalityDetailList);

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

        public byte[] GenerateExcel(StudentDemographicParameterDescriptionResult paramDesc, List<GetStudentNationalityDemographyResult> data, List<StudentDemographicsDetailGenerateExcelDataResult> detail)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var nationalitySheetName = "Student Nationality";
                ISheet excelSheet = workbook.CreateSheet(nationalitySheetName);

                //Create Style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleHeader = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                ICellStyle styleValue = workbook.CreateCellStyle();

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

                styleValue.BorderBottom = BorderStyle.Thin;
                styleValue.BorderLeft = BorderStyle.Thin;
                styleValue.BorderRight = BorderStyle.Thin;
                styleValue.BorderTop = BorderStyle.Thin;

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
                styleValue.SetFont(font);

                style.VerticalAlignment = VerticalAlignment.Top;
                style.WrapText = true;

                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.Alignment = HorizontalAlignment.Center;
                //styleHeaderTable.WrapText = true;

                styleValue.Alignment = HorizontalAlignment.Center;
                styleHeader.Alignment = HorizontalAlignment.Center;

                int rowCell = 0;
                foreach (var nationality in data)
                {
                    //Title 
                    IRow titleRow = excelSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Student Nationality Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = excelSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {nationality.Semester}");
                    rowCell++;

                    //Summary Header
                    var headerList = new string[1] { "Year Level" };
                    var gradeHomeroom = nationality.Header;
                    var genderList = new string[2] { "Male", "Female" };

                    int indexHeader = 0;
                    IRow rowHeader = excelSheet.CreateRow(rowCell);
                    IRow rowGenderHeader = excelSheet.CreateRow(rowCell+1);
                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        excelSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell+1, 0, 0));
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    foreach (string header in gradeHomeroom)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        excelSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 1));
                        for (int i = indexHeader; i <= indexHeader + 1; i++)
                        {
                            ICell cell = rowHeader.GetCell(i);
                            if (cell == null)
                            {
                                cell = rowHeader.CreateCell(i);
                            }
                            cell.CellStyle = styleHeaderTable;
                        }
                        int genderIndex = indexHeader;
                        indexHeader += 2;

                        foreach (string gender in genderList)
                        {
                            ICell cellGender = rowGenderHeader.CreateCell(genderIndex);
                            cellGender.SetCellValue(gender);
                            cellGender.CellStyle = styleHeaderTable;
                            genderIndex++;
                        }
                    }
                    rowCell++;

                    excelSheet.AddMergedRegion(new CellRangeAddress(rowCell - 3, rowCell - 3, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    excelSheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = secondTitleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = secondTitleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    ICell cellTotalHeader = titleRow.CreateCell(indexHeader);
                    cellTotalHeader.SetCellValue("Total Students");
                    excelSheet.AddMergedRegion(new CellRangeAddress(rowCell - 3, rowCell, indexHeader, indexHeader));
                    cellTotalHeader.CellStyle = styleHeaderTable;

                    rowCell++;
                    //Summary Value
                    int indexSummaryValue = 0;
                    foreach (var item in nationality.Body)
                    {
                        IRow rowValue = excelSheet.CreateRow(rowCell + indexSummaryValue);

                        ICell cellCountry = rowValue.CreateCell(0);
                        cellCountry.SetCellValue(item.Country.Description);
                        cellCountry.CellStyle = style;

                        int indexGenderValue = 1;

                        foreach (var gender in item.ListData)
                        {
                            ICell cellMale = rowValue.CreateCell(indexGenderValue);
                            cellMale.SetCellValue(gender.Male);
                            cellMale.CellStyle = styleValue;
                            indexGenderValue++;

                            ICell cellFemale = rowValue.CreateCell(indexGenderValue);
                            cellFemale.SetCellValue(gender.Female);
                            cellFemale.CellStyle = styleValue;
                            indexGenderValue++;
                        }

                        ICell cellTotal = rowValue.CreateCell(indexGenderValue);
                        cellTotal.SetCellValue(item.Total);
                        cellTotal.CellStyle = styleValue;

                        indexSummaryValue++;
                    }

                    IRow rowValueTotal = excelSheet.CreateRow(rowCell + indexSummaryValue);

                    ICell cellTotalStudent = rowValueTotal.CreateCell(0);
                    cellTotalStudent.SetCellValue("Total");
                    cellTotalStudent.CellStyle = style;

                    int indexTotalGenderValue = 1;

                    foreach (var gender in nationality.TotalStudent.ListData)
                    {
                        ICell cellMale = rowValueTotal.CreateCell(indexTotalGenderValue);
                        cellMale.SetCellValue(gender.Male);
                        cellMale.CellStyle = styleValue;
                        indexTotalGenderValue++;

                        ICell cellFemale = rowValueTotal.CreateCell(indexTotalGenderValue);
                        cellFemale.SetCellValue(gender.Female);
                        cellFemale.CellStyle = styleValue;
                        indexTotalGenderValue++;
                    }

                    ICell cellTotalValue = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalValue.SetCellValue(nationality.TotalStudent.Total);
                    cellTotalValue.CellStyle = styleValue;

                    for (int columnIndex = 0; columnIndex <= indexTotalGenderValue; columnIndex++)
                    {
                        excelSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }

                foreach (var itemDetail in detail)
                {
                    ISheet nationalityDetailSheet = workbook.CreateSheet("Semester " + itemDetail.Semester);

                    //Summary Header
                    var headerList = new string[] { "No", "Student ID", "Student Name", "Year Level", "Homeroom Class", "Streaming/Pathway", "Nationality Type", "Nationality Country" };

                    int indexHeader = 0;
                    IRow rowHeader = nationalityDetailSheet.CreateRow(0);
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

                    foreach (var item in itemDetail.NationalityDetail)
                    {
                        IRow rowValue = nationalityDetailSheet.CreateRow(1 + indexSummaryValue);

                        ICell cellNumber = rowValue.CreateCell(0);
                        cellNumber.SetCellValue(number);
                        cellNumber.CellStyle = style;

                        ICell cellIdStudent = rowValue.CreateCell(1);
                        cellIdStudent.SetCellValue(item.Student.Id);
                        cellIdStudent.CellStyle = style;

                        ICell cellStudentName = rowValue.CreateCell(2);
                        cellStudentName.SetCellValue(item.Student.Name);
                        cellStudentName.CellStyle = style;

                        ICell cellYearLevel = rowValue.CreateCell(3);
                        cellYearLevel.SetCellValue(item.Grade);
                        cellYearLevel.CellStyle = style;

                        ICell cellClass = rowValue.CreateCell(4);
                        cellClass.SetCellValue(item.Homeroom);
                        cellClass.CellStyle = style;

                        ICell cellPathway = rowValue.CreateCell(5);
                        cellPathway.SetCellValue(item.Pathway);
                        cellPathway.CellStyle = style;

                        ICell cellNationalityType = rowValue.CreateCell(6);
                        cellNationalityType.SetCellValue(item.NationalityType);
                        cellNationalityType.CellStyle = style;

                        ICell cellCountry = rowValue.CreateCell(7);
                        cellCountry.SetCellValue(item.NationalityCountry);
                        cellCountry.CellStyle = style;

                        indexSummaryValue++;
                        number++;
                    }

                    for (int columnIndex = 0; columnIndex <= indexSummaryValue; columnIndex++)
                    {
                        nationalityDetailSheet.AutoSizeColumn(columnIndex);
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

        public byte[] GenerateExcelDetail(StudentDemographicParameterDescriptionResult paramDesc, GetStudentNationalityDemographyDetailResult data)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var nationalitySheetName = regex.Replace(paramDesc.Semester.ToString(), " ");
                nationalitySheetName = "Semester " + nationalitySheetName;
                ISheet excelSheet = workbook.CreateSheet(nationalitySheetName);

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
                var headerList = new string[] { "No", "Student ID", "Student Name", "Year Level", "Homeroom Class", "Streaming/Pathway", "Nationality Type", "Nationality Country" };

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

                foreach (var item in data.ListData)
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

                    ICell cellYearLevel = rowValue.CreateCell(3);
                    cellYearLevel.SetCellValue(item.Grade);
                    cellYearLevel.CellStyle = style;

                    ICell cellClass = rowValue.CreateCell(4);
                    cellClass.SetCellValue(item.Homeroom);
                    cellClass.CellStyle = style;

                    ICell cellPathway = rowValue.CreateCell(5);
                    cellPathway.SetCellValue(item.Pathway);
                    cellPathway.CellStyle = style;

                    ICell cellNationalityType = rowValue.CreateCell(6);
                    cellNationalityType.SetCellValue(item.NationalityType);
                    cellNationalityType.CellStyle = style;

                    ICell cellCountry = rowValue.CreateCell(7);
                    cellCountry.SetCellValue(item.NationalityCountry);
                    cellCountry.CellStyle = style;

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
