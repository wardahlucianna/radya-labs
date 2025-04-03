using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Persistence.AttendanceDb;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
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

namespace BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification
{

    public class ATD1Notification : FunctionsNotificationHandler, IDisposable
    {
        private IDictionary<string, object> _notificationData;
        private IReadOnlyCollection<GetNotificationATD1Result> _getNotificationATDs;
        private readonly IAttendanceDbContext _dbContext;
        private readonly ILogger<ATD1Notification> _logger;

        public ATD1Notification(INotificationManager notificationManager,
            DbContextOptions<AttendanceDbContext> options,
            IConfiguration configuration,
            ILogger<ATD1Notification> logger,
            IDictionary<string, object> notificationData) :
             base("ATD1", notificationManager, configuration, logger)
        {
            _dbContext = new AttendanceDbContext(options); ;
            _logger = logger;
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
                    Web = false
                }
            };

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            (_dbContext as AttendanceDbContext)?.Dispose();
        }
        protected override async Task Prepare()
        {
            try
            {
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                _notificationData = new Dictionary<string, object>
                {
                    { "hostUrl", hostUrl }
                };
                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;
                var data = KeyValues.Where(x => x.Key == "data").Select(x => x.Value).FirstOrDefault();
                var objectData = JsonConvert.DeserializeObject<List<string>>((string)data);
                var dataEntry =
                (
                    from _attendanceEntry in _dbContext.Entity<TrAttendanceEntry>()
                    join _mappingAttendancec in _dbContext.Entity<MsAttendanceMappingAttendance>() on _attendanceEntry.IdAttendanceMappingAttendance equals _mappingAttendancec.Id
                    join _attendance in _dbContext.Entity<MsAttendance>() on _mappingAttendancec.IdAttendance equals _attendance.Id
                    join _scheduleLesson in _dbContext.Entity<TrGeneratedScheduleLesson>() on _attendanceEntry.IdGeneratedScheduleLesson equals _scheduleLesson.Id
                    join _scheduleStudent in _dbContext.Entity<TrGeneratedScheduleStudent>() on _scheduleLesson.IdGeneratedScheduleStudent equals _scheduleStudent.Id
                    join _homeroom in _dbContext.Entity<MsHomeroom>() on _scheduleLesson.IdHomeroom equals _homeroom.Id
                    join _homeroomTeacher in _dbContext.Entity<MsHomeroomTeacher>() on _homeroom.Id equals _homeroomTeacher.IdHomeroom
                    join _staff in _dbContext.Entity<MsStaff>() on _homeroomTeacher.IdBinusian equals _staff.IdBinusian
                    join _grade in _dbContext.Entity<MsGrade>() on _homeroom.IdGrade equals _grade.Id
                    join _gradePathwayClassroom in _dbContext.Entity<MsGradePathwayClassroom>() on _homeroom.IdGradePathwayClassRoom equals _gradePathwayClassroom.Id
                    join _classroom in _dbContext.Entity<MsClassroom>() on _gradePathwayClassroom.IdClassroom equals _classroom.Id
                    join _student in _dbContext.Entity<MsStudent>() on _scheduleStudent.IdStudent equals _student.Id
                    join _studentParent in _dbContext.Entity<MsStudentParent>() on _student.Id equals _studentParent.IdStudent
                    join _parent in _dbContext.Entity<MsParent>() on _studentParent.IdParent equals _parent.Id
                    join _session in _dbContext.Entity<MsSession>() on _scheduleLesson.IdSession equals _session.Id
                    where
                    1 == 1
                    && objectData.Contains(_attendanceEntry.Id)
                    && _homeroomTeacher.IsAttendance
                    && _attendance.AttendanceCategory == AttendanceCategory.Absent
                    select new GetNotificationATD1Result
                    {
                        ParentId = _parent.Id,
                        ParentEmail = _parent.PersonalEmailAddress,
                        ParentFullname = _parent.FirstName + " " + _parent.LastName,
                        StudentId = _student.Id,
                        StudentFullname = _student.FirstName + " " + _student.LastName,
                        Grade = _grade.Code,
                        Homeroom = _grade.Code + _classroom.Code,
                        ScheduleDate = _scheduleLesson.ScheduleDate,
                        SessionId = _scheduleLesson.SessionID,
                        AbsenceCategory = _attendance.AbsenceCategory,
                        Status = _attendance.Description,
                        FileEvidence = _attendanceEntry.FileEvidence,
                        Notes = _attendanceEntry.Notes,
                        TeacherFullname = _staff.FirstName + " " + _staff.LastName,
                        StartTime = _session.StartTime,
                        EndTime = _session.EndTime,
                    }
                ).AsQueryable();
                _getNotificationATDs = await dataEntry.ToListAsync(CancellationToken);
                IdUserRecipients = _getNotificationATDs.Select(x => x.ParentId)
                                   .Distinct()
                                   .ToList();
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, ex.Message);
            }
        }

        private class GetNotificationATD1Result
        {
            public string ParentId { get; set; }
            public string ParentEmail { get; set; }
            public string ParentFullname { get; set; }
            public string StudentId { get; set; }
            public string StudentFullname { get; set; }
            public string Grade { get; set; }
            public string Homeroom { get; set; }
            public DateTime ScheduleDate { get; set; }
            public string SessionId { get; set; }
            public AbsenceCategory? AbsenceCategory { get; set; }
            public string Status { get; set; }
            public string FileEvidence { get; set; }
            public string Notes { get; set; }
            public string TeacherFullname { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }

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
            foreach (var item in _getNotificationATDs)
            {
                //{{studentName}} Attendance on {{date}}
                //{{studentName}}
                //{{binussianID}}
                //{{studentGrade}}
                //{{studentClass}}
                //{{date}}
                //{{time}}
                //{{category}}
                //{{attendanceStatus}}
                //{{file}}
                //{{notes}}
                //{{teacherName}}
                _notificationData["receiverName"] = item.ParentFullname;
                _notificationData["studentName"] = item.StudentFullname;
                _notificationData["date"] = item.ScheduleDate.ToString("dd MMM yyyy");
                _notificationData["binussianID"] = item.StudentId;
                _notificationData["studentGrade"] = item.Grade;
                _notificationData["studentClass"] = item.Homeroom;
                _notificationData["time"] = $"{item.StartTime} - {item.EndTime}";
                _notificationData["category"] = item.AbsenceCategory.ToString();
                _notificationData["attendanceStatus"] = item.Status.ToString();
                _notificationData["file"] = item.FileEvidence;
                _notificationData["notes"] = item.Notes;
                _notificationData["teacherName"] = item.TeacherFullname;
                var compileSubject = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Email);
                var message = new EmailData
                {
                    Subject = compileSubject(_notificationData),
                    Body = compileBody(_notificationData),
                    IsHtml = NotificationTemplate.EmailIsHtml,
                    ToAddresses = new[] { new Address(item.ParentEmail) }.ToList(),
                    BccAddresses = bccs
                };
                sendEmailTasks.Add(NotificationManager.SendSmtp(message));
            }
            await Task.WhenAll(sendEmailTasks);
        }

        protected override async Task SendPushNotification()
        {
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

            foreach (var item in _getNotificationATDs)
            {

                var tokenByUser = tokens.Where(x => x.IdUser == item.ParentId).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;
                var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Push);
                //{{studentName}}
                //{{homeroom}}
                //{{binussianID}}
                //{{attendanceStatus}}
                //{{date}}
                //{{emailReceiver}}
                // NOTE: create MulticastMessage object to send push notification
                _notificationData["studentName"] = item.StudentFullname;
                _notificationData["date"] = item.ScheduleDate.ToString("dd MMM yyyy");
                _notificationData["binussianID"] = item.StudentId;
                _notificationData["studentGrade"] = item.Grade;
                _notificationData["studentClass"] = item.Homeroom;
                _notificationData["time"] = $"{item.StartTime} - {item.EndTime}";
                _notificationData["category"] = item.AbsenceCategory.ToString();
                _notificationData["attendanceStatus"] = item.Status.ToString();
                _notificationData["file"] = item.FileEvidence;
                _notificationData["notes"] = item.Notes;
                _notificationData["teacherName"] = item.TeacherFullname;
                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = compileTitle(_notificationData),
                        Body = compileBody(_notificationData)
                    },
                    Tokens = tokenByUser,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                // send push notification
                await NotificationManager.SendPushNotification(message);
            }
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            foreach (var item in _getNotificationATDs)
            {
                var compileTitle = Handlebars.Compile(NotificationTemplate.Title);
                var compileBody = Handlebars.Compile(NotificationTemplate.Push);
                //{{studentName}}
                //{{homeroom}}
                //{{binussianID}}
                //{{attendanceStatus}}
                //{{date}}
                //{{emailReceiver}}
                // NOTE: create MulticastMessage object to send push notification
                _notificationData["studentName"] = item.StudentFullname;
                _notificationData["date"] = item.ScheduleDate.ToString("dd MMM yyyy");
                _notificationData["binussianID"] = item.StudentId;
                _notificationData["studentGrade"] = item.Grade;
                _notificationData["studentClass"] = item.Homeroom;
                _notificationData["time"] = $"{item.StartTime} - {item.EndTime}";
                _notificationData["category"] = item.AbsenceCategory.ToString();
                _notificationData["attendanceStatus"] = item.Status.ToString();
                _notificationData["file"] = item.FileEvidence;
                _notificationData["notes"] = item.Notes;
                _notificationData["teacherName"] = item.TeacherFullname;

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
