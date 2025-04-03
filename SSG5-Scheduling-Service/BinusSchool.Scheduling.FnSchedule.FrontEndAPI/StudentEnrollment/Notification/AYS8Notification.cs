using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Utils;
using BinusSchool.Persistence.SchedulingDb.Abstractions;
using BinusSchool.Persistence.SchedulingDb.Entities;
using BinusSchool.Persistence.SchedulingDb.Entities.Employee;
using BinusSchool.Persistence.SchedulingDb.Entities.School;
using BinusSchool.Persistence.SchedulingDb.Entities.Student;
using BinusSchool.Persistence.SchedulingDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace BinusSchool.Scheduling.FnSchedule.StudentEnrollment.Notification
{
    public class AYS8Notification : FunctionsNotificationHandler
    {
        private string _schoolName, _idGrade;
        private IDictionary<string, object> _notificationData;
        private (string Id, IEnumerable<IEnumerable<char>> ClassIds)[] _lessonTeachers;

        private readonly ISchedulingDbContext _dbContext;
        
        public AYS8Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<AYS8Notification> logger, ISchedulingDbContext dbContext, IDictionary<string, object> notificationData) : 
            base("AYS8", notificationManager, configuration, logger)
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
            var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
            
            _schoolName = await _dbContext.Entity<MsSchool>()
                .Where(x => x.Id == IdSchool)
                .Select(x => x.Name.ToUpper())
                .FirstOrDefaultAsync(CancellationToken);
            _notificationData = new Dictionary<string, object>
            {
                { "date", ((DateTime)KeyValues["date"]).ToString("d MMMM yyyy") },
                { "hostUrl", hostUrl },
                { "schoolName", _schoolName },
            };
            _idGrade = KeyValues["idGrade"] as string;
            
            // remove additional dictionary
            KeyValues.Remove("date");
            KeyValues.Remove("idGrade");
            
            var allClassIds = KeyValues
                .SelectMany(x =>
                {
                    var classIds = ((JObject)x.Value).ToObject<IDictionary<string, IEnumerable<string>>>();
                    return classIds!["old"].Concat(classIds["new"]);
                })
                .Distinct();
            var lts = await _dbContext.Entity<MsLessonTeacher>()
                .Where(x => x.Lesson.IdGrade == _idGrade && allClassIds.Contains(x.Lesson.ClassIdGenerated))
                .Select(x => new
                {
                    x.IdUser, 
                    ClassId = x.Lesson.ClassIdGenerated.TrimEnd().Distinct()
                })
                .ToListAsync(CancellationToken);
            
            _lessonTeachers = lts
                .GroupBy(x => x.IdUser)
                .Select(x => (Id: x.Key, ClassIds: x.Select(y => y.ClassId)))
                .ToArray();
            IdUserRecipients = _lessonTeachers.Select(x => x.Id);

            var pushTitle = Handlebars.Compile(NotificationTemplate.Title);
            var pushContent = Handlebars.Compile(NotificationTemplate.Push);

            GeneratedTitle = pushTitle(_notificationData);
            GeneratedContent = pushContent(_notificationData);
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x 
                    => KeyValues.Keys.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => x.FirebaseToken)
                .ToListAsync(CancellationToken);
            
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
                Tokens = tokens
            };
                    
            // send push notification
            await NotificationManager.SendPushNotification(message);
        }

        protected override async Task SendEmailNotification()
        {
            var teachers = await _dbContext.Entity<MsStaff>()
                .Where(x => IdUserRecipients.Contains(x.IdBinusian))
                .Select(x => new
                {
                    x.IdBinusian,
                    EmailAddress = new Address(x.BinusianEmailAddress, NameUtil.GenerateFullName(x.FirstName, x.LastName))
                })
                .ToListAsync(CancellationToken);
            
            if (!EnsureAnyEmails(teachers.Select(x => x.EmailAddress)))
                return;
            
            var sendEmailTasks = new List<Task>(_lessonTeachers.Length);
            var enrollHistory = KeyValues
                .Select(x =>
                    KeyValuePair.Create(x.Key, ((JObject)x.Value).ToObject<IDictionary<string, IEnumerable<string>>>()))
                .ToList();
            var idStudents = KeyValues.Keys;
            var studentNames = await _dbContext.Entity<MsStudent>()
                .Where(x => idStudents.Contains(x.Id))
                .Select(x => new { x.Id, Name = NameUtil.GenerateFullName(x.FirstName, x.MiddleName, x.LastName) })
                .ToListAsync(CancellationToken);

            // send unique email to each subject teacher
            foreach (var lessonTeacher in _lessonTeachers)
            {
                var subjectTeacherEmail = teachers.FirstOrDefault(x => x.IdBinusian == lessonTeacher.Id)?.EmailAddress;
                if (subjectTeacherEmail is null)
                    continue;

                // TODO: compare two list
                var studentOfClassIds = enrollHistory
                    .Where(x => x.Value.Values.Any(y => y.Union(lessonTeacher.ClassIds).Any()))
                    .Select(x => new
                    {
                        studentName = studentNames.FirstOrDefault(y => y.Id == x.Key)?.Name,
                        old = string.Join(", ", x.Value["old"]),
                        @new = string.Join(", ", x.Value["new"])
                    });

                _notificationData["subjectTeacherName"] = subjectTeacherEmail.Name;
                _notificationData["studentOfClassIds"] = studentOfClassIds;

                var emailTemplate = Handlebars.Compile(NotificationTemplate.Email);
                var emailBody = emailTemplate(_notificationData);

                // NOTE: create EmailData object to send email
                var message = new EmailData
                {
                    FromAddress = new Address(IdSchool),
                    ToAddresses = new List<Address> { subjectTeacherEmail },
                    Subject = NotificationTemplate.Title,
                    Body = emailBody,
                    IsHtml = NotificationTemplate.EmailIsHtml
                };

                sendEmailTasks.Add(NotificationManager.SendSmtp(message));
            }

            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}
