﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Api.User.FnUser;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.User.FnUser.Register;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.BreakSetting
{
    public class APP1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<APP1Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRegister _registerService;
        private readonly IConfiguration _configuration;

        public APP1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<APP1Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData, IRegister registerService) :
           base("APP1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _registerService = registerService;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "IBS_CREATE_TEACHER";
        }
        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = true,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingteacher";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingSettingEmail").Value;
                var GetInvitationBookingSettingEmail = JsonConvert.DeserializeObject<EmailInvitationBookingSettingResult>(JsonConvert.SerializeObject(Object));

                _notificationData["invitationName"] = GetInvitationBookingSettingEmail.InvitationName;
                _notificationData["dateInvitation"] = GetInvitationBookingSettingEmail.InvitationStartDate;
                //_notificationData["link"] = $"{_notificationData["UrlBase"]}?IdInvitationBookingSetting={GetInvitationBookingSettingEmail.IdInvitationBookingSetting}&IsFromEmail=true";

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);

                GeneratedContent = pushTemplate(_notificationData);
                GeneratedTitle = pushTitle(_notificationData);

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);

                return Task.CompletedTask;
            }
        }

        protected override async Task SendPushNotification()
        {
            var paramToken = new GetFirebaseTokenRequest
            {
                IdUserRecipient = IdUserRecipients.ToList(),
            };

            var apiGetToken = await _registerService.GetFirebaseToken(paramToken);
            var getFirebaseToken = apiGetToken.IsSuccess ? apiGetToken.Payload : null;
            if (getFirebaseToken == null)
                return;

            var tokens = getFirebaseToken.Select(e => e.Token).ToList();
            if (!EnsureAnyPushTokens(tokens))
                return;

            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new Notification
                {
                    Title = GeneratedTitle,
                    Body = GeneratedContent
                },
                Tokens = tokens,
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingSettingEmail").Value;
            var GetInvitationBookingSettingEmail = JsonConvert.DeserializeObject<EmailInvitationBookingSettingResult>(JsonConvert.SerializeObject(Object));

            _notificationData["invitationName"] = GetInvitationBookingSettingEmail.InvitationName;
            _notificationData["dateInvitation"] = GetInvitationBookingSettingEmail.InvitationStartDate;
            //_notificationData["link"] = $"{_notificationData["UrlBase"]}?IdInvitationBookingSetting={GetInvitationBookingSettingEmail.IdInvitationBookingSetting}&IsFromEmail=true";

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);

            GeneratedContent = pushTemplate(_notificationData);
            GeneratedTitle = pushTitle(_notificationData);

            var saveNotificationTasks = new List<Task>
            {
                NotificationManager.SaveNotification(
                    CreateNotificationHistory(
                        idUserRecipients,
                        isBlast,
                        GeneratedTitle ?? NotificationTemplate.Title,
                        GeneratedContent ?? NotificationTemplate.Push))
            };

            await Task.WhenAll(saveNotificationTasks);
        }

        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                .Where(x => IdUserRecipients.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var schoolName = await _dbContext.Entity<MsSchool>()
                            .Where(e => e.Id == IdSchool
                                    )
                            .Select(e => e.Name)
                            .FirstOrDefaultAsync(CancellationToken);

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingSettingEmail").Value;
            var GetInvitationBookingSettingEmail = JsonConvert.DeserializeObject<EmailInvitationBookingSettingResult>(JsonConvert.SerializeObject(Object));

            var dataTeacher = _dbContext.Entity<TrInvitationBookingSettingVenueDtl>()
                .Include(x => x.InvitationBookingSettingVenueDate)
                .Where(x => x.InvitationBookingSettingVenueDate.IdInvitationBookingSetting == GetInvitationBookingSettingEmail.IdInvitationBookingSetting)
                .Select(x => x.IdUserTeacher)
                .Distinct()
                .ToList();

            var dataUserTeacher = _dbContext.Entity<MsUser>()
                .Where(x => dataTeacher.Contains(x.Id))
                .Select(x => new
                {
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                })
                .ToList();

            foreach (var sendEmailTo in dataUserTeacher)
            {
                _notificationData["teacherName"] = sendEmailTo.EmailAddress.Name;

                _notificationData["invitationName"] = GetInvitationBookingSettingEmail.InvitationName;
                _notificationData["academicYear"] = GetInvitationBookingSettingEmail.AcademicYear;
                _notificationData["invitationStartDate"] = GetInvitationBookingSettingEmail.InvitationStartDate;
                _notificationData["invitationEndDate"] = GetInvitationBookingSettingEmail.InvitationEndDate;
                _notificationData["parentBookingStartDate"] = GetInvitationBookingSettingEmail.ParentBookingStartDate;
                _notificationData["parentBookingEndDate"] = GetInvitationBookingSettingEmail.ParentBookingEndDate;
                _notificationData["staffBookingStartDate"] = GetInvitationBookingSettingEmail.StaffBookingStartDate;
                _notificationData["staffBookingEndDate"] = GetInvitationBookingSettingEmail.StaffBookingEndDate;
                _notificationData["linkDetailInvitation"] = $"{_notificationData["UrlBase"]}/detailinvitation?IdInvitationBookingSetting={GetInvitationBookingSettingEmail.IdInvitationBookingSetting}&IsFromEmail=true";
                _notificationData["link"] = $"{_notificationData["UrlBase"]}/list";
                _notificationData["SchoolName"] = schoolName;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var titleBody = titleTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = titleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization
                        {
                            Tos = new List<EmailAddress>
                            {
                                sendEmailTo.EmailAddress
                            }
                        }
                    }
                };

                //message.AddBcc("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu");
                message.AddBcc("itdevschool@binus.edu", "itdevschool@binus.edu");

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));
            }

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}
