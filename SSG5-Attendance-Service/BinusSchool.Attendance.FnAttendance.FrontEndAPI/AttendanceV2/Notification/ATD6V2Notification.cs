using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BinusSchool.Attendance.FnAttendance.AttendanceEntry.Notification.ViewModels;
using BinusSchool.Common.Abstractions;
using BinusSchool.Common.Functions.Abstractions;
using BinusSchool.Common.Functions.Handler;
using BinusSchool.Common.Model;
using BinusSchool.Common.Model.Enums;
using BinusSchool.Common.Utils;
using BinusSchool.Data.Model.Attendance.FnAttendance.AttendanceV2;
using BinusSchool.Persistence.AttendanceDb.Abstractions;
using BinusSchool.Persistence.AttendanceDb.Entities;
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
    public class ATD6V2Notification : FunctionsNotificationHandler
    {
        private IDictionary<string, object> _notificationData;
        private readonly ILogger<ATD6V2Notification> _logger;
        private readonly IAttendanceDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly IMachineDateTime _dateTime;
        private IReadOnlyCollection<UserAttendanceVm<PendingAttendanceVm>> _homeroomTeachers;

        public ATD6V2Notification(INotificationManager notificationManager, IConfiguration configuration, ILogger<ATD6V2Notification> logger, IAttendanceDbContext dbContext, IDictionary<string, object> notificationData, IMachineDateTime dateTime) :
           base("ATD6", notificationManager, configuration, logger)
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
                var hostUrl = Configuration.GetSection("ClientApp:Web:Host").Get<string>();
                var attendanceUrlBase = $"{hostUrl}/attendance/detailsessionattendancev2";
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

                var listPeriod = await _dbContext.Entity<MsPeriod>()
                      .Where(e => e.Grade.Level.IdAcademicYear == idAcademicYear)
                      .ToListAsync(CancellationToken);

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

                var listHomeroom = await _dbContext.Entity<MsHomeroomTeacher>()
                                .Include(e => e.Homeroom).ThenInclude(e => e.Grade).ThenInclude(e => e.Level)
                                .Include(e => e.Homeroom).ThenInclude(e => e.GradePathwayClassroom).ThenInclude(e => e.Classroom)
                                .Where(e => e.Homeroom.IdAcademicYear == idAcademicYear
                                        )
                                .Select(e => new
                                {
                                    e.IdHomeroom,
                                    homeroom = $"{e.Homeroom.Grade.Code}{e.Homeroom.GradePathwayClassroom.Classroom.Code}",
                                    e.Homeroom.Grade.IdLevel,
                                    level = e.Homeroom.Grade.Level.Description,
                                    levelCode = e.Homeroom.Grade.Level.Code,
                                    e.Homeroom.IdGrade
                                })
                                .ToListAsync(CancellationToken);

                var listIdHomeroom = listHomeroom.Select(e => e.IdHomeroom).Distinct().ToList();
                var listIdlevel = listHomeroom.Select(e => e.IdLevel).Distinct().ToList();
                var listIdGrade = listHomeroom.Select(e => e.IdGrade).Distinct().ToList();

                var listHomeroomStudentEnrollment = await _dbContext.Entity<MsHomeroomStudentEnrollment>()
                                   .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                                   .Where(e => listIdHomeroom.Contains(e.HomeroomStudent.IdHomeroom))
                                   .GroupBy(e => new
                                   {
                                       e.HomeroomStudent.IdStudent,
                                       e.IdLesson,
                                       e.IdHomeroomStudent,
                                       e.HomeroomStudent.IdHomeroom,
                                       e.HomeroomStudent.Homeroom.Semester,
                                       IdHomeroomStudentEnrollment = e.Id,
                                       e.HomeroomStudent.Homeroom.IdGrade
                                   })
                                   .Select(e => new GetHomeroom
                                   {
                                       IdLesson = e.Key.IdLesson,
                                       IdHomeroomStudent = e.Key.IdHomeroomStudent,
                                       Homeroom = new ItemValueVm
                                       {
                                           Id = e.Key.IdHomeroom
                                       },
                                       IdStudent = e.Key.IdStudent,
                                       Semester = e.Key.Semester,
                                       IdHomeroomStudentEnrollment = e.Key.IdHomeroomStudentEnrollment,
                                       IsFromMaster = true,
                                       IsDelete = false,
                                       Grade = new CodeWithIdVm
                                       {
                                           Id = e.Key.IdGrade
                                       }
                                   })
                                   .ToListAsync(CancellationToken);

                listHomeroomStudentEnrollment.ForEach(e => e.EffectiveDate = listPeriod.Where(f => f.IdGrade == e.Grade.Id).Select(f => f.AttendanceStartDate).Min());

                var getTrHomeroomStudentEnrollment = await _dbContext.Entity<TrHomeroomStudentEnrollment>()
                       .Include(e => e.SubjectNew)
                       .Include(e => e.LessonNew)
                       .Include(e => e.HomeroomStudent).ThenInclude(e=>e.Homeroom).ThenInclude(e=>e.Grade)
                       .Where(x => x.StartDate.Date <= _dateTime.ServerTime.Date && x.LessonOld.IdAcademicYear == idAcademicYear)
                       .OrderBy(e => e.StartDate).ThenBy(e => e.DateIn)
                       .Select(e => new GetHomeroom
                       {
                           IdLesson = e.IdLessonNew,
                           IdHomeroomStudent = e.IdHomeroomStudent,
                           Homeroom = new ItemValueVm
                           {
                               Id = e.HomeroomStudent.IdHomeroom
                           },
                           IdStudent = e.HomeroomStudent.IdStudent,
                           Semester = e.HomeroomStudent.Homeroom.Semester,
                           IdHomeroomStudentEnrollment = e.IdHomeroomStudentEnrollment,
                           IsFromMaster = false,
                           IsDelete = e.IsDelete,
                           EffectiveDate = e.StartDate,
                           Grade = new CodeWithIdVm
                           {
                               Id = e.HomeroomStudent.Homeroom.IdGrade
                           }
                       })
                       .ToListAsync(CancellationToken);

                var listStudentEnrollmentUnion = listHomeroomStudentEnrollment.Union(getTrHomeroomStudentEnrollment)
                                               .OrderBy(e => e.IsFromMaster == true ? 0 : 1).ThenBy(e => e.IsShowHistory == true ? 1 : 0).ThenBy(e => e.EffectiveDate).ThenBy(e => e.Datein)
                                               .ToList();

                var listIdLesson = listStudentEnrollmentUnion.Select(f => f.IdLesson).Distinct().ToList();

                var listAttendanceEntry = await _dbContext.Entity<TrAttendanceEntryV2>()
                                   .Include(e => e.ScheduleLesson)
                                   .Where(e => e.Status == AttendanceEntryStatus.Pending && e.ScheduleLesson.IdAcademicYear == idAcademicYear && listIdLesson.Contains(e.ScheduleLesson.IdLesson))
                                   .Select(e => new
                                   {
                                       e.IdScheduleLesson,
                                       e.IdHomeroomStudent
                                   })
                                   .ToListAsync(CancellationToken);

                var listScheduleLesoon = await _dbContext.Entity<MsScheduleLesson>()
                                    .Include(e => e.Subject)
                                    .Include(e => e.Session)
                                    .Include(e => e.Lesson)
                                    .Where(e => listIdLesson.Contains(e.IdLesson)
                                            && e.IdAcademicYear == idAcademicYear)
                                    .GroupBy(e => new
                                    {
                                        e.Id,
                                        e.ScheduleDate,
                                        e.IdLesson,
                                        e.ClassID,
                                        idSession = e.Session.Id,
                                        sessionName = e.Session.Name,
                                        e.IdGrade,
                                        e.IdSubject,
                                        e.IdDay,
                                        e.IdWeek,
                                        e.IdAcademicYear,
                                        e.Lesson.Semester
                                    })
                                    .Select(e => new
                                    {
                                        e.Key.Id,
                                        e.Key.ScheduleDate,
                                        e.Key.IdLesson,
                                        e.Key.ClassID,
                                        idSession = e.Key.idSession,
                                        sessionName = e.Key.sessionName,
                                        e.Key.IdGrade,
                                        e.Key.IdSubject,
                                        e.Key.IdDay,
                                        e.Key.IdWeek,
                                        e.Key.IdAcademicYear,
                                        e.Key.Semester
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

                var listSchedule = await _dbContext.Entity<MsSchedule>()
                               .Include(e => e.Lesson)
                               .Include(e => e.User)
                               .Where(e => listIdLesson.Contains(e.IdLesson)
                                       && e.Lesson.IdAcademicYear == idAcademicYear)
                               .Select(e => new
                               {
                                   e.Id,
                                   e.IdLesson,
                                   e.Lesson.IdSubject,
                                   e.IdWeek,
                                   e.IdDay,
                                   e.IdUser,
                                   TeacherName = NameUtil.GenerateFullName(e.User.FirstName, e.User.LastName),
                                   e.IdSession
                               })
                               .ToListAsync(CancellationToken);

                var countStudent = listStudentEnrollmentUnion.Select(e => e.IdHomeroomStudent).Distinct().Count();
                List<UnresolvedAttendanceGroupEmailResult> attendance = new List<UnresolvedAttendanceGroupEmailResult>();
                for (var i = 0; i < listScheduleLesoon.Count(); i++)
                {
                    var data = listScheduleLesoon[i];
                    var semester = listPeriod
                                    .Where(e => e.IdGrade == data.IdGrade && (e.StartDate.Date >= data.ScheduleDate.Date && e.EndDate.Date <= data.ScheduleDate.Date))
                                    .Select(e => e.Semester).ToList();
                    var listIdStudent = listStudentStatus.Where(e => e.StartDate.Date <= data.ScheduleDate.Date).Select(e => e.IdStudent).ToList();

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

                    var HomeroomBySchedule = listHomeroomStudentEnrollment.Where(e => e.IdLesson == data.IdLesson).Select(e => e.Homeroom.Id).FirstOrDefault();
                    var homeroom = listHomeroom.Where(e => e.IdHomeroom == HomeroomBySchedule).FirstOrDefault();

                    if (listAttendanceEntryBySchedule.Any())
                    {
                        var GetTeacher = listSchedule
                               .Where(e => e.IdLesson == data.IdLesson && e.IdSubject == data.IdSubject && e.IdWeek == data.IdWeek && e.IdDay == data.IdDay && e.IdSession == data.idSession)
                               .GroupBy(e => new
                               {
                                   e.IdUser,
                                   e.TeacherName
                               })
                               .Select(e => new
                               {
                                   e.Key.IdUser,
                                   e.Key.TeacherName
                               })
                               .ToList();

                        foreach (var itemTeacher in GetTeacher)
                        {
                            attendance.Add(new UnresolvedAttendanceGroupEmailResult
                            {
                                Date = data.ScheduleDate.Date,
                                ClassID = data.ClassID,
                                IdAcademicYear = data.IdAcademicYear,
                                Teacher = new ItemValueVm
                                {
                                    Id = itemTeacher.IdUser,
                                    Description = itemTeacher.TeacherName
                                },
                                Homeroom = new ItemValueVm
                                {
                                    Id = homeroom.IdHomeroom,
                                    Description = homeroom.homeroom
                                },
                                Session = new ItemValueVm
                                {
                                    Id = data.idSession,
                                    Description = data.sessionName,
                                },
                                TotalStudent = listMappingAttendance.Where(e => e.AbsentTerms == AbsentTerm.Session).Any()
                                                ? studentEnrollmentMoving.Count()
                                                : countStudent,
                            });
                        }
                    }
                }

                var homeroomTeachers = attendance
                                        .GroupBy(e => new
                                        {
                                            IdUser = e.Teacher.Id
                                        })
                                        .Select(e => new UserAttendanceVm<PendingAttendanceVm>
                                        {
                                            IdUser = e.Key.IdUser,
                                            Attendances = attendance
                                                            .Where(f => f.Teacher.Id == e.Key.IdUser)
                                                            .GroupBy(f => new
                                                            {
                                                                f.Date,
                                                                idHomeroom = f.Homeroom.Id,
                                                                idSession = f.Session.Id,
                                                                sessionName = f.Session.Description,
                                                                HomeroomName = f.Homeroom.Description,
                                                                f.ClassID,
                                                                f.IdAcademicYear,
                                                            })
                                                            .Select(f => new PendingAttendanceVm
                                                            {
                                                                Date = f.Key.Date,
                                                                HomeroomName = f.Key.HomeroomName,
                                                                IdSession = f.Key.idSession,
                                                                SessionId = f.Key.sessionName,
                                                                ClassId = f.Key.ClassID,
                                                                LinkAttendance = attendanceUrlBase + $"?idAcadYear={f.Key.IdAcademicYear}&idHomeroom=       {f.Key.idHomeroom}&idSession={f.Key.idSession}&date={f.Key.Date.ToString("MM/dd/yyyy")}&classId={f.Key.ClassID}&CurrentPos=CA"
                                                            })
                                                            .ToList()



                                        }).ToList();

                PushNotificationData["action"] = "ATD_ENTRY";
                _homeroomTeachers = homeroomTeachers;
                IdUserRecipients = homeroomTeachers.Select(x => x.IdUser).Distinct();
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

                _notificationData["total"] = _homeroomTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();

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
                    _notificationData["total"] = _homeroomTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();

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

                var dataTable = string.Empty;
                var count = 1;

                foreach (var item in _homeroomTeachers.Where(x => x.IdUser == idUser)
                                                      .SelectMany(x => x.Attendances)
                                                      .GroupBy(e => new
                                                      {
                                                          e.Date,
                                                          e.HomeroomName,
                                                          e.SessionId,
                                                          e.LinkAttendance,
                                                          e.ClassId
                                                      })
                                                      .OrderBy(x => x.Key.Date).ThenBy(x => x.Key.HomeroomName).ThenBy(x => x.Key.SessionId))
                {
                    dataTable += "<tr>" +
                                    "<td>" + count + "</td>" +
                                    "<td>" + item.Key.Date.ToString("dd/MM/yyyy") + "</td>" +
                                    "<td>" + item.Key.HomeroomName + "</td>" +
                                    "<td>" + item.Key.SessionId + "</td>" +
                                    "<td><a href='" + item.Key.LinkAttendance + "'>Click Here</a></td>" +
                                "</tr>";
                    count++;
                }

                _notificationData["total"] = _homeroomTeachers.Where(x => x.IdUser == idUser).SelectMany(x => x.Attendances).Count();
                _notificationData["receiverName"] = GetUserById.EmailAddress.Name;

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
