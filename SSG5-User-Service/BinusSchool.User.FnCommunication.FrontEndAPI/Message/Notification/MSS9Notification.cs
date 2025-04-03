using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.School;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using System.IO;
using System.Net.Http;
using BinusSchool.Common.Model.Enums;
using System.Web;

namespace BinusSchool.User.FnCommunication.Message.Notification
{
    public class MSS9Notification : FunctionsNotificationHandler
    {
        private readonly IUserDbContext _dbContext;

        private readonly IConfiguration _configuration;

        private readonly INotificationManager _notificationManager;

        private IDictionary<string, object> _notificationData;

        private readonly ILogger<MSS9Notification> _logger;

        private readonly MemoryStream _memoryStream;

        private string _idMessage, _messageType, _schoolName;

        public MSS9Notification(string idScenario, INotificationManager notificationManager, IConfiguration configuration,
            ILogger<MSS9Notification> logger, IUserDbContext dbContext, IDictionary<string, object> notificationData)
            : base("MSS9", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
            _configuration = configuration;
            _notificationManager = notificationManager;
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
                var url = _configuration.GetSection("ClientApp:Web:Host").Get<string>();

                _schoolName = await _dbContext.Entity<MsSchool>()
                    .Where(x => x.Id == IdSchool)
                    .Select(x => x.Name.ToUpper())
                    .FirstOrDefaultAsync(CancellationToken);

                _notificationData = new Dictionary<string, object>(KeyValues)
                {
                    { "countMessage", "1" },
                    //{ "atttachment", "test attachment" },
                    { "linkUrl", url },
                    { "schoolName", _schoolName },
                };

                _idMessage = _notificationData.Where(e => e.Key == "idMessage").Select(e => e.Value).FirstOrDefault().ToString();

                _messageType = _notificationData.Where(e => e.Key == "typeMessage").Select(e => e.Value).FirstOrDefault().ToString();

                var messageBody = _dbContext.Entity<TrMessage>().Where(x => x.Id == _idMessage).Select(x => x.Content).FirstOrDefault();

                messageBody = HttpUtility.HtmlDecode(messageBody);

                if (!messageBody.Contains("<"))
                    messageBody = HttpUtility.HtmlDecode(messageBody);

                _notificationData.Add("message", messageBody);

                var pushTitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);

                GeneratedTitle = pushTitleTemplate(_notificationData);
                GeneratedContent = pushTemplate(_notificationData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        protected async override Task SendEmailNotification()
        {
            try
            {
                var sendEmailTasks = new List<Task>();

                //get email and idRecipient
                var recipient = await _dbContext.Entity<MsUser>()
                    .Where(x => IdUserRecipients.Contains(x.Id))
                    .Select(x => new
                    {
                        Id = x.Id,
                        EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                    })
                    .ToListAsync(CancellationToken);

                #region get file attachment message

                var listAttachmentMessage = await _dbContext.Entity<TrMessageAttachment>().Where(x => x.IdMessage == _idMessage).ToListAsync(CancellationToken);

                var data = new Dictionary<string, byte[]>();

                foreach (var item in listAttachmentMessage)
                {
                    if (!string.IsNullOrWhiteSpace(item.Url))
                    {
                        var client = new HttpClient();
                        var response = await client.GetAsync(item.Url);
                        var myByteArray = await response.Content.ReadAsByteArrayAsync();

                        data.Add(item.Id, myByteArray);
                    }
                }
                #endregion

                foreach (var iduserRecipient in IdUserRecipients)
                {
                    var recipientMail = recipient.Where(x => x.Id == iduserRecipient).Select(x => new EmailAddress
                    {
                        Email = x.EmailAddress.Email,
                        Name = x.EmailAddress.Name
                    }).FirstOrDefault();

                    if (string.IsNullOrEmpty(recipientMail.Email))
                        return;

                    var recipientName = await _dbContext.Entity<MsUser>().Where(x => x.Id == iduserRecipient)
                                        .Select(x => x.DisplayName).FirstOrDefaultAsync();

                    _notificationData.Add("recipientName", recipientName);

                    //subject title
                    var compileSubject = Handlebars.Compile(NotificationTemplate.Title);
                    var compileBody = Handlebars.Compile(NotificationTemplate.Email.Replace("{{message}}", _notificationData["message"].ToString()));

                    //NOTE: Create SendGrid object to send email
                    var message = new SendGridMessage
                    {
                        Subject = compileSubject(_notificationData),
                        Personalizations = new List<Personalization>
                            {
                                new Personalization
                                {
                                    Tos = new List<EmailAddress>()
                                    {
                                        new EmailAddress
                                        {
                                            Email = recipientMail.Email,
                                            Name = recipientMail.Name
                                        }
                                    },
                                    Ccs = null
                                }
                            }
                    };

                    if (data.Any())
                    {
                        message.Attachments = data.Select(x => new SendGrid.Helpers.Mail.Attachment
                        {
                            Filename = listAttachmentMessage.FirstOrDefault()?.Filename,
                            Content = Convert.ToBase64String(x.Value),
                            Type = GetContentType(listAttachmentMessage.FirstOrDefault(y => y.Id == x.Key)?.Filetype),
                            Disposition = "attachment"
                        }).ToList();
                    }

                    if (NotificationTemplate.EmailIsHtml)
                        message.HtmlContent = compileBody(_notificationData);
                    else
                        message.PlainTextContent = compileBody(_notificationData);

                    // send email
                    sendEmailTasks.Add(NotificationManager.SendEmail(message));

                    _notificationData.Remove("recipientName");
                }
                await Task.WhenAll(sendEmailTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected async override Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens))
                return;

            //data push notif
            PushNotificationData["action"] = "MSS_INBOX";
            PushNotificationData["id"] = _idMessage;
            PushNotificationData["mgType"] = _messageType;
            PushNotificationData["type"] = NotificationType.Message.ToString();

            var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
            var pushBody = pushTemplate(_notificationData);

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

            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();

            //data push notif
            PushNotificationData["action"] = "MSS_INBOX";
            PushNotificationData["id"] = _idMessage;
            PushNotificationData["mgType"] = _messageType;
            PushNotificationData["type"] = NotificationType.Message.ToString();

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

        //private Stream GetSteam(MemoryStream ms)
        //{
        //    ms.Flush();

        //    ms.Seek(0,SeekOrigin.Begin);

        //    return ms;
        //}

        private string GetContentType(string ext)
        {
            switch (ext.Replace(".", "").ToLower())
            {
                case "pdf":
                    return "application/pdf";
                case "jpg":
                    return "image/jpeg";
                case "jpeg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "gif":
                    return "image/gif";
                case "doc":
                    return "application/msword";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "xls":
                    return "application/vnd.ms-excel";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "csv":
                    return "text/csv";
                case "ppt":
                    return "application/vnd.ms-powerpoint";
                case "pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "vsd":
                    return "application/vnd.visio";
                case "txt":
                    return "text/plain";
                case "zip":
                    return "application/zip";
                case "rar":
                    return "application/vnd.rar";
                case "jar":
                    return "application/java-archive";
                case "mp4":
                    return "video/mp4";
                case "mp3":
                    return "audio/mpeg";
                default:
                    return "application/octet-stream";
            }
        }
    }
}
