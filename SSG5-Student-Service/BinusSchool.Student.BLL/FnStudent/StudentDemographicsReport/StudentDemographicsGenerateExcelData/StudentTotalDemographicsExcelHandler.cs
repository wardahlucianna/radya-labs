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
    public class StudentTotalDemographicsExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly IStudentDemographicsReport _studentDemographicsReport;

        public StudentTotalDemographicsExcelHandler(IStudentDbContext dbContext, IStudentDemographicsReport studentDemographicsReport)
        {
            _dbContext = dbContext;
            _studentDemographicsReport = studentDemographicsReport;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var body = await Request.ValidateBody<MasterStudentDemographicsGenerateExcelRequest, MasterStudentDemographicsGenerateExcelValidator>();

            var result = await GetStudentTotalDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

        public async Task<MasterStudentDemographicsGenerateExcelResult> GetStudentTotalDemographicsReport(MasterStudentDemographicsGenerateExcelRequest param)
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
                var getStudentTotalData = new GetSDRTotalStudentReportsResult();

                var getStudentTotal = await _studentDemographicsReport.GetStudentDemographicsReportTotalStudent(new GetSDRTotalStudentReportsRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = param.Semester,
                    Level = param.Level,
                    Grade = param.Grade,
                    ViewCategoryType = param.ViewCategoryType
                });

                getStudentTotalData.SemestersData = getStudentTotal.Payload.SemestersData;
                getStudentTotalData.ViewCategoryType = getStudentTotal.Payload.ViewCategoryType;

                var getStudentTotalDetailList = new List<StudentDemographicsDetailGenerateExcelDataResult>();

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
                    var getStudentTotalDetail = await _studentDemographicsReport.GetStudentDemographicsReportTotalDetailsStudent(new GetSDRTotalStudentReportDetailsRequest
                    {
                        IdAcademicYear = param.IdAcademicYear,
                        Semester = i,
                        Grade = param.Grade,
                        Homeroom = param.Homeroom,
                        ViewCategoryType = param.ViewCategoryType,
                        IdType = param.IdReportDetailType,
                        GetAll = true
                    });

                    var insertStudentTotalDetail = new StudentDemographicsDetailGenerateExcelDataResult
                    {
                        Semester = i,
                        TotalStudentDetail = getStudentTotalDetail.Payload.ToList()
                    };

                    getStudentTotalDetailList.Add(insertStudentTotalDetail);
                }

                if (getStudentTotalData != null)
                {
                    var getExcelByte = GenerateExcel(paramDesc, getStudentTotalData, getStudentTotalDetailList);

                    var result = new MasterStudentDemographicsGenerateExcelResult
                    {
                        ExcelOutput = getExcelByte
                    };

                    return result;
                }
                else
                {
                    var getExcelByte = GenerateBlankExcel();

                    var result = new MasterStudentDemographicsGenerateExcelResult
                    {
                        ExcelOutput = getExcelByte
                    };

                    return result;
                }
            }
            else
            {
                var getStudentTotalDetailData = new List<GetSDRTotalStudentReportDetailsResult>();

                var getStudentTotalDetail = await _studentDemographicsReport.GetStudentDemographicsReportTotalDetailsStudent(new GetSDRTotalStudentReportDetailsRequest
                {
                    IdAcademicYear = param.IdAcademicYear,
                    Semester = (int)param.Semester,
                    Grade = param.Grade,
                    Homeroom = param.Homeroom,
                    ViewCategoryType = param.ViewCategoryType,
                    IdType = param.IdReportDetailType,
                    GetAll = true
                });

                getStudentTotalDetailData.AddRange(getStudentTotalDetail.Payload);

                if (getStudentTotalDetailData != null)
                {
                    var getExcelByte = GenerateExcelDetail(paramDesc, getStudentTotalDetailData);

                    var result = new MasterStudentDemographicsGenerateExcelResult
                    {
                        ExcelOutput = getExcelByte
                    };

                    return result;
                }
                else
                {
                    var getExcelByte = GenerateBlankExcel();

                    var result = new MasterStudentDemographicsGenerateExcelResult
                    {
                        ExcelOutput = getExcelByte
                    };

                    return result;
                }
            }
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

        public byte[] GenerateExcel(StudentDemographicParameterDescriptionResult paramDesc, GetSDRTotalStudentReportsResult data, List<StudentDemographicsDetailGenerateExcelDataResult> detail)
        {
            var result = new byte[0];

            using (var ms = new MemoryStream())
            {
                IWorkbook workbook;
                workbook = new XSSFWorkbook();

                var pattern = "[/\\\\:*?<>|\"]";
                var regex = new Regex(pattern);
                var studentTotalSheetName = "Student Total";
                ISheet excelSheet = workbook.CreateSheet(studentTotalSheetName);

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

                foreach (var studentTotal in data.SemestersData)
                {
                    //Title 
                    IRow titleRow = excelSheet.CreateRow(rowCell);
                    var cellTitleRow = titleRow.CreateCell(0);
                    cellTitleRow.SetCellValue($"Total Student Reports - BINUS SCHOOL {paramDesc.School}");
                    rowCell++;

                    IRow secondTitleRow = excelSheet.CreateRow(rowCell);
                    var secondCellTitleRow = secondTitleRow.CreateCell(0);
                    secondCellTitleRow.SetCellValue($"Academic Year : {paramDesc.AcademicYear} / Semester : {studentTotal.Semester}");
                    rowCell++;

                    //Summary Header
                    int indexHeader = 0;
                    IRow rowHeader = excelSheet.CreateRow(rowCell);

                    var headerList = new string[] { "Year Level", "Internal Intake", "External Intake", "Inactive", "Withdrawal Process", "Transfer (New Student)", "Active", "Withdrawn", "Total Students"};

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
                    foreach (var item in studentTotal.ListData)
                    {
                        IRow rowValue = excelSheet.CreateRow(rowCell + indexSummaryValue);

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

                    IRow rowValueTotal = excelSheet.CreateRow(rowCell + indexSummaryValue);

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
                        excelSheet.AutoSizeColumn(columnIndex);
                    }

                    rowCell = (rowCell + indexSummaryValue) + 2;
                }

                foreach (var itemDetail in detail)
                {
                    ISheet totalStudentDetail = workbook.CreateSheet("Semester " + itemDetail.Semester);

                    //Summary Header
                    var headerList = new string[] { "No", "Student ID", "Student Name", "Year Level", "Homeroom Class", "Streaming/Pathway", "Join To School Date" };

                    int indexHeader = 0;
                    IRow rowHeader = totalStudentDetail.CreateRow(0);
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

                    foreach (var item in itemDetail.TotalStudentDetail)
                    {
                        IRow rowValue = totalStudentDetail.CreateRow(1 + indexSummaryValue);

                        ICell cellNumber = rowValue.CreateCell(0);
                        cellNumber.SetCellValue(number);
                        cellNumber.CellStyle = style;

                        ICell cellIdStudent = rowValue.CreateCell(1);
                        cellIdStudent.SetCellValue(item.Student.Id);
                        cellIdStudent.CellStyle = style;

                        ICell cellStudentName = rowValue.CreateCell(2);
                        cellStudentName.SetCellValue(item.Student.Description);
                        cellStudentName.CellStyle = style;

                        ICell cellGrade = rowValue.CreateCell(3);
                        cellGrade.SetCellValue(item.Level.Description);
                        cellGrade.CellStyle = style;

                        ICell cellHomeroom = rowValue.CreateCell(4);
                        cellHomeroom.SetCellValue(item.Homeroom.Description);
                        cellHomeroom.CellStyle = style;

                        ICell cellPathway = rowValue.CreateCell(5);
                        cellPathway.SetCellValue(item.Streaming?.Description ?? "-");
                        cellPathway.CellStyle = style;

                        ICell cellStudentReligion = rowValue.CreateCell(6);
                        cellStudentReligion.SetCellValue(item.JoinToSchoolDate);
                        cellStudentReligion.CellStyle = style;

                        indexSummaryValue++;
                        number++;
                    }

                    for (int columnIndex = 0; columnIndex <= indexSummaryValue; columnIndex++)
                    {
                        totalStudentDetail.AutoSizeColumn(columnIndex);
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

        public byte[] GenerateExcelDetail(StudentDemographicParameterDescriptionResult paramDesc, List<GetSDRTotalStudentReportDetailsResult> data)
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
                var headerList = new string[] { "No", "Student ID", "Student Name", "Year Level", "Homeroom Class", "Streaming/Pathway", "Join To School Date"};

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
                    cellStudentName.SetCellValue(item.Student.Description);
                    cellStudentName.CellStyle = style;

                    ICell cellGrade = rowValue.CreateCell(3);
                    cellGrade.SetCellValue(item.Level.Description);
                    cellGrade.CellStyle = style;

                    ICell cellHomeroom = rowValue.CreateCell(4);
                    cellHomeroom.SetCellValue(item.Homeroom.Description);
                    cellHomeroom.CellStyle = style;

                    ICell cellPathway = rowValue.CreateCell(5);
                    cellPathway.SetCellValue(item.Streaming?.Description ?? "-");
                    cellPathway.CellStyle = style;

                    ICell cellStudentReligion = rowValue.CreateCell(6);
                    cellStudentReligion.SetCellValue(item.JoinToSchoolDate);
                    cellStudentReligion.CellStyle = style;

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
