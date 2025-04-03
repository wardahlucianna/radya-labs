using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceAdministration;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceAdministration.Notification
{
    public class ATD12Notification : FunctionsNotificationHandler
    {
        private List<ATD12NotificationModel> _listData;
        private string _schoolName;
        private IDictionary<string, object> _notificationData;
        private readonly IAttendanceDbContext _dbContext;

        public ATD12Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD12Notification> logger, 
            IAttendanceDbContext dbContext, IDictionary<string, object> notificationData) :
        base("ATD12", notificationManager, configuration, logger)
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
        protected override Task Prepare()
        {
            _listData = KeyValues["ListData"] as List<ATD12NotificationModel>;
            _schoolName = KeyValues["SchoolName"] as string;

            return Task.CompletedTask;
        }
        protected override async Task SendPushNotification()
        {
            var sendPushNotifTasks = new List<Task>(_listData.ToList().Count);

            foreach (var data in _listData)
            {
                var dataStudent = await _dbContext.Entity<MsStudent>().FirstOrDefaultAsync(x => x.Id == data.IdStudent);
                var scheduleGroups = data.SessionUseds.GroupBy(x => new { x.IdTeacher, x.Date, x.ClassID });
                foreach (var scheduleGroup in scheduleGroups)
                {
                    var tokens = await _dbContext.Entity<MsUserPlatform>()
                                .Where(x => x.IdUser == scheduleGroup.Key.IdTeacher && x.FirebaseToken != null
                                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                                .Select(x => x.FirebaseToken)
                                .ToListAsync();
                    var dataTeacher = await _dbContext.Entity<MsUser>().FirstOrDefaultAsync(x => x.Id == scheduleGroup.Key.IdTeacher);

                    _notificationData.Add("StudentName", string.Join(" ", (new string[] {
                            dataStudent.FirstName,
                            dataStudent.MiddleName,
                            dataStudent.LastName }).Where(s => !string.IsNullOrWhiteSpace(s))));
                    _notificationData.Add("ClassId", scheduleGroup.Key.ClassID);
                    _notificationData.Add("BinussianID", dataStudent.Id);
                    _notificationData.Add("AttendanceStatus", data.Reason);
                    _notificationData.Add("Date", scheduleGroup.Key.Date.ToShortDateString());
                    _notificationData.Add("IdSession", string.Join(", ", scheduleGroup.ToList().Select(x => x.SessionID)));
                    _notificationData.Add("Emailreceiver", dataTeacher.Email);

                    var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                    var pushBody = pushTemplate(_notificationData);

                    var message = new MulticastMessage
                    {
                        Notification = new FirebaseAdmin.Messaging.Notification
                        {
                            Title = NotificationTemplate.Title,
                            Body = pushBody
                        },
                        Tokens = tokens
                    };

                    sendPushNotifTasks.Add(NotificationManager.SendPushNotification(message));
                    _notificationData.Clear();
                }
            }
            // send batch email
            await Task.WhenAll(sendPushNotifTasks);
        }
        protected override async Task SendEmailNotification()
        {
            var sendPushNotifTasks = new List<Task>(_listData.ToList().Count);
            var bccs = new List<Address>();

            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (envName == "Staging")
            {
                //bccs.Add(new Address("bsslog.prod@gmail.com", "bsslog.prod@gmail.com"));
                //bccs.Add(new Address("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu"));
                bccs.Add(new Address("itdevschool@binus.edu", "itdevschool@binus.edu"));
            }
            foreach (var data in _listData)
            {
                var dataStudent = await _dbContext.Entity<MsStudent>().FirstOrDefaultAsync(x => x.Id == data.IdStudent);
                var scheduleGroups = data.SessionUseds.GroupBy(x => new { x.IdTeacher, x.Date, x.ClassID });
                foreach (var scheduleGroup in scheduleGroups)
                {
                    var dataTeacher = await _dbContext.Entity<MsUser>().FirstOrDefaultAsync(x => x.Id == scheduleGroup.Key.IdTeacher);

                    _notificationData.Add("StudentName", string.Join(" ", (new string[] {
                            dataStudent.FirstName,
                            dataStudent.MiddleName,
                            dataStudent.LastName }).Where(s => !string.IsNullOrWhiteSpace(s))));
                    _notificationData.Add("ClassId", scheduleGroup.Key.ClassID);
                    _notificationData.Add("BinussianID", dataStudent.Id);
                    _notificationData.Add("AttendanceStatus", data.Reason);
                    _notificationData.Add("Date", scheduleGroup.Key.Date.ToShortDateString());
                    _notificationData.Add("IdSession", string.Join(", ", scheduleGroup.ToList().Select(x => x.SessionID)));

                    var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                    var pushBody = pushTemplate(_notificationData);
                    var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                    var email = emailTemplate(_notificationData);

                    var message = new EmailData
                    {
                        Subject = NotificationTemplate.Title,
                        ToAddresses = new List<Address>() { new Address(dataTeacher.Email) },
                        IsHtml = NotificationTemplate.EmailIsHtml,
                        BccAddresses = bccs
                    };

                    if (NotificationTemplate.EmailIsHtml)
                        message.Body = email;
                    else
                        message.PlaintextAlternativeBody = pushBody;

                    sendPushNotifTasks.Add(NotificationManager.SendSmtp(message));

                    _notificationData.Clear();
                }
            }
            // send batch email
            await Task.WhenAll(sendPushNotifTasks);
        }
        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            foreach (var data in _listData)
            {
                var dataStudent = await _dbContext.Entity<MsStudent>().FirstOrDefaultAsync(x => x.Id == data.IdStudent);
                var scheduleGroups = data.SessionUseds.GroupBy(x => new { x.IdTeacher, x.Date, x.ClassID });
                foreach (var scheduleGroup in scheduleGroups)
                {
                    var tokens = await _dbContext.Entity<MsUserPlatform>()
                                .Where(x => x.IdUser == scheduleGroup.Key.IdTeacher && x.FirebaseToken != null
                                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                                .Select(x => x.FirebaseToken)
                                .ToListAsync();
                    var dataTeacher = await _dbContext.Entity<MsUser>().FirstOrDefaultAsync(x => x.Id == scheduleGroup.Key.IdTeacher);

                    _notificationData.Add("StudentName", string.Join(" ", (new string[] {
                            dataStudent.FirstName,
                            dataStudent.MiddleName,
                            dataStudent.LastName }).Where(s => !string.IsNullOrWhiteSpace(s))));
                    _notificationData.Add("TeacherName", dataTeacher.DisplayName);
                    _notificationData.Add("ClassId", scheduleGroup.Key.ClassID);
                    _notificationData.Add("BinussianID", dataStudent.Id);
                    _notificationData.Add("AttendanceStatus", data.Reason);
                    _notificationData.Add("Date", scheduleGroup.Key.Date.ToShortDateString());
                    _notificationData.Add("IdSession", string.Join(", ", scheduleGroup.ToList().Select(x => x.SessionID)));
                    _notificationData.Add("Emailreceiver", dataTeacher.Email);

                    var pushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                    var pushBody = pushTemplate(_notificationData);

                    var notification = CreateNotificationHistory(idUserRecipients, isBlast, NotificationTemplate.Title, pushBody);

                    await NotificationManager.SaveNotification(notification);

                    _notificationData.Clear();
                }
            }
        }
    }
}
