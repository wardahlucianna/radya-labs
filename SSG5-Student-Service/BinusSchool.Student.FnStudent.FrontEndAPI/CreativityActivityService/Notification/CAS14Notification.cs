using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Exceptions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Student.FnStudent.CreativityActivityService;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.Employee;
using BinusSchool.Persistence.StudentDb.Entities.School;
using BinusSchool.Persistence.StudentDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.CreativityActivityService
{
    public class CAS14Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<CAS14Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public CAS14Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<CAS14Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
           base("CAS14", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
        }
        protected override Task FetchNotificationConfig()
        {
            // TODO: get config from actual source
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = false,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected override Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/creativityactivityservice/teacherdetailexperience";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

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
            

                return;
        }
        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                 .Where(x => IdUserRecipients.Contains(x.Id))
                 .Select(x => new
                 {
                     x.Id,
                     EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                 })
                 .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetEmailSupervisor").Value;
            var GetEmailSupervisor = JsonConvert.DeserializeObject<EmailSupervisorResult>(JsonConvert.SerializeObject(Object));

            _notificationData["StudentName"] = GetEmailSupervisor.StudentName;
            _notificationData["SupervisorName"] = GetEmailSupervisor.SupervisorName;
            _notificationData["Username"] = User.EmailAddress.Email;
            _notificationData["LabelPassword"] = GetEmailSupervisor.IsLabelPassword==true?"Password: "+ GetEmailSupervisor.Password:"";
            _notificationData["Link"] = $"{_notificationData["UrlBase"]}?id={GetEmailSupervisor.Id}";

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
                    new Personalization { Tos = new List<EmailAddress> { User.EmailAddress } }
                }
            };

            if (NotificationTemplate.EmailIsHtml)
                message.HtmlContent = emailBody;
            else
                message.PlainTextContent = emailBody;

            sendEmailTasks.Add(NotificationManager.SendEmail(message));
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }

       
    }
}
