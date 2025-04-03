using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Constants;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
using BinusSchool.Persistence.AttendanceDb.Entities.Employee;
using BinusSchool.Persistence.AttendanceDb.Entities.Scheduling;
using BinusSchool.Persistence.AttendanceDb.Entities.School;
using BinusSchool.Persistence.AttendanceDb.Entities.Student;
using BinusSchool.Persistence.AttendanceDb.Entities.User;
using FirebaseAdmin.Messaging;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid.Helpers.Mail;

namespace BinusSchool.Attendance.FnAttendance.AttendanceV2.Notification
{
    public class ATD8V2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ATD8V2Notification> _logger;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private List<UnsubmitSessionAttendanceVm> _subjectTeachers = new List<UnsubmitSessionAttendanceVm>();

        public ATD8V2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD8V2Notification> logger, IAttendanceDbContext dbContext, IDictionary<string, object> notificationData, IMachineDateTime dateTime) :
           base("ATD8", notificationManager, configuration, logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _logger = logger;
            _dateTime = dateTime;
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
                PushNotificationData["action"] = "ATD_ENTRY";
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                var attendanceUrlBase = $"{hostUrl}/attendance/detailsessionattendanceV2";
                _notificationData = new Dictionary<string, object>();

                var school = await _dbContext.Entity<MsSchool>().Where(x => x.Id == IdSchool).FirstOrDefaultAsync(CancellationToken);
                if (school != null)
                    _notificationData["schoolName"] = school.Name;

                var idAcademicYear = await _dbContext.Entity<MsPeriod>()
                                .Include(e => e.Grade).ThenInclude(e => e.Level).ThenInclude(e => e.AcademicYear)
                                .Where(x => _dateTime.ServerTime.Date >= x.StartDate && _dateTime.ServerTime.Date <= x.EndDate)
                                .Where(x => x.Grade.Level.AcademicYear.IdSchool == IdSchool)
                                .Select(x => x.Grade.Level.IdAcademicYear)
                                .FirstOrDefaultAsync(CancellationToken);

                var levelIds = await _dbContext.Entity<MsLevel>()
                                .Include(x => x.MappingAttendances)
                                .Where(x => x.MappingAttendances.Any(y => y.AbsentTerms == AbsentTerm.Session) && x.IdAcademicYear == idAcademicYear)
                                .Select(x => x.Id)
                                .ToListAsync(CancellationToken);

                var listSchedule = await _dbContext.Entity<MsSchedule>()
                          .Include(e => e.Lesson)
                          .Include(e => e.Sessions)
                          .Where(e => e.Lesson.IdAcademicYear == idAcademicYear
                                    && levelIds.Contains(e.Lesson.Grade.IdLevel))
                          .ToListAsync(CancellationToken);

                var listIdLesson = listSchedule.Select(e => e.IdLesson).Distinct().ToList();

                var listStudentStatus = await _dbContext.Entity<TrStudentStatus>()
                                .Where(e => e.IdAcademicYear == idAcademicYear && e.ActiveStatus)
                                .Select(e => new
                                {
                                    e.IdStudent,
                                    e.StartDate,
                                    endDate = e.EndDate == null
                                            ? _dateTime.ServerTime.Date
                                            : Convert.ToDateTime(e.EndDate),
                                    e.Student.IdBinusian
                                })
                                .ToListAsync(CancellationToken);

                var listPeriod = await _dbContext.Entity<MsPeriod>()
                       .Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear)
                       .ToListAsync(CancellationToken);

                var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                   .Include(e => e.HomeroomStudent)
                                   .Where(e => listIdLesson.Contains(e.IdLesson))
                                   .GroupBy(e => new
                                   {
                                       idLesson = e.IdLesson,
                                       idStudent = e.HomeroomStudent.IdStudent,
                                       idHomeroomStudent = e.HomeroomStudent.Id,
                                       idHomeroom = e.HomeroomStudent.IdHomeroom,
                                       classroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                                       idGrade = e.HomeroomStudent.Homeroom.IdGrade,
                                       gradeCode = e.HomeroomStudent.Homeroom.Grade.Code,
                                       gradeDescription = e.HomeroomStudent.Homeroom.Grade.Description,
                                       idLevel = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                                       levelCode = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                                       levelDescription = e.HomeroomStudent.Homeroom.Grade.Level.Description,
                                       semester = e.HomeroomStudent.Homeroom.Semester,
                                       IdHomeroomStudentEnrollment = e.Id
                                   })
                                   .Select(e => new GetHomeroom
                                   {
                                       IdLesson = e.Key.idLesson,
                                       IdStudent = e.Key.idStudent,
                                       IdHomeroomStudent = e.Key.idHomeroomStudent,
                                       Homeroom = new ItemValueVm
                                       {
                                           Id = e.Key.idHomeroom,
                                       },
                                       ClassroomCode = e.Key.classroomCode,
                                       Grade = new CodeWithIdVm
                                       {
                                           Id = e.Key.idGrade,
                                           Code = e.Key.gradeCode,
                                           Description = e.Key.gradeDescription
                                       },
                                       Level = new CodeWithIdVm
                                       {
                                           Id = e.Key.idLevel,
                                           Code = e.Key.levelCode,
                                           Description = e.Key.levelDescription
                                       },
                                       Semester = e.Key.semester,
                                       IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                       IsFromMaster = true,
                                       IsDelete = false
                                   })
                                   .ToListAsync(CancellationToken);

                listHomeroomStudentEnrollment.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min());

                var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                       .Include(e => e.SubjectNew)
                       .Include(e => e.LessonNew)
                       .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.GradePathwayClassroom).ThenInclude(e=>e.Classroom)
                       .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade).ThenInclude(e=>e.Level)
                       .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && x.LessonOld.IdAcademicYear == idAcademicYear)
                       .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                       .Select(e => new GetHomeroom
                       {
                           IdLesson = e.IdLessonNew,
                           IdStudent = e.HomeroomStudent.IdStudent,
                           IdHomeroomStudent = e.IdHomeroomStudent,
                           Homeroom = new ItemValueVm
                           {
                               Id = e.HomeroomStudent.IdHomeroom,
                           },
                           ClassroomCode = e.HomeroomStudent.Homeroom.GradePathwayClassroom.Classroom.Code,
                           Grade = new CodeWithIdVm
                           {
                               Id = e.HomeroomStudent.Homeroom.Grade.Id,
                               Code = e.HomeroomStudent.Homeroom.Grade.Code,
                               Description = e.HomeroomStudent.Homeroom.Grade.Description
                           },
                           Level = new CodeWithIdVm
                           {
                               Id = e.HomeroomStudent.Homeroom.Grade.Level.Id,
                               Code = e.HomeroomStudent.Homeroom.Grade.Level.Code,
                               Description = e.HomeroomStudent.Homeroom.Grade.Level.Description
                           },
                           Semester = e.HomeroomStudent.Homeroom.Semester,
                           IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                           IsFromMaster = false,
                           IsDelete = e.IsDelete,
                           EffectiveDate = e.StartDate,
                       })
                       .ToListAsync(CancellationToken);

                var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                                 .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                                 .ToList();

                var listIdlevel = listStudentEnrollmentUnion.Select(e => e.Level.Id).Distinct().ToList();
                var listIdGrade = listStudentEnrollmentUnion.Select(e => e.Grade.Id).Distinct().ToList();

                var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                   .Include(e => e.ScheduleLesson)
                                   .Where(e => e.ScheduleLesson.IdAcademicYear == idAcademicYear && listIdLesson.Contains(e.ScheduleLesson.IdLesson))
                                   .Select(e => new
                                   {
                                       e.IdScheduleLesson,
                                       e.IdHomeroomStudent,
                                   })
                                   .ToListAsync(CancellationToken);

                var listScheduleLesoon = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e => e.Subject)
                                    .Include(e => e.Session)
                                    .Include(e => e.Lesson)
                                    .Include(e => e.AcademicYear)
                                    .Where(e => listIdLesson.Contains(e.IdLesson)
                                            && e.IdAcademicYear == idAcademicYear
                                            && e.ScheduleDate.Date <= _dateTime.ServerTime.Date)
                                    .Select(e => new
                                    {
                                        e.Id,
                                        e.ScheduleDate,
                                        e.IdLesson,
                                        e.ClassID,
                                        idSession = e.Session.Id,
                                        nameSession = e.Session.Name,
                                        e.IdGrade,
                                        e.SubjectName,
                                        e.IdSubject,
                                        e.IdWeek,
                                        e.IdDay,
                                        e.IdSession,
                                        e.Session.StartTime,
                                        e.Session.EndTime,
                                        e.AcademicYear.IdSchool,
                                        e.Lesson.Semester
                                    })
                                    .ToListAsync(CancellationToken);

                var listMappingAttendance = await _dbContext.Entity<MsMappingAttendance>()
                                   .Where(e => listIdlevel.Contains(e.IdLevel))
                                   .Select(e => new
                                   {
                                       e.IdLevel,
                                       e.AbsentTerms
                                   })
                                   .ToListAsync(CancellationToken);

                var listLessonTeacher = await _dbContext.Entity<MsLessonTeacher>()
                            .Include(e => e.Lesson)
                            .Where(e => e.Lesson.IdAcademicYear == idAcademicYear
                                    && e.IsAttendance
                                    && listIdLesson.Contains(e.IdLesson))
                            .ToListAsync(CancellationToken);

                for (var i = 0; i < listScheduleLesoon.Count(); i++)
                {
                    var data = listScheduleLesoon[i];
                    var semester = listPeriod
                                    .Where(e => e.IdGrade == data.IdGrade && (e.StartDate.Date <= data.ScheduleDate.Date && e.EndDate.Date >= data.ScheduleDate.Date))
                                    .Select(e => e.Semester)
                                    .FirstOrDefault();

                    var listStatusStudentByDate = listStudentStatus
                                         .Where(e => e.StartDate.Date <= data.ScheduleDate.Date
                                                     && e.endDate.Date >= data.ScheduleDate.Date)
                                         .Select(e => e.IdStudent).ToList();

                    // moving student enrollment
                    var listStudentEnrollmentMoving = GetAttendanceEntryV2Handler.GetMovingStudent(listStudentEnrollmentUnion, data.ScheduleDate, data.Semester.ToString(), data.IdLesson);

                    var studentEnrollmentMoving = listStudentEnrollmentMoving
                                                  .Where(e => listStatusStudentByDate.Contains(e.IdStudent))
                                                  .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.EffectiveDate)
                                                  .ToList();

                    var listidHomeroomStudent = studentEnrollmentMoving.Select(e => e.IdHomeroomStudent).ToList();

                    var listAttendanceEntryBySchedule = listAttendanceEntry
                                                        .Where(e => e.IdScheduleLesson == data.Id 
                                                                && listidHomeroomStudent.Contains(e.IdHomeroomStudent))
                                                        .Select(e => e.IdHomeroomStudent)
                                                        .ToList();

                    var HomeroomBySchedule = studentEnrollmentMoving.Select(e => e.Homeroom.Id).FirstOrDefault();
                    var homeroom = listHomeroomStudentEnrollment.Where(e => e.Homeroom.Id == HomeroomBySchedule && e.Semester == semester).FirstOrDefault();
                    var listIdUserTeacher = listSchedule
                                            .Where(e => e.IdLesson == data.IdLesson 
                                                    && e.IdWeek== data.IdWeek 
                                                    && e.Sessions.StartTime == data.StartTime
                                                    && e.Sessions.EndTime == data.EndTime
                                                    && e.IdDay==data.IdDay)
                                            .Select(e=>e.IdUser).Distinct().ToList();

                    if (homeroom != null)
                    {
                        if (listAttendanceEntryBySchedule.Any())
                        {
                            var studentExcludeEnrollment = studentEnrollmentMoving
                                                           .Where(e => !listAttendanceEntryBySchedule
                                                           .Contains(e.IdHomeroomStudent))
                                                           .ToList();


                            if (studentExcludeEnrollment.Any() && data.IdSchool!="2")
                            {
                                foreach(var itemIdUserTeacher in listIdUserTeacher)
                                {
                                    var accessAttendance = listLessonTeacher
                                          .Where(e => e.Lesson.ClassIdGenerated == data.ClassID
                                                  && e.IdLesson == data.IdLesson
                                                  && e.IdUser == itemIdUserTeacher)
                                          .Select(e => e.IdLesson)
                                          .ToList();

                                    if (!accessAttendance.Any())
                                        continue;

                                    _subjectTeachers.Add(new UnsubmitSessionAttendanceVm
                                    {
                                        Date = data.ScheduleDate.Date,
                                        Subject = data.SubjectName,
                                        HomeroomName = homeroom.Grade.Code + homeroom.ClassroomCode,
                                        IdSession = data.idSession,
                                        SessionId = data.nameSession,
                                        ClassId = data.ClassID,
                                        IdUserTeacher = itemIdUserTeacher,
                                        countStudent = studentEnrollmentMoving.Count(),
                                    LinkAttendance = attendanceUrlBase + $"?idSession={data.idSession}&date={data.ScheduleDate.ToString("MM/dd/yyyy")}&classId={data.ClassID}&CurrentPos=ST"
                                    });
                                }
                            }
                        }
                        else
                        {
                            foreach (var itemIdUserTeacher in listIdUserTeacher)
                            {
                                var accessAttendance = listLessonTeacher
                                         .Where(e => e.Lesson.ClassIdGenerated == data.ClassID
                                                 && e.IdLesson == data.IdLesson
                                                 && e.IdUser == itemIdUserTeacher)
                                         .Select(e => e.IdLesson)
                                         .ToList();

                                if (!accessAttendance.Any())
                                    continue;

                                _subjectTeachers.Add(new UnsubmitSessionAttendanceVm
                                {
                                    Date = data.ScheduleDate.Date,
                                    Subject = data.SubjectName,
                                    HomeroomName = homeroom.Grade.Code + homeroom.ClassroomCode,
                                    IdSession = data.idSession,
                                    SessionId = data.nameSession,
                                    ClassId = data.ClassID,
                                    IdUserTeacher = itemIdUserTeacher,
                                    countStudent = studentEnrollmentMoving.Count(),
                                    LinkAttendance = attendanceUrlBase + $"?idSession={data.idSession}&date={data.ScheduleDate.ToString("MM/dd/yyyy")}&classId={data.ClassID}&CurrentPos=ST"
                                });
                            }
                        }
                    }
                }

                IdUserRecipients = _subjectTeachers.Select(x => x.IdUserTeacher).Distinct().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendPushNotification()
        {
            var tokens = await _dbContext.Entity<MsUserPlatform>()
                .Where(x
                    => IdUserRecipients.Contains(x.IdUser) && x.FirebaseToken != null
                    && NotificationConfig.EnPush.AllowedPlatforms.Contains(x.AppPlatform))
                .Select(x => new { x.FirebaseToken, x.IdUser })
                .ToListAsync(CancellationToken);

            if (!EnsureAnyPushTokens(tokens.Select(e => e.FirebaseToken).ToList()))
                return;

            var SendPushNotificationTaks = new List<Task>();

            foreach (var idUser in IdUserRecipients)
            {
                var tokenByUser = tokens.Where(x => x.IdUser == idUser).Select(x => x.FirebaseToken).ToList();

                if (!EnsureAnyPushTokens(tokenByUser))
                    continue;

                _notificationData["total"] = _subjectTeachers.Where(x => x.IdUserTeacher == idUser).Count();

                var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                var PushBody = PushTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);

                var message = new MulticastMessage
                {
                    Notification = new FirebaseAdmin.Messaging.Notification
                    {
                        Title = TitleBody,
                        Body = TitleBody
                    },
                    Tokens = tokenByUser,
                    Data = (IReadOnlyDictionary<string, string>)PushNotificationData
                };

                // send push notification
                SendPushNotificationTaks.Add(NotificationManager.SendPushNotification(message));
            }

            await Task.WhenAll(SendPushNotificationTaks);
        }

        protected override async Task SaveNotification(IEnumerable<string> idUserRecipients, bool isBlast)
        {
            try
            {
                if (idUserRecipients is null)
                {
                    _logger.LogInformation($"Skip sending notification. No Id User Recipients");
                    return;
                }

                var saveNotificationTasks = new List<Task>();

                foreach (var idUser in idUserRecipients)
                {
                    _notificationData["total"] = _subjectTeachers.Where(x => x.IdUserTeacher == idUser).Count();

                    var PushTemplate = Handlebars.Compile(NotificationTemplate.Push);
                    var PushBody = PushTemplate(_notificationData);

                    var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                    var TitleBody = TitleTemplate(_notificationData);

                    saveNotificationTasks.Add(NotificationManager.SaveNotification(
                        CreateNotificationHistory(
                            new[] { idUser },
                            isBlast,
                            TitleBody,
                            PushBody)));
                }
                await Task.WhenAll(saveNotificationTasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }

        protected override async Task SendEmailNotification()
        {
            var User = await _dbContext.Entity<MsUser>()
                  .Where(x => IdUserRecipients.Contains(x.Id))
                  .Select(x => new
                  {
                      x.Id,
                      EmailAddress = new EmailAddress(x.Email, x.DisplayName)
                  })
                  .ToListAsync(CancellationToken);

            if (!User.Any())
                return;

            var sendEmailTasks = new List<Task>();

            foreach (var idUser in IdUserRecipients)
            {
                var GetUserById = User.Where(e => e.Id == idUser).FirstOrDefault();
                if (GetUserById == null)
                    continue;

                var dataTeacher = _subjectTeachers.First(x => x.IdUserTeacher == idUser);
                var dataTable = string.Empty;
                var count = 1;
                foreach (var item in _subjectTeachers.Where(x => x.IdUserTeacher == idUser)
                                                     .OrderBy(x => x.Date).ThenBy(x => x.Subject).ThenBy(x => x.SessionId))
                {
                    dataTable += "<tr>" +
                                    "<td>" + count + "</td>" +
                                    "<td>" + item.Date.ToString("dd/MM/yyyy") + "</td>" +
                                    "<td>" + item.Subject + "</td>" +
                                    "<td>" + item.HomeroomName + "</td>" +
                                    "<td>" + item.SessionId + "</td>" +
                                    "<td><a href='" + item.LinkAttendance + "'>Click Here</a></td>" +
                                "</tr>";
                    count++;
                }

                _notificationData["total"] = _subjectTeachers.Where(x => x.IdUserTeacher == idUser).Count();
                _notificationData["receiverName"] = GetUserById.EmailAddress.Name;
                _notificationData["dateValid"] = $"{_dateTime.ServerTime:dd MMMM yyyy hh:mm tt} (GMT+{DateTimeUtil.OffsetHour})";

                var emailReplace = NotificationTemplate.Email.Replace("{{data}}", dataTable);
                var EmailTemplate = Handlebars.Compile(emailReplace);
                var EmailBody = EmailTemplate(_notificationData);

                var TitleTemplate = Handlebars.Compile(NotificationTemplate.Title);
                var TitleBody = TitleTemplate(_notificationData);

                var message = new SendGridMessage
                {
                    Subject = TitleBody,
                    Personalizations = new List<Personalization>
                    {
                        new Personalization { Tos = new List<EmailAddress> { GetUserById.EmailAddress } }
                    }
                };

                if (NotificationTemplate.EmailIsHtml)
                    message.HtmlContent = EmailBody;
                else
                    message.PlainTextContent = EmailBody;

                sendEmailTasks.Add(NotificationManager.SendEmail(message));
            }
            // send batch email
            await Task.WhenAll(sendEmailTasks);
        }
    }
}
