using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs.Models;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.Finance.FnPayment;
using BinusSchool.Data.Model.Finance.FnPayment.ExtracurricularInvoice;
using BinusSchool.Data.Model.Scheduling.FnExtracurricular.MasterParticipant;
using BinusSchool.Data.Model.Teaching.FnLessonPlan.WeekSetting;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Scheduling.FnExtracurricular.MasterParticipant.Validator;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.OData.Edm;
using NPOI.SS.Formula.Functions;

namespace BinusSchool.Scheduling.FnExtracurricular.MasterParticipant
{
    public class AddStudentParticipantByExcelHandler : FunctionsHttpSingleHandler
    {
        private readonly ISchedulingDbContext _dbContext;
        private readonly IMachineDateTime _dateTime;
        private readonly IExtracurricularInvoice _extracurricularInvoiceApi;
        private IDbContextTransaction _transaction;

        public AddStudentParticipantByExcelHandler(ISchedulingDbContext dbContext, IMachineDateTime dateTime, IExtracurricularInvoice extracurricularInvoiceApi)
        {
            _dbContext = dbContext;
            _dateTime = dateTime;
            _extracurricularInvoiceApi = extracurricularInvoiceApi;
        }

        protected async override Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.ValidateBodyForm<AddStudentParticipantByExcelRequest, AddStudentParticipantByExcelValidator>();

            var res = new AddStudentParticipantByExcelResult
            {
                TotalRowData = 0,
                TotalRowSuccess = 0,
                TotalRowFailed = 0,
                ErrorList = new List<AddStudentParticipantByExcelResult_Error>()
            };

            try
            {
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var fileDataList = ExtractExcelDataForAddParticipant(Request.Form.Files.FirstOrDefault());

                var getElectiveGrade = await _dbContext.Entity<TrExtracurricularGradeMapping>()
                    .Include(x => x.Extracurricular).ThenInclude(x => x.ExtracurricularType)
                    .Include(x => x.Grade).ThenInclude(x => x.Level)
                    .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                    .ToListAsync(CancellationToken);

                if (getElectiveGrade == null)
                    throw new BadRequestException("Extracurricular Grade data not found.");

                var getElectiveData = getElectiveGrade?.FirstOrDefault().Extracurricular;

                if (getElectiveData == null)
                    throw new BadRequestException("Extracurricular data not found.");

                if(getElectiveData.Status == false)
                    throw new BadRequestException("Failed! Cannot add participant to the inactive extracurricular.");

                var getElectiveParticipant = await _dbContext.Entity<MsExtracurricularParticipant>()
                    .Where(x => x.IdExtracurricular == param.IdExtracurricular)
                    .ToListAsync(CancellationToken);

                var electiveAcademicYear = getElectiveGrade.FirstOrDefault().Grade.Level.IdAcademicYear;

                var getHomeroomStudentList = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                    .Where(x => x.Semester == getElectiveData.Semester && x.Homeroom.Grade.Level.IdAcademicYear == electiveAcademicYear)
                    .Select(x => new
                    {
                        IdSchool = x.Homeroom.Grade.Level.AcademicYear.IdSchool,
                        IdHomeroomStudent = x.Id,
                        IdHomeroom = x.Homeroom.Id,
                        IdGrade = x.Homeroom.IdGrade,
                        Semester = x.Homeroom.Semester,
                        IdStudent = x.IdStudent,
                        StudentStatus = x.Student.IdStudentStatus
                    })
                    .Distinct()
                    .ToListAsync(CancellationToken);

                var getElectiveRule = await _dbContext.Entity<TrExtracurricularRuleGradeMapping>()
                    .Include(x => x.ExtracurricularRule)
                    .Where(x => getElectiveGrade.Select(y => y.IdGrade).Contains(x.IdGrade) && x.ExtracurricularRule.Status == true)
                    .ToListAsync(CancellationToken);

                var createInvoiceDataList = new List<CreateStudentExtracurricularInvoiceRequest>();

                var TotalRowSuccess = 0;
                var TotalRowFailed = 0;
                var TotalRowData = 0;

                var ErrorList = new List<AddStudentParticipantByExcelResult_Error>();

                foreach (var fileData in fileDataList)
                {
                    var createNewExtracurricularInvoiceList = new List<CreateStudentExtracurricularInvoiceRequest_ExtracurricularData>();

                    TotalRowData++;

                    var errorData = new AddStudentParticipantByExcelResult_Error();

                    var homeroomStudentDataList = getHomeroomStudentList.Where(x => x.IdStudent == fileData.IdStudent).ToList();

                    if (!homeroomStudentDataList.Any())
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Student with ID {fileData.IdStudent} not found.");
                        continue;
                    }
                        

                    var homeroomStudentData = homeroomStudentDataList
                        .Where(x => getElectiveGrade.Select(y => y.IdGrade).Contains(x.IdGrade))
                        .FirstOrDefault();

                    if (homeroomStudentData == null)
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Student with ID {fileData.IdStudent} cannot be registered because of the elective grades does not allow it.");
                        continue;
                    }

