using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.StudentDb.Abstractions;
using BinusSchool.Persistence.StudentDb.Entities;
using BinusSchool.Persistence.StudentDb.Entities.User;
using BinusSchool.Persistence.StudentDb.Entities.School;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;
using BinusSchool.Data.Model.Student.FnGuidanceCounseling.ReportStudentToGc;
using BinusSchool.Data.Model.Scheduling.FnSchedule.AppointmentBooking;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.Encoders;

namespace BinusSchool.Student.FnGuidanceCounseling.ReportStudentToGc.Notification
{
    public class GC3Notification : FunctionsNotificationHandler
    {
        private List<GC3NotificationResult> _dataReport;
        private string _schoolName;
        private IDictionary<string, object> _notificationData;
        private readonly IStudentDbContext _dbContext;

        public GC3Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<GC3Notification> logger, IStudentDbContext dbContext, IDictionary<string, object> notificationData) :
    base("GC3", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _notificationData = notificationData;
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
            var Object = KeyValues.FirstOrDefault(e => e.Key == "ListDeletedData").Value;
            _dataReport = JsonConvert.DeserializeObject<List<GC3NotificationResult>>(JsonConvert.SerializeObject(Object));

            var schoolName = await _dbContext.Entity<MsSchool>()
                                        .Where(e => e.Id == IdSchool
                                                )
                                        .Select(e => e.Name)
                                        .FirstOrDefaultAsync(CancellationToken);

            //_dataReport = KeyValues["ListDeletedData"] as List<GC3NotificationResult>;
            _schoolName = schoolName;
        }
        protected override async Task SendPushNotification()
        {
            foreach (var data in _dataReport)
            {
                var SendPushNotificationTaks = new List<Task>();
                _notificationData = new Dictionary<string, object> { };
                _notificationData.Add("StudentName", data.StudentName);
                _notificationData.Add("Staff/TeacherName", data.UserConsellor);
                _notificationData.Add("DeleteDate", data.Date);
                _notificationData.Add("SchoolName", _schoolName);

                var tokens = await _dbContext.Entity<MsUserPlatform>()
                    .Where(x
                        => x.IdUser == data.IdConsellor && x.FirebaseToken != null
                        && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                    .Select(x => x.FirebaseToken)
                    .ToListAsync();

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var pushTitle = titleTemplate(_notificationData);
                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var pushBody = pushTemplate(_notificationData);

                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = pushTitle,
                        Body = pushBody
                    },
                    Tokens = tokens
                };

                _notificationData.Clear();
                // send push notification
                await Task.WhenAll(SendPushNotificationTaks);
            }
        }
        protected override async Task SendEmailNotification()
        {
            foreach (var data in _dataReport)
            {
                _notificationData = new Dictionary<string, object> { };
                _notificationData.Add("StudentName", data.StudentName);
                _notificationData.Add("Staff/TeacherName", data.UserConsellor);
                _notificationData.Add("DeleteDate", data.Date);
                _notificationData.Add("CounselorName", data.UserConsellor);
                _notificationData.Add("AcademicYear", data.AcademicYear);
                _notificationData.Add("BinussianId", data.IdStudent);
                _notificationData.Add("Date", data.DateUpdate);
                _notificationData.Add("ReportedBy", data.ReportedBy);
                _notificationData.Add("Note", data.Note);
                _notificationData.Add("SchoolName", _schoolName);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var pushTitle = titleTemplate(_notificationData);
                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);
                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var pushBody = pushTemplate(_notificationData);

                var message = new SendGridMessage
                {
                    Subject = pushTitle,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { new EmailAddress(data.EmailConsellor) } }
                    }
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = emailBody;
                else
                    message.PlainTextContent = pushBody;

                _notificationData.Clear();
                await NotificationManager.SendEmail(message);
            }
        }
        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            foreach (var data in _dataReport)
            {
                _notificationData = new Dictionary<string, object> { };
                _notificationData.Add("StudentName", data.StudentName);
                _notificationData.Add("Staff/TeacherName", data.UserConsellor);
                _notificationData.Add("DeleteDate", data.DateUpdate);
                _notificationData.Add("SchoolName", _schoolName);

                var titleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var pushTitle = titleTemplate(_notificationData);
                var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var pushBody = pushTemplate(_notificationData);

                var notification = CreateNotificationHistory(idUserRecipients, isBlast, pushTitle, pushBody);

                _notificationData.Clear();
                await NotificationManager.SaveNotification(notification);
            }
        }
    }
}
