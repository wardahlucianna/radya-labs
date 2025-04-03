using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Extensions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Api.School.FnSchool;
using BinusSchool.Data.Api.Util.FnNotification;
using BinusSchool.Data.Model.Attendance.FnAttendance.EmergencyAttendanceV2;
using BinusSchool.Data.Model.Scheduling.FnSchedule.SchoolEvent;
using BinusSchool.Data.Model.School.FnSchool.NotificationTemplate;
using BinusSchool.Data.Model.Scoring.FnScoring.Queue;
using BinusSchool.Data.Model.Util.FnNotification.SendGrid;
using BinusSchool.Data.Model.Util.FnNotification.SmtpEmail;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace BinusSchool.Attendance.FnLongRun.HttpTriggers
{
    public class SendEmailEmergencyAttendanceHandler : FunctionsHttpSingleHandler
    {
        // MsNotificationTemplate - DB SCHOOL
        private readonly string _notificationScenario = "ATD23";

        private readonly IAttendanceDbContext _dbContext;
        private readonly IServiceProvider _serviceProvider;
        private readonly IMachineDateTime _dateTime;
        private readonly ILogger<SendEmailEmergencyAttendanceHandler> _logger;
        private IDbContextTransaction _transaction;

        private readonly ISendGrid _sendGridApi;
        private readonly ISmtpEmail _smtpEmailApi;
        private readonly INotificationTemplate _notificationTemplate;

        private string _emailSubject = "";
        private string _emailTemplate = "";

        public SendEmailEmergencyAttendanceHandler(IAttendanceDbContext dbContext,
        IServiceProvider serviceProvider,
        IMachineDateTime dateTime,
        ILogger<SendEmailEmergencyAttendanceHandler> logger,
        ISendGrid sendGridApi,
        ISmtpEmail smtpEmailApi,
        INotificationTemplate notificationTemplate)
        {
            _dbContext = dbContext;
            _serviceProvider = serviceProvider;
            _dateTime = dateTime;
            _logger = logger;

            _sendGridApi = sendGridApi;
            _smtpEmailApi = smtpEmailApi;
            _notificationTemplate = notificationTemplate;
        }

        protected override async Task<ApiErrorResult<object>> Handler()
        {
            var param = await Request.GetBody<SendEmailEmergencyAttendanceRequest>();

            var result = await SendEmailEmergencyAttendance(param);

            return Request.CreateApiResult2(result as object);
        }

        public async Task<SendEmailEmergencyAttendanceResult> SendEmailEmergencyAttendance(SendEmailEmergencyAttendanceRequest paramstudents)
        {

            int succ = 0;
            int fail = 0;
            int limitsmpt = 10;

            var getEmergencyAttendance = await _dbContext.Entity<TrEmergencyAttendance>()
                                        .Include(x => x.EmergencyReport)
                                            .ThenInclude(y => y.AcademicYear)
                                        .Include(x => x.Student)
                                        .Where(a => paramstudents.studentEmergencyList.Select(b => b.ToString()).Contains(a.Id))
                                        .ToListAsync();

            var GetIdSchool = getEmergencyAttendance.First().EmergencyReport.AcademicYear.IdSchool;
            var GetSchoolName = await _dbContext.Entity<MsSchool>()
                                .Where(a => a.Id == GetIdSchool)
                                .FirstOrDefaultAsync();
            if(GetIdSchool == null)
            {
                _logger.LogInformation("[Queue] Data Cannot Processing. IdSchool not found");
                return (new SendEmailEmergencyAttendanceResult() { status = false, msg = "IdSchool not found" });
            }

            #region get email template
            var getEmailTemplateApi = await _notificationTemplate.GetNotificationTemplateScenario(new GetNotificationTemplateScenarioRequest
            {
                IdSchool = GetIdSchool,
                IdScenario = _notificationScenario
            });

            var getEmailTemplate = getEmailTemplateApi.Payload;

            if (getEmailTemplate == null)
            {
                _logger.LogInformation("[Queue] Data Cannot Processing. Template not found");
                return (new SendEmailEmergencyAttendanceResult() { status = false, msg = "Template not found" });
            }
            #endregion


            var emailSubject = getEmailTemplate.Title;
            var emailHtmlTemplate = getEmailTemplate.Email;

            _emailSubject = emailSubject;
            _emailTemplate = emailHtmlTemplate;



            var getEmailParent = await _dbContext.Entity<MsUser>()
                            .Where(x => getEmergencyAttendance.Select(b => "P"+b.IdStudent).Contains(x.Id))
                            .Select(x => new
                            {
                                x.Id,
                                EmailAddress = x.Email,
                                Name = x.DisplayName
                            })
                            .ToListAsync(CancellationToken);


            foreach (var stud in getEmergencyAttendance)
            {
                var getStudName = NameUtil.GenerateFullName(stud.Student.FirstName, stud.Student.MiddleName, stud.Student.LastName);
                var emailParentList = getEmailParent.Where(a => a.Id.Contains(stud.IdStudent))
                                    .Select(x => new SendSendGridEmailRequest_AddressBuilder
                                    {
                                        Address = x.EmailAddress,
                                        DisplayName = x.Name,
                                    })
                                    .ToList();

                var toListAll = new List<SendSendGridEmailRequest_AddressBuilder>();
                var ccAdditionalList = new List<SendSendGridEmailRequest_AddressBuilder>();
                var bccAdditionalList = new List<SendSendGridEmailRequest_AddressBuilder>();

                toListAll.AddRange(emailParentList);

                string primaryTo = "";
                foreach (var toPrimary in emailParentList)
                {
                    primaryTo += toPrimary.Address + ";";
                }

                var tempEmailTemplate = _emailTemplate
                                 .Replace("{{SchoolName}}", (GetSchoolName != null ? GetSchoolName.Name : "") )
                                 .Replace("{{StudentName}}", (!string.IsNullOrWhiteSpace(getStudName)? getStudName : "-"));


                var tempEmailSubject = _emailSubject
                                 .Replace("{{StudentName}}", (!string.IsNullOrWhiteSpace(getStudName) ? getStudName : "-"));


                // send email
                var sendEmail = await _sendGridApi.SendSendGridEmail(new SendSendGridEmailRequest
                {
                    IdSchool = GetIdSchool,
                    MessageContent = new SendSendGridEmailRequest_MessageContent
                    {
                        Subject = tempEmailSubject,
                        BodyHtml = tempEmailTemplate
                    },
                    RecepientConfiguration = new SendSendGridEmailRequest_RecepientConfiguration
                    {
                        ToList = toListAll,
                        CcList = ccAdditionalList,
                        BccList = bccAdditionalList
                    }
                });

                // save to log table
                if (sendEmail.Payload.IsSuccess)
                {
                    stud.SendEmailStatus = true;
                    succ++;
                }
                else
                {
                    if(limitsmpt > 0)
                    {
                        // second attempt send email using SMTP
                        var sendEmailSmtp = _smtpEmailApi.SendSmtpEmail(new SendSmtpEmailRequest
                        {
                            IdSchool = GetIdSchool,
                            MessageContent = new SendSmtpEmailRequest_MessageContent
                            {
                                Subject = "(S)" + tempEmailSubject,
                                BodyHtml = tempEmailTemplate
                            },
                            RecepientConfiguration = new SendSmtpEmailRequest_RecepientConfiguration
                            {
                                ToList = toListAll != null && toListAll.Count > 0 ?
                                            toListAll
                                            .Select(x => new SendSmtpEmailRequest_AddressBuilder
                                            {
                                                Address = x.Address,
                                                DisplayName = x.DisplayName
                                            })
                                            .ToList() : null
                            }
                        });

                        stud.SendEmailStatus = true;
                        succ++;
                        limitsmpt--;
                    }
                    else
                    {
                        stud.SendEmailStatus = false;
                        fail++;
                    }

                  
                }
             
            }

            await _dbContext.SaveChangesAsync(CancellationToken);

            SendEmailEmergencyAttendanceResult ReturnResult = new SendEmailEmergencyAttendanceResult();
            ReturnResult.successCount = succ;
            ReturnResult.failedCount = fail;
            ReturnResult.status = (succ> 0);
            ReturnResult.msg = string.Format("Proccess send email completed. {0} success, {1} failed.", succ.ToString(), fail.ToString());
            return ReturnResult;
        }

    }
}