                    if (homeroomStudentData.StudentStatus != 1)
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Student with ID {fileData.IdStudent} is not active.");
                        continue;
                    }
                        
                    //Get total participant
                    var maxElectiveParticipant = getElectiveData.MaxParticipant;
                    var electiveParticipantCount = getElectiveParticipant.Count();

                    var isStudentAlreadyParticipant = getElectiveParticipant
                        .Where(x => x.IdStudent == homeroomStudentData.IdStudent &&
                                    x.IdGrade == homeroomStudentData.IdGrade)
                        .Any();

                    if (isStudentAlreadyParticipant)
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Student with ID {fileData.IdStudent} is already a participant.");
                        continue;
                    }
                    else if (electiveParticipantCount >= maxElectiveParticipant)
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Failed! Maximum participant has been reached.");
                        continue;
                    }

                    var electiveRuleData = getElectiveRule.FirstOrDefault(x => x.IdGrade == homeroomStudentData.IdGrade);
                    if (electiveRuleData == null)
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Failed! Rule for grade {homeroomStudentData.IdGrade} not found. Please contact the staff");
                        continue;
                    }

                    bool isValidDate = DateTime.TryParseExact(fileData.JoinDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);
                    if (!isValidDate)
                    {
                        AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Failed! Invalid date format");
                        continue;
                    }

                    
                    decimal invoiceAmount = 0;
                    if (string.IsNullOrEmpty(fileData.InvoiceAmount))
                    {
                        invoiceAmount = getElectiveData.Price;
                    }
                    else
                    {
                        if (!fileData.InvoiceAmount.All(char.IsDigit))
                        {
                            AddError(ref ErrorList, ref TotalRowFailed, $"Row {fileData.RowNum}: Failed! Invalid invoice amount format");
                            continue;
                        }

                        invoiceAmount = decimal.Parse(fileData.InvoiceAmount);
                    }
                    
                    //Create Data
                    TotalRowSuccess++;
                    if (homeroomStudentData.IdSchool == "1" && (electiveRuleData.ExtracurricularRule.ReviewDate.HasValue ? _dateTime.ServerTime <= electiveRuleData.ExtracurricularRule.ReviewDate : false))
                    {
                        var addQuery = _dbContext.Entity<MsExtracurricularParticipant>()
                            .Add(new MsExtracurricularParticipant
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = getElectiveData.Id,
                                IdStudent = homeroomStudentData.IdStudent,
                                IdGrade = homeroomStudentData.IdGrade,
                                JoinDate = DateTime.ParseExact(fileData.JoinDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                Status = true,
                                //Priority = maxPriority + 1,
                                IsPrimary = true
                            });
                    }
                    else
                    {
                        var addQuery = _dbContext.Entity<MsExtracurricularParticipant>()
                            .Add(new MsExtracurricularParticipant
                            {
                                Id = Guid.NewGuid().ToString(),
                                IdExtracurricular = getElectiveData.Id,
                                IdStudent = homeroomStudentData.IdStudent,
                                IdGrade = homeroomStudentData.IdGrade,
                                JoinDate = DateTime.ParseExact(fileData.JoinDate, "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                Status = true,
                                //Priority = maxPriority + 1,
                                IsPrimary = true
                            });

                        var createNewExtracurricularInvoice = new CreateStudentExtracurricularInvoiceRequest_ExtracurricularData
                        {
                            IdExtracurricular = getElectiveData.Id,
                            ExtracurricularPrice = invoiceAmount,
                            ExtracurricularType = getElectiveData.ExtracurricularType.Code
                        };

                        if (createNewExtracurricularInvoiceList.Where(a => a.IdExtracurricular == getElectiveData.Id).Count() == 0)
                        {
                            createNewExtracurricularInvoiceList.Add(createNewExtracurricularInvoice);
                        }

                        if (createNewExtracurricularInvoiceList.Count > 0)
                        {
                            var createInvoiceData = new CreateStudentExtracurricularInvoiceRequest
                            {
                                ExtracurricularList = createNewExtracurricularInvoiceList,
                                IdStudent = homeroomStudentData.IdStudent,
                                IdHomeroomStudent = homeroomStudentData.IdHomeroomStudent,
                                Semester = homeroomStudentData.Semester,
                                InvoiceStartDate = _dateTime.ServerTime,
                                InvoiceEndDate = _dateTime.ServerTime.AddDays(electiveRuleData.ExtracurricularRule.DueDayInvoice),
                                SendEmailNotification = param.SendEmail,
                            };

                            createInvoiceDataList.Add(createInvoiceData);
                        }
                    }
                }

                res = new AddStudentParticipantByExcelResult
                {
                    TotalRowSuccess = TotalRowSuccess,
                    TotalRowFailed = TotalRowFailed,
                    TotalRowData = TotalRowData,
                    ErrorList = ErrorList
                };

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // create invoice
                if (createInvoiceDataList.Count > 0)
                {
                    var createInvoice = await _extracurricularInvoiceApi.CreateStudentExtracurricularInvoice(createInvoiceDataList);
                }


            }
            catch(Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }

            return Request.CreateApiResult2(res as object);
        }

        private void AddError(ref List<AddStudentParticipantByExcelResult_Error> ErrorList, ref int TotalRowFailed, string Message)
        {
            TotalRowFailed++;
            ErrorList.Add(new AddStudentParticipantByExcelResult_Error { Error = Message });
        }

        private List<AddStudentParticipantByExcel_ExcelResult> ExtractExcelDataForAddParticipant(IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new BadRequestException("Excel file is not found");

            var fileInfo = new FileInfo(file.FileName);
            if (fileInfo.Extension != ".xlsx")
                throw new BadRequestException($"File not allowed. Allowed file extension: .xlsx");

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;

                var excludeExcelRow = new int[] { 1, 2, 3, 4, 5, 6 };
                var counterExcelRow = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var result = new List<AddStudentParticipantByExcel_ExcelResult>();

                    while (reader.Read())
                    {
                        counterExcelRow++;

                        // Skip the header rows
                        if (!excludeExcelRow.Contains(counterExcelRow))
                        {
                            // Read and extract the data
                            var idStudent = reader.GetValue(0)?.ToString().Trim();
                            var invoiceAmount = reader.GetValue(1)?.ToString().Trim();
                            var joinDate = reader.GetValue(2)?.ToString().Trim();

                            var resultData = new AddStudentParticipantByExcel_ExcelResult
                            {
                                IdStudent = idStudent,
                                InvoiceAmount = invoiceAmount,
                                JoinDate = joinDate,
                                RowNum = counterExcelRow.ToString()
                            };

                            result.Add(resultData);
                        }
                    }

                    return result;
                }
            }
        }

        private class AddStudentParticipantByExcel_ExcelResult
        {
            public string IdStudent { get; set; }
            public string InvoiceAmount { get; set; }
            public string JoinDate { get; set; }
            public string RowNum { get; set; }
        }
    }
}
