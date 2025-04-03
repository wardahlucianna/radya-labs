using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.Parent;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Parent.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;

namespace BinusSchool.Student.FnStudent.Parent
{
    public class UpdateParentContactInformationHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;
        private readonly int _newEntryApproval;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;

        public UpdateParentContactInformationHandler(IStudentDbContext dbContext, IConfiguration configuration
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
                var body = await Request.ValidateBody<UpdateParentContactInformationRequest, UpdateParentContactInformationValidator>();

                var newParentInfoUpdateIdList = new List<string>();

                var newBody = new SetParentContactInformation {
                        ResidencePhoneNumber = body.ResidencePhoneNumber,
                        MobilePhoneNumber1 = body.MobilePhoneNumber1,
                        PersonalEmailAddress = body.PersonalEmailAddress
                    };
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                var getdata = await _dbContext.Entity<MsParent>().Where(p => p.Id == body.IdParent).FirstOrDefaultAsync();
                if (getdata is null)
                    throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Parent ID"], "Id", body.IdParent));

                var getStudentInfoUpdate = await _dbContext.Entity<TrStudentInfoUpdate>()
                    .Where(p => p.IdUser == body.IdParent && p.Constraint3Value == body.IdStudent && p.IdApprovalStatus == _newEntryApproval)
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

                    var newId = Guid.NewGuid().ToString();
                    var newParentInfoUpdate = new TrStudentInfoUpdate
                    {
                        Id = newId,
                        DateIn = _dateTime.ServerTime,
                        IdUser = body.IdParent,
                        TableName = getdataType.Split('.', StringSplitOptions.RemoveEmptyEntries).Last(),
                        FieldName = prop.Name,
                        Constraint1 = "ParentRole",
                        Constraint2 = propType.Equals(typeof(ItemValueVm)) ? "Description" : null,
                        Constraint3 = "IdStudent",
                        OldFieldValue = oldVal == null ? null : oldVal.ToString(),
                        CurrentFieldValue = newVal == null ? null : newVal.ToString(),
                        Constraint1Value = body.IdParentRole,
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
