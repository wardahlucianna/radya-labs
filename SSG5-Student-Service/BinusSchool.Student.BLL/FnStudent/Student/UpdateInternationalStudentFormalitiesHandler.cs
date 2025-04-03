using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Abstractions;
using BinusSchool.Data.Api.Employee.FnStaff;
using BinusSchool.Data.Api.Extensions;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;

namespace BinusSchool.Student.FnStudent.Student
{
    public class UpdateInternationalStudentFormalitiesHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;
        private readonly IStudent _studentService;
        private readonly ITeacher _staffService;
        private readonly int _newEntryApproval;
        private readonly IMachineDateTime _dateTime;

        public UpdateInternationalStudentFormalitiesHandler(IStudentDbContext dbContext
            , IConfiguration configuration
            , IStudent studentService
            , ITeacher staffService
            , IStudentEmailNotification sendEmailForProfileUpdateService
            , IMachineDateTime dateTime)
        {
            _dbContext = dbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _studentService = studentService;
            _staffService = staffService;
            _sendEmailForProfileUpdateService = sendEmailForProfileUpdateService;
            _dateTime = dateTime;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            try
            {
                var body = await Request.ValidateBody<UpdateInternationalStudentFormalitiesRequest, UpdateInternationalStudentFormalitiesValidator>();

                var newParentInfoUpdateIdList = new List<string>();

                /*
                string RequestedBy = null;
                if (body.IsParentUpdate == 0)
                {
                    var staffDetail = await _staffService.GetTeacherDetail(AuthInfo.UserId);
                    RequestedBy = staffDetail.Payload.PersonalInfo.LastName;
                }
                if (body.IsParentUpdate == 1)
                {
                    var studentDetail = await _studentService.GetStudentDetail(body.IdStudent);
                    RequestedBy = "Parent of " + studentDetail.Payload.NameInfo.LastName;
                }*/

                var newBody = new SetInternationalStudentFormalities
                {
                    KITASNumber = body.KITASNumber,
                    KITASExpDate = body.KITASExpDate,
                    NSIBNumber = body.NSIBNumber,
                    NSIBExpDate = body.NSIBExpDate
                };

                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getdata = await _dbContext.Entity<MsStudent>().Where(p => p.Id == body.IdStudent).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Student ID"], "Id", body.IdStudent));

                //var ListFielddata = new List<string>();
                //var ListOldValue = new List<string>();
                //var ListNewValue = new List<string>();

                var getStudentInfoUpdate = await _dbContext.Entity<TrStudentInfoUpdate>()
                            .Where(p => p.IdUser == body.IdStudent && p.Constraint3Value == body.IdStudent && p.IdApprovalStatus == _newEntryApproval)
                            .Select(p => p.FieldName)
                            .ToListAsync();

                foreach (var prop in newBody.GetType().GetProperties())
                {
                    if (getStudentInfoUpdate.Contains(prop.Name))
                        continue;

                    var oldVal = getdata.GetType().GetProperty(prop.Name).GetValue(getdata, null);
                    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);
                    var setVal = newBody.GetType().GetProperty(prop.Name).GetValue(newBody, null);
                    var propType = prop.PropertyType;
                    var getdataType = getdata.GetType().ToString();

                    //ListFielddata.Add(prop.Name);
                    //ListOldValue.Add(oldVal.ToString());
                    //ListNewValue.Add(newVal.ToString());

                    var newId = Guid.NewGuid().ToString();
                    var newParentInfoUpdate = new TrStudentInfoUpdate
                    {
                        Id = newId,
                        DateIn = _dateTime.ServerTime,
                        IdUser = body.IdStudent,
                        TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                        FieldName = prop.Name,
                        Constraint1 = "",
                        Constraint2 = propType.Equals(typeof(ItemValueVm)) ? "Description" : null,
                        Constraint3 = "IdStudent",
                        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                        Constraint1Value = "",
                        Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString() : null,
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
