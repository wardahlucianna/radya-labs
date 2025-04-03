using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Abstractions;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model;
using BinusSchool.Common.Model.Information;
using BinusSchool.Data.Model.Student.FnStudent.Student;
using BinusSchool.Data.Model.Student.FnStudent.StudentPrevSchoolInfo;
using BinusSchool.Student.FnStudent.StudentPrevSchoolInfo.Validator;
using BinusSchool.Domain.Extensions;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using BinusSchool.Common.Abstractions;
using BinusSchool.Data.Api.Student.FnStudent;
using BinusSchool.Data.Model.Student.FnStudent.SendEmail;

namespace BinusSchool.Student.FnStudent.StudentPrevSchoolInfo
{
    public class StudentPrevSchoolInfoHandler : FunctionsHttpCrudHandler
    {
        private readonly IStudentDbContext _dbStudentContext;
        private readonly int _newEntryApproval;
        private IDbContextTransaction _transaction;
        private readonly IMachineDateTime _dateTime;
        private readonly IStudentEmailNotification _sendEmailForProfileUpdateService;
        public StudentPrevSchoolInfoHandler(IStudentDbContext dbStudentContext, IConfiguration configuration, IMachineDateTime dateTime, IStudentEmailNotification sendEmailForProfileUpdateService)
        {
            _dbStudentContext = dbStudentContext;
            _newEntryApproval = Convert.ToInt32(configuration["NewEntryApproval"]);
            _dateTime = dateTime;
            _sendEmailForProfileUpdateService = sendEmailForProfileUpdateService;
        }
        protected override Task<ApiErrorResult<object>> DeleteHandler(IEnumerable<string> ids)
        {
            throw new NotImplementedException();
        }

        protected override async Task<ApiErrorResult<object>> GetDetailHandler(string id)
        {
            var studentData = _dbStudentContext.Entity<MsStudentPrevSchoolInfo>()
                                .Where(x => x.IdStudent == id)
                                .Select(x => new { x.IdPreviousSchoolNew, x.IdPreviousSchoolOld})
                                .FirstOrDefault();

            var previousSchoolNew = _dbStudentContext.Entity<MsPreviousSchoolNew>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdPreviousSchoolNew))
                                .Select(x => new { id = x.Id, description = x.SchoolName })
                                .FirstOrDefault();

            var previousSchoolOld = _dbStudentContext.Entity<MsPreviousSchoolOld>()
                                .Where(x => x.Id == (studentData == null ? null : studentData.IdPreviousSchoolOld))
                                .Select(x => new { id = x.Id, description = x.SchoolName })
                                .FirstOrDefault();

            //var statusApproval = _dbStudentContext.Entity<TrStudentInfoUpdate>()
            //                    .Where(x => x.Constraint3Value == id && x.IdApprovalStatus == _newEntryApproval && x.Constraint1Value == "Add" && x.TableName == "MsStudentPrevSchoolInfo")
            //                    .Select(x => new { id = x.Constraint3Value, description = x.Constraint1Value })
            //                    .Distinct()
            //                    .FirstOrDefault(); 

            var previousSchoolNewInfo = new ItemValueVm
            {
                Id = previousSchoolNew == null ? null : previousSchoolNew.id,
                Description = previousSchoolNew == null ? null : previousSchoolNew.description,
            };

            var previousSchoolOldInfo = new ItemValueVm
            {
                Id = previousSchoolOld == null ? null : previousSchoolOld.id,
                Description = previousSchoolOld == null ? null : previousSchoolOld.description,
            };

