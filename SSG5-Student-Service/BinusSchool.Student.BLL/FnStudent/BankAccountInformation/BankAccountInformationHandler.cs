using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.BankAccountInformation;
using BinusSchool.Student.FnStudent.BankAccountInformation.Validator;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Common.Exceptions;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;

namespace BinusSchool.Student.FnStudent.BankAccountInformation
{
    public class BankAccountInformationHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;
        public BankAccountInformationHandler(IStudentDbContext studentDbContext, IConfiguration configuration, IMachineDateTime dateTime, IStudentEmailNotification sendEmailForProfileUpdateService)
        {
            _dbContext = studentDbContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _dateTime = dateTime;
            _sendEmailForProfileUpdateService = sendEmailForProfileUpdateService;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<GetBankAccountInformationRequest>(
                nameof(GetBankAccountInformationRequest.IdStudent),
                nameof(GetBankAccountInformationRequest.IdBank));

            var predicate = PredicateBuilder.Create<MsBankAccountInformation>(x => x.IdBank == param.IdBank);
            if (!string.IsNullOrEmpty(param.IdStudent))
                predicate = predicate.And(x => x.IdStudent == param.IdStudent);

            var query = _dbContext.Entity<MsBankAccountInformation>()
                .Include(x => x.Bank)
                .Include(x => x.Student)
                .Where(predicate);

            IReadOnlyList<IItemValueVm> items;
            if (param.Return == CollectionType.Lov)
                items = await query.Select(x => new GetBankAccountInformationResult
                {
                        IdBank = x.IdBank.ToString(),
                        IdStudent = x.IdStudent,
                        AccountNumberCurrentValue = x.AccountNumberCurrentValue,
                        AccountNameCurrentValue = x.AccountNameCurrentValue,
                        BankAccountNameCurrentValue = x.BankAccountNameCurrentValue,
                        AccountNumberNewValue = x.AccountNumberNewValue,
                        AccountNameNewValue = x.AccountNameNewValue,
                        BankAccountNameNewValue = x.BankAccountNameNewValue,
                        RequestedDate = x.RequestedDate,
                        ApprovalDate = x.ApprovalDate,
                        RejectDate = x.RejectDate,
                        Status = x.Status,
                        Notes = x.Notes
                    })
                    .ToListAsync(CancellationToken);
            else
                items = await query
                    .SetPagination(param)
                    .Select(x => new GetBankAccountInformationResult
                    {
                        IdBank = x.IdBank.ToString(),
                        IdStudent = x.IdStudent,
                        AccountNumberCurrentValue = x.AccountNumberCurrentValue,
                        AccountNameCurrentValue = x.AccountNameCurrentValue,
                        BankAccountNameCurrentValue = x.BankAccountNameCurrentValue,
                        AccountNumberNewValue = x.AccountNumberNewValue,
                        AccountNameNewValue = x.AccountNameNewValue,
                        BankAccountNameNewValue = x.BankAccountNameNewValue,
                        RequestedDate = x.RequestedDate,
                        ApprovalDate = x.ApprovalDate,
                        RejectDate = x.RejectDate,
                        Status = x.Status,
                        Notes = x.Notes
                    })
                    .ToListAsync(CancellationToken);
            var count = param.CanCountWithoutFetchDb(items.Count)
                ? items.Count
                : await query.Select(x => x.IdBank).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateBankAccountInformationRequest, UpdateBankAccountInformationValidator>();

            var newParentInfoUpdateIdList = new List<string>();

            var newBody = new SetBankAccountInformation
            {
                IdBank = new ItemValueVm{ 
                    Id = body.IdBank ?? "",
                    Description = body.BankAccountNameCurrentValue ?? ""
                },
                BankAccountNameCurrentValue = body.BankAccountNameCurrentValue,
                AccountNumberCurrentValue = body.AccountNumberCurrentValue,
                AccountNameCurrentValue = body.AccountNameCurrentValue
            };

            var getdata = await _dbContext.Entity<MsBankAccountInformation>()
                        .Where(x => x.IdStudent == body.IdStudent)
                        .FirstOrDefaultAsync();

            var bankCheck = await _dbContext.Entity<MsBank>().FindAsync(body.IdBank);
            if (bankCheck is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Bank"], "Id", body.IdBank));

            var StudentCheck = await _dbContext.Entity<MsStudent>().FindAsync(body.IdStudent);
            if (StudentCheck is null)
                throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Student"], "Id", body.IdStudent));

            Transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            if (getdata is null)
            {
                var data = new MsBankAccountInformation();
                data.IdBank = body.IdBank;
                data.IdStudent = body.IdStudent;
                data.BankAccountNameCurrentValue = body.BankAccountNameCurrentValue;
                data.AccountNumberCurrentValue = body.AccountNumberCurrentValue;
                data.AccountNameCurrentValue = body.AccountNameCurrentValue;
                data.UserIn = AuthInfo.UserId;
                data.DateIn = _dateTime.ServerTime;
                data.RequestedDate = _dateTime.ServerTime;
                data.Status = _newEntryApproval;

                _dbContext.Entity<MsBankAccountInformation>().Add(data);

                foreach (var prop in newBody.GetType().GetProperties())
                {
                    var oldVal = "";
                    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);
                    var setVal = newBody.GetType().GetProperty(prop.Name).GetValue(newBody, null);
                    var propType = prop.PropertyType;
                    var getdataType = data.GetType().ToString();

                    var newId = Guid.NewGuid().ToString();
                    var newParentInfoUpdate = new TrStudentInfoUpdate
                    {
                        Id = newId,
                        IdUser = body.IdStudent,
                        DateIn = _dateTime.ServerTime,
                        TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                        FieldName = prop.Name,
                        UserIn = AuthInfo.UserId,
                        Constraint1 = "action",
                        Constraint2 = propType.Equals(typeof(ItemValueVm)) ? "Description" : null,
                        Constraint3 = "IdStudent",
                        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                        Constraint1Value = "Add",
                        Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString() : null,
                        Constraint3Value = body.IdStudent,
                        RequestedDate = _dateTime.ServerTime,
                        RequestedBy = body.IsParentUpdate == 1 ? "Parent Of " + AuthInfo.UserName : AuthInfo.UserName,
                        //ApprovalDate = "",
                        IdApprovalStatus = _newEntryApproval,
                        //Notes = "",
                        IsParentUpdate = body.IsParentUpdate
                    };

                    if (!(oldVal ?? "").Equals(newVal))
                        _dbContext.Entity<TrStudentInfoUpdate>().Add(newParentInfoUpdate);

                    newParentInfoUpdateIdList.Add(newId);
                }

            }
            else
            {
                //var data = new MsBankAccountInformation();
                getdata.BankAccountNameNewValue = body.BankAccountNameCurrentValue;
                getdata.AccountNumberNewValue = body.AccountNumberCurrentValue;
                getdata.AccountNameNewValue = body.AccountNameCurrentValue;
                getdata.UserUp = AuthInfo.UserId;
                getdata.DateUp = _dateTime.ServerTime;
                getdata.RequestedDate = _dateTime.ServerTime;
                getdata.Status = _newEntryApproval;

                _dbContext.Entity<MsBankAccountInformation>().Update(getdata);

                foreach (var prop in newBody.GetType().GetProperties())
                {
                    var oldVal = getdata.GetType().GetProperty(prop.Name).GetValue(getdata, null);
                    var newVal = body.GetType().GetProperty(prop.Name).GetValue(body, null);
                    var setVal = newBody.GetType().GetProperty(prop.Name).GetValue(newBody, null);
                    var propType = prop.PropertyType;
                    var getdataType = getdata.GetType().ToString();

                    var newId = Guid.NewGuid().ToString();
                    var newParentInfoUpdate = new TrStudentInfoUpdate
                    {
                        Id = newId,
                        IdUser = body.IdStudent,
                        DateIn = _dateTime.ServerTime,
                        TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                        FieldName = prop.Name,
                        UserIn = AuthInfo.UserId,
                        Constraint1 = "action",
                        Constraint2 = propType.Equals(typeof(ItemValueVm)) ? "Description" : null,
                        Constraint3 = "IdStudent",
                        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                        Constraint1Value = "Add",
                        Constraint2Value = propType.Equals(typeof(ItemValueVm)) ? setVal.GetType().GetProperty("Description").GetValue(setVal, null).ToString() : null,
                        Constraint3Value = body.IdStudent,
                        RequestedDate = _dateTime.ServerTime,
                        RequestedBy = body.IsParentUpdate == 1 ? "Parent Of " + AuthInfo.UserName : AuthInfo.UserName,
                        //ApprovalDate = "",
                        IdApprovalStatus = _newEntryApproval,
                        //Notes = "",
                        IsParentUpdate = body.IsParentUpdate
                    };

                    if (!(oldVal ?? "").Equals(newVal))
                        _dbContext.Entity<TrStudentInfoUpdate>().Add(newParentInfoUpdate);

                    newParentInfoUpdateIdList.Add(newId);
                }

            }

            //var getdataType = getdata.GetType().ToString();



            /*var newParentInfoUpdate = new TrStudentInfoUpdate
            {
                IdUser = body.IdStudent,
                DateIn = DateTime.Now,
                TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                FieldName = "IdBank",
                UserIn = AuthInfo.UserId,
                UserUp = AuthInfo.UserId,
                Constraint1 = "BankAccountNameCurrentValue",
                Constraint2 = "AccountNumberCurrentValue",
                Constraint3 = "AccountNameCurrentValue",
                OldFieldValue = getdata.IdBank,
                CurrentFieldValue = body.IdBank,
                Constraint1Value = body.BankAccountNameCurrentValue,
                Constraint2Value = body.AccountNumberCurrentValue,
                Constraint3Value = body.AccountNameCurrentValue,
                RequestedDate = DateTime.Now,
                //ApprovalDate = "",
                IdApprovalStatus = _newEntryApproval,
                //Notes = "",
                IsParentUpdate = body.IsParentUpdate
            };
            _dbContext.Entity<TrStudentInfoUpdate>().Add(newParentInfoUpdate);*/

            await _dbContext.SaveChangesAsync(CancellationToken);
            await Transaction.CommitAsync(CancellationToken);

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
    
    }
}
