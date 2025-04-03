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
    public class PAR3Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<PAR3Notification> _logger;
        private readonly IStudentDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public PAR3Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<PAR3Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
        base("PAR3", notificationManager, configuration, logger)
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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDataCourseworkAnecdotalCommentEmail").Value;
            var GetDataCourseworkAnecdotalEmails = JsonConvert.DeserializeObject<List<EmailCommentPortofolioResult>>(JsonConvert.SerializeObject(Object));

            foreach (var receipment in IdUserRecipients)
            {
                var User = await _dbContext.Entity<MsUser>()
                    .Where(x => x.Id == receipment)
                    .Select(x => new
                    {
                        x.Id,
                        EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                        Name = x.DisplayName,
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                var GetDataCourseworkAnecdotalEmail = GetDataCourseworkAnecdotalEmails.Where(x => x.IdReceive == User.Id).FirstOrDefault();

                _notificationData["userNameComment"] = GetDataCourseworkAnecdotalEmail.UserNameComment;
                _notificationData["tabMenu"] = GetDataCourseworkAnecdotalEmail.TabMenu;
                _notificationData["userNameCreate"] = GetDataCourseworkAnecdotalEmail.UserNameCreate;
                _notificationData["Comment"] = GetDataCourseworkAnecdotalEmail.Comment;
                _notificationData["schoolname"] = GetDataCourseworkAnecdotalEmail.SchoolName;

                var finalEmail = new List<EmailAddress> { };

                if (GetDataCourseworkAnecdotalEmail.ForWho == "Student")
                {
                    finalEmail.Add(User.EmailAddress);
                    _notificationData["receiverName"] = User.Name;
                    _notificationData["Link"] = $"{_notificationData["UrlBase"]}/studentportfolio?tabbing={GetDataCourseworkAnecdotalEmail.TabMenuIdx}";
                }
                else if(GetDataCourseworkAnecdotalEmail.ForWho == "Parent")
                {
                    var ParentName = string.Empty;
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
                    _notificationData["receiverName"] = ParentName;
                    _notificationData["Link"] = $"{_notificationData["UrlBase"]}/parentportfolio?tabbing={GetDataCourseworkAnecdotalEmail.TabMenuIdx}";
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
                }
                else
                {
                    finalEmail.Add(User.EmailAddress);
                    _notificationData["receiverName"] = User.Name;
                    _notificationData["Link"] = $"{_notificationData["UrlBase"]}/detailteacherportfolio?ay={GetDataCourseworkAnecdotalEmail.IdAcademicYear}&id={GetDataCourseworkAnecdotalEmail.IdStudent}&semester={GetDataCourseworkAnecdotalEmail.Semester}&tabbing={GetDataCourseworkAnecdotalEmail.TabMenuIdx}";
                }

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = $"New comment on {GetDataCourseworkAnecdotalEmail.Date}",
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
                var tokenByUser = tokens.Where(x => x.IdUser == receiptment).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;

                var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDataCourseworkAnecdotalCommentEmail").Value;
                var GetDataCourseworkAnecdotalEmails = JsonConvert.DeserializeObject<List<EmailCommentPortofolioResult>>(JsonConvert.SerializeObject(Object));

                var GetDataCourseworkAnecdotalEmail = GetDataCourseworkAnecdotalEmails.Where(x => x.IdReceive == receiptment).FirstOrDefault();


                if (GetDataCourseworkAnecdotalEmail.ForWho == "Student")
                {
                    PushNotificationData["action"] = "PORTOFOLIO_STUDENT";
                }
                else if (GetDataCourseworkAnecdotalEmail.ForWho == "Parent")
                {
                    PushNotificationData["action"] = "PORTOFOLIO_PARENT";
                }
                else
                {
                    PushNotificationData["action"] = "PORTOFOLIO_TEACHER";
                    PushNotificationData["ay"] = GetDataCourseworkAnecdotalEmail.IdAcademicYear;
                    PushNotificationData["id"] = GetDataCourseworkAnecdotalEmail.IdStudent;
                    PushNotificationData["semester"] = GetDataCourseworkAnecdotalEmail.Semester.ToString();
                    PushNotificationData["tabbing"] = GetDataCourseworkAnecdotalEmail.TabMenuIdx;
                }
                _notificationData["tabMenu"] = GetDataCourseworkAnecdotalEmail.TabMenu;
                _notificationData["date"] = $"{GetDataCourseworkAnecdotalEmail.Date:dd MMM yyyy} - {string.Format("{0:hh:mm:ss tt}", GetDataCourseworkAnecdotalEmail.Date)}";
                //_notificationData["link"] = $"{_notificationData["UrlBase"]}?ay={GetDataCourseworkAnecdotalEmail.IdAcademicYear}&id={GetDataCourseworkAnecdotalEmail.IdStudent}&semester={GetDataCourseworkAnecdotalEmail.Semester}";

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

            var Object = KeyValues.FirstOrDefault(e => e.Key == "GetDataCourseworkAnecdotalCommentEmail").Value;
            var GetDataCourseworkAnecdotalEmails = JsonConvert.DeserializeObject<List<EmailCommentPortofolioResult>>(JsonConvert.SerializeObject(Object));

            foreach (var receiptment in IdUserRecipients)
            {
                var GetDataCourseworkAnecdotalEmail = GetDataCourseworkAnecdotalEmails.Where(x => x.IdReceive == receiptment).FirstOrDefault();

                _notificationData["tabMenu"] = GetDataCourseworkAnecdotalEmail.TabMenu;
                _notificationData["date"] = $"{GetDataCourseworkAnecdotalEmail.Date:dd MMM yyyy} - {string.Format("{0:hh:mm:ss tt}", GetDataCourseworkAnecdotalEmail.Date)}";
                //_notificationData["link"] = $"{_notificationData["UrlBase"]}?ay={GetDataCourseworkAnecdotalEmail.IdAcademicYear}&id={GetDataCourseworkAnecdotalEmail.IdStudent}&semester={GetDataCourseworkAnecdotalEmail.Semester}&IsFromEmail=true";
                if (GetDataCourseworkAnecdotalEmail.ForWho == "Student")
                {
                    PushNotificationData["action"] = "PORTOFOLIO_STUDENT";
                }
                else if (GetDataCourseworkAnecdotalEmail.ForWho == "Parent")
                {
                    PushNotificationData["action"] = "PORTOFOLIO_PARENT";
                }
                else
                {
                    PushNotificationData["action"] = "PORTOFOLIO_TEACHER";
                    PushNotificationData["ay"] = GetDataCourseworkAnecdotalEmail.IdAcademicYear;
                    PushNotificationData["id"] = GetDataCourseworkAnecdotalEmail.IdStudent;
                    PushNotificationData["semester"] = GetDataCourseworkAnecdotalEmail.Semester.ToString();
                    PushNotificationData["tabbing"] = GetDataCourseworkAnecdotalEmail.TabMenuIdx;
                }
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
            }

            await Task.WhenAll(saveNotificationTasks);
        }
    }
}
