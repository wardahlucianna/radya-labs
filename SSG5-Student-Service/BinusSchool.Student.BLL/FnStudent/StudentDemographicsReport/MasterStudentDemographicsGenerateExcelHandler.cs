using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.StudentDemographicsGenerateExcelData;
using BinusSchool.Student.FnStudent.StudentDemographicsReport.Validator;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BinusSchool.Student.FnStudent.StudentDemographicsReport
{
    public class MasterStudentDemographicsGenerateExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;

        private readonly StudentNationalityDemographicsExcelHandler _studentNationalityDemographicsExcelHandler;
        private readonly StudentGenderDemographicsExcelHandler _studentGenderDemographicsExcelHandler;
        private readonly StudentReligionDemographicsExcelHandler _studentReligionDemographicsExcelHandler;
        private readonly StudentTotalDemographicsExcelHandler _studentTotalDemographicsExcelHandler;
        private readonly StudentTotalFamilyDemographicsExcelHandler _studentTotalFamilyDemographicsExcelHandler;
        private readonly GenerateAllStudentDemograhicsExcelHandler _generateAllStudentDemograhicsExcelHandler;

        public MasterStudentDemographicsGenerateExcelHandler(IStudentDbContext dbContext, StudentNationalityDemographicsExcelHandler studentNationalityDemographicsExcelHandler, StudentGenderDemographicsExcelHandler studentGenderDemographicsExcelHandler, StudentReligionDemographicsExcelHandler studentReligionDemographicsExcelHandler, StudentTotalDemographicsExcelHandler studentTotalDemographicsExcelHandler, StudentTotalFamilyDemographicsExcelHandler studentTotalFamilyDemographicsExcelHandler, GenerateAllStudentDemograhicsExcelHandler generateAllStudentDemograhicsExcelHandler)
        {
            _dbContext = dbContext;
            _studentNationalityDemographicsExcelHandler = studentNationalityDemographicsExcelHandler;
            _studentGenderDemographicsExcelHandler = studentGenderDemographicsExcelHandler;
            _studentReligionDemographicsExcelHandler = studentReligionDemographicsExcelHandler;
            _studentTotalDemographicsExcelHandler = studentTotalDemographicsExcelHandler;
            _studentTotalFamilyDemographicsExcelHandler = studentTotalFamilyDemographicsExcelHandler;
            _generateAllStudentDemograhicsExcelHandler = generateAllStudentDemograhicsExcelHandler;
        }

        protected override async Task<IActionResult> RawHandler()
        {
            var param = await Request.ValidateBody<MasterStudentDemographicsGenerateExcelRequest, MasterStudentDemographicsGenerateExcelValidator>();
            var retval = new MasterStudentDemographicsGenerateExcelResult();
            retval.ExcelOutput = new byte[0];

            if (param.ReportType.Count > 1)
            {
                var generateAll = await _generateAllStudentDemograhicsExcelHandler.GenerateAllStudentDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

                retval.ExcelOutput = retval.ExcelOutput.Concat(generateAll.ExcelOutput).ToArray();
                retval.FileName = "StudentDemograhicsSummary";
            }
            else
            {
                foreach (var item in param.ReportType)
                {
                    if (item == "sg")
                    {
                        var getStudentGender = await _studentGenderDemographicsExcelHandler.GetStudentGenderDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

                        retval.ExcelOutput = retval.ExcelOutput.Concat(getStudentGender.ExcelOutput).ToArray();
                        retval.FileName = param.IsDetail == false ? "StudentGender" : "StudentGenderDetail";
                    }
                    else if (item == "sn")
                    {
                        var getStudentNationality = await _studentNationalityDemographicsExcelHandler.GetStudentNationalityDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

                        retval.ExcelOutput = retval.ExcelOutput.Concat(getStudentNationality.ExcelOutput).ToArray();
                        retval.FileName = param.IsDetail == false ? "StudentNationality" : "StudentNationalityDetail";
                    }
                    else if (item == "sr")
                    {
                        var getStudentReligion = await _studentReligionDemographicsExcelHandler.GetStudentReligionDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

                        retval.ExcelOutput = retval.ExcelOutput.Concat(getStudentReligion.ExcelOutput).ToArray();
                        retval.FileName = param.IsDetail == false ? "StudentReligion" : "StudentReligionDetail";
                    }
                    else if (item == "tf")
                    {
                        var getStudentTotalFamily = await _studentTotalFamilyDemographicsExcelHandler.GetStudentTotalFamilyDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

                        retval.ExcelOutput = retval.ExcelOutput.Concat(getStudentTotalFamily.ExcelOutput).ToArray();
                        retval.FileName = param.IsDetail == false ? "StudentTotalFamily" : "StudentTotalFamilyDetail";
                    }
                    else if (item == "tnos")
                    {
                        var getStudentTotal = await _studentTotalDemographicsExcelHandler.GetStudentTotalDemographicsReport(new MasterStudentDemographicsGenerateExcelRequest
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

                        retval.ExcelOutput = retval.ExcelOutput.Concat(getStudentTotal.ExcelOutput).ToArray();
                        retval.FileName = param.IsDetail == false ? "TotalStudent" : "TotalStudentDetail";
                    }
                }
            }

            return new FileContentResult(retval.ExcelOutput, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = $"{retval.FileName}_{DateTime.Now.Ticks}.xlsx"
            };
        }

        protected override Task<ApiErrorResult<object>> Handler()
        {
            throw new NotImplementedException();
        }
        #region Not Implemented
        /*private byte[] MergeExcelBytes(List<byte[]> excelBytesList)
        {
            // Pastikan list tidak kosong
            if (excelBytesList == null || excelBytesList.Count == 0)
            {
                return null;
            }

            // Menggunakan library EPPlus untuk menggabungkan file Excel
            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                foreach (var excelBytes in excelBytesList)
                {
                    if (excelBytes != null && excelBytes.Length > 0)
                    {
                        using (var stream = new MemoryStream(excelBytes))
                        {
                            using (var workbook = new OfficeOpenXml.ExcelPackage(stream))
                            {
                                foreach (var sheet in workbook.Workbook.Worksheets)
                                {
                                    var newSheet = package.Workbook.Worksheets.Add(sheet.Name);
                                    newSheet.Cells["A1"].LoadFromCollection(sheet.Cells["A1:AZ100000"], true);
                                }
                            }
                        }
                    }
                }

                // Mengembalikan byte[] hasil gabungan
                return package.GetAsByteArray();
            }
        }*/
        #endregion
    }
}
