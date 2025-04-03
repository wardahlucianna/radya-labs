using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministrationV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin.Messaging;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Data.Model.Scoring.FnScoring.SendEmail.ApprovalByEmail;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Web;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.Notification
{
    public class ATD22Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly IConfiguration _configuration;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD22Notification> _logger;
        private List<ATD22NotificationModel> listEmailData;
        private string _schoolName;

        public ATD22Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD22Notification> logger,
            IAttendanceDbContext dbContext, IDictionary<string, object> notificationData) :
        base("ATD22", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
            _logger = logger;
            _configuration = configuration;
        }

        protected override Task FetchNotificationConfig()
        {
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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "AttendanceAdministration").Value;
            listEmailData = JsonConvert.DeserializeObject<List<ATD22NotificationModel>>(JsonConvert.SerializeObject(Object));
            _schoolName = _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).Select(x => x.Name).FirstOrDefault();

            _notificationData = new Dictionary<string, object>
            {
                { "SchoolName", _schoolName.ToUpper()},

            };

            return Task.CompletedTask;
        }

        protected override async Task SendEmailNotification()
        {
            try
            {
                if (KeyValues is null)
                {
                    _logger.LogInformation($"Skip sending notification. No data");
                    return;
                }

                if (IdUserRecipients is null)
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

                var User = await _dbContext.Entity<MsUser>()
                  .Where(x => IdUserRecipients.Contains(x.Id))
                  .Select(x => new
                  {
                      Id = x.Id,
                      DisplayName = x.DisplayName,
                      EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                  })
                  .ToListAsync(CancellationToken);


                var sendEmailTasks = new List<Task>();

                foreach (var idUser in IdUserRecipients)
                {
                    var sendTo = User.Where(e => e.Id == idUser).FirstOrDefault();

                    var listEmailDataByRecepient = listEmailData.Where(e => e.IdRecepient == idUser).ToList();
                    var ListCancel = listEmailDataByRecepient.SelectMany(e => e.ListCancel).ToList();

                    _notificationData["SendTo"] = sendTo.EmailAddress.Name;
                    _notificationData["Data"] = ListCancel;

                    var EmailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                    var EmailBody = EmailTemplate(_notificationData);

                    var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                    var TitleBody = TitleTemplate(_notificationData);


                    var message = new SendGridMessage
                    {
                        Subject = TitleBody,
                        Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { sendTo.EmailAddress } }
                    }
                    };

                    if (NotificationTemplate.EmailIsHtml)
                        message.HtmlContent = EmailBody;
                    else
                        message.PlainTextContent = EmailBody;

                    sendEmailTasks.Add(NotificationManager.SendEmail(message));
                }
                // send batch email
                await Task.WhenAll(sendEmailTasks);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            try
            {
                if (idUserRecipients is null)
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }

                var saveNotificationTasks = new List<Task>();

                var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var PushBody = PushTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);

                saveNotificationTasks.Add(NotificationManager.SaveNotification(
                    CreateNotificationHistory(
                        idUserRecipients,
                        isBlast,
                        TitleBody,
                        PushBody)));
                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendPushNotification()
        {

            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens))
                return;

            var SendPushNotificationTaks = new List<Task>();

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);

            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = TitleBody,
                    Body = TitleBody
                },
                Tokens = tokens,
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            // send push notification
            SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));

            await Task.WhenAll(SendPushNotificationTaks);
        }
    }
}
