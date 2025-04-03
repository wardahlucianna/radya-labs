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
    public class GenerateAllStudentDemograhicsExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentDemographicsReport _studentDemographicsReport;

        public GenerateAllStudentDemograhicsExcelHandler(IStudentDbContext dbContext, IStudentDemographicsReport studentDemographicsReport)
        {
            _dbContext = dbContext;
            _studentDemographicsReport = studentDemographicsReport;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MasterStudentDemographicsGenerateExcelRequest, MasterStudentDemographicsGenerateExcelValidator>();

            var result = await GenerateAllStudentDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

        public async Task<MasterStudentDemographicsGenerateExcelResult> GenerateAllStudentDemographicsReport(MasterStudentDemographicsGenerateExcelRequest body)
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

            var getStudentNationalityList = new List<GetStudentNationalityDemographyResult>();

            var getStudentNationality = await _studentDemographicsReport.GetStudentNationalityDemography(new GetStudentNationalityDemographyRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                IdGrade = body.Grade,
                IdLevel = body.Level,
                Semester = body.Semester,
                ViewCategoryType = body.ViewCategoryType
            });

            getStudentNationalityList.AddRange(getStudentNationality.Payload);

            var getStudentReligionData = new List<GetSDRReligionReportsResult>();

            var getStudentReligion = await _studentDemographicsReport.GetStudentDemographicsReportReligion(new GetSDRReligionReportsRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester,
                Level = body.Level,
                Grade = body.Grade,
                ViewCategoryType = body.ViewCategoryType
            });

            getStudentReligionData.AddRange(getStudentReligion.Payload);

            var getTotalFamilyData = new List<GetStudentTotalFamilyDemographicsResult>();

            var getTotalFamily = await _studentDemographicsReport.GetStudentTotalFamilyDemographics(new GetStudentTotalFamilyDemographicsRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester,
                ViewCategoryType = body.ViewCategoryType,
                Level = body.Level,
                Grade = body.Grade
            });

            getTotalFamilyData.AddRange(getTotalFamily.Payload);

            var getStudentTotalData = new GetSDRTotalStudentReportsResult();

            var getStudentTotal = await _studentDemographicsReport.GetStudentDemographicsReportTotalStudent(new GetSDRTotalStudentReportsRequest
            {
                IdAcademicYear = body.IdAcademicYear,
                Semester = body.Semester,
                Level = body.Level,
                Grade = body.Grade,
                ViewCategoryType = body.ViewCategoryType
            });

            getStudentTotalData.SemestersData = getStudentTotal.Payload.SemestersData;
            getStudentTotalData.ViewCategoryType = getStudentTotal.Payload.ViewCategoryType;

            var generateExcelByte = GenerateExcel(paramDesc, getStudentGenderList, getStudentNationalityList, getStudentReligionData, getTotalFamilyData, getStudentTotalData);

            var result = new MasterStudentDemographicsGenerateExcelResult()
            {
                ExcelOutput = generateExcelByte
            };

            return result;
        }

        public byte[] GenerateExcel(StudentDemographicParameterDescriptionResult paramDesc, List<GetStudentGenderDemographyResult> gender, List<GetStudentNationalityDemographyResult> nationality, List<GetSDRReligionReportsResult> religion, List<GetStudentTotalFamilyDemographicsResult> totalFamily, GetSDRTotalStudentReportsResult totalStudent)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                #region Style
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
                #endregion

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                ISheet genderSheet = workbook.CreateSheet("Student Gender");

                int rowCell = 0;

                #region Gender
                foreach (var item in gender)
                {
                    //Title 
                    IRow titleRow = genderSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Student Gender Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = genderSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {item.Semester}");
                    rowCell++;

                    //Summary Header
                    var headerList = new string[] { "Year Level", "Male", "Female", "Total" };

                    int indexHeader = 0;
                    IRow rowHeader = genderSheet.CreateRow(rowCell);
                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    genderSheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    genderSheet.AddMergedRegion(new CellRangeAddress(rowCell - 1, rowCell - 1, 0, indexHeader - 1));
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
                    foreach (var itemData in item.DataList)
                    {
                        IRow rowValue = genderSheet.CreateRow(rowCell + indexSummaryValue);

                        ICell cellYearLevel = rowValue.CreateCell(0);
                        cellYearLevel.SetCellValue(itemData.CategoryType.Description);
                        cellYearLevel.CellStyle = style;

                        ICell cellMale = rowValue.CreateCell(1);
                        cellMale.SetCellValue(itemData.Male);
                        cellMale.CellStyle = style;

                        ICell cellFemale = rowValue.CreateCell(2);
                        cellFemale.SetCellValue(itemData.Female);
                        cellFemale.CellStyle = style;

                        ICell cellTotal = rowValue.CreateCell(3);
                        cellTotal.SetCellValue(itemData.Total);
                        cellTotal.CellStyle = style;

                        indexSummaryValue++;
                    }

                    IRow rowValueTotal = genderSheet.CreateRow(rowCell + indexSummaryValue);

                    ICell cellTotalStudent = rowValueTotal.CreateCell(0);
                    cellTotalStudent.SetCellValue("Total Students");
                    cellTotalStudent.CellStyle = style;

                    int indexTotalGenderValue = 1;

                    ICell cellTotalMale = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalMale.SetCellValue(item.TotalStudent.Male);
                    cellTotalMale.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalFemale = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalFemale.SetCellValue(item.TotalStudent.Female);
                    cellTotalFemale.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalValue = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalValue.SetCellValue(item.TotalStudent.Total);
                    cellTotalValue.CellStyle = style;

                    for (int columnIndex = 0; columnIndex <= indexTotalGenderValue; columnIndex++)
                    {
                        genderSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }
                #endregion

                rowCell = 0;
                ISheet nationalitySheet = workbook.CreateSheet("Student Nationality");

                #region Nationality
                foreach (var item in nationality)
                {
                    //Title 
                    IRow titleRow = nationalitySheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Student Nationality Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = nationalitySheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {item.Semester}");
                    rowCell++;

                    //Summary Header
                    var headerList = new string[1] { "Year Level" };
                    var gradeHomeroom = item.Header;
                    var genderList = new string[2] { "Male", "Female" };

                    int indexHeader = 0;
                    IRow rowHeader = nationalitySheet.CreateRow(rowCell);
                    IRow rowGenderHeader = nationalitySheet.CreateRow(rowCell + 1);
                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        nationalitySheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell + 1, 0, 0));
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    foreach (string header in gradeHomeroom)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        nationalitySheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 1));
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

                        foreach (string genderData in genderList)
                        {
                            ICell cellGender = rowGenderHeader.CreateCell(genderIndex);
                            cellGender.SetCellValue(genderData);
                            cellGender.CellStyle = styleHeaderTable;
                            genderIndex++;
                        }
                    }
                    rowCell++;

                    nationalitySheet.AddMergedRegion(new CellRangeAddress(rowCell - 3, rowCell - 3, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    nationalitySheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
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
                    nationalitySheet.AddMergedRegion(new CellRangeAddress(rowCell - 3, rowCell, indexHeader, indexHeader));
                    cellTotalHeader.CellStyle = styleHeaderTable;

                    rowCell++;
                    //Summary Value
                    int indexSummaryValue = 0;
                    foreach (var itemData in item.Body)
                    {
                        IRow rowValue = nationalitySheet.CreateRow(rowCell + indexSummaryValue);

                        ICell cellCountry = rowValue.CreateCell(0);
                        cellCountry.SetCellValue(itemData.Country.Description);
                        cellCountry.CellStyle = style;

                        int indexGenderValue = 1;

                        foreach (var genderData in itemData.ListData)
                        {
                            ICell cellMale = rowValue.CreateCell(indexGenderValue);
                            cellMale.SetCellValue(genderData.Male);
                            cellMale.CellStyle = styleValue;
                            indexGenderValue++;

                            ICell cellFemale = rowValue.CreateCell(indexGenderValue);
                            cellFemale.SetCellValue(genderData.Female);
                            cellFemale.CellStyle = styleValue;
                            indexGenderValue++;
                        }

                        ICell cellTotal = rowValue.CreateCell(indexGenderValue);
                        cellTotal.SetCellValue(itemData.Total);
                        cellTotal.CellStyle = styleValue;

                        indexSummaryValue++;
                    }

                    IRow rowValueTotal = nationalitySheet.CreateRow(rowCell + indexSummaryValue);

                    ICell cellTotalStudent = rowValueTotal.CreateCell(0);
                    cellTotalStudent.SetCellValue("Total");
                    cellTotalStudent.CellStyle = style;

                    int indexTotalGenderValue = 1;

                    foreach (var genderData in item.TotalStudent.ListData)
                    {
                        ICell cellMale = rowValueTotal.CreateCell(indexTotalGenderValue);
                        cellMale.SetCellValue(genderData.Male);
                        cellMale.CellStyle = styleValue;
                        indexTotalGenderValue++;

                        ICell cellFemale = rowValueTotal.CreateCell(indexTotalGenderValue);
                        cellFemale.SetCellValue(genderData.Female);
                        cellFemale.CellStyle = styleValue;
                        indexTotalGenderValue++;
                    }

                    ICell cellTotalValue = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalValue.SetCellValue(item.TotalStudent.Total);
                    cellTotalValue.CellStyle = styleValue;

                    for (int columnIndex = 0; columnIndex <= indexTotalGenderValue; columnIndex++)
                    {
                        nationalitySheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }
                #endregion

                rowCell = 0;
                ISheet religionSheet = workbook.CreateSheet("Student Religion");

                #region Religion
                foreach (var item in religion)
                {
                    //Title 
                    IRow titleRow = religionSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Student Religion Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = religionSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {item.Semester}");
                    rowCell++;

                    //Summary Header
                    var genderList = new string[2] { "Male", "Female" };

                    int indexHeader = 0;
                    IRow rowHeader = religionSheet.CreateRow(rowCell);
                    IRow rowGenderHeader = religionSheet.CreateRow(rowCell + 1);

                    ICell cellYearLevelHeader = rowHeader.CreateCell(indexHeader);
                    cellYearLevelHeader.SetCellValue("Year Level");
                    religionSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell + 1, 0, 0));
                    cellYearLevelHeader.CellStyle = styleHeaderTable;
                    indexHeader++;

                    foreach (var header in item.Header)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        religionSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell, indexHeader, indexHeader + 1));
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

                        foreach (string genderData in genderList)
                        {
                            ICell cellGender = rowGenderHeader.CreateCell(genderIndex);
                            cellGender.SetCellValue(genderData);
                            cellGender.CellStyle = styleHeaderTable;
                            genderIndex++;
                        }
                    }

                    ICell cellCurrentTotalHeader = rowHeader.CreateCell(indexHeader);
                    cellCurrentTotalHeader.SetCellValue("Current Total");
                    religionSheet.AddMergedRegion(new CellRangeAddress(rowCell, rowCell + 1, indexHeader, indexHeader));
                    cellCurrentTotalHeader.CellStyle = styleHeaderTable;
                    indexHeader++;

                    rowCell++;

                    religionSheet.AddMergedRegion(new CellRangeAddress(rowCell - 3, rowCell - 3, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    religionSheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
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
                    foreach (var itemData in item.Body)
                    {
                        IRow rowValue = religionSheet.CreateRow(rowCell + indexSummaryValue);

                        ICell cellYearLevel = rowValue.CreateCell(0);
                        cellYearLevel.SetCellValue(itemData.CategoryType.Description);
                        cellYearLevel.CellStyle = style;

                        int indexGenderValue = 1;

                        foreach (var genderData in itemData.DataList)
                        {
                            ICell cellMale = rowValue.CreateCell(indexGenderValue);
                            cellMale.SetCellValue(genderData.Male);
                            cellMale.CellStyle = styleValue;
                            indexGenderValue++;

                            ICell cellFemale = rowValue.CreateCell(indexGenderValue);
                            cellFemale.SetCellValue(genderData.Female);
                            cellFemale.CellStyle = styleValue;
                            indexGenderValue++;
                        }

                        ICell cellTotal = rowValue.CreateCell(indexGenderValue);
                        cellTotal.SetCellValue(itemData.Total);
                        cellTotal.CellStyle = styleValue;

                        indexSummaryValue++;
                    }

                    IRow rowValueTotal = religionSheet.CreateRow(rowCell + indexSummaryValue);

                    ICell cellTotalStudent = rowValueTotal.CreateCell(0);
                    cellTotalStudent.SetCellValue("Total");
                    cellTotalStudent.CellStyle = style;

                    int indexTotalGenderValue = 1;

                    foreach (var genderData in item.TotalStudent.DataList)
                    {
                        ICell cellMale = rowValueTotal.CreateCell(indexTotalGenderValue);
                        cellMale.SetCellValue(genderData.Male);
                        cellMale.CellStyle = styleValue;
                        indexTotalGenderValue++;

                        ICell cellFemale = rowValueTotal.CreateCell(indexTotalGenderValue);
                        cellFemale.SetCellValue(genderData.Female);
                        cellFemale.CellStyle = styleValue;
                        indexTotalGenderValue++;
                    }

                    ICell cellTotalValue = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalValue.SetCellValue(item.TotalStudent.Total);
                    cellTotalValue.CellStyle = styleValue;

                    for (int columnIndex = 0; columnIndex <= indexTotalGenderValue; columnIndex++)
                    {
                        religionSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }
                #endregion

                rowCell = 0;
                ISheet totalFamilySheet = workbook.CreateSheet("Student Total Family");

                #region Total Family
                foreach (var item in totalFamily)
                {
                    //Title 
                    IRow titleRow = totalFamilySheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Total Family Student Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = totalFamilySheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {item.Semester}");
                    rowCell++;

                    //Summary Header
                    int indexHeader = 0;
                    IRow rowHeader = totalFamilySheet.CreateRow(rowCell);

                    var headerList = new string[] { "Academic Year", "Semester", "Total Family" };

                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    totalFamilySheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    totalFamilySheet.AddMergedRegion(new CellRangeAddress(rowCell - 1, rowCell - 1, 0, indexHeader - 1));
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
                    IRow rowValue = totalFamilySheet.CreateRow(rowCell);

                    ICell cellAcademicYear = rowValue.CreateCell(0);
                    cellAcademicYear.SetCellValue(item.AcademicYear.Description);
                    cellAcademicYear.CellStyle = style;

                    ICell cellSemester = rowValue.CreateCell(1);
                    cellSemester.SetCellValue((int)item.Semester);
                    cellSemester.CellStyle = style;

                    ICell cellTotalFamily = rowValue.CreateCell(2);
                    cellTotalFamily.SetCellValue(item.TotalFamily);
                    cellTotalFamily.CellStyle = style;

                    indexSummaryValue++;

                    for (int columnIndex = 0; columnIndex <= 2; columnIndex++)
                    {
                        totalFamilySheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = rowCell + 2;
                }
                #endregion

                rowCell = 0;
                ISheet totalStudentSheet = workbook.CreateSheet("Student Total");

                #region Total Student
                foreach (var studentTotal in totalStudent.SemestersData)
                {
                    //Title 
                    IRow titleRow = totalStudentSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Total Student Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = totalStudentSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {studentTotal.Semester}");
                    rowCell++;

                    //Summary Header
                    int indexHeader = 0;
                    IRow rowHeader = totalStudentSheet.CreateRow(rowCell);

                    var headerList = new string[] { "Year Level", "Internal Intake", "External Intake", "Inactive", "Withdrawal Process", "Transfer (New Student)", "Active", "Withdrawn", "Total Students" };

                    foreach (string header in headerList)
                    {
                        ICell cellHeader = rowHeader.CreateCell(indexHeader);
                        cellHeader.SetCellValue(header);
                        cellHeader.CellStyle = styleHeaderTable;
                        indexHeader++;
                    }

                    totalStudentSheet.AddMergedRegion(new CellRangeAddress(rowCell - 2, rowCell - 2, 0, indexHeader - 1));
                    for (int i = 0; i <= indexHeader - 1; i++)
                    {
                        ICell cell = titleRow.GetCell(i);
                        if (cell == null)
                        {
                            cell = titleRow.CreateCell(i);
                        }
                        cell.CellStyle = styleHeader;
                    }

                    totalStudentSheet.AddMergedRegion(new CellRangeAddress(rowCell - 1, rowCell - 1, 0, indexHeader - 1));
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
                    foreach (var item in studentTotal.ListData)
                    {
                        IRow rowValue = totalStudentSheet.CreateRow(rowCell + indexSummaryValue);

                        ICell cellYearLevel = rowValue.CreateCell(0);
                        cellYearLevel.SetCellValue(item.CategoryType.Description);
                        cellYearLevel.CellStyle = style;

                        ICell cellInternalIntake = rowValue.CreateCell(1);
                        cellInternalIntake.SetCellValue(item.InternalIntake.Value);
                        cellInternalIntake.CellStyle = style;

                        ICell cellExternalIntake = rowValue.CreateCell(2);
                        cellExternalIntake.SetCellValue(item.ExternalIntake.Value);
                        cellExternalIntake.CellStyle = style;

                        ICell cellInactive = rowValue.CreateCell(3);
                        cellInactive.SetCellValue(item.Inactive.Value);
                        cellInactive.CellStyle = style;

                        ICell cellWithdrawalProcess = rowValue.CreateCell(4);
                        cellWithdrawalProcess.SetCellValue(item.WithdrawalProcess.Value);
                        cellWithdrawalProcess.CellStyle = style;

                        ICell cellTransfer = rowValue.CreateCell(5);
                        cellTransfer.SetCellValue(item.Transfer.Value);
                        cellTransfer.CellStyle = style;

                        ICell cellActive = rowValue.CreateCell(6);
                        cellActive.SetCellValue(item.Active.Value);
                        cellActive.CellStyle = style;

                        ICell cellWithdrawn = rowValue.CreateCell(7);
                        cellWithdrawn.SetCellValue(item.Withdrawn.Value);
                        cellWithdrawn.CellStyle = style;

                        ICell cellTotalStudents = rowValue.CreateCell(8);
                        cellTotalStudents.SetCellValue(item.TotalStudents.Value);
                        cellTotalStudents.CellStyle = style;

                        indexSummaryValue++;
                    }

                    IRow rowValueTotal = totalStudentSheet.CreateRow(rowCell + indexSummaryValue);

                    ICell cellTotalStudent = rowValueTotal.CreateCell(0);
                    cellTotalStudent.SetCellValue("Sub Total Students");
                    cellTotalStudent.CellStyle = style;

                    int indexTotalGenderValue = 1;

                    ICell cellTotalInternalIntake = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalInternalIntake.SetCellValue(studentTotal.RowTotal.InternalIntake.Value);
                    cellTotalInternalIntake.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalExternalIntake = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalExternalIntake.SetCellValue(studentTotal.RowTotal.ExternalIntake.Value);
                    cellTotalExternalIntake.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalInactive = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalInactive.SetCellValue(studentTotal.RowTotal.Inactive.Value);
                    cellTotalInactive.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalWithdrawalProcess = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalWithdrawalProcess.SetCellValue(studentTotal.RowTotal.WithdrawalProcess.Value);
                    cellTotalWithdrawalProcess.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalTransfer = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalTransfer.SetCellValue(studentTotal.RowTotal.Transfer.Value);
                    cellTotalTransfer.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalActive = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalActive.SetCellValue(studentTotal.RowTotal.Active.Value);
                    cellTotalActive.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellTotalWithdrawn = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellTotalWithdrawn.SetCellValue(studentTotal.RowTotal.Withdrawn.Value);
                    cellTotalWithdrawn.CellStyle = style;
                    indexTotalGenderValue++;

                    ICell cellStudentTotal = rowValueTotal.CreateCell(indexTotalGenderValue);
                    cellStudentTotal.SetCellValue(studentTotal.RowTotal.TotalStudents.Value);
                    cellStudentTotal.CellStyle = style;
                    indexTotalGenderValue++;

                    for (int columnIndex = 0; columnIndex <= indexTotalGenderValue; columnIndex++)
                    {
                        totalStudentSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }
                #endregion

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
