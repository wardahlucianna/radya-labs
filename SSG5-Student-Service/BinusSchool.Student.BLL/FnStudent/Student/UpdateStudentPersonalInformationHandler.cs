using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;

namespace BinusSchool.Student.FnStudent.Student
{
    public class UpdateStudentPersonalInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;

        public UpdateStudentPersonalInformationHandler(IStudentDbContext dbContext, IConfiguration configuration
            , IMachineDateTime dateTime, IStudentEmailNotification sendEmailForProfileUpdateService)
        {
            _dbContext = dbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _dateTime = dateTime;
            _sendEmailForProfileUpdateService = sendEmailForProfileUpdateService;
        }
        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateStudentPersonalInformationRequest, UpdatePersonalInformationValidator>();

                var newParentInfoUpdateIdList = new List<string>();

                var newBody = new SetStudentPersonalInformation
                {
                    IdBirthCountry = new ItemValueVm
                    {
                        Id = body.IdBirthCountry ?? "",
                        Description = body.IdBirthCountryDesc ?? ""
                    },
                    IdBirthStateProvince = new ItemValueVm
                    {
                        Id = body.IdBirthStateProvince ?? "",
                        Description = body.IdBirthStateProvinceDesc ?? ""
                    },
                    IdBirthCity = new ItemValueVm
                    {
                        Id = body.IdBirthCity ?? "",
                        Description = body.IdBirthCityDesc ?? ""
                    },
                    IdNationality = new ItemValueVm
                    {
                        Id = body.IdNationality ?? "",
                        Description = body.IdNationalityDesc ?? ""
                    },
                    IdCountry = new ItemValueVm
                    {
                        Id = body.IdCountry ?? "",
                        Description = body.IdCountryDesc ?? ""
                    },
                    IdReligion = new ItemValueVm
                    {
                        Id = body.IdReligion ?? "",
                        Description = body.IdReligionDesc ?? ""
                    },
                    IdReligionSubject = new ItemValueVm
                    {
                        Id = body.IdReligionSubject ?? "",
                        Description = body.IdReligionSubjectDesc ?? ""
                    },
                    IdChildStatus = new ItemValueVm
                    {
                        Id = body.IdChildStatus ?? "",
                        Description = body.IdChildStatusDesc ?? ""
                    },
                    //IdBinusian = body.IdBinusian,
                    Gender = body.Gender,
                    NISN = body.NISN,
                    FirstName = body.FirstName,
                    LastName = body.LastName,
                    POB = body.POB,
                    DOB = body.DOB,
                    FamilyCardNumber = body.FamilyCardNumber,
                    NIK = body.NIK,
                    PassportNumber = body.PassportNumber,
                    ChildNumber = body.ChildNumber,
                    TotalChildInFamily = body.TotalChildInFamily,
                    IsHavingKJP = body.IsHavingKJP
                };
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getdata = await _dbContext.Entity<MsStudent>().Where(p => p.Id == body.IdStudent).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Student ID"], "Id", body.IdStudent));

                var getStudentInfoUpdate = await _dbContext.Entity<TrStudentInfoUpdate>()
                            .Where(p => p.IdUser == body.IdStudent && p.Constraint3Value == body.IdStudent && p.IdApprovalStatus == _newEntryApproval)
                            .Select(p => p.FieldName)
                            .ToListAsync();

