using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Scheduling.FnMovingStudent.MoveStudentEnrollment;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Scheduling.FnMovingStudent.MoveStudentEnrollment.Notification
{
    public class EMS1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<EMS1Notification> _logger;
        private readonly ISchedulingDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public EMS1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<EMS1Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) :
           base("EMS1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;

            //PushNotificationData["action"] = "EMS_INSERT";
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
                //var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingparent/create2";

                _notificationData = new Dictionary<string, object>
                {
                    //{ "UrlBase", UrlBase },
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

            var userEmails = new List<EmailAddress>();

            var distinctUserEmailTO = User.GroupBy(x => x.EmailAddress.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

            if (!distinctUserEmailTO.Any())
            {
                userEmails.AddRange(User.Select(x => x.EmailAddress).ToList());
            }
            else
            {
                var userEmailAddress = User.Where(x => !distinctUserEmailTO.Contains(x.EmailAddress.Email)).ToList();

                foreach (var emailTo in distinctUserEmailTO)
                {
                    var usrEmail = User.Where(x => x.EmailAddress.Email == emailTo).FirstOrDefault();
                    userEmails.Add(usrEmail.EmailAddress);
                }
                userEmails.AddRange(userEmailAddress.Select(x => x.EmailAddress));
            }

            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "EmailMoveStudentEnrollment").Value;
            var EmailMoveStudentEnrollment = JsonConvert.DeserializeObject<EmailMoveStudentEnrollmentRequest>(JsonConvert.SerializeObject(Object));

            var listUserId = User.Select(e => e.Id).ToList();
            var listCC = new List<EmailAddress>();

            var UserCC = await _dbContext.Entity<MsUser>()
                .Where(x => EmailMoveStudentEnrollment.IdUserCc.Contains(x.Id) && !listUserId.Contains(x.Email))
                .Select(x => new EmailAddress(x.Email, x.DisplayName))
                //.Select(x => new EmailAddress("dev.apsedon@dispostable.com", x.DisplayName))
                .ToListAsync(CancellationToken);

            var distinctUserEmailCC = UserCC.GroupBy(x => x.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();

            if (!distinctUserEmailCC.Any())
            {
                listCC.AddRange(UserCC);
            }

            var duplicateEmail = UserCC.GroupBy(x => x.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
            if (duplicateEmail.Count() != 0)
            {
                UserCC.RemoveAll(x => userEmails.Select(x => x.Email).Contains(x.Email));

                listCC.AddRange(UserCC.Select(x => x));
            }
            else
            {
                var listCCUnionUserEmail = listCC.Concat(userEmails).ToList();
                var duplicateCC = listCCUnionUserEmail.GroupBy(x => x.Email).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                listCC.RemoveAll(x => duplicateCC.Contains(x.Email));
            }

            foreach (var itemUser in userEmails)
            {
                _notificationData["SubjectTeacher"] = itemUser.Name;

                _notificationData["AcademicYear"] = EmailMoveStudentEnrollment.academicYear;
                _notificationData["Semester"] = EmailMoveStudentEnrollment.semester;
                _notificationData["Homeroom"] = EmailMoveStudentEnrollment.homeRoom;
                _notificationData["StudentName"] = $"{EmailMoveStudentEnrollment.studentId} - {EmailMoveStudentEnrollment.studentName}";
                _notificationData["NewSubject"] = EmailMoveStudentEnrollment.newSubject;
                _notificationData["NewSubjectLevel"] = EmailMoveStudentEnrollment.newSubjectLevel;
                _notificationData["OldSubject"] = EmailMoveStudentEnrollment.oldSubject;
                _notificationData["OldSubjectLevel"] = EmailMoveStudentEnrollment.oldSubjectLevel;
                _notificationData["EffectiveDate"] = EmailMoveStudentEnrollment.effectiveDate;
                _notificationData["Note"] = EmailMoveStudentEnrollment.note;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var titleBody = titleTemplate(_notificationData);


                //var UserCC = await _dbContext.Entity<MsUser>()
                //  .Where(x => EmailMoveStudentEnrollment.IdUserCc.Contains(x.Id) && !listUserId.Contains(x.Email))
                //  //.Select(x => new EmailAddress(x.Email, x.DisplayName))
                //  .Select(x => new EmailAddress("dev.apsedon@dispostable.com", x.DisplayName))
                //  .ToListAsync(CancellationToken);

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
                                itemUser
                            },
                            Ccs = listCC.Count() > 0 ? listCC : null
                        }
                    }
                };
                //if (UserCC.Any())
                //    message.AddBccs(UserCC);

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