            //if (statusApproval != null)
            //{
            //    var query = new List<GetStudentPrevSchoolInfoResult>
            //        {
            //            new GetStudentPrevSchoolInfoResult 
            //            { 
            //                Id = id,
            //                Description = "PrevSchoolInfo",
            //                Grade = "Process Add",
            //                YearAttended = "Process Add",
            //                YearWithdrawn = "Process Add"
            //            }
            //        };
            //    return Request.CreateApiResult2(query as object);
            //}
            //else
            //{
                var query = await _dbStudentContext.Entity<MsStudentPrevSchoolInfo>()
                        .Where(x => x.IdStudent == id)
                        .Select(x => new GetStudentPrevSchoolInfoResult
                        {
                            Id = id,
                            Description = "PrevSchoolInfo",
                            Grade = x.Grade,
                            YearAttended = x.YearAttended,
                            YearWithdrawn = x.YearWithdrawn,
                            IsHomeSchooling = x.IsHomeSchooling,
                            IdPreviousSchoolNew = previousSchoolNewInfo,
                            IdPreviousSchoolOld = previousSchoolOldInfo
                        })
                        .ToListAsync(CancellationToken);
                return Request.CreateApiResult2(query as object);
            //}
        }

        protected override async Task<ApiErrorResult<IReadOnlyList<IItemValueVm>>> GetHandler()
        {
            var param = Request.ValidateParams<CollectionRequest>(nameof(CollectionSchoolRequest.IdSchool));

            var query = _dbStudentContext.Entity<MsStudentPrevSchoolInfo>().Include(x => x.PreviousSchoolNew);
            IReadOnlyList<IItemValueVm> items;
            items = await query
                    .Select(x => new GetStudentPrevSchoolInfoResult
                    {
                        Id = x.IdStudent,
                        Description = "PrevSchoolInfo",
                        Grade = x.Grade,
                        YearAttended = x.YearAttended,
                        YearWithdrawn = x.YearWithdrawn,
                        IsHomeSchooling = x.IsHomeSchooling,
                        IdPreviousSchoolNew = new ItemValueVm
                        {
                            Id = x.PreviousSchoolNew.Id,
                            Description = x.PreviousSchoolNew.SchoolName
                        },
                        IdPreviousSchoolOld = new ItemValueVm
                        {
                            Id = x.PreviousSchoolOld.Id,
                            Description = x.PreviousSchoolOld.SchoolName
                        },
                    })
                    .ToListAsync(CancellationToken);

            var count = param.GetAll == true
                ? items.Count
                : await query.Select(x => x.IdStudent).CountAsync(CancellationToken);

            return Request.CreateApiResult2(items, param.CreatePaginationProperty(count));
        }

        protected override Task<ApiErrorResult<object>> PostHandler()
        {
            throw new NotImplementedException();
        }
        protected override async Task<ApiErrorResult<object>> PutHandler()
        {
            var body = await Request.ValidateBody<UpdateStudentPrevSchoolInfoRequest, UpdateStudentPrevSchoolInfoValidator>();

            var newParentInfoUpdateIdList = new List<string>();

            var statusApproval = _dbStudentContext.Entity<TrStudentInfoUpdate>()
                .Where(x => x.Constraint3Value == body.IdStudent && x.IdApprovalStatus == _newEntryApproval && x.TableName == "MsStudentPrevSchoolInfo")
                .Select(x => new { id = x.Constraint3Value, description = x.Constraint1Value })
                .Distinct()
                .FirstOrDefault();

            if (statusApproval == null)
            {
                try
                {
                    var newBody = new SetStudentPrevSchoolInfo
                    {
                        IdPreviousSchoolNew = new ItemValueVm
                        {
                            Id = body.IdPreviousSchoolNew,
                            Description = body.IdPreviousSchoolNewDesc
                        },
                        Grade = body.Grade,
                        YearAttended = body.YearAttended,
                        YearWithdrawn = body.YearWithdrawn//,
                        //IsHomeSchooling = body.IsHomeSchooling
                    };
                    _transaction = await _dbStudentContext.BeginTransactionAsync(CancellationToken);

                    var getdata = await _dbStudentContext.Entity<MsStudentPrevSchoolInfo>()
                        .Where(p => p.IdStudent == body.IdStudent)
                        .FirstOrDefaultAsync();

                    var idreg = await _dbStudentContext.Entity<MsStudent>()
                        .Where(p => p.Id == body.IdStudent)
                        .Select(p => p.IdRegistrant)
                        .FirstOrDefaultAsync();

                    if (getdata is null)
                    {
                        var data = new MsStudentPrevSchoolInfo();
                        data.IdStudent = body.IdStudent;
                        data.IdRegistrant = idreg == null ? "" : idreg.ToString();
                        data.Grade = body.Grade;
                        data.IdPreviousSchoolNew = body.IdPreviousSchoolNew;
                        data.IsHomeSchooling = body.IsHomeSchooling;
                        data.YearAttended = body.YearAttended;
                        data.YearWithdrawn = body.YearWithdrawn;

                        _dbStudentContext.Entity<MsStudentPrevSchoolInfo>().Add(data);

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
                                _dbStudentContext.Entity<TrStudentInfoUpdate>().Add(newParentInfoUpdate);

                            newParentInfoUpdateIdList.Add(newId);
                        }

                    }
                    else
                    {
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
                                Constraint1Value = "Update",
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
                                _dbStudentContext.Entity<TrStudentInfoUpdate>().Add(newParentInfoUpdate);

                            newParentInfoUpdateIdList.Add(newId);
                        }
                    }

                    await _dbStudentContext.SaveChangesAsync(CancellationToken);
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
            else {
                return Request.CreateApiResult2(message: "processing another request");
            }
            
        }
    }
}
