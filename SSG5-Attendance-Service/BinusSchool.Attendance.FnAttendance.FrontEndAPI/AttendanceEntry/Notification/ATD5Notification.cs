using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using FluentEmail.Core.Models;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static BinusSchool.Attendance.FnAttendance.AttendanceEntry.UpdateAttendanceEntryHandler;

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification
{
    public class ATD5Notification : FunctionsNotificationHandler, IDisposable
    {
        private IReadOnlyCollection<StudentBelowVm> _studentBelowVms;
        private IReadOnlyCollection<AttendanceEntryStudent> _attendanceEntryStudents;
        private IDictionary<string, object> _notificationData;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD5Notification> _logger;
        private readonly IMachineDateTime _machineDateTime;
        public ATD5Notification(INotificationManager notificationManager,
              DbContextOptions<AttendanceDbContext> options,
              IConfiguration configuration,
              ILogger<ATD5Notification> logger,
              IDictionary<string, object> notificationData,
              IMachineDateTime machineDateTime) :
               base("ATD5", notificationManager, configuration, logger)
        {
            _dbContext = new AttendanceDbContext(options); ;
            _logger = logger;
            _notificationData = notificationData;
            _machineDateTime = machineDateTime;
        }

        public void Dispose()
        {
            (_dbContext as AttendanceDbContext)?.Dispose();
        }
        protected override Task FetchNotificationConfig()
        {
            NotificationConfig = new NotificationConfig
            {
                EnEmail = true,
                EnPush = new EnablePushConfig
                {
                    Mobile = true,
                    Web = false
                }
            };

            return Task.CompletedTask;
        }

        protected override async Task Prepare()
        {
            try
            {
                var data = KeyValues.Where(x => x.Key == "data").Select(x => x.Value).FirstOrDefault();
                if (data == null)
                    throw new Exception("data not found");
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                _notificationData = new Dictionary<string, object>
                {
                    { "hostUrl", hostUrl }
                };
                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;
                var objectData = JsonConvert.DeserializeObject<List<StudentBelowVm>>((string)data);
                _studentBelowVms =
                 (from _student in objectData
                  join _studentParent in _dbContext.Entity<MsStudentParent>() on _student.IdStudent equals _studentParent.IdStudent
                  join _parent in _dbContext.Entity<MsParent>() on _studentParent.IdParent equals _parent.Id
                  where
                  1 == 1
                  select new StudentBelowVm
                  {
                      IdParent = _parent.Id,
                      IdStudent = _student.IdStudent,
                      StudentName = _student.StudentName,
                      AttendanceRate = _student.AttendanceRate,
                      ParentEmail = _parent.PersonalEmailAddress,
                      ParentName = _parent.FirstName + " " + _parent.LastName,
                      MinPercentage = _student.MinPercentage,
                      CurrentDate = _student.CurrentDate,
                      YearLevel = _student.YearLevel,
                  }
                ).ToList();
                var idStudents = _studentBelowVms.Select(x => x.IdStudent).ToList();
                _attendanceEntryStudents = await _dbContext.Entity<TrAttendanceEntry>()
                    .Include(x => x.GeneratedScheduleLesson)
                        .ThenInclude(x => x.GeneratedScheduleStudent)
                    .Include(x => x.AttendanceMappingAttendance)
                        .ThenInclude(x => x.Attendance)
                    .Where(x => idStudents.Contains(x.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent))
                    .Select(x => new AttendanceEntryStudent
                    {
                        IdStudent = x.GeneratedScheduleLesson.GeneratedScheduleStudent.IdStudent,
                        ScheduleDate = x.GeneratedScheduleLesson.ScheduleDate,
                        SessionId = x.GeneratedScheduleLesson.SessionID,
                        SubjectName = x.GeneratedScheduleLesson.SubjectName,
                        TeacherName = x.GeneratedScheduleLesson.TeacherName,
                        Attendance = x.AttendanceMappingAttendance.Attendance.Description
                    })
                    .ToListAsync(CancellationToken);
                _notificationData["minPercentage"] = objectData.FirstOrDefault()?.MinPercentage.ToString();
                IdUserRecipients = _studentBelowVms.Select(x => x.IdParent).Distinct().ToList();

            }
            catch (Exception)
            {

                throw;
            }
        }

        private class AttendanceEntryStudent
        {
            public string IdStudent { get; set; }
            public DateTime ScheduleDate { get; set; }
            public string SessionId { get; set; }
            public string SubjectName { get; set; }
            public string TeacherName { get; set; }
            public string Attendance { get; set; }
        }
        protected override async Task SendPushNotification()
        {
            if (IdUserRecipients is null)
                _logger.LogInformation($"Skip sending notification. No Id User Recipients");
            var tokenUsers = await _dbContext.Entity<MsUserPlatform>()
                  .Where(x
                      => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                       && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                  .Select(x => new
                  {
                      x.IdUser,
                      x.FirebaseToken
                  }
                  )
                  .ToListAsync(cancellationToken: CancellationToken);
            foreach (var item in _studentBelowVms)
            {
                var tokens = tokenUsers.Where(x => x.IdUser == item.IdParent).Select(x => x.FirebaseToken).ToList();
                if (!EnsureAnyPushTokens(tokens))
                    continue;
                var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Push);
                _notificationData["studentName"] = item.StudentName;
                _notificationData["yearLevel"] = item.YearLevel;
                _notificationData["actualPercentage"] = item.AttendanceRate.ToString();
                _notificationData["minPercentage"] = item.MinPercentage.ToString();
                _notificationData["dateTime"] = $"{_machineDateTime.ServerTime.Date.ToString("dd MMM yyyy")} {_machineDateTime.ServerTime.Date.ToString("HH:mm")}";
                _notificationData["date"] = item.CurrentDate.ToString("dd MMM yyyy");
                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = compileTitle(_notificationData),
                        Body = compileBody(_notificationData)
                    },
                    Tokens = tokens,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };
                await NotificationManager.SendPushNotification(message);
            }
        }

        protected override async Task SendEmailNotification()
        {
            if (IdUserRecipients is null)
                _logger.LogInformation($"Skip sending notification. No Id User Recipients");

            var bccs = new List<Address>();

            var envName = Environment.GetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT") ??
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (envName == "Staging")
            {
                //bccs.Add(new Address("bsslog.prod@gmail.com", "bsslog.prod@gmail.com"));
                //bccs.Add(new Address("group-itdevelopmentschools@binus.edu", "group-itdevelopmentschools@binus.edu"));
                bccs.Add(new Address("itdevschool@binus.edu", "itdevschool@binus.edu"));
            }
            var sendEmailTasks = new List<Task>();
            foreach (var item in _studentBelowVms)
            {
                var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Email);
                _notificationData["studentName"] = item.StudentName;
                _notificationData["yearLevel"] = item.YearLevel;
                _notificationData["actualPercentage"] = item.AttendanceRate.ToString();
                _notificationData["minPercentage"] = item.MinPercentage.ToString();
                _notificationData["dateTime"] = $"{_machineDateTime.ServerTime.Date.ToString("dd MMM yyyy")} {_machineDateTime.ServerTime.Date.ToString("HH:mm")}";
                _notificationData["date"] = item.CurrentDate.ToString("dd MMM yyyy");
                _notificationData["attendanceStudent"] = _attendanceEntryStudents.Where(x => x.IdStudent == item.IdStudent)
                .Select((x, row) => new Dictionary<string, object>
                {
                    {"no",row + 1 },
                    {"scheduleDate",x.ScheduleDate.ToString("dd MMM yyyy") },
                    {"session",x.SessionId },
                    {"subjectName",x.SubjectName },
                    {"teacherName",x.TeacherName },
                    {"attendanceStatus",x.Attendance },
                })
                .ToList();
                var message = new EmailData
                {
                    Subject = compileTitle(_notificationData),
                    Body = compileBody(_notificationData),
                    IsHtml = NotificationTemplate.EmailIsHtml,
                    ToAddresses = new[] { new Address(item.ParentEmail) }.ToList(),
                    BccAddresses = bccs
                };
                sendEmailTasks.Add(NotificationManager.SendSmtp(message));
            }
            await Task.WhenAll(sendEmailTasks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            foreach (var item in _studentBelowVms)
            {
                var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Push);
                _notificationData["studentName"] = item.StudentName;
                _notificationData["yearLevel"] = item.YearLevel;
                _notificationData["actualPercentage"] = item.AttendanceRate.ToString();
                _notificationData["minPercentage"] = item.MinPercentage.ToString();
                _notificationData["dateTime"] = $"{_machineDateTime.ServerTime.Date.ToString("dd MMM yyyy")} {_machineDateTime.ServerTime.Date.ToString("HH:mm")}";
                _notificationData["date"] = item.CurrentDate.ToString("dd MMM yyyy");

                var notification = CreateNotificationHistory(
                    idUserRecipients,
                    isBlast,
                    compileTitle(_notificationData),
                    compileBody(_notificationData));

                await NotificationManager.SaveNotification(notification);
            }

        }
    }
}
