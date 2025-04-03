using System;
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
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnSchedule.AppointmentBooking
{
    public class APP28Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<APP28Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IRegister _registerService;
        private readonly IConfiguration _configuration;

        public APP28Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<APP28Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData, IRegister registerService) :
           base("APP28", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _registerService = registerService;
            _configuration = configuration;
            _logger = logger;

            PushNotificationData["action"] = "MD_APPROVAL";
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingparent/detail";

                _notificationData = new Dictionary<string, object>
                {
                    { "UrlBase", UrlBase },
                };

                var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
                var RescheduleInvitationEmail = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));
                var RescheduleInvitationEmailOld = RescheduleInvitationEmail.Where(e => e.Type == "Old").FirstOrDefault();
                var RescheduleInvitationEmailNew = RescheduleInvitationEmail.Where(e => e.Type == "New").FirstOrDefault();

                _notificationData["InvitationName"] = RescheduleInvitationEmailNew.InvitationName;
                _notificationData["StudentName"] = RescheduleInvitationEmailNew.StudentName;
                _notificationData["Date"] = RescheduleInvitationEmailNew.Date;
                _notificationData["Time"] = RescheduleInvitationEmailNew.Time;

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                GeneratedContent = pushTemplate(_notificationData);

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
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
                Notification = new FirebaseAdmin.Messaging.Notification
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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
            var RescheduleInvitationEmail = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));
            var RescheduleInvitationEmailOld = RescheduleInvitationEmail.Where(e => e.Type == "Old").FirstOrDefault();
            var RescheduleInvitationEmailNew = RescheduleInvitationEmail.Where(e => e.Type == "New").FirstOrDefault();

            _notificationData["InvitationName"] = RescheduleInvitationEmailNew.InvitationName;
            _notificationData["StudentName"] = RescheduleInvitationEmailNew.StudentName;
            _notificationData["Date"] = RescheduleInvitationEmailNew.Date;
            _notificationData["Time"] = RescheduleInvitationEmailNew.Time;

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            GeneratedContent = pushTemplate(_notificationData);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "RescheduleInvitationBookingEmail").Value;
            var RescheduleInvitationEmail = JsonConvert.DeserializeObject<List<EmailRescheduleResult>>(JsonConvert.SerializeObject(Object));
            var RescheduleInvitationEmailOld = RescheduleInvitationEmail.Where(e => e.Type == "Old").FirstOrDefault();
            var RescheduleInvitationEmailNew = RescheduleInvitationEmail.Where(e => e.Type == "New").FirstOrDefault();

            _notificationData["ParentName"] = User.Name;

            _notificationData["AcademicYearOld"] = RescheduleInvitationEmailOld.AcademicYear;
            _notificationData["InvitationNameOld"] = RescheduleInvitationEmailOld.InvitationName;
            _notificationData["InvitationDateOld"] = RescheduleInvitationEmailOld.InvitationDate;
            _notificationData["StudentNameOld"] = RescheduleInvitationEmailOld.StudentName;
            _notificationData["BinusianIDOld"] = RescheduleInvitationEmailOld.BinusianId;
            _notificationData["VenueOld"] = RescheduleInvitationEmailOld.Venue;
            _notificationData["DateOld"] = RescheduleInvitationEmailOld.Date;
            _notificationData["TimeOld"] = RescheduleInvitationEmailOld.Time;
            _notificationData["TeacherNameOld"] = RescheduleInvitationEmailOld.TeacherName;

            _notificationData["AcademicYearNew"] = RescheduleInvitationEmailNew.AcademicYear;
            _notificationData["InvitationNameNew"] = RescheduleInvitationEmailNew.InvitationName;
            _notificationData["InvitationDateNew"] = RescheduleInvitationEmailNew.InvitationDate;
            _notificationData["StudentNameNew"] = RescheduleInvitationEmailNew.StudentName;
            _notificationData["BinusianIDNew"] = RescheduleInvitationEmailNew.BinusianId;
            _notificationData["VenueNew"] = RescheduleInvitationEmailNew.Venue;
            _notificationData["DateNew"] = RescheduleInvitationEmailNew.Date;
            _notificationData["TimeNew"] = RescheduleInvitationEmailNew.Time;
            _notificationData["TeacherNameNew"] = RescheduleInvitationEmailNew.TeacherName;
            _notificationData["StudentName"] = RescheduleInvitationEmailNew.StudentName;
            _notificationData["InvitationName"] = RescheduleInvitationEmailNew.InvitationName;
            _notificationData["Link"] = $"{_notificationData["UrlBase"]}?id={RescheduleInvitationEmailNew.IdInvitationBooking}&IsFromEmail=true";
            //_notificationData["AcopEmail"] = RescheduleInvitationEmailNew.IdSchool == "1" ? "simprug-acop@binus.edu" : RescheduleInvitationEmailNew.IdSchool == "2" ? "schoolserpong.acop@binus.edu" : RescheduleInvitationEmailNew.IdSchool == "3" ? "acop.bekasi@binus.edu" : RescheduleInvitationEmailNew.IdSchool == "4" ? "schoolsemarang.acop@binus.edu" : "-";
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

            if (RescheduleInvitationEmailOld.IdSchool == "2")
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
