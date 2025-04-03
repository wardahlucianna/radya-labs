using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Student.FnStudent.Portfolio.Coursework;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Student.FnStudent.Portfolio.Coursework.Notification
{
    public class PAR2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<PAR2Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PAR2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<PAR2Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
        base("PAR2", notificationManager, configuration, logger)
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

        protected override Task Prepare()
        {
            try
            {
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/portfolio";

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

        protected override async Task SendEmailNotification()
        {
            var sendEmailTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDataCourseworkAnecdotalEmail").Value;
            var GetDataCourseworkAnecdotalEmail = JsonConvert.DeserializeObject<EmailCourseworkResult>(JsonConvert.SerializeObject(Object));


            foreach (var receipment in IdUserRecipients)
            {
                var ParentName = string.Empty;
                var User = await _dbContext.Entity<MsUser>()
                    .Where(x => receipment.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                        Name = x.DisplayName,
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                foreach (var dataParent in GetDataCourseworkAnecdotalEmail.DataParents)
                {
                    if (string.IsNullOrEmpty(dataParent.ParentName))
                        continue;

                    if (string.IsNullOrEmpty(ParentName))
                    {
                        ParentName = dataParent.ParentName;
                    }
                    else
                    {
                        ParentName += $" ,{dataParent.ParentName}";
                    }
                }

                var finalEmail = new List<EmailAddress> { };

                if (GetDataCourseworkAnecdotalEmail.ForStudent)
                {
                    finalEmail.Add(User.EmailAddress);
                    _notificationData["Link"] = $"{_notificationData["UrlBase"]}/studentportfolio?tabbing=Coursework";
                }
                else
                {
                    foreach (var dataParent in GetDataCourseworkAnecdotalEmail.DataParents)
                    {
                        if (string.IsNullOrEmpty(dataParent.EmailParent))
                            continue;

                        finalEmail.Add(new EmailAddress
                        {
                            Email = dataParent.EmailParent,
                            Name = dataParent.ParentName,
                        });
                    }
                    _notificationData["Link"] = $"{_notificationData["UrlBase"]}/parentportfolio?tabbing=Coursework";
                }

                _notificationData["parentName"] = ParentName;
                _notificationData["studentName"] = User.Name;
                _notificationData["teacherName"] = GetDataCourseworkAnecdotalEmail.TeacherName;
                _notificationData["UOI"] = GetDataCourseworkAnecdotalEmail.UOI.Description;
                _notificationData["Content"] = GetDataCourseworkAnecdotalEmail.Content;
                _notificationData["schoolname"] = GetDataCourseworkAnecdotalEmail.SchoolName;


                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var titleBody = titleTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = $"{GetDataCourseworkAnecdotalEmail.TeacherName} posted Anecdotal Records on {GetDataCourseworkAnecdotalEmail.Date}",
                    Personalizations = new List<Personalization>
                        {
                            new Personalization { Tos = finalEmail }
                        }
                };
                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));



                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = emailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));
            }
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }

        protected override async Task SendPushNotification()
        {
            var SendPushNotificationTaks = new List<Task>();

            var tokens = await _dbContext.Entity<MsUserPlatform>()
                    .Where(x
                        => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                        && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                    .Select(x => new { x.FirebaseToken, x.IdUser })
                    .ToListAsync(CancellationToken);

            foreach (var receiptment in IdUserRecipients)
            {
                var userReceiptment = receiptment;
                var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDataCourseworkAnecdotalEmail").Value;
                var GetDataCourseworkAnecdotalEmail = JsonConvert.DeserializeObject<EmailCourseworkResult>(JsonConvert.SerializeObject(Object));
                if (GetDataCourseworkAnecdotalEmail.ForStudent)
                {
                    PushNotificationData["action"] = "PORTOFOLIO_STUDENT";
                }
                else
                {
                    PushNotificationData["action"] = "PORTOFOLIO_PARENT";
                    userReceiptment = $"P{receiptment}";
                }
                var tokenByUser = tokens.Where(x => x.IdUser == userReceiptment).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;

                PushNotificationData["tabbing"] = "Coursework";

                _notificationData["teacherName"] = GetDataCourseworkAnecdotalEmail.TeacherName;
                _notificationData["date"] = $"{GetDataCourseworkAnecdotalEmail.Date:dd MMM yyyy} - {string.Format("{0:hh:mm:ss tt}", GetDataCourseworkAnecdotalEmail.Date)}";
                //_notificationData["link"] = $"{_notificationData["UrlBase"]}?ay={GetDataCourseworkAnecdotalEmail.IdAcademicYear}&id={GetDataCourseworkAnecdotalEmail.IdStudent}&semester={GetDataCourseworkAnecdotalEmail.Semester}&IsFromEmail=true";

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
                    Tokens = tokenByUser,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                GeneratedTitle = TitleBody;
                GeneratedContent = PushBody;

                // send push notification
                SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));
            }

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDataCourseworkAnecdotalEmail").Value;
            var GetDataCourseworkAnecdotalEmail = JsonConvert.DeserializeObject<EmailCourseworkResult>(JsonConvert.SerializeObject(Object));

            foreach (var receiptment in IdUserRecipients)
            {
                var userReceipment = receiptment;
                if (GetDataCourseworkAnecdotalEmail.ForStudent == false)
                {
                    userReceipment = $"P{receiptment}";
                }
                var IdRecipients = new List<string>
                        {
                            userReceipment
                        };
                _notificationData["teacherName"] = GetDataCourseworkAnecdotalEmail.TeacherName;
                _notificationData["date"] = $"{GetDataCourseworkAnecdotalEmail.Date:dd MMM yyyy} - {string.Format("{0:hh:mm:ss tt}", GetDataCourseworkAnecdotalEmail.Date)}";
                PushNotificationData["tabbing"] = "Coursework";
                //_notificationData["link"] = $"{_notificationData["UrlBase"]}?ay={GetDataCourseworkAnecdotalEmail.IdAcademicYear}&id={GetDataCourseworkAnecdotalEmail.IdStudent}&semester={GetDataCourseworkAnecdotalEmail.Semester}&IsFromEmail=true";

                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                GeneratedContent = pushTemplate(_notificationData);

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                GeneratedTitle = pushTitle(_notificationData);

                saveNotificationTasks.Add(NotificationManager.SaveNotification(
                CreateNotificationHistory(
                    IdRecipients,
                    isBlast,
                GeneratedTitle ?? NotificationTemplate.Title,
                GeneratedContent ?? NotificationTemplate.Push)));

                await Task.WhenAll(saveNotificationTasks);
            }
        }
    }
}
