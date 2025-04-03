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
    public class StudentTotalFamilyDemographicsExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentDemographicsReport _studentDemographicsReport;

        public StudentTotalFamilyDemographicsExcelHandler(IStudentDbContext dbContext, IStudentDemographicsReport studentDemographicsReport)
        {
            _dbContext = dbContext;
            _studentDemographicsReport = studentDemographicsReport;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MasterStudentDemographicsGenerateExcelRequest, MasterStudentDemographicsGenerateExcelValidator>();

            var result = await GetStudentTotalFamilyDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

        public async Task<MasterStudentDemographicsGenerateExcelResult> GetStudentTotalFamilyDemographicsReport(MasterStudentDemographicsGenerateExcelRequest param)
        {
            var retVal = new MasterStudentDemographicsGenerateExcelResult();

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
                var getTotalFamilyData = new List<GetStudentTotalFamilyDemographicsResult>();

                var getTotalFamily = await _studentDemographicsReport.GetStudentTotalFamilyDemographics(new GetStudentTotalFamilyDemographicsRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = param.Semester,
                    ViewCategoryType = param.ViewCategoryType,
                    Level = param.Level,
                    Grade = param.Grade
                });

                getTotalFamilyData.AddRange(getTotalFamily.Payload);

                if (getTotalFamilyData != null)
                {
                    var getExcelByte = GenerateExcel(paramDesc, getTotalFamilyData);

                    retVal.ExcelOutput = getExcelByte;
                }
                else
                {
                    var getExcelByte = GenerateBlankExcel();

                    retVal.ExcelOutput = getExcelByte;
                }
            }
            else
            {
                var getTotalFamilyDetailData = new List<GetStudentTotalFamilyDemographicsDetailResult>();

                var getTotalFamilyDetail = await _studentDemographicsReport.GetStudentTotalFamilyDemographicsDetail(new GetStudentTotalFamilyDemographicsDetailRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = (int)param.Semester,
                    ViewCategoryType = param.ViewCategoryType,
                    Grade = param.Grade,
                    Homeroom = param.Homeroom
                });

                getTotalFamilyDetailData.AddRange(getTotalFamilyDetail.Payload);

                if (getTotalFamilyDetailData != null)
                {
                    var getExcelByte = GenerateExcelDetail(paramDesc, getTotalFamilyDetailData);

                    retVal.ExcelOutput = getExcelByte;
                }
                else
                {
                    var getExcelByte = GenerateBlankExcel();

                    retVal.ExcelOutput = getExcelByte;
                }
            }

            return retVal;
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

        public byte[] GenerateExcel(StudentDemographicParameterDescriptionResult paramDesc, List<GetStudentTotalFamilyDemographicsResult> data)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var sheetName = "Student Total Family";
                ISheet excelSheet = workbook.CreateSheet(sheetName);

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

                foreach (var item in data)
                {
                    //Title 
                    IRow titleRow = excelSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Total Family Student Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = excelSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {item.Semester}");
                    rowCell++;

                    //Summary Header
                    int indexHeader = 0;
                    IRow rowHeader = excelSheet.CreateRow(rowCell);

                    var headerList = new string[] { "Academic Year", "Semester", "Total Family"};

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
                    IRow rowValue = excelSheet.CreateRow(rowCell);

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
                        excelSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = rowCell + 2;
                }

                ms.Position = 0;
                workbook.Write(ms);

                result = ms.ToArray();
                ms.Close();
                ms.Dispose();
                return result;
            }
        }

        public byte[] GenerateExcelDetail(StudentDemographicParameterDescriptionResult paramDesc, List<GetStudentTotalFamilyDemographicsDetailResult> data)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var religionSheetName = regex.Replace(paramDesc.Semester.ToString(), " ");
                religionSheetName = "Semester " + religionSheetName;
                ISheet excelSheet = workbook.CreateSheet(religionSheetName);

                //Create Style
                ICellStyle style = workbook.CreateCellStyle();
                ICellStyle styleHeaderTable = workbook.CreateCellStyle();
                ICellStyle styleValue = workbook.CreateCellStyle();

                //Set border style 
                style.BorderBottom = BorderStyle.Thin;
                style.BorderLeft = BorderStyle.Thin;
                style.BorderRight = BorderStyle.Thin;
                style.BorderTop = BorderStyle.Thin;

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
                styleHeaderTable.SetFont(font);
                styleValue.SetFont(font);

                style.VerticalAlignment = VerticalAlignment.Center;
                style.Alignment = HorizontalAlignment.Center;
                style.WrapText = true;

                styleHeaderTable.VerticalAlignment = VerticalAlignment.Center;
                styleHeaderTable.Alignment = HorizontalAlignment.Center;
                //styleHeaderTable.WrapText = true;

                styleValue.VerticalAlignment = VerticalAlignment.Center;
                styleValue.Alignment = HorizontalAlignment.Left;
                styleValue.WrapText = false;

                //Summary Header
                var headerList = new string[] { "No", "Student Name", "Name of Mother", "Name of Father", "Mother Phone", "Father Phone", "Mother Email", "Father Email"};

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
                int indexSummaryValue = 1;
                int indexTemp = 1;
                int indexStudent = 1;
                int number = 1;

                foreach (var item in data)
                {
                    IRow rowValue = excelSheet.CreateRow(indexTemp);

                    ICell cellNumber = rowValue.CreateCell(0);
                    cellNumber.SetCellValue(number);
                    cellNumber.CellStyle = style;
                    
                    foreach (var student in item.SiblingData)
                    {
                        ICell cellStudent = rowValue.CreateCell(1);
                        cellStudent.SetCellValue(student.Student.Name + " (" + student.Grade.Description + ")");
                        cellStudent.CellStyle = styleValue;

                        indexStudent++;
                        indexTemp++;
                    }

                    indexTemp = indexSummaryValue;

                    CellRangeAddress numberRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 0, 0);
                    //excelSheet.AddMergedRegion(numberRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, numberRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, numberRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, numberRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, numberRange, excelSheet);

                    ICell cellMotherName = rowValue.CreateCell(2);
                    cellMotherName.SetCellValue(item.MotherName);
                    cellMotherName.CellStyle = styleValue;

                    CellRangeAddress motherNameRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 2, 2);
                    //excelSheet.AddMergedRegion(motherNameRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, motherNameRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, motherNameRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, motherNameRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, motherNameRange, excelSheet);

                    ICell cellFatherName = rowValue.CreateCell(3);
                    cellFatherName.SetCellValue(item.FatherName);
                    cellFatherName.CellStyle = styleValue;

                    CellRangeAddress fatherNameRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 3, 3);
                    //excelSheet.AddMergedRegion(fatherNameRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, fatherNameRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, fatherNameRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, fatherNameRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, fatherNameRange, excelSheet);

                    ICell cellMotherPhone = rowValue.CreateCell(4);
                    cellMotherPhone.SetCellValue(item.MotherPhone);
                    cellMotherPhone.CellStyle = styleValue;

                    CellRangeAddress motherPhoneRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 4, 4);
                    //excelSheet.AddMergedRegion(motherPhoneRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, motherPhoneRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, motherPhoneRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, motherPhoneRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, motherPhoneRange, excelSheet);

                    ICell cellFatherPhone = rowValue.CreateCell(5);
                    cellFatherPhone.SetCellValue(item.FatherPhone);
                    cellFatherPhone.CellStyle = styleValue;

                    CellRangeAddress fatherPhoneRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 5, 5);
                    //excelSheet.AddMergedRegion(fatherPhoneRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, fatherPhoneRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, fatherPhoneRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, fatherPhoneRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, fatherPhoneRange, excelSheet);

                    ICell cellMotherEmail = rowValue.CreateCell(6);
                    cellMotherEmail.SetCellValue(item.MotherEmail);
                    cellMotherEmail.CellStyle = styleValue;

                    CellRangeAddress motherEmailRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 6, 6);
                    //excelSheet.AddMergedRegion(motherEmailRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, motherEmailRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, motherEmailRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, motherEmailRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, motherEmailRange, excelSheet);

                    ICell cellFatherEmail = rowValue.CreateCell(7);
                    cellFatherEmail.SetCellValue(item.FatherEmail);
                    cellFatherEmail.CellStyle = styleValue;

                    CellRangeAddress fatherEmailRange = new CellRangeAddress(indexSummaryValue, indexStudent-1, 7, 7);
                    //excelSheet.AddMergedRegion(fatherEmailRange);

                    RegionUtil.SetBorderTop((short)BorderStyle.Thin, fatherEmailRange, excelSheet);
                    RegionUtil.SetBorderRight((short)BorderStyle.Thin, fatherEmailRange, excelSheet);
                    RegionUtil.SetBorderBottom((short)BorderStyle.Thin, fatherEmailRange, excelSheet);
                    RegionUtil.SetBorderLeft((short)BorderStyle.Thin, fatherEmailRange, excelSheet);

                    indexSummaryValue = indexStudent;
                    indexTemp = indexSummaryValue;
                    number++;
                }

                for (int columnIndex = 0; columnIndex <= 7; columnIndex++)
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
