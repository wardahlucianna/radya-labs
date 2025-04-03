using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SiblingGroup;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.SiblingGroup.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;

namespace BinusSchool.Student.FnStudent.SiblingGroup
{
    public class AddSiblingGroupHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly int _newEntryApproval;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;

        public AddSiblingGroupHandler(IStudentDbContext dbContext, IConfiguration configuration, IMachineDateTime dateTime, IStudentEmailNotification sendEmailForProfileUpdateService)
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
                var body = await Request.ValidateBody<AddSiblingGroupRequest, AddSiblingGroupValidator>(); 
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var newParentInfoUpdateIdList = new List<string>();

                var getdata = await _dbContext.Entity<MsSiblingGroup>().Where(x => x.IdStudent == body.IdSibling ).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["ID Sibling Group With ID Student "], "Id", body.IdSiblingGroup));

                var oldVal = getdata.Id;
                var newVal = body.IdSiblingGroup;
                var getdataType = getdata.GetType().ToString();

                var newId = Guid.NewGuid().ToString();
                var newSiblingGroupInfoUpdate = new TrStudentInfoUpdate
                {
                    Id = newId,
                    IdUser = body.IdSibling,
                    DateIn = _dateTime.ServerTime,
                    TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                    FieldName = "IdSiblingGroup",
                    UserUp = AuthInfo.UserId,
                    Constraint1 = "action",
                    Constraint2 = "",
                    Constraint3 = "IdStudent",
                    OldFieldValue = oldVal,
                    CurrentFieldValue = newVal.ToString(),
                    Constraint1Value = "Add",
                    Constraint2Value = "",
                    Constraint3Value = body.IdStudent,
                    RequestedDate = _dateTime.ServerTime,
                    RequestedBy = body.IsParentUpdate == 1 ? "Parent Of " + AuthInfo.UserName : AuthInfo.UserName,
                    //ApprovalDate = "",
                    IdApprovalStatus = _newEntryApproval,
                    //Notes = "",
                    IsParentUpdate = body.IsParentUpdate
                };
                _dbContext.Entity<TrStudentInfoUpdate>().Add(newSiblingGroupInfoUpdate);

                newParentInfoUpdateIdList.Add(newId);

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