                /*var birthCountry = await _dbContext.Entity<LtCountry>().FindAsync(body.IdBirthCountry);
                if (birthCountry is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Birth Country"], "Id", body.IdBirthCountry));

                var birthStateProvince = await _dbContext.Entity<LtDistrict>().FindAsync(body.IdBirthStateProvince);
                if (birthStateProvince is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Birth State Province"], "Id", body.IdBirthStateProvince));

                var birthCity = await _dbContext.Entity<LtCity>().FindAsync(body.IdBirthCity);
                if (birthCity is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Birth City"], "Id", body.IdBirthCity));

                var nationality = await _dbContext.Entity<LtNationality>().FindAsync(body.IdNationality);
                if (nationality is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Nationality"], "Id", body.IdNationality));

                var nationalityCountry = await _dbContext.Entity<MsNationalityCountry>().FindAsync(body.IdCountry);
                if (nationalityCountry is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Nationality Country"], "Id", body.IdCountry));

                var religion = await _dbContext.Entity<LtReligion>().FindAsync(body.IdReligion);
                if (religion is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Religion"], "Id", body.IdReligion));

                var religionSubject = await _dbContext.Entity<LtReligionSubject>().FindAsync(body.IdReligionSubject);
                if (religionSubject is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Religio nSubject"], "Id", body.IdReligionSubject));

                var childStatus = await _dbContext.Entity<LtChildStatus>().FindAsync(body.IdChildStatus);
                if (childStatus is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Child Status"], "Id", body.IdChildStatus));
                */
                foreach (var prop in newBody.GetType().GetProperties())
                {
                    if (getStudentInfoUpdate.Contains(prop.Name))
                        continue;

                    var oldVal = getdata.GetType().GetProperty(prop.Name).GetValue(getdata, null);
                    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);
                    var setVal = newBody.GetType().GetProperty(prop.Name).GetValue(newBody, null);
                    var propType = prop.PropertyType;
                    var getdataType = getdata.GetType().ToString();

                    if (prop.Name == "Gender")
                    {
                        var info = 1;
                        var tipedata = propType.IsEnum;
                    }

                    var Constraint2 = propType.Equals(typeof(ItemValueVm)) ? "Description" :
                        (propType.IsEnum ? "Id" : null);

                    //var Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString() :
                    //    (propType.IsEnum ? ((int)setVal).ToString() : null);

                    var Constraint2Value = propType.Equals(typeof(ItemValueVm))
                        ? (setVal != null ? setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString() : null)
                        : (propType.IsEnum ? ((int)setVal).ToString() : null);

                    var newId = Guid.NewGuid().ToString();
                    var newParentInfoUpdate = new TrStudentInfoUpdate
                    {
                        Id = newId,
                        DateIn = _dateTime.ServerTime,
                        IdUser = body.IdStudent,
                        TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                        FieldName = prop.Name,
                        Constraint1 = "",
                        Constraint2 = Constraint2,
                        Constraint3 = "IdStudent",
                        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                        Constraint1Value = "",
                        Constraint2Value = Constraint2Value,
                        Constraint3Value = body.IdStudent,
                        RequestedDate = _dateTime.ServerTime,
                        RequestedBy = body.IsParentUpdate == 1 ? "Parent Of " + AuthInfo.UserName : AuthInfo.UserName,
                        //ApprovalDate = "",
                        IdApprovalStatus = _newEntryApproval,
                        //Notes = "",
                        IsParentUpdate = body.IsParentUpdate
                    };

                    if (!(oldVal ?? "").Equals(newVal ?? ""))
                        _dbContext.Entity<TrStudentInfoUpdate>().Add(newParentInfoUpdate);

                    newParentInfoUpdateIdList.Add(newId);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // send email notification to staff
                if (newParentInfoUpdateIdList.Any())
                {
                    try
                    {
                        var sendEmail = await _sendEmailForProfileUpdateService.SendEmailProfileUpdateToStaff(new SendEmailProfileUpdateToStaffRequest
                        {
                            IdStudent = body.IdStudent,
                            IdStudentInfoUpdateList = newParentInfoUpdateIdList
                        });
                    }
                    catch (Exception ex)
                    {

                    }
                }

                return Request.CreateApiResult2();
            }
            catch (Exception ex)
            {
                _transaction?.Rollback();
                throw new Exception(ex.Message);
            }
            finally
            {
                _transaction?.Dispose();
            }
        }
    }
}
