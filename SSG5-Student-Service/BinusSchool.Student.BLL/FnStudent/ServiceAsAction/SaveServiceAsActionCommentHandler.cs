using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Auth.Authentications.Jwt;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.ServiceAsAction;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Scheduling;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Student.FnStudent.ServiceAsAction.Validator;
using FluentEmail.Core;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Azure.WebJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using NPOI.OpenXmlFormats.Wordprocessing;
using Org.BouncyCastle.Asn1.Pkcs;

namespace BinusSchool.Student.FnStudent.ServiceAsAction
{
    public class SaveServiceAsActionCommentHandler : FunctionsHttpSingleHandler
    {
        private readonly IStudentDbContext _dbContext;
        private IDbContextTransaction _transaction;

        public SaveServiceAsActionCommentHandler
        (
            IStudentDbContext studentDbContext
        )
        {
            _dbContext = studentDbContext;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            _transaction = await _dbContext.BeginTransactionAsync(CancellationToken);

            try
            {
                var param = await Request.ValidateBody<SaveServiceAsActionCommentRequest, SaveServiceAsActionCommentValidator>();

                if (String.IsNullOrEmpty(param.IdServiceAsActionComment))
                {
                    var newData = new TrServiceAsActionComment
                    {
                        Id = Guid.NewGuid().ToString(),
                        IdServiceAsActionEvidence = param.IdServiceAsActionEvidence,
                        IdCommentator = param.IdUser,
                        CommentDesc = param.Comment
                    };
                    _dbContext.Entity<TrServiceAsActionComment>().Add(newData);
                }
                else
                {
                    var ExistingComment = await _dbContext.Entity<TrServiceAsActionComment>()
                        .Where(x => x.Id == param.IdServiceAsActionComment)
                        .FirstOrDefaultAsync(CancellationToken);

                    if (ExistingComment == null) throw new Exception("Comment Not Found");

                    ExistingComment.CommentDesc = param.Comment;
                    _dbContext.Entity<TrServiceAsActionComment>().Update(ExistingComment);
                }

                await _dbContext.SaveChangesAsync(CancellationToken);
                

                var getUserRole = await _dbContext.Entity<MsUserRole>()
                .Include(x => x.User)
                .Include(x => x.Role)
                .Where(x => x.IdUser == param.IdUser)
                .ToListAsync(CancellationToken);

                var getDetailHeader = await _dbContext.Entity<TrServiceAsActionEvidence>()
                    .Include(x => x.ServiceAsActionForm)
                        .ThenInclude(x => x.ServiceAsActionHeader)
                    .Where(x => x.Id == param.IdServiceAsActionEvidence)
                    .Select(x => new 
                    {
                        x.ServiceAsActionForm.ServiceAsActionHeader.IdStudent, 
                        x.ServiceAsActionForm.ServiceAsActionHeader.IdAcademicYear,
                        x.ServiceAsActionForm
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                var getAdvisors = await _dbContext.Entity<MsHomeroomStudent>()
                    .Include(x => x.Homeroom)
                        .ThenInclude(x => x.HomeroomTeachers)
                    .Where(x => x.IdStudent == getDetailHeader.IdStudent && x.Homeroom.Grade.MsLevel.MsAcademicYear.Id  == getDetailHeader.IdAcademicYear)
                    .SelectMany(x => x.Homeroom.HomeroomTeachers.Select(y => y.IdBinusian))
                    .Distinct()
                    .ToListAsync(CancellationToken);

                List<string> Recipients = new List<string>();

                if (getUserRole.FirstOrDefault().Role.Code == "Student")
                {
                    var formDetail = getDetailHeader.ServiceAsActionForm;

                    Recipients.AddRange(getAdvisors);
                    if (!String.IsNullOrEmpty(formDetail.IdSupervisor))
                    {
                        Recipients.Add(formDetail.IdSupervisor);
                    }
                }
                else
                {
                    if (param.IsAdvisor)
                    {
                        var formDetail = getDetailHeader.ServiceAsActionForm;
                        var student = getDetailHeader.IdStudent;

                        Recipients.Add(student);
                        if (!String.IsNullOrEmpty(formDetail.IdSupervisor))
                        {
                            Recipients.Add(formDetail.IdSupervisor);
                        }
                    }
                    else
                    {
                        var formDetail = getDetailHeader.ServiceAsActionForm;
                        var student = getDetailHeader.IdStudent;

                        Recipients.Add(student);

                        if (!String.IsNullOrEmpty(formDetail.IdSupervisor))
                        {
                            Recipients.Add(formDetail.IdSupervisor);
                        }
                    }
                }

                var activityName = getDetailHeader.ServiceAsActionForm.ExpName;
                var ceomentator = getUserRole.FirstOrDefault().User.DisplayName;

                var Notification = SASNotification(KeyValues, ceomentator, Recipients, activityName, AuthInfo);

                await _transaction.CommitAsync(CancellationToken);

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

            return Request.CreateApiResult2();
        }

        public static string SASNotification(IDictionary<string, object> KeyValues, string comentator, List<string> recipients, string activityName, AuthenticationInfo AuthInfo)
        {
            if (KeyValues.ContainsKey("comentator"))
            {
                KeyValues.Remove("comentator");
            }

            if (KeyValues.ContainsKey("activityName"))
            {
                KeyValues.Remove("activityName");
            }

            KeyValues.Add("comentator",comentator);
            KeyValues.Add("activityName", activityName);


            if (KeyValues.TryGetValue("collector", out var collect) && collect is ICollector<string> collector)
            {
                var message = JsonConvert.SerializeObject(new NotificationQueue(AuthInfo.IdCurrentTenant, "SAASA1")
                {
                    IdRecipients = recipients,
                    KeyValues = KeyValues
                });
                collector.Add(message);
            }
            return "";
        }
    }
}
