using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
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
    public class APP27Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<APP27Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public APP27Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<APP27Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("APP27", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "IB_PARENT_LIST";
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingparent/list";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["SchoolName"] = school.Name.ToUpper();
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingParentEmail").Value;
            var GetInvitationBookingEmail = JsonConvert.DeserializeObject<EmailInvitatinBookingParentResult>(JsonConvert.SerializeObject(Object));

            var SendPushNotificationTaks = new List<Task>();

            _notificationData["InvitationName"] = GetInvitationBookingEmail.InvitationName;
            _notificationData["StudentName"] = GetInvitationBookingEmail.StudentName;
            _notificationData["Date"] = GetInvitationBookingEmail.Date;
            _notificationData["Time"] = GetInvitationBookingEmail.Time;

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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingParentEmail").Value;
            var GetInvitationBookingEmail = JsonConvert.DeserializeObject<EmailInvitatinBookingParentResult>(JsonConvert.SerializeObject(Object));

            var saveNotificationTasks = new List<Task>();

            _notificationData["InvitationName"] = GetInvitationBookingEmail.InvitationName;
            _notificationData["StudentName"] = GetInvitationBookingEmail.StudentName;
            _notificationData["Date"] = GetInvitationBookingEmail.Date;
            _notificationData["Time"] = GetInvitationBookingEmail.Time;

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
                    Name = x.DisplayName,
                })
                .FirstOrDefaultAsync(CancellationToken);

            if (User == null)
                return;

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetInvitationBookingParentEmail").Value;
            var GetInvitationBookingEmail = JsonConvert.DeserializeObject<EmailInvitatinBookingParentResult>(JsonConvert.SerializeObject(Object));
            
            var UserTeacher = await _dbContext.Entity<MsUser>()
                .Where(x => x.Id == GetInvitationBookingEmail.IdTeacher)
                .Select(x => new
                {
                    x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                    Name = x.DisplayName,
                })
                .FirstOrDefaultAsync(CancellationToken);

            _notificationData["ParentName"] = User.Name;

            _notificationData["AcademicYear"] = GetInvitationBookingEmail.AcademicYear;
            _notificationData["InvitationName"] = GetInvitationBookingEmail.InvitationName;
            _notificationData["InvitationDate"] = GetInvitationBookingEmail.InvitationDate;
            _notificationData["StudentName"] = GetInvitationBookingEmail.StudentName;
            _notificationData["BinusianID"] = GetInvitationBookingEmail.BinusianId;
            _notificationData["Venue"] = GetInvitationBookingEmail.Venue;
            _notificationData["Date"] = GetInvitationBookingEmail.Date;
            _notificationData["Time"] = GetInvitationBookingEmail.Time;
            _notificationData["TeacherName"] = UserTeacher.Name;
            _notificationData["Link"] = $"{_notificationData["UrlBase"]}?IsFromEmail=true";
            //_notificationData["AcopEmail"] = GetInvitationBookingEmail.IdSchool == "1" ? "simprug-acop@binus.edu" : GetInvitationBookingEmail.IdSchool == "2" ? "schoolserpong.acop@binus.edu" : GetInvitationBookingEmail.IdSchool == "3" ? "acop.bekasi@binus.edu" : GetInvitationBookingEmail.IdSchool == "4" ? "schoolsemarang.acop@binus.edu" : "-";

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
                            User.EmailAddress
                        }
                    }
                }
            };

            //message.AddBcc("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu");
            message.AddBcc("itdevschool@binus.edu", "itdevschool@binus.edu");
            //message.AddBcc("fambi@radyalabs.id", "fambi@radyalabs.id");
            //message.AddBcc("tri@radyalabs.com", "tri@radyalabs.com");
            //message.AddBcc("riki@radyalabs.id", "riki@radyalabs.id");

            if (GetInvitationBookingEmail.IdSchool == "2")
                message.AddBcc("schoolserpong.acop@binus.edu", "schoolserpong.acop@binus.edu");

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
