using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.TeachingDb.Abstractions;
using BinusSchool.Persistence.TeachingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Teaching.FnLessonPlan.LessonPlan.Notification
{
    public class LP2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;

        private readonly ITeachingDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<LP2Notification> _logger;

        protected string IdLessonPlan, IdLessonPlanDocument;
        

        public LP2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<LP2Notification> logger, ITeachingDbContext dbContext) :
        base("LP2", notificationManager, configuration, logger)
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
                IdLessonPlan = KeyValues.Where(e => e.Key == "IdLessonPlan").Select(e => e.Value).SingleOrDefault().ToString();
                IdLessonPlanDocument = KeyValues.Where(e => e.Key == "IdLessonPlanDocument").Select(e => e.Value).SingleOrDefault().ToString();

                var urlApproveDecline = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/lessonplan/lessonplandocumentdetail?idLessonPlan={IdLessonPlan}&idLessonPlanDocument={IdLessonPlanDocument}";
                var url = $"{_configuration.GetSection("ClientApp:Web:Host").Get<string>()}/lessonplan/approval";

                _notificationData = new Dictionary<string, object>(KeyValues)
                {
                    { "Link", url },
                    { "LinkApprove", urlApproveDecline },
                    { "LinkDecline", urlApproveDecline },
                };

                // remove additional dictionary
                KeyValues.Remove("IdLessonPlan");
                KeyValues.Remove("IdLessonPlanDocument");

                var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
                var pushContent = Handlebars.Compile(NotificationTemplate.Push);

                GeneratedTitle = pushTitle(_notificationData);
                GeneratedContent = pushContent(_notificationData);

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
            try
            {
                if (KeyValues is null)
                {
                    _logger.LogInformation($"Skip sending notification. No data");
                    return;
                }

                if (IdUserRecipients is null)
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");

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

                var sendPushTasks = new List<Task>();
                PushNotificationData["action"] = "LP_APPROVAL";
                PushNotificationData["idLessonPlan"] = IdLessonPlan;
                PushNotificationData["idLessonPlanDocument"] = IdLessonPlanDocument;
                PushNotificationData["isFromApproval"] = "true";

                foreach (var idReceipment in IdUserRecipients)
                {
                    var tokenByUser = tokens.Where(x => x.IdUser == idReceipment).Select(x => x.FirebaseToken).ToList();
                    if (!EnsureAnyPushTokens(tokenByUser))
                        return;

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
                    sendPushTasks.Add(NotificationManager.SendPushNotification(message));
                }

                await Task.WhenAll(sendPushTasks);
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
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

                var staff = await _dbContext.Entity<MsUser>()
                .Where(x => IdUserRecipients.Contains(x.Id))
                .Select(x => new
                {
                    Id = x.Id,
                    EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                })
                .ToListAsync(CancellationToken);

                foreach (var idUser in IdUserRecipients)
                {
                    var staffMail = staff.Where(x => x.Id == idUser).Select(x => new Address
                    {
                        EmailAddress = x.EmailAddress.Email,
                        Name = x.EmailAddress.Name
                    }).FirstOrDefault();

                    if (string.IsNullOrEmpty(staffMail.EmailAddress))
                        return;

                    var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                    var emailBody = emailTemplate(_notificationData);

                    // NOTE: create Smtp object to send email
                    var message = new EmailData
                    {
                        IsHtml = NotificationTemplate.EmailIsHtml,
                        //FromAddress = new Address
                        //{
                        //    EmailAddress = _configuration.GetValue<string>("EmailSender:1:Email"),
                        //    Name = _configuration.GetValue<string>("EmailSender:1:Name")
                        //},
                        Subject = NotificationTemplate.Title,
                        ToAddresses = new List<Address> { new Address { EmailAddress = staffMail.EmailAddress, Name = staffMail.Name } },
                    };

                    if (NotificationTemplate.EmailIsHtml)
                        message.Body = emailBody;
                    else
                        message.PlaintextAlternativeBody = emailBody;

                    // send email
                    await NotificationManager.SendSmtp(message);
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }

        }
    }
}
