using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Data.Model.Student.FnStudent.StudentDemographicsReport;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking.Notification
{
    public class ENS1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ENS1Notification> _logger;
        private readonly IConfiguration _configuration;
        //private readonly IMachineDateTime _datetime;
        private readonly ISchedulingDbContext _dbContext;
        private ENS1Result getInvitationBookingSetting = new ENS1Result();
        public ENS1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ENS1Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("ENS1", notificationManager, configuration, logger)
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

        protected override async Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingsetting/detailinvitation";

                _notificationData = new Dictionary<string, object>();

                var schoolName = await _dbContext.Entity<MsSchool>()
                                            .Where(e => e.Id == IdSchool
                                                    )
                                            .Select(e => e.Name)
                                            .FirstOrDefaultAsync(CancellationToken);

                _notificationData["SchoolName"] = schoolName;

                var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingSettingVenue").Value;
                getInvitationBookingSetting = JsonConvert.DeserializeObject<ENS1Result>(JsonConvert.SerializeObject(Object));

                getInvitationBookingSetting.InvitationBookingSettingNew.ForEach(e => e.Link = $"{UrlBase}?idInvitationBookingSetting={getInvitationBookingSetting.IdInvitationBookingSetting}&canEdit=false&canDelete=false");

                var listIdParent = await _dbContext.Entity<TrInvitationBookingSettingUser>()
                                       .Include(e => e.HomeroomStudent)
                                       .Where(e => e.IdInvitationBookingSetting == getInvitationBookingSetting.IdInvitationBookingSetting)
                                       .Select(e => "P" + e.HomeroomStudent.IdStudent)
                                       .Distinct()
                                       .ToListAsync(CancellationToken);

                if (listIdParent.Any())
                    getInvitationBookingSetting.IdRecepient.AddRange(listIdParent);

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
            _notificationData["InvitationName"] = getInvitationBookingSetting.InvitationName;

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
                _notificationData["InvitationName"] = getInvitationBookingSetting.InvitationName;

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

        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                  .Where(x => IdUserRecipients.Contains(x.Id))
                  .Select(x => new
                  {
                      x.Id,
                      EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                  })
                  .ToListAsync(CancellationToken);

            if (!User.Any())
                return;

            var sendEmailTasks = new List<Task>();
            foreach (var IdRecepient in getInvitationBookingSetting.IdRecepient)
            {
                var listIdUser = IdRecepient;
                var GetUserById = User.Where(e => e.Id == listIdUser).FirstOrDefault();
                if (GetUserById == null)
                    continue;

                _notificationData["RecipientName"] = GetUserById.EmailAddress.Name;
                _notificationData["DataOld"] = getInvitationBookingSetting.InvitationBookingSettingOld;
                _notificationData["DataNew"] = getInvitationBookingSetting.InvitationBookingSettingNew;

                var EmailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var EmailBody = EmailTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);


                var message = new SendGridMessage
                {
                    Subject = TitleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { GetUserById.EmailAddress } }
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
    }

}
