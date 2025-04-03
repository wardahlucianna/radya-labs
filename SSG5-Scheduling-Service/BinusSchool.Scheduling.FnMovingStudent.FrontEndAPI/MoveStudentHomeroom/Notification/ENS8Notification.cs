using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MovingStudentHomeroom;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentHomeroom.Notification
{
    public class ENS8Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ENS8Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private ENS8Result getMoveHomeroom = new ENS8Result();

        public ENS8Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ENS8Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("ENS8", notificationManager, configuration, logger)
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
                    Mobile = true,
                    Web = true
                }
            };

            return Task.CompletedTask;
        }

        protected async override Task Prepare()
        {
            try
            {
                //var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingparent/create2";

                _notificationData = new Dictionary<string, object>
                {
                    //{ "UrlBase", UrlBase },
                };

                var Object = KeyValues.FirstOrDefault(e => e.Key == "listMoveHomeroom").Value;
                getMoveHomeroom = JsonConvert.DeserializeObject<ENS8Result>(JsonConvert.SerializeObject(Object));


                _notificationData = new Dictionary<string, object>();

                var schoolName = await _dbContext.Entity<MsSchool>()
                                            .Where(e => e.Id == IdSchool
                                                    )
                                            .Select(e => e.Name)
                                            .FirstOrDefaultAsync(CancellationToken);

                _notificationData["SchoolName"] = schoolName;
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
                .Select(x => new { x.FirebaseToken, x.IdUser })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens.Select(e => e.FirebaseToken).ToList()))
                return;

            var SendPushNotificationTaks = new List<Task>();

            var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var PushBody = PushTemplate(_notificationData);

            var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
            var TitleBody = TitleTemplate(_notificationData);


            // NOTE: create MulticastMessage object to send push notification
            var message = new MulticastMessage
            {
                Notification = new FirebaseAdmin.Messaging.Notification
                {
                    Title = TitleBody,
                    Body = PushBody
                },
                Tokens = tokens.Select(e => e.FirebaseToken).ToList(),
                Data = (IReadOnlyDictionary<string, string>)PushNotificationData
            };

            GeneratedTitle = TitleBody;
            GeneratedContent = PushBody;

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            GeneratedContent = pushTemplate(_notificationData);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            GeneratedTitle = pushTitle(_notificationData);

            saveNotificationTasks.Add(NotificationManager.SaveNotification(
            CreateNotificationHistory(
                idUserRecipients,
                isBlast,
            GeneratedTitle ?? NotificationTemplate.Title,
            GeneratedContent ?? NotificationTemplate.Push)));
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
                    Name = x.DisplayName
                })
                .ToListAsync(CancellationToken);

            if (!User.Any())
                return;

            var sendEmailTasks = new List<Task>();
            var listUserEmail = User.Select(e => e.EmailAddress.Email).Distinct().ToList();

            var listUserCC = await _dbContext.Entity<MsUser>()
                .Where(x => getMoveHomeroom.IdUserCC.Contains(x.Id) && !listUserEmail.Contains(x.Email))
                .Select(x => new EmailAddress(x.Email, x.DisplayName))
                .ToListAsync(CancellationToken);

            foreach (var itemUser in User)
            {
                _notificationData["HomeroomTeacher"] = itemUser.Name;
                _notificationData["Data"] = getMoveHomeroom.MoveHomeroom;

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
                                itemUser.EmailAddress
                            },
                            Ccs = listUserCC.Count() > 0 ? listUserCC : null
                        }
                    }
                };

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
