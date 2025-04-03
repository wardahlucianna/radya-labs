using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.User.FnBlocking.Blocking;
using BinusSchool.Persistence.UserDb.Abstractions;
using BinusSchool.Persistence.UserDb.Entities;
using BinusSchool.Persistence.UserDb.Entities.School;
using BinusSchool.Persistence.UserDb.Entities.Student;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace BinusSchool.User.FnBlocking.StudentBlocking.Notification
{
    public class BS1Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<BS1Notification> _logger;
        private readonly IUserDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public BS1Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<BS1Notification> logger, IUserDbContext dbContext, IDictionary<string, object> notificationData) :
           base("BS1", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            PushNotificationData["action"] = "BLOCKING_PAGE";
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
                var UrlBase = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/invitationbookingteacher/detailinvitation";

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
            var blockingStudentDatas = JsonConvert.DeserializeObject<List<GetBlockingNotifResult>>(JsonConvert.SerializeObject(KeyValues["studentBlockingData"]));
            var dataBlockingFeature = new List<string>();
            var result = new List<GetBlockingEmailResult>();
            if (blockingStudentDatas.Any(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE"))
            {
                foreach (var data in blockingStudentDatas.Where(x => x.BlockingTypeCategory.ToUpper() == "WEBSITE").ToList())
                {
                    result.Add(new GetBlockingEmailResult
                    {
                        IdStudent = data.IdStudent,
                        StudentName = data.StudentName,
                        DataParents = data.DataParents,
                        BlockingCategoryName = data.BlockingCategoryName,
                        BlockingData = "WEBSITE",
                        SchoolName = data.SchoolName,
                        Action = "",
                        Controller = "",
                    });
                }

            }
            else
            {
                var GetStudentBlockingFeature = blockingStudentDatas.Where(x => x.BlockingTypeCategory == "FEATURE");
                foreach (var item in GetStudentBlockingFeature.Select(x => new { x.IdStudent, x.BlockingCategoryName, x.StudentName, x.SchoolName, x.DataParents, x.Action, x.Controller }).Distinct().ToList())
                {
                    var dataFeature = GetStudentBlockingFeature.Where(x => x.IdSubFeature == null).Select(x => x.FeatureName).ToList();
                    var dataBLocking = string.Join(", ", dataFeature);
                    dataFeature = GetStudentBlockingFeature.Where(x => x.IdSubFeature != null).Select(x => x.SubFeatureName).ToList();
                    dataBLocking = dataBLocking + string.Join(", ", dataFeature);

                    result.Add(new GetBlockingEmailResult
                    {
                        IdStudent = item.IdStudent,
                        StudentName = item.StudentName,
                        DataParents = item.DataParents,
                        BlockingCategoryName = item.BlockingCategoryName,
                        BlockingData = dataBLocking,
                        SchoolName = item.SchoolName,
                        Action = !string.IsNullOrEmpty(item.Action) ? item.Action : "",
                        Controller = item.Controller
                    });
                }
            }
            var sendEmailTasks = new List<Task>();
            foreach (string recipment in IdUserRecipients)
            {
                var User = await _dbContext.Entity<MsUser>()
                    .Where(x => recipment.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        EmailAddress = new EmailAddress(x.Email, x.DisplayName),
                        Name = x.DisplayName,
                    })
                    .FirstOrDefaultAsync(CancellationToken);

                var blockingStudentData = result.Where(x => x.IdStudent == recipment).FirstOrDefault();
                var ParentName = string.Empty;
                foreach (var dataParent in blockingStudentData.DataParents)
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

                _notificationData["ParentName"] = ParentName;
                _notificationData["StudentName"] = blockingStudentData.StudentName;
                _notificationData["Data"] = result.Where(x => x.IdStudent == recipment).Select(x=> new {x.BlockingCategoryName, x.BlockingData}).Distinct().ToList();
                _notificationData["SchoolName"] = blockingStudentData.SchoolName;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                var finalEmail = new List<EmailAddress> {};

                finalEmail.Add(User.EmailAddress);
                foreach (var dataParent in blockingStudentData.DataParents)
                {
                    if (string.IsNullOrEmpty(dataParent.EmailParent))
                        continue;

                    finalEmail.Add(new EmailAddress
                    {
                        Email = dataParent.EmailParent,
                        Name = dataParent.ParentName,
                    });
                }

                // NOTE: create SendGridMessage object to send email
                var message = new SendGridMessage
                {
                    Subject = "Your access to the page has been blocked",
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

            var blockingStudentDatas = JsonConvert.DeserializeObject<List<GetBlockingNotifResult>>(JsonConvert.SerializeObject(KeyValues["studentBlockingData"]));

            var tokens = await _dbContext.Entity<MsUserPlatform>()
             .Where(x
                 => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                 && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
             .Select(x => new
             {
                 x.IdUser,
                 x.FirebaseToken
             }
             )
             .ToListAsync(CancellationToken);

            foreach (string recipment in IdUserRecipients)
            {
                var tokenByUser = tokens.Where(x => x.IdUser == recipment).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;

                var blockingStudentData = blockingStudentDatas.Where(x => x.IdStudent == recipment).ToList();

                if (blockingStudentData.Any(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE"))
                {
                    foreach (var data in blockingStudentData.Where(x => x.BlockingTypeCategory.ToUpper() == "WEBSITE").ToList())
                    {
                        _notificationData["blockingCategoryName"] = data.BlockingCategoryName;
                        _notificationData["blockingData"] = "WEBSITE";

                        var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                        GeneratedTitle = pushTitle(_notificationData);

                        // NOTE: create MulticastMessage object to send push notification
                        var message = new MulticastMessage
                        {
                            Notification = new FirebaseAdmin.Messaging.Notification
                            {
                                Title = GeneratedTitle,
                                Body = GeneratedContent
                            },
                            Tokens = tokenByUser,
                            Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                        };

                        // send push notification
                        SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));
                    }
                }
                else
                {
                    foreach (var data in blockingStudentData.Where(x => x.BlockingTypeCategory.ToUpper() == "FEATURE").ToList())
                    {
                        _notificationData["blockingCategoryName"] = data.BlockingCategoryName;
                        _notificationData["blockingData"] = !string.IsNullOrEmpty(data.SubFeatureName) ? data.SubFeatureName : data.FeatureName;

                        var url = string.IsNullOrEmpty(data.Action) ? $"/{data.Controller}" : $"/{data.Controller}/{data.Action}";
                        PushNotificationData["url"] = url;

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
                }
            }

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            var saveNotificationTasks = new List<Task>();

            var blockingStudentDatas = JsonConvert.DeserializeObject<List<GetBlockingNotifResult>>(JsonConvert.SerializeObject(KeyValues["studentBlockingData"]));

            foreach (string recipment in idUserRecipients)
            {
                var blockingStudentData = blockingStudentDatas.Where(x => x.IdStudent == recipment).ToList();

                if (blockingStudentData.Any(e => e.BlockingTypeCategory.ToUpper() == "WEBSITE"))
                {
                    foreach (var data in blockingStudentData.Where(x => x.BlockingTypeCategory.ToUpper() == "WEBSITE").ToList())
                    {
                        var IdRecipients = new List<string>
                        {
                            data.IdStudent,
                            $"P{ data.IdStudent}"
                        };

                        _notificationData["blockingCategoryName"] = data.BlockingCategoryName;
                        _notificationData["blockingData"] = "WEBSITE";

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
                    }
                }
                else
                {
                    var GetStudentBlockingFeature = blockingStudentData.Where(x => x.BlockingTypeCategory == "FEATURE").ToList();
                    foreach (var data in GetStudentBlockingFeature)
                    {
                        var IdRecipients = new List<string>
                        {
                            data.IdStudent,
                            $"P{ data.IdStudent}"
                        };

                        _notificationData["blockingCategoryName"] = data.BlockingCategoryName;
                        _notificationData["blockingData"] = !string.IsNullOrEmpty(data.SubFeatureName) ? data.SubFeatureName : data.FeatureName;

                        var url = string.IsNullOrEmpty(data.Action) ? $"/{data.Controller}" : $"/{data.Controller}/{data.Action}";
                        PushNotificationData["url"] = url;

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
                    }
                }
            }
            await Task.WhenAll(saveNotificationTasks);
        }
    }
}
