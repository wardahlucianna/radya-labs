﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.StudentInfoUpdate;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Student.FnStudent.Student.Validator;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using BinusSchool.Student.FnStudent.StudentInfoUpdate.Validator;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;

namespace BinusSchool.Student.FnStudent.StudentInfoUpdate
{
    public class UpdateSiblingGroupInfoUpdateHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private readonly int _newEntryApproval;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;

        public UpdateSiblingGroupInfoUpdateHandler(IStudentDbContext dbContext, IConfiguration configuration
            , IMachineDateTime dateTime
            , IStudentEmailNotification sendEmailForProfileUpdateService)
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
                var bodys = await Request.GetBody<IEnumerable<UpdateStudentInfoUpdate>>();
                var parentInfoUpdateIdList = new List<string>();
                _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

                foreach (var body in bodys)
                {
                    var getdata = await _dbContext.Entity<MsSiblingGroup>().Where(p => p.IdStudent == body.IdUser).FirstOrDefaultAsync();
                    if (getdata is null)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id User"], "Id", body.IdUser));
                    else
                        _dbContext.Entity<MsSiblingGroup>().Remove(getdata);
                        await _dbContext.SaveChangesAsync(CancellationToken);

                    var getdataStudentInfo = await _dbContext.Entity<TrStudentInfoUpdate>()
                                            .Where(p => p.IdUser == body.IdUser && p.TableName == body.TableName && p.FieldName == body.FieldName && p.IdApprovalStatus == _newEntryApproval)
                                            .FirstOrDefaultAsync();
                    if (getdataStudentInfo is null)
                        throw new BadRequestException(string.Format(Localizer["ExNotExist"], Localizer["Id User"], "Id", body.IdUser));

                    getdataStudentInfo.IdApprovalStatus = body.IdApprovalStatus;
                    getdataStudentInfo.Notes = body.Notes;
                    getdataStudentInfo.ApprovalDate = _dateTime.ServerTime;

                    _dbContext.Entity<TrStudentInfoUpdate>().Update(getdataStudentInfo);
                    parentInfoUpdateIdList.Add(getdataStudentInfo.Id);
                    if (body.IdApprovalStatus == 1)
                    {
                        var propType = getdata.GetType().GetProperty(body.FieldName == "IdSiblingGroup" ? "Id" : body.FieldName).PropertyType;
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(propType);
                        var convertedObject = converter.ConvertFromString(body.CurrentFieldValue);

                        var propertyInfo = getdata.GetType().GetProperty(body.FieldName == "IdSiblingGroup" ? "Id" : body.FieldName);
                        propertyInfo.SetValue(getdata, convertedObject);
                        _dbContext.Entity<MsSiblingGroup>().Add(getdata);
                    }
                    else {
                        _dbContext.Entity<MsSiblingGroup>().Add(getdata);
                    }
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                await _transaction.CommitAsync(CancellationToken);

                // send email notification to parent
                if (parentInfoUpdateIdList.Any())
                {
                    try
                    {
                        var sendEmail = await _sendEmailForProfileUpdateService.SendEmailProfileApprovalUpdateToParent(new SendEmailProfileApprovalUpdateToParentRequest
                        {
                            IdUser = bodys.FirstOrDefault().IdUser,
                            IdStudentInfoUpdateList = parentInfoUpdateIdList
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
